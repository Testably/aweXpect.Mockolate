using aweXpect.Core;
using aweXpect.Helpers;
using aweXpect.Results;
using Mockolate.Verify;

namespace aweXpect;

public static partial class ThatVerificationResult
{
	/// <summary>
	///     Verifies that the checked interaction happened at most once.
	/// </summary>
	[GuaranteesNotNull]
	public static AndOrResult<VerificationResult<TVerify>, IThat<VerificationResult<TVerify>>>
		AtMostOnce<TVerify>(this IThat<VerificationResult<TVerify>> subject)
		=> new(subject.Get().ExpectationBuilder.AddConstraint((it, grammars)
				=> new HasAtMostConstraint<TVerify>(it, grammars, 1)),
			subject);
}
