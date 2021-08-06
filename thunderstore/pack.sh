#!/bin/bash
shopt -s extglob

MANIFEST=manifest.json
version=$(dotnet minver --tag-prefix v | sed 's/\([0-9.]*\).*/\1/')

cd $(dirname $0)
mkdir -p out

pushd raw
mkfifo "$MANIFEST"

jq --compact-output --arg version "$version" '.version_number = $version' "../$MANIFEST" > "$MANIFEST" &
zip -FS9r --fifo "../out/Stratum-Stratum-$version.zip" .

rm "$MANIFEST"
popd
