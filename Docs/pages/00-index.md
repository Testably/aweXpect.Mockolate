import PackageInstall from '@site/src/components/PackageInstall';

# aweXpect.Mockolate

[aweXpect.Mockolate](https://github.com/Testably/aweXpect.Mockolate) contains expectations to verify interactions with
mocks from [Mockolate](/Mockolate). It is an extension for [aweXpect](/aweXpect).

## Installation

<PackageInstall package="aweXpect.Mockolate" />

The package adds its expectations to aweXpect, so the test project also needs the
[`aweXpect`](/aweXpect/getting-started) package.

## At a glance

```csharp
IPlayer player = IPlayer.CreateMock();
player.Play("Let It Be");
player.Stop();

// verify how often a method was invoked
await Expect.That(player.Mock.Verify.Play("Let It Be")).Once();

// verify the order of the interactions
await Expect.That(player.Mock.Verify.Play("Let It Be")).Then(m => m.Stop());

// verify that no interaction was forgotten
await Expect.That(player.Mock.Verify).AllInteractionsAreVerified();
```

A failing verification lists the matching and all interactions with the mock. If the track was played twice, `Once()`
would fail with:

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

## Overview

| Expectation                                                                                         | Summary                                                     |
|-----------------------------------------------------------------------------------------------------|-------------------------------------------------------------|
| [`Once`, `Twice`, `Never`](./01-interaction-count.md)                                               | the interaction happened exactly once, twice or never       |
| [`AtLeastOnce`, `AtLeastTwice`, `AtLeast`](./01-interaction-count.md)                               | the interaction happened at least the given number of times |
| [`AtMostOnce`, `AtMostTwice`, `AtMost`](./01-interaction-count.md)                                  | the interaction happened at most the given number of times  |
| [`Exactly`, `Between`](./01-interaction-count.md)                                                   | the interaction happened exactly or in a range of times     |
| [`Times`](./01-interaction-count.md)                                                                | the number of interactions satisfies a predicate            |
| [`Then`](./02-interaction-order.md)                                                                 | the interactions happened in the given order                |
| [`AllInteractionsAreVerified`](./03-interactions-and-setups.md#all-interactions-are-verified)       | every interaction with the mock was verified                |
| [`AllSetupsAreUsed`](./03-interactions-and-setups.md#all-setups-are-used)                           | every setup of the mock was used                            |

Verifications of a minimum number of interactions and of their order can wait for interactions that happen in the
background with `Within(timeout)`, see [Waiting for interactions](./01-interaction-count.md#waiting-for-interactions)
and [Interaction order](./02-interaction-order.md#waiting-for-interactions).

The [HTTP content matchers](./04-http-content-matchers.md) add JSON matchers for `It.IsHttpContent()` in setups and
verifications of a mocked `HttpClient`.
