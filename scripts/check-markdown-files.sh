#!/bin/bash

EXCLUDED_DIRECTORIES="./docs|./samples|.github"
REPO_README="./README.md"
CONTRIBUTING_README="./CONTRIBUTING.md"

# Count all readme files in repo not in docs, excluding certain directories, the main repo readme, and the contributing readme
COUNT_OUT_OF_PLACE_FILES=$(find -iname "*md" ! -iwholename $REPO_README ! -iwholename $CONTRIBUTING_README | egrep -v $EXCLUDED_DIRECTORIES | wc -l)

if [ $COUNT_OUT_OF_PLACE_FILES -ge 1 ];
then
    printf "README files should be located in docs folder. Found $COUNT_OUT_OF_PLACE_FILES '*.md' file(s) not located in the docs directory.\n"
    find -iname "*md" ! -iwholename $REPO_README ! -iwholename $CONTRIBUTING_README | egrep -v $EXCLUDED_DIRECTORIES
    exit 1
fi
