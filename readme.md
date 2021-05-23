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
#### Updating configuration files
Probe reads several configuration files to retrieve the setup:
* `probe.metrics.yaml` used to set up a list of metrics that Probe will collect
* `probe.mqtt.yaml` used to set up connection to MQTT broker
* `probe.serilog.logging.yaml` defines logging configuration

Having several configuration files allows to share connection information or logging settings between different instances of application. 
MQTT connection setup contains password, access to this file should be properly restricted.

#### CLI
Application can be started as a regular process from command line:
```
#!/bin/sh
probe -i sh -f '-c' -c probe.metrics.yaml -m probe.mqtt.yaml
```
Probe also works with Windows, both Powershell and CMD are supported.
Application can use any other app that reads command line arguments and returns output into STDOUT to measure the metrics.

## Configuration
### Command line options
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
### Metrics configuration (probe.metrics.yaml)
Probe uses YAML configuration files for metrics. Example can be found below:
```
- topic: topic/cpu_load 
  command: "cat /proc/loadavg | awk '{print $1}'"
- topic: topic/mem_free 
  command: "cat /proc/meminfo | grep MemFree | awk '{print $2}'" 
```
#### Metric definition
* `topic` - string, required. Defines MQTT topic that will be used to publish values;
* `command` - string, required. Defines CLI command that Probe will run to measure metric value.

### MQTT Connection setup (probe.mqtt.yaml)
Another YAML file is used to define MQTT connection options:
```
client_id: probe
broker: localhost
user: Vasya
password: "no clue"
retries_interval: "00:00:30"
retries_count: 5
```
Following parameters are required:
* `client_id` - MQTT Client Identifier;
* `broker` - the hostname or IP address of MQTT broker;
* `user` - username used for authentication on MQTT broker;
* `password` - password used for authentication on MQTT broker.

Following parameters are optional:
* `port` - port number of MQTT broker. Default is 1883;
* `retries_count` - count of attempts to reconnect to MQTT broker in case of connectivity issues. Application would not try to reconnect if this parameter is not provided. `retries_interval` must be provided together with this setting. If defined, the value should be positive;
* `retries_interval` - Base interval between attempts to reconnect to MQTT broker. Must be defined if `retries_count` is set. Must not be defined otherwise.

### Logging
Applicaion uses [Serilog](https://serilog.net/) to produce logs. Logging configuration is set up in `probe.serilog.logging.yaml` file.

## Development notes
### Automated tests
There are few tests that helped me to write this tool.
Some of them tests .Net components and can be started from any supported runtime.
Tests with "Category" equals to "Windows" will work on Windows hosts. Tests with "Linux" "Category" works on linux machines.
`NVs.Probe.Tests\Dockerfile` was created to facilitate execution of Linux-specific tests on Windows hosts.
Command ` docker build --no-cache -f .\NVs.Probe.Tests\Dockerfile .` executed from solution root will build this container. Execution of Linux-specific tests is a part of the build.
