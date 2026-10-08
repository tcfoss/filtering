---
description: Build, test, and contribute to TcfOss.Filtering.
---

# Contributing


## Getting Started

Install the [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0).
The repository's `global.json` selects the supported SDK feature bands and Microsoft.Testing.Platform test runner.
Install Node.js 24 to work on the TypeScript contracts or demo frontend.

From the repository root:

```bash
dotnet build Filtering.slnx
dotnet test --project test/TcfOss.Filtering.Linq.Tests/TcfOss.Filtering.Linq.Tests.csproj
dotnet test --project test/TcfOss.Filtering.Programmatic.Tests/TcfOss.Filtering.Programmatic.Tests.csproj
```

Package consumers should start with the [package usage guides](../index.md#choose-a-package)
rather than these contributor instructions. For a runnable example, see the
[demo README](https://github.com/tcfoss/filtering/blob/master/demo/README.md).


## Formatting

Check formatting before opening a pull request:

```bash
dotnet format --severity info --verify-no-changes
```

To apply formatting fixes, run `dotnet format --severity info` and review the diff.
Keep formatting changes scoped to the code you are changing.


## Integration Tests

The LINQ integration tests use Testcontainers to start MariaDB.
Run them only with Docker installed and running, and with your user able to access the Docker daemon:

```bash
dotnet test --project test/TcfOss.Filtering.Linq.IntegrationTests/TcfOss.Filtering.Linq.IntegrationTests.csproj
```

With that Docker environment available, run every test project in the solution:

```bash
dotnet test --solution Filtering.slnx
```

Pull requests run the two unit-test projects. Integration tests also run for release branches
or pull requests labeled `full-ci`.


## TypeScript Contracts

From the repository root:

```bash
cd npm/tcfoss-filtering-contracts
npm ci
npm run build
npm run typecheck
```


## Documentation and Releases

- [Documentation maintenance](documentation.md): install MkDocs, preview changes, and check links.
- [Release workflow](ReleaseWorkflow.md): release branches, package publishing, symbols, and GitHub Pages setup.


## Feedback and Pull Requests

Report bugs and feature requests on [IssueTracker](https://issues.tcflanagan.net/filtering).
Contributions use the usual fork-and-pull-request workflow. Include tests for new behavior,
ensure existing tests pass, and update relevant package READMEs and documentation.
See the [repository contribution checklist](https://github.com/tcfoss/filtering#reporting-issues-and-contributing)
before opening a pull request.
