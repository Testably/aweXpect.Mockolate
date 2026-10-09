using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using aweXpect.Core;
using aweXpect.Core.Constraints;
using aweXpect.Core.EvaluationContext;
using aweXpect.Helpers;
using aweXpect.Options;
using Mockolate;
using Mockolate.Exceptions;
using Mockolate.Interactions;
using Mockolate.Verify;

namespace aweXpect;

/// <summary>
///     Expectations on the <see cref="VerificationResult{TVerify}" /> returned from a mockolate Mock.
/// </summary>
public static partial class ThatVerificationResult
{
	private static string ToAmountString(this int number)
		=> number switch
		{
			0 => "never",
			1 => "once",
			2 => "twice",
			_ => $"{number} times",
		};

	private static IInteraction[]? GetAllInteractions<TVerify>(VerificationResult<TVerify> actual)
		=> ((IVerificationResult<TVerify>)actual).Object is IMock mock
			? mock.MockRegistry.Interactions.ToArray()
			: null;

	private static void AppendInteractionContexts(ResultContextCollector contexts,
		IInteraction[]? matchingInteractions, IInteraction[]? allInteractions)
	{
		if (matchingInteractions is not null)
		{
			contexts.Add(new ResultContext.SyncCallback("Matching Interactions",
				() => Formatter.Format(matchingInteractions, FormattingOptions.MultipleLines)));
		}

		if (allInteractions is not null)
		{
			contexts.Add(new ResultContext.SyncCallback("All Interactions",
				() => Formatter.Format(allInteractions, FormattingOptions.MultipleLines)));
		}
	}

	/// <remarks>
	///     The constraint is evaluated again for every item of a collection, so each evaluation resets the state that the
	///     failure message reads.
	/// </remarks>
	private abstract class VerificationCountConstraint<TVerify>(string it, ExpectationGrammars grammars)
		: ConstraintResult.WithNotNullValue<VerificationResult<TVerify>>(it, grammars)
	{
		private IInteraction[]? _allInteractions;
		private string? _expectation;
		private IInteraction[]? _matchingInteractions;

		protected int Count => _matchingInteractions?.Length ?? 0;

		protected string Expectation => _expectation ?? "had the interaction";

		protected abstract bool IsMet(int count);

		protected void Verify(VerificationResult<TVerify>? actual)
		{
			if (Start(actual))
			{
				Complete(actual, ((IVerificationResult)actual).Verify(Check));
			}
		}

		/// <remarks>
		///     A cancellation by the caller, or by a timeout of the evaluation that ends the wait early, leaves the outcome
		///     undecided, so that core reports it like for its own expectations.
		///     <para />
		///     Without <paramref name="options" />, e.g. for <c>Never</c>, which further interactions cannot satisfy,
		///     neither a timeout nor a cancellation makes the verification wait.
		/// </remarks>
		protected async ValueTask VerifyAsync(VerificationResult<TVerify>? actual,
			ExpectationBuilder expectationBuilder, WithinOptions? options, IEvaluationContext context,
			CancellationToken cancellationToken)
		{
			if (!Start(actual))
			{
				return;
			}

			VerificationResult<TVerify> verificationResult = actual;
			TimeSpan? timeout = options is null ? null : options.Timeout ?? expectationBuilder.Timeout;
			if (timeout is not null || (options is not null && expectationBuilder.CancellationToken is not null))
			{
				// An awaitable subject of the caller would keep the token after the evaluation released it.
				if (cancellationToken.CanBeCanceled && verificationResult is not IAsyncVerificationResult)
				{
					verificationResult = verificationResult.WithCancellation(cancellationToken);
				}

				if (timeout is not null)
				{
					verificationResult = verificationResult.Within(timeout.Value);
				}
			}

			if (verificationResult is not IAsyncVerificationResult asyncVerificationResult)
			{
				Complete(verificationResult, ((IVerificationResult)verificationResult).Verify(Check));
				return;
			}

			long startTimestamp = context.GetTimestamp();
			try
			{
				Complete(verificationResult, await asyncVerificationResult.VerifyAsync(Check));
			}
			catch (MockVerificationTimeoutException)
			{
				if (IsInconclusive(context.Cancellation, timeout, context.GetElapsedTime(startTimestamp)))
				{
					Outcome = Outcome.Undecided;
					_allInteractions = GetAllInteractions(verificationResult);
				}
				else
				{
					Complete(verificationResult, false);
				}
			}
		}

		private static bool IsInconclusive(EvaluationCancellation cancellation, TimeSpan? timeout, TimeSpan waited)
			=> cancellation.Reason switch
			{
				CancellationReason.Caller => true,
				CancellationReason.Timeout => timeout is null || !cancellation.HasWaitElapsed(timeout.Value, waited),
				_ => false,
			};

		private bool Start([NotNullWhen(true)] VerificationResult<TVerify>? actual)
		{
			Actual = actual;
			_expectation = null;
			_matchingInteractions = null;
			_allInteractions = null;
			if (actual is null)
			{
				return false;
			}

			_expectation = ((IVerificationResult)actual).Expectation;
			return true;
		}

		private bool Check(IInteraction[] interactions)
		{
			_matchingInteractions = interactions;
			return IsMet(interactions.Length);
		}

		private void Complete(VerificationResult<TVerify> actual, bool isMet)
		{
			Outcome = isMet ? Outcome.Success : Outcome.Failure;
			if (!isMet)
			{
				_allInteractions = GetAllInteractions(actual);
			}
		}

		public override void AppendContexts(ResultContextCollector contexts)
			=> AppendInteractionContexts(contexts, _matchingInteractions, _allInteractions);

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

	private sealed class HasExactlyConstraint<TVerify>(
		ExpectationBuilder expectationBuilder,
		string it,
		ExpectationGrammars grammars,
		int expected,
		WithinOptions? options)
		: VerificationCountConstraint<TVerify>(it, grammars),
			IAsyncContextConstraint<VerificationResult<TVerify>>
	{
		public async ValueTask<ConstraintResult> IsMetBy(VerificationResult<TVerify>? actual,
			IEvaluationContext context, CancellationToken cancellationToken)
		{
			await VerifyAsync(actual, expectationBuilder, options, context, cancellationToken);
			return this;
		}

		protected override bool IsMet(int count) => count == expected;

		protected override void AppendNormalExpectation(StringBuilder stringBuilder, string? indentation = null)
		{
			if (expected == 0)
			{
				stringBuilder.Append("never ").Append(Expectation);
			}
			else
			{
				stringBuilder.Append(Expectation).Append(" exactly ").Append(expected.ToAmountString());
			}

			stringBuilder.Append(options);
		}

		protected override void AppendNormalResult(StringBuilder stringBuilder, string? indentation = null)
		{
			if (Count == 0)
			{
				stringBuilder.Append("never found ").Append(It);
			}
			else
			{
				// Stryker disable once Equality : unreachable boundary — AppendNormalResult only runs when Count != expected
				stringBuilder.Append("found ").Append(It).Append(Count < expected ? " only " : " ")
					.Append(Count.ToAmountString());
			}
		}

		protected override void AppendNegatedExpectation(StringBuilder stringBuilder, string? indentation = null)
		{
			if (expected == 0)
			{
				stringBuilder.Append(Expectation).Append(" at least once");
			}
			else
			{
				stringBuilder.Append(Expectation).Append(" not exactly ").Append(expected.ToAmountString());
			}

			stringBuilder.Append(options);
		}

		protected override void AppendNegatedResult(StringBuilder stringBuilder, string? indentation = null)
			=> stringBuilder.Append(It).Append(" was");
	}

	private sealed class HasAtMostConstraint<TVerify>(
		string it,
		ExpectationGrammars grammars,
		int expected)
		: VerificationCountConstraint<TVerify>(it, grammars),
			IValueConstraint<VerificationResult<TVerify>>
	{
		public ConstraintResult IsMetBy(VerificationResult<TVerify>? actual)
		{
			Verify(actual);
			return this;
		}

		protected override bool IsMet(int count) => count <= expected;

		protected override void AppendNormalExpectation(StringBuilder stringBuilder, string? indentation = null)
			=> stringBuilder.Append(Expectation).Append(" at most ").Append(expected.ToAmountString());

		protected override void AppendNormalResult(StringBuilder stringBuilder, string? indentation = null)
			=> stringBuilder.Append("found ").Append(It).Append(' ').Append(Count.ToAmountString());

		protected override void AppendNegatedExpectation(StringBuilder stringBuilder, string? indentation = null)
			=> stringBuilder.Append(Expectation).Append(" more than ").Append(expected.ToAmountString());

		protected override void AppendNegatedResult(StringBuilder stringBuilder, string? indentation = null)
		{
			if (Count == 0)
			{
				stringBuilder.Append("never found ").Append(It);
			}
			else
			{
				stringBuilder.Append("found ").Append(It).Append(" only ").Append(Count.ToAmountString());
			}
		}
	}

	private sealed class HasAtLeastConstraint<TVerify>(
		ExpectationBuilder expectationBuilder,
		string it,
		ExpectationGrammars grammars,
		int expected,
		WithinOptions options)
		: VerificationCountConstraint<TVerify>(it, grammars),
			IAsyncContextConstraint<VerificationResult<TVerify>>
	{
		public async ValueTask<ConstraintResult> IsMetBy(VerificationResult<TVerify>? actual,
			IEvaluationContext context, CancellationToken cancellationToken)
		{
			await VerifyAsync(actual, expectationBuilder, options, context, cancellationToken);
			return this;
		}

		protected override bool IsMet(int count) => count >= expected;

		protected override void AppendNormalExpectation(StringBuilder stringBuilder, string? indentation = null)
			=> stringBuilder.Append(Expectation).Append(" at least ").Append(expected.ToAmountString())
				.Append(options);

		protected override void AppendNormalResult(StringBuilder stringBuilder, string? indentation = null)
		{
			if (Count == 0)
			{
				stringBuilder.Append("never found ").Append(It);
			}
			else
			{
				stringBuilder.Append("found ").Append(It).Append(" only ").Append(Count.ToAmountString());
			}
		}

		protected override void AppendNegatedExpectation(StringBuilder stringBuilder, string? indentation = null)
			=> stringBuilder.Append(Expectation).Append(" less than ").Append(expected.ToAmountString())
				.Append(options);

		protected override void AppendNegatedResult(StringBuilder stringBuilder, string? indentation = null)
			=> stringBuilder.Append("found ").Append(It).Append(' ').Append(Count.ToAmountString());
	}
}
