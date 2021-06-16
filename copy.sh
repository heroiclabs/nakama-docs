#!/bin/bash

rm -Rf ./docs/
rm mkdocs.yml

mkdir ./docs/
cp -R ../docs-centre/docs/ ./docs/
cp ../docs-centre/mkdocs.yml .
