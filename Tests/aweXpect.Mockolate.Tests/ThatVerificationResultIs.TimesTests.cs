using System.Diagnostics;
using System.Threading;
using aweXpect.Chronology;
using Mockolate;
using Xunit.Sdk;

namespace aweXpect.Mockolate.Tests;

public sealed partial class ThatVerificationResultIs
{
	public sealed class TimesTests
	{
		[Theory]
		[InlineData(0, true)]
		[InlineData(1, false)]
		[InlineData(2, true)]
		[InlineData(3, false)]
		[InlineData(4, true)]
		[InlineData(5, false)]
		public async Task Times_WithEvenPredicate_ShouldReturnExpectedResult(int count, bool expectSuccess)
		{
			IMyService sut = IMyService.CreateMock();
			for (int i = 0; i < count; i++)
			{
				sut.MyMethod(1, false);
			}

			async Task Act()
			{
				await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).Times(n => n % 2 == 0);
			}

			string expectedFoundTimes = count switch
			{
				0 => "never found it",
				1 => "found it once",
				2 => "found it twice",
				_ => $"found it {count} times",
			};

			await That(Act).Throws<XunitException>().OnlyIf(!expectSuccess)
				.WithMessage($"""
				              Expected that the aweXpect.Mockolate.Tests.ThatVerificationResultIs.IMyService mock
				              invoked method MyMethod(1, false) according to the predicate n => n % 2 == 0,
				              but {expectedFoundTimes}

				              Matching Interactions:
				              [*]
				              """).AsWildcard();
		}

		[Theory]
		[InlineData(0, false)]
		[InlineData(1, true)]
		[InlineData(2, false)]
		[InlineData(3, true)]
		[InlineData(4, false)]
		[InlineData(5, true)]
		public async Task Times_WithOddPredicate_ShouldReturnExpectedResult(int count, bool expectSuccess)
		{
			IMyService sut = IMyService.CreateMock();
			for (int i = 0; i < count; i++)
			{
				sut.MyMethod(1, false);
			}

			async Task Act()
			{
				await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).Times(n => n % 2 == 1);
			}

			string expectedFoundTimes = count switch
			{
				0 => "never found it",
				1 => "found it once",
				2 => "found it twice",
				_ => $"found it {count} times",
			};

			await That(Act).Throws<XunitException>().OnlyIf(!expectSuccess)
				.WithMessage($"""
				              Expected that the aweXpect.Mockolate.Tests.ThatVerificationResultIs.IMyService mock
				              invoked method MyMethod(1, false) according to the predicate n => n % 2 == 1,
				              but {expectedFoundTimes}

				              Matching Interactions:
				              [*]
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
				await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).Times(n => n % 3 == 2);
			}

			await That(Act).Throws<XunitException>()
				.WithMessage("""
				             Expected that the aweXpect.Mockolate.Tests.ThatVerificationResultIs.IMyService mock
				             invoked method MyMethod(1, false) according to the predicate n => n % 3 == 2,
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
		[InlineData(8)]
		[InlineData(3)]
		public async Task WhenInvokedInBackground_WithCancellation_ShouldSucceed(int invocationTimes)
		{
			IMyService sut = IMyService.CreateMock();
			using CancellationTokenSource cts = new(30.Seconds());
			CancellationToken token = cts.Token;

			Task backgroundTask = Task.Run(async () =>
			{
				await Task.Delay(50, token);
				for (int i = 0; i < invocationTimes; i++)
				{
					sut.MyMethod(1, false);
					await Task.Delay(50);
				}
			}, token);

			await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).Times(n => n % 3 == 2)
				.WithCancellation(token);

			await backgroundTask;
		}

		[Theory]
		[InlineData(8)]
		[InlineData(3)]
		public async Task WhenInvokedInBackground_Within_ShouldSucceed(int invocationTimes)
		{
			IMyService sut = IMyService.CreateMock();

			Task backgroundTask = Task.Run(async () =>
			{
				await Task.Delay(50);
				for (int i = 0; i < invocationTimes; i++)
				{
					sut.MyMethod(1, false);
					await Task.Delay(50);
				}
			});

			await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).Times(n => n % 3 == 2)
				.Within(30.Seconds());

			await backgroundTask;
		}

		[Theory]
		[InlineData(8)]
		[InlineData(3)]
		public async Task WhenInvokedInBackground_WithTimeout_ShouldSucceed(int invocationTimes)
		{
			IMyService sut = IMyService.CreateMock();

			Task backgroundTask = Task.Run(async () =>
			{
				await Task.Delay(50);
				for (int i = 0; i < invocationTimes; i++)
				{
					sut.MyMethod(1, false);
					await Task.Delay(50);
				}
			});

			await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).Times(n => n % 3 == 2)
				.WithTimeout(30.Seconds());

			await backgroundTask;
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
				await That(sut.Mock.Verify.MyMethod(It.IsAny<int>(), It.Is(true))).Times(x => x != 4)
					.Within(50.Milliseconds());
			}

			await That(Act).Throws<XunitException>()
				.WithMessage("""
				             Expected that the aweXpect.Mockolate.Tests.ThatVerificationResultIs.IMyService mock
				             invoked method MyMethod(It.IsAny<int>(), true) according to the predicate x => x != 4 within 0:00.050,
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

		[Fact]
		public async Task WhenPredicateThrows_ShouldFail()
		{
			InvalidOperationException exception = new("predicate failed");
			IMyService sut = IMyService.CreateMock();

			sut.MyMethod(1, false);

			async Task Act()
			{
				await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).Times(_ => throw exception);
			}

			await That(Act).Throws<XunitException>()
				.WithMessage("""
				             Expected that the aweXpect.Mockolate.Tests.ThatVerificationResultIs.IMyService mock
				             invoked method MyMethod(1, false) according to the predicate _ => throw exception,
				             but the predicate did throw an InvalidOperationException:
				               predicate failed

				             Matching Interactions:
				             [
				               invoke method MyMethod(1, False)
				             ]
				             """).And
				.Whose(e => e.InnerException, i => i.IsSameAs(exception))
				.Because("the predicate answered nothing, so the expectation fails with its exception");
		}

		[Fact]
		public async Task WhenPredicateStillThrowsAtTheEnd_Within_ShouldFail()
		{
			InvalidOperationException exception = new("predicate failed");
			IMyService sut = IMyService.CreateMock();

			async Task Act()
			{
				await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).Times(_ => throw exception)
					.Within(50.Milliseconds());
			}

			await That(Act).Throws<XunitException>()
				.WithMessage("""
				             Expected that the aweXpect.Mockolate.Tests.ThatVerificationResultIs.IMyService mock
				             invoked method MyMethod(1, false) according to the predicate _ => throw exception within 0:00.050,
				             but the predicate did throw an InvalidOperationException:
				               predicate failed

				             Matching Interactions:
				             []
				             """).And
				.Whose(e => e.InnerException, i => i.IsSameAs(exception))
				.Because("the last check still threw, so the expectation fails with its exception");
		}

		[Fact]
		public async Task WhenPredicateStillThrowsAtTheEnd_WithTimeout_ShouldFail()
		{
			InvalidOperationException exception = new("predicate failed");
			IMyService sut = IMyService.CreateMock();

			async Task Act()
			{
				await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).Times(_ => throw exception)
					.WithTimeout(50.Milliseconds());
			}

			await That(Act).Throws<XunitException>()
				.WithMessage("""
				             Expected that the aweXpect.Mockolate.Tests.ThatVerificationResultIs.IMyService mock
				             invoked method MyMethod(1, false) according to the predicate _ => throw exception,
				             but the predicate did throw an InvalidOperationException:
				               predicate failed

				             Matching Interactions:
				             []
				             """).And
				.Whose(e => e.InnerException, i => i.IsSameAs(exception))
				.Because("the timeout also makes the verification wait, and the last check still threw");
		}

		[Fact]
		public async Task WhenPredicateThrowsFirstAndIsSatisfiedLater_Within_ShouldSucceed()
		{
			IMyService sut = IMyService.CreateMock();
			TaskCompletionSource<bool> hasThrown = new(TaskCreationOptions.RunContinuationsAsynchronously);
			Task backgroundTask = hasThrown.Task.ContinueWith(_ => sut.MyMethod(1, false));

			async Task Act()
			{
				await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).Times(n =>
					{
						if (n == 0)
						{
							hasThrown.TrySetResult(true);
							throw new InvalidOperationException("predicate failed");
						}

						return n == 1;
					})
					.Within(30.Seconds());
			}

			Stopwatch stopwatch = Stopwatch.StartNew();
			await That(Act).DoesNotThrow()
				.Because("an exception of the predicate counts as not met while waiting for further interactions");
			await That(stopwatch.Elapsed).IsLessThan(10.Seconds())
				.Because("the verification succeeds as soon as the interaction happened");

			await backgroundTask;
		}

		[Fact]
		public async Task WhenPredicateIsNull_ShouldThrowArgumentNullException()
		{
			IMyService sut = IMyService.CreateMock();

			async Task Act()
			{
				await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false))).Times(null!);
			}

			await That(Act).Throws<ArgumentNullException>()
				.WithParamName("predicate").And
				.WithMessage("The 'predicate' cannot be null.").AsPrefix()
				.Because("a missing argument fails where it is passed instead of during the evaluation");
		}

		public sealed class NegatedTests
		{
			[Fact]
			public async Task WhenPredicateDoesNotMatch_ShouldSucceed()
			{
				IMyService sut = IMyService.CreateMock();

				sut.MyMethod(1, false);

				async Task Act()
				{
					await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false)))
						.DoesNotComplyWith(it => it.Times(n => n == 2));
				}

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenPredicateMatches_ShouldFail()
			{
				IMyService sut = IMyService.CreateMock();

				sut.MyMethod(1, false);
				sut.MyMethod(1, false);

				async Task Act()
				{
					await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false)))
						.DoesNotComplyWith(it => it.Times(n => n == 2));
				}

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that the aweXpect.Mockolate.Tests.ThatVerificationResultIs.IMyService mock
					             invoked method MyMethod(1, false) not according to the predicate n => n == 2,
					             but found it twice

					             Matching Interactions:
					             [
					               invoke method MyMethod(1, False),
					               invoke method MyMethod(1, False)
					             ]
					             """);
			}

			[Fact]
			public async Task WhenPredicateThrows_ShouldFail()
			{
				InvalidOperationException exception = new("predicate failed");
				IMyService sut = IMyService.CreateMock();

				sut.MyMethod(1, false);

				async Task Act()
				{
					await That(sut.Mock.Verify.MyMethod(It.Is(1), It.Is(false)))
						.DoesNotComplyWith(it => it.Times(_ => throw exception));
				}

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that the aweXpect.Mockolate.Tests.ThatVerificationResultIs.IMyService mock
					             invoked method MyMethod(1, false) not according to the predicate _ => throw exception,
					             but the predicate did throw an InvalidOperationException:
					               predicate failed

					             Matching Interactions:
					             [
					               invoke method MyMethod(1, False)
					             ]
					             """).And
					.Whose(e => e.InnerException, i => i.IsSameAs(exception))
					.Because("a predicate that answered nothing must not be inverted into a success");
			}
		}
	}
}
