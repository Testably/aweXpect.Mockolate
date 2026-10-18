# aweXpect.Mockolate agent guide

aweXpect.Mockolate is a single NuGet package with [aweXpect](https://github.com/Testably/aweXpect) expectations that verify interactions with [Mockolate](https://github.com/Testably/Mockolate) mocks.

## Setup

- Use the SDK pinned in `global.json` (10.0.300, latest patch). The test runs also need the .NET 8 runtime, and `net48` tests run only on Windows.
- Package versions are managed centrally in `Directory.Packages.props`. Library settings (target frameworks, warnings as errors, trimming, packing) live in `Source/Directory.Build.props`, test settings in `Tests/Directory.Build.props`.
- The build pipeline is a Fallout project (migrated from NUKE) in `Pipeline/`, started with `./build.ps1 <Target>` (or `build.sh`, `build.cmd`).

## Design goals

Correctness is a must. The other goals are strong defaults and win in this order when they conflict: stable and compatible, AOT and trimming friendly, a consistent and pleasant API, a fast happy path, extensible, errors at compile time, documented. [Docs/agents/design-goals.md](Docs/agents/design-goals.md) explains each goal, the code rules, the checks before a commit and the goals of this repository.

## Project constraints

- The library targets `net10.0`, `net8.0` and `netstandard2.0`; the tests also run on `net48`. Neither `netstandard2.0` nor `net48` has `IsExternalInit` or `Range`, so records, `init` accessors, ranges and slice captures in list patterns do not compile there. Build `net48` first when using a newer language feature.
- Warnings are errors in the library, except `NU5104` and missing XML docs (`CS1591`). Still document every public member, and mark every public expectation method with `[GuaranteesNotNull]` like the existing ones.
- Expectations live in the `aweXpect` namespace as extension methods on `IThat<VerificationResult<TVerify>>` (`ThatVerificationResult`) and `IThat<IMockVerify<TVerify>>` (`ThatMockVerify`). The HTTP content matchers live in `Mockolate.Web` and exist only for `net8.0` and newer.
- Failure messages follow the aweXpect conventions (`Expected that <subject>`, the expectation, then `but ...`, followed by the relevant interactions); match an existing expectation.
- Keep the package trimmable and Native AOT compatible. Reflection is allowed only behind the `ReflectionFallback` feature guard of aweXpect.Core. A new or changed expectation or matcher needs a passing and a failing check in the AOT smoke tests.
- The versions of `aweXpect.Core` and `Mockolate` in `Directory.Packages.props` become the minimum dependency versions of the package. Raise them only when the library needs it; `aweXpect` and `aweXpect.Chronology` are used by the tests only.
- The assembly is strong-name signed with `strongname.snk`; test projects are not signed.

## Validation

- Run the affected tests in `Tests/aweXpect.Mockolate.Tests` for every target framework you can, at least `net10.0` and `net48` on Windows.
- A public API change fails `Tests/aweXpect.Mockolate.Api.Tests`. Accept intended changes by running it with `AWEXPECT_ACCEPT_SNAPSHOTS=all` (or the explicit `AcceptApiChanges` test) and commit the updated `*.awexpect-snapshot.txt` files for all three target frameworks.
- Behaviour that could differ when trimmed belongs in `Tests/aweXpect.Mockolate.Aot` and is verified with `./build.ps1 AotSmokeTests --configuration Release`.
- Pin failure messages exactly with `.WithMessage(...)` instead of asserting only the exception type.
- Files are checked out with CRLF (`.editorconfig`, `* text=auto`), and tests hold multi-line failure messages in raw string literals. Edit with tools that keep the line endings instead of `sed -i`, which can leave mixed endings.
- Some files start with a UTF-8 BOM. Keep it as it is: PowerShell rewrites strip it, so check `git diff` for a changed first line (U+FEFF).
- No em-dashes in `Docs/`, the README or XML docs.

## Git workflow

- Work on a feature branch and open a pull request against `main`. CI runs on every pull request.
- Commit subjects follow conventional commits (`feat:`, `fix:`, `refactor:`, `test:`, `docs:`, `chore:`, `ci:`). The subject is the PR title, the body is the PR description and explains why, with each paragraph on a single line. No co-author or attribution trailers.
- Dependabot pull requests (prefix `chore`) are merged automatically by Mergify once the unit and API tests pass.
- Pushing a tag `vX.Y.Z` (optionally with a pre-release suffix) makes the Build workflow push the package to nuget.org; only tags without a suffix get a GitHub release.

## References

- [Docs/agents/design-goals.md](Docs/agents/design-goals.md): the goals and code rules for every change, the checks before a commit and the goals of this repository.
- [Docs/agents/workflows.md](Docs/agents/workflows.md): build, test, benchmark, mutation and documentation commands, pipeline targets and CI workflows.
- [Docs/agents/project-structure.md](Docs/agents/project-structure.md): architecture and source locations.
- [CONTRIBUTING.md](CONTRIBUTING.md): pull request expectations and the SonarCloud quality gate.
