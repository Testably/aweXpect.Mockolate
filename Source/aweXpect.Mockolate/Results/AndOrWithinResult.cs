using System;
using System.Threading;
using aweXpect.Core;
using aweXpect.Mockolate.Options;
using aweXpect.Results;

namespace aweXpect.Mockolate.Results;

/// <summary>
///     The result of a verification result which allows specifying a timeout for the verification.
/// </summary>
/// <remarks>
///     <seealso cref="AndOrResult{TType, TThat}" />
/// </remarks>
public class AndOrWithinResult<TType, TThat>
	: AndOrResult<TType, TThat, AndOrWithinResult<TType, TThat>>, IOptionsProvider<WithinOptions>
{
	private readonly OptionsHolder<WithinOptions> _options;

	internal AndOrWithinResult(ExpectationBuilder expectationBuilder, TThat returnValue,
		OptionsHolder<WithinOptions> options)
		: base(expectationBuilder, returnValue)
	{
		_options = options;
	}

	/// <inheritdoc cref="IOptionsProvider{TOptions}.Options" />
	OptionsHolder<WithinOptions> IOptionsProvider<WithinOptions>.Options => _options;

	/// <summary>
	///     …within the given <paramref name="timeout" />.
	/// </summary>
	/// <remarks>
	///     <see cref="Timeout.InfiniteTimeSpan" /> imposes no limit. A repeated call replaces the earlier timeout.
	/// </remarks>
	/// <exception cref="ArgumentOutOfRangeException">The <paramref name="timeout" /> is negative.</exception>
	public AndOrWithinResult<TType, TThat> Within(TimeSpan timeout)
	{
		_options.Value = _options.Value.Within(timeout);
		return this;
	}
}
