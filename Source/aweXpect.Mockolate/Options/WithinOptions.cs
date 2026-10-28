using System;
using aweXpect.Core;

namespace aweXpect.Mockolate.Options;

/// <summary>
///     The options for a verification result which allows specifying a timeout for the verification.
/// </summary>
/// <remarks>
///     An immutable value: <see cref="Within(TimeSpan)" /> returns new options, and <see langword="default" /> has no
///     timeout, like <see cref="Default" />.
/// </remarks>
[ImmutableOptions]
public readonly struct WithinOptions
{
	private WithinOptions(TimeSpan timeout)
	{
		Timeout = timeout;
	}

	/// <summary>
	///     The options without a timeout.
	/// </summary>
	public static WithinOptions Default => default;

	/// <summary>
	///     The timeout that is applied to the verification.
	/// </summary>
	public TimeSpan? Timeout { get; }

	/// <summary>
	///     Returns options that wait for the verification within the given <paramref name="timeout" />.
	/// </summary>
	/// <remarks>
	///     <see cref="System.Threading.Timeout.InfiniteTimeSpan" /> imposes no limit.
	/// </remarks>
	/// <exception cref="ArgumentOutOfRangeException">The <paramref name="timeout" /> is negative.</exception>
	public static WithinOptions Within(TimeSpan timeout)
	{
		if (timeout < TimeSpan.Zero && timeout != System.Threading.Timeout.InfiniteTimeSpan)
		{
			throw Tracing.WriteException(
				new ArgumentOutOfRangeException(nameof(timeout), "The timeout must not be negative."));
		}

		return new WithinOptions(timeout);
	}

	/// <inheritdoc cref="object.ToString()" />
	/// <remarks>
	///     An infinite timeout is omitted, because it does not add any information to the expectation.
	/// </remarks>
	public override string ToString()
		=> Timeout is { } timeout && timeout != System.Threading.Timeout.InfiniteTimeSpan
			? $" within {Formatter.Format(timeout)}"
			: "";
}
