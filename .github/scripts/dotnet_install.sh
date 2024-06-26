#!/bin/bash
set -euo pipefail

# Install .NET SDK over ssh
#
# Sample usage:
# $ ssh -o StrictHostKeyChecking=accept-new -i ~/.ssh/id_ed25519 azure-user@20.188.7.19 'bash -s -- --dotnet-version 8.0' < ./scripts/dotnet_install.sh
# $ ssh -o StrictHostKeyChecking=accept-new -i ~/.ssh/id_ed25519 azure-user@20.188.1.54 'bash -s -- --dotnet-version 8.0' < ./scripts/dotnet_install.sh
# $ echo $?
#

function usage {
    echo "usage: $(basename $0) [options]"
    echo "Options:"
    echo "  --dotnet-version string  Version of dotnet sdk to install (default: 8.0)"
    echo "  --help                   Show this help message"
    echo ""
    echo "Examples:"
    echo "  1. Install dotnet sdk version 8.0 over ssh"
    echo "    ssh -o StrictHostKeyChecking=accept-new -i ~/.ssh/id_ed25519 azure-user@255.255.255.255 'bash -s -- --dotnet-version 8.0' < ./scripts/$(basename $0).sh"
    echo '    echo $? # use $? to get the exit code of the remote command'
}

while [ $# -gt 0 ]; do
  case $1 in
    # optional
    --dotnet-version) _DOTNET_VERSION=$2; shift 2; ;;
    --help) usage; exit 1; ;;
    *) shift ;;
  esac
done

function print() {
  echo ""
  echo "$*"
}

dotnet_version="${_DOTNET_VERSION:="8.0"}"

# show machine name
print "MACHINE_NAME: $(hostname)"

# install dotnet (dotnet-install.sh must be downloaded before running script)
print "# Install dotnet sdk version: ${dotnet_version}"
sudo bash /opt/dotnet-install.sh --channel "${dotnet_version}" --install-dir /usr/share/dotnet

# link dotnet to /usr/local/bin
print "# Link to /usr/local/bin/dotnet"
if [[ ! -h "/usr/local/bin/dotnet" ]]; then
  sudo ln -s /usr/share/dotnet/dotnet /usr/local/bin/dotnet
fi

# show dotnet verison
print "# Show installed dotnet sdk versions"
echo "dotnet sdk versions (list): $(dotnet --list-sdks)"
echo "dotnet sdk version (default): $(dotnet --version)"
