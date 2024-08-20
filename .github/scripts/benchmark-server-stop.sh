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

function print() {
  echo ""
  echo "$*"
}

# parameter setup
binary_name="PerformanceTest.Server"

# stop process
print "# Stopping process $binary_name if exists"
ps -eo pid,cmd | while read -r pid cmd; do
  if echo "$cmd" | grep -E "^./$binary_name" >/dev/null 2>&1; then
    echo "Found & killing process $pid ($cmd)"
    kill "$pid"
  fi
done
