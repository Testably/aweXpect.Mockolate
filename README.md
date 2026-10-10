# aweXpect.Mockolate

[![Nuget](https://img.shields.io/nuget/v/aweXpect.Mockolate)](https://www.nuget.org/packages/aweXpect.Mockolate)
[![Build](https://github.com/Testably/aweXpect.Mockolate/actions/workflows/build.yml/badge.svg)](https://github.com/Testably/aweXpect.Mockolate/actions/workflows/build.yml)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=Testably_aweXpect.Mockolate&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=Testably_aweXpect.Mockolate)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=Testably_aweXpect.Mockolate&metric=coverage)](https://sonarcloud.io/summary/overall?id=Testably_aweXpect.Mockolate)
[![Mutation testing badge](https://img.shields.io/endpoint?style=flat&url=https%3A%2F%2Fbadge-api.stryker-mutator.io%2Fgithub.com%2FTestably%2FaweXpect.Mockolate%2Fmain)](https://dashboard.stryker-mutator.io/reports/github.com/Testably/aweXpect.Mockolate/main)

Expectations to verify interactions with mocks from [Mockolate](https://github.com/aweXpect/Mockolate).

## Features

### Interaction count

Verify that a method was called a specific number of times:

```csharp
var sut = IMyService.CreateMock();
sut.MyMethod();

await That(sut.Mock.Verify.MyMethod()).Once();                    // Exactly once
await That(sut.Mock.Verify.MyMethod()).Twice();                   // Exactly twice
await That(sut.Mock.Verify.MyMethod()).Never();                   // Never called
await That(sut.Mock.Verify.MyMethod()).AtLeastOnce();             // At least once
await That(sut.Mock.Verify.MyMethod()).AtLeastTwice();            // At least twice
await That(sut.Mock.Verify.MyMethod()).AtLeast(3.Times());        // At least 3 times
await That(sut.Mock.Verify.MyMethod()).AtMostOnce();              // At most once
await That(sut.Mock.Verify.MyMethod()).AtMostTwice();             // At most twice
await That(sut.Mock.Verify.MyMethod()).AtMost(4.Times());         // At most 4 times
await That(sut.Mock.Verify.MyMethod()).Exactly(2.Times());        // Exactly 2 times
await That(sut.Mock.Verify.MyMethod()).Between(2).And(5.Times()); // Between 2 and 5 times
```

#### Asynchronous verification

With `Within(TimeSpan timeout)`, you can check whether the expected number of calls occurred within a given time
interval. This is useful for asynchronous or delayed invocations in the background.

`Within` and `WithCancellation` are available on `AtLeast*`, `Once`, `Twice`, `Exactly`, and `Between`. They are not
available on `Never` or `AtMost*`, since an upper bound cannot be confirmed by waiting longer.

```csharp
var sut = IMyService.CreateMock();

// Start asynchronous calls, e.g., in a Task
Task.Run(async () =>
{
    await Task.Delay(500);
    sut.MyMethod();
});

// Verifies that MyMethod was called at least once within 1 second
await That(sut.Mock.Verify.MyMethod())
    .AtLeastOnce()
    .Within(TimeSpan.FromSeconds(1));
```

Instead of a fixed time span, you can also provide a `CancellationToken` to wait for the expected interactions until it
is canceled. Like for all aweXpect expectations, a cancellation leaves the expectation inconclusive instead of failing
it, so use `Within` to limit how long the verification should wait:

```csharp
// Verifies that MyMethod was called at least once, unless the token is canceled before
await That(sut.Mock.Verify.MyMethod())
    .AtLeastOnce()
    .WithCancellation(token);
```

### Interaction order

Verify that methods were called in a specific sequence:

```csharp
var sut = IMyService.CreateMock();
sut.MyMethod(1);
sut.MyMethod(2);
sut.MyMethod(3);
sut.MyMethod(4);

// Verifies MyMethod(1), then MyMethod(2), then MyMethod(4) were called in order
await That(sut.Mock.Verify.MyMethod(It.Is(1))).Then(
    m => m.MyMethod(It.Is(2)),
    m => m.MyMethod(It.Is(4))
);
```

### Additional Verifications

#### All interactions are verified

With `AllInteractionsAreVerified` you can check whether all interactions with the mock have actually been verified. This
helps to detect unintended or forgotten calls.

```csharp
var sut = IMyService.CreateMock();
sut.MyMethod(1);
sut.MyMethod(2);

await That(sut.Mock.Verify.MyMethod(It.IsAny<int>())).AtLeastOnce();
 // Succeeds, because the verification applies to both method calls.
await That(sut.Mock.Verify).AllInteractionsAreVerified();
```

#### All setups are used

With `AllSetupsAreUsed` you can check whether all defined setups on the mock have actually been used. This ensures that
no setup configurations remain unused.

```csharp
var sut = IMyService.CreateMock();
sut.Mock.Setup.MyMethod(It.Is(1)).Returns(10);
sut.Mock.Setup.MyMethod(It.Is(2)).Returns(20);

sut.MyMethod(1);

// Fails, because the setup for MyMethod(2) was never used.
await That(sut.Mock.Verify).AllSetupsAreUsed();
```

### Web Extensions

> Web extensions require .NET 8.0 or later.

#### JSON Content

You can precisely verify JSON content in HTTP requests during your tests. This feature is
especially useful for testing HTTP clients and web APIs.

```csharp
// Verifies that a request was sent with a JSON body equivalent to { "foo": 1, "bar": "baz" }
httpClient.Mock.Setup
    .PostAsync(It.IsAny<Uri>(), It.IsHttpContent().WithJsonMatching(new { foo = 1, bar = "baz" }))
    .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

// You can also provide a string representation of the JSON, and it ignores formatting differences or property order
httpClient.Mock.Setup
    .PostAsync(It.IsAny<Uri>(), It.IsHttpContent().WithJson("{\"bar\": \"baz\", \"foo\": 1}"))
    .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
```

`WithJsonMatching` serializes the expected value by reflection, which is switched off when publishing with trimming or
Native AOT enabled, so use `WithJson` with the expected JSON as a string there.

By default, additional properties in the actual JSON are ignored. Use `IgnoringAdditionalProperties(false)` to require
an exact match:

```csharp
// Fails if the request body contains any property other than `foo` and `bar`
httpClient.Mock.Setup
    .PostAsync(It.IsAny<Uri>(), It.IsHttpContent()
        .WithJsonMatching(new { foo = 1, bar = "baz" })
        .IgnoringAdditionalProperties(false))
    .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
```
