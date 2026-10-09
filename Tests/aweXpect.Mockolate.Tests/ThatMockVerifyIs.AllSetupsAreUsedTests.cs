using Mockolate;
using Mockolate.Verify;
using Xunit.Sdk;

namespace aweXpect.Mockolate.Tests;

public sealed partial class ThatMockVerifyIs
{
	public sealed class AllSetupsAreUsedTests
	{
		[Fact]
		public async Task Negated_WhenAllSetupsAreUsed_ShouldThrow()
		{
			IMyService sut = IMyService.CreateMock();
			sut.Mock.Setup.DoWork(It.IsAny<int>());

			sut.DoWork(1);
			sut.DoWork(2);

			async Task Act()
			{
				await That(sut.Mock.Verify).DoesNotComplyWith(it => it.AllSetupsAreUsed());
			}

			await That(Act).Throws<XunitException>()
				.WithMessage("""
				             Expected that the aweXpect.Mockolate.Tests.ThatMockVerifyIs.IMyService mock
				             has not used all setups,
				             but all were
				             """);
		}

		[Fact]
		public async Task Negated_WhenSubjectIsNull_ShouldThrow()
		{
			IMockVerify<IMyService>? subject = null;

			async Task Act()
			{
				await That(subject!).DoesNotComplyWith(it => it.AllSetupsAreUsed());
			}

			await That(Act).Throws<XunitException>()
				.WithMessage("""
				             Expected that the ThatMockVerifyIs.IMyService mock
				             has not used all setups,
				             but it was <null>
				             """)
				.Because("a null subject cannot be verified, so the negation fails as well");
		}

		[Fact]
		public async Task WhenSubjectIsNull_ShouldThrow()
		{
			IMockVerify<IMyService>? subject = null;

			async Task Act()
			{
				await That(subject!).AllSetupsAreUsed();
			}

			await That(Act).Throws<XunitException>()
				.WithMessage("""
				             Expected that the ThatMockVerifyIs.IMyService mock
				             has used all setups,
				             but it was <null>
				             """);
		}

		[Fact]
		public async Task WhenAllInvocationsWereVerified_ShouldNotThrow()
		{
			IMyService sut = IMyService.CreateMock();
			sut.Mock.Setup.DoWork(It.IsAny<int>());

			sut.DoWork(1);
			sut.DoWork(2);

			async Task Act()
			{
				await That(sut.Mock.Verify).AllSetupsAreUsed();
			}

			await That(Act).DoesNotThrow();
		}

		[Fact]
		public async Task WhenMultipleSetupsAreNotUsed_ShouldThrow()
		{
			IMyService sut = IMyService.CreateMock();
			sut.Mock.Setup.DoWork(It.Is(1));
			sut.Mock.Setup.DoWork(It.Is(2));
			sut.Mock.Setup.DoWork(It.Is(3));

			sut.DoWork(2);

			async Task Act()
			{
				await That(sut.Mock.Verify).AllSetupsAreUsed();
			}

			await That(Act).Throws<XunitException>()
				.WithMessage("""
				             Expected that the aweXpect.Mockolate.Tests.ThatMockVerifyIs.IMyService mock
				             has used all setups,
				             but the following 2 setups were not used:
				              - void DoWork(1)
				              - void DoWork(3)
				             """);
		}

		[Fact]
		public async Task WhenOneSetupIsNotUsed_ShouldThrow()
		{
			IMyService sut = IMyService.CreateMock();
			sut.Mock.Setup.DoWork(It.Is(1));
			sut.Mock.Setup.DoWork(It.Is(2));

			sut.DoWork(1);

			async Task Act()
			{
				await That(sut.Mock.Verify).AllSetupsAreUsed();
			}

			await That(Act).Throws<XunitException>()
				.WithMessage("""
				             Expected that the aweXpect.Mockolate.Tests.ThatMockVerifyIs.IMyService mock
				             has used all setups,
				             but the following setup was not used:
				              - void DoWork(2)
				             """);
		}
	}
}
