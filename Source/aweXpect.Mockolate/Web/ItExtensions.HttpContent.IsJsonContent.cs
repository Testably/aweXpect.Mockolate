#if NET8_0_OR_GREATER
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Mockolate.Parameters;

// ReSharper disable once CheckNamespace
namespace Mockolate.Web;

#pragma warning disable S2325 // Methods and properties that don't access instance data should be static
/// <summary>
///     Extensions for parameter matchers for HTTP-related types.
/// </summary>
public static class AweXpectItExtensions
{
	/// <inheritdoc cref="ItExtensions" />
	extension(ItExtensions.IHttpContentParameter parameter)
	{
		/// <summary>
		///     Expects the <see cref="HttpContent" /> to have a body equal to the given <paramref name="json" />.
		/// </summary>
		public IJsonContentBodyParameter WithJson(string json, JsonDocumentOptions? options = null)
		{
			JsonContentParameter jsonContentParameter = new(parameter);
			jsonContentParameter.WithBody(json, options);
			parameter.WithString(b => jsonContentParameter.Matches(b));
			return jsonContentParameter;
		}

		/// <summary>
		///     Expects the <see cref="HttpContent" /> to have a JSON body which matches the <paramref name="expected" /> value.
		/// </summary>
		public IJsonContentBodyParameter WithJsonMatching(object? expected, JsonDocumentOptions? options = null)
		{
			JsonContentParameter jsonContentParameter = new(parameter);
			jsonContentParameter.WithBodyMatching(expected, options);
			parameter.WithString(b => jsonContentParameter.Matches(b));
			return jsonContentParameter;
		}

		/// <summary>
		///     Expects the <see cref="HttpContent" /> to have a JSON body which matches the <paramref name="expected" /> value.
		/// </summary>
		public IJsonContentBodyParameter WithJsonMatching<T>(IEnumerable<T> expected,
			JsonDocumentOptions? options = null)
		{
			JsonContentParameter jsonContentParameter = new(parameter);
			jsonContentParameter.WithBodyMatching(expected, options);
			parameter.WithString(b => jsonContentParameter.Matches(b));
			return jsonContentParameter;
		}
	}

	/// <summary>
	///     Further expectations on the matching of a JSON body of the <see cref="HttpContent" />.
	/// </summary>
	public interface IJsonContentBodyParameter : ItExtensions.IHttpContentParameter
	{
		/// <summary>
		///     Ignores additional properties in JSON objects when comparing.
		/// </summary>
		IJsonContentBodyParameter IgnoringAdditionalProperties(bool ignoreAdditionalProperties = true);
	}

	private sealed class JsonContentParameter : IJsonContentBodyParameter
	{
		private readonly ItExtensions.IHttpContentParameter _parameter;

		private string? _body;
		private bool _ignoringAdditionalProperties = true;
		private JsonDocumentOptions? _jsonDocumentOptions;

		public JsonContentParameter(ItExtensions.IHttpContentParameter parameter)
		{
			_parameter = parameter;
		}

		/// <inheritdoc cref="IJsonContentBodyParameter.IgnoringAdditionalProperties(bool)" />
		public IJsonContentBodyParameter IgnoringAdditionalProperties(bool ignoreAdditionalProperties = true)
		{
			_ignoringAdditionalProperties = ignoreAdditionalProperties;
			return this;
		}

		public IParameterWithCallback<HttpContent?> Do(Action<HttpContent?> callback)
			=> _parameter.Do(callback);

		public ItExtensions.IHttpContentHeaderParameter WithHeaders(
			params IEnumerable<(string Name, HttpHeaderValue Value)> headers)
			=> _parameter.WithHeaders(headers);

		public ItExtensions.IHttpContentParameter WithString(Func<string, bool> predicate,
			[CallerArgumentExpression(nameof(predicate))] string doNotPopulateThisValue = "")
			=> _parameter.WithString(predicate, doNotPopulateThisValue);

		public ItExtensions.IHttpContentParameter WithBytes(Func<byte[], bool> predicate,
			[CallerArgumentExpression(nameof(predicate))] string doNotPopulateThisValue = "")
			=> _parameter.WithBytes(predicate, doNotPopulateThisValue);

		public ItExtensions.IHttpContentParameter WithMediaType(string? mediaType)
			=> _parameter.WithMediaType(mediaType);

		public void InvokeCallbacks(object? value)
			=> ((IParameter)_parameter).InvokeCallbacks(value);

		public bool Matches(object? value)
			=> ((IParameter)_parameter).Matches(value);

		public bool Matches(string value)
		{
			if (_body is not null)
			{
				try
				{
					JsonDocumentOptions options = _jsonDocumentOptions ?? GetDefaultOptions();
					using JsonDocument actualDocument = JsonDocument.Parse(value, options);
					using JsonDocument expectedDocument = JsonDocument.Parse(_body, options);

					if (!Compare(actualDocument.RootElement, expectedDocument.RootElement,
						    _ignoringAdditionalProperties))
					{
						return false;
					}
				}
				catch (JsonException)
				{
					return false;
				}
			}

			return true;
		}

		public void WithBody(string json,
			JsonDocumentOptions? options = null)
		{
			_body = json;
			_jsonDocumentOptions = options;
		}

		public void WithBodyMatching(object? expected,
			JsonDocumentOptions? options = null)
			=> WithBody(JsonSerializer.Serialize(expected, JsonSerializerOptions.Default), options);

		public void WithBodyMatching<T>(IEnumerable<T> expected,
			JsonDocumentOptions? options = null)
			=> WithBody(JsonSerializer.Serialize<object>(expected, JsonSerializerOptions.Default), options);

		private static JsonDocumentOptions GetDefaultOptions() => new()
		{
			AllowTrailingCommas = true,
		};

		private static bool Compare(
			JsonElement actualElement,
			JsonElement expectedElement,
			bool ignoreAdditionalProperties)
		{
			if (actualElement.ValueKind != expectedElement.ValueKind)
			{
				return false;
			}

			return actualElement.ValueKind switch
			{
				JsonValueKind.Array => CompareJsonArray(actualElement, expectedElement, ignoreAdditionalProperties),
				JsonValueKind.Number => CompareJsonNumber(actualElement, expectedElement),
				JsonValueKind.String => CompareJsonString(actualElement, expectedElement),
				JsonValueKind.Object => CompareJsonObject(actualElement, expectedElement, ignoreAdditionalProperties),
				_ => true,
			};
		}

		private static bool CompareJsonObject(JsonElement actualElement, JsonElement expectedElement,
			bool ignoreAdditionalProperties)
		{
			foreach (JsonProperty item in expectedElement.EnumerateObject())
			{
				if (!actualElement.TryGetProperty(item.Name, out JsonElement property))
				{
					return false;
				}

				if (!Compare(property, item.Value, ignoreAdditionalProperties))
				{
					return false;
				}
			}

			if (!ignoreAdditionalProperties)
			{
				foreach (JsonProperty property in actualElement.EnumerateObject())
				{
					if (!expectedElement.TryGetProperty(property.Name, out _))
					{
						return false;
					}
				}
			}

			return true;
		}

		private static bool CompareJsonArray(JsonElement actualElement, JsonElement expectedElement,
			bool ignoreAdditionalProperties)
		{
			for (int index = 0; index < expectedElement.GetArrayLength(); index++)
			{
				JsonElement expectedArrayElement = expectedElement[index];
				if (actualElement.GetArrayLength() <= index)
				{
					return false;
				}

				JsonElement actualArrayElement = actualElement[index];
				if (!Compare(actualArrayElement, expectedArrayElement, ignoreAdditionalProperties))
				{
					return false;
				}
			}

			return ignoreAdditionalProperties || actualElement.GetArrayLength() <= expectedElement.GetArrayLength();
		}

		private static bool CompareJsonString(JsonElement actualElement, JsonElement expectedElement)
		{
			string? value1 = actualElement.GetString();
			string? value2 = expectedElement.GetString();
			return value1 == value2;
		}

		private static bool CompareJsonNumber(JsonElement actualElement, JsonElement expectedElement)
		{
			if (actualElement.TryGetInt32(out int v1) && expectedElement.TryGetInt32(out int v2))
			{
				return v1 == v2;
			}

			if (actualElement.TryGetDouble(out double n1) && expectedElement.TryGetDouble(out double n2))
			{
				return n1.Equals(n2);
			}

			return false;
		}
	}
}
#pragma warning restore S2325 // Methods and properties that don't access instance data should be static
#endif
