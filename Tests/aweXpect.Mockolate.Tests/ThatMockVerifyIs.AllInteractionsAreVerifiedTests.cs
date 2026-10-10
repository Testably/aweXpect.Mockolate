using Mockolate;
using Mockolate.Verify;
using Xunit.Sdk;

namespace aweXpect.Mockolate.Tests;

public sealed partial class ThatMockVerifyIs
{
	public sealed class AllInteractionsAreVerifiedTests
	{
		[Fact]
		public async Task Negated_WhenAllAreVerified_ShouldThrow()
		{
			IMyService sut = IMyService.CreateMock();

			sut.DoWork(1);
			sut.DoWork(2);

			await That(sut.Mock.Verify.DoWork(It.IsAny<int>())).AtLeastOnce();

			async Task Act()
			{
				await That(sut.Mock.Verify).DoesNotComplyWith(it => it.AllInteractionsAreVerified());
			}

			await That(Act).Throws<XunitException>()
				.WithMessage("""
				             Expected that the aweXpect.Mockolate.Tests.ThatMockVerifyIs.IMyService mock
				             does not have all interactions verified,
				             but it did
				             """);
		}

		[Fact]
		public async Task Negated_WhenSubjectIsNull_ShouldThrow()
		{
			IMockVerify<IMyService>? subject = null;

			async Task Act()
			{
				await That(subject!).DoesNotComplyWith(it => it.AllInteractionsAreVerified());
			}

			await That(Act).Throws<XunitException>()
				.WithMessage("""
				             Expected that the ThatMockVerifyIs.IMyService mock
				             does not have all interactions verified,
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
				await That(subject!).AllInteractionsAreVerified();
			}

			await That(Act).Throws<XunitException>()
				.WithMessage("""
				             Expected that the ThatMockVerifyIs.IMyService mock
				             has all interactions verified,
				             but it was <null>
				             """);
		}

		[Fact]
		public async Task WhenAllInvocationsWereVerified_ShouldNotThrow()
		{
			IMyService sut = IMyService.CreateMock();

			sut.DoWork(1);
			sut.DoWork(2);

			await That(sut.Mock.Verify.DoWork(It.IsAny<int>())).AtLeastOnce();

			async Task Act()
			{
				await That(sut.Mock.Verify).AllInteractionsAreVerified();
			}

			await That(Act).DoesNotThrow();
		}

		[Fact]
		public async Task WhenMultipleInvocationIsNotVerified_ShouldThrow()
		{
			IMyService sut = IMyService.CreateMock();

			sut.DoWork(1);
			sut.DoWork(2);
			sut.DoWork(3);

			await That(sut.Mock.Verify.DoWork(It.Is(2))).AtLeastOnce();

			async Task Act()
			{
				await That(sut.Mock.Verify).AllInteractionsAreVerified();
			}

			await That(Act).Throws<XunitException>()
				.WithMessage("""
				             Expected that the aweXpect.Mockolate.Tests.ThatMockVerifyIs.IMyService mock
				             has all interactions verified,
				             but it had 2 unverified interactions

				             Unverified Interactions:
				             [
				               invoke method DoWork(1),
				               invoke method DoWork(3)
				             ]
				             """);
		}

		[Fact]
		public async Task WhenOneInvocationIsNotVerified_ShouldThrow()
		{
			IMyService sut = IMyService.CreateMock();

			sut.DoWork(1);
			sut.DoWork(2);

			await That(sut.Mock.Verify.DoWork(It.Is(1))).AtLeastOnce();

			async Task Act()
			{
				await That(sut.Mock.Verify).AllInteractionsAreVerified();
			}

			await That(Act).Throws<XunitException>()
				.WithMessage("""
				             Expected that the aweXpect.Mockolate.Tests.ThatMockVerifyIs.IMyService mock
				             has all interactions verified,
				             but it had 1 unverified interaction

				             Unverified Interactions:
				             [
				               invoke method DoWork(2)
				             ]
				             """);
		}
	}
}
