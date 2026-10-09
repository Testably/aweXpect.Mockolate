using System.Threading;
using aweXpect.Chronology;
using Mockolate;
using Xunit.Sdk;

namespace aweXpect.Mockolate.Tests;

public sealed partial class ThatVerificationResultIs
{
	public sealed class OnceTests
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
				await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).Once();
			}

			await That(Act).Throws<XunitException>()
				.WithMessage("""
				             Expected that the aweXpect.Mockolate.Tests.ThatVerificationResultIs.IMyService mock
				             invoked method MyMethod(1, false) exactly once,
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

			await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).Once().WithCancellation(token);

			await backgroundTask;
		}

		[Theory]
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

			await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).Once().Within(30.Seconds());

			await backgroundTask;
		}

		[Theory]
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

			await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).Once().WithTimeout(30.Seconds());

			await backgroundTask;
		}

		[Fact]
		public async Task WhenInvokedMoreThanTwice_ShouldFail()
		{
			IMyService sut = IMyService.CreateMock();

			sut.MyMethod(1, false);
			sut.MyMethod(1, false);
			sut.MyMethod(1, false);

			async Task Act()
			{
				await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).Once();
			}

			await That(Act).Throws<XunitException>()
				.WithMessage("""
				             Expected that the aweXpect.Mockolate.Tests.ThatVerificationResultIs.IMyService mock
				             invoked method MyMethod(1, false) exactly once,
				             but found it 3 times

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
		public async Task WhenInvokedNever_ShouldFail()
		{
			IMyService sut = IMyService.CreateMock();


			async Task Act()
			{
				await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).Once();
			}

			await That(Act).Throws<XunitException>()
				.WithMessage("""
				             Expected that the aweXpect.Mockolate.Tests.ThatVerificationResultIs.IMyService mock
				             invoked method MyMethod(1, false) exactly once,
				             but never found it

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
				await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).Once();
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
				await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).Once();
			}

			await That(Act).Throws<XunitException>()
				.WithMessage("""
				             Expected that the aweXpect.Mockolate.Tests.ThatVerificationResultIs.IMyService mock
				             invoked method MyMethod(1, false) exactly once,
				             but found it twice

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

		[Fact]
		public async Task WhenNotInvoked_Within_ShouldFailWithDescriptiveMessage()
		{
			IMyService sut = IMyService.CreateMock();

			async Task Act()
			{
				await That(sut.Mock.Verify.MyMethod(It.IsAny<int>(), It.Is(true))).Once().Within(50.Milliseconds());
			}

			await That(Act).Throws<XunitException>()
				.WithMessage("""
				             Expected that the aweXpect.Mockolate.Tests.ThatVerificationResultIs.IMyService mock
				             invoked method MyMethod(It.IsAny<int>(), true) exactly once,
				             but never found it

				             Matching Interactions:
				             []

				             All Interactions:
				             []
				             """);
		}

		[Fact]
		public async Task WhenOtherMethodsAreInvoked_Within_ShouldShowMatchingAndAllInteractions()
		{
			IMyService sut = IMyService.CreateMock();

			sut.MyMethod(2, false);
			sut.MyMethod(3, true);

			async Task Act()
			{
				await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).Once().Within(50.Milliseconds());
			}

			await That(Act).Throws<XunitException>()
				.WithMessage("""
				             Expected that the aweXpect.Mockolate.Tests.ThatVerificationResultIs.IMyService mock
				             invoked method MyMethod(1, false) exactly once,
				             but never found it

				             Matching Interactions:
				             []

				             All Interactions:
				             [
				               invoke method MyMethod(2, False),
				               invoke method MyMethod(3, True)
				             ]
				             """);
		}

		[Fact]
		public async Task WhenNotInvoked_WithTimeout_ShouldFailWithDescriptiveMessage()
		{
			IMyService sut = IMyService.CreateMock();

			async Task Act()
			{
				await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).Once().WithTimeout(50.Milliseconds());
			}

			await That(Act).Throws<XunitException>()
				.WithMessage("""
				             Expected that the aweXpect.Mockolate.Tests.ThatVerificationResultIs.IMyService mock
				             invoked method MyMethod(1, false) exactly once,
				             but never found it

				             Matching Interactions:
				             []

				             All Interactions:
				             []
				             """)
				.Because("the timeout of the expectation is the time to wait for the interaction");
		}

		[Fact]
		public async Task WhenEvaluatedForSeveralItems_ShouldOnlyShowInteractionsOfFailingItem()
		{
			IMyService sut1 = IMyService.CreateMock();
			IMyService sut2 = IMyService.CreateMock();
			sut1.MyMethod(1);
			sut1.MyMethod(1);
			sut1.MyMethod(99);
			sut2.MyMethod(1);

			async Task Act()
			{
				await That(new[] { sut1.Mock.Verify.MyMethod(It.Is(1)), sut2.Mock.Verify.MyMethod(It.Is(1)), })
					.All().ComplyWith(x => x.DoesNotComplyWith(y => y.Once()));
			}

			await That(Act).Throws<XunitException>()
				.WithMessage("""
				             *
				             Matching Interactions (item [1]):
				             [
				               invoke method MyMethod(1)
				             ]
				             """).AsWildcard()
				.Because("the interactions of the first mock must not be shown for the second item");
		}
	}
}
