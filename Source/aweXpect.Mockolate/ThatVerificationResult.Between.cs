using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using aweXpect.Core;
using aweXpect.Core.Constraints;
using aweXpect.Helpers;
using aweXpect.Options;
using aweXpect.Results;
using Mockolate;
using Mockolate.Exceptions;
using Mockolate.Verify;

namespace aweXpect;

public static partial class ThatVerificationResult
{
	/// <summary>
	///     Verifies that the checked interaction happened between <paramref name="minimum" />…
	/// </summary>
	public static BetweenResult<AndOrWithinResult<VerificationResult<TVerify>, IThat<VerificationResult<TVerify>>>,
			Times>
		Between<TVerify>(this IThat<VerificationResult<TVerify>> subject, int minimum)
	{
		WithinOptions options = new();
		return new
			BetweenResult<AndOrWithinResult<VerificationResult<TVerify>, IThat<VerificationResult<TVerify>>>,
				Times>(maximum
				=> new AndOrWithinResult<VerificationResult<TVerify>, IThat<VerificationResult<TVerify>>>(
					subject.Get().ExpectationBuilder.AddConstraint((expectationBuilder, it, grammars)
						=> new HasBetweenConstraint<TVerify>(expectationBuilder, it, grammars, minimum,
							maximum.Value, options)),
					subject,
					options));
	}

	private sealed class HasBetweenConstraint<TVerify>(
		ExpectationBuilder expectationBuilder,
		string it,
		ExpectationGrammars grammars,
		int minimum,
		int maximum,
		WithinOptions options)
		: ConstraintResult.WithValue<VerificationResult<TVerify>>(it, grammars),
			IAsyncConstraint<VerificationResult<TVerify>>
	{
		private int _count = -1;
		private string _expectation = "";

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
						return interactions.Length >= minimum && interactions.Length <= maximum;
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
				return interactions.Length >= minimum && interactions.Length <= maximum;
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
			=> stringBuilder.Append(_expectation).Append(" between ").Append(minimum).Append(" and ").Append(maximum)
				.Append(" times");

		protected override void AppendNormalResult(StringBuilder stringBuilder, string? indentation = null)
		{
			if (_count == 0)
			{
				stringBuilder.Append("never found ").Append(It);
			}
			else
			{
				stringBuilder.Append("found ").Append(It).Append(_count < minimum ? " only " : " ")
					.Append(_count.ToAmountString());
			}
		}

		protected override void AppendNegatedExpectation(StringBuilder stringBuilder, string? indentation = null)
			=> stringBuilder.Append(_expectation).Append(" not between ").Append(minimum).Append(" and ")
				.Append(maximum).Append(" times");

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
