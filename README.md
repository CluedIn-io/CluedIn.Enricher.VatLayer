# CluedIn.CSharp.Template

Template containing common files and folder structure for creating new C# repositories.

## Simple Usage

1. To create a Git repository for a new C# project follow the steps below

```Shell
# Clone the CSharp template repository and step into the new folder
git clone https://github.com/CluedIn-io/CluedIn.CSharp.Template.git CluedIn.ProjectName
cd CluedIn.ProjectName

# Rename the Git remote to template
git remote rename origin template
```

1. Rename the Visual Studio solution in the root folder

    ```Shell
    ren CluedIn.CSharp.Template.sln CluedIn.ProjectName.sln
    ```

1. Create the CluedIn.ProjectName repository under the CluedIn organization in GitHub

1. Set the new GitHub repository as the _origin_ remote and push your changes

```Shell
# Add the new GitHub repository as the origin remote
git remote add origin https://github.com/CluedIn-io/CluedIn.ProjectName

# Push the content to the new Git repository
git push
```

Substitute _CluedIn.ProjectName_ for your project name.

-----

TODO usage