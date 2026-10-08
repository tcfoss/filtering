# Release Workflow

## Verify Release Metadata
Make sure the [Issue Tracker](https://issues.tcflanagan.net/filtering) is up to date with the work items that are being released:

- There must be at least one release-eligible work item tied to the release version in the "Done" state.
- All work items tied to the release version must be in the "Done" state.
- The release must be in the "Active" state.

## Create a Release PR

Create a new branch from `master` with the name `release/vX.Y.Z[-suffix]`, where `X.Y.Z` is the version number to be released, and `-suffix` is an optional pre-release suffix (e.g., `-beta.0`).

Create a pull request from the release branch to `master`. The PR should ideally contain no code changes.

## What Happens on Merge

When the release PR is merged, the following will happen automatically:

1. The release version will be parsed from the branch name.
2. The `master` branch will be tagged with the parsed release version.
3. The projects will be built, causing MinVer to capture the version number from the tag and assign it to the assemblies.
4. The .NET packages will be packed and published to GitHub Packages and NuGet.org.
5. The TypeScript contracts will be built, versioned, and published to npm and GitHub Packages.
6. Documentation will be built with MkDocs from the exact release tag and uploaded as a Pages artifact.
7. After both package publishers succeed and work items are resolved, the release will be finalized, including a GitHub release with notes generated from its work items.
8. After successful documentation build and release finalization, the deployment job checks whether this is the highest published stable version. Only eligible releases replace the [GitHub Pages site](https://tcfoss.github.io/filtering/).

Each .NET package also produces a `.snupkg` with portable PDBs and Source Link metadata.
The NuGet.org push uploads the matching symbol package automatically; the GitHub Packages push disables symbol uploads.
Consumers can enable the NuGet.org symbol server (`https://symbols.nuget.org/download/symbols`) in their debugger to step into package source.
GitHub Actions builds enable `ContinuousIntegrationBuild` for normalized source paths. Source Link and embedding of untracked compiled sources use the .NET SDK defaults.

Package publishing and documentation builds run independently after tagging.
A documentation failure is visible in the workflow but does not block release finalization; deployment depends on finalization, not the reverse.
Drafts and prereleases are never deployed. An older-version rerun cannot replace the site when a higher stable version is published.
Stable versions are compared numerically, rather than by publication date or the GitHub "latest" designation.
Ordinary PR merges and unmerged PR closures do not publish the documentation site.

## Manual Releases

Run the **Release** workflow with `workflow_dispatch` to release or retry a specific version.
Provide the `version` input (for example, `1.0.0`); enable `force-retag` only when deliberately replacing an existing tag.
Choose the intended release ref, normally `master`. The documentation build checks out `refs/tags/v<version>`.
Rerunning the highest published stable version can redeploy its documentation; rerunning an older version or prerelease does not replace the site.
Work-item resolution is skipped for manual runs; package publishing, documentation publishing, and release finalization still run.

See the [release workflow source](https://github.com/tcfoss/filtering/blob/master/.github/workflows/closed-pr.yml) for the complete job configuration.

## GitHub Configuration

Navigate to *Settings* > *Secrets and variables* > *Actions*.

Add the following *Variables*:
- `ISSUETRACKER_API_URL` - The API URL for the Issue Tracker project:
  ```
  https://issues.tcflanagan.net/api/filtering
  ```

Add the following *Secrets*:
- `ISSUETRACKER_API_TOKEN` - The API key for the Issue Tracker project. It must have Rel.Manage and Rel.View permissions for the project, plus the permissions required by the work-item actions.
- `TAGGER_APP_CLIENT_ID` and `TAGGER_APP_PRIVATE_KEY` - GitHub App credentials used by the reusable tagging workflow.

NuGet publishing uses trusted publishing via `NuGet/login`; npm publishing uses OIDC trusted publishing.
Configure the corresponding publishers in the package registries. GitHub Packages uses the workflow's `GITHUB_TOKEN`.

### GitHub Pages

Before the first documentation deployment:

1. Open **Settings > Pages** and set **Source** to **GitHub Actions**.
2. Check the `github-pages` environment's deployment protection rules. Allow `master` and any refs deliberately used for manual releases.
3. Merge a stable release PR or run the Release workflow manually for the highest stable version, then check the `build-docs` and `deploy-docs` jobs and the published site.

The build job uses `contents: read` and uploads the generated site as a Pages artifact.
The deployment job uses `contents: read`, `pages: write`, and `id-token: write` permissions.
It queries published GitHub releases and deploys the artifact with GitHub's Pages actions; no `gh-pages` branch or additional secret is needed.
Documentation deployments are serialized without cancelling package release jobs, and eligibility is checked inside that serialized job.

For local previews and documentation contribution guidelines, see [documentation maintenance](documentation.md).