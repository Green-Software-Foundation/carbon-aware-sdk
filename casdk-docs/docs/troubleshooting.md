---
sidebar_position: 10
---

# Troubleshooting guide

This chapter shows the hint of troubleshooting if you face trouble(s) when you use Carbon Aware SDK.

- [System.IO.IOException caused by number of inotify instances](#systemioioexception-caused-by-number-of-inotify-instances)

## System.IO.IOException caused by number of inotify instances

You could see `System.IO.IOException` caused by number of inotify instances when you run Carbon Aware SDK on Linux.

.NET runtime uses inotify to watch updating configuration file (e.g. `appsettings.json`). See [Microsoft Lean](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/docker/?view=aspnetcore-8.0) and [discussion on GitHub](https://github.com/dotnet/AspNetCore.Docs/issues/19814) for details.

It is useful for debugging, but it would not be needed in most of production system. So you can disable this feature via environment variable.

You need to set `DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE=false` to avoid the problem. You can set this in `values.yaml` as following when you deploy WebAPI via Helm chart.

```yaml
env:
  - name: DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE
    value: "false"
```
