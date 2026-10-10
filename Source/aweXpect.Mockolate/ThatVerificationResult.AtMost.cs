using System;
using aweXpect.Core;
using aweXpect.Core.Extending;
using aweXpect.Results;
using Mockolate.Verify;

namespace aweXpect;

public static partial class ThatVerificationResult
{
	/// <summary>
	///     Verifies that the checked interaction happened at most the number of <paramref name="times" />.
	/// </summary>
	/// <exception cref="ArgumentOutOfRangeException">The <paramref name="times" /> are negative.</exception>
	[GuaranteesNotNull]
	public static AndOrResult<VerificationResult<TVerify>, IThat<VerificationResult<TVerify>>>
		AtMost<TVerify>(this IThat<VerificationResult<TVerify>> subject, Times times)
	{
		if (times.Value < 0)
		{
			throw Tracing.WriteException(
				new ArgumentOutOfRangeException(nameof(times), "The maximum must not be negative."));
		}

		return new AndOrResult<VerificationResult<TVerify>, IThat<VerificationResult<TVerify>>>(
			subject.Get().ExpectationBuilder.AddConstraint(times.Value, static (t, expectationBuilder, it, grammars)
				=> new HasAtMostConstraint<TVerify>(expectationBuilder, it, grammars, t)),
			subject);
	}
}
