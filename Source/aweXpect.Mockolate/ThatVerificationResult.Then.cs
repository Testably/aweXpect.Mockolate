using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using aweXpect.Core;
using aweXpect.Core.Constraints;
using aweXpect.Core.EvaluationContext;
using aweXpect.Core.Extending;
using aweXpect.Helpers;
using aweXpect.Mockolate.Options;
using aweXpect.Mockolate.Results;
using Mockolate;
using Mockolate.Exceptions;
using Mockolate.Interactions;
using Mockolate.Verify;

namespace aweXpect;

public static partial class ThatVerificationResult
{
	/// <summary>
	///     Verifies that the <paramref name="interactions" /> happen after the current interaction in the given order.
	/// </summary>
	/// <remarks>
	///     A timeout of <c>Within</c> is a budget for the whole sequence. An interaction with a timeout of its own waits
	///     up to that timeout instead, while the others only wait for what is left of the budget.
	/// </remarks>
	/// <exception cref="ArgumentNullException">The <paramref name="interactions" /> are <see langword="null" />.</exception>
	[GuaranteesNotNull]
	public static AndOrWithinResult<VerificationResult<TVerify>, IThat<VerificationResult<TVerify>>> Then<TVerify>(
		this IThat<VerificationResult<TVerify>> subject, params Func<TVerify, VerificationResult<TVerify>>[] interactions)
	{
		if (interactions is null)
		{
			throw Tracing.WriteException(
				new ArgumentNullException(nameof(interactions), "The 'interactions' cannot be null."));
		}

		OptionsHolder<WithinOptions> options = new(WithinOptions.Default);
		return new AndOrWithinResult<VerificationResult<TVerify>, IThat<VerificationResult<TVerify>>>(
			subject.Get().ExpectationBuilder.AddConstraint((Interactions: interactions, Options: options),
				static (s, expectationBuilder, it, grammars)
					=> new ThenConstraint<TVerify>(expectationBuilder, it, grammars, s.Interactions, s.Options)),
			subject,
			options);
	}

	/// <remarks>
	///     Interactions are only appended, so an interaction found for a step keeps its place before the interactions
	///     that are recorded later, and every step is checked on the newest snapshot.
	/// </remarks>
	private sealed class ThenConstraint<TVerify>(
		ExpectationBuilder expectationBuilder,
		string it,
		ExpectationGrammars grammars,
		Func<TVerify, VerificationResult<TVerify>>[] interactions,
		OptionsHolder<WithinOptions> options)
		: ConstraintResult.WithNotNullValue<VerificationResult<TVerify>>(it, grammars),
			IAsyncContextConstraint<VerificationResult<TVerify>>
	{
		private readonly List<string> _expectations = new();
		private readonly Dictionary<IInteraction, int> _positions = new();
		private IInteraction[]? _allInteractions;
		private string? _error;
		private IInteraction? _previous;
		private IInteraction[]? _snapshot;
		private IVerificationResult? _step;

		public async ValueTask<ConstraintResult> IsMetBy(VerificationResult<TVerify>? actual,
			IEvaluationContext context, CancellationToken cancellationToken)
		{
			Actual = actual;
			_expectations.Clear();
			_positions.Clear();
			_allInteractions = null;
			_snapshot = null;
			_error = null;
			_previous = null;
			if (actual is null)
			{
				return this;
			}

			TVerify verify = ((IVerificationResult<TVerify>)actual).Object;
			TimeSpan? timeout = options.Value.Timeout ?? expectationBuilder.Timeout;
			using CancellationTokenSource? budgetCts =
				timeout is not null || expectationBuilder.CancellationToken is not null
					? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)
					: null;
			if (budgetCts is not null && timeout is not null)
			{
				context.CancelAfter(budgetCts, timeout.Value);
			}

			long startTimestamp = context.GetTimestamp();
			Func<IInteraction[], bool> check = Check;
			VerificationResult<TVerify> step = actual;
			Outcome outcome = Outcome.Success;
			int index = 0;
			while (true)
			{
				_expectations.Add(((IVerificationResult)step).Expectation);
				if (outcome == Outcome.Success)
				{
					outcome = await VerifyStep(step, check, budgetCts, timeout, startTimestamp, context,
						cancellationToken);
				}

				if (index == interactions.Length)
				{
					break;
				}

				step = UserCode.Invoke(interactions[index++], verify, "the interaction selector");
			}

			Outcome = outcome;
			_allInteractions = _snapshot;
			return this;
		}

		/// <remarks>
		///     Only a step without a timeout of its own gets the rest of the <paramref name="budgetCts" />. Without any
		///     rest it is checked once, without waiting.
		/// </remarks>
		private async ValueTask<Outcome> VerifyStep(VerificationResult<TVerify> step, Func<IInteraction[], bool> check,
			CancellationTokenSource? budgetCts, TimeSpan? timeout, long startTimestamp, IEvaluationContext context,
			CancellationToken cancellationToken)
		{
			_step = step;
			if (budgetCts is not null && step is not IAsyncVerificationResult &&
			    (timeout is null || timeout == Timeout.InfiniteTimeSpan ||
			     context.GetElapsedTime(startTimestamp) < timeout))
			{
				step = step.WithCancellation(budgetCts.Token);
			}

			if (step is not IAsyncVerificationResult asyncStep)
			{
				return ((IVerificationResult)step).Verify(check) ? Outcome.Success : Outcome.Failure;
			}

			try
			{
				return await asyncStep.VerifyAsync(check, cancellationToken) ? Outcome.Success : Outcome.Failure;
			}
			catch (Exception exception) when (exception is MockVerificationTimeoutException
				                                  or OperationCanceledException)
			{
				return IsInconclusive(context.Cancellation, timeout, context.GetElapsedTime(startTimestamp))
					? Outcome.Undecided
					: Outcome.Failure;
			}
		}

		private bool Check(IInteraction[] matchingInteractions)
		{
			IMockInteractions allInteractions = ((IVerificationResult)Actual!).Interactions;
			if (_snapshot is null || allInteractions.Count != _snapshot.Length)
			{
				_snapshot = allInteractions.ToArray();
				_positions.Clear();
				for (int i = 0; i < _snapshot.Length; i++)
				{
					_positions[_snapshot[i]] = i;
				}
			}

			int after = _previous is not null && _positions.TryGetValue(_previous, out int previousPosition)
				? previousPosition
				: -1;
			int position = FirstPositionAfter(matchingInteractions, _positions, after);
			if (position == int.MaxValue)
			{
				_error = matchingInteractions.Length > 0
					? $"{_step!.Expectation} too early"
					: $"{_step!.Expectation} not at all";
				return false;
			}

			_error = null;
			_previous = _snapshot[position];
			return true;
		}

		/// <summary>
		///     Returns the first position of the <paramref name="interactions" /> after the given position,
		///     or <see cref="int.MaxValue" /> if there is none.
		/// </summary>
		private static int FirstPositionAfter(IInteraction[] interactions,
			Dictionary<IInteraction, int> positions, int after)
		{
			int bestPosition = int.MaxValue;
			foreach (IInteraction candidate in interactions)
			{
				if (positions.TryGetValue(candidate, out int position) &&
				    position > after &&
				    position < bestPosition)
				{
					bestPosition = position;
				}
			}

			return bestPosition;
		}

		protected override void AppendNormalExpectation(StringBuilder stringBuilder, string? indentation = null)
			=> AppendExpectations(stringBuilder, indentation).Append(" in order").Append(options);

		protected override void AppendNormalResult(StringBuilder stringBuilder, string? indentation = null)
			=> stringBuilder.Append(It).Append(' ').Append(_error);

		protected override void AppendNegatedExpectation(StringBuilder stringBuilder, string? indentation = null)
			=> AppendExpectations(stringBuilder, indentation).Append(" not in order").Append(options);

		protected override void AppendNegatedResult(StringBuilder stringBuilder, string? indentation = null)
			=> stringBuilder.Append(It).Append(" did");

		private StringBuilder AppendExpectations(StringBuilder stringBuilder, string? indentation)
		{
			if (_expectations.Count == 0)
			{
				return stringBuilder.Append("has the interactions");
			}

			string separator = $", then{Environment.NewLine}{indentation}";
			return stringBuilder.Append(string.Join(separator, _expectations));
		}

		/// <remarks>
		///     The order is verified across several interactions, so the matching interactions of a single one would not
		///     explain the failure, while all interactions show their order.
		/// </remarks>
		public override void AppendContexts(ResultContextCollector contexts)
			=> AppendInteractionContexts(contexts, null, _allInteractions);

		public override bool TryGetStoredValue<TValue>(out TValue? value) where TValue : default
		{
			if (typeof(TValue) == typeof(IDescribableSubject) &&
			    Actual is IVerificationResult<TVerify> verificationResult &&
			    new MyDescribableSubject<TVerify>(verificationResult.Object as IMock) is TValue describableSubject)
			{
				value = describableSubject;
				return true;
			}

			return base.TryGetStoredValue(out value);
		}
	}
}
