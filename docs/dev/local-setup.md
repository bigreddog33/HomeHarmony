# Local Development Setup

## Run the current mock login

The implemented API and clients do not need SQL Server yet. From the repository root, run `dotnet run --project api/Household.Api --launch-profile http` to start the API on `http://localhost:5193`.

For web, use Node.js 22, run `npm ci` in `web/`, copy `.env.example` to `.env.local`, then run `npm run dev`. Open `http://localhost:3000`. Keep the host as `localhost` to match the API's default CORS origin. If you change the web origin, configure the API's `WebClientOrigin` to match.

Use `demo@homeharmony.local` / `Password123!` on either client. Login leads to a placeholder home screen and creates no session or token.

### Android

Open `mobile/` in Android Studio. Install Android SDK Platform 37 and configure the SDK location through Android Studio (`local.properties`) or `ANDROID_HOME`. The app supports Android 9 / API 28 and newer. The Gradle wrapper pins Gradle; `gradle/gradle-daemon-jvm.properties` requests a Java 25 daemon toolchain, which Gradle can provision when network access is available. Java source/target compatibility is set separately to 11.

For a USB-connected device or emulator, enable debugging and run:

```sh
adb reverse tcp:5193 tcp:5193
```

Then run the debug app from Android Studio. Its default API URL is `http://127.0.0.1:5193/`. Alternatively, use the emulator's host address by setting `DEBUG_API_BASE_URL=http://10.0.2.2:5193/` in your local Gradle properties, or pass `-PDEBUG_API_BASE_URL=http://10.0.2.2:5193/` to Gradle.

API base URLs must end in `/`. Debug builds permit HTTP for local development. Release builds require an HTTPS `RELEASE_API_BASE_URL`; the default `https://api.homeharmony.invalid/` intentionally points to no deployed service.

The application ID is `io.github.bigreddog33.homeharmony`. Android treats this as a separate app from earlier builds with the template ID; those development installs are not upgraded in place.

### Checks

- Repository root: `dotnet test api/Household.slnx --configuration Release`.
- `web/`: `npm run lint`, `npm test`, `npm run build`.
- `mobile/`: `./gradlew build` (PowerShell: `.\gradlew.bat build`). Runs JVM tests and lint and builds the app without a connected device.

## Development background

The local setup for this project is a little bit more confusing than usual, mainly because I want to be able to work on it from both a **Windows computer** and a **Mac**.

Because of that, I am trying to keep the development environment as consistent as possible between the two systems, while still using the tools that make the most sense on each platform.

## .NET API

For the .NET API project, I am currently using:

* **.NET 10 LTS SDK**
* **Visual Studio** on Windows
* **JetBrains Rider** on Mac

The API should be buildable and runnable on both systems without requiring platform-specific changes.

## Web application

The web application uses:

* **Next.js**
* **React**
* **TypeScript**

Use the Node.js version and commands above for the current application.

## Android application

For Android development, I use **Android Studio** together with the Android Jetpack ecosystem. The minimum supported Android version is Pie (API 28).
The Android development environment is intended to be fully usable from both development machines where possible.
Debugging is done on a Pixel 8 physical device.

## Data storage

The planned main database is **SQL Server**. Persistence is not implemented in the current mock login slice.

On Windows, I can run SQL Server normally and use **SQL Server Management Studio (SSMS)** for viewing and managing the database.

On Mac, since SQL Server is a Microsoft technology and does not run natively in the same way, I plan to run it inside a **Docker container**.

This should also help keep the database environment relatively consistent between both development systems.

As the project develops, this document will be expanded with the actual setup steps, Docker configuration, required versions, environment variables, and any platform-specific differences that appear along the way.
