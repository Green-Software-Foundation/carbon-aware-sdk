#!/bin/bash

set -e

BRANCH=${1:-"auto-pr-$RANDOM"}
BASE=${2:-"dev"}
UPSTREAM=${3:-"Green-Software-Foundation/carbon-aware-sdk"}

UPSTREAMREPO="https://github.com/$UPSTREAM"

git config user.name "GitHub Actions Bot"
git config user.email "<>"

# upstream contents needed for creating the PR
set +e # don't fail script if upstream exists
git remote add upstream $UPSTREAMREPO
set -e
git fetch upstream
git checkout -b $BRANCH upstream/$BASE 

# git push, capturing output text and exit code
echo "Creating a PR with fetched-contents from upstream:$BASE into origin:$BASE."
set +e # don't fail script if git push fails, since we want to continue
GIT_PUSH_OUTPUT=$(git push --set-upstream origin $BRANCH 2>&1) # without 'workflow' permission this fails if changes to .github\workflow\*.yml file(s) are present
STATUS=$?
set -e
echo $GIT_PUSH_OUTPUT

# 'Failure-to-Launch' if git push fails -- create and empty-PR with the error details for manual resolution
if [ $STATUS -ne 0 ]; then
    COMMITMESSAGE="Warning: git push of upstream contents failed, saving error info into an empty-PR for investigation"
    echo $COMMITMESSAGE

    # 1. re-create branch from origin:dev
    git checkout -f $BASE
    git branch -D $BRANCH
    git fetch origin
    git checkout -b $BRANCH origin/$BASE

    # 2. add + commit the error details
    echo $COMMITMESSAGE > README-$BRANCH.md
    echo "-----------------------" >> README-$BRANCH.md
    echo "------BEGIN-ERROR------" >> README-$BRANCH.md
    echo $GIT_PUSH_OUTPUT >> README-$BRANCH.md
    echo "-------END-ERROR-------" >> README-$BRANCH.md
    echo "-----------------------" >> README-$BRANCH.md
    echo "Recommend: run \"$0\" manually in your local clone of repo, and manually create a PR from the branch that gets created." >> README-$BRANCH.md
    git add README-$BRANCH.md
    git commit -m "$COMMITMESSAGE"

    # 3. push to origin
    git push --set-upstream origin $BRANCH 
fi
