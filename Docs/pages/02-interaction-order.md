---
description: Verify with Then that the interactions with a Mockolate mock happened in the given order, also while waiting for them.
---

# Interaction order

Describes the possible expectations for verifying the order of interactions with a mock.

| Expectation      | Negated | Summary                                      |
|------------------|---------|----------------------------------------------|
| [`Then`](#order) |         | the interactions happened in the given order |

The samples use the `IPlayer` interface from [Interaction count](./01-interaction-count.md).

## Order

You can verify that methods were invoked in a specific order:

```csharp
IPlayer player = IPlayer.CreateMock();
player.Play("Let It Be");
player.Pause();
player.Stop();

await That(player.Mock.Verify.Play("Let It Be")).Then(
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

## Waiting for interactions

With `Within(TimeSpan timeout)`, the verification waits until the whole sequence happened, e.g. when the interactions
happen asynchronously in the background. Interactions that are recorded while waiting count for every step, so a
`Stop()` after the `Pause()` also satisfies a sequence whose first `Stop()` came too early.

```csharp
IPlayer player = IPlayer.CreateMock();

_ = Task.Run(async () =>
{
    player.Play("Let It Be");
    await Task.Delay(50);
    player.Pause();
    player.Stop();
});

await That(player.Mock.Verify.Play("Let It Be"))
    .Then(m => m.Pause(), m => m.Stop())
    .Within(TimeSpan.FromSeconds(1));
```

When the sequence is not complete in time, the expectation fails:

```text title="Failure message"
Expected that the MusicStore.IPlayer mock
invoked method Play(Let It Be), then
invoked method Pause(), then
invoked method Stop() in order within 0:01,
but it invoked method Stop() not at all

All Interactions:
[
  invoke method Play(Let It Be),
  invoke method Pause()
]
```

The timeout is a budget for the whole sequence, counted from the start of the verification:

- A step with a timeout of its own, e.g. `m => m.Stop().Within(TimeSpan.FromSeconds(5))`, waits up to that timeout,
  regardless of the budget.
- Every other step only waits for what is left of the budget. When nothing is left, e.g. because a step with its own
  timeout took longer, it is checked once without waiting.
- Without any timeout, the sequence is checked once.

Like for the [interaction count](./01-interaction-count.md#waiting-for-interactions), `WithTimeout` without `Within` also sets the budget, and `WithCancellation` waits until the token is canceled, which leaves the expectation [inconclusive](/aweXpect/how-it-works/time-and-cancellation#outcome).
