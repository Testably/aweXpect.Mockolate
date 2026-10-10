using Mockolate;
using Xunit.Sdk;

namespace aweXpect.Mockolate.Tests;

public sealed partial class ThatVerificationResultIs
{
	public sealed class AtMostOnceTests
	{
		[Fact]
		public async Task WhenInvokedMoreThanTwice_ShouldFail()
		{
			IMyService sut = IMyService.CreateMock();

			sut.MyMethod(1, false);
			sut.MyMethod(1, false);
			sut.MyMethod(1, false);

			async Task Act()
			{
				await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).AtMostOnce();
			}

			await That(Act).Throws<XunitException>()
				.WithMessage("""
				             Expected that the aweXpect.Mockolate.Tests.ThatVerificationResultIs.IMyService mock
				             invoked method MyMethod(1, false) at most once,
				             but it was found 3 times

				             Matching Interactions:
				             [
				               invoke method MyMethod(1, False),
				               invoke method MyMethod(1, False),
				               invoke method MyMethod(1, False)
				             ]

				             All Interactions:
				             [
				               invoke method MyMethod(1, False),
				               invoke method MyMethod(1, False),
				               invoke method MyMethod(1, False)
				             ]
				             """);
		}

		[Fact]
		public async Task WhenInvokedNever_ShouldSucceed()
		{
			IMyService sut = IMyService.CreateMock();

			async Task Act()
			{
				await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).AtMostOnce();
			}

			await That(Act).DoesNotThrow();
		}

		[Fact]
		public async Task WhenInvokedOnce_ShouldSucceed()
		{
			IMyService sut = IMyService.CreateMock();

			sut.MyMethod(1, false);

			async Task Act()
			{
				await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).AtMostOnce();
			}

			await That(Act).DoesNotThrow();
		}

		[Fact]
		public async Task WhenInvokedTwice_ShouldFail()
		{
			IMyService sut = IMyService.CreateMock();

			sut.MyMethod(1, false);
			sut.MyMethod(1, false);
			sut.MyMethod(2, false);

			async Task Act()
			{
				await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).AtMostOnce();
			}

			await That(Act).Throws<XunitException>()
				.WithMessage("""
				             Expected that the aweXpect.Mockolate.Tests.ThatVerificationResultIs.IMyService mock
				             invoked method MyMethod(1, false) at most once,
				             but it was found twice

				             Matching Interactions:
				             [
				               invoke method MyMethod(1, False),
				               invoke method MyMethod(1, False)
				             ]

				             All Interactions:
				             [
				               invoke method MyMethod(1, False),
				               invoke method MyMethod(1, False),
				               invoke method MyMethod(2, False)
				             ]
				             """);
		}
	}
}
