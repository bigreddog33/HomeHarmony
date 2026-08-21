# Project Description

## Why this project exists

This project was born from the need to advance my technical knowledge while building something I could actually use.

After some brainstorming, I came up with the idea of a **household planner**, or rather a calendar that is easier to share and edit as a group.

Something like collaborative online documents, but for calendars.

The idea came from an actual need in my household: we both like planning, but keeping calendars, chores, reminders, appointments, and shared responsibilities in sync is surprisingly difficult.

So, instead of building something only for the sake of learning, I wanted to create something that could genuinely be useful in my daily life.

At the same time, after taking interviews and looking at current technical requirements, I realised there are areas where I want to deepen my knowledge and broaden my experience.

I already work professionally with full-stack web applications, so the purpose of this project is not to relearn what I already use every day, but to build on that foundation and deliberately explore technologies, architectural decisions, and development practices I do not always get the chance to work with.

## Where I am starting from

Currently, I work as a web developer on full-stack applications focused on digitising and improving existing processes.

Besides implementation, I also analyse requirements, design solutions, and maintain them afterwards.

My main stack is:

* .NET / ASP.NET Core
* TypeScript
* SQL Server
* adjacent tooling around them

This gives me a solid starting point for the project, especially for the API, backend logic, data modelling, and general application design.

At the same time, I have developed a curiosity for Android applications.

I did play with Android development a little during my studying days, so I thought it would be meaningful to properly learn this stack as well and become more comfortable building beyond the web.

I also like the look and functionality of modern front-end applications built with React-based frameworks such as **Next.js**.

As a full-stack developer, I think becoming comfortable with this type of front-end would be valuable and would give me more flexibility when designing complete products.

Another area I want to explore more deeply is **real-time communication**.

Most applications I work with are based around the usual request/response flow, scheduled operations, or manually refreshed data. For this project, I want to deal with the challenge of propagating changes between users while they are actively using the application.

Adding these technologies and concepts to my stack would increase my flexibility and help me become more comfortable designing and building a product from end to end.

## Improving the way I develop

Besides learning new technologies, I also want to improve the way I approach the development process itself.

I want to become more consistent with writing clear tasks, following standards, keeping repositories clean, and structuring work in a way that is easier to understand and maintain.

I also want to gain more hands-on experience with modern development and deployment practices, including:

* CI/CD pipelines;
* pull requests and code review flows;
* automated testing;
* branch protection and repository rules;
* containerisation;
* repeatable deployments;
* safer releases;
* better failure and rollback strategies.

The point is not only to make the application work.

I also want to improve how I **design, organise, test, document, and deploy** software.

## AI tooling

Furthermore, more companies are starting to inquire about experience with AI tooling.

While I strongly believe in the capabilities and importance of a human developer, I also think these tools can be useful when used with clear boundaries.

For this project, I plan to use AI tooling mainly for:

* learning technologies and concepts I do not know yet;
* deeper research;
* polishing documentation;
* repetitive activities;
* generating base configurations or repetitive repository functions;
* helping generate or fix content that would otherwise take unnecessary time;
* brainstorming possible approaches.

I want to keep the **architecture, ideas, implementation choices, and final decisions as my own**.

I see AI more as a partner for research and brainstorming than something that should make the engineering decisions for me.

## What the application should do

For starters, described in natural language, the application should be able to:

* create a household and invite people into it;
* let each person choose a colour that is used consistently across the UI;
* create events with an optional date, time, title, description, and participants;
* create tasks with an optional date/time, assignees, and completion state;
* allow tasks to appear on the calendar without becoming completed automatically;
* support recurring events and recurring tasks;
* show a dashboard or home screen that merges everything into one timeline;
* support filters such as events only, tasks only, or both.

This is obviously still written in natural language, but these requirements are the starting point for figuring out how the application should actually behave.

## Designing before implementing

With those goals in mind, I first have to think about how the user interacts with the application, identify the backend elements needed to support those interactions, and properly model them.

Before writing the deeper implementation, my intention is to first create an abstraction of the information and the relationships between it.

From my experience, the better the data modelling is, the easier it becomes to design the backend functionality afterwards.

A good model should also leave space for:

* improvements;
* changes of scope;
* removal of features;
* addition of new features;

without every change turning into a full-blown refactoring.

Another important step will be identifying most of the API endpoints that the web and mobile applications will need.

Only after those boundaries and interactions become clearer will I move deeper into the implementation.

## Developing from the outside to the inside

I am thinking about developing this project somewhat **from the outside to the inside**.

I want to start from what the user expects to do, then identify what the front-end needs, what the API needs to expose, and finally what the internal implementation needs in order to support it.

This also means that I might start with a fairly basic implementation and later decide that some areas deserve more complex patterns.

For example, I could eventually end up creating factories that defer to the correct repository or implementation based on the type they receive.

I do not want to introduce complex patterns simply because they exist or because they look impressive.

I would rather reach them naturally when the application actually gives me a reason to use them.

## Separating the scope better

This time, I also want to separate the scope of tasks more carefully.

I am used to tasks such as:

> Create a login page.

For this project, I want to break things down further and think about the actual components involved:

* data;
* backend;
* front-end;
* mobile;
* expected behaviour;
* visual behaviour.

Instead of treating one feature as a single large task, I want to describe what each part should do, how the pieces interact, and what the user should see.

This should make the work easier to reason about and should also help me practise writing better technical tasks and requirements.

## Tasks are not events

Another distinction I made very early is the difference between **tasks** and **events** inside the application.

This is actually one of the bigger problems I have with the calendars I currently use, even more than manually colour-coding events.

Calendars are very good at events, but tasks do not behave exactly like events.

I might want a task on the calendar for Wednesday, but not at 2 AM.

I simply want it to exist for **Wednesday**.

Or maybe I really do want the task scheduled for 2 PM, but I do not want it disappearing from my screen just because 2 PM passed.

I want it there until I actually check it as completed.

Because of that:

* an **event** represents something happening at a certain time or during a certain period;
* a **task** represents something that needs to be completed.

A task can have a date or a time, but its lifecycle should depend on completion rather than simply on the clock moving forward.

This distinction will influence both the data model and the way the calendar itself behaves.

## Architecture

Another decision I have made so far is to build the application initially as a **modular monolith**.

I want to create modules around domains that could theoretically work independently, while still keeping them inside the same application and deployment.

The modules should have clear boundaries in terms of their responsibilities and functionality.

This gives me the simplicity of working with a monolith while still encouraging proper separation.

It also leaves room to separate certain modules into independent services in the future if the application ever grows enough for that to make sense.

I do not want to start with distributed services simply because they are interesting to work with.

I would rather have the architecture become more complex only when the requirements justify it.

## Real-time collaboration

One of the bigger technical challenges will be keeping calendar information consistent between all the users inside the same household.

If one person creates, edits, completes, or deletes something, the others should see that change without needing to manually refresh the page.

This is where the **real-time notification and synchronisation system** comes in.

The web application should behave more like a modern single-page application, where relevant changes made by other household members can appear almost immediately.

This will also introduce interesting problems around:

* concurrent changes;
* synchronisation;
* consistency;
* reconnecting clients;
* deciding what happens when two people edit the same information;
* keeping the web and Android applications in sync.

These are exactly the kinds of problems I want this project to give me the opportunity to explore.

## Overall goal

In the end, the goal of this project is not simply to create another calendar application.

I already have professional experience building and maintaining full-stack software, and I want to use that foundation to deliberately expand into areas I have less exposure to.

I want to build something I can genuinely use while improving both my technical range and the way I approach software engineering as a whole.

If the project eventually grows into something useful for other people as well, even better.
