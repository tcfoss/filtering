---
description: Build, preview, and maintain the TcfOss.Filtering documentation site.
---

# Documentation


## Building Documentation Locally

From the repository root, create a virtual environment and install the pinned dependencies:

```bash
python3 -m venv .venv-docs
```

On Linux/macOS, activate it with `source .venv-docs/bin/activate`.
In Windows PowerShell, use `.venv-docs\Scripts\Activate.ps1`. Then run:

```bash
python -m pip install -r requirements-docs.txt
mkdocs build --strict
mkdocs serve
```

CI uses Python 3.12. `mkdocs serve` normally serves the preview at `http://127.0.0.1:8000/filtering/`.
If that port is occupied, use `mkdocs serve --dev-addr 127.0.0.1:8123` and open `http://127.0.0.1:8123/filtering/`.
The generated `site/` directory and local `.venv-docs/` environment are not committed.


## Editing Pages

Package usage pages include the source package READMEs at build time, so edit those READMEs
rather than duplicating examples in `docs/packages/`. The preview watches included READMEs for changes.

Keep links between documentation files relative to their Markdown sources, including fragments when linking to headings.
Link repository files outside `docs/` using their GitHub URLs.
Package READMEs use absolute Pages links so they work on GitHub, NuGet, npm, and this site.
Update the navigation in `mkdocs.yml` when adding or moving a page.


## Validation and Publishing

Pull requests run `mkdocs build --strict` to check navigation and internal links, including anchors.
Absolute/external URLs need separate checking. Repository administrators can require the **Documentation** status check in branch protection rules.

The release workflow builds documentation from the exact `vX.Y.Z` tag on release merges and manual releases.
The build runs independently of package publishers. Deployment waits for successful release finalization
and only publishes the highest numeric stable version among non-draft, non-prerelease GitHub releases.
Prereleases and reruns of older versions do not replace the site; reruns of the current highest stable version can redeploy it.
Ordinary PR merges and unmerged PR closures do not publish the site.

Build and deployment appear as separate jobs. Documentation failure does not block release finalization.
The serialized deployment job rechecks eligibility immediately before configuring and deploying Pages,
so an older run that reaches deployment after a newer release has been published is skipped.

With Node.js 24 installed, run the release-policy regression tests from the repository root:

```bash
node --test .github/scripts/docs-release.test.cjs
```

The PR documentation job also runs these tests.

See the [release workflow](ReleaseWorkflow.md#github-pages) for GitHub Pages configuration,
deployment permissions, and environment protection requirements.