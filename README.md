# aweXpect.Mockolate

[![Nuget](https://img.shields.io/nuget/v/aweXpect.Mockolate)](https://www.nuget.org/packages/aweXpect.Mockolate)
[![Build](https://github.com/Testably/aweXpect.Mockolate/actions/workflows/build.yml/badge.svg)](https://github.com/Testably/aweXpect.Mockolate/actions/workflows/build.yml)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=Testably_aweXpect.Mockolate&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=Testably_aweXpect.Mockolate)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=Testably_aweXpect.Mockolate&metric=coverage)](https://sonarcloud.io/summary/overall?id=Testably_aweXpect.Mockolate)
[![Mutation testing badge](https://img.shields.io/endpoint?style=flat&url=https%3A%2F%2Fbadge-api.stryker-mutator.io%2Fgithub.com%2FTestably%2FaweXpect.Mockolate%2Fmain)](https://dashboard.stryker-mutator.io/reports/github.com/Testably/aweXpect.Mockolate/main)

Expectations to verify interactions with mocks from [Mockolate](https://github.com/Testably/Mockolate) for
[aweXpect](https://github.com/Testably/aweXpect).

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

| Expectation                                                                                            | Summary                                                     |
|--------------------------------------------------------------------------------------------------------|-------------------------------------------------------------|
| [`Once`, `Twice`, `Never`](Docs/pages/01-interaction-count.md)                                         | the interaction happened exactly once, twice or never       |
| [`AtLeastOnce`, `AtLeastTwice`, `AtLeast`](Docs/pages/01-interaction-count.md)                         | the interaction happened at least the given number of times |
| [`AtMostOnce`, `AtMostTwice`, `AtMost`](Docs/pages/01-interaction-count.md)                            | the interaction happened at most the given number of times  |
| [`Exactly`, `Between`](Docs/pages/01-interaction-count.md)                                             | the interaction happened exactly or in a range of times     |
| [`Times`](Docs/pages/01-interaction-count.md)                                                          | the number of interactions satisfies a predicate            |
| [`Then`](Docs/pages/02-interaction-order.md)                                                           | the interactions happened in the given order                |
| [`AllInteractionsAreVerified`](Docs/pages/03-interactions-and-setups.md#all-interactions-are-verified) | every interaction with the mock was verified                |
| [`AllSetupsAreUsed`](Docs/pages/03-interactions-and-setups.md#all-setups-are-used)                     | every setup of the mock was used                            |

Verifications of a minimum number of interactions and of their order can wait for interactions that happen in the
background with `Within(timeout)`, see [Waiting for interactions](Docs/pages/01-interaction-count.md#waiting-for-interactions)
and [Interaction order](Docs/pages/02-interaction-order.md#waiting-for-interactions).

## Documentation

The full documentation is available at
[docs.testably.org](https://docs.testably.org/aweXpect/extensions/aweXpect.Mockolate/):

- [Interaction count](Docs/pages/01-interaction-count.md): how often a method was invoked, and waiting for
  interactions that happen in the background
- [Interaction order](Docs/pages/02-interaction-order.md): `Then` to verify that methods were invoked in a specific order
- [All interactions and setups](Docs/pages/03-interactions-and-setups.md): `AllInteractionsAreVerified` and
  `AllSetupsAreUsed`
- [HTTP content matchers](Docs/pages/04-http-content-matchers.md): JSON matchers for `It.IsHttpContent()` in setups and
  verifications of a mocked `HttpClient`
