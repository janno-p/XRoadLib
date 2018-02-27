#!/usr/bin/env bash

set -eu
set -o pipefail

cd `dirname $0`

PAKET_EXE=.paket/paket.exe
FAKE_EXE=packages/FAKE/tools/FAKE.exe

FSIARGS=""
OS=${OS:-"unknown"}
if [[ "$OS" != "Windows_NT" ]]
then
  FSIARGS="--fsiargs -d:MONO"
fi

function run() {
  if [[ "$OS" != "Windows_NT" ]]
  then
    mono "$@"
  else
    "$@"
  fi
}

if [[ "$OS" != "Windows_NT" ]] &&
       [ ! -e ~/.config/.mono/certs ]
then
  mozroots --import --sync --quiet
fi

run $PAKET_EXE restore
run $FAKE_EXE "$@" $FSIARGS build.fsx

