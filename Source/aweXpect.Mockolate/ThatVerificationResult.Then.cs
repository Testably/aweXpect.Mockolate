using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using aweXpect.Core;
using aweXpect.Core.Constraints;
using aweXpect.Helpers;
using aweXpect.Results;
using Mockolate;
using Mockolate.Interactions;
using Mockolate.Verify;

namespace aweXpect;

public static partial class ThatVerificationResult
{
	/// <summary>
	///     Verifies that the <paramref name="interactions" /> happen after the current interaction in the given order.
	/// </summary>
	[GuaranteesNotNull]
	public static AndOrResult<VerificationResult<T>, IThat<VerificationResult<T>>> Then<T>(
		this IThat<VerificationResult<T>> subject, params Func<T, VerificationResult<T>>[] interactions)
		=> new(subject.Get().ExpectationBuilder.AddConstraint((_, it, grammars)
				=> new ThenConstraint<T>(it, grammars, interactions)),
			subject);

	private sealed class ThenConstraint<T>(
		string it,
		ExpectationGrammars grammars,
		Func<T, VerificationResult<T>>[] interactions)
		: ConstraintResult.WithNotNullValue<VerificationResult<T>>(it, grammars),
			IValueConstraint<VerificationResult<T>>
	{
		private readonly List<string> _expectations = new();
		private string? _error;
		private IInteraction[]? _allInteractions;

		public ConstraintResult IsMetBy(VerificationResult<T>? actual)
		{
			Actual = actual;
			_expectations.Clear();
			_error = null;
			_allInteractions = null;
			if (actual is null)
			{
				return this;
			}

			bool result = true;
			T verify = ((IVerificationResult<T>)actual).Object;
			IVerificationResult verificationResult = actual;
			IInteraction[] snapshot = verificationResult.Interactions.ToArray();
			Dictionary<IInteraction, int> positions = new(snapshot.Length);
			for (int i = 0; i < snapshot.Length; i++)
			{
				positions[snapshot[i]] = i;
			}

			int after = -1;
			foreach (Func<T, VerificationResult<T>> check in interactions)
			{
				IVerificationResult currentVerificationResult = verificationResult;
				_expectations.Add(currentVerificationResult.Expectation);
				if (!verificationResult.Verify(i => VerifyInteractions(i, currentVerificationResult)))
				{
					result = false;
				}

				verificationResult = check(verify);
			}

			_expectations.Add(verificationResult.Expectation);
			result = verificationResult.Verify(i => VerifyInteractions(i, verificationResult)) && result;
			Outcome = result ? Outcome.Success : Outcome.Failure;
			if (!result)
			{
				_allInteractions = snapshot;
			}

			return this;

			bool VerifyInteractions(IInteraction[] filteredInteractions, IVerificationResult currentVerificationResult)
			{
				int bestPosition = int.MaxValue;
				IInteraction? firstInteraction = null;
				foreach (IInteraction candidate in filteredInteractions)
				{
					if (positions.TryGetValue(candidate, out int position) &&
					    position > after &&
					    position < bestPosition)
					{
						bestPosition = position;
						firstInteraction = candidate;
					}
				}

				bool hasInteractionAfter = firstInteraction is not null;
				after = hasInteractionAfter ? bestPosition : int.MaxValue;
				if (!hasInteractionAfter && _error is null)
				{
					_error = filteredInteractions.Length > 0
						? $"{currentVerificationResult.Expectation} too early"
						: $"{currentVerificationResult.Expectation} not at all";
				}

				return hasInteractionAfter;
			}
		}

		protected override void AppendNormalExpectation(StringBuilder stringBuilder, string? indentation = null)
			=> AppendExpectations(stringBuilder, indentation).Append(" in order");

		protected override void AppendNormalResult(StringBuilder stringBuilder, string? indentation = null)
			=> stringBuilder.Append(It).Append(' ').Append(_error);

		protected override void AppendNegatedExpectation(StringBuilder stringBuilder, string? indentation = null)
			=> AppendExpectations(stringBuilder, indentation).Append(" not in order");

		protected override void AppendNegatedResult(StringBuilder stringBuilder, string? indentation = null)
			=> stringBuilder.Append(It).Append(" did");

		private StringBuilder AppendExpectations(StringBuilder stringBuilder, string? indentation)
		{
			if (_expectations.Count == 0)
			{
				return stringBuilder.Append("had the interactions");
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
			    Actual is IVerificationResult<T> verificationResult &&
			    new MyDescribableSubject<T>(verificationResult.Object as IMock) is TValue describableSubject)
			{
				value = describableSubject;
				return true;
			}

			return base.TryGetStoredValue(out value);
		}
	}
}
