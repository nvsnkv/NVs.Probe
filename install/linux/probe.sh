./probe -c '' -b '' -u '' -p '' -i sh --interpeter-flags=-c \
topic/cpu_load "cat /proc/loadavg | awk '{print \$1}'" \
topic/mem_free "cat /proc/meminfo | grep MemFree | awk '{print \$2}'" \
topic/cpu_temp "/opt/vc/bin/vcgencmd measure_temp | awk -F= '{print \$2}' | awk -F\' '{print \$1}'"