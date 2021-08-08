#!/bin/bash
shopt -s extglob

MANIFEST=manifest.json
cd $(dirname $0)

raw=$(jq -r '.author, .name' "$MANIFEST")
author=$(echo "$raw" | sed -n '1 p')
name=$(echo "$raw" | sed -n '2 p')
version=$(dotnet minver --tag-prefix v | sed 's/\([0-9.]*\).*/\1/')

mkdir -p out

pushd raw
mkfifo "$MANIFEST"

jq --compact-output --arg version "$version" '.version_number = $version' "../$MANIFEST" > "$MANIFEST" &
zip -FS9r --fifo "../out/$author-$name-$version.zip" .

rm "$MANIFEST"
popd
