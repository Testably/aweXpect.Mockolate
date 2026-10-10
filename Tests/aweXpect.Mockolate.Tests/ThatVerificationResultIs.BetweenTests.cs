using System.Threading;
using aweXpect.Chronology;
using aweXpect.Core;
using Mockolate;
using Xunit.Sdk;

namespace aweXpect.Mockolate.Tests;

public sealed partial class ThatVerificationResultIs
{
	public sealed class BetweenTests
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
				await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).Between(3).And(6);
			}

			await That(Act).Throws<XunitException>()
				.WithMessage("""
				             Expected that the aweXpect.Mockolate.Tests.ThatVerificationResultIs.IMyService mock
				             invoked method MyMethod(1, false) between 3 and 6 times,
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
		[InlineData(3, 8, 7)]
		[InlineData(5, 8, 5)]
		[InlineData(5, 8, 8)]
		public async Task WhenInvokedInBackground_WithCancellation_ShouldSucceed(int minimum, int maximum,
			int invocationTimes)
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

			await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).Between(minimum).And(maximum)
				.WithCancellation(token);

			await backgroundTask;
		}

		[Theory]
		[InlineData(3, 8, 7)]
		[InlineData(5, 8, 5)]
		[InlineData(5, 8, 8)]
		public async Task WhenInvokedInBackground_Within_ShouldSucceed(int minimum, int maximum, int invocationTimes)
		{
			IMyService sut = IMyService.CreateMock();

			Task backgroundTask = Task.Delay(50).ContinueWith(_ =>
			{
				for (int i = 0; i < invocationTimes; i++)
				{
					sut.MyMethod(1, false);
				}
			});

			await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).Between(minimum).And(maximum)
				.Within(30.Seconds());

			await backgroundTask;
		}

		[Theory]
		[InlineData(3, 8, 7)]
		[InlineData(5, 8, 5)]
		[InlineData(5, 8, 8)]
		public async Task WhenInvokedInBackground_WithTimeout_ShouldSucceed(int minimum, int maximum,
			int invocationTimes)
		{
			IMyService sut = IMyService.CreateMock();

			Task backgroundTask = Task.Delay(50).ContinueWith(_ =>
			{
				for (int i = 0; i < invocationTimes; i++)
				{
					sut.MyMethod(1, false);
				}
			});

			await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).Between(minimum).And(maximum)
				.WithTimeout(30.Seconds());

			await backgroundTask;
		}

		[Theory]
		[InlineData(3, 5, 3)]
		[InlineData(3, 5, 5)]
		[InlineData(3, 5, 4)]
		[InlineData(3, 8, 6)]
		public async Task WhenInvokedInRange_ShouldSucceed(int minimum, int maximum, int invocationTimes)
		{
			IMyService sut = IMyService.CreateMock();

			for (int i = 0; i < invocationTimes; i++)
			{
				sut.MyMethod(1, false);
			}

			async Task Act()
			{
				await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).Between(minimum).And(maximum);
			}

			await That(Act).DoesNotThrow();
		}

		[Theory]
		[InlineData(0, 1, false)]
		[InlineData(3, 5, true)]
		[InlineData(1, 2, true)]
		public async Task WhenInvokedNever_ShouldFailUnlessZero(int minimum, int maximum, bool shouldThrow)
		{
			IMyService sut = IMyService.CreateMock();

			async Task Act()
			{
				await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).Between(minimum).And(maximum);
			}

			await That(Act).Throws<XunitException>().OnlyIf(shouldThrow)
				.WithMessage($"""
				              Expected that the aweXpect.Mockolate.Tests.ThatVerificationResultIs.IMyService mock
				              invoked method MyMethod(1, false) between {minimum} and {maximum} times,
				              but it was never found

				              Matching Interactions:
				              []

				              All Interactions:
				              []
				              """);
		}

		[Theory]
		[InlineData(4, 5, 3)]
		[InlineData(8, 12, 6)]
		public async Task WhenInvokedTooFewTimes_ShouldFail(int minimum, int maximum, int invocationTimes)
		{
			IMyService sut = IMyService.CreateMock();

			for (int i = 0; i < invocationTimes; i++)
			{
				sut.MyMethod(1, false);
			}

			async Task Act()
			{
				await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).Between(minimum).And(maximum);
			}

			await That(Act).Throws<XunitException>()
				.WithMessage($"""
				              Expected that the aweXpect.Mockolate.Tests.ThatVerificationResultIs.IMyService mock
				              invoked method MyMethod(1, false) between {minimum} and {maximum} times,
				              but it was found only {invocationTimes} times

				              Matching Interactions:
				              [
				              *
				              ]
				              """).AsWildcard();
		}

		[Theory]
		[InlineData(4, 5, 6)]
		[InlineData(8, 12, 14)]
		public async Task WhenInvokedTooOften_ShouldFail(int minimum, int maximum, int invocationTimes)
		{
			IMyService sut = IMyService.CreateMock();

			for (int i = 0; i < invocationTimes; i++)
			{
				sut.MyMethod(1, false);
			}

			async Task Act()
			{
				await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).Between(minimum).And(maximum);
			}

			await That(Act).Throws<XunitException>()
				.WithMessage($"""
				              Expected that the aweXpect.Mockolate.Tests.ThatVerificationResultIs.IMyService mock
				              invoked method MyMethod(1, false) between {minimum} and {maximum} times,
				              but it was found {invocationTimes} times

				              Matching Interactions:
				              [
				              *
				              ]
				              """).AsWildcard();
		}

		[Fact]
		public async Task WhenNotInvoked_Within_ShouldFailWithDescriptiveMessage()
		{
			IMyService sut = IMyService.CreateMock();

			sut.MyMethod(1, true);
			sut.MyMethod(2, true);
			sut.MyMethod(3, true);
			sut.MyMethod(4, false);

			async Task Act()
			{
				await That(sut.Mock.Verify.MyMethod(It.IsAny<int>(), It.Is(true))).Between(4).And(6.Times())
					.Within(50.Milliseconds());
			}

			await That(Act).Throws<XunitException>()
				.WithMessage("""
				             Expected that the aweXpect.Mockolate.Tests.ThatVerificationResultIs.IMyService mock
				             invoked method MyMethod(It.IsAny<int>(), true) between 4 and 6 times within 0:00.050,
				             but it was found only 3 times

				             Matching Interactions:
				             [
				               invoke method MyMethod(1, True),
				               invoke method MyMethod(2, True),
				               invoke method MyMethod(3, True)
				             ]

				             All Interactions:
				             [
				               invoke method MyMethod(1, True),
				               invoke method MyMethod(2, True),
				               invoke method MyMethod(3, True),
				               invoke method MyMethod(4, False)
				             ]
				             """);
		}

		[Fact]
		public async Task WhenMaximumIsLessThanMinimum_ShouldThrowArgumentOutOfRangeException()
		{
			IMyService sut = IMyService.CreateMock();

			async Task Act()
			{
				await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).Between(5).And(2);
			}

			await That(Act).Throws<ArgumentOutOfRangeException>()
				.WithParamName("maximum").And
				.WithMessage("The maximum must be greater than or equal to the minimum.").AsPrefix()
				.Because("an inverted range is rejected with the same message as in core");
		}

		[Fact]
		public async Task WhenMaximumIsNegative_ShouldThrowArgumentOutOfRangeException()
		{
			IMyService sut = IMyService.CreateMock();

			async Task Act()
			{
				await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).Between(0).And(-1);
			}

			await That(Act).Throws<ArgumentOutOfRangeException>()
				.WithParamName("maximum").And
				.WithMessage("The maximum must not be negative.").AsPrefix()
				.Because("a negative count is rejected with the same message as in core");
		}

		[Fact]
		public async Task WhenMinimumIsNegative_ShouldThrowArgumentOutOfRangeException()
		{
			IMyService sut = IMyService.CreateMock();

			async Task Act()
			{
				await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).Between(-1).And(2);
			}

			await That(Act).Throws<ArgumentOutOfRangeException>()
				.WithParamName("minimum").And
				.WithMessage("The minimum must not be negative.").AsPrefix()
				.Because("a negative count is rejected with the same message as in core");
		}

		public sealed class NegatedTests
		{
			[Theory]
			[InlineData(3, 5, 3)]
			[InlineData(3, 5, 4)]
			[InlineData(3, 5, 5)]
			public async Task WhenInvokedInRange_ShouldFail(int minimum, int maximum, int invocationTimes)
			{
				IMyService sut = IMyService.CreateMock();

				for (int i = 0; i < invocationTimes; i++)
				{
					sut.MyMethod(1, false);
				}

				async Task Act()
				{
					await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false)))
						.DoesNotComplyWith(it => it.Between(minimum).And(maximum));
				}

				await That(Act).Throws<XunitException>()
					.WithMessage($"""
					              Expected that the aweXpect.Mockolate.Tests.ThatVerificationResultIs.IMyService mock
					              invoked method MyMethod(1, false) not between {minimum} and {maximum} times,
					              but it was found {invocationTimes} times

					              Matching Interactions:
					              [
					              *
					              ]
					              """).AsWildcard();
			}

			[Theory]
			[InlineData(3, 5, 2)]
			[InlineData(8, 12, 6)]
			public async Task WhenInvokedTooFewTimes_ShouldSucceed(int minimum, int maximum, int invocationTimes)
			{
				IMyService sut = IMyService.CreateMock();

				for (int i = 0; i < invocationTimes; i++)
				{
					sut.MyMethod(1, false);
				}

				async Task Act()
				{
					await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false)))
						.DoesNotComplyWith(it => it.Between(minimum).And(maximum));
				}

				await That(Act).DoesNotThrow();
			}

			[Theory]
			[InlineData(3, 5, 6)]
			[InlineData(8, 12, 14)]
			public async Task WhenInvokedTooOften_ShouldSucceed(int minimum, int maximum, int invocationTimes)
			{
				IMyService sut = IMyService.CreateMock();

				for (int i = 0; i < invocationTimes; i++)
				{
					sut.MyMethod(1, false);
				}

				async Task Act()
				{
					await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false)))
						.DoesNotComplyWith(it => it.Between(minimum).And(maximum));
				}

				await That(Act).DoesNotThrow();
			}
		}
	}
}
