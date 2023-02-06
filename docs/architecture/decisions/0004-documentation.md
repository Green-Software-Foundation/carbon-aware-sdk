# 4. Documentation

## Status

Accepted

## Context

There are README.md files in many different directories and little clarity as to
who that documentation is meant to serve. Some are focused on users operating
the software, others on developers extending the software, sometimes both in the
same file. There are also various other markdown files with a wide range of
completeness. As a user it is hard to know where to look to run the software and
as a developer it is unclear where new documentation should live and for whom it
needs to be written.

## Decision

Documents containing metadata about the repository/project or communicating
organizational processes shall live at the root. Examples include the project
overview `README.md`, `CONTRIBUTING.md`, `LICENSE.md`, etc.

All documentation regarding the usage, extension, or additional development of
the carbon-aware-sdk software shall live in the `/docs` directory.

Documentation focused on using the software as-is (CLI installation,
configuration docs, running the web API, etc) shall be kept separate from
documentation focused on developing new features or extending the sdk.

No documentation shall live outside the `root` or `/docs` directories. All other
`README.md` files must only contain relative links back to the appropriate
`/docs` file. EG:

> `./src/CarbonAware.WebApi/README.md`
>
> ```markdown
> # CarbonAware.WebApi
>
> - [Getting started](/docs/carbon-aware-webapi.md)
> - [Architecture](/docs/architecture/overview.md)
> ```

To enforce file consistency for readers and authors, documentation will be
linted using
[markdownlint](https://github.com/DavidAnson/markdownlint/tree/main).

## Consequences

### Immediate Changes

If this structure is accepted the follow changes would be required to align the
repository:

- placeholder files in `/carbon-aware-sdk` are deleted.
- `/carbon-aware-sdk/license.md` is moved to `/LICENSE.md`
- `/carbon-aware-sdk/Readme.md` content is rehomed to `/README.md` as
  appropriate.
- `/images/placehold.md` is deleted.
- `/images` directory is moved to `/docs/images`.
- `GettingStarted.md` content is rehomed to the `/docs` directory as
  appropriate.
- Content from the `README.md` files in the `/src` directory/sub-directories
  (detailed below) are rehomed to new files in the `/docs` directory.
  - `src\README.md`
  - `src\CarbonAware.Aggregators\src\CarbonAware\README.md`
  - `src\CarbonAware.LocationSources\CarbonAware.LocationSources.Azure\README.md`
  - `src\CarbonAware.Tools\CarbonAware.Tools.AWSRegionTestDataGenerator\README.md`
  - `src\CarbonAware.Tools\CarbonAware.Tools.AzureRegionTestDataGenerator\README.md`
  - `src\CarbonAware.Tools\CarbonAware.Tools.WattTimeClient\src\README.md`
  - `src\CarbonAware.WebApi\src\README.md`
  - `src\CarbonAware\src\docs\README.md`
- Minor formatting changes made to all files to address existing linting
  warning.

More examples provided in the [Appendix](#appendix).

### Longer Term Impact

In the current [monorepo](https://en.wikipedia.org/wiki/Monorepo) structure,
this change should make documentation easier to find, use, and write. However,
this type of consolidation is non-trivial to separate out (especially as it
grows) should the project decide to split components into different repositories
in the future.

## Green Impact

Neutral

## Appendix

Here is an example of what this repository's documentation may look like
following implementation of this ADR proposal:

```text
./
 ┣ docs/
 ┃ ┣ architecture/
 ┃ ┃ ┣ decisions/
 ┃ ┃ ┃ ┣ 0000-ladr-template.md
 ┃ ┃ ┃ ┣ 0001-record-architecture-decisions.md
 ┃ ┃ ┃ ┣ 0002-dev-containers.md
 ┃ ┃ ┃ ┣ 0003-command-line-params-to-config.md
 ┃ ┃ ┃ ┗ 0004-documentation.md
 ┃ ┃ ┣ overview.md
 ┃ ┃ ┣ user-interfaces.md
 ┃ ┃ ┣ aggregators.md
 ┃ ┃ ┗ data-sources.md
 ┃ ┣ quickstart.md
 ┃ ┣ configuration.md
 ┃ ┣ carbon-aware-webapi.md
 ┃ ┗ carbon-aware-cli.md
 ┣ samples/
 ┃ ┣ helmexample/
 ┃ ┃ ┗ README.md
 ┃ ┗ python-proxy-server/
 ┃   ┗ README.md
 ┣ README.md
 ┣ CONTRIBUTIING.md
 ┗ LICENSE.md
```
