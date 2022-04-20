#!/bin/bash

localRepo="https://github.com/microsoft/carbon-aware-sdk"
upstreamRepo="https://github.com/Green-Software-Foundation/carbon-aware-sdk"

git config user.name "GitHub Actions Bot"
git config user.email "<>"

git branch -v

git remote -v

git fetch origin
git checkout -b origin-$1 origin/dev

git remote add upstream $upstreamRepo
git fetch upstream
git checkout -b $1 upstream/dev

# git push -u origin $1

gh auth login
gh repo sync microsoft/carbon-aware-sdk
# gh repo sync

git status

#gh pr create --title "test" --body "test" --repo microsoft/carbon-aware-sdk

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