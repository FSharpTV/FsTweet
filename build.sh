#!/bin/bash
if test "$OS" = "Windows_NT"
then # For Windows    
    .paket/paket.exe restore
    exit_code=$?
    if [ $exit_code -ne 0 ]; then
    	exit $exit_code
    fi
    packages/FAKE/tools/FAKE.exe $@ --fsiargs build.fsx
else # For NonWindows    
    mono .paket/paket.exe restore
    exit_code=$?
    if [ $exit_code -ne 0 ]; then
    	exit $exit_code
    fi
    export MONO_MANAGED_WATCHER=false
    mono packages/FAKE/tools/FAKE.exe $@ --fsiargs -d:MONO build.fsx
fi