using System.Collections.Generic;
using System.Text;
using aweXpect.Core;
using aweXpect.Core.Constraints;
using aweXpect.Helpers;
using aweXpect.Results;
using Mockolate;
using Mockolate.Interactions;
using Mockolate.Verify;

namespace aweXpect;

public static partial class ThatMockVerify
{
	/// <summary>
	///     Verifies that all interactions on the mock have been verified.
	/// </summary>
	[GuaranteesNotNull]
	public static AndOrResult<IMockVerify<TVerify>, IThat<IMockVerify<TVerify>>>
		AllInteractionsAreVerified<TVerify>(
			this IThat<IMockVerify<TVerify>> subject)
		=> new(subject.Get().ExpectationBuilder.AddConstraint((it, grammars)
				=> new AllInteractionsAreVerifiedConstraint<TVerify>(it, grammars)),
			subject);

	private sealed class AllInteractionsAreVerifiedConstraint<TVerify>(
		string it,
		ExpectationGrammars grammars)
		: ConstraintResult.WithNotNullValue<IMockVerify<TVerify>>(it, grammars),
			IValueConstraint<IMockVerify<TVerify>>
	{
		private IReadOnlyCollection<IInteraction>? _unverifiedInteractions;

		public ConstraintResult IsMetBy(IMockVerify<TVerify>? actual)
		{
			Actual = actual;
			_unverifiedInteractions = actual is IMock mock
				? mock.MockRegistry.Interactions.GetUnverifiedInteractions()
				: null;
			Outcome = _unverifiedInteractions?.Count == 0 ? Outcome.Success : Outcome.Failure;
			return this;
		}

		protected override void AppendNormalExpectation(StringBuilder stringBuilder, string? indentation = null)
			=> stringBuilder.Append("has all interactions verified");

		protected override void AppendNormalResult(StringBuilder stringBuilder, string? indentation = null)
		{
			if (_unverifiedInteractions is null)
			{
				stringBuilder.Append(It).Append(" was not a Mockolate mock");
			}
			else
			{
				stringBuilder.Append(It).Append(" had ").Append(_unverifiedInteractions.Count)
					.Append(_unverifiedInteractions.Count == 1 ? " unverified interaction" : " unverified interactions");
			}
		}

		protected override void AppendNegatedExpectation(StringBuilder stringBuilder, string? indentation = null)
			=> stringBuilder.Append("does not have all interactions verified");

		protected override void AppendNegatedResult(StringBuilder stringBuilder, string? indentation = null)
			=> stringBuilder.Append(It).Append(" did");

		public override void AppendContexts(ResultContextCollector contexts)
		{
			if (_unverifiedInteractions is { Count: > 0, } unverifiedInteractions)
			{
				contexts.Add(new ResultContext.SyncCallback("Unverified Interactions",
					() => Formatter.Format(unverifiedInteractions, FormattingOptions.MultipleLines)));
			}
		}

		public override bool TryGetStoredValue<TValue>(out TValue? value) where TValue : default
		{
			if (typeof(TValue) == typeof(IDescribableSubject) &&
			    new MyDescribableSubject<TVerify>(Actual as IMock) is TValue describableSubject)
			{
				value = describableSubject;
				return true;
			}

			return base.TryGetStoredValue(out value);
		}
	}
}
