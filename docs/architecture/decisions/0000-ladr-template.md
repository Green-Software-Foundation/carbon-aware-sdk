# 1. Title

## Status

[Proposed, Accepted, Deprecated, Superseded]

## Context

## Decision

## Consequences

## Green Architecture Considerations
[Positive, Neutral, Negative]

> Does this ADR have a notable positive, neutral, or negative impact in relation to the Principles of Green Software.  
>
> Include the major impact considerations across CPU intensity, hardware, network, behavioural, usage.  Avoid minor impact concerns to give focus.  It does not need to be detailed and in depth, but it should be clear in explaining why it's considered positive, neutral, or negative.
> 
> Neutral cases may at times be self explanatory and a description won't be required.
>
> If it is negative, is there any plan or action to revisit this decision?
> 
> If the ADR is a Green focused ADR, the section detail can simply say "Refer to the above".
> ### Positive example 
>   * ADR: Moved to elastic servers to reduce costs.
>   * Green Archtiecture Considerations: 
>      * Positive.  Elastic servers reduce hardware requirements and also reduce compute intensity during low demand periods.
> ### Neutral example 
>   * ADR: Moved to different version of Tensorflow for feature
>   * Green Archtiecture Considerations: 
>      * Neutral 
> ### Negative example 
>   * ADR: Moved to static VM deployment from elastic due to platform bug
>   * Green Archtiecture Considerations: 
>      * Negative.  Static VM's will require a higher hardware and CPU intensity at all times.  Once the bug is resolved we will look to move back to elastic.  This is tracked via Issue #... 
