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
git cherry-pick e104b556835391cb8a4aaf4a0d0bf23d021871c9 # issue #195 - Bug fix for location string localization + bug fix for JSON integration test
git cherry-pick c031ddffd1a3281123ec4fd4df6004a16b6bb591 # issue #161 - new data source interfaces
git cherry-pick 229990b68b7eb1776bc803194d2eb7fb0a82498e # issue #161 - data source interfaces in config
git cherry-pick fcac60c507dc90447a42678347501cbb25a86963 # issue #160 - C# library
git cherry-pick ef154b9f9d254d2b3eee2beb5542d634668ab5c6 # issue #166 - SDK library tooling
git cherry-pick b0e7c8598dc56168c2f932f72a1fd39f5d7598c1 # issue #164 - New DataSource Config Schema
git cherry-pick eebc9202cbc509fcaca137d3930b902b046240a6 # issue #167 - ElectricityMaps Data Source - Forecasts

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