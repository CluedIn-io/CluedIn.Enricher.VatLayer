# CluedIn.CSharp.Template

Template containing common files and folder structure for creating new C# repositories.

## Simple Usage

To create a Git repository for a new C# project follow the steps below:

```Shell
# Clone the CSharp template repository and step into the new folder
git clone https://github.com/CluedIn-io/CluedIn.CSharp.Template.git CluedIn.ProjectName
cd CluedIn.ProjectName

# Rename the Git remote to template
git remote rename origin template
```

Create the CluedIn.ProjectName repository under the CluedIn organization in GitHub then perform the following steps:

```Shell
# Add the new GitHub repository as the origin remote
git remote add origin https://github.com/CluedIn-io/CluedIn.ProjectName

# Push the content to the new Git repository
git push
```

Substitute _CluedIn.ProjectName_ for your project name.

-----

TODO usage