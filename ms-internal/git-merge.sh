#!/bin/bash

upstreamRepo="https://github.com/Green-Software-Foundation/carbon-aware-sdk"

git config user.name "GitHub Actions Bot"
git config user.email "<>"

git remote add upstream $upstreamRepo
git fetch upstream
git checkout -b upstream-dev-$1 upstream/dev
git push  --set-upstream origin upstream-dev-$1
gh pr create -f

# if [ $status -eq 0 ]; then
#     echo "No merge conflicts. Opening PR against the new branch."
#     git push --set-upstream origin upstream-pr-${GITHUB_RUN_ID}
#     gh pr create --title "Pull request title" --body "Pull request body"
#     exit 0
# else
#     echo "Merge Conflicts are preventing auto-merging."
#     git merge --abort
#     git checkout upstream/dev
#     gh pr create -f
#     exit 0
# fi