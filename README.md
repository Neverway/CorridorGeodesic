# Riven Framework - Unity Edition

## Contributors Agreement

Any contributions made to this repository are property of Neverway Studio. "Property" in this context is defined as giving Neverway Studio the rights to use, re-use, modify, sell, and distribute any of the contributions how they see fit. Third party software, plugins, textures, and all other content not created for this project specifically retain their existing licences. This in no way prevents you from using your contributions in other projects, it only guarantees Neverway Studio the rights to use it without restrictions.

In summary, if you create and commit content to this repository, Neverway Studio is allowed to use that content in any Neverway Studio projects. This includes modifying the content, selling projects containing the content, or distributing projects containing the content.

By committing content to this project, you agree to this Contributors Agreement.

## About
[WIP] LIZ UPDATE THIS LATER<br />
This is a template/framework for Neverway Unity projects. <br />
Don't touch any folders with the locks on them.

## How to create a Fragment Repository
Fragment Repositories are just project repos that are built off of this framework.
To create a Fragment Repo follow these steps:
1. Create an empty folder for your project
2. In your terminal, enter the command `git init -b main` in the folder you just created
3. Pull the latest version of the framework with `git pull https://github.com/Neverway/RivenFramework-Unity.git --allow-unrelated-histories`
4. Add the 'remote origin' (the destination of your project repo, this is usually just a new empty repo on GitHub) with `git remote add origin {YOUR REPO DESTINATION HERE}`
5. Push your changes with `git push -u origin main`
6. Finally configure the repo to pull the framework updates correctly with `git config pull.rebase true`

## How to update a Fragment Repository to a newer framework version
If you haven't already, make sure that the pull mode is set to rebase true with `git config pull.rebase true`<br />
Now all you have to do is make sure all of your changes in your Fragment Repository are committed and pushed, then execute this command to rebase the Fragment on the latest framework update
`git pull https://github.com/Neverway/RivenFramework-Unity.git --allow-unrelated-histories`
