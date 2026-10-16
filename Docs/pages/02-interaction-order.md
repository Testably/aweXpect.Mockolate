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
