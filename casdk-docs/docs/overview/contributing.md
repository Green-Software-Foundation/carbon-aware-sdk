# GSF Member Contribution Guide

Welcome to the GSF Carbon Aware SDK, and thank you for your interest in contributing to this
project. This guide outlines the steps for working within the Carbon Aware SDK
and the approved contribution process that members should follow.

#### _Before contributing, please read the [GSF Opensource Working Group charter](https://github.com/Green-Software-Foundation/charter/blob/main/charter.md). Any contributions must comply with the charter._ ####

## Table of Contents

- [GSF Member Contribution Guide](#gsf-member-contribution-guide)
  - [Table of Contents](#table-of-contents)
  - [Current Opportunities](#current-opportunities)
  - [How To Get Started](#how-to-get-started)
  - [Code Contribution Steps](#code-contribution-steps)
  - [Public Issues](#public-issues)
  - [Collaborating with the OSWG](#collaborating-with-the-opensource-working-group)

## Current Opportunities
We have opportunities for both code and non code contributors. We're currently looking all contributions, with some areas of extra opportunity outlined in the table below.


| Contribution Areas | Description |
|----------|----------|
|**Sample Creation** | These help adopters of the SDK learn how they can quickly get started and build their own carbon aware solutions.|
|**Documentation Updates** | The documentation always can be improved to make the Carbon Aware SDK more accessible to everyone.  Guides, SDK and API document, and more! |
|**Video Content Creation (how to enable, demos etc)** | Quick videos help adopters understand just how easy it is to get started in an easy to consume form.
|**Slide Deck Creation <br /> Available for presenter use, including real time video demo**| We get a lot of traction at conferences, and if we have a standard deck for anyone to present, it will enable those who might not be able to create a deck, but could easily present it, to also participate.

## How To Get Started 
Introduce yourself on our [discussions page](https://github.com/orgs/Green-Software-Foundation/discussions/65) and let us know where you think you can help. 

Find the Project Key contacts in the [Confluence page](https://greensoftwarefoundation.atlassian.net/wiki/spaces/~612dd45e45cd76006a84071a/pages/17137665/Opensource+Carbon+Aware+SDK).

If you are a GSF member organisation employee, you should:
Fill out the [Onboarding form](https://greensoftware.foundation/onboarding/) if you are new to the GSF; or
Fill out the [Subscribe form](https://greensoftware.foundation/subscribe/) if you are already part of the GSF but want to join this project.
Following this, you'll receive an invite to join the Carbon Aware SDK Weekly Meeting. 
Only members of the foundation can join meetings and internal conversations.

If you are NOT a GSF member organisation employee, individual contributions are still welcome on our public Github repo eg. raising PRs, joining discussions. 

Only our Project Leads have the right to merge PRs. 

Any questions, email help@greensoftware.foundation. 

## Code Contribution Steps

For the following code contribution:

"Member and "Contributor" refer to the GSF member looking to make a feature
code contribution. "Chair" refers to the Chair, Co-Chair or other accountable
authority within GSF.

1. Submit a Public Issue using the Issue Template
2. The Issue will be looked at by a Chair and approved.
3. The Contributor is assigned an "Approved Reviewer" who will help shepherd the
   feature into the GSF Repository
4. Fork `GSF/carbon-aware-sdk/dev` into a member repository,
   `member/carbon-aware-sdk/dev`
5. Open a Draft PR from `member/dev` into `GSF/carbon-aware-sdk/dev` using the
   PR Template
6. Once development is over, the Approved Reviewer pushes the PR into the "Ready
   for Review" state
7. If the Chair accepts the PR, it merges into `GSF/carbon-aware-sdk/dev`

### Project Release Schedule: 
At most once per month. 
As frequent as possible. 
We prioritise Security release over Feature release. Documentation release is not restricted. 

## Public Issues

All contributions to the GSF are tracked through public issues. Please make a
public issue and fill out details on the proposed feature contribution. The
issue serves as a commitment by the contributor to developing the feature.

The Issue is **not** a feature request, but tracks expected feature work. Please
do **not** open an issue to request features.

## Collaborating With The [Opensource Working Group](https://github.com/Green-Software-Foundation/opensource-wg)

1. Create a
   [new Issue](https://github.com/Green-Software-Foundation/standards_wg/issues/new)
2. Discuss Issue with WG --> Create PR if required
3. PR to be submitted against the **DEV feature branch**
4. PR discussed with the WG. If agreed, the WG Chair will merge into **DEV
   Feature branch**
   ![GSF Single-Trunk Based Branch Flow](./images/readme/single-trunk-branch.svg)
5. See
   [The Way we Work](https://github.com/Green-Software-Foundation/standards_wg/blob/main/the_way_we_work.md)
   for futher details.
