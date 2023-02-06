#!/bin/bash

# Script to check for any markdown files located outside of docs directory. 
# Includes exceptions for certain directories, the main repo readme, and the contributing readme. 
# If any files found, lists them out and exit 1.

DOCS_DIRECTORY="./docs"
EXCLUDED_DIRECTORIES="./samples|.github"
REPO_README="./README.md"
CONTRIBUTING_README="./CONTRIBUTING.md"

# Count all out of place files excluding exceptions
COUNT_OF_OUT_OF_PLACE_FILES=$(find -iname "*md" ! -iwholename $REPO_README ! -iwholename $CONTRIBUTING_README | egrep -v $DOCS_DIRECTORY || egrep -v $EXCLUDED_DIRECTORIES | wc -l)

# If any out of place files, exit 1
if [ $COUNT_OF_OUT_OF_PLACE_FILES -ge 1 ]; then
    printf "README files should be located in docs folder. Found $COUNT_OF_OUT_OF_PLACE_FILES '*.md' file(s) not located in the docs directory.\n"
    find -iname "*md" ! -iwholename $REPO_README ! -iwholename $CONTRIBUTING_README | egrep -v $EXCLUDED_DIRECTORIES
    exit 1
fi
