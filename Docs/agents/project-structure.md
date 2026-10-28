# Project structure

## Layout

| Path | Content |
|------|---------|
| `Source/aweXpect.Mockolate/` | The library and the only shipped package |
| `Tests/aweXpect.Mockolate.Tests/` | Unit tests (TUnit, `net10.0`, `net8.0`, `net48`) |
| `Tests/aweXpect.Mockolate.Api.Tests/` | Public API snapshot tests (TUnit, `net8.0`, covers all library target frameworks) |
| `Tests/aweXpect.Mockolate.Aot/` | Native AOT smoke tests as a console application (`net10.0`) |
| `Benchmarks/aweXpect.Mockolate.Benchmarks/` | BenchmarkDotNet benchmarks (`net10.0`) |
| `Pipeline/` | Fallout build project; one `Build.<Area>.cs` file per group of targets |
| `Docs/pages/` | User documentation published to docs.testably.org |
| `Docs/agents/` | Guides for authors and agents, not published |
| `.github/workflows/` | CI, Build, CI-Analysis and documentation site workflows |

`Source/Directory.Build.props` and `Tests/Directory.Build.props` import the root `Directory.Build.props` (strong naming). `Tests/aweXpect.Mockolate.Aot/Directory.Build.props` replaces the test settings, because the smoke tests are no test project. `Pipeline/` has its own empty `Directory.Build.*` files so that it does not inherit the repository settings.

## Library

The library depends on `aweXpect.Core` (not `aweXpect`) and `Mockolate`. Its root namespace is `aweXpect`.

- `ThatVerificationResult.cs` holds the shared count logic: `VerificationCountConstraint<TVerify>` evaluates a Mockolate `VerificationResult<TVerify>`, waits for an awaitable result or a `Within` timeout, and adds the "Matching Interactions" and "All Interactions" contexts to a failure. `HasExactlyConstraint`, `HasAtMostConstraint` and `HasAtLeastConstraint` derive from it.
- `ThatVerificationResult.<Expectation>.cs` contains one public expectation each (`Once`, `Twice`, `Never`, `AtLeast*`, `AtMost*`, `Exactly`, `Between`, `Times`, `Then`). An expectation adds its constraint with `subject.Get().ExpectationBuilder.AddConstraint(...)`, passing the `OptionsHolder<WithinOptions>` as state to a static lambda, and returns an `AndOrWithinResult` when it supports `Within`. Constraints specific to one expectation are nested private classes in its file.
- `ThatMockVerify.<Expectation>.cs` contains the expectations on `IMockVerify<TVerify>` (`AllInteractionsAreVerified`, `AllSetupsAreUsed`).
- `Results/AndOrWithinResult.cs` (`aweXpect.Mockolate.Results`) and `Options/WithinOptions.cs` (`aweXpect.Mockolate.Options`) add `.Within(timeout)` to count expectations and `Then`. `WithinOptions` is an immutable value in an `OptionsHolder<WithinOptions>`, which the expectation shares with its constraint and its result: `Within` replaces the value, and the constraint reads it when it evaluates.
- `Web/ItExtensions.HttpContent.IsJsonContent.cs` adds `WithJson` and `WithJsonMatching` to `It.IsHttpContent()` with C# extension members in the `Mockolate.Web` namespace, only for `net8.0` and newer.
- `Helpers/` contains `MyDescribableSubject<T>`, which names the mock in failure messages, and polyfills for `netstandard2.0`.
- `Usings.cs` imports `aweXpect.Formatting` globally.

Argument errors are thrown through `Tracing.WriteException(...)` from aweXpect.Core. Add a new expectation as a new `ThatVerificationResult.<Name>.cs` or `ThatMockVerify.<Name>.cs` file, with tests in the matching `ThatVerificationResultIs.<Name>Tests.cs` or `ThatMockVerifyIs.<Name>Tests.cs`, checks in `Tests/aweXpect.Mockolate.Aot/Checks.cs`, an updated API snapshot and documentation in `Docs/pages` and the README overview.

## Tests

- `Tests/aweXpect.Mockolate.Tests` mirrors the library: `ThatVerificationResultIs` and `ThatMockVerifyIs` are partial classes that define the mocked interfaces, with one nested test class per expectation in its own file. `Web/` tests the HTTP content matchers.
- `Tests/aweXpect.Mockolate.Api.Tests` locates the solution by `aweXpect.Mockolate.slnx`, reads the target frameworks from `Source/Directory.Build.props` and compares each build with its snapshot file.
- `Tests/aweXpect.Mockolate.Aot` mocks `IGreeter` and `HttpClient` and runs every check both under the JIT and published with Native AOT.
