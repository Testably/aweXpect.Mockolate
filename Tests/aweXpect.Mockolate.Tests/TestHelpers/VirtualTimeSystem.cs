using System.Threading;
using aweXpect.Core.Internal;

namespace aweXpect.Mockolate.Tests.TestHelpers;

/// <summary>
///     A clock for the durations that an evaluation measures, which only moves with <see cref="Advance(TimeSpan)" />.
/// </summary>
/// <remarks>
///     The timeouts still elapse in real time, as the waits of Mockolate do not use the clock of the evaluation.
/// </remarks>
internal sealed class VirtualTimeSystem : ITimeSystem
{
	private long _ticks;

	public long GetTimestamp() => Interlocked.Read(ref _ticks);

	public TimeSpan GetElapsedTime(long startTimestamp) => TimeSpan.FromTicks(GetTimestamp() - startTimestamp);

	public Task Delay(TimeSpan delay, CancellationToken cancellationToken)
	{
		Advance(delay);
		return Task.CompletedTask;
	}

	public void CancelAfter(CancellationTokenSource cancellationTokenSource, TimeSpan delay)
		=> cancellationTokenSource.CancelAfter(delay);

	public void Advance(TimeSpan duration) => Interlocked.Add(ref _ticks, duration.Ticks);
}
