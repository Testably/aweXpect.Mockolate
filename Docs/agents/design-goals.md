# Design goals

These goals apply to every Testably repository. They guide agents and contributors when a change has more than one reasonable solution. The goals of this repository at the end refine them.

Correctness is a hard requirement. Every other goal is a strong default: follow it, and when a change has to break one, say so in the pull request and explain why.

## Decision test

Before proposing a change, ask: does it keep the library correct, and does it make it faster, more consistent, easier to extend or more pleasant to use? If the answer is no, reconsider the change.

## Goals

When goals conflict, the earlier one wins.

1. **Correct, with clear failures (must).** Behaviour is exact and covered by tests. A failure states what was expected and what was found, and tests pin the complete message.
2. **Stable and compatible.** Follow semantic versioning: breaking changes only in a major version, on purpose and documented. Public API changes show up in the API snapshots and are reviewed. Keep dependencies minimal and their version floors low, and support the declared target frameworks.
3. **AOT and trimming friendly.** No unguarded reflection: prefer source generators, otherwise put reflection behind a feature guard with trimming annotations. This covers member reflection (`GetProperties`, `GetMethods`, `GetCustomAttributes`, reading or invoking a member) and `MakeGenericType` or `MakeGenericMethod`. Queries on the shape of a type that the code already holds need no guard: `IsGenericType`, `GetGenericTypeDefinition` and `GetGenericArguments` are trimming-safe. `GetInterfaces` is not exact, as the trimmer or the AOT compiler can remove the implementation of an interface that the program never uses, so call it unguarded only where a missed interface degrades to the behaviour for a type without it, like a dictionary that is formatted as a sequence of pairs, and say so in its suppression, as `FindGenericInterface` in `aweXpect.Core` does; a new suppression needs the maintainer's approval. When the interface is what an expectation checks, guard the lookup; when a missed interface would silently change a comparison, root the interfaces, like the generated registrations do for equivalency. A change that touches reflection runs the AOT smoke tests.
4. **A consistent, discoverable and pleasant API.** Extension methods exist only on the types they apply to, so IntelliSense shows what makes sense. Naming, option chaining and null handling follow the existing patterns (see the repository-specific rules). Every public member has XML documentation. Prefer minimal ceremony for the user: sensible defaults, no boilerplate, failure messages that read well.
5. **Fast, especially on the happy path.** Succeeding calls are the common case. Avoid allocations in hot paths, and back every performance claim or change with a benchmark before and after.
6. **Extensible.** Third parties build on the same extension points the library uses itself. New features use the public extension surface instead of internals, and the core stays small and stable.
7. **Errors at compile time rather than at run time.** Prefer analyzers, source generators and types that make misuse a compiler error over runtime checks.
8. **Documented and pleasant to read.** A public feature is not done until it is documented on the docs site, with examples that are tested where the repository can test them. The docs are rendered with Docusaurus and follow the [docs style guide](https://github.com/Testably/Testably.Site/blob/main/Docs/style-guide.md): use Docusaurus features where they help the reader, such as admonitions with the meaning the guide gives each type, tabs for alternatives (for example synchronous and asynchronous usage, or one tab per test framework) and collapsible `<details>` for long output and background explanations. Examples follow the music theme of the aweXpect documentation (albums, tracks, artists, playlists) instead of placeholder names like `Foo` or `Bar`.

## Code

- **Modern C#.** Use current language features and APIs where the target frameworks allow them, with polyfills for netstandard2.0 and net48 instead of avoiding a feature. Check that the oldest target framework still compiles.
- **Async.**
  - Use `ValueTask` for operations on the happy path that often complete synchronously, like the evaluation of a constraint. An API that only runs while a failure is built, like `ResultContext.GetContent` or the content processors of `aweXpect.Web`, may return `Task`, as it does not run when the expectation succeeds.
  - Pass the `CancellationToken` through every async path.
  - Do not use `ConfigureAwait(false)` in library code or in generated code. Expectations run user code after their own awaits (predicates, mappings, async delegates and enumerables, custom comparers and formatters), and this code must run on the synchronization context of the caller, for example on the UI thread of a WPF or WinForms test. A synchronous API boundary that blocks detaches from the context of the caller instead, as `Synchronously` does. The analyzers and code fixers run in Roslyn and not in a test, so they keep `ConfigureAwait(false)`.
  - Do not block on async code (`.Result`, `.Wait()`, `.GetAwaiter().GetResult()`), except at a deliberate synchronous API boundary, and complete synchronously there where possible.
  - Do not use `Task.Run` to make code look asynchronous; use it only where the real counterpart does work in the background.
- **Avoid.**
  - Catching exceptions without handling or rethrowing them.
  - LINQ, closures and boxing in hot paths; use loops and cached delegates there.
  - String concatenation in loops; use a `StringBuilder`.

## Before you commit

Run the checks that match the change:

- **Any code change:** the tests of the affected projects, on the oldest and the newest target framework.
- **Public API:** the API snapshot tests; review and accept intentional changes, never commit pending snapshots.
- **Reflection, trimming or AOT:** the AOT smoke tests.
- **Hot path or a performance claim:** a benchmark before and after.
- **New public feature:** documentation and its examples.
- **Failure messages:** tests that pin the complete message.
- **Static analysis:** no new Sonar or analyzer findings. Avoid the common ones: high cognitive complexity, duplicated string literals, unused members and parameters, methods that could be static, long parameter lists. Do not suppress a finding without asking the maintainer first.

## Reviews

Verify every review finding against the code before changing anything. If you disagree, push back once with concrete reasoning; if the reviewer or the maintainer confirms the finding, implement it.

## Goals of this repository

aweXpect.Mockolate is an extension package of aweXpect, so the [goals of the extension packages](https://github.com/Testably/aweXpect/blob/main/Docs/agents/design-goals.md#extension-packages) apply, together with the goals of aweXpect and aweXpect.Core that they refer to. The goals below refine them.

1. **Never contradicts Mockolate.** Use Mockolate's own vocabulary where it exists, with the same counts, matching and ordering semantics.
2. **Feels like core aweXpect.** Options, count types, argument checks and messages follow the core conventions.
3. **Fast happy path.** No closures in constraints and no eager formatting.

The HTTP content matchers (`WithJson` and `WithJsonMatching`) are an accepted exception to rule 6 of the extension packages: they are not expectations but Mockolate parameter matchers, which extend `It.IsHttpContent()` with C# extension members, so they live in `Mockolate.Web`, the namespace of the parameter they extend, where users find the other HTTP matchers of Mockolate without an extra `using`.
