using System;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using aweXpect.Core;
using aweXpect.Core.Constraints;
using aweXpect.Core.EvaluationContext;
using aweXpect.Helpers;
using aweXpect.Options;
using aweXpect.Results;
using Mockolate.Verify;

namespace aweXpect;

public static partial class ThatVerificationResult
{
	/// <summary>
	///     Verifies that the checked interaction happened according to the <paramref name="predicate" />.
	/// </summary>
	/// <exception cref="ArgumentNullException">The <paramref name="predicate" /> is <see langword="null" />.</exception>
	[GuaranteesNotNull]
	public static AndOrWithinResult<VerificationResult<TVerify>, IThat<VerificationResult<TVerify>>> Times<TVerify>(
		this IThat<VerificationResult<TVerify>> subject, Func<int, bool> predicate,
		[CallerArgumentExpression("predicate")]
		string doNotPopulateThisValue = "")
	{
		if (predicate is null)
		{
			throw Tracing.WriteException(
				new ArgumentNullException(nameof(predicate), "The 'predicate' cannot be null."));
		}

		WithinOptions options = new();
		return new AndOrWithinResult<VerificationResult<TVerify>, IThat<VerificationResult<TVerify>>>(
			subject.Get().ExpectationBuilder.AddConstraint((expectationBuilder, it, grammars)
				=> new TimesConstraint<TVerify>(expectationBuilder, it, grammars, predicate, doNotPopulateThisValue,
					options)),
			subject,
			options);
	}

	private sealed class TimesConstraint<TVerify>(
		ExpectationBuilder expectationBuilder,
		string it,
		ExpectationGrammars grammars,
		Func<int, bool> predicate,
		string predicateExpression,
		WithinOptions options)
		: VerificationCountConstraint<TVerify>(it, grammars),
			IAsyncContextConstraint<VerificationResult<TVerify>>
	{
		public async ValueTask<ConstraintResult> IsMetBy(VerificationResult<TVerify>? actual,
			IEvaluationContext context, CancellationToken cancellationToken)
		{
			await VerifyAsync(actual, expectationBuilder, options, context, cancellationToken);
			return this;
		}

		protected override bool IsMet(int count) => UserCode.Invoke(predicate, count, "the predicate");

		protected override void AppendNormalExpectation(StringBuilder stringBuilder, string? indentation = null)
			=> stringBuilder.Append(Expectation).Append(" according to the predicate ").Append(predicateExpression)
				.Append(options);

		protected override void AppendNormalResult(StringBuilder stringBuilder, string? indentation = null)
		{
			if (Count == 0)
			{
				stringBuilder.Append("never found ").Append(It);
			}
			else
			{
				stringBuilder.Append("found ").Append(It).Append(' ').Append(Count.ToAmountString());
			}
		}

		protected override void AppendNegatedExpectation(StringBuilder stringBuilder, string? indentation = null)
			=> stringBuilder.Append(Expectation).Append(" not according to the predicate ")
				.Append(predicateExpression).Append(options);

		protected override void AppendNegatedResult(StringBuilder stringBuilder, string? indentation = null)
			=> AppendNormalResult(stringBuilder, indentation);
	}
}
