[assembly: System.Reflection.AssemblyMetadata("IsAotCompatible", "True")]
[assembly: System.Reflection.AssemblyMetadata("IsTrimmable", "True")]
[assembly: System.Reflection.AssemblyMetadata("RepositoryUrl", "https://github.com/Testably/aweXpect.Mockolate.git")]
[assembly: System.Runtime.Versioning.TargetFramework(".NETCoreApp,Version=v10.0", FrameworkDisplayName = ".NET 10.0")]
namespace Mockolate.Web
{
    public static class AweXpectItExtensions
    {
        extension(Mockolate.Web.ItExtensions.IHttpContentParameter parameter)
        {
            public Mockolate.Web.AweXpectItExtensions.IJsonContentBodyParameter WithJson(string json, System.Text.Json.JsonDocumentOptions? options = null) { }
            public Mockolate.Web.AweXpectItExtensions.IJsonContentBodyParameter WithJsonMatching(object? expected, System.Text.Json.JsonDocumentOptions? options = null) { }
            public Mockolate.Web.AweXpectItExtensions.IJsonContentBodyParameter WithJsonMatching<T>(System.Collections.Generic.IEnumerable<T> expected, System.Text.Json.JsonDocumentOptions? options = null) { }
        }
        public interface IJsonContentBodyParameter : Mockolate.Parameters.IParameter, Mockolate.Parameters.IParameter<System.Net.Http.HttpContent?>, Mockolate.Parameters.IParameterWithCallback<System.Net.Http.HttpContent?>, Mockolate.Web.ItExtensions.IHttpContentParameter, Mockolate.Web.ItExtensions.IHttpHeaderParameter<Mockolate.Web.ItExtensions.IHttpContentHeaderParameter>
        {
            Mockolate.Web.AweXpectItExtensions.IJsonContentBodyParameter IgnoringAdditionalArrayElements(bool ignoreAdditionalArrayElements = true);
            Mockolate.Web.AweXpectItExtensions.IJsonContentBodyParameter IgnoringAdditionalProperties(bool ignoreAdditionalProperties = true);
        }
    }
}
namespace aweXpect
{
    public static class ThatMockVerify
    {
        [aweXpect.Core.GuaranteesNotNull]
        public static aweXpect.Results.AndOrResult<Mockolate.Verify.IMockVerify<TVerify>, aweXpect.Core.IThat<Mockolate.Verify.IMockVerify<TVerify>>> AllInteractionsAreVerified<TVerify>(this aweXpect.Core.IThat<Mockolate.Verify.IMockVerify<TVerify>> subject) { }
        [aweXpect.Core.GuaranteesNotNull]
        public static aweXpect.Results.AndOrResult<Mockolate.Verify.IMockVerify<TVerify>, aweXpect.Core.IThat<Mockolate.Verify.IMockVerify<TVerify>>> AllSetupsAreUsed<TVerify>(this aweXpect.Core.IThat<Mockolate.Verify.IMockVerify<TVerify>> subject) { }
    }
    public static class ThatVerificationResult
    {
        [aweXpect.Core.GuaranteesNotNull]
        public static aweXpect.Mockolate.Results.AndOrWithinResult<Mockolate.Verify.VerificationResult<TVerify>, aweXpect.Core.IThat<Mockolate.Verify.VerificationResult<TVerify>>> AtLeast<TVerify>(this aweXpect.Core.IThat<Mockolate.Verify.VerificationResult<TVerify>> subject, aweXpect.Core.Times times) { }
        [aweXpect.Core.GuaranteesNotNull]
        public static aweXpect.Mockolate.Results.AndOrWithinResult<Mockolate.Verify.VerificationResult<TVerify>, aweXpect.Core.IThat<Mockolate.Verify.VerificationResult<TVerify>>> AtLeastOnce<TVerify>(this aweXpect.Core.IThat<Mockolate.Verify.VerificationResult<TVerify>> subject) { }
        [aweXpect.Core.GuaranteesNotNull]
        public static aweXpect.Mockolate.Results.AndOrWithinResult<Mockolate.Verify.VerificationResult<TVerify>, aweXpect.Core.IThat<Mockolate.Verify.VerificationResult<TVerify>>> AtLeastTwice<TVerify>(this aweXpect.Core.IThat<Mockolate.Verify.VerificationResult<TVerify>> subject) { }
        [aweXpect.Core.GuaranteesNotNull]
        public static aweXpect.Results.AndOrResult<Mockolate.Verify.VerificationResult<TVerify>, aweXpect.Core.IThat<Mockolate.Verify.VerificationResult<TVerify>>> AtMost<TVerify>(this aweXpect.Core.IThat<Mockolate.Verify.VerificationResult<TVerify>> subject, aweXpect.Core.Times times) { }
        [aweXpect.Core.GuaranteesNotNull]
        public static aweXpect.Results.AndOrResult<Mockolate.Verify.VerificationResult<TVerify>, aweXpect.Core.IThat<Mockolate.Verify.VerificationResult<TVerify>>> AtMostOnce<TVerify>(this aweXpect.Core.IThat<Mockolate.Verify.VerificationResult<TVerify>> subject) { }
        [aweXpect.Core.GuaranteesNotNull]
        public static aweXpect.Results.AndOrResult<Mockolate.Verify.VerificationResult<TVerify>, aweXpect.Core.IThat<Mockolate.Verify.VerificationResult<TVerify>>> AtMostTwice<TVerify>(this aweXpect.Core.IThat<Mockolate.Verify.VerificationResult<TVerify>> subject) { }
        [aweXpect.Core.GuaranteesNotNull]
        public static aweXpect.Results.BetweenResult<aweXpect.Mockolate.Results.AndOrWithinResult<Mockolate.Verify.VerificationResult<TVerify>, aweXpect.Core.IThat<Mockolate.Verify.VerificationResult<TVerify>>>, aweXpect.Core.Times> Between<TVerify>(this aweXpect.Core.IThat<Mockolate.Verify.VerificationResult<TVerify>> subject, int minimum) { }
        [aweXpect.Core.GuaranteesNotNull]
        public static aweXpect.Mockolate.Results.AndOrWithinResult<Mockolate.Verify.VerificationResult<TVerify>, aweXpect.Core.IThat<Mockolate.Verify.VerificationResult<TVerify>>> Exactly<TVerify>(this aweXpect.Core.IThat<Mockolate.Verify.VerificationResult<TVerify>> subject, aweXpect.Core.Times times) { }
        [aweXpect.Core.GuaranteesNotNull]
        public static aweXpect.Results.AndOrResult<Mockolate.Verify.VerificationResult<TVerify>, aweXpect.Core.IThat<Mockolate.Verify.VerificationResult<TVerify>>> Never<TVerify>(this aweXpect.Core.IThat<Mockolate.Verify.VerificationResult<TVerify>> subject) { }
        [aweXpect.Core.GuaranteesNotNull]
        public static aweXpect.Mockolate.Results.AndOrWithinResult<Mockolate.Verify.VerificationResult<TVerify>, aweXpect.Core.IThat<Mockolate.Verify.VerificationResult<TVerify>>> Once<TVerify>(this aweXpect.Core.IThat<Mockolate.Verify.VerificationResult<TVerify>> subject) { }
        [aweXpect.Core.GuaranteesNotNull]
        public static aweXpect.Mockolate.Results.AndOrWithinResult<Mockolate.Verify.VerificationResult<TVerify>, aweXpect.Core.IThat<Mockolate.Verify.VerificationResult<TVerify>>> Then<TVerify>(this aweXpect.Core.IThat<Mockolate.Verify.VerificationResult<TVerify>> subject, params System.Func<TVerify, Mockolate.Verify.VerificationResult<TVerify>>[] interactions) { }
        [aweXpect.Core.GuaranteesNotNull]
        public static aweXpect.Mockolate.Results.AndOrWithinResult<Mockolate.Verify.VerificationResult<TVerify>, aweXpect.Core.IThat<Mockolate.Verify.VerificationResult<TVerify>>> Times<TVerify>(this aweXpect.Core.IThat<Mockolate.Verify.VerificationResult<TVerify>> subject, System.Func<int, bool> predicate, [System.Runtime.CompilerServices.CallerArgumentExpression("predicate")] string doNotPopulateThisValue = "") { }
        [aweXpect.Core.GuaranteesNotNull]
        public static aweXpect.Mockolate.Results.AndOrWithinResult<Mockolate.Verify.VerificationResult<TVerify>, aweXpect.Core.IThat<Mockolate.Verify.VerificationResult<TVerify>>> Twice<TVerify>(this aweXpect.Core.IThat<Mockolate.Verify.VerificationResult<TVerify>> subject) { }
    }
}
namespace aweXpect.Mockolate.Options
{
    [aweXpect.Core.ImmutableOptions]
    public readonly struct WithinOptions
    {
        public System.TimeSpan? Timeout { get; }
        public static aweXpect.Mockolate.Options.WithinOptions Default { get; }
        public override string ToString() { }
        public static aweXpect.Mockolate.Options.WithinOptions Within(System.TimeSpan timeout) { }
    }
}
namespace aweXpect.Mockolate.Results
{
    public class AndOrWithinResult<TType, TThat> : aweXpect.Results.AndOrResult<TType, TThat, aweXpect.Mockolate.Results.AndOrWithinResult<TType, TThat>>, aweXpect.Core.IOptionsProvider<aweXpect.Mockolate.Options.WithinOptions>
    {
        public aweXpect.Mockolate.Results.AndOrWithinResult<TType, TThat> Within(System.TimeSpan timeout) { }
    }
}

