mkdir -p /usr/local/bin/probe
\cp ./Probe /usr/local/bin/probe/probe
\cp ./probe.settings.yaml /usr/local/bin/probe/
\cp ./probe.settings.logging.yaml /usr/local/bin/probe/

chmod +x /usr/local/bin/probe/probe
chown -R $1 /usr/local/bin/probe 

cp ./probe.service /lib/systemd/system
systemctl start probe.service
systemctl enable probe.service