#!/bin/sh
#Compiles the Visual Studio solution.
cd `dirname $0`

# Handle Windows-style paths in project files
export MONO_IOMAP=all

xbuild ZeroInstall_xbuild.sln
