# Probe
![.NET](https://github.com/nvsnkv/NVs.Probe/actions/workflows/dotnet.yml/badge.svg)

Probe is cross-platform application that simplifies metrics collection and publishing. 
* No custom metrics format - Probe is designed to use CLI to gather metrics. From one stand point it's not as easy as tell "Gather CPU load", but from other hand it allows you to gather any stats you can get from command line!
* No custom telemetry protocol - Probe uses MQTT to publish measurement results. That allows you to connect Probe with you smart home infrastructure and setup as complex monitoring and automation as you wish.

# Usage
Grab the binary you need from the [releases](https://github.com/nvsnkv/NVs.Probe/releases) page, copy it to the location you need, update the configuration and run it!
If the binaries posted on releases page do not work for you, [development notes](#development-notes) at the end of this readme will help to build the Probe from the source code.

## Configuration
Probe reads configuration file  (`-c` option) to retrieve its setup. Sample file content can be found below:
```
mqtt:
  client_id: probe
  broker: mqtt.local
  user: mqtt_user
  password: mqtt_password

runner:
  shell: sh
  flags: "-c"

metrics:
  - topic: /probe/echo/1
    command: echo 1
  - topic: /probe/echo/2
    command: echo 2
```
[!] Configuration file contains password for MQTT broker in it, so please ensure the access to this file is properly restricted!

Configuration file consists of 3 required groups - MQTT configuration, Command Runner setup and the list of metrics.
### Metrics configuration (mertics)
Metrics are configured as a pairs of MQTT topic and CLI command associated with it:
* `topic` - string, required. Defines MQTT topic that will be used to publish values;
* `command` - string, required. Defines CLI command that Probe will run to measure metric value.

In addition to the sequence of metrics optional `inter_series_delay` parameter can be used to adjust time interval between two series of measurements. Default is "00:02:00", 2 minutes.
### MQTT Connection setup (mqtt)
Following parameters are required:
* `client_id` - string, MQTT Client Identifier;
* `broker` - string, the hostname or IP address of MQTT broker;
* `user` - string, username used for authentication on MQTT broker;
* `password` - string, password used for authentication on MQTT broker.

Following parameters are optional:
* `port` - interger, port number of MQTT broker. Default is 1883;
* `retries_count` - integer, count of attempts to reconnect to MQTT broker in case of connectivity issues. Application would not try to reconnect if this parameter is not provided. `retries_interval` must be provided together with this setting. If defined, the value should be positive;
* `retries_interval` - TimeSpan. Base interval between attempts to reconnect to MQTT broker. Must be defined if `retries_count` is set. Must not be defined otherwise.
### Runner configuration (runner)
Runner configuration allows to define which interpreter will be used to run the metrics and what would be a command timeout. Following parameter is required:
* `shell` - string, required. Interpreter to use for measurements. Can be any application that receives command as command line arguments.

Following parameters are optional
* `flags` - string, optional. Interpreter flags. Should be used if interpreter does not execute commands from command line parameters by default. Some interpreters require special flags to be provided to treat remaining command line args as a command to execute (`sh -c 'echo 1'` or `cmd /c 'echo 1'` ).
Some interpreters do not requre it (`powershell 'echo 1'`);
* `command_timeout` - TimeSpan, optional. Command timeout. Default is "00:00:02".

## Starting the Probe
The recommended way is to start new application using `deploy` verb - it will launch a separate process without blocking initial one:
```
probe deploy -c probe.settings.yaml my_probe
```
Use `stop` verb to terminate a host:
```
probe stop my_probe
```
### Command line options
#### Serve
Command `serve` is used to start the Probe. The process will establish connection to MQTT broker and start reporting measured metric values in accordance with configuration from configuration file.
The command has two required parameters:
* `-c`, `--configuration-path` - a path to configuration file
* `-i`, `--instsance-id` - instance identifier

Example:
`probe serve -c probe.settings.yaml -i my_probe`
#### Deploy
Like `serve`, `deploy` command is used to start the Probe. But unlike `serve`, `deploy` starts new instance of Probe in a separate process and then exits.
Following parameter is required:
* `-c`, `--configuration-path` - a path to configuration file for new instance

Following parameters are optional:
* `-v,`, `--verbose` - flag that enables verbose output. Disabled by default
* `-t`. `--connection-timeout` - Communication timeout for requests to new instance in milliseconds. Default is 1900

By default, `deploy` will generate a random and creepy instance id for newly created instance. If you prefer to name it yourself, just provide the name as an additional argument:
```
probe deploy -c probe.settings.yaml my_awesome_instance_id
```
#### Stop
No surprises here - `stop` just stops previosly deployed instance of Probe.
This command needs an instance id to stop - it needs to be passed as an additional argument:
```
probe stop my_awesome_instance_id
```
`stop` also accepts optional `--verbose` and `--connection-timeout` arguments with the same behavior as for `deploy`
#### Stub
A command that was created to debug `deploy` and `stop` commands. Useful for development, but makes no sense in production.

## Logging
Applicaion uses [Serilog](https://serilog.net/) to produce logs. Logging configuration is set up in `probe.serilog.logging.yaml` file.

# Development notes
## Building
### linux-arm
The steps below will help to build Probe for ARM-based Linux host, such as Raspberry Pi:
1. run `dotnet publish NVs.Probe\NVs.Probe.csproj /p:PublishProfile=Linux_arm`;
1. copy files from `publish/linux-arm` to the target machine.
### windows-x64
1. run `dotnet publish NVs.Probe\NVs.Probe.csproj /p:PublishProfile=Win_x64`;
1. copy files from `publish/win-x64` to the target machine.
### Other runtimes
Application is runtime-agnostic by itself and can be compiled for any runtime supported by .Net Core by specifying `runtime (-r)`  argument for `dotnet build` or `dotnet publish`

## Automated tests
There are few tests that helped me to write this tool.
Some of them tests .NET components and can be started from any supported runtime. And some of them were used to check interop with CLI on a specific OSes.
Tests with "Category" equals to "Windows" will work on Windows hosts. Tests with "Linux" "Category" work on linux machines.
`NVs.Probe.Tests\Dockerfile` was created to facilitate execution of Linux-specific tests on Windows hosts.
Command ` docker build --no-cache -f .\NVs.Probe.Tests\Dockerfile .` executed from solution root will build this container. Execution of Linux-specific tests is a part of the build for this container.
