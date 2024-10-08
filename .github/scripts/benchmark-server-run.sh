#!/bin/bash
set -euo pipefail

# benchmark over ssh
#
# MagicOnion Server
# ssh -o StrictHostKeyChecking=accept-new -i ~/.ssh/id_ed25519 azure-user@4.215.238.2 'bash -s -- ' < ./scripts/benchmark-server-run.sh
# $ echo $?

function usage {
  cat <<EOF
usage: $(basename $0) --run-args <string> [options]
Required:
  --run-args          string      Arguments to pass when running the built binary (default: \"\")
Options:
  --help                          Show this help message
EOF
}

while [ $# -gt 0 ]; do
  case $1 in
    # required
    --run-args) _RUN_ARGS=$2; shift 2; ;;
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
function error {
  echo "$(date "+%Y-%m-%d %H:%M:%S") ERROR(${FUNCNAME[1]:-unknown}): # $*" >&2
}
# parameter setup
title "Arguments:"
print "--run-args=${_RUN_ARGS:=""}"

title "Constants:"
print "  * repo=${repo:="MagicOnion"}"
print "  * build_config=${build_config:="Release"}"
print "  * build_csproj=${build_csproj:="perf/BenchmarkApp/PerformanceTest.Server/PerformanceTest.Server.csproj"}"
print "  * env_settings=${env_settings:=""}"
print "  * binary_name=${binary_name:=$(basename "$(dirname "$build_csproj")")}"
print "  * publish_dir=${publish_dir:="artifacts/$binary_name"}"
print "  * clone_path=${clone_path:="$HOME/github/$repo"}"
print "  * output_dir=${output_dir:="$clone_path/$publish_dir"}"
print "  * full_process_path=${full_process_path:="$output_dir/$binary_name"}"
print "  * stdoutfile=${stdoutfile:="stdout.log"}"
print "  * stderrfile=${stderrfile:="stderr.log"}"

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
    if [ -n "$item" ]; then
      export "$item"
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

    print "dotnet publish $build_csproj"
    dotnet publish -c "$build_config" -p:PublishSingleFile=true --runtime linux-x64 --self-contained false "$build_csproj" -o "$publish_dir"

    print "List published files under $publish_dir"
    ls "$publish_dir"

    print "Add +x permission to published file $full_process_path"
    chmod +x "$full_process_path"
  popd > /dev/null
echo "::endgroup::"

# run dotnet app
title "Run $full_process_path $_RUN_ARGS"
pushd "$output_dir" > /dev/null
  touch "${stdoutfile}"
  # use nohup to run background https://stackoverflow.com/questions/29142/getting-ssh-to-execute-a-command-in-the-background-on-target-machine
  # shellcheck disable=SC2086
  nohup "./$binary_name" $_RUN_ARGS > "${stdoutfile}" 2> "${stderrfile}" < /dev/null &

  # wait 10s will be enough to start the server or not
  sleep 10s

  # output stdout
  cat "${stdoutfile}"

  # output stderr
  if [[ "$(stat -c%s "$stderrfile")" -ne "0" ]]; then
    error "Error found when running the server."
    cat "${stderrfile}"
    exit 1
  fi
popd > /dev/null
