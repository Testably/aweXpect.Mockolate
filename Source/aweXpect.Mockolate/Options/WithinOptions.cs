using System;

namespace aweXpect.Options;

/// <summary>
///     The options for a verification result which allows specifying a timeout for the verification.
/// </summary>
public class WithinOptions
{
	/// <summary>
	///     The timeout that is applied to the verification.
	/// </summary>
	public TimeSpan? Timeout { get; set; }

	/// <inheritdoc cref="object.ToString()" />
	/// <remarks>
	///     An infinite timeout is omitted, because it does not add any information to the expectation.
	/// </remarks>
	public override string ToString()
		=> Timeout is { } timeout && timeout != System.Threading.Timeout.InfiniteTimeSpan
			? $" within {Formatter.Format(timeout)}"
			: "";
}
