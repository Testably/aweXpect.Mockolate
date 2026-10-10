using System.Collections.Generic;
using System.Text;
using aweXpect.Core;
using aweXpect.Core.Constraints;
using aweXpect.Helpers;
using aweXpect.Results;
using Mockolate;
using Mockolate.Setup;
using Mockolate.Verify;

namespace aweXpect;

public static partial class ThatMockVerify
{
	/// <summary>
	///     Verifies that all setups on the mock have been used.
	/// </summary>
	[GuaranteesNotNull]
	public static AndOrResult<IMockVerify<TVerify>, IThat<IMockVerify<TVerify>>>
		AllSetupsAreUsed<TVerify>(
			this IThat<IMockVerify<TVerify>> subject)
		=> new(subject.Get().ExpectationBuilder.AddConstraint((it, grammars)
				=> new AllSetupsAreUsedConstraint<TVerify>(it, grammars)),
			subject);

	private sealed class AllSetupsAreUsedConstraint<TVerify>(
		string it,
		ExpectationGrammars grammars)
		: ConstraintResult.WithNotNullValue<IMockVerify<TVerify>>(it, grammars),
			IValueConstraint<IMockVerify<TVerify>>
	{
		private IReadOnlyCollection<ISetup>? _unusedSetups;

		public ConstraintResult IsMetBy(IMockVerify<TVerify>? actual)
		{
			Actual = actual;
			_unusedSetups = actual is IMock mock
				? mock.MockRegistry.GetUnusedSetups(mock.MockRegistry.Interactions)
				: null;
			Outcome = _unusedSetups?.Count == 0 ? Outcome.Success : Outcome.Failure;
			return this;
		}

		protected override void AppendNormalExpectation(StringBuilder stringBuilder, string? indentation = null)
			=> stringBuilder.Append("has used all setups");

		protected override void AppendNormalResult(StringBuilder stringBuilder, string? indentation = null)
		{
			if (_unusedSetups is null)
			{
				stringBuilder.Append(It).Append(" was not a Mockolate mock");
			}
			else
			{
				stringBuilder.Append(It).Append(" had ").Append(_unusedSetups.Count)
					.Append(_unusedSetups.Count == 1 ? " unused setup" : " unused setups");
			}
		}

		protected override void AppendNegatedExpectation(StringBuilder stringBuilder, string? indentation = null)
			=> stringBuilder.Append("has not used all setups");

		protected override void AppendNegatedResult(StringBuilder stringBuilder, string? indentation = null)
			=> stringBuilder.Append(It).Append(" did");

		public override void AppendContexts(ResultContextCollector contexts)
		{
			if (_unusedSetups is { Count: > 0, } unusedSetups)
			{
				contexts.Add(new ResultContext.SyncCallback("Unused Setups",
					() => Formatter.Format(unusedSetups, FormattingOptions.MultipleLines)));
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
