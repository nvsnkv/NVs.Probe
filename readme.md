# Probe
A simple .net core app that collects and publishes metrics to MQTT server.
## Usage
1. Set up MQTT Connection settings, polling interval and shell using environment variables:
```
#!/bin/sh
export "PROBE::MQTT::ClientID" = "my_device"
export "PROBE::MQTT::Server" = "127.0.0.1"
export "PROBE::MQTT::Port" = "1883"
export "PROBE::MQTT::User" = "user"
export "PROBE::MQTT::User" = "no idea"
export "PROBE::PollingInterval" = "00:01:00"
export "PROBE::Shell" = "/bin/sh"
```
1. Start the probe any provide the list of metrics to collect:
```
#!/bin/sh
dotnet run probe.dll \
  "cpu_load" "cat /proc/loadavg | awk '{print $1}'" \
  "mem" "cat /proc/meminfo | grep MemFree | awk '{print $2}'"
```
