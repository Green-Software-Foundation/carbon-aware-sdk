# Green Software Foundation Security Policy

The Green Software Foundation (GSF) takes the security of its executable tooling seriously. GSF is hosted as Joint Development Foundation Projects, LLC, Green Software Foundation Series — an affiliate of the Linux Foundation acting as an **Open-Source Software Steward** under the EU Cyber Resilience Act (CRA).

This policy outlines how to report security vulnerabilities, our response SLAs, and our coordinated disclosure process.

---

## Scope

This policy covers GSF's executable, network-connected tooling:

| Deliverable / Component | Repository | Supported | Notes |
| :--- | :--- | :---: | :--- |
| Impact Framework (CLI + API server) | `Green-Software-Foundation/if` | **Yes** | Graduated Project; `if-run` CLI and `if-api` network server |
| Impact Framework plugins | `Green-Software-Foundation/if-plugins` | **Yes** | Plugin ecosystem for the Impact Framework |
| Carmen (Carbon Measurement Engine) | `Green-Software-Foundation/if-carmen` | **Yes** | Standalone daemon / Kubernetes sidecar; alpha stage, formal GSF approval in progress |
| Carbon Aware SDK | `Green-Software-Foundation/carbon-aware-sdk` | **Yes** | WebApi + CLI; production use includes UBS, Vestas |
| Harmony | `Green-Software-Foundation/harmony` | **Yes** | Existing `docs/SECURITY.md` under review against the CRA bar |
| Badges / Credentials | `Green-Software-Foundation/credentials` | **Yes** | No existing security policy; drafted from scratch here |
| Reference Implementations | `Green-Software-Foundation/reference-implementations` | **Yes** | Educational/illustrative runnable code, not production tooling |
| Software Carbon Intensity (SCI) Specification | `Green-Software-Foundation/sci` | **No** | Methodology/scoring standard only, no executable code — not a Product with Digital Elements (PDE) under the CRA |

*Note: Carmen is in active alpha-stage development for the broader community, distinct from its production maturity at Amadeus. Harmony and Badges/Credentials are newly confirmed in scope and are covered directly under this policy rather than tracked separately. Reference Implementations is included as executable code GSF produces and releases, though its educational, non-production character is relevant to how a reported issue here should be triaged. As new repositories reach "executable tooling" status, they will be brought under the scope of this policy.*

---

## Reporting a Vulnerability

If you discover a security vulnerability in any of the tools listed above, **please do not open a public GitHub issue or discuss it in public channels.** Public disclosure before a fix is available puts downstream adopters at risk.

### How to Submit a Private Report
Please report security vulnerabilities through one of the following channels:

1. **GitHub Private Vulnerability Reporting (Preferred):**
   Navigate to the **Security** tab of the affected repository, click on **Advisories**, and select **Report a vulnerability**.
2. **Encrypted / Private Email:**
   Send your report to **security@greensoftware.foundation**.

### What to Include in Your Report
To help us triage and resolve the issue quickly, please include:
* The affected repository, component, and commit SHA/version.
* A clear description of the vulnerability and its potential security impact.
* Step-by-step instructions or a proof-of-concept (PoC) to reproduce the issue.
* Any potential mitigations or remediations you have identified.

---

## Vulnerability Handling & Response SLAs

Upon receiving a private security report, GSF maintainers will adhere to the following response timeline:

* **Acknowledgment:** Receipt of report acknowledged within **72 hours**.
* **Triage & Assessment:** Initial validation, severity assessment (CVSS), and impact analysis completed within **7 business days**.
* **Remediation & Advisory:** Fix developed, tested, and coordinated for release alongside a security advisory within a mutually agreed disclosure window (typically 30–90 days, depending on complexity).

---

## Coordinated Disclosure & CRA Compliance

GSF follows Coordinated Vulnerability Disclosure (CVD) principles in accordance with OpenSSF and Linux Foundation guidance.

### 1. EU CRA Steward Escalation Protocol
In compliance with Article 24 and Article 14 of the EU Cyber Resilience Act (Regulation (EU) 2024/2847):
* **Actively Exploited Zero-Days & Severe Incidents:** If a reported vulnerability is determined to be actively exploited in the wild, or if a severe cybersecurity incident affects GSF's development infrastructure, GSF will immediately escalate the issue to Linux Foundation Security (`security@linuxfoundation.org`).
* **EU Authority Reporting Timeline:** LF Security will coordinate the required notifications to the European Union Agency for Cybersecurity (ENISA) via the Single Reporting Platform (SRP) and designated National CSIRTs, in accordance with the mandatory legal timeframes:
  * **Early warning:** within **24 hours** of becoming aware of an actively exploited vulnerability or severe incident. *("Becoming aware" means credible confirmation — often reached during the 7-business-day triage step above — not the moment a report is first received. If a report already shows clear evidence of active exploitation, the clock starts immediately.)*
  * **Vulnerability / incident notification:** within **72 hours**, providing general information on the vulnerability, its nature, and any mitigations taken. *(Measured from the same "becoming aware" point as the early warning above, not a fresh 72-hour count.)*
  * **Final report:** no later than **14 days** after a corrective or mitigating measure is available for an actively exploited vulnerability (**1 month** for a severe incident), including a description of the issue, severity, and remediation details. *(This deadline is measured from fix availability, not from the awareness date.)*

### 2. Downstream Security Advisories
Once a fix is available, GSF will publish a machine-readable GitHub Security Advisory (GHSA) and request a CVE identifier where applicable. This provides full transparency to organisations integrating GSF tooling into their own carbon measurement and reporting pipelines.

---

## Contact

* **GSF Security Team:** `security@greensoftware.foundation`
* **Linux Foundation Security Services:** `security@linuxfoundation.org`
* **Documentation:** [if.greensoftware.foundation](https://if.greensoftware.foundation) · [greensoftware.foundation](https://greensoftware.foundation)
