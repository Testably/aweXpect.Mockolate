using System.Diagnostics;
using System.Threading;
using aweXpect.Chronology;
using Mockolate;

namespace aweXpect.Mockolate.Tests;

public sealed partial class ThatVerificationResultIs
{
	public sealed class NeverTests
	{
		[Test]
		public async Task WhenInvokedMoreThanTwice_ShouldFail()
		{
			IMyService sut = IMyService.CreateMock();

			sut.MyMethod(1, false);
			sut.MyMethod(1, false);
			sut.MyMethod(2, false);
			sut.MyMethod(1, false);

			async Task Act()
			{
				await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).Never();
			}

			await That(Act).Throws<FailException>()
				.WithMessage("""
				             Expected that the aweXpect.Mockolate.Tests.ThatVerificationResultIs.IMyService mock
				             never invoked method MyMethod(1, false),
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
				               invoke method MyMethod(2, False),
				               invoke method MyMethod(1, False)
				             ]
				             """);
		}

		[Test]
		public async Task WhenInvoked_WithCancellation_ShouldFailWithoutWaiting()
		{
			IMyService sut = IMyService.CreateMock();
			using CancellationTokenSource cts = new(30.Seconds());

			sut.MyMethod(1, false);

			async Task Act()
			{
				await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).Never().WithCancellation(cts.Token);
			}

			await That(Act).Throws<FailException>()
				.WithMessage("""
				             Expected that the aweXpect.Mockolate.Tests.ThatVerificationResultIs.IMyService mock
				             never invoked method MyMethod(1, false),
				             but it was found once

				             Matching Interactions:
				             [
				               invoke method MyMethod(1, False)
				             ]

				             All Interactions:
				             [
				               invoke method MyMethod(1, False)
				             ]
				             """)
				.Because("further interactions cannot satisfy Never, so the cancellation token must not make it wait");
		}

		[Test]
		public async Task WhenInvoked_WithTimeout_ShouldFailWithoutWaiting()
		{
			IMyService sut = IMyService.CreateMock();
			Stopwatch stopwatch = Stopwatch.StartNew();

			sut.MyMethod(1, false);

			async Task Act()
			{
				await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).Never().WithTimeout(30.Seconds());
			}

			await That(Act).Throws<FailException>()
				.WithMessage("""
				             Expected that the aweXpect.Mockolate.Tests.ThatVerificationResultIs.IMyService mock
				             never invoked method MyMethod(1, false),
				             but it was found once

				             Matching Interactions:
				             [
				               invoke method MyMethod(1, False)
				             ]

				             All Interactions:
				             [
				               invoke method MyMethod(1, False)
				             ]
				             """);
			await That(stopwatch.Elapsed).IsLessThan(10.Seconds())
				.Because("further interactions cannot satisfy Never, so the timeout must not make it wait");
		}

		[Test]
		public async Task WhenInvokedNever_ShouldSucceed()
		{
			IMyService sut = IMyService.CreateMock();

			async Task Act()
			{
				await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).Never();
			}

			await That(Act).DoesNotThrow();
		}

		[Test]
		public async Task WhenInvokedOnce_ShouldFail()
		{
			IMyService sut = IMyService.CreateMock();

			sut.MyMethod(1, false);

			async Task Act()
			{
				await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).Never();
			}

			await That(Act).Throws<FailException>()
				.WithMessage("""
				             Expected that the aweXpect.Mockolate.Tests.ThatVerificationResultIs.IMyService mock
				             never invoked method MyMethod(1, false),
				             but it was found once

				             Matching Interactions:
				             [
				               invoke method MyMethod(1, False)
				             ]

				             All Interactions:
				             [
				               invoke method MyMethod(1, False)
				             ]
				             """);
		}

		[Test]
		public async Task WhenInvokedTwice_ShouldFail()
		{
			IMyService sut = IMyService.CreateMock();

			sut.MyMethod(1, false);
			sut.MyMethod(1, false);

			async Task Act()
			{
				await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).Never();
			}

			await That(Act).Throws<FailException>()
				.WithMessage("""
				             Expected that the aweXpect.Mockolate.Tests.ThatVerificationResultIs.IMyService mock
				             never invoked method MyMethod(1, false),
				             but it was found twice

				             Matching Interactions:
				             [
				               invoke method MyMethod(1, False),
				               invoke method MyMethod(1, False)
				             ]

				             All Interactions:
				             [
				               invoke method MyMethod(1, False),
				               invoke method MyMethod(1, False)
				             ]
				             """);
		}

		public sealed class NegatedTests
		{
			[Test]
			public async Task WhenInvokedNever_ShouldFail()
			{
				IMyService sut = IMyService.CreateMock();

				async Task Act()
				{
					await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false)))
						.DoesNotComplyWith(it => it.Never());
				}

				await That(Act).Throws<FailException>()
					.WithMessage("""
					             Expected that the aweXpect.Mockolate.Tests.ThatVerificationResultIs.IMyService mock
					             invoked method MyMethod(1, false) at least once,
					             but it was

					             Matching Interactions:
					             []

					             All Interactions:
					             []
					             """);
			}

			[Test]
			public async Task WhenInvokedOnce_ShouldSucceed()
			{
				IMyService sut = IMyService.CreateMock();

				sut.MyMethod(1, false);

				async Task Act()
				{
					await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false)))
						.DoesNotComplyWith(it => it.Never());
				}

				await That(Act).DoesNotThrow();
			}
		}
	}
}
