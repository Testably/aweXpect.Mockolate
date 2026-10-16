# Interaction count

The samples use the following interface:

```csharp
public interface IPlayer
{
    bool Play(string title);
    void Pause();
    void Stop();
}
```

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

## Waiting for interactions

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
and only fails with the exception when the predicate still throws at the end. A selector of
[`Then`](./02-interaction-order.md) that throws fails the verification immediately.

## Waiting for an interaction not to happen

`DoesNotComplyWith` evaluates the verification inside it non-negated, so put `Within` on the verification inside it to
verify that an interaction does not happen within the given time. It waits the whole time and fails as soon as the
interaction happens:

```csharp
await Expect.That(player.Mock.Verify.Stop()).DoesNotComplyWith(it => it.AtLeastOnce().Within(TimeSpan.FromSeconds(1)));
```

`Within` on `DoesNotComplyWith` itself does not help here: it only checks again until the interaction is not found, so
it succeeds immediately when the interaction did not happen yet.
