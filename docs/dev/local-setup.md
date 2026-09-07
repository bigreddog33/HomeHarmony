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

For Android development, I will be using **Android Studio** together with the Android Jetpack ecosystem. The Android version is Pie.
The Android development environment is intended to be fully usable from both development machines where possible.
Debugging is done on a Pixel 8 physical device.

### Android API environments

The `debug` build allows HTTP for local testing. Its default API URL is
`http://127.0.0.1:5193/`; on a USB-connected phone, use `adb reverse tcp:5193 tcp:5193`
to forward that address to the development computer. For Wi-Fi testing, override
`DEBUG_API_BASE_URL` with the computer's reachable LAN address.

The `release` build blocks HTTP and uses a separate `RELEASE_API_BASE_URL`, which
must use HTTPS. Until the deployment address is known, the default is
`https://api.homeharmony.invalid/`, an intentionally non-working placeholder that
allows CI to compile release builds. Set the real address before distributing an APK.

Both URLs must end with `/`. Configure them through Gradle project properties,
for example in your user-level `~/.gradle/gradle.properties`:

```properties
DEBUG_API_BASE_URL=http://192.168.1.50:5193/
RELEASE_API_BASE_URL=https://your-api.example.com/
```

They can also be passed with `-PDEBUG_API_BASE_URL=...` or
`-PRELEASE_API_BASE_URL=...` when invoking Gradle. These are build-time settings;
rebuild the app after changing them. The release example above is not a deployed service.

## Data storage

The main database for the project will be **SQL Server**.

On Windows, I can run SQL Server normally and use **SQL Server Management Studio (SSMS)** for viewing and managing the database.

On Mac, since SQL Server is a Microsoft technology and does not run natively in the same way, I plan to run it inside a **Docker container**.

This should also help keep the database environment relatively consistent between both development systems.

As the project develops, this document will be expanded with the actual setup steps, Docker configuration, required versions, environment variables, and any platform-specific differences that appear along the way.
