# Probe
Probe is cross-platform application that simplifies metrics collection and publishing. 
* No custom metrics format - Probe is designed to use CLI to gather metrics. From one stand point it's not as easy as tell "Gather CPU load", but from other hand it allows you to gather any stats you can get from command line!
* No custom telemetry protocol - Probe uses MQTT to publish measurement results. That allows you to connect Probe with you smart home infrastructure and setup as complex monitoring and automation as you wish.

## Usage
### Building
#### linux-arm
The steps below will help to build Probe for ARM-based Linux host, such as Raspberry Pi:
1. run `dotnet publish NVs.Probe\NVs.Probe.csproj /p:PublishProfile=Linux_arm`;
1. copy files from `install/linux` to `publish/linux-arm`;
1. copy files from `publish/linux-arm` to the target machine.
#### windows-x64
1. run `dotnet publish NVs.Probe\NVs.Probe.csproj /p:PublishProfile=Win_x64`;
1. copy files from `publish/win-x64` to the target machine.
#### Other runtimes
Application is runtime-agnostic by itself and can be compiled for any runtime supported by .Net Core by specifying `runtime (-r)`  argument for `dotnet build` or `dotnet publish`

### Running the app
#### Updating configuration file
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

Configuration file consist of 3 required groups - MQTT configuration, Command Runner setup and the list of metrics.
##### Metrics configuration (mertics)
Metrics are configured as a pairs of MQTT topic and CLI command associated with it:
* `topic` - string, required. Defines MQTT topic that will be used to publish values;
* `command` - string, required. Defines CLI command that Probe will run to measure metric value.

In addition to the sequence of metrics optional `inter_series_delay` parameter can be used to adjust time interval between two series of measurements. Default is "00:02:00", 2 minutes.
##### MQTT Connection setup (mqtt)
Following parameters are required:
* `client_id` - string, MQTT Client Identifier;
* `broker` - string, the hostname or IP address of MQTT broker;
* `user` - string, username used for authentication on MQTT broker;
* `password` - string, password used for authentication on MQTT broker.

Following parameters are optional:
* `port` - interger, port number of MQTT broker. Default is 1883;
* `retries_count` - integer, count of attempts to reconnect to MQTT broker in case of connectivity issues. Application would not try to reconnect if this parameter is not provided. `retries_interval` must be provided together with this setting. If defined, the value should be positive;
* `retries_interval` - TimeSpan. Base interval between attempts to reconnect to MQTT broker. Must be defined if `retries_count` is set. Must not be defined otherwise.
##### Runner configuration (runner)
Runner configuration allows to define which interpreter will be used to run the metrics and what would be a command timeout. Following parameter is required:
* `shell` - string, required. Interpreter to use for measurements. Can be any application that receives command as command line arguments.
Following parameters are optional
* `flags` - string, optional. Interpreter flags. Should be used if interpreter does not execute commands from command line parameters by default. Some interpreters requires special flags to be provided to treat remaining command line as a command to execute (`sh -c 'echo 1'` or `cmd /c 'echo 1'` ).
Some interpreters does not requre it (`powershell 'echo 1'`);
* `command_timeout` - TimeSpan, optional. Command timeout. Default is "00:00:02".

#### CLI
Application can be started as a regular process from command line:
```
#!/bin/sh
probe serve -c probe.settings.yaml -i my_probe
```
Probe also works with Windows, both Powershell and CMD are supported.
Application can use any other app that reads command line arguments and returns output into STDOUT to measure the metrics.

##### Command line options
`probe --help` will list available configuration options:
```
 -i, --interpreter                Required. Interpreter used to execute
                                  commands

 -f, --interpreter-flags          Required. Interpreter flags

 -s, --metrics-setup              Required. (Default: probe.metrics.yaml) Path
                                  to metrics configuration

 -m, --mqtt-options               Required. (Default: probe.mqtt.yaml) Path to
                                  MQTT configuration

 --measurement-timeout            (Default: 1000) Timeout of a single
                                  measurement

 --measurement-series-interval    (Default: 120000) Base interval between
                                  measurement series

 --help                           Display this help screen.

 --version                        Display version information.
```
### Logging
Applicaion uses [Serilog](https://serilog.net/) to produce logs. Logging configuration is set up in `probe.serilog.logging.yaml` file.

## Development notes
### Automated tests
There are few tests that helped me to write this tool.
Some of them tests .Net components and can be started from any supported runtime.
Tests with "Category" equals to "Windows" will work on Windows hosts. Tests with "Linux" "Category" works on linux machines.
`NVs.Probe.Tests\Dockerfile` was created to facilitate execution of Linux-specific tests on Windows hosts.
Command ` docker build --no-cache -f .\NVs.Probe.Tests\Dockerfile .` executed from solution root will build this container. Execution of Linux-specific tests is a part of the build.
