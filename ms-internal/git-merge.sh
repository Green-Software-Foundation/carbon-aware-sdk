#!/bin/bash
# gh auth login
# gh auth setup-git

localRepo="https://github.com/microsoft/carbon-aware-sdk"
upstreamRepo="https://github.com/Green-Software-Foundation/carbon-aware-sdk"
commitmessage="git push of upstream contents failed, saving error info into an empty-manual-PR for investigation"
status=0

git config user.name "GitHub Actions Bot"
git config user.email "<>"

git branch -v
git remote -v

# upstream needed for merging
git remote add upstream $upstreamRepo
git fetch upstream
git checkout -b $1 upstream/dev 

# # created for upstream/dev merge-attempt
git fetch origin
git checkout -b mergetest-$1 origin/dev

# BEGIN TEST
# push and pr-create used for testing -- this push works, since has no workflow changes
echo TEST_TEST >> README.md
git add .
git commit -m "TEST_TEST"
git push -u origin mergetest-$1 
# gh pr create --title "[automation test] Pull request title" --body "[automation test] Pull request body" --repo microsoft/carbon-aware-sdk
# END TEST

# # see if merge has conflicts
# git merge upstream/dev
# status=$?


#gh pr create --title "test" --body "test" --repo microsoft/carbon-aware-sdk

# if [ $status -eq 0 ]; then
#     echo "No merge conflicts. Fetching Upstream directly."
#     gh repo sync microsoft/carbon-aware-sdk --branch dev
#     exit 0
# else
    echo "Creating a PR with fetched-contents from upstream:dev into origin:dev."
    # git merge --abort
    # git checkout $1
    GIT_PUSH_OUTPUT=$(git push --set-upstream origin $1 2>&1) # fails without workflow privilege if changes to .github\workflow\*.yml file(s) are present
    status=$?
    echo "BEGIN TEST CAPTURE git-push OUTPUT"
    echo $GIT_PUSH_OUTPUT
    echo "END TEST CAPTURE git-push OUTPUT"
    if [ $status -ne 0 ]; then
        echo $commitmessage

        # 1. re-create branch
        git branch -D $1
        git fetch origin
        git checkout -b $1 origin/dev

        # 2. add + commit
        echo commitmessage > README-$1.md
        echo "" > README-$1.md
        echo $GIT_PUSH_OUTPUT >> README-$1.md
        git add README-$1.md
        git commit -m commitmessage

        # 3. push it
        git push -u origin $1         
        status=$?
    fi
    # gh pr create -f
    # gh pr create --title "[automation test] Pull request title" --body "[automation test] Pull request body" --repo microsoft/carbon-aware-sdk
    exit $status
# fi