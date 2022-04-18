#!/bin/bash

upstreamRepo="https://github.com/Green-Software-Foundation/carbon-aware-sdk"

git config user.name "GitHub Actions Bot"
git config user.email "<>"

git checkout -b $1

git remote add upstream $upstreamRepo
git fetch upstream
git merge upstream/dev --allow-unrelated-histories

status=$?

if [ $status -eq 0 ]; then
    echo "No merge conflicts. Opening PR against the new branch."
    git push --set-upstream origin upstream-pr-${GITHUB_RUN_ID}
    exit 0
else
    echo "Merge Conflicts are preventing auto-merging."
    exit 1
fi