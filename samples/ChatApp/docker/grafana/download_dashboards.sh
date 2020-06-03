#!/usr/bin/env sh
set -euf
curl -skf \
--connect-timeout 60 \
--max-time 60 \
-H "Accept: application/json" \
-H "Content-Type: application/json;charset=UTF-8" \
    "https://grafana.com/api/dashboards/10584/revisions/7/download" | sed 's/"datasource":[^,]*/"datasource": "Prometheus"/g'\
> "/var/tmp/grafana/dashboards/default/magiconion-overview.json"