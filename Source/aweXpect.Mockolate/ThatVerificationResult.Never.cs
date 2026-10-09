using aweXpect.Core;
using aweXpect.Helpers;
using aweXpect.Options;
using aweXpect.Results;
using Mockolate.Verify;

namespace aweXpect;

public static partial class ThatVerificationResult
{
	/// <summary>
	///     Verifies that the checked interaction happened never.
	/// </summary>
	[GuaranteesNotNull]
	public static AndOrResult<VerificationResult<TVerify>, IThat<VerificationResult<TVerify>>>
		Never<TVerify>(this IThat<VerificationResult<TVerify>> subject)
	{
		WithinOptions options = new();
		return new AndOrResult<VerificationResult<TVerify>, IThat<VerificationResult<TVerify>>>(
			subject.Get().ExpectationBuilder.AddConstraint((expectationBuilder, it, grammars)
				=> new HasExactlyConstraint<TVerify>(expectationBuilder, it, grammars, 0, options)),
			subject);
	}
}
