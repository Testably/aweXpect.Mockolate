# Workflows

Commands for building, testing and releasing aweXpect.Mockolate. Run them from the repository root.

## Build

```powershell
dotnet build aweXpect.Mockolate.slnx
```

The solution contains the library, the tests, the AOT smoke tests, the benchmarks and the `Pipeline/Build.csproj` build project. Every build of the library also creates a NuGet package (`GeneratePackageOnBuild`), and the snapshot tests read the library builds of all target frameworks from `Source/aweXpect.Mockolate/bin/<Configuration>/<tfm>/`.

## Tests

```powershell
dotnet test
dotnet test --project Tests/aweXpect.Mockolate.Tests --framework net10.0
dotnet test --project Tests/aweXpect.Mockolate.Tests --framework net10.0 --treenode-filter "/*/*/*OnceTests/*"
dotnet test --project Tests/aweXpect.Mockolate.Tests --framework net48
```

- All tests use TUnit and are executables of the Microsoft.Testing.Platform, which `global.json` selects for `dotnet test`. `--treenode-filter "/<assembly>/<namespace>/<class>/<test>"` selects tests, with `*` as wildcard. A single project can also be started with `dotnet run --project <project> -f <framework>`.
- `Tests/aweXpect.Mockolate.Tests` targets `net10.0`, `net8.0` and `net48`. `net48` runs only on Windows; the pipeline skips it elsewhere.
- Test classes are nested by subject and expectation (`ThatVerificationResultIs.OnceTests`) and mostly named `When..._Should...`. Failure messages are pinned with `Throws<FailException>().WithMessage("""...""")`; a canceled verification throws TUnit's `InconclusiveTestException`.
- Tests that wait for background interactions use `Within(...)` with generous timeouts and `aweXpect.Chronology` helpers such as `30.Seconds()`.

## Public API

```powershell
dotnet test --project Tests/aweXpect.Mockolate.Api.Tests
dotnet test --project Tests/aweXpect.Mockolate.Api.Tests --treenode-filter "/*/*/ApiApprovalTests/AcceptApiChanges"
$env:AWEXPECT_ACCEPT_SNAPSHOTS = 'all'; dotnet test --project Tests/aweXpect.Mockolate.Api.Tests; Remove-Item Env:AWEXPECT_ACCEPT_SNAPSHOTS
```

- The tests use `aweXpect.Snapshots.PublicApi` to compare the public API of every target framework listed in `Source/Directory.Build.props` with `ApiApprovalTests.aweXpect%2EMockolate.<tfm>.awexpect-snapshot.txt`.
- Building the test project also builds every target framework of the library (`BuildAllTargetFrameworksOfReferencedProjects`), so the snapshots stay in sync with the configuration (Debug or Release) of the test run.
- Accept an intended change with the environment variable above, by running the explicit `AcceptApiChanges` test (only with the filter above; a run without it skips the test), or by renaming the pending snapshot next to the test file. Review the snapshot diff before committing it.

## AOT smoke tests

```powershell
dotnet run --project Tests/aweXpect.Mockolate.Aot
./build.ps1 AotSmokeTests --configuration Release
```

- `Tests/aweXpect.Mockolate.Aot` is a console application, not a test project. Each entry in `Checks.cs` passes or expects a failure message, and `Program.cs` returns a non-zero exit code when a check fails.
- The pipeline target runs the program under the JIT, then publishes it with Native AOT for the current runtime and runs it again. Trimmer and AOT compiler warnings are errors in this project.
- Publishing needs the native toolchain of the platform (on Windows the Visual Studio C++ build tools). Without it, `dotnet run` still covers the JIT half.
- `WithJsonMatching` serializes by reflection, so its checks expect either success or the error naming the `aweXpect.ReflectionFallback.IsSupported` switch.

## Pipeline targets

`./build.ps1 --help` lists the targets. The pipeline defaults to Debug locally and Release on CI. On Windows, pass `--configuration Release` to targets that compile: the Debug solution build includes `Pipeline/Build.csproj` and fails to overwrite the running `Pipeline/bin/Debug/Build.exe`.

| Target | Purpose |
|--------|---------|
| `Compile` | Cleans `Artifacts/` and `TestResults/`, restores and builds the solution with the GitVersion version |
| `UnitTests` | Runs `aweXpect.Mockolate.Tests` with `dotnet run` for every target framework (`net48` only on Windows) and writes `TestResults/<project>_<tfm>.trx` and `.cobertura.xml` |
| `CodeCoverage` | `UnitTests` plus a SonarQube coverage report in `TestResults/reports` |
| `ApiChecks` | Runs `aweXpect.Mockolate.Api.Tests` and writes `TestResults/aweXpect.Mockolate.Api.Tests.trx` |
| `AotSmokeTests` | Runs the smoke tests under the JIT and published with Native AOT |
| `MutationTests` | Runs Stryker (the version of the `PackageDownload` in `Pipeline/Build.csproj`, `--test-runner mtp`) on the library with the unit and API tests (`net8.0`), only on changes since `main` outside `main` |
| `MutationTestsDashboard` | Uploads the Stryker report to the dashboard (CI only) |
| `MutationTestsComment` | Comments the mutation results on the pull request (CI only) |
| `Benchmarks` | Builds and runs the benchmarks in Release and logs the GitHub report |
| `BenchmarkComment` | Comments the benchmark results on the pull request (CI only) |
| `CodeAnalysis` | SonarCloud analysis around `Compile` and `CodeCoverage` (needs `SonarToken`) |
| `Pack` | Writes a release README to `Artifacts/README.md` and moves the packages to `Artifacts/Packages` |

Do not run `MutationTests` or `Benchmarks` locally unless asked; both take long.

## Benchmarks

```powershell
dotnet run -c Release --project Benchmarks/aweXpect.Mockolate.Benchmarks -- --filter *
```

`HappyCaseBenchmarks` is a partial class with one file per benchmark group. Methods ending in `_aweXpect` are highlighted in the pull request comment.

## Documentation

- The user documentation is `README.md` and `Docs/pages/*.md`. The pages are published to [docs.testably.org](https://docs.testably.org/aweXpect/extensions/aweXpect.Mockolate/): after a successful Build on `main`, `notify-docs-site.yml` triggers a rebuild of `Testably/Testably.Site`.
- `Docs/pages/00-index.md` embeds the README through the `{README}` placeholder. Keep the overview table in the README and the tables on the pages in sync when adding an expectation.
- The README is the package readme. `Pack` rewrites its badges for the release version.
- `Docs/agents/` holds this guide and is not published.

## CI workflows

| Workflow | Trigger | Jobs |
|----------|---------|------|
| `ci.yml` (CI) | Pull requests to `main` | Unit tests (`CodeCoverage`) and AOT smoke tests on Ubuntu, Windows and macOS; API tests; mutation tests; benchmarks; SonarCloud (not for Dependabot or forks); test result publishing |
| `ci-analysis.yml` (CI-Analysis) | After CI completes | Mutation dashboard upload and pull request comments for mutation tests and benchmarks |
| `build.yml` (Build) | Pushes to `main`, `v*` tags, manual | The CI jobs plus `Pack`; on a tag, pushes the packages to nuget.org via trusted publishing, creates the GitHub release and labels the released issues |
| `notify-docs-site.yml` | After Build on `main` | Triggers the documentation site rebuild |

Mergify squash-merges Dependabot pull requests once the unit tests on all three platforms and the API tests pass.
