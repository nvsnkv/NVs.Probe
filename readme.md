# Probe
A simple .net core app that collects and publishes metrics to MQTT server.
## Usage
```
#!/bin/sh
dotnet run probe.dll -c probe -s 127.0.0.1 -u Vasya -p "no idea!" -- \
  "cpu_load" "cat /proc/loadavg | awk '{print $1}'" \
  "mem" "cat /proc/meminfo | grep MemFree | awk '{print $2}'"
```
### Options
`dotnet run probe.dll --help` will list available configuration options:
```
  -c, --mqtt-client-id                  Required. MQTT Client Identifier

  -b, --mqtt-broker-host                Required. The hostname or IP address of
                                        MQTT broker

  -u, --mqtt-user                       Required. Username used for
                                        authentication on MQTT broker

  -p, --mqtt-password                   Required. Password used for
                                        authentication on MQTT broker

  --mqtt-broker-port                    (Default: 1883) Port number of MQTT
                                        broker

  --mqtt-broker-reconnect-attempts      (Default: 0) Count of attempts to
                                        reconnect to MQTT broker in case of
                                        broken connection

  --mqtt-broker-reconnect-interval      (Default: 5000) Base interval between
                                        attempts to reconnect to MQTT broker

  --measurement-timeout                 (Default: 1000) Timeout of a single
                                        measurement

  --measurement-series-interval         (Default: 120000) Base interval between
                                        measurement series

  --help                                Display this help screen.

  --version                             Display version information.

  Measurements configuration (pos. 0)    Required. A series of topic-command
                                        pairs, like 'dotnet/version' 'dotnet --
                                        version'
```
## Logging
Applicaion uses [Serilog](https://serilog.net/) to produce logs. Logging configuration is set up in `appsettings.json` file.