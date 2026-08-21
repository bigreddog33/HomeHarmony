# Git and GitHub Configuration

While I do have some experience with Git, I want to gain more hands-on experience with **CI/CD pipelines, pull requests, branch protection, and GitHub rulesets**.

## Initial repository setup

I started by creating an empty project, added a few basic placeholder files, and pushed the initial structure to GitHub.

The first thing I wanted to set up was a working pipeline. I created a YAML workflow for **GitHub Actions** that instructs GitHub to:

* restore the project dependencies;
* compile the .NET API project;
* run the tests;
* fail the pipeline if something goes wrong.

For this, I obviously had to create some empty/dummy .NET projects first, just to have something that could actually be compiled and tested.

After successfully testing the GitHub Actions pipeline, I moved on to protecting the `main` branch.

## Main branch protection

As I am the solo developer working on this project, I decided to keep only the `main` branch protected.

I did not want the additional overhead of having something like:

`feature branch -> dev -> main`

For the current size of the project, that felt unnecessary. I can always introduce a more complex branching strategy later if the project grows and actually needs it.

Nevertheless, I created a ruleset for `main`.

The rules currently:

* restrict branch deletion;
* require changes to go through a pull request;
* require the configured status checks to pass;
* block force pushes;
* require code checks to succeed before changes can be merged.

This way, even though I am working alone, I still have to follow a proper pull request flow instead of pushing everything directly into `main`.

## License

I also chose a license for the project.

As I want the project to be **open source**, but at the same time I want to protect my work and keep improvements to publicly hosted versions available to the community, I chose the **GNU Affero General Public License v3.0 (AGPL-3.0)**.

## Project structure

After the initial Git and GitHub setup was working, I proceeded with finishing the base folder structure of the repository.

The structure is still expected to evolve together with the project, but I wanted to have a clear starting point before beginning the actual implementation.

## GitHub Project and task management

I then moved on to creating a **GitHub Project** with a Kanban board for managing future work.

While setting it up, I experimented with:

* creating issues and sub-issues;
* figuring out how I want to represent epics and stories;
* setting priorities;
* adding deadlines;
* configuring the project so that tasks show properly in both the **Priority** and **Roadmap** views.

While doing this, I also created labels to describe and organize the issues better.

So far, most of the issues are simple and fairly self-explanatory, but I am planning to write better and more detailed tasks as the project becomes more complex.

<img width="1154" height="385" alt="image" src="https://github.com/user-attachments/assets/ab71ab57-03ee-42e7-9937-ef872e4dc96c" />

<img width="781" height="783" alt="image" src="https://github.com/user-attachments/assets/c9176b4d-c025-4f56-b1e7-a5ab03fe77a6" />

## Cleaning up the initial repository

After all of this, I had to purge the initial version of the repository.

Since I put quite a lot of value on privacy, one blunder I initially missed was that my local Git configuration was still using personal information.

Some of the commits therefore contained an email address I did not want to expose publicly.

I looked into rewriting the repository history using `git-filter-repo`, but for a project that was still in such an early stage, it felt like overkill.

Instead, I decided that it was cleaner to properly configure Git for this project in particular, using my GitHub public **no-reply email address**, and then start again from a blank slate.

This also gave me the opportunity to make sure the repository started cleanly, with the Git configuration I actually wanted to use from the beginning.
