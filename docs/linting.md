# Linting

- [Linting](#linting)
  - [Markdown Linting](#markdown-linting)
    - [Github Action](#github-action)
    - [Linting Locally](#linting-locally)
    - [VS Code Extension](#vs-code-extension)

## Markdown Linting

Per [ADR 0004](./architecture/decisions/0004-documentation.md), documentation
will be linted using
[markdownlint](https://github.com/DavidAnson/markdownlint/tree/main) to enforce
file consistency for readers and authors.

This repo contains a
[custom.markdownlint.jsonc](../../custom.markdownlint.jsonc) configuration file
at the root which handles the markdown rules enforced. This file can be used
locally (to check/fix violations) and is also consumed by the Github Action
pipeline to check for consistency on PRs.

### Github Action

The [linting.yaml](../.github/workflows/linting.yaml) workflow contains the
github action that will run on for each PR. The `"Markdown Linting"` job handles
running the [markdown linter](https://github.com/DavidAnson/markdownlint-cli2)
using the [custom.markdownlint.jsonc](../../custom.markdownlint.jsonc) config
file at the root of the repo. This action is non-blocking, and is meant to
provide information to the user about violations .

### Linting Locally

In order to run markdown linting locally, you will need to have an installation
of [markdownlint](https://github.com/DavidAnson/markdownlint). We reccomend the
[markdownlint-cli2](https://github.com/DavidAnson/markdownlint-cli2), which is
the same used by the Github action pipeline.

The various CLI commands are detailed in the Github docs but the command the
pipeline will run (checks all files given the custom config except for .github
folder) is:

```bash
markdownlint-cli2-config "./custom.markdownlint.jsonc" {"*[^.github]/**,*"}.md
```

The result will list all of the violations including the file, line number, and
code for the violation. An example successful result is included below:

```bash
Finding: **/*.md
Linting: 35 file(s)
Summary: 0 error(s)
```

#### VS Code Extension

If you are developing in VS Code, there is also a
[markdownlint](https://marketplace.visualstudio.com/items?itemName=DavidAnson.vscode-markdownlint)
extension which you can install. This extension will let you leverage the VS
Code formatter to fix your markdown files. While it may not be able to fix all
of the violations, it will catch most of the small formatting ones.

> Note the extension uses the default formatting configuration. If you want to
> auto-format based on our custom config file, you will need to manually include
> those in the extensions' settings.
