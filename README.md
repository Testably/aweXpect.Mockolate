# aweXpect.Mockolate

[![Nuget](https://img.shields.io/nuget/v/aweXpect.Mockolate)](https://www.nuget.org/packages/aweXpect.Mockolate)
[![Build](https://github.com/Testably/aweXpect.Mockolate/actions/workflows/build.yml/badge.svg)](https://github.com/Testably/aweXpect.Mockolate/actions/workflows/build.yml)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=Testably_aweXpect.Mockolate&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=Testably_aweXpect.Mockolate)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=Testably_aweXpect.Mockolate&metric=coverage)](https://sonarcloud.io/summary/overall?id=Testably_aweXpect.Mockolate)
[![Mutation testing badge](https://img.shields.io/endpoint?style=flat&url=https%3A%2F%2Fbadge-api.stryker-mutator.io%2Fgithub.com%2FTestably%2FaweXpect.Mockolate%2Fmain)](https://dashboard.stryker-mutator.io/reports/github.com/Testably/aweXpect.Mockolate/main)

Expectations to verify interactions with mocks from [Mockolate](https://github.com/Testably/Mockolate) for
[aweXpect](https://github.com/Testably/aweXpect).

## Overview

| Expectation                                                    | Summary                                                     |
|----------------------------------------------------------------|-------------------------------------------------------------|
| [`Once`, `Twice`, `Never`](#interaction-count)                 | the interaction happened exactly once, twice or never       |
| [`AtLeastOnce`, `AtLeastTwice`, `AtLeast`](#interaction-count) | the interaction happened at least the given number of times |
| [`AtMostOnce`, `AtMostTwice`, `AtMost`](#interaction-count)    | the interaction happened at most the given number of times  |
| [`Exactly`, `Between`](#interaction-count)                     | the interaction happened exactly or in a range of times     |
| [`Times`](#interaction-count)                                  | the number of interactions satisfies a predicate            |
| [`Then`](#interaction-order)                                   | the interactions happened in the given order                |
| [`AllInteractionsAreVerified`](#all-interactions-are-verified) | every interaction with the mock was verified                |
| [`AllSetupsAreUsed`](#all-setups-are-used)                     | every setup of the mock was used                            |

The samples use the following interface:

```csharp
public interface IPlayer
{
    bool Play(string title);
    void Pause();
    void Stop();
}
```

## Interaction count

You can verify how often a method was invoked:

```csharp
using aweXpect.Core; // for Times()

IPlayer player = IPlayer.CreateMock();
player.Play("Let It Be");

await Expect.That(player.Mock.Verify.Play("Let It Be")).Once();
await Expect.That(player.Mock.Verify.Play("Yesterday")).Never();
await Expect.That(player.Mock.Verify.Play(It.IsAny<string>())).AtLeastOnce();
await Expect.That(player.Mock.Verify.Play(It.IsAny<string>())).AtMost(3.Times());
await Expect.That(player.Mock.Verify.Play(It.IsAny<string>())).Between(1).And(3.Times());
await Expect.That(player.Mock.Verify.Play(It.IsAny<string>())).Times(n => n % 2 == 1);
```

When the track was played twice, `Once()` fails with:

```text title="Failure message"
Expected that the MusicStore.IPlayer mock
invoked method Play(Let It Be) exactly once,
but it was found twice

Matching Interactions:
[
  invoke method Play(Let It Be),
  invoke method Play(Let It Be)
]

All Interactions:
[
  invoke method Play(Let It Be),
  invoke method Play(Let It Be)
]
```

### Waiting for interactions

With `Within(TimeSpan timeout)`, you can verify that the expected number of interactions happens within the given
time. This is useful for interactions that happen asynchronously in the background.

`Within` and `WithCancellation` are available on `AtLeast…`, `Once`, `Twice`, `Exactly`, `Between` and `Times`. They
are not available on `Never` or `AtMost…`, since an upper bound cannot be confirmed by waiting longer.

```csharp
IPlayer player = IPlayer.CreateMock();

_ = Task.Run(async () =>
{
    await Task.Delay(500);
    player.Play("Hey Jude");
});

await Expect.That(player.Mock.Verify.Play("Hey Jude")).AtLeastOnce().Within(TimeSpan.FromSeconds(1));
```

When the interaction does not happen in time, e.g. a `Stop()` expected within 100 ms, the expectation fails:

```text title="Failure message"
Expected that the MusicStore.IPlayer mock
invoked method Stop() at least once within 0:00.100,
but it was never found

Matching Interactions:
[]

All Interactions:
[
  invoke method Play(Hey Jude)
]
```

Instead of a fixed time span, you can also provide a `CancellationToken` to wait for the expected interactions until
it is canceled. Like for all aweXpect expectations, a cancellation leaves the expectation inconclusive instead of
failing it, so use `Within` to limit how long the verification waits:

```csharp
await Expect.That(player.Mock.Verify.Play("Hey Jude")).AtLeastOnce().WithCancellation(token);
```

Unlike other aweXpect expectations, these verifications also wait for the interaction when only `WithTimeout` or
`WithCancellation` is given: the timeout or the token bounds how long they wait, and the verification fails when the
timeout elapses. `Within` sets an explicit wait, which is shown in the expectation; a shorter `WithTimeout` still ends
it early with "it did not finish within …".

A `Times` predicate that throws counts as not met while waiting, so the verification waits for further interactions,
and only fails with the exception when the predicate still throws at the end. A selector of `Then` that throws fails the
verification immediately.

### Waiting for an interaction not to happen

`DoesNotComplyWith` evaluates the verification inside it non-negated, so put `Within` on the verification inside it to
verify that an interaction does not happen within the given time. It waits the whole time and fails as soon as the
interaction happens:

```csharp
await Expect.That(player.Mock.Verify.Stop()).DoesNotComplyWith(it => it.AtLeastOnce().Within(TimeSpan.FromSeconds(1)));
```

`Within` on `DoesNotComplyWith` itself does not help here: it only checks again until the interaction is not found, so
it succeeds immediately when the interaction did not happen yet.

## Interaction order

You can verify that methods were invoked in a specific order:

```csharp
IPlayer player = IPlayer.CreateMock();
player.Play("Let It Be");
player.Pause();
player.Stop();

await Expect.That(player.Mock.Verify.Play("Let It Be")).Then(
    m => m.Pause(),
    m => m.Stop());
```

Other interactions may happen in between. When the player was stopped before it was paused, the expectation fails:

```text title="Failure message"
Expected that the MusicStore.IPlayer mock
invoked method Play(Let It Be), then
invoked method Pause(), then
invoked method Stop() in order,
but it invoked method Stop() too early

All Interactions:
[
  invoke method Play(Let It Be),
  invoke method Stop(),
  invoke method Pause()
]
```

## All interactions are verified

You can verify that all interactions with the mock were verified. This detects unintended or forgotten interactions:

```csharp
IPlayer player = IPlayer.CreateMock();
player.Play("Let It Be");
player.Play("Yesterday");
player.Stop();

await Expect.That(player.Mock.Verify.Play(It.IsAny<string>())).Twice();
await Expect.That(player.Mock.Verify).AllInteractionsAreVerified();
```

The verification of `Play` covers both tracks, but `Stop()` was never verified:

```text title="Failure message"
Expected that the MusicStore.IPlayer mock
has all interactions verified,
but it had 1 unverified interaction

Unverified Interactions:
[
  invoke method Stop()
]
```

## All setups are used

You can verify that all setups of the mock were used. This detects setups that are no longer needed:

```csharp
IPlayer player = IPlayer.CreateMock();
player.Mock.Setup.Play("Let It Be").Returns(true);
player.Mock.Setup.Play("Yesterday").Returns(true);

player.Play("Let It Be");

await Expect.That(player.Mock.Verify).AllSetupsAreUsed();
```

```text title="Failure message"
Expected that the MusicStore.IPlayer mock
has used all setups,
but it had 1 unused setup

Unused Setups:
[
  bool Play("Yesterday")
]
```

## HTTP content matchers

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
