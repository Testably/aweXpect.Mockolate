# Contributor guide

## Pull requests

**Pull requests are welcome!**  
Please include a clear description of the changes you have made with your request; the title should follow
the [conventional commits](https://www.conventionalcommits.org/en/v1.0.0/) guideline.
All code should be covered by unit tests and comply with the coding guideline in this project.

### Technical expectations

As a framework for supporting unit testing, this project has a high standard for testing itself.  
In order to support this, static code analysis is performed
using [SonarCloud](https://sonarcloud.io/project/overview?id=Testably_aweXpect.Mockolate) with a quality gate that requires:

- all issues reported by SonarCloud to be resolved
- code coverage to be > 90%

Additionally each pull request and each push to the `main` branch checks the quality of the unit tests using [Stryker.NET](https://stryker-mutator.io/docs/stryker-net/introduction/).

### Running the tests
The tests use [TUnit](https://tunit.dev/) on the Microsoft.Testing.Platform. Run all of them with:
```
dotnet test
```

The public API tests in `Tests/aweXpect.Mockolate.Api.Tests` fail whenever the public API changes. If the change is intended, accept it with the explicit test `AcceptApiChanges`, which a normal run skips, and commit the updated `*.awexpect-snapshot.txt` files:
```
dotnet test --project Tests/aweXpect.Mockolate.Api.Tests --treenode-filter "/*/*/ApiApprovalTests/AcceptApiChanges"
```
