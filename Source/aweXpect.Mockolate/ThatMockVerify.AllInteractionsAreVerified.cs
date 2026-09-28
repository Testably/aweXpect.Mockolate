using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
	public static AndOrResult<IMockVerify<TVerify>, IThat<IMockVerify<TVerify>>>
		AllInteractionsAreVerified<TVerify>(
			this IThat<IMockVerify<TVerify>> subject)
		=> new(subject.Get().ExpectationBuilder.AddConstraint((_, it, grammars)
				=> new AllInteractionsAreVerifiedConstraint<TVerify>(it, grammars)),
			subject);

	private sealed class AllInteractionsAreVerifiedConstraint<TVerify>(
		string it,
		ExpectationGrammars grammars)
		: ConstraintResult.WithValue<IMockVerify<TVerify>>(it, grammars),
			IValueConstraint<IMockVerify<TVerify>>
	{
		public ConstraintResult IsMetBy(IMockVerify<TVerify> actual)
		{
			Actual = actual;
			Outcome = actual is IMock mock && mock.MockRegistry.Interactions.GetUnverifiedInteractions().Count == 0
				? Outcome.Success
				: Outcome.Failure;
			return this;
		}

		protected override void AppendNormalExpectation(StringBuilder stringBuilder, string? indentation = null)
			=> stringBuilder.Append("has all interactions verified");

		protected override void AppendNormalResult(StringBuilder stringBuilder, string? indentation = null)
		{
			if (Actual is IMock mock)
			{
				IReadOnlyCollection<IInteraction> missingInteractions =
					mock.MockRegistry.Interactions.GetUnverifiedInteractions();
				stringBuilder.Append("the following ");
				if (missingInteractions.Count == 1)
				{
					stringBuilder.Append("interaction was not verified:");
				}
				else
				{
					stringBuilder.Append(missingInteractions.Count)
						.Append(" interactions were not verified:");
				}

				stringBuilder.AppendLine().Append(" - ");
				stringBuilder.Append(string.Join($"{Environment.NewLine} - ", missingInteractions));
			}
			else
			{
				stringBuilder.Append("not all were");
			}
		}

		protected override void AppendNegatedExpectation(StringBuilder stringBuilder, string? indentation = null)
			=> stringBuilder.Append("has not all interactions verified");

		protected override void AppendNegatedResult(StringBuilder stringBuilder, string? indentation = null)
			=> stringBuilder.Append("all were");

		public override bool TryGetValue<TValue>([NotNullWhen(true)] out TValue? value) where TValue : default
		{
			if (typeof(TValue) == typeof(IDescribableSubject) &&
			    new MyDescribableSubject<TVerify>(Actual as IMock) is TValue describableSubject)
			{
				value = describableSubject;
				return true;
			}

			return base.TryGetValue(out value);
		}
	}
}
