using System.Diagnostics;
using System.Threading;
using aweXpect.Chronology;
using aweXpect.Core;
using aweXpect.Mockolate.Tests.TestHelpers;
using Mockolate;

namespace aweXpect.Mockolate.Tests;

public sealed partial class ThatVerificationResultIs
{
	public sealed class WithinTests
	{
		[Test]
		public async Task WhenSpecifiedTwice_ShouldUseLastTimeout()
		{
			IMyService sut = IMyService.CreateMock();

			async Task Act()
			{
				await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).Once().Within(30.Seconds())
					.Within(50.Milliseconds());
			}

			await That(Act).Throws<FailException>()
				.WithMessage("""
				             Expected that the aweXpect.Mockolate.Tests.ThatVerificationResultIs.IMyService mock
				             invoked method MyMethod(1, false) exactly once within 0:00.050,
				             but it was never found

				             Matching Interactions:
				             []

				             All Interactions:
				             []
				             """)
				.Because("a repeated option replaces the earlier one, like in core");
		}

		[Test]
		public async Task WhenTimeoutIsInfinite_ShouldSucceed()
		{
			IMyService sut = IMyService.CreateMock();
			sut.MyMethod(1, false);

			async Task Act()
			{
				await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).Once().Within(Timeout.InfiniteTimeSpan);
			}

			await That(Act).DoesNotThrow()
				.Because("an infinite timeout imposes no limit, like in core");
		}

		[Test]
		public async Task WhenTimeoutIsNegative_ShouldThrowArgumentOutOfRangeException()
		{
			IMyService sut = IMyService.CreateMock();

			async Task Act()
			{
				await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).Once().Within(-1.Seconds());
			}

			await That(Act).Throws<ArgumentOutOfRangeException>()
				.WithParamName("timeout").And
				.WithMessage("The timeout must not be negative.").AsPrefix()
				.Because("a negative timeout is rejected with the same message as in core");
		}

		[Test]
		public async Task WithTimeProvider_ShouldElapseOnItsClock()
		{
			IMyService sut = IMyService.CreateMock();
			VirtualTimeProvider timeProvider = new();
			Stopwatch stopwatch = Stopwatch.StartNew();

			async Task Act()
			{
				await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).Once().Within(30.Seconds())
					.WithTimeProvider(timeProvider);
			}

			Task act = Act();
			// The timeout is only scheduled once the evaluation reaches the wait.
			while (!act.IsCompleted)
			{
				timeProvider.Advance(31.Seconds());
				await Task.Delay(10);
			}

			await That(() => act).Throws<FailException>()
				.WithMessage("""
				             Expected that the aweXpect.Mockolate.Tests.ThatVerificationResultIs.IMyService mock
				             invoked method MyMethod(1, false) exactly once within 0:30,
				             but it was never found

				             Matching Interactions:
				             []

				             All Interactions:
				             []
				             """);
			await That(stopwatch.Elapsed).IsLessThan(10.Seconds())
				.Because("the timeout elapses on the clock of the evaluation");
		}
	}
}
