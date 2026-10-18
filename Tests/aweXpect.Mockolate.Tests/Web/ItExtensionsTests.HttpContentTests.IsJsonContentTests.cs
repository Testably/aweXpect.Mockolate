#if NET8_0_OR_GREATER
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using Mockolate;
using Mockolate.Web;

namespace aweXpect.Mockolate.Tests.Web;

public sealed partial class ItExtensionsTests
{
	public sealed class HttpContentTests
	{
		public sealed class IsJsonContentTests
		{
			[Test]
			[Arguments("true", true, true)]
			[Arguments("true", false, false)]
			[Arguments("false", true, false)]
			[Arguments("false", false, true)]
			public async Task BooleanValue_ShouldSucceedWhenMatching(string body, bool expected, bool expectSuccess)
			{
				HttpClient httpClient = HttpClient.CreateMock();
				httpClient.Mock.Setup
					.PostAsync(It.IsAny<Uri>(), It.IsHttpContent().WithJsonMatching(expected))
					.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

				HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
					new StringContent(body),
					CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}

			[Test]
			[Arguments("42.1", 42.1, true)]
			[Arguments("1.2", 2.1, false)]
			public async Task DoubleValue_ShouldSucceedWhenMatching(string body, double expected, bool expectSuccess)
			{
				HttpClient httpClient = HttpClient.CreateMock();
				httpClient.Mock.Setup
					.PostAsync(It.IsAny<Uri>(), It.IsHttpContent().WithJsonMatching(expected))
					.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

				HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
					new StringContent(body),
					CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}

			[Test]
			[Arguments("42", 42, true)]
			[Arguments("1", 2, false)]
			public async Task IntegerValue_ShouldSucceedWhenMatching(string body, int expected, bool expectSuccess)
			{
				HttpClient httpClient = HttpClient.CreateMock();
				httpClient.Mock.Setup
					.PostAsync(It.IsAny<Uri>(), It.IsHttpContent().WithJsonMatching(expected))
					.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

				HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
					new StringContent(body),
					CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}

			[Test]
			[Arguments("null", true)]
			[Arguments("{}", false)]
			public async Task NullValue_ShouldSucceedWhenMatching(string body, bool expectSuccess)
			{
				HttpClient httpClient = HttpClient.CreateMock();
				httpClient.Mock.Setup
					.PostAsync(It.IsAny<Uri>(), It.IsHttpContent().WithJsonMatching(null))
					.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

				HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
					new StringContent(body),
					CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}

			[Test]
			public async Task ShouldSupportNestedObjects()
			{
				string body = "[{\"foo\": 2}, {\"foo\": 3}, {\"foo\": 4}]";
				HttpClient httpClient = HttpClient.CreateMock();
				httpClient.Mock.Setup
					.PostAsync(It.IsAny<Uri>(), It.IsHttpContent().WithJsonMatching([
						new
						{
							foo = 2,
						},
						new
						{
							foo = 3,
						},
						new
						{
							foo = 4,
						},
					]))
					.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

				HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
					new StringContent(body),
					CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(HttpStatusCode.OK);
				await That(httpClient.Mock.Verify
						.PostAsync(It.IsAny<Uri>(), It.IsHttpContent().WithJsonMatching([
							new
							{
								foo = 2,
							},
							new
							{
								foo = 3,
							},
							new
							{
								foo = 4,
							},
						])))
					.Once();
			}

			[Test]
			[Arguments("\"foo\"", "foo", true)]
			[Arguments("\"foo\"", "bar", false)]
			public async Task StringValue_ShouldSucceedWhenMatching(string body, string expected, bool expectSuccess)
			{
				HttpClient httpClient = HttpClient.CreateMock();
				httpClient.Mock.Setup
					.PostAsync(It.IsAny<Uri>(), It.IsHttpContent().WithJsonMatching(expected))
					.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

				HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
					new StringContent(body),
					CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}

			[Test]
			[Arguments("{\n  \"foo\": 1,\n  \"bar\": 2,\n}", "{\"bar\":2,\"foo\": 1}", true)]
			[Arguments("\"foo\"", "\"foo\"", true)]
			[Arguments("foo", "\"foo\"", false)]
			public async Task WithBody_ShouldCompareAsJson(string body,
				string expected, bool expectSuccess)
			{
				HttpClient httpClient = HttpClient.CreateMock();
				httpClient.Mock.Setup
					.PostAsync(It.IsAny<Uri>(), It.IsHttpContent().WithJson(expected))
					.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

				HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
					new StringContent(body),
					CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}

			[Test]
			public async Task WithBodyMatching_ShouldCompareAsJson()
			{
				HttpClient httpClient = HttpClient.CreateMock();
				httpClient.Mock.Setup
					.PostAsync(It.IsAny<Uri>(), It.IsHttpContent().WithJsonMatching(new
					{
						foo = 1,
						bar = 2,
					}))
					.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

				HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
					new StringContent("""
					                  {
					                    "bar": 2,
					                    "foo": 1
					                  }
					                  """),
					CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(HttpStatusCode.OK);
				await That(httpClient.Mock.Verify
						.PostAsync(It.IsAny<Uri>(), It.IsHttpContent().WithJsonMatching(new
						{
							foo = 1,
							bar = 2,
						})))
					.Once();
			}

			[Test]
			[Arguments(true)]
			[Arguments(false)]
			public async Task WithBodyMatching_WithAdditionalProperties_ShouldMatchWhenIgnoringAdditionalProperties(
				bool ignoreAdditionalProperties)
			{
				HttpClient httpClient = HttpClient.CreateMock();
				httpClient.Mock.Setup
					.PostAsync(It.IsAny<Uri>(), It.IsHttpContent().WithJsonMatching(new
					{
						foo = 1,
						bar = 2,
					}).IgnoringAdditionalProperties(ignoreAdditionalProperties))
					.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

				HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
					new StringContent("""
					                  {
					                    "bar": 2,
					                    "foo": 1,
					                    "baz": null,
					                  }
					                  """),
					CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(ignoreAdditionalProperties ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}

			[Test]
			public async Task WithJson_WhenActualContentIsInvalid_ShouldNotMatch()
			{
				HttpClient httpClient = HttpClient.CreateMock();
				httpClient.Mock.Setup
					.PostAsync(It.IsAny<Uri>(), It.IsHttpContent().WithJson("{\"foo\": 1}"))
					.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

				HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
					new StringContent("{\"foo\": "),
					CancellationToken.None);

				await That(result.StatusCode).IsEqualTo(HttpStatusCode.NotImplemented)
					.Because("content that is not valid JSON cannot match the expected JSON");
			}

			[Test]
			public async Task WithJson_WhenExpectedIsInvalid_ShouldThrowArgumentException()
			{
				void Act()
				{
					It.IsHttpContent().WithJson("{\"foo\": ");
				}

				await That(Act).Throws<ArgumentException>()
					.WithParamName("json").And
					.WithMessage("The 'json' is not valid JSON: *").AsWildcard()
					.Because("invalid expected JSON fails where it is passed instead of never matching");
			}

			[Test]
			public async Task WithJson_WhenExpectedIsInvalidForOptions_ShouldThrowArgumentException()
			{
				void Act()
				{
					It.IsHttpContent().WithJson("[1, 2,]", new JsonDocumentOptions
					{
						AllowTrailingCommas = false,
					});
				}

				await That(Act).Throws<ArgumentException>()
					.WithParamName("json").And
					.WithMessage("The 'json' is not valid JSON: *").AsWildcard()
					.Because("the expected JSON is parsed with the given options");
			}

			[Test]
			public async Task WithJson_WhenExpectedIsNull_ShouldThrowArgumentNullException()
			{
				void Act()
				{
					It.IsHttpContent().WithJson(null!);
				}

				await That(Act).Throws<ArgumentNullException>()
					.WithParamName("json").And
					.WithMessage("The 'json' cannot be null.").AsPrefix()
					.Because("a missing argument fails where it is passed instead of never matching");
			}

			[Test]
			[Arguments("[1, 2,]", "[1, 2,]", true)]
			[Arguments("[1, 2,]", "[1, 2]", false)]
			public async Task WithOptions_ShouldApplyOptions(string body, string expected, bool allowTrailingCommas)
			{
				HttpClient httpClient = HttpClient.CreateMock();
				httpClient.Mock.Setup
					.PostAsync(It.IsAny<Uri>(), It.IsHttpContent().WithJson(expected, new JsonDocumentOptions
					{
						AllowTrailingCommas = allowTrailingCommas,
					}))
					.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

				HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
					new StringContent(body),
					CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(allowTrailingCommas ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}

			public sealed class AdditionalMembersTests
			{
				public enum Body
				{
					Exact,
					AdditionalProperty,
					AdditionalNestedProperty,
					AdditionalArrayElement,
					AdditionalPropertyAndArrayElement,
					MissingProperty,
					MissingArrayElement,
					DifferentValue,
				}

				[Test]
				[MatrixDataSource]
				public async Task WithJson_ShouldApplyOptions(
					[Matrix(null, true, false)] bool? ignoreAdditionalProperties,
					[Matrix(null, true, false)] bool? ignoreAdditionalArrayElements,
					Body body)
				{
					AweXpectItExtensions.IJsonContentBodyParameter parameter = It.IsHttpContent()
						.WithJson("""{"foo": 1, "items": [{"bar": 2}, {"bar": 3}]}""");

					HttpStatusCode result = await PostAsync(parameter,
						ignoreAdditionalProperties, ignoreAdditionalArrayElements, body);

					await That(result).IsEqualTo(ExpectedStatusCode(body,
							ignoreAdditionalProperties ?? false, ignoreAdditionalArrayElements ?? false))
						.Because("WithJson does not match additional properties or array elements by default");
				}

				[Test]
				[MatrixDataSource]
				public async Task WithJsonMatching_ShouldApplyOptions(
					[Matrix(null, true, false)] bool? ignoreAdditionalProperties,
					[Matrix(null, true, false)] bool? ignoreAdditionalArrayElements,
					Body body)
				{
					AweXpectItExtensions.IJsonContentBodyParameter parameter = It.IsHttpContent().WithJsonMatching(new
					{
						foo = 1,
						items = new[]
						{
							new
							{
								bar = 2,
							},
							new
							{
								bar = 3,
							},
						},
					});

					HttpStatusCode result = await PostAsync(parameter,
						ignoreAdditionalProperties, ignoreAdditionalArrayElements, body);

					await That(result).IsEqualTo(ExpectedStatusCode(body,
							ignoreAdditionalProperties ?? true, ignoreAdditionalArrayElements ?? true))
						.Because("WithJsonMatching ignores additional properties and array elements by default");
				}

				private static HttpStatusCode ExpectedStatusCode(Body body,
					bool ignoreAdditionalProperties, bool ignoreAdditionalArrayElements)
				{
					bool isMatch = body switch
					{
						Body.Exact => true,
						Body.AdditionalProperty => ignoreAdditionalProperties,
						Body.AdditionalNestedProperty => ignoreAdditionalProperties,
						Body.AdditionalArrayElement => ignoreAdditionalArrayElements,
						Body.AdditionalPropertyAndArrayElement => ignoreAdditionalProperties &&
						                                          ignoreAdditionalArrayElements,
						_ => false,
					};
					return isMatch ? HttpStatusCode.OK : HttpStatusCode.NotImplemented;
				}

				private static async Task<HttpStatusCode> PostAsync(AweXpectItExtensions.IJsonContentBodyParameter parameter,
					bool? ignoreAdditionalProperties, bool? ignoreAdditionalArrayElements, Body body)
				{
					if (ignoreAdditionalProperties.HasValue)
					{
						parameter.IgnoringAdditionalProperties(ignoreAdditionalProperties.Value);
					}

					if (ignoreAdditionalArrayElements.HasValue)
					{
						parameter.IgnoringAdditionalArrayElements(ignoreAdditionalArrayElements.Value);
					}

					string content = body switch
					{
						Body.Exact => """{"items": [{"bar": 2}, {"bar": 3}], "foo": 1}""",
						Body.AdditionalProperty => """{"foo": 1, "baz": 4, "items": [{"bar": 2}, {"bar": 3}]}""",
						Body.AdditionalNestedProperty => """{"foo": 1, "items": [{"bar": 2}, {"bar": 3, "baz": 4}]}""",
						Body.AdditionalArrayElement => """{"foo": 1, "items": [{"bar": 2}, {"bar": 3}, {"bar": 4}]}""",
						Body.AdditionalPropertyAndArrayElement =>
							"""{"foo": 1, "baz": 4, "items": [{"bar": 2}, {"bar": 3}, {"bar": 4}]}""",
						Body.MissingProperty => """{"items": [{"bar": 2}, {"bar": 3}]}""",
						Body.MissingArrayElement => """{"foo": 1, "items": [{"bar": 2}]}""",
						_ => """{"foo": 1, "items": [{"bar": 2}, {"bar": 4}]}""",
					};
					HttpClient httpClient = HttpClient.CreateMock();
					httpClient.Mock.Setup
						.PostAsync(It.IsAny<Uri>(), parameter)
						.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

					HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
						new StringContent(content),
						CancellationToken.None);
					return result.StatusCode;
				}
			}

			public sealed class ArrayTests
			{
				[Test]
				[MethodDataSource(nameof(MatchingArrayValues))]
				public async Task MatchingValues_ShouldSucceed(string[] expected, string body)
				{
					HttpClient httpClient = HttpClient.CreateMock();
					httpClient.Mock.Setup
						.PostAsync(It.IsAny<Uri>(), It.IsHttpContent().WithJsonMatching(expected))
						.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

					HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
						new StringContent(body),
						CancellationToken.None);

					await That(result.StatusCode)
						.IsEqualTo(HttpStatusCode.OK);
					await That(httpClient.Mock.Verify
							.PostAsync(It.IsAny<Uri>(), It.IsHttpContent().WithJsonMatching(expected)))
						.Once();
				}

				[Test]
				[MethodDataSource(nameof(NotMatchingArrayValues))]
				public async Task NotMatchingValues_ShouldFail(string[] expected, string body)
				{
					HttpClient httpClient = HttpClient.CreateMock();
					httpClient.Mock.Setup
						.PostAsync(It.IsAny<Uri>(), It.IsHttpContent().WithJsonMatching(expected))
						.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

					HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
						new StringContent(body),
						CancellationToken.None);

					await That(result.StatusCode)
						.IsNotEqualTo(HttpStatusCode.OK);
				}

				[Test]
				public async Task WhenElementsAreInDifferentOrder_ShouldFail()
				{
					string body = "[1, 2]";
					HttpClient httpClient = HttpClient.CreateMock();
					httpClient.Mock.Setup
						.PostAsync(It.IsAny<Uri>(), It.IsHttpContent().WithJsonMatching([2, 1,]))
						.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

					HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
						new StringContent(body),
						CancellationToken.None);

					await That(result.StatusCode)
						.IsNotEqualTo(HttpStatusCode.OK);
				}

				[Test]
				public async Task WhenExpectedContainsAdditionalElements_ShouldFail()
				{
					string body = "[1, 2]";
					HttpClient httpClient = HttpClient.CreateMock();
					httpClient.Mock.Setup
						.PostAsync(It.IsAny<Uri>(), It.IsHttpContent().WithJsonMatching([1, 2, 3,]))
						.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

					HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
						new StringContent(body),
						CancellationToken.None);

					await That(result.StatusCode)
						.IsNotEqualTo(HttpStatusCode.OK);
				}

				[Test]
				public async Task WhenSubjectContainsAdditionalElements_ShouldSucceed()
				{
					string body = "[1, 2, 3]";
					HttpClient httpClient = HttpClient.CreateMock();
					httpClient.Mock.Setup
						.PostAsync(It.IsAny<Uri>(), It.IsHttpContent().WithJsonMatching([1, 2,]))
						.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

					HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
						new StringContent(body),
						CancellationToken.None);

					await That(result.StatusCode)
						.IsEqualTo(HttpStatusCode.OK);
					await That(httpClient.Mock.Verify
							.PostAsync(It.IsAny<Uri>(), It.IsHttpContent().WithJsonMatching([1, 2,])))
						.Once();
				}

				[Test]
				public async Task WhenSubjectContainsAdditionalElements_WhenNotIgnoringAdditionalArrayElements_ShouldFail()
				{
					string body = "[1, 2, 3]";
					HttpClient httpClient = HttpClient.CreateMock();
					httpClient.Mock.Setup
						.PostAsync(It.IsAny<Uri>(),
							It.IsHttpContent().WithJsonMatching([1, 2,]).IgnoringAdditionalArrayElements(false))
						.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

					HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
						new StringContent(body),
						CancellationToken.None);

					await That(result.StatusCode)
						.IsNotEqualTo(HttpStatusCode.OK);
				}

				[Test]
				[Arguments(true)]
				[Arguments(false)]
				public async Task WithOptions_ShouldApplyOptions(bool allowTrailingCommas)
				{
					string body = "[1, 2,]";
					HttpClient httpClient = HttpClient.CreateMock();
					httpClient.Mock.Setup
						.PostAsync(It.IsAny<Uri>(), It.IsHttpContent().WithJsonMatching([1, 2,], new JsonDocumentOptions
						{
							AllowTrailingCommas = allowTrailingCommas,
						}))
						.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

					HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
						new StringContent(body),
						CancellationToken.None);

					await That(result.StatusCode)
						.IsEqualTo(allowTrailingCommas ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
				}

				public static IEnumerable<Func<(string[], string)>> MatchingArrayValues()
				{
					yield return () => ([], "[]");
					yield return () => ([], "[\"foo\"]");
					yield return () => (["foo", "bar",], "[\"foo\", \"bar\"]");
				}

				public static IEnumerable<Func<(string[], string)>> NotMatchingArrayValues()
				{
					yield return () => (["foo",], "[]");
					yield return () => (["bar", "foo",], "[\"foo\", \"bar\"]");
				}
			}

			public sealed class ObjectTests
			{
				[Test]
				[Arguments("{}", false)]
				[Arguments("{\"foo\": 2}", true)]
				public async Task ShouldFailIfPropertyIsMissing(string body, bool expectSuccess)
				{
					HttpClient httpClient = HttpClient.CreateMock();
					httpClient.Mock.Setup
						.PostAsync(It.IsAny<Uri>(), It.IsHttpContent().WithJsonMatching(new
						{
							foo = 2,
						}))
						.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

					HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
						new StringContent(body),
						CancellationToken.None);

					await That(result.StatusCode)
						.IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
				}

				[Test]
				[Arguments("{}")]
				[Arguments("{\"foo\": 1}")]
				public async Task WhenExpectedIsEmpty_ShouldSucceed(string body)
				{
					HttpClient httpClient = HttpClient.CreateMock();
					httpClient.Mock.Setup
						.PostAsync(It.IsAny<Uri>(), It.IsHttpContent().WithJsonMatching(new object()))
						.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

					HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
						new StringContent(body),
						CancellationToken.None);

					await That(result.StatusCode)
						.IsEqualTo(HttpStatusCode.OK);
					await That(httpClient.Mock.Verify
							.PostAsync(It.IsAny<Uri>(), It.IsHttpContent().WithJsonMatching(new object())))
						.Once();
				}

				[Test]
				public async Task WhenPropertyHasDifferentValue_ShouldFail()
				{
					string body = "{\"bar\": 2}";
					HttpClient httpClient = HttpClient.CreateMock();
					httpClient.Mock.Setup
						.PostAsync(It.IsAny<Uri>(), It.IsHttpContent().WithJsonMatching(new
						{
							bar = 3,
						}))
						.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

					HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
						new StringContent(body),
						CancellationToken.None);

					await That(result.StatusCode)
						.IsNotEqualTo(HttpStatusCode.OK);
				}

				[Test]
				public async Task WhenSubjectHasAdditionalProperties_ShouldSucceed()
				{
					string body = "{\"foo\": null, \"bar\": 2}";
					HttpClient httpClient = HttpClient.CreateMock();
					httpClient.Mock.Setup
						.PostAsync(It.IsAny<Uri>(), It.IsHttpContent().WithJsonMatching(new
						{
							bar = 2,
						}))
						.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

					HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
						new StringContent(body),
						CancellationToken.None);

					await That(result.StatusCode)
						.IsEqualTo(HttpStatusCode.OK);
					await That(httpClient.Mock.Verify
							.PostAsync(It.IsAny<Uri>(), It.IsHttpContent().WithJsonMatching(new
							{
								bar = 2,
							})))
						.Once();
				}

				[Test]
				public async Task WhenSubjectHasAdditionalProperties_WhenNotIgnoringAdditionalProperties_ShouldFail()
				{
					string body = "{\"foo\": null, \"bar\": 2}";
					HttpClient httpClient = HttpClient.CreateMock();
					httpClient.Mock.Setup
						.PostAsync(It.IsAny<Uri>(), It.IsHttpContent().WithJsonMatching(new
						{
							bar = 2,
						}).IgnoringAdditionalProperties(false))
						.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

					HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
						new StringContent(body),
						CancellationToken.None);

					await That(result.StatusCode)
						.IsNotEqualTo(HttpStatusCode.OK);
				}

				[Test]
				[Arguments(true)]
				[Arguments(false)]
				public async Task WithOptions_ShouldApplyOptions(bool allowTrailingCommas)
				{
					string body = "{\"foo\": 1,}";
					HttpClient httpClient = HttpClient.CreateMock();
					httpClient.Mock.Setup
						.PostAsync(It.IsAny<Uri>(), It.IsHttpContent().WithJsonMatching(new
							{
								foo = 1,
							},
							new JsonDocumentOptions
							{
								AllowTrailingCommas = allowTrailingCommas,
							}))
						.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

					HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
						new StringContent(body),
						CancellationToken.None);

					await That(result.StatusCode)
						.IsEqualTo(allowTrailingCommas ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
				}
			}

			public sealed class ChainedMatcherTests
			{
				[Test]
				[Arguments("application/json", true)]
				[Arguments("text/plain", false)]
				public async Task WithMediaType_ShouldForwardToInnerParameter(string expectedMediaType,
					bool expectSuccess)
				{
					HttpClient httpClient = HttpClient.CreateMock();
					httpClient.Mock.Setup
						.PostAsync(It.IsAny<Uri>(),
							It.IsHttpContent().WithJsonMatching(new
							{
								foo = 1,
							}).WithMediaType(expectedMediaType))
						.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

					StringContent content = new("{\"foo\": 1}", System.Text.Encoding.UTF8, "application/json");
					HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
						content,
						CancellationToken.None);

					await That(result.StatusCode)
						.IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
				}

				[Test]
				[Arguments(true)]
				[Arguments(false)]
				public async Task WithString_ShouldForwardToInnerParameter(bool predicateResult)
				{
					HttpClient httpClient = HttpClient.CreateMock();
					httpClient.Mock.Setup
						.PostAsync(It.IsAny<Uri>(),
							It.IsHttpContent().WithJsonMatching(new
							{
								foo = 1,
							}).WithString(_ => predicateResult))
						.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

					HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
						new StringContent("{\"foo\": 1}"),
						CancellationToken.None);

					await That(result.StatusCode)
						.IsEqualTo(predicateResult ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
				}

				[Test]
				[Arguments(true)]
				[Arguments(false)]
				public async Task WithBytes_ShouldForwardToInnerParameter(bool predicateResult)
				{
					HttpClient httpClient = HttpClient.CreateMock();
					httpClient.Mock.Setup
						.PostAsync(It.IsAny<Uri>(),
							It.IsHttpContent().WithJsonMatching(new
							{
								foo = 1,
							}).WithBytes(_ => predicateResult))
						.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

					HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
						new StringContent("{\"foo\": 1}"),
						CancellationToken.None);

					await That(result.StatusCode)
						.IsEqualTo(predicateResult ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
				}

				[Test]
				[Arguments("expected-value", true)]
				[Arguments("other-value", false)]
				public async Task WithHeaders_ShouldForwardToInnerParameter(string requiredHeaderValue,
					bool expectSuccess)
				{
					HttpClient httpClient = HttpClient.CreateMock();
					httpClient.Mock.Setup
						.PostAsync(It.IsAny<Uri>(),
							It.IsHttpContent().WithJsonMatching(new
							{
								foo = 1,
							}).WithHeaders(("X-Custom", new HttpHeaderValue(requiredHeaderValue))))
						.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

					StringContent content = new("{\"foo\": 1}");
					content.Headers.Add("X-Custom", "expected-value");
					HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
						content,
						CancellationToken.None);

					await That(result.StatusCode)
						.IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
				}

				[Test]
				public async Task Do_ShouldForwardToInnerParameter()
				{
					int invocationCount = 0;
					HttpClient httpClient = HttpClient.CreateMock();
					httpClient.Mock.Setup
						.PostAsync(It.IsAny<Uri>(),
							It.IsHttpContent().WithJsonMatching(new
							{
								foo = 1,
							}).Do(_ => invocationCount++))
						.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

					HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
						new StringContent("{\"foo\": 1}"),
						CancellationToken.None);

					await That(result.StatusCode).IsEqualTo(HttpStatusCode.OK);
					await That(invocationCount).IsEqualTo(1);
				}
			}

			public sealed class ExactMatchWithoutIgnoringAdditionalPropertiesTests
			{
				[Test]
				public async Task WhenSubjectMatchesExpectedExactly_ShouldSucceed()
				{
					HttpClient httpClient = HttpClient.CreateMock();
					httpClient.Mock.Setup
						.PostAsync(It.IsAny<Uri>(), It.IsHttpContent().WithJsonMatching(new
						{
							foo = 1,
							bar = 2,
						}).IgnoringAdditionalProperties(false))
						.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

					HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
						new StringContent("""
						                  {
						                    "foo": 1,
						                    "bar": 2
						                  }
						                  """),
						CancellationToken.None);

					await That(result.StatusCode)
						.IsEqualTo(HttpStatusCode.OK);
				}
			}
		}
	}
}
#endif
