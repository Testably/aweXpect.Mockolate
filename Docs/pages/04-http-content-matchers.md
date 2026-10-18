# HTTP content matchers

Describes the JSON matchers that this package adds to Mockolate's `It.IsHttpContent()`.

| Matcher                                                                  | Summary                                                        |
|--------------------------------------------------------------------------|----------------------------------------------------------------|
| [`WithJsonMatching`](#json-body)                                         | the JSON body matches the expected object                      |
| [`WithJson`](#json-body)                                                 | the JSON body is equivalent to the expected JSON string        |
| [`IgnoringAdditionalProperties`](#additional-properties-and-elements)    | whether additional properties in the JSON body are ignored     |
| [`IgnoringAdditionalArrayElements`](#additional-properties-and-elements) | whether additional array elements in the JSON body are ignored |

:::note[.NET 8 or later]
The HTTP content matchers require .NET 8 or later.
:::

The matchers are in the namespace `Mockolate.Web`. They are argument matchers, not expectations: use them in setups or
verifications of a mocked `HttpClient` to match the JSON body of a request.

## JSON body

`WithJsonMatching` compares the body with an object, and `WithJson` with a JSON string, ignoring differences in
formatting and property order:

```csharp
HttpClient httpClient = HttpClient.CreateMock();
httpClient.Mock.Setup
    .PostAsync(It.IsAny<Uri>(), It.IsHttpContent().WithJsonMatching(new { title = "Let It Be", year = 1970 }))
    .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Created));

httpClient.Mock.Setup
    .PostAsync(It.IsAny<Uri>(), It.IsHttpContent().WithJson("{\"year\": 1965, \"title\": \"Yesterday\"}"))
    .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Created));
```

:::note[Trimming and Native AOT]
`WithJsonMatching` serializes the expected value by reflection, which is switched off when publishing with trimming or
Native AOT enabled, so use `WithJson` with the expected JSON as a string there.
:::

## Additional properties and elements

`WithJson` requires the body to be equivalent to the expected JSON: additional properties of objects and additional
elements at the end of arrays do not match. `WithJsonMatching` ignores both by default.

Change this on either matcher with `IgnoringAdditionalProperties` and `IgnoringAdditionalArrayElements`:

```csharp
httpClient.Mock.Setup
    .PostAsync(It.IsAny<Uri>(), It.IsHttpContent()
        .WithJson("{\"title\": \"Let It Be\", \"tracks\": [\"Two of Us\"]}")
        .IgnoringAdditionalProperties()
        .IgnoringAdditionalArrayElements())
    .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Created));

httpClient.Mock.Setup
    .PostAsync(It.IsAny<Uri>(), It.IsHttpContent()
        .WithJsonMatching(new { title = "Let It Be", year = 1970 })
        .IgnoringAdditionalProperties(false))
    .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Created));
```

:::warning[No match returns the default response]
When the body does not match, the setup is not used and the mocked `HttpClient` returns its default response instead
of failing. Earlier versions of `WithJson` ignored additional properties and array elements: to keep that behaviour,
add `.IgnoringAdditionalProperties().IgnoringAdditionalArrayElements()` or pass the expected object to
`WithJsonMatching`.
:::
