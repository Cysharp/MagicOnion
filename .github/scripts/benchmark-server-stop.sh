#!/bin/bash
set -euo pipefail

# stop server over ssh
#
# MagicOnion Server
# ssh -o StrictHostKeyChecking=accept-new -i ~/.ssh/id_ed25519 azure-user@4.215.238.2 'bash -s -- ' < ./scripts/benchmark-server-stop.sh
# $ echo $?

function usage {
    echo "usage: $(basename $0) [options]"
    echo "Options:"
    echo "  --help                      Show this help message"
}

while [ $# -gt 0 ]; do
  case $1 in
    # optional
    --help) usage; exit 1; ;;
    *) shift ;;
  esac
done

function print {
  echo "$(date "+%Y-%m-%d %H:%M:%S") INFO(${FUNCNAME[1]:-unknown}): $*"
}
function title {
  echo ""
  echo "$(date "+%Y-%m-%d %H:%M:%S") INFO(${FUNCNAME[1]:-unknown}): # $*"
}

# parameter setup
title "Constants:"
print "  * binary_name=${binary_name:=PerformanceTest.Server}"

# stop process
title "Stopping process $binary_name if exists"
ps -eo pid,cmd | while read -r pid cmd; do
  if echo "$cmd" | grep -E "^./$binary_name" >/dev/null 2>&1; then
    print "Found & killing process $pid ($cmd)"
    kill "$pid"
  fi
done
