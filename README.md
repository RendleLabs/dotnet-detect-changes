# dotnet-detect-changes

This is a .NET tool to detect whether any files affecting a given project
or projects in a repository were changed in a commit or pull request.

The idea is that you can have a single Git repo with one or more
.NET solutions, and determine whether the CI/CD pipeline needs to be
run for a subset of projects within that repo. For example, you might
have a web application, an API and a NuGet package all in the same
repo. If you just change a button style in the web application, you
probably don't want to release a new version of the API and NuGet package.

# Usage

You can install the tool locally

Detect whether projects and associated files were changed in the most recent git commit
