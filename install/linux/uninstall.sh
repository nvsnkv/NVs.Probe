systemctl stop probe.service
systemctl disable probe.service

rm -f /usr/local/bin/probe
rm /lib/systemd/system/probe.service