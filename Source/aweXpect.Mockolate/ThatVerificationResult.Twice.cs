using aweXpect.Core;
using aweXpect.Core.Extending;
using aweXpect.Options;
using aweXpect.Results;
using Mockolate.Verify;

namespace aweXpect;

public static partial class ThatVerificationResult
{
	/// <summary>
	///     Verifies that the checked interaction happened exactly twice.
	/// </summary>
	[GuaranteesNotNull]
	public static AndOrWithinResult<VerificationResult<TVerify>, IThat<VerificationResult<TVerify>>> Twice<TVerify>(
		this IThat<VerificationResult<TVerify>> subject)
	{
		WithinOptions options = new();
		return new AndOrWithinResult<VerificationResult<TVerify>, IThat<VerificationResult<TVerify>>>(
			subject.Get().ExpectationBuilder.AddConstraint(options, static (o, expectationBuilder, it, grammars)
				=> new HasExactlyConstraint<TVerify>(expectationBuilder, it, grammars, 2, o)),
			subject,
			options);
	}
}
