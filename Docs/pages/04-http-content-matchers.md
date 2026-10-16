# HTTP content matchers

Describes the JSON matchers that this package adds to Mockolate's `It.IsHttpContent()`.

| Matcher                                                  | Summary                                                    |
|----------------------------------------------------------|------------------------------------------------------------|
| [`WithJsonMatching`](#json-body)                         | the JSON body matches the expected object                  |
| [`WithJson`](#json-body)                                 | the JSON body matches the expected JSON string             |
| [`IgnoringAdditionalProperties`](#additional-properties) | whether additional properties in the JSON body are ignored |

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

## Additional properties

By default, additional properties in the actual JSON are ignored. Use `IgnoringAdditionalProperties(false)` to require
an exact match:

```csharp
httpClient.Mock.Setup
    .PostAsync(It.IsAny<Uri>(), It.IsHttpContent()
        .WithJsonMatching(new { title = "Let It Be", year = 1970 })
        .IgnoringAdditionalProperties(false))
    .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Created));
```
