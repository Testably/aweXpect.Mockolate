using aweXpect.Core;
using aweXpect.Helpers;
using aweXpect.Results;
using Mockolate.Verify;

namespace aweXpect;

public static partial class ThatVerificationResult
{
	/// <summary>
	///     Verifies that the checked interaction happened at most the number of <paramref name="times" />.
	/// </summary>
	[GuaranteesNotNull]
	public static AndOrResult<VerificationResult<TVerify>, IThat<VerificationResult<TVerify>>>
		AtMost<TVerify>(this IThat<VerificationResult<TVerify>> subject, Times times)
		=> new(subject.Get().ExpectationBuilder.AddConstraint(times.Value,
				static (expected, it, grammars) => new HasAtMostConstraint<TVerify>(it, grammars, expected)),
			subject);
}
