# 2. Dev Containers

## Status

Accepted

## Context

Development activities require consistency for all developers to have the same
level of access to create Greeen Software as much as possible. The barrier to
entry should be as low as possible, the need to install the correct versions,
and get started with a pre-configured developer environment is key to leveraging
an ASK. The focus should be on "creating green software" as soon as possible, as
opposed to "getting ready to install the environment to create green software".

In addition consistency between developer environments, the ability to resolve
issues, debug fellow contributor issues should be as consistent as possible.

## Decision

All developer experience and documentation should be focused on the dev
container experience. Any platform dependent documentation (for now) should be
kept minimal.

## Consequences

All platforms will have consistency, allowing for faster development of the SDK,
and more focus on features vs platform dependencies.

Platform dependenct implementations and deployments will not have a focus as
they are abstracted.

## Green Impact

Positive

By creating consistency in the developer environment we can ensure green
practices can be considered across all developer environments. This consistency
removes the compute minutes/hours of setup and time lost debugging across
environments, and testing time due to consistency across all environments.

Operating a dev container requires similar CPU intensity with higher memory
requirements. Developers are highly likely to already meet these hardware
requirements and be using dev containers (no additional hardware required).
