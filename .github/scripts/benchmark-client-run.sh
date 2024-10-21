#!/bin/bash
set -euo pipefail

# benchmark over ssh
#
# MagicOnion Client
# $ ssh -o StrictHostKeyChecking=accept-new -i ~/.ssh/id_ed25519 azure-user@4.215.237.255 'bash -s -- --args "-u http://${BENCHMARK_SERVER_NAME}:5000 -s streaminghub --channels 1 --streams 1"' < ./scripts/benchmark-client-run.sh
# $ echo $?

function usage {
  cat <<EOF
usage: $(basename $0) --run-args <string> [options]
Required:
  --run-args          string      Arguments to pass when running the built binary (default: \"\")
Options:
  --build-args        string      Arguments to pass when building the project (default: \"\")
  --help                          Show this help message
EOF
}

while [ $# -gt 0 ]; do
  case $1 in
    # required
    --run-args) _RUN_ARGS=$2; shift 2; ;;
    # optional
    --build-args) _BUILD_ARGS=$2; shift 2; ;;
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
function error {
  echo "$(date "+%Y-%m-%d %H:%M:%S") ERROR(${FUNCNAME[1]:-unknown}): # $*" >&2
}

# parameter setup
title "Arguments:"
print "--run-args=${_RUN_ARGS:=""}"
print "--build-args=${_BUILD_ARGS:=""}"

title "Constants:"
print "  * repo=${repo:="MagicOnion"}"
print "  * build_config=${build_config:="Release"}"
print "  * build_csproj=${build_csproj:="perf/BenchmarkApp/PerformanceTest.Client/PerformanceTest.Client.csproj"}"
print "  * env_settings=${env_settings:=""}"
print "  * binary_name=${binary_name:=$(basename "$(dirname "$build_csproj")")}"
print "  * publish_dir=${publish_dir:="artifacts/$binary_name"}"
print "  * clone_path=${clone_path:="$HOME/github/$repo"}"
print "  * output_dir=${output_dir:="$clone_path/$publish_dir"}"
print "  * full_process_path=${full_process_path:="$output_dir/$binary_name"}"

# machine name
title "Show this machine name"
print "  * MACHINE_NAME=$(hostname)"

# is dotnet installed?
title "Show installed dotnet sdk versions"
print "  * dotnet sdk versions (list): $(dotnet --list-sdks)"
print "  * dotnet sdk version (default): $(dotnet --version)"

echo "::group::Is already cloned?"
  title "Check if already cloned $repo"
  if [[ ! -d "$clone_path" ]]; then
    error "Failed to find $clone_path, not yet git cloned?"
    exit 1
  fi
echo "::endgroup::"

echo "::group::Get branch name and set to environment variables"
  title "Set branch name as Environment variable"
  pushd "$clone_path" > /dev/null
    print "Get current git branch name"
    git_branch=$(git rev-parse --abbrev-ref HEAD)
    if [ -z "$git_branch" ]; then
      error "Failed to get branch name, exiting..."
      exit 1
    fi

    print "Set branch name to environment variables $git_branch"
    export BRANCH_NAME="$git_branch"
  popd > /dev/null
echo "::endgroup::"

echo "::group::Setup environment variables"
  title "Setup environment"
  IFS=';' read -ra env_array <<< "$env_settings"
  for item in "${env_array[@]}"; do
    if [[ -n "$item" ]]; then
      export "${item?}"
    fi
  done
echo "::endgroup::"

echo "::group::Kill existing process"
  title "Checking process $binary_name already runnning, kill if exists"
  ps -eo pid,cmd | while read -r pid cmd; do
    if echo "$cmd" | grep -E "^./$binary_name" >/dev/null 2>&1; then
      print "Found & killing process $pid ($cmd)"
      kill "$pid"
    fi
  done
echo "::endgroup::"

echo "::group::dotnet publish"
  title "dotnet publish $build_csproj"
  pushd "$clone_path" > /dev/null
    print "List current files under $(pwd)"
    ls -l

    print "Remove all files under $publish_dir"
    if [[ -d "./$publish_dir" ]]; then
      rm -rf "./$publish_dir"
    fi

    print "dotnet publish $build_csproj $_BUILD_ARGS"
    dotnet publish -c "$build_config" -p:PublishSingleFile=true --runtime linux-x64 --self-contained false "$build_csproj" -o "$publish_dir" $_BUILD_ARGS

    print "List published files under $publish_dir"
    ls "$publish_dir"

    print "Add +x permission to published file $full_process_path"
    chmod +x "$full_process_path"
  popd > /dev/null
echo "::endgroup::"

# run dotnet app
title "Run $full_process_path $_RUN_ARGS"
pushd "$output_dir" > /dev/null
  # run foreground
  "./$binary_name" $_RUN_ARGS
popd > /dev/null
