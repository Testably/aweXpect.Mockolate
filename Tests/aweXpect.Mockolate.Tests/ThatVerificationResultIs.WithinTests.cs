using System.Threading;
using aweXpect.Chronology;
using Mockolate;

namespace aweXpect.Mockolate.Tests;

public sealed partial class ThatVerificationResultIs
{
	public sealed class WithinTests
	{
		[Fact]
		public async Task WhenSpecifiedTwice_ShouldThrowInvalidOperationException()
		{
			IMyService sut = IMyService.CreateMock();

			async Task Act()
			{
				await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).Once().Within(30.Seconds())
					.Within(50.Milliseconds());
			}

			await That(Act).Throws<InvalidOperationException>()
				.WithMessage("Within cannot be specified more than once.")
				.Because("the second timeout would silently replace the first one");
		}

		[Fact]
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

		[Fact]
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
	}
}
