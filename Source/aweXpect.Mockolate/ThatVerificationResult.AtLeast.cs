using aweXpect.Core;
using aweXpect.Helpers;
using aweXpect.Options;
using aweXpect.Results;
using Mockolate.Verify;

namespace aweXpect;

public static partial class ThatVerificationResult
{
	/// <summary>
	///     Verifies that the checked interaction happened at least the number of <paramref name="times" />.
	/// </summary>
	[GuaranteesNotNull]
	public static AndOrWithinResult<VerificationResult<TVerify>, IThat<VerificationResult<TVerify>>>
		AtLeast<TVerify>(this IThat<VerificationResult<TVerify>> subject, Times times)
	{
		WithinOptions options = new();
		return new AndOrWithinResult<VerificationResult<TVerify>, IThat<VerificationResult<TVerify>>>(
			subject.Get().ExpectationBuilder.AddConstraint((expectationBuilder, it, grammars)
				=> new HasAtLeastConstraint<TVerify>(expectationBuilder, it, grammars, times.Value, options)),
			subject,
			options);
	}
}
