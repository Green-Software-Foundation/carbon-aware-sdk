# Security Policy

## Reporting a Vulnerability

To report a security issue, please email carbon-aware-sdk@greensoftware.foundation with a description of the issue, steps required to reproduce the issue, affected versions and, if known, mitigations for the issue.

Our contributors are comprised of volunteers so we cannot guarantee a specific response time, but someone from our team will reply and address the issue as soon as possible.

# Security Review
We perform regular reviews inline with the information provided below.  All releases go through these reviews but multiple people in the project team prior to release as part of our quality and security review.

## Basics
### Basic Project Website Content
- Describe what the project does - ✅  in README
- Provide info how to obtain/provide feedback and contribute - ✅ https://github.com/Green-Software-Foundation/carbon-aware-sdk/blob/dev/CONTRIBUTING.md#code-contribution-steps
- Explain contribution process - ✅ https://github.com/Green-Software-Foundation/carbon-aware-sdk/blob/dev/CONTRIBUTING.md#collaborating-with-the-opensource-working-group

### FLOSS license
- Must be released as FLOSS - ✅ MIT License https://github.com/Green-Software-Foundation/carbon-aware-sdk/blob/dev/LICENSE
- Must post the license - ✅ https://github.com/Green-Software-Foundation/carbon-aware-sdk/blob/dev/LICENSE
- Also approved by OSI - ✅  https://opensource.org/license/MIT/

### Documentation
- Provides basic documentation - ✅ https://github.com/Green-Software-Foundation/carbon-aware-sdk/tree/dev/docs
- Provides documentation for external interface - ✅ https://github.com/Green-Software-Foundation/carbon-aware-sdk/blob/dev/docs/carbon-aware-webapi.md

### Other
- Project site, downloads etc must support HTTPS with TLS - ✅ using GitHub to host which supports this https://github.com/Green-Software-Foundation/carbon-aware-sdk/
- Have mechanism for discussion - ✅ github issues https://github.com/Green-Software-Foundation/carbon-aware-sdk/issues
- Project must be maintained - ✅ actively maintaned by GSF and its members

## Change control
###  Public VCS repo
- Readable public VCS repo - ✅ yes, Github https://github.com/Green-Software-Foundation/carbon-aware-sdk/
- Track changes - ✅ yes, Git https://github.com/Green-Software-Foundation/carbon-aware-sdk/commits/dev/
- Interim versions between releases available for review - ✅ yes, interim versions actively developed and availble on the `dev` branch https://github.com/Green-Software-Foundation/carbon-aware-sdk

### Unique versioning numbering
- Unique indentifier for each release - ✅ https://github.com/Green-Software-Foundation/carbon-aware-sdk/releases

### Release notes
- Human readable release notes for each release (not git log) - ✅ https://github.com/Green-Software-Foundation/carbon-aware-sdk/releases
- Address each publicly known vulnerability - ✅ N/A, no vulnerability reported yet

## Reporting
### Bug reporting process
- Process to submit bugs - ✅ https://github.com/Green-Software-Foundation/carbon-aware-sdk/issues/new/choose
- Must acknowledge bugs (reply) submitted between 2-12 months - ✅ each bug has at least an acknowledgement or was opened by a maintainer (so acknowledged by a maintainer): https://github.com/Green-Software-Foundation/carbon-aware-sdk/issues?q=is%3Aopen+is%3Aissue+label%3Abug
- Publicly available archive for reports and responses - ✅ github issues: https://github.com/Green-Software-Foundation/carbon-aware-sdk/issues?q=is%3Aopen+is%3Aissue+label%3Abug

## Vulnerability report process
- Have a vulnerability report process - ✅ Added in this PR: #464 
- Private vulnerability if supported must include info how to send - ✅ N/A (allowed) - no private vulnerability reporting set up but proposed
- Initial response time for vulnerability submitted in last 6 months must be <= 14 days - ✅ N/A (allowed) - project run by volunteers, does not provide response time guarantee as stated in SECURITY.md (this pr)

## Quality
### Working build system
- Must provide a working build system - ✅ https://github.com/Green-Software-Foundation/carbon-aware-sdk/blob/dev/docs/carbon-aware-cli.md#build-and-install https://github.com/Green-Software-Foundation/carbon-aware-sdk/blob/dev/docs/containerization.md

### Automated test suite
- Have at least one automated test suite and documentation hwo to run it - ✅  https://github.com/Green-Software-Foundation/carbon-aware-sdk/blob/dev/.github/workflows/1-pr.yaml as automated CI during PRs

## New functionaility testing
- Formal/informal policy for adding tests for new features - ✅ PR template requires stating if a breaking feature added, maintainers ensure tests are in place: https://github.com/Green-Software-Foundation/carbon-aware-sdk/blob/dev/.github/pull_request_template.md 
- Evidence of policy being adhered to - ✅ on release code coverage increase (new code added did not decrease test coverage): https://github.com/Green-Software-Foundation/carbon-aware-sdk/pull/437#issuecomment-1862346606 

### Warning flags
- Compiler warning flags or linter tools for code quality/errors - ✅ CodeQL analysis in automated CI : https://github.com/Green-Software-Foundation/carbon-aware-sdk/blob/dev/.github/workflows/1-pr.yaml#L82
- Address warnings from these tools - ✅ blocking PRs on fail

## Security
### Secure development knowledge
- At least one primary developer who knows how to design secure software - ✅ @vaughanknight is at least one of them :)
- At least one of the project's primary developers MUST know of common kinds of errors that lead to vulnerabilities in this kind of software, as well as at least one method to counter or mitigate each of them - ✅ 

### Use basic good cryptographic practices
- https://www.bestpractices.dev/en/criteria/0#0.crypto_published - ✅ uses HTTPS for WebAPI, N/A for CLI
- https://www.bestpractices.dev/en/criteria/0#0.crypto_floss - ✅ uses dotnet 8.0 implementations
- https://www.bestpractices.dev/en/criteria/0#0.crypto_keylength - ✅ uses dotnet 8.0 implementations
- https://www.bestpractices.dev/en/criteria/0#0.crypto_working - ✅ uses dotnet 8.0 implementations
- https://www.bestpractices.dev/en/criteria/0#0.crypto_password_storage - ✅ ⚠️  uses dotnet 8.0 implementations
- https://www.bestpractices.dev/en/criteria/0#0.crypto_random - ✅ uses dotnet 8.0 implementatons for HTTPS

### Secured delivery against man-in-the-middle (MITM) attacks
- Delivery mechanisms that counters MITM - ✅ uses HTTPS
- Cyrptographic hash NOT retrived over HTTP - ✅  ues HTTPS

### Publicly known vulnerabilities fixed
- No unpatched vulnerabilities of medium or higher severity that have been publicly known for more than 60 day - ✅ no such vulnerabilities

### Other security issues
- Public repo doesnt leak private credential - ✅ does not do that

## Analysis
### Static code analysis
- At least one FLOSS static code analysis tool - ✅ uses CodeQL https://codeql.github.com/ - https://github.com/Green-Software-Foundation/carbon-aware-sdk/blob/dev/.github/workflows/1-pr.yaml#L82
- All medium and higher severity exploitable vulnerabilities discovered with static code analysis MUST be fixed in a timely way after they are confirmed - ✅ ⚠️ NOTE: Medium vulnerabilities are identified but these are **by design** due to relating to geolocation data being transmitted, and geolocation is required for the CA SDK. The code will be  annotated to ignore this: https://github.com/Green-Software-Foundation/carbon-aware-sdk/issues/415#issuecomment-1882622776 

### Dynamic code analysis
- All medium and higher severity exploitable vulnerabilities discovered with dynamic code analysis MUST be fixed in a timely way after they are confirmed. - ✅ N/A (allowed, no Dynamic code analysis in place).

