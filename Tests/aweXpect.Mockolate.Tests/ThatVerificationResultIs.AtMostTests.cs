using System.Diagnostics;
using System.Threading;
using aweXpect.Chronology;
using Mockolate;
using Mockolate.Verify;
using Xunit.Sdk;

namespace aweXpect.Mockolate.Tests;

public sealed partial class ThatVerificationResultIs
{
	public sealed class AtMost
	{
		[Fact]
		public async Task WhenCanceledWhileWaitingForAwaitableSubject_ShouldBeInconclusive()
		{
			IMyService sut = IMyService.CreateMock();
			using CancellationTokenSource cts = new();
			cts.CancelAfter(50.Milliseconds());
			Stopwatch stopwatch = Stopwatch.StartNew();

			sut.MyMethod(1, false);
			sut.MyMethod(1, false);

			async Task Act()
			{
				await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false)).Within(30.Seconds())).AtMost(1)
					.WithCancellation(cts.Token);
			}

			await That(Act).Throws<InconclusiveException>()
				.WithMessage("""
				             Expected that the aweXpect.Mockolate.Tests.ThatVerificationResultIs.IMyService mock
				             invoked method MyMethod(1, false) at most once,
				             but it could not be verified, because the evaluation was already canceled

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
			await That(stopwatch.Elapsed).IsLessThan(10.Seconds())
				.Because("the cancellation ends the wait of the subject before its own timeout elapses");
		}

		[Theory]
		[InlineData(1, "once")]
		[InlineData(2, "twice")]
		public async Task WhenExpectedNeverButInvoked_ShouldFail(int invocationTimes, string amountString)
		{
			IMyService sut = IMyService.CreateMock();

			for (int i = 0; i < invocationTimes; i++)
			{
				sut.MyMethod(1, false);
			}

			async Task Act()
			{
				await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).AtMost(0);
			}

			await That(Act).Throws<XunitException>()
				.WithMessage($"""
				              Expected that the aweXpect.Mockolate.Tests.ThatVerificationResultIs.IMyService mock
				              invoked method MyMethod(1, false) at most never,
				              but found it {amountString}

				              Matching Interactions:
				              [
				              *
				              ]
				              """).AsWildcard();
		}

		[Theory]
		[InlineData(3)]
		[InlineData(6)]
		[InlineData(18)]
		public async Task WhenInvokedAtMostTheSameTimes_ShouldSucceed(int times)
		{
			IMyService sut = IMyService.CreateMock();

			for (int i = 0; i < times; i++)
			{
				sut.MyMethod(1, false);
			}

			async Task Act()
			{
				await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).AtMost(times);
			}

			await That(Act).DoesNotThrow();
		}

		[Theory]
		[InlineData(2, 0)]
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
				await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).AtMost(times);
			}

			await That(Act).DoesNotThrow();
		}

		[Fact]
		public async Task WhenInvokedMoreOften_AwaitableSubject_ShouldFailAfterTimeoutOfSubject()
		{
			IMyService sut = IMyService.CreateMock();

			sut.MyMethod(1, false);
			sut.MyMethod(2, false);
			sut.MyMethod(1, false);

			async Task Act()
			{
				await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false)).Within(50.Milliseconds())).AtMost(1);
			}

			await That(Act).Throws<XunitException>()
				.WithMessage("""
				             Expected that the aweXpect.Mockolate.Tests.ThatVerificationResultIs.IMyService mock
				             invoked method MyMethod(1, false) at most once,
				             but found it twice

				             Matching Interactions:
				             [
				               invoke method MyMethod(1, False),
				               invoke method MyMethod(1, False)
				             ]

				             All Interactions:
				             [
				               invoke method MyMethod(1, False),
				               invoke method MyMethod(2, False),
				               invoke method MyMethod(1, False)
				             ]
				             """);
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
				await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).AtMost(times);
			}

			await That(Act).Throws<XunitException>()
				.WithMessage($"""
				              Expected that the aweXpect.Mockolate.Tests.ThatVerificationResultIs.IMyService mock
				              invoked method MyMethod(1, false) at most {times} times,
				              but found it {invocationTimes} times

				              Matching Interactions:
				              [
				              *
				              ]
				              """).AsWildcard();
		}

		[Fact]
		public async Task WhenSubjectIsNull_ShouldFail()
		{
			VerificationResult<IMyService>? subject = null;

			async Task Act()
			{
				await That(subject!).AtMost(2);
			}

			await That(Act).Throws<XunitException>()
				.WithMessage("""
				             Expected that subject
				             had the interaction at most twice,
				             but it was <null>
				             """);
		}

		public sealed class NegatedTests
		{
			[Fact]
			public async Task WhenSubjectIsNull_ShouldFail()
			{
				VerificationResult<IMyService>? subject = null;

				async Task Act()
				{
					await That(subject!).DoesNotComplyWith(it => it.AtMost(2));
				}

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             had the interaction more than twice,
					             but it was <null>
					             """)
					.Because("a null subject cannot be verified, so the negation fails as well");
			}

			[Theory]
			[InlineData(4, 3)]
			[InlineData(6, 4)]
			public async Task WhenInvokedAtMostExpected_ShouldFail(int times, int invocationTimes)
			{
				IMyService sut = IMyService.CreateMock();

				for (int i = 0; i < invocationTimes; i++)
				{
					sut.MyMethod(1, false);
				}

				async Task Act()
				{
					await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false)))
						.DoesNotComplyWith(it => it.AtMost(times));
				}

				await That(Act).Throws<XunitException>()
					.WithMessage($"""
					              Expected that the aweXpect.Mockolate.Tests.ThatVerificationResultIs.IMyService mock
					              invoked method MyMethod(1, false) more than {times} times,
					              but found it only {invocationTimes} times

					              Matching Interactions:
					              [
					              *
					              ]
					              """).AsWildcard();
			}

			[Fact]
			public async Task WhenInvokedMoreThanExpected_AwaitableSubject_ShouldSucceed()
			{
				IMyService sut = IMyService.CreateMock();

				sut.MyMethod(1, false);
				sut.MyMethod(1, false);

				async Task Act()
				{
					await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false)).Within(50.Milliseconds()))
						.DoesNotComplyWith(it => it.AtMost(1));
				}

				await That(Act).DoesNotThrow()
					.Because("the timeout of the subject fails the verification, which satisfies the negation");
			}

			[Theory]
			[InlineData(3, 4)]
			[InlineData(6, 8)]
			public async Task WhenInvokedMoreThanExpected_ShouldSucceed(int times, int invocationTimes)
			{
				IMyService sut = IMyService.CreateMock();

				for (int i = 0; i < invocationTimes; i++)
				{
					sut.MyMethod(1, false);
				}

				async Task Act()
				{
					await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false)))
						.DoesNotComplyWith(it => it.AtMost(times));
				}

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenNeverInvoked_ShouldFail()
			{
				IMyService sut = IMyService.CreateMock();

				async Task Act()
				{
					await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false)))
						.DoesNotComplyWith(it => it.AtMost(3));
				}

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that the aweXpect.Mockolate.Tests.ThatVerificationResultIs.IMyService mock
					             invoked method MyMethod(1, false) more than 3 times,
					             but never found it

					             Matching Interactions:
					             []
					             """);
			}
		}
	}
}
