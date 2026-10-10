using System.Diagnostics;
using System.Threading;
using aweXpect.Chronology;
using Mockolate;
using Mockolate.Verify;
using Xunit.Sdk;

namespace aweXpect.Mockolate.Tests;

public sealed partial class ThatVerificationResultIs
{
	public sealed class ThenTests
	{
		[Fact]
		public async Task Then_ShouldVerifyInOrder()
		{
			IMyService sut = IMyService.CreateMock();

			sut.MyMethod(1);
			sut.MyMethod(2);
			sut.MyMethod(3);
			sut.MyMethod(4);

			await That(sut.Mock.Verify.MyMethod(It.Is(3))).Then(m => m.MyMethod(It.Is(4)));
			await That(async Task ()
					=> await That(sut.Mock.Verify.MyMethod(It.Is(2))).Then(m => m.MyMethod(It.Is(1))))
				.Throws<XunitException>()
				.WithMessage("""
				             Expected that the aweXpect.Mockolate.Tests.ThatVerificationResultIs.IMyService mock
				             invoked method MyMethod(2), then
				             invoked method MyMethod(1) in order,
				             but it invoked method MyMethod(1) too early

				             All Interactions:
				             [
				               invoke method MyMethod(1),
				               invoke method MyMethod(2),
				               invoke method MyMethod(3),
				               invoke method MyMethod(4)
				             ]
				             """);
			await That(sut.Mock.Verify.MyMethod(It.Is(1)))
				.Then(m => m.MyMethod(It.Is(2)), m => m.MyMethod(It.Is(3)));
		}

		[Fact]
		public async Task Then_WhenCanceledWhileWaitingForAwaitableSubject_ShouldBeInconclusive()
		{
			IMyService sut = IMyService.CreateMock();
			using CancellationTokenSource cts = new();
			cts.CancelAfter(50.Milliseconds());
			Stopwatch stopwatch = Stopwatch.StartNew();

			sut.MyMethod(2);

			async Task Act()
			{
				await That(sut.Mock.Verify.MyMethod(It.Is(1)).Within(30.Seconds())).Then(m => m.MyMethod(It.Is(2)))
					.WithCancellation(cts.Token);
			}

			await That(Act).Throws<InconclusiveException>()
				.WithMessage("""
				             Expected that the aweXpect.Mockolate.Tests.ThatVerificationResultIs.IMyService mock
				             invoked method MyMethod(1), then
				             invoked method MyMethod(2) in order,
				             but it could not be verified, because the evaluation was already canceled

				             All Interactions:
				             [
				               invoke method MyMethod(2)
				             ]
				             """);
			await That(stopwatch.Elapsed).IsLessThan(10.Seconds())
				.Because("the cancellation ends the wait of the subject before its own timeout elapses");
		}

		[Fact]
		public async Task Then_WhenFirstFilterMatchesMultipleInteractions_ShouldUseEarliestIndex()
		{
			IMyService sut = IMyService.CreateMock();

			sut.MyMethod(1);
			sut.MyMethod(2);
			sut.MyMethod(1);

			await That(sut.Mock.Verify.MyMethod(It.Is(1))).Then(m => m.MyMethod(It.Is(2)));
		}

		[Fact]
		public async Task Then_WhenInitialAndThenFiltersMatchSameSingleInteraction_ShouldFail()
		{
			IMyService sut = IMyService.CreateMock();

			sut.MyMethod(1);

			await That(async Task () => await That(sut.Mock.Verify.MyMethod(It.Is(1)))
					.Then(m => m.MyMethod(It.Is(1))))
				.Throws<XunitException>();
		}

		[Fact]
		public async Task Then_WhenInteractionSelectorThrows_ShouldFail()
		{
			InvalidOperationException exception = new("selector failed");
			IMyService sut = IMyService.CreateMock();

			sut.MyMethod(1);
			sut.MyMethod(2);

			async Task Act()
			{
				await That(sut.Mock.Verify.MyMethod(It.Is(1)))
					.Then(m => m.MyMethod(It.Is(2)), _ => throw exception);
			}

			await That(Act).Throws<XunitException>()
				.WithMessage("""
				             Expected that the aweXpect.Mockolate.Tests.ThatVerificationResultIs.IMyService mock
				             invoked method MyMethod(1), then
				             invoked method MyMethod(2) in order,
				             but the interaction selector did throw an InvalidOperationException:
				               selector failed
				             """).And
				.Whose(e => e.InnerException, i => i.IsSameAs(exception))
				.Because("the selector answered nothing, so the expectation fails with its exception");
		}

		[Fact]
		public async Task Then_WhenInvokedWhileWaitingForAwaitableSubject_ShouldSucceed()
		{
			IMyService sut = IMyService.CreateMock();
			Stopwatch stopwatch = Stopwatch.StartNew();

			Task invocation = Task.Run(async () =>
			{
				await Task.Delay(50.Milliseconds());
				sut.MyMethod(1);
			});

			await That(sut.Mock.Verify.MyMethod(It.Is(1)).Within(30.Seconds())).Then();

			await invocation;
			await That(stopwatch.Elapsed).IsLessThan(10.Seconds())
				.Because("the verification is repeated when the subject is invoked while waiting");
		}

		[Fact]
		public async Task Then_WhenNoMatch_ShouldReturnFalse()
		{
			IMyService sut = IMyService.CreateMock();

			sut.MyMethod(1);
			sut.MyMethod(2);
			sut.MyMethod(3);
			sut.MyMethod(4);

			await That(async Task ()
					=> await That(sut.Mock.Verify.MyMethod(It.Is(6))).Then(m => m.MyMethod(It.Is(4))))
				.Throws<XunitException>()
				.WithMessage("""
				             Expected that the aweXpect.Mockolate.Tests.ThatVerificationResultIs.IMyService mock
				             invoked method MyMethod(6), then
				             invoked method MyMethod(4) in order,
				             but it invoked method MyMethod(6) not at all

				             All Interactions:
				             [
				               invoke method MyMethod(1),
				               invoke method MyMethod(2),
				               invoke method MyMethod(3),
				               invoke method MyMethod(4)
				             ]
				             """);

			await That(async Task () => await That(sut.Mock.Verify.MyMethod(It.Is(1)))
					.Then(m => m.MyMethod(It.Is(6)), m => m.MyMethod(It.Is(3))))
				.Throws<XunitException>()
				.WithMessage("""
				             Expected that the aweXpect.Mockolate.Tests.ThatVerificationResultIs.IMyService mock
				             invoked method MyMethod(1), then
				             invoked method MyMethod(6), then
				             invoked method MyMethod(3) in order,
				             but it invoked method MyMethod(6) not at all

				             All Interactions:
				             [
				               invoke method MyMethod(1),
				               invoke method MyMethod(2),
				               invoke method MyMethod(3),
				               invoke method MyMethod(4)
				             ]
				             """);

			await That(async Task () => await That(sut.Mock.Verify.MyMethod(It.Is(1)))
					.Then(m => m.MyMethod(It.Is(2)), m => m.MyMethod(It.Is(6))))
				.Throws<XunitException>()
				.WithMessage("""
				             Expected that the aweXpect.Mockolate.Tests.ThatVerificationResultIs.IMyService mock
				             invoked method MyMethod(1), then
				             invoked method MyMethod(2), then
				             invoked method MyMethod(6) in order,
				             but it invoked method MyMethod(6) not at all

				             All Interactions:
				             [
				               invoke method MyMethod(1),
				               invoke method MyMethod(2),
				               invoke method MyMethod(3),
				               invoke method MyMethod(4)
				             ]
				             """);
		}

		[Fact]
		public async Task Then_WhenNotInvoked_AwaitableSubject_ShouldFailAfterTimeoutOfSubject()
		{
			IMyService sut = IMyService.CreateMock();

			sut.MyMethod(2);

			async Task Act()
			{
				await That(sut.Mock.Verify.MyMethod(It.Is(1)).Within(50.Milliseconds()))
					.Then(m => m.MyMethod(It.Is(2)));
			}

			await That(Act).Throws<XunitException>()
				.WithMessage("""
				             Expected that the aweXpect.Mockolate.Tests.ThatVerificationResultIs.IMyService mock
				             invoked method MyMethod(1), then
				             invoked method MyMethod(2) in order,
				             but it invoked method MyMethod(1) not at all

				             All Interactions:
				             [
				               invoke method MyMethod(2)
				             ]
				             """);
		}

		[Fact]
		public async Task Then_WhenSecondFilterMatchesAtAndAfterEarliest_ShouldAdvanceStrictly()
		{
			IMyService sut = IMyService.CreateMock();

			sut.MyMethod(1);
			sut.MyMethod(3);
			sut.MyMethod(2);

			await That(async Task () => await That(sut.Mock.Verify.MyMethod(It.Is(1)))
					.Then(m => m.MyMethod(It.IsAny<int>()), m => m.MyMethod(It.Is(3))))
				.Throws<XunitException>();
		}

		[Fact]
		public async Task Then_WhenSubjectIsNull_ShouldFail()
		{
			VerificationResult<IMyService>? subject = null;

			async Task Act()
			{
				await That(subject!).Then();
			}

			await That(Act).Throws<XunitException>()
				.WithMessage("""
				             Expected that subject
				             has the interactions in order,
				             but it was <null>
				             """);
		}

		[Fact]
		public async Task Then_WhenInteractionsAreNull_ShouldThrowArgumentNullException()
		{
			IMyService sut = IMyService.CreateMock();

			async Task Act()
			{
				await That(sut.Mock.Verify.MyMethod(It.Is(1))).Then(null!);
			}

			await That(Act).Throws<ArgumentNullException>()
				.WithParamName("interactions").And
				.WithMessage("The 'interactions' cannot be null.").AsPrefix()
				.Because("a missing argument fails where it is passed instead of during the evaluation");
		}

		public sealed class NegatedTests
		{
			[Fact]
			public async Task WhenCanceledWhileWaitingForAwaitableSubject_ShouldBeInconclusive()
			{
				IMyService sut = IMyService.CreateMock();
				using CancellationTokenSource cts = new();
				cts.CancelAfter(50.Milliseconds());
				Stopwatch stopwatch = Stopwatch.StartNew();

				sut.MyMethod(2);

				async Task Act()
				{
					await That(sut.Mock.Verify.MyMethod(It.Is(1)).Within(30.Seconds()))
						.DoesNotComplyWith(it => it.Then(m => m.MyMethod(It.Is(2))))
						.WithCancellation(cts.Token);
				}

				await That(Act).Throws<InconclusiveException>()
					.WithMessage("""
					             Expected that the aweXpect.Mockolate.Tests.ThatVerificationResultIs.IMyService mock
					             invoked method MyMethod(1), then
					             invoked method MyMethod(2) not in order,
					             but it could not be verified, because the evaluation was already canceled

					             All Interactions:
					             [
					               invoke method MyMethod(2)
					             ]
					             """);
				await That(stopwatch.Elapsed).IsLessThan(10.Seconds())
					.Because("the cancellation ends the wait of the subject before its own timeout elapses");
			}

			[Fact]
			public async Task WhenEvaluatedForSeveralItems_ShouldNotShowInteractionsOfPreviousItem()
			{
				IMyService sut1 = IMyService.CreateMock();
				IMyService sut2 = IMyService.CreateMock();
				sut1.MyMethod(2);
				sut1.MyMethod(1);
				sut2.MyMethod(1);
				sut2.MyMethod(2);

				async Task Act()
				{
					await That(new[] { sut1.Mock.Verify.MyMethod(It.Is(1)), sut2.Mock.Verify.MyMethod(It.Is(1)), })
						.All().ComplyWith(x => x.DoesNotComplyWith(y => y.Then(m => m.MyMethod(It.Is(2)))));
				}

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             *
					             All Interactions (item [1]):
					             [
					               invoke method MyMethod(1),
					               invoke method MyMethod(2)
					             ]
					             """).AsWildcard()
					.Because("only the second item fails, so only its interactions are shown");
			}

			[Fact]
			public async Task WhenSubjectIsNull_ShouldFail()
			{
				VerificationResult<IMyService>? subject = null;

				async Task Act()
				{
					await That(subject!).DoesNotComplyWith(it => it.Then());
				}

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             has the interactions not in order,
					             but it was <null>
					             """)
					.Because("a null subject cannot be verified, so the negation fails as well");
			}

			[Fact]
			public async Task WhenInteractionsAreInOrder_ShouldFail()
			{
				IMyService sut = IMyService.CreateMock();

				sut.MyMethod(1);
				sut.MyMethod(2);

				async Task Act()
				{
					await That(sut.Mock.Verify.MyMethod(It.Is(1)))
						.DoesNotComplyWith(it => it.Then(m => m.MyMethod(It.Is(2))));
				}

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that the aweXpect.Mockolate.Tests.ThatVerificationResultIs.IMyService mock
					             invoked method MyMethod(1), then
					             invoked method MyMethod(2) not in order,
					             but it did

					             All Interactions:
					             [
					               invoke method MyMethod(1),
					               invoke method MyMethod(2)
					             ]
					             """);
			}

			[Fact]
			public async Task WhenInteractionsAreNotInOrder_ShouldSucceed()
			{
				IMyService sut = IMyService.CreateMock();

				sut.MyMethod(2);
				sut.MyMethod(1);

				async Task Act()
				{
					await That(sut.Mock.Verify.MyMethod(It.Is(1)))
						.DoesNotComplyWith(it => it.Then(m => m.MyMethod(It.Is(2))));
				}

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenInteractionSelectorThrows_ShouldFail()
			{
				InvalidOperationException exception = new("selector failed");
				IMyService sut = IMyService.CreateMock();

				sut.MyMethod(2);
				sut.MyMethod(1);

				async Task Act()
				{
					await That(sut.Mock.Verify.MyMethod(It.Is(1)))
						.DoesNotComplyWith(it => it.Then(_ => throw exception));
				}

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that the aweXpect.Mockolate.Tests.ThatVerificationResultIs.IMyService mock
					             invoked method MyMethod(1) not in order,
					             but the interaction selector did throw an InvalidOperationException:
					               selector failed
					             """).And
					.Whose(e => e.InnerException, i => i.IsSameAs(exception))
					.Because("a selector that answered nothing must not be inverted into a success");
			}

			[Fact]
			public async Task WhenNotInvoked_AwaitableSubject_ShouldSucceed()
			{
				IMyService sut = IMyService.CreateMock();

				sut.MyMethod(2);

				async Task Act()
				{
					await That(sut.Mock.Verify.MyMethod(It.Is(1)).Within(50.Milliseconds()))
						.DoesNotComplyWith(it => it.Then(m => m.MyMethod(It.Is(2))));
				}

				await That(Act).DoesNotThrow()
					.Because("the timeout of the subject fails the verification, which satisfies the negation");
			}

			[Fact]
			public async Task WithMultipleInteractionsInOrder_ShouldFail()
			{
				IMyService sut = IMyService.CreateMock();

				sut.MyMethod(1);
				sut.MyMethod(2);
				sut.MyMethod(3);

				async Task Act()
				{
					await That(sut.Mock.Verify.MyMethod(It.Is(1)))
						.DoesNotComplyWith(it => it.Then(
							m => m.MyMethod(It.Is(2)),
							m => m.MyMethod(It.Is(3))));
				}

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that the aweXpect.Mockolate.Tests.ThatVerificationResultIs.IMyService mock
					             invoked method MyMethod(1), then
					             invoked method MyMethod(2), then
					             invoked method MyMethod(3) not in order,
					             but it did

					             All Interactions:
					             [
					               invoke method MyMethod(1),
					               invoke method MyMethod(2),
					               invoke method MyMethod(3)
					             ]
					             """);
			}
		}
	}
}
