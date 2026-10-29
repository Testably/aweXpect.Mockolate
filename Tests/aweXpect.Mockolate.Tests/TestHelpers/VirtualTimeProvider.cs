using System.Collections.Generic;
using System.Threading;

namespace aweXpect.Mockolate.Tests.TestHelpers;

/// <summary>
///     A clock for an evaluation, which only moves with <see cref="Advance(TimeSpan)" />.
/// </summary>
/// <remarks>
///     The timers of the evaluation, e.g. the timeout of <c>Within(…)</c>, fire when <see cref="Advance(TimeSpan)" />
///     reaches them. The waits of Mockolate itself still elapse in real time.
/// </remarks>
internal sealed class VirtualTimeProvider : TimeProvider
{
	private readonly object _lock = new();
	private readonly List<VirtualTimer> _timers = [];
	private long _ticks;

	/// <inheritdoc />
	public override long TimestampFrequency => TimeSpan.TicksPerSecond;

	/// <inheritdoc />
	public override long GetTimestamp()
	{
		lock (_lock)
		{
			return _ticks;
		}
	}

	/// <inheritdoc />
	public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
	{
		VirtualTimer timer = new(this, callback, state);
		if (dueTime != Timeout.InfiniteTimeSpan)
		{
			lock (_lock)
			{
				timer.DueTicks = _ticks + dueTime.Ticks;
				_timers.Add(timer);
			}
		}

		return timer;
	}

	/// <summary>
	///     Advances the clock by the <paramref name="duration" /> and fires the timers that are due.
	/// </summary>
	public void Advance(TimeSpan duration)
	{
		List<VirtualTimer> dueTimers;
		lock (_lock)
		{
			_ticks += duration.Ticks;
			dueTimers = _timers.FindAll(timer => timer.DueTicks <= _ticks);
			_timers.RemoveAll(timer => timer.DueTicks <= _ticks);
		}

		foreach (VirtualTimer timer in dueTimers)
		{
			timer.Fire();
		}
	}

	private void Remove(VirtualTimer timer)
	{
		lock (_lock)
		{
			_timers.Remove(timer);
		}
	}

	private sealed class VirtualTimer(VirtualTimeProvider timeProvider, TimerCallback callback, object? state)
		: ITimer
	{
		public long DueTicks { get; set; }

		public bool Change(TimeSpan dueTime, TimeSpan period)
			=> throw new NotSupportedException("The virtual clock does not reschedule a timer.");

		public void Dispose() => timeProvider.Remove(this);

		public ValueTask DisposeAsync()
		{
			Dispose();
			return default;
		}

		public void Fire() => callback(state);
	}
}
