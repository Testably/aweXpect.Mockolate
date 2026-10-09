using System.Threading;
using aweXpect.Chronology;
using aweXpect.Core;
using Mockolate;
using Xunit.Sdk;

namespace aweXpect.Mockolate.Tests;

public sealed partial class ThatVerificationResultIs
{
	public sealed class ExactlyTests
	{
		[Theory]
		[InlineData(3)]
		[InlineData(6)]
		[InlineData(18)]
		public async Task WhenInvokedExactlyTheSameTimes_ShouldSucceed(int times)
		{
			IMyService sut = IMyService.CreateMock();

			for (int i = 0; i < times; i++)
			{
				sut.MyMethod(1, false);
			}

			async Task Act()
			{
				await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).Exactly(times);
			}

			await That(Act).DoesNotThrow();
		}

		[Theory]
		[InlineData(4, 3)]
		[InlineData(8, 6)]
		public async Task WhenInvokedFewerTimes_ShouldFail(int times, int invocationTimes)
		{
			IMyService sut = IMyService.CreateMock();

			for (int i = 0; i < invocationTimes; i++)
			{
				sut.MyMethod(1, false);
			}

			async Task Act()
			{
				await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).Exactly(times);
			}

			await That(Act).Throws<XunitException>()
				.WithMessage($"""
				              Expected that the aweXpect.Mockolate.Tests.ThatVerificationResultIs.IMyService mock
				              invoked method MyMethod(1, false) exactly {times} times,
				              but found it only {invocationTimes} times

				              Matching Interactions:
				              [
				              *
				              ]
				              """).AsWildcard();
		}

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
				await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).Exactly(3);
			}

			await That(Act).Throws<XunitException>()
				.WithMessage("""
				             Expected that the aweXpect.Mockolate.Tests.ThatVerificationResultIs.IMyService mock
				             invoked method MyMethod(1, false) exactly 3 times,
				             but never found it

				             Matching Interactions:
				             []

				             All Interactions:
				             []
				             """);
			cts.Cancel();
			await backgroundTask;
		}

		[Theory]
		[InlineData(3, 3)]
		[InlineData(5, 5)]
		public async Task WhenInvokedInBackground_WithCancellation_ShouldSucceed(int times, int invocationTimes)
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

			await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).Exactly(times).WithCancellation(token);

			await backgroundTask;
		}

		[Theory]
		[InlineData(3, 3)]
		[InlineData(5, 5)]
		public async Task WhenInvokedInBackground_Within_ShouldSucceed(int times, int invocationTimes)
		{
			IMyService sut = IMyService.CreateMock();

			Task backgroundTask = Task.Delay(50).ContinueWith(_ =>
			{
				for (int i = 0; i < invocationTimes; i++)
				{
					sut.MyMethod(1, false);
				}
			});

			await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).Exactly(times).Within(30.Seconds());

			await backgroundTask;
		}

		[Theory]
		[InlineData(3, 3)]
		[InlineData(5, 5)]
		public async Task WhenInvokedInBackground_WithTimeout_ShouldSucceed(int times, int invocationTimes)
		{
			IMyService sut = IMyService.CreateMock();

			Task backgroundTask = Task.Delay(50).ContinueWith(_ =>
			{
				for (int i = 0; i < invocationTimes; i++)
				{
					sut.MyMethod(1, false);
				}
			});

			await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).Exactly(times)
				.WithTimeout(30.Seconds());

			await backgroundTask;
		}

		[Theory]
		[InlineData(3, 4)]
		[InlineData(6, 8)]
		public async Task WhenInvokedMoreOften_ShouldFail(int times, int invocationTimes)
		{
			IMyService sut = IMyService.CreateMock();

			for (int i = 0; i < invocationTimes; i++)
			{
				sut.MyMethod(1, false);
			}

			async Task Act()
			{
				await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).Exactly(times);
			}

			await That(Act).Throws<XunitException>()
				.WithMessage($"""
				              Expected that the aweXpect.Mockolate.Tests.ThatVerificationResultIs.IMyService mock
				              invoked method MyMethod(1, false) exactly {times} times,
				              but found it {invocationTimes} times

				              Matching Interactions:
				              [
				              *
				              ]
				              """).AsWildcard();
		}

		[Theory]
		[InlineData(0, false)]
		[InlineData(3, true)]
		[InlineData(8, true)]
		public async Task WhenInvokedNever_ShouldFailUnlessZero(int times, bool shouldThrow)
		{
			IMyService sut = IMyService.CreateMock();

			async Task Act()
			{
				await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).Exactly(times);
			}

			await That(Act).Throws<XunitException>().OnlyIf(shouldThrow)
				.WithMessage($"""
				              Expected that the aweXpect.Mockolate.Tests.ThatVerificationResultIs.IMyService mock
				              invoked method MyMethod(1, false) exactly {times} times,
				              but never found it

				              Matching Interactions:
				              []

				              All Interactions:
				              []
				              """);
		}

		[Fact]
		public async Task WhenNotInvoked_Within_ShouldFailWithDescriptiveMessage()
		{
			IMyService sut = IMyService.CreateMock();

			sut.MyMethod(1, true);
			sut.MyMethod(2, true);
			sut.MyMethod(3, true);
			sut.MyMethod(4, true);
			sut.MyMethod(5, false);

			async Task Act()
			{
				await That(sut.Mock.Verify.MyMethod(It.IsAny<int>(), It.Is(true))).Exactly(3.Times())
					.Within(50.Milliseconds());
			}

			await That(Act).Throws<XunitException>()
				.WithMessage("""
				             Expected that the aweXpect.Mockolate.Tests.ThatVerificationResultIs.IMyService mock
				             invoked method MyMethod(It.IsAny<int>(), true) exactly 3 times within 0:00.050,
				             but found it 4 times

				             Matching Interactions:
				             [
				               invoke method MyMethod(1, True),
				               invoke method MyMethod(2, True),
				               invoke method MyMethod(3, True),
				               invoke method MyMethod(4, True)
				             ]

				             All Interactions:
				             [
				               invoke method MyMethod(1, True),
				               invoke method MyMethod(2, True),
				               invoke method MyMethod(3, True),
				               invoke method MyMethod(4, True),
				               invoke method MyMethod(5, False)
				             ]
				             """);
		}

		public sealed class NegatedTests
		{
			[Theory]
			[InlineData(3)]
			[InlineData(6)]
			public async Task WhenInvokedExactlyTheSameTimes_ShouldFail(int times)
			{
				IMyService sut = IMyService.CreateMock();

				for (int i = 0; i < times; i++)
				{
					sut.MyMethod(1, false);
				}

				async Task Act()
				{
					await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false)))
						.DoesNotComplyWith(it => it.Exactly(times));
				}

				await That(Act).Throws<XunitException>()
					.WithMessage($"""
					              Expected that the aweXpect.Mockolate.Tests.ThatVerificationResultIs.IMyService mock
					              invoked method MyMethod(1, false) not exactly {times} times,
					              but it was

					              Matching Interactions:
					              [
					              *
					              ]
					              """).AsWildcard();
			}

			[Theory]
			[InlineData(4, 3)]
			[InlineData(8, 6)]
			public async Task WhenInvokedFewerTimes_ShouldSucceed(int times, int invocationTimes)
			{
				IMyService sut = IMyService.CreateMock();

				for (int i = 0; i < invocationTimes; i++)
				{
					sut.MyMethod(1, false);
				}

				async Task Act()
				{
					await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false)))
						.DoesNotComplyWith(it => it.Exactly(times));
				}

				await That(Act).DoesNotThrow();
			}

			[Theory]
			[InlineData(3, 4)]
			[InlineData(6, 8)]
			public async Task WhenInvokedMoreTimes_ShouldSucceed(int times, int invocationTimes)
			{
				IMyService sut = IMyService.CreateMock();

				for (int i = 0; i < invocationTimes; i++)
				{
					sut.MyMethod(1, false);
				}

				async Task Act()
				{
					await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false)))
						.DoesNotComplyWith(it => it.Exactly(times));
				}

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenNeverInvoked_ShouldSucceed()
			{
				IMyService sut = IMyService.CreateMock();

				async Task Act()
				{
					await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false)))
						.DoesNotComplyWith(it => it.Exactly(3));
				}

				await That(Act).DoesNotThrow();
			}
		}
	}
}
