using System;
using System.Threading;
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
	/// <remarks>
	///     <see cref="Timeout.InfiniteTimeSpan" /> imposes no limit.
	/// </remarks>
	/// <exception cref="ArgumentOutOfRangeException">The <paramref name="timeout" /> is negative.</exception>
	/// <exception cref="InvalidOperationException">A timeout is already set.</exception>
	public AndOrWithinResult<TType, TThat> Within(TimeSpan timeout)
	{
		if (options.Timeout is not null)
		{
			throw Tracing.WriteException(
				new InvalidOperationException($"{nameof(Within)} cannot be specified more than once."));
		}

		if (timeout < TimeSpan.Zero && timeout != Timeout.InfiniteTimeSpan)
		{
			throw Tracing.WriteException(
				new ArgumentOutOfRangeException(nameof(timeout), "The timeout must not be negative."));
		}

		options.Timeout = timeout;
		return this;
	}
}
