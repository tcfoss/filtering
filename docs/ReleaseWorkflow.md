# Release Workflow

## Verify Release Meta-Data
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
4. The projects will be packed and published to NuGet.org.
5. A release will be created on GitHub, with the release notes generated from the work items tied to the release version.


## GitHub Configuration

Navigate to *Settings* > *Secrets and variables* > *Actions*.

Add the following *Variables*:
- `ISSUETRACKER_API_URL` - The API URL for the Issue Tracker project:
  ```
  https://issues.tcflanagan.net/api/filtering
  ```

Add the following *Secrets*:
- `ISSUETRACKER_API_TOKEN` - The API key for the Issue Tracker project. It must have Rel.Manage and Rel.View permissions for the project.