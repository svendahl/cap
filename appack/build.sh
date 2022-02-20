#!/bin/sh
dotnet publish -p:PublishSingleFile=true -r linux-x64 -c Release
