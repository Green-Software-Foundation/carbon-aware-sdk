[![Join #CarbonHack22](images/hackathon-banner.png)](https://grnsft.org/hack22)

**Decarbonize Software. 40K Top Prize.**

Carbon Hack 22, the best carbon aware software solution built using the [Carbon Aware SDK and API](https://github.com/Green-Software-Foundation/carbon-aware-sdk) wins $40K with a total prize pool of $90K.

We're running weekly webinars to talk about the hackathon, demo the SDK and API, talk to hackathon participants and answer any questions. To register for the next one click this link 👉 https://grnsft.org/hack22/webinar

For the duration of the hackathon you can try a hosted version of the API with real-data, try it out here 👉 https://grnsft.org/hack22/api

A carbon aware application does more when the electricity is clean, and less when it's dirty. Whether your specialty is machine learning, web, mobile, cloud, IoT or anything in between you can make any type of application carbon aware with the Carbon Aware SDK and API. Review [FAQ doc](https://grnsft.org/hack22/FAQ) to learn more about the SDK.

You can compete by yourself or join another team if you don't have an idea! Visit our [Project Ideas Matchmaking doc](https://docs.google.com/document/d/14VQZwFe-Q8bxf1TbsNNOXfTT37BFGVfUfk0MzP7rE6c/edit#) where you can find teams to join.



Competition starts on Oct 13 and runs for 3 weeks.

Hackathon ends with a two-minute pitch presentation on Nov 10, judged by global industry leaders from [Accenture](https://www.linkedin.com/company/accenture/), [Avanade](https://www.linkedin.com/company/avanade/), [Boston Consulting Group (BCG)](https://www.linkedin.com/company/boston-consulting-group/), [Globant](https://www.linkedin.com/company/globant/), [Goldman Sachs](https://www.linkedin.com/company/goldman-sachs/), [Intel Corporation](https://www.linkedin.com/company/intel-corporation/), [Thoughtworks](https://www.linkedin.com/company/thoughtworks/), and [UBS](https://www.linkedin.com/company/ubs/). Hosted by the [Green Software Foundation](https://greensoftware.foundation/).

If you are convinced already and want to signup to the hackathon register at https://grnsft.org/hack22.

# Carbon Aware SDK
You can reduce the carbon footprint of your application by just running things at different times and in different locations. That is because not all electricity is produced in the same way. Most is produced through burning fossil fuels, some is produced using cleaner sources like wind and solar.

When software does more when the electricity is clean and do less when the electricity is dirty, or runs in a location where the energy is cleaner, we call this **carbon aware software**.

The Carbon Aware SDK helps you build the carbon aware software solutions with the intelligence to use the greenest energy sources. Run them at the greenest time, or in the greenest locations, or both! Capture consistent telemetry and report on your emissions reduction and make informed decisions.

With the Carbon Aware SDK you can build software that chooses to run when the wind is blowing, enable systems to follow the sun, moving around the world to where energy is the greenest, and create tools that give insights and help software innovators to make greener software decisions. All of this helps reduce carbon emissions.

Get started on creating sustainable software innovation for a greener future today!

# Getting Started

Head on over to the [Getting Started Guide](./GettingStarted.md) to get up and running.

# What is the Carbon Aware SDK?

At its core the Carbon Aware SDK is a WebApi and Command Line Interface (CLI) to assist in building carbon aware software. The functionality across the CLI and WebApi is identical by design.

## The WebApi
The WebApi is the preferred deployment within large organisations to centralise management and increase control and auditability, especially in regulated environments. It can be deployed as a container for easy management, and can be deployed alongside an application within a cluster or separately.

![WebApi Screenshot](./images/screenshot_web_api.png)

## The CLI

The CLI tends to be handy for legacy integration and non-cloud deployments, where a command-line can be used. This tends to be common with legacy DevOps pipelines to drive deployment for integration testing where you can test your deployment in the greenest location.

![WebApi Screenshot](./images/screenshot_cli.png)

# Who Is Using the Carbon Aware SDK?

The Carbon Aware SDK is being used by large and small companies around the world. Some of the world’s biggest enterprises and software companies, through to start-ups.

Machine Learning (ML) workloads are a great example of long running compute intensive workloads, that often are also not time critical. By moving these workloads to a different time, the carbon emissions from the ML training can be reduced by up to 15%, and by moving the location of the training this can be reduced even further, at times by up to 50% or more.

# What does the SDK/API provide that 3rd party data providers such as WattTime or ElectricityMaps do not?
Many of the benefits tend to relate to removing the tight coupling of an application from the 3rd party data source it is using, and allow the application to focus on the sustainability impact it is looking to drive.  This abstraction allows for changing of data providers, data provider aggregation, centralised management, auditability and traceability, and more.

## Collaborative Effort
The Carbon Aware SDK is a collaborative effort between companies around the world, with the intention of providing a platform that everyone can use.  This means the API will be striving towards what solves the highest impact issues with diverse perspectives from these organisation and contributors.

## Standardization 
Something we are driving with the Carbon Aware SDK is towards standardisation of the interface into these data providers.  This ultimately will help to drive SCI calculations in the future, and also helps to drive innovation.  The 3rd party API’s do differ, and the results can vary in units, from lbCO2/kWh to gCO2/Wh.  The Carbon Aware SDK will take care of all conversions to a standardised gCO2/kWh, which becomes increasingly valuable with aggregated data sources.

Standardisation also helps drive innovation.  For example, if a 3rd party develops tools to scale Kubernetes clusters based on emissions, they can build against the Carbon Aware SDK.  If you want to use this 3rd party tool, the SDK allows the tool to plug in _your_ choice of data providers, not _their_ choice of data provider.  In this way the standardisation drives innovation and flexibility of choice.

The intention is to have other compatible tooling and software that leverages the Carbon Aware SDK to obtain emissions data, while being agnostic to the data provider.

## Centralised secret and key management
The ability to manage keys to 3rd party API’s can be centralised with the Carbon Aware API.  This means that any changes to keys or rotation can be done in a centralised and controlled manner without exposing the keys to application development teams.

It also can be upgraded across all applications within an organisation when centralised, with new data sources being added without consuming applications to make any changes.

In addition, the need for the Carbon Aware SDK is something that has been identified by some of the largest enterprises when looking to drive innovation within their own organisations by centralising the capability within their business, creating green software engineering practices and providing the API internally across their organisation.

## Auditability
Due to the API being centralised, this gives you the ability to audit a controlled environment for when decisions are made.  With increasing regulatory need, the ability to prove sustainability actions and impact will need to be from highly trusted sources, and having centralised management provides this capability.

## Aggregated Sources
A feature we have in the roadmap is the ability aggregate data sources across multiple providers.  Different data providers have different levels of granularity depending on region, and it may be that data provider A is preferred in Japan, while data provider B is preferred in US regions.

Similarly, you may have your own data for your data centres that you would prefer to use for on premises workloads, which you can combine in aggregate with 3rd party data providers.

# Is it possible to retrieve energy mix information from the SDK?
Energy mix (the percentages that are from different energy soruces i.e. coal, nuclear, wind, gas, solar, tidal, hydro etc) is not provided in the API to date.  This may be a feature we will consider in the future.  The SDK provides emissions percentage information only at the moment.

# Contributing

The Carbon Aware SDK is open for contribution! Want to contribute? Check out the [contribution guide](./CONTRIBUTING.md).

# Green Software Foundation Project Summary

The Carbon Aware SDK is a project as part of the [Green Software Foundation](https://greensoftware.foundation/) (GSF) and the GSF Open Source Working Group.

## Appointments

-   Chair/Project lead - Vaughan Knight (Microsoft)
-   Vice Chair - Szymon Duchniewicz (Avanade)

## GSF Project Scope

For developers to build carbon aware software, there is a need for a unified baseline to be implemented. The Carbon Aware Core SDK is a project to build a common core, that is flexible, agnostic, and open, allowing software and systems to build around carbon aware capabilities, and provide the information so those systems themselves become carbon aware.

The Carbon Aware Core API will look to standardise and simplify carbon awareness for developers through a unified API, command line interface, and modular carbon-aware-logic plugin architecture.
