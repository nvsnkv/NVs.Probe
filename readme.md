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
1. start the application 
#### Other runtimes
Application is runtime-agnostic by itself and can be compiled for any runtime supported by .Net Core by specifying `runtime (-r)`  argument for `dotnet build` or `dotnet publish`
### Running the app
#### systemd service creation
Installation files from `install/linux` folder contains sample service definition, installation and uninstallation script.
The following steps are required to install application as a service
1. update logging settings in `probe.settings.json` if needed
1. update `probe.sh` script:
    1. provide `--mqtt-client-id`, `--mqtt-broker-host`, `--mqtt-user`, `--mqtt-password`;
    1. update `--interpreter` and `--interpreter-flags` if needed;
    1. update measurement configuration file `probe.metrics.yaml` and check that path to the file is correct (`-m`, `metrics-setup`);
    1. provide desired optional parameters;
    1. ensure MQTT topics are correct!
1. update `probe.service` script - set user if needed;
1. review content of `install.sh` file and correct it if needed;
1. run `sudo ./install.sh my_user` to complete installation. Don't forget to replace _my_user_ with the user id from probe.service!
1. check that service is running by executing `sudo systemctl status probe.service`.
#### CLI sample (linux)
Application also can be started as a regular process from command line:
```
#!/bin/sh
./probe -c probe -s 127.0.0.1 -u Vasya -p "no idea!" -i sh -f '-c' -m probe.metrics.yaml
```
#### CLI sample (windows)
Application should work with powershell. Leave `--interpreter-flags` blank and update probe.metrics.yaml with powershell-friendly commands.
```
probe.exe -c probe -s 127.0.0.1 -u Vasya -p "no idea!" -i powershell -c '' -m -m probe.metrics.yaml
```

## Configuration
### Options
`probe --help` will list available configuration options:
```
  -c, --mqtt-client-id                Required. MQTT Client Identifier

  -b, --mqtt-broker-host              Required. The hostname or IP address of
                                      MQTT broker

  -u, --mqtt-user                     Required. Username used for authentication
                                      on MQTT broker

  -p, --mqtt-password                 Required. Password used for authentication
                                      on MQTT broker

  -i, --interpreter                   Required. Interpreter used to execute
                                      commands

  -f, --interpreter-flags             Required. Interpreter flags

  -m, --metrics-setup                 Required. Path to metrics configuration

  --mqtt-broker-port                  (Default: 1883) Port number of MQTT broker

  --mqtt-broker-reconnect-attempts    (Default: 0) Count of attempts to
                                      reconnect to MQTT broker in case of broken
                                      connection

  --mqtt-broker-reconnect-interval    (Default: 5000) Base interval between
                                      attempts to reconnect to MQTT broker

  --measurement-timeout               (Default: 1000) Timeout of a single
                                      measurement

  --measurement-series-interval       (Default: 120000) Base interval between
                                      measurement series

  --help                              Display this help screen.

  --version                           Display version information.
```
### Metrics configuration
Starting from version 0.2 Probe uses YAML configuration files for metrics. Example can be found below:
```
- topic: topic/cpu_load 
  command: "cat /proc/loadavg | awk '{print $1}'"
- topic: topic/mem_free 
  command: "cat /proc/meminfo | grep MemFree | awk '{print $2}'" 
```

#### Metric definition
* `topic` - string, required. Defines MQTT topic that will be used to publish values
* `command` - string, required. Defines CLI command that Probe will run to measure metric value.

### Logging
Applicaion uses [Serilog](https://serilog.net/) to produce logs. Logging configuration is set up in `probe.settings.json` file.

## Development notes
### Automated tests
There are few tests that helped me to write this tool.
Some of them tests .Net components and can be started from any supported runtime.
Tests with "Category" equals to "Windows" will work on Windows hosts. Tests with "Linux" "Category" works on linux machines.
`NVs.Probe.Tests\Dockerfile` was created to facilitate execution of Linux-specific tests on Windows hosts.
Command ` docker build --no-cache -f .\NVs.Probe.Tests\Dockerfile .` executed from solution root will build this container. Execution of Linux-specific tests is a part of the build.
