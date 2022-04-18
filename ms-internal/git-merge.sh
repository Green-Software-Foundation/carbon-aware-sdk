#!/bin/bash

upstreamRepo="https://github.com/Green-Software-Foundation/carbon-aware-sdk"

git remote add upstream $upstreamRepo

git checkout -b $1

git fetch upstream
git merge upstream/dev

status=$?

if [ $status -eq 0 ]; then
    echo "No merge conflicts. Opening PR against the new branch."
    git push --set-upstream origin upstream-pr-${GITHUB_RUN_ID}
    exit 0
else
    echo "Merge Conflicts are preventing auto-merging."
    exit 1
fi