# 3. Move Command Line Parameters to Config File

## Status

Accepted

## Context

The CLI works currently through a variety of command line parameters, and while
this works for the current array of options, it is unlikely to cater for future
needs.

With a dynamic plugin based architecture, plugins will require a variety of
custom configurations that can not be predetermined by the command line.

To handle this we need to abstract this complexity of "how" the SDK is
configured from the command line parameters.

## Decision

The decisions is to move all command line parameters other than time "-t --time
-toTime" and location "-l --location" parameters to a standard
"carbon-aware.config". The only other command line parameters that will remain
are "-h --help" and a new command line parameter to define the configuration
file location "-c --config".

The file will be a json file due to json being widely known, and static data
files already being in json, so this will not introduce any other dependencies
or skills.

## Consequences

This will mean the application will have a standalone executable + a config
file.

The usage focus becomes more of "when" and "where" vs "how". "How" is now
configured.

Regardless of plugin, the command line parameters will always be the same. This
will create more consistency. Due to this, testing command line will now be
consistent across plugins due to the configuration file being the only change.

For native integration to the SDK, this configuration would usually be in code.
This can still occur, however it would not be possible to store this
configuration externally if required, making the native code configurable also.

Configuration may differ betweeen platforms/languages - however this would be
the case via command line. This means there will be consistency in the command
line between platforms, hiding platform depdendencies from the parameters and
moved to the config.

## Green Impact

Neutral
