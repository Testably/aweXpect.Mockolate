# HTTP content matchers

> HTTP content matchers require .NET 8.0 or later.

This package also adds JSON matchers to Mockolate's `It.IsHttpContent()` (namespace `Mockolate.Web`). They are
argument matchers, not expectations: use them in setups or verifications of a mocked `HttpClient` to match the JSON
body of a request.

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

`WithJsonMatching` serializes the expected value by reflection, which is switched off when publishing with trimming or
Native AOT enabled, so use `WithJson` with the expected JSON as a string there.

By default, additional properties in the actual JSON are ignored. Use `IgnoringAdditionalProperties(false)` to require
an exact match:

```csharp
httpClient.Mock.Setup
    .PostAsync(It.IsAny<Uri>(), It.IsHttpContent()
        .WithJsonMatching(new { title = "Let It Be", year = 1970 })
        .IgnoringAdditionalProperties(false))
    .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Created));
```
