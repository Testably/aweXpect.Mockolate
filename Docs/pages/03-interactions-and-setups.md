# All interactions and setups

The samples use the `IPlayer` interface from [Interaction count](./01-interaction-count.md).

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
