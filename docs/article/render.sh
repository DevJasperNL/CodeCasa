#!/usr/bin/env sh
# Renders every diagram to SVG (light/dark) and PNG (light). The ELK spacing flags have no d2-config equivalent.
cd "$(dirname "$0")"
for f in thinking-in-layers hallway light-group notifications; do
  d2 --elk-nodeNodeBetweenLayers 40 --elk-padding "[top=50,left=25,bottom=25,right=25]" "$f.d2" "$f.svg"
  d2 --elk-nodeNodeBetweenLayers 40 --elk-padding "[top=50,left=25,bottom=25,right=25]" "$f.d2" "$f.png"
done
