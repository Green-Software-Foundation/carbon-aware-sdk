#!/bin/bash
gh auth login
gh auth setup-git

localRepo="https://github.com/microsoft/carbon-aware-sdk"
upstreamRepo="https://github.com/Green-Software-Foundation/carbon-aware-sdk"

git config user.name "GitHub Actions Bot"
git config user.email "<>"

git branch -v
git remote -v

# upstream needed for merging
git remote add upstream $upstreamRepo
git fetch upstream
git checkout -b $1 upstream/dev 

# # created for upstream/dev merge-attempt
# git fetch origin
# git checkout -b mergetest-$1 origin/dev

# # BEGIN TEST
# # push and pr-create used for testing -- this push works, since has no workflow changes
# echo TEST_TEST >> README.md
# git add .
# git commit -m "TEST_TEST"
# git push -u origin mergetest-$1 
# gh pr create --title "[automation test] Pull request title" --body "[automation test] Pull request body" --repo microsoft/carbon-aware-sdk
# # END TEST

# # see if merge has conflicts
# git merge upstream/dev
# status=$?


#gh pr create --title "test" --body "test" --repo microsoft/carbon-aware-sdk

# if [ $status -eq 0 ]; then
#     echo "No merge conflicts. Fetching Upstream directly."
#     gh repo sync microsoft/carbon-aware-sdk --branch dev
#     exit 0
# else
    echo "Merge Conflicts are preventing auto-merging. Creating a PR with upstream:dev -> origin:dev."
    # git merge --abort
    # git checkout $1
    git push --set-upstream origin $1 # fails without workflow privilege due to new .github\workflow\*.yml file(s)
    # gh pr create -f
    gh pr create --title "[automation test] Pull request title" --body "[automation test] Pull request body" --repo microsoft/carbon-aware-sdk
    exit 0
# fi