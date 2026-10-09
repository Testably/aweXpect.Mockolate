using System;
using aweXpect.Core;
using aweXpect.Options;

namespace aweXpect.Results;

/// <summary>
///     The result of a verification result which allows specifying a timeout for the verification.
/// </summary>
/// <remarks>
///     <seealso cref="AndOrResult{TType, TThat}" />
/// </remarks>
public class AndOrWithinResult<TType, TThat>(
	ExpectationBuilder expectationBuilder,
	TThat returnValue,
	WithinOptions options)
	: AndOrResult<TType, TThat, AndOrWithinResult<TType, TThat>>(expectationBuilder, returnValue)
{
	/// <summary>
	///     …within the given <paramref name="timeout" />.
	/// </summary>
	public AndOrWithinResult<TType, TThat> Within(TimeSpan timeout)
	{
		options.Timeout = timeout;
		return this;
	}
}
