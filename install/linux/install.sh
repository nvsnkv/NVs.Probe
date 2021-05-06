mkdir -p /usr/local/bin/probe
\cp ./Probe /usr/local/bin/probe/probe
\cp ./probe.settings.json /usr/local/bin/probe/
\cp ./probe.sh /usr/local/bin/probe/

chmod +x /usr/local/bin/probe/probe
chmod 700 /usr/local/bin/probe/probe.sh
chown -R $1 /usr/local/bin/probe 

cp ./probe.service /lib/systemd/system
systemctl start probe.service
systemctl enable probe.service