using System.Text;
using System.Threading;
using System.Threading.Tasks;
using aweXpect.Core;
using aweXpect.Core.Constraints;
using aweXpect.Helpers;
using aweXpect.Options;
using aweXpect.Results;
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
		: VerificationCountConstraint<TVerify>(it, grammars),
			IAsyncConstraint<VerificationResult<TVerify>>
	{
		public async ValueTask<ConstraintResult> IsMetBy(VerificationResult<TVerify> actual,
			CancellationToken cancellationToken)
		{
			await VerifyAsync(actual, expectationBuilder, options);
			return this;
		}

		protected override bool IsMet(int count) => count >= minimum && count <= maximum;

		protected override void AppendNormalExpectation(StringBuilder stringBuilder, string? indentation = null)
			=> stringBuilder.Append(Expectation).Append(" between ").Append(minimum).Append(" and ").Append(maximum)
				.Append(" times");

		protected override void AppendNormalResult(StringBuilder stringBuilder, string? indentation = null)
		{
			if (Count == 0)
			{
				stringBuilder.Append("never found ").Append(It);
			}
			else
			{
				stringBuilder.Append("found ").Append(It).Append(Count < minimum ? " only " : " ")
					.Append(Count.ToAmountString());
			}
		}

		protected override void AppendNegatedExpectation(StringBuilder stringBuilder, string? indentation = null)
			=> stringBuilder.Append(Expectation).Append(" not between ").Append(minimum).Append(" and ")
				.Append(maximum).Append(" times");

		protected override void AppendNegatedResult(StringBuilder stringBuilder, string? indentation = null)
			=> stringBuilder.Append("found ").Append(It).Append(' ').Append(Count.ToAmountString());
	}
}
