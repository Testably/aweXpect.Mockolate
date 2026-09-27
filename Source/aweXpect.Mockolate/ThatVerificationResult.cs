using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using aweXpect.Core;
using aweXpect.Core.Constraints;
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

	private static void AppendAllInteractions(ExpectationBuilder expectationBuilder, IMock? mock)
	{
		if (mock is null)
		{
			return;
		}

		IMockInteractions allInteractions = mock.MockRegistry.Interactions;
		string interactionsText = Formatter.Format(allInteractions, FormattingOptions.MultipleLines);
		expectationBuilder.UpdateContexts(contexts => contexts
			.Remove("All Interactions")
			.Add(new ResultContext.SyncCallback("All Interactions", () => interactionsText)));
	}

	private sealed class HasExactlyConstraint<TVerify>(
		ExpectationBuilder expectationBuilder,
		string it,
		ExpectationGrammars grammars,
		int expected,
		WithinOptions options)
		: ConstraintResult.WithValue<VerificationResult<TVerify>>(it, grammars),
			IAsyncConstraint<VerificationResult<TVerify>>
	{
		private int _count;
		private string? _expectation;

		public async Task<ConstraintResult> IsMetBy(VerificationResult<TVerify> actual,
			CancellationToken cancellationToken)
		{
			if (options.CancellationToken is not null)
			{
				actual = actual.WithCancellation(options.CancellationToken.Value);
			}

			if (options.Timeout is not null)
			{
				actual = actual.Within(options.Timeout.Value);
			}
			else if (expectationBuilder.Timeout is not null)
			{
				actual = actual.Within(expectationBuilder.Timeout.Value);
			}

			if (actual is IAsyncVerificationResult asyncVerificationResult)
			{
				_expectation = asyncVerificationResult.Expectation;
				Actual = actual;
				try
				{
					Outcome = await asyncVerificationResult.VerifyAsync(interactions =>
					{
						string interactionsText = Formatter.Format(interactions, FormattingOptions.MultipleLines);
						expectationBuilder.UpdateContexts(contexts => contexts
							.Remove("Matching Interactions")
							.Add(new ResultContext.SyncCallback("Matching Interactions", () => interactionsText)));
						_count = interactions.Length;
						return interactions.Length == expected;
					})
						? Outcome.Success
						: Outcome.Failure;
					if (Outcome == Outcome.Failure)
					{
						AppendAllInteractions(expectationBuilder,
							((IVerificationResult<TVerify>)actual).Object as IMock);
					}

					return this;
				}
				catch (MockVerificationTimeoutException)
				{
					string interactionsText = Formatter.Format(((IVerificationResult)actual).Interactions,
						FormattingOptions.MultipleLines);
					expectationBuilder.UpdateContexts(contexts => contexts
						.Remove("Matching Interactions")
						.Add(new ResultContext.SyncCallback("Matching Interactions",
							() => interactionsText)));
					Outcome = Outcome.Failure;
					return this;
				}
			}

			IVerificationResult result = actual;
			_expectation = result.Expectation;
			Actual = actual;
			Outcome = result.Verify(interactions =>
			{
				string context = Formatter.Format(interactions, FormattingOptions.MultipleLines);
				expectationBuilder.UpdateContexts(contexts => contexts.Add(
					new ResultContext.SyncCallback("Matching Interactions", () => context)));
				_count = interactions.Length;
				return interactions.Length == expected;
			})
				? Outcome.Success
				: Outcome.Failure;
			if (Outcome == Outcome.Failure)
			{
				AppendAllInteractions(expectationBuilder, ((IVerificationResult<TVerify>)actual).Object as IMock);
			}

			return this;
		}

		protected override void AppendNormalExpectation(StringBuilder stringBuilder, string? indentation = null)
		{
			if (expected == 0)
			{
				stringBuilder.Append("never ").Append(_expectation);
			}
			else
			{
				stringBuilder.Append(_expectation).Append(" exactly ").Append(expected.ToAmountString());
			}
		}

		protected override void AppendNormalResult(StringBuilder stringBuilder, string? indentation = null)
		{
			if (_count == 0)
			{
				stringBuilder.Append("never found ").Append(It);
			}
			else
			{
				// Stryker disable once Equality : unreachable boundary — AppendNormalResult only runs when _count != expected
				stringBuilder.Append("found ").Append(It).Append(_count < expected ? " only " : " ")
					.Append(_count.ToAmountString());
			}
		}

		protected override void AppendNegatedExpectation(StringBuilder stringBuilder, string? indentation = null)
		{
			if (expected == 0)
			{
				stringBuilder.Append(_expectation).Append(" at least once");
			}
			else
			{
				stringBuilder.Append(_expectation).Append(" not exactly ").Append(expected.ToAmountString());
			}
		}

		protected override void AppendNegatedResult(StringBuilder stringBuilder, string? indentation = null)
			=> stringBuilder.Append(It).Append(" was");

		public override bool TryGetValue<TValue>([NotNullWhen(true)] out TValue? value) where TValue : default
		{
			if (typeof(TValue) == typeof(IDescribableSubject) &&
			    Actual is IVerificationResult<TVerify> verificationResult &&
			    new MyDescribableSubject<TVerify>(verificationResult.Object as IMock) is TValue describableSubject)
			{
				value = describableSubject;
				return true;
			}

			return base.TryGetValue(out value);
		}
	}

	private sealed class HasAtMostConstraint<TVerify>(
		ExpectationBuilder expectationBuilder,
		string it,
		ExpectationGrammars grammars,
		int expected)
		: ConstraintResult.WithValue<VerificationResult<TVerify>>(it, grammars),
			IValueConstraint<VerificationResult<TVerify>>
	{
		private int _count;
		private string? _expectation;

		public ConstraintResult IsMetBy(VerificationResult<TVerify> actual)
		{
			IVerificationResult result = actual;
			_expectation = result.Expectation;
			Actual = actual;
			Outcome = result.Verify(interactions =>
			{
				string context = Formatter.Format(interactions, FormattingOptions.MultipleLines);
				expectationBuilder.UpdateContexts(contexts => contexts.Add(
					new ResultContext.SyncCallback("Matching Interactions", () => context)));
				_count = interactions.Length;
				return interactions.Length <= expected;
			})
				? Outcome.Success
				: Outcome.Failure;
			if (Outcome == Outcome.Failure)
			{
				AppendAllInteractions(expectationBuilder,
					((IVerificationResult<TVerify>)actual).Object as IMock);
			}
			return this;
		}

		protected override void AppendNormalExpectation(StringBuilder stringBuilder, string? indentation = null)
			=> stringBuilder.Append(_expectation).Append(" at most ").Append(expected.ToAmountString());

		protected override void AppendNormalResult(StringBuilder stringBuilder, string? indentation = null)
			=> stringBuilder.Append("found ").Append(It).Append(' ').Append(_count.ToAmountString());

		protected override void AppendNegatedExpectation(StringBuilder stringBuilder, string? indentation = null)
			=> stringBuilder.Append(_expectation).Append(" more than ").Append(expected.ToAmountString());

		protected override void AppendNegatedResult(StringBuilder stringBuilder, string? indentation = null)
		{
			if (_count == 0)
			{
				stringBuilder.Append("never found ").Append(It);
			}
			else
			{
				stringBuilder.Append("found ").Append(It).Append(" only ").Append(_count.ToAmountString());
			}
		}

		public override bool TryGetValue<TValue>([NotNullWhen(true)] out TValue? value) where TValue : default
		{
			if (typeof(TValue) == typeof(IDescribableSubject) &&
			    Actual is IVerificationResult<TVerify> verificationResult &&
			    new MyDescribableSubject<TVerify>(verificationResult.Object as IMock) is TValue describableSubject)
			{
				value = describableSubject;
				return true;
			}

			return base.TryGetValue(out value);
		}
	}

	private sealed class HasAtLeastConstraint<TVerify>(
		ExpectationBuilder expectationBuilder,
		string it,
		ExpectationGrammars grammars,
		int expected,
		WithinOptions options)
		: ConstraintResult.WithValue<VerificationResult<TVerify>>(it, grammars),
			IAsyncConstraint<VerificationResult<TVerify>>
	{
		private int _count;
		private string? _expectation;

		public async Task<ConstraintResult> IsMetBy(VerificationResult<TVerify> actual,
			CancellationToken cancellationToken)
		{
			if (options.CancellationToken is not null)
			{
				actual = actual.WithCancellation(options.CancellationToken.Value);
			}

			if (options.Timeout is not null)
			{
				actual = actual.Within(options.Timeout.Value);
			}
			else if (expectationBuilder.Timeout is not null)
			{
				actual = actual.Within(expectationBuilder.Timeout.Value);
			}

			if (actual is IAsyncVerificationResult asyncVerificationResult)
			{
				_expectation = asyncVerificationResult.Expectation;
				Actual = actual;
				try
				{
					Outcome = await asyncVerificationResult.VerifyAsync(interactions =>
					{
						string interactionsText = Formatter.Format(interactions, FormattingOptions.MultipleLines);
						expectationBuilder.UpdateContexts(contexts => contexts
							.Remove("Matching Interactions")
							.Add(new ResultContext.SyncCallback("Matching Interactions", () => interactionsText)));
						_count = interactions.Length;
						return interactions.Length >= expected;
					})
						? Outcome.Success
						: Outcome.Failure;
					if (Outcome == Outcome.Failure)
					{
						AppendAllInteractions(expectationBuilder,
							((IVerificationResult<TVerify>)actual).Object as IMock);
					}
					return this;
				}
				catch (MockVerificationTimeoutException)
				{
					string interactionsText = Formatter.Format(((IVerificationResult)actual).Interactions,
						FormattingOptions.MultipleLines);
					expectationBuilder.UpdateContexts(contexts => contexts
						.Remove("Matching Interactions")
						.Add(new ResultContext.SyncCallback("Matching Interactions",
							() => interactionsText)));
					Outcome = Outcome.Failure;
					return this;
				}
			}

			IVerificationResult result = actual;
			_expectation = result.Expectation;
			Actual = actual;
			Outcome = result.Verify(interactions =>
			{
				string context = Formatter.Format(interactions, FormattingOptions.MultipleLines);
				expectationBuilder.UpdateContexts(contexts => contexts
					.Add(new ResultContext.SyncCallback("Matching Interactions", () => context)));
				_count = interactions.Length;
				return interactions.Length >= expected;
			})
				? Outcome.Success
				: Outcome.Failure;
			if (Outcome == Outcome.Failure)
			{
				AppendAllInteractions(expectationBuilder,
					((IVerificationResult<TVerify>)actual).Object as IMock);
			}
			return this;
		}

		protected override void AppendNormalExpectation(StringBuilder stringBuilder, string? indentation = null)
			=> stringBuilder.Append(_expectation).Append(" at least ").Append(expected.ToAmountString());

		protected override void AppendNormalResult(StringBuilder stringBuilder, string? indentation = null)
		{
			if (_count == 0)
			{
				stringBuilder.Append("never found ").Append(It);
			}
			else
			{
				stringBuilder.Append("found ").Append(It).Append(" only ").Append(_count.ToAmountString());
			}
		}

		protected override void AppendNegatedExpectation(StringBuilder stringBuilder, string? indentation = null)
			=> stringBuilder.Append(_expectation).Append(" less than ").Append(expected.ToAmountString());

		protected override void AppendNegatedResult(StringBuilder stringBuilder, string? indentation = null)
			=> stringBuilder.Append("found ").Append(It).Append(' ').Append(_count.ToAmountString());

		public override bool TryGetValue<TValue>([NotNullWhen(true)] out TValue? value) where TValue : default
		{
			if (typeof(TValue) == typeof(IDescribableSubject) &&
			    Actual is IVerificationResult<TVerify> verificationResult &&
			    new MyDescribableSubject<TVerify>(verificationResult.Object as IMock) is TValue describableSubject)
			{
				value = describableSubject;
				return true;
			}

			return base.TryGetValue(out value);
		}
	}
}
