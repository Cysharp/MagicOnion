#!/bin/bash
set -euo pipefail

# git clone and run benchmark over ssh
#
# MagicOnion Server
# $ ssh -o StrictHostKeyChecking=accept-new -i ~/.ssh/id_ed25519 azure-user@20.188.7.19 'bash -s -- --branch main --build-csproj "perf/BenchmarkApp/PerformanceTest.Server/PerformanceTest.Server.csproj" --env-settings "Kestrel__EndpointDefaults__Protocols=Http2;Kestrel__Endpoints__Grpc__Url=http://+:5000;" --owner Cysharp --repo MagicOnion' < ./scripts/run_benchmark.sh
# $ echo $?
#
# MagicOnion Client
# $ ssh -o StrictHostKeyChecking=accept-new -i ~/.ssh/id_ed25519 azure-user@20.188.1.54 'bash -s -- --args "-u http://benchmark-server-vm:5000 -s streaminghub --channels 1 --streams 1" --branch main --build-csproj "perf/BenchmarkApp/PerformanceTest.Client/PerformanceTest.Client.csproj" --owner Cysharp --repo MagicOnion' < ./scripts/run_benchmark.sh
# $ echo $?

function usage {
    echo "usage: $(basename $0) --build-csproj <string> --repo <string> [options]"
    echo "Required:"
    echo "  --build-csproj  string  Path to the csproj file to build"
    echo "  --repo          string  Repository name to clone"
    echo "Options:"
    echo "  --args          string  Arguments to pass when running the built binary (default: \"\")"
    echo "  --branch        string  Branch name to checkout (default: main)"
    echo "  --build-config  string  Build configuration (default: Release)"
    echo "  --env-settings  string  Environment settings to set before running the binary, use ; to delinate multiple environments (default: \"\")"
    echo "  --owner         string  Repository owner (default: Cysharp)"
    echo "  --help                  Show this help message"
    echo ""
    echo "Examples:"
    echo "  1. Run MagicOnion Benchmark Server over ssh"
    echo "    ssh -o StrictHostKeyChecking=accept-new -i ~/.ssh/id_ed25519 azure-user@255.255.255.255 'bash -s -- --branch main --build-csproj 'perf/BenchmarkApp/PerformanceTest.Server/PerformanceTest.Server.csproj' --env-settings \"Kestrel__EndpointDefaults__Protocols=Http2;Kestrel__Endpoints__Grpc__Url=http://+:5000;\" --owner Cysharp --repo MagicOnion' < ./scripts/$(basename $0).sh"
    echo '    echo $? # use $? to get the exit code of the remote command'
    echo ""
    echo "  2. Run MagicOnion Benchmark Client over ssh"
    echo "    ssh -o StrictHostKeyChecking=accept-new -i ~/.ssh/id_ed25519 azure-user@255.255.255.255 'bash -s -- --args \"-u http://benchmark-server-vm:5000 -s streaminghub --channels 1 --streams 1\" --branch main --build-csproj 'perf/BenchmarkApp/PerformanceTest.Client/PerformanceTest.Client.csproj' --owner Cysharp --repo MagicOnion' < ./scripts/$(basename $0).sh"
    echo '    echo $? # use $? to get the exit code of the remote command'
}

while [ $# -gt 0 ]; do
  case $1 in
    # required
    --build-csproj) _BUILD_CSPROJ=$2; shift 2; ;;
    --repo) _REPO=$2; shift 2; ;;
    # optional
    --args) _ARGS=$2; shift 2; ;;
    --branch) _BRANCH=$2; shift 2; ;;
    --build-config) _BUILD_CONFIG=$2; shift 2; ;;
    --env-settings) _ENV_SETTINGS=$2; shift 2; ;;
    --owner) _OWNER=$2; shift 2; ;;
    --help) usage; exit 1; ;;
    *) shift ;;
  esac
done

function print() {
  echo ""
  echo "$*"
}

# parameter setup
args="${_ARGS:=""}"
owner="${_OWNER:="Cysharp"}"
repo="${_REPO}"
branch="${_BRANCH:="main"}"
env_settings="${_ENV_SETTINGS:=""}"
build_config="${_BUILD_CONFIG:="Release"}"
build_csproj="${_BUILD_CSPROJ}"

binary_name=$(basename "$(dirname "$build_csproj")")
publish_dir="artifacts/$binary_name"
clone_path="$HOME/github/$repo"
full_process_path="$clone_path/$publish_dir/$binary_name"

# show machine name
print "MACHINE_NAME: $(hostname)"

# is dotnet installed?
print "# Show installed dotnet sdk versions"
echo "dotnet sdk versions (list): $(dotnet --list-sdks)"
echo "dotnet sdk version (default): $(dotnet --version)"

# setup env
print "# Setup environment"
IFS=';' read -ra env_array <<< "$env_settings"
for item in "${env_array[@]}"; do
  if [ -n "$item" ]; then
      export "$item"
  fi
done
export

# git clone cysharp repo
print "# git clone $owner/$repo"
mkdir -p "$(dirname "$clone_path")"
if [[ ! -d "$clone_path" ]]; then
  git clone "https://github.com/$owner/$repo" "$clone_path"
fi

# list files
print "# List cloned files"
ls "$clone_path"

# git pull
print "# git pull $branch"
pushd "$clone_path"
  git switch "$branch"
  git pull
  git reset --hard HEAD
popd

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
if pgrep -f "$full_process_path"; then
  pkill -f "$full_process_path" || true
fi

# run dotnet app
print "# Run $full_process_path"
"$full_process_path" $args
