#!/bin/bash
set -euo pipefail

# benchmark over ssh
#
# MagicOnion Client
# $ ssh -o StrictHostKeyChecking=accept-new -i ~/.ssh/id_ed25519 azure-user@4.215.237.255 'bash -s -- --args "-u http://${BENCHMARK_SERVER_NAME}:5000 -s streaminghub --channels 1 --streams 1"' < ./scripts/benchmark-client-run.sh
# $ echo $?

function usage {
    echo "usage: $(basename $0) --args <string> [options]"
    echo "Required:"
    echo "  --args          string      Arguments to pass when running the built binary (default: \"\")"
    echo "Options:"
    echo "  --help                      Show this help message"
}

while [ $# -gt 0 ]; do
  case $1 in
    # required
    --args) _ARGS=$2; shift 2; ;;
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
repo="MagicOnion"
build_config="Release"
args="${_ARGS:=""}"
build_csproj="perf/BenchmarkApp/PerformanceTest.Client/PerformanceTest.Client.csproj"
env_settings=""

binary_name=$(basename "$(dirname "$build_csproj")")
publish_dir="artifacts/$binary_name"
clone_path="$HOME/github/$repo"
output_dir="$clone_path/$publish_dir"
full_process_path="$output_dir/$binary_name"

# show machine name
print "MACHINE_NAME: $(hostname)"

# is dotnet installed?
print "# Show installed dotnet sdk versions"
echo "dotnet sdk versions (list): $(dotnet --list-sdks)"
echo "dotnet sdk version (default): $(dotnet --version)"

# is already clones?
print "# Check if already cloned $repo"
if [[ ! -d "$clone_path" ]]; then
  echop "Failed to find $clone_path, not yet git cloned?"
  exit 1
fi

# get branch name and set to environment variables
print "# Set branch name as Environment variable"
pushd "$clone_path"
  print "  ## get current git branch name"
  git_branch=$(git rev-parse --abbrev-ref HEAD)
  if [ -z "$git_branch" ]; then
    echo "Failed to get branch name, exiting..."
    exit 1
  fi

  print "  ## set branch name to environment variables $git_branch"
  export BRANCH_NAME="$git_branch"
popd

# setup env
print "# Setup environment"
IFS=';' read -ra env_array <<< "$env_settings"
for item in "${env_array[@]}"; do
  if [[ -n "$item" ]]; then
    export "${item?}"
  fi
done

# dotnet publish
print "# dotnet publish $build_csproj"
pushd "$clone_path"
  print "  ## list current files under $(pwd)"
  ls -l

  print "  ## dotnet publish $build_csproj"
  dotnet publish -c "$build_config" -p:PublishSingleFile=true --runtime linux-x64 --self-contained false "$build_csproj" -o "$publish_dir"

  print "  ## list published files under $publish_dir"
  ls "$publish_dir"

  print "  ## add +x permission to published file $full_process_path"
  chmod +x "$full_process_path"
popd

# process check
print "# Checking process $binary_name already runnning, kill if exists"
ps -eo pid,cmd | while read -r pid cmd; do
  if echo "$cmd" | grep -E "^./$binary_name" >/dev/null 2>&1; then
    echo "Found & killing process $pid ($cmd)"
    kill "$pid"
  fi
done

# run dotnet app
print "# Run $full_process_path $args"
pushd "$output_dir"
  # run foreground
  "./$binary_name" $args
popd
