# Local Development Setup

The local setup for this project is a little bit more confusing than usual, mainly because I want to be able to work on it from both a **Windows computer** and a **Mac**.

Because of that, I am trying to keep the development environment as consistent as possible between the two systems, while still using the tools that make the most sense on each platform.

## .NET API

For the .NET API project, I am currently using:

* **.NET 10 LTS SDK**
* **Visual Studio** on Windows
* **JetBrains Rider** on Mac

The API should be buildable and runnable on both systems without requiring platform-specific changes.

## Web application

The web application is planned to use:

* **Next.js**
* **React**
* **TypeScript**

The JavaScript/TypeScript will require the appropriate Node.js tooling as the web application starts taking shape.

## Android application

For Android development, I will be using **Android Studio** together with the Android Jetpack ecosystem.

The Android development environment is intended to be fully usable from both development machines where possible.

## Data storage

The main database for the project will be **SQL Server**.

On Windows, I can run SQL Server normally and use **SQL Server Management Studio (SSMS)** for viewing and managing the database.

On Mac, since SQL Server is a Microsoft technology and does not run natively in the same way, I plan to run it inside a **Docker container**.

This should also help keep the database environment relatively consistent between both development systems.

As the project develops, this document will be expanded with the actual setup steps, Docker configuration, required versions, environment variables, and any platform-specific differences that appear along the way.
