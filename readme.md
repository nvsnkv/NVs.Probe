# Probe
A simple .net core app that collects and publishes command line based metrics to MQTT server.
## Usage
The example below demonstrates how to start collecting CPU load and available memory and publish them to MQTT broker hosted on the same machine
```
#!/bin/sh
./probe -c probe -s 127.0.0.1 -u Vasya -p "no idea!" -i sh -f '-c' -- \
  "cpu_load" "cat /proc/loadavg | awk '{print $1}'" \
  "mem" "cat /proc/meminfo | grep MemFree | awk '{print $2}'"
```
### Options
`dotnet run probe.dll --help` will list available configuration options:
```
   -c, --mqtt-client-id                   Required. MQTT Client Identifier

  -b, --mqtt-broker-host                 Required. The hostname or IP address of
                                         MQTT broker

  -u, --mqtt-user                        Required. Username used for
                                         authentication on MQTT broker

  -p, --mqtt-password                    Required. Password used for
                                         authentication on MQTT broker

  -i, --interpreter                      Required. Interpreter used to execute
                                         commands

  -f, --interpreter-flags                Required. Interpreter flags

  --mqtt-broker-port                     (Default: 1883) Port number of MQTT
                                         broker

  --mqtt-broker-reconnect-attempts       (Default: 0) Count of attempts to
                                         reconnect to MQTT broker in case of
                                         broken connection

  --mqtt-broker-reconnect-interval       (Default: 5000) Base interval between
                                         attempts to reconnect to MQTT broker

  --measurement-timeout                  (Default: 1000) Timeout of a single
                                         measurement

  --measurement-series-interval          (Default: 120000) Base interval between
                                         measurement series

  --help                                 Display this help screen.

  --version                              Display version information.

  Measurements configuration (pos. 0)    Required. A series of topic-command
                                         pairs, like 'dotnet/version' 'dotnet --
                                         version'
```
## Logging
Applicaion uses [Serilog](https://serilog.net/) to produce logs. Logging configuration is set up in `probe.settings.json` file.

## Building and deployment
Publish profiles in NVs.Probe project (`.\NVs.Probe\Properties\PublishProfiles`) allows to create single-file executables for windows (x64) and linux (arm).
Both profiles publishes application to `.\publish` folder.

Application is runtime-agnostic by itself, so application can be compiled for any runtime supported by .Net Core

## Automated tests
There are few tests that helped me to write this tool.
Some of them tests .Net components and can be started from any supported runtime.
Tests with "Category" equals to "Windows" will work on Windows hosts. Tests with "Linux" "Category" works on linux machines.
`NVs.Probe.Tests\Dockerfile` was created to facilitate execution of Linux-specific tests on Windows hosts.
Command ` docker build --no-cache -f .\NVs.Probe.Tests\Dockerfile .` executed from solution root will build this container. Execution of Linux-specific tests is a part of the build.
