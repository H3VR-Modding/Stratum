#!/bin/bash
final=$1
if [[ -z "$final" ]]; then
	echo Final destination required
	exit 1
fi

tmp=$(mktemp -d)
plugins=$tmp/plugins
dest=$tmp/dest.zip

mkdir -p "$plugins"

cp README.md Stratum/manifest.json "$tmp/"
cp media/icon/256.png "$tmp/icon.png"
cp Stratum/bin/Release/net35/Stratum.dll "$plugins/"

pushd "$tmp"
zip -9r "$dest" .
popd

mv "$dest" "$final"

rm -rf "$tmp"
