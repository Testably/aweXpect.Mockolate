using aweXpect.Core;
using aweXpect.Helpers;
using aweXpect.Options;
using aweXpect.Results;
using Mockolate.Verify;

namespace aweXpect;

public static partial class ThatVerificationResult
{
	/// <summary>
	///     Verifies that the checked interaction happened exactly once.
	/// </summary>
	[GuaranteesNotNull]
	public static AndOrWithinResult<VerificationResult<TVerify>, IThat<VerificationResult<TVerify>>>
		Once<TVerify>(this IThat<VerificationResult<TVerify>> subject)
	{
		WithinOptions options = new();
		return new AndOrWithinResult<VerificationResult<TVerify>, IThat<VerificationResult<TVerify>>>(
			subject.Get().ExpectationBuilder.AddConstraint((expectationBuilder, it, grammars)
				=> new HasExactlyConstraint<TVerify>(expectationBuilder, it, grammars, 1, options)),
			subject,
			options);
	}
}
