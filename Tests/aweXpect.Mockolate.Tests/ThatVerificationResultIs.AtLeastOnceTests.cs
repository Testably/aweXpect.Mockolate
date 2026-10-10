using System.Diagnostics;
using System.Threading;
using aweXpect.Chronology;
using Mockolate;
using Xunit.Sdk;

namespace aweXpect.Mockolate.Tests;

public sealed partial class ThatVerificationResultIs
{
	public sealed class AtLeastOnceTests
	{
		[Fact]
		public async Task WhenInvokedInBackground_ShouldFail()
		{
			IMyService sut = IMyService.CreateMock();
			using CancellationTokenSource cts = new();
			CancellationToken token = cts.Token;

			Task backgroundTask = Task.Run(async () =>
			{
				try
				{
					await Task.Delay(5000, token);
					while (!token.IsCancellationRequested)
					{
						await Task.Delay(50, token).ConfigureAwait(false);
						sut.MyMethod(1, false);
					}
				}
				catch (OperationCanceledException)
				{
					// Ignore cancellation
				}
			}, CancellationToken.None);

			async Task Act()
			{
				await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).AtLeastOnce();
			}

			await That(Act).Throws<XunitException>()
				.WithMessage("""
				             Expected that the aweXpect.Mockolate.Tests.ThatVerificationResultIs.IMyService mock
				             invoked method MyMethod(1, false) at least once,
				             but it was never found

				             Matching Interactions:
				             []

				             All Interactions:
				             []
				             """);
			cts.Cancel();
			await backgroundTask;
		}

		[Theory]
		[InlineData(7)]
		[InlineData(1)]
		public async Task WhenInvokedInBackground_WithCancellation_ShouldSucceed(int invocationTimes)
		{
			IMyService sut = IMyService.CreateMock();
			using CancellationTokenSource cts = new(30.Seconds());
			CancellationToken token = cts.Token;

			Task backgroundTask = Task.Delay(50, token).ContinueWith(_ =>
			{
				for (int i = 0; i < invocationTimes; i++)
				{
					sut.MyMethod(1, false);
				}
			}, token);

			await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).AtLeastOnce().WithCancellation(token);

			await backgroundTask;
		}

		[Theory]
		[InlineData(7)]
		[InlineData(1)]
		public async Task WhenInvokedInBackground_Within_ShouldSucceed(int invocationTimes)
		{
			IMyService sut = IMyService.CreateMock();

			Task backgroundTask = Task.Delay(50).ContinueWith(_ =>
			{
				for (int i = 0; i < invocationTimes; i++)
				{
					sut.MyMethod(1, false);
				}
			});

			await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).AtLeastOnce().Within(30.Seconds());

			await backgroundTask;
		}

		[Theory]
		[InlineData(7)]
		[InlineData(1)]
		public async Task WhenInvokedInBackground_WithTimeout_ShouldSucceed(int invocationTimes)
		{
			IMyService sut = IMyService.CreateMock();

			Task backgroundTask = Task.Delay(50).ContinueWith(_ =>
			{
				for (int i = 0; i < invocationTimes; i++)
				{
					sut.MyMethod(1, false);
				}
			});

			await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).AtLeastOnce()
				.WithTimeout(30.Seconds());

			await backgroundTask;
		}

		[Fact]
		public async Task WhenInvokedMoreThanTwice_ShouldSucceed()
		{
			IMyService sut = IMyService.CreateMock();

			sut.MyMethod(1, false);
			sut.MyMethod(1, false);
			sut.MyMethod(1, false);

			async Task Act()
			{
				await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).AtLeastOnce();
			}

			await That(Act).DoesNotThrow();
		}

		[Fact]
		public async Task WhenInvokedNever_ShouldFail()
		{
			IMyService sut = IMyService.CreateMock();

			async Task Act()
			{
				await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).AtLeastOnce();
			}

			await That(Act).Throws<XunitException>()
				.WithMessage("""
				             Expected that the aweXpect.Mockolate.Tests.ThatVerificationResultIs.IMyService mock
				             invoked method MyMethod(1, false) at least once,
				             but it was never found

				             Matching Interactions:
				             []

				             All Interactions:
				             []
				             """);
		}

		[Fact]
		public async Task WhenInvokedOnce_ShouldSucceed()
		{
			IMyService sut = IMyService.CreateMock();

			sut.MyMethod(1, false);

			async Task Act()
			{
				await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).AtLeastOnce();
			}

			await That(Act).DoesNotThrow();
		}

		[Fact]
		public async Task WhenInvokedTwice_ShouldSucceed()
		{
			IMyService sut = IMyService.CreateMock();

			sut.MyMethod(1, false);
			sut.MyMethod(1, false);

			async Task Act()
			{
				await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).AtLeastOnce();
			}

			await That(Act).DoesNotThrow();
		}

		[Fact]
		public async Task WhenNotInvoked_Within_ShouldFailWithDescriptiveMessage()
		{
			IMyService sut = IMyService.CreateMock();

			sut.MyMethod(1, true);
			sut.MyMethod(2, true);

			async Task Act()
			{
				await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).AtLeastOnce().Within(50.Milliseconds());
			}

			await That(Act).Throws<XunitException>()
				.WithMessage("""
				             Expected that the aweXpect.Mockolate.Tests.ThatVerificationResultIs.IMyService mock
				             invoked method MyMethod(1, false) at least once within 0:00.050,
				             but it was never found

				             Matching Interactions:
				             []

				             All Interactions:
				             [
				               invoke method MyMethod(1, True),
				               invoke method MyMethod(2, True)
				             ]
				             """);
		}

		public sealed class NegatedTests
		{
			[Fact]
			public async Task WhenInvokedInBackground_Within_ShouldFailWhenInvoked()
			{
				IMyService sut = IMyService.CreateMock();
				Task backgroundTask = Task.Delay(50).ContinueWith(_ => sut.MyMethod(1, false));

				async Task Act()
				{
					await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false)))
						.DoesNotComplyWith(it => it.AtLeastOnce().Within(30.Seconds()));
				}

				Stopwatch stopwatch = Stopwatch.StartNew();
				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that the aweXpect.Mockolate.Tests.ThatVerificationResultIs.IMyService mock
					             invoked method MyMethod(1, false) less than once within 0:30,
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
					.Because("the inner verification is evaluated non-negated and waits for the interaction");
				await That(stopwatch.Elapsed).IsLessThan(10.Seconds())
					.Because("the negation fails as soon as the interaction happened");

				await backgroundTask;
			}

			[Fact]
			public async Task WhenNotInvoked_Within_ShouldSucceedAfterWaiting()
			{
				IMyService sut = IMyService.CreateMock();

				async Task Act()
				{
					await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false)))
						.DoesNotComplyWith(it => it.AtLeastOnce().Within(100.Milliseconds()));
				}

				Stopwatch stopwatch = Stopwatch.StartNew();
				await That(Act).DoesNotThrow();
				await That(stopwatch.Elapsed).IsGreaterThanOrEqualTo(90.Milliseconds())
					.Because("the inner verification waits the whole time for an interaction that does not happen");
			}

			[Fact]
			public async Task WhenInvokedNever_ShouldSucceed()
			{
				IMyService sut = IMyService.CreateMock();

				async Task Act()
				{
					await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false)))
						.DoesNotComplyWith(it => it.AtLeastOnce());
				}

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenInvokedOnce_ShouldFail()
			{
				IMyService sut = IMyService.CreateMock();

				sut.MyMethod(1, false);

				async Task Act()
				{
					await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false)))
						.DoesNotComplyWith(it => it.AtLeastOnce());
				}

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that the aweXpect.Mockolate.Tests.ThatVerificationResultIs.IMyService mock
					             invoked method MyMethod(1, false) less than once,
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

			[Fact]
			public async Task WhenInvokedTwice_ShouldFail()
			{
				IMyService sut = IMyService.CreateMock();

				sut.MyMethod(1, false);
				sut.MyMethod(1, false);

				async Task Act()
				{
					await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false)))
						.DoesNotComplyWith(it => it.AtLeastOnce());
				}

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that the aweXpect.Mockolate.Tests.ThatVerificationResultIs.IMyService mock
					             invoked method MyMethod(1, false) less than once,
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
		}
	}
}
