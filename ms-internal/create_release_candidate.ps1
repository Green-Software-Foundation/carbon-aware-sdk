#####################################################################################
# THIS SCRIPT ASSUMES YOU HAVE THE FOLLOWING REMOTES SETUP.
# DO NOT RUN THIS UNTIL YOU DO (either HTTPS or SSH remotes will work)
#####################################################################################
#
# git remote -v
# gsf     https://github.com/Green-Software-Foundation/carbon-aware-sdk.git (fetch)
# gsf     https://github.com/Green-Software-Foundation/carbon-aware-sdk.git (push)
# origin  https://github.com/microsoft/carbon-aware-sdk.git (fetch)
# origin  https://github.com/microsoft/carbon-aware-sdk.git (push)
#####################################################################################

# Remove the local release branch if exists
git branch -D release

# Get the latest commits
git fetch gsf
git fetch origin

# checkout gsf/dev and cut a new release branch from it
git checkout gsf/dev
git checkout -b release

# Cherry-pick our required, but unmerged commits onto the branch
git cherry-pick 707b09183f0e2cb52a2846ff14813e06963058f3 # issue #166 - SDK library tooling
git cherry-pick 83f0d5f579f9be542a4a8a1a1b7b448f1cbba271 # issue #164 - New DataSource Config Schema
git cherry-pick c1ca823366dc45eaeed96bb6b58ad43f68f54b7b # issue #167 - ElectricityMaps Client
git cherry-pick 3592f9d8a823728605d04f8287a6036df17af247 # issue #167 - ElectricityMaps Data Source - Forecasts
git cherry-pick cbb00885f1d86170b5ac99c0ba0f2671caefb081 # issue #193 - Emissions and Best Emissions Endpoint in Library
git cherry-pick 346210fedff80079dfd24ee8335d63188e9cd93e # issue #192 - GetForecastByDate Endpoint in Library
git cherry-pick a14e60b77eb271f1b253810dbc6b02f539ad6b25 # issue #174 - ElectricityMaps Data Source - Emissions

####### Adding/updating features with this script:
####### 1) switch to your feature branch
#
# git switch <###/your-feature-branch>
#
#######
####### 2) Update your branch with the latest `release` commits
#
# git pull --rebase origin release
#
#######
####### 2a) Resolve any conflicts.
#######
####### 3) Squash your feature into a single commit
#
# git rebase -i HEAD~<number-of-commits-in-your-feature>  # EG: git rebase -i HEAD~3
#
#######
####### In the interactive window, squash your those commits into a single commit
####### follows the naming convention:
####### [M#][Issue#] Feature name
#######
####### 4) Push your feature branch to the remote
#
# git push --force-with-lease
#
####### 5) Copy your commit hashes
#
# git log --pretty=format:'git cherry-pick %H # %s' gsf/dev..HEAD | Set-Clipboard
#
#######
####### 6) Pull `dev` and branch off it
#
# git switch dev
# git pull
# git checkout -b release-update-<YYYY-MM-DD>
#
####### 7) Replace the `git cherry-pick` section of BOTH `create_release_candidate` scripts
####### If you used the `git log` command above, it will already be in your clipboard :)
#######
####### NOTE: All commit hashes will likely change from the original
#######       script, due to the nature of rebasing our commits.
#######
####### 8) Push and PR into `dev`
####### Dont worry about filling out the template.
####### Assign PR to the current Release Branch Owner
####### Tag the rest of the team for review