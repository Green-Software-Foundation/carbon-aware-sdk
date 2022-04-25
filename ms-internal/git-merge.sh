#!/bin/bash

localRepo="https://github.com/microsoft/carbon-aware-sdk"
upstreamRepo="https://github.com/Green-Software-Foundation/carbon-aware-sdk"
commitmessage="Warning: git push of upstream contents failed, saving error info into an empty-PR for investigation"
status=0

git config user.name "GitHub Actions Bot"
git config user.email "<>"

# upstream contents needed for creating the PR
git remote add upstream $upstreamRepo
git fetch upstream
git checkout -b $1 upstream/dev 

# git push, capturing output text and exit code
echo "Creating a PR with fetched-contents from upstream:dev into origin:dev."
GIT_PUSH_OUTPUT=$(git push --set-upstream origin $1 2>&1) # without 'workflow' permission this fails if changes to .github\workflow\*.yml file(s) are present
status=$?
echo $GIT_PUSH_OUTPUT

# 'Failure-to-Launch' if git push fails -- create and empty-PR with the error details for manual resolution
if [ $status -ne 0 ]; then
    echo $commitmessage

    # 1. re-create branch from origin:dev
    git checkout -f origin/dev
    git branch -D $1
    git fetch origin
    git checkout -b $1 origin/dev

    # 2. add + commit the error details
    echo $commitmessage > README-$1.md
    echo "-------------" >> README-$1.md
    echo $GIT_PUSH_OUTPUT >> README-$1.md
    git add README-$1.md
    git commit -m "$commitmessage"

    # 3. push to origin
    git push --set-upstream origin $1         
    status=$?
fi

# tell next pipeline step if push failed
exit $status
