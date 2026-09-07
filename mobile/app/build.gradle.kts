import java.net.URI

plugins {
    alias(libs.plugins.android.application)
    alias(libs.plugins.kotlin.compose)
}

val debugApiBaseUrl = providers.gradleProperty("DEBUG_API_BASE_URL")
    .orElse("http://127.0.0.1:5193/").get()
val releaseApiBaseUrl = providers.gradleProperty("RELEASE_API_BASE_URL")
    .orElse("https://api.homeharmony.invalid/").get()

mapOf("DEBUG_API_BASE_URL" to debugApiBaseUrl, "RELEASE_API_BASE_URL" to releaseApiBaseUrl)
    .forEach { (name, value) ->
        val uri = URI(value)
        require(
            uri.scheme in listOf("http", "https") && uri.host != null &&
                uri.userInfo == null && uri.query == null && uri.fragment == null &&
                value.endsWith("/")
        ) { "$name must be an HTTP(S) base URL ending in /, without credentials, query or fragment." }
    }
require(URI(releaseApiBaseUrl).scheme == "https") {
    "RELEASE_API_BASE_URL must use HTTPS."
}

android {
    namespace = "com.example.myapplication"

    compileSdk {
        version = release(37)
    }

    defaultConfig {
        applicationId = "com.example.myapplication"
        minSdk = 28
        targetSdk = 37
        versionCode = 1
        versionName = "1.0"

        testInstrumentationRunner = "androidx.test.runner.AndroidJUnitRunner"
    }

    buildTypes {
        debug {
            buildConfigField("String", "API_BASE_URL", "\"$debugApiBaseUrl\"")
        }
        release {
            buildConfigField("String", "API_BASE_URL", "\"$releaseApiBaseUrl\"")
            optimization {
                enable = false
            }
        }
    }

    compileOptions {
        sourceCompatibility = JavaVersion.VERSION_11
        targetCompatibility = JavaVersion.VERSION_11
    }

    buildFeatures {
        compose = true
        buildConfig = true
    }
}

dependencies {
    implementation(platform(libs.androidx.compose.bom))
    implementation(libs.androidx.activity.compose)
    implementation(libs.androidx.compose.material3)
    implementation(libs.androidx.compose.ui)
    implementation(libs.androidx.compose.ui.graphics)
    implementation(libs.androidx.compose.ui.tooling.preview)
    implementation(libs.androidx.core.ktx)
    implementation(libs.androidx.lifecycle.runtime.ktx)
    implementation(libs.androidx.lifecycle.runtime.compose)

    implementation(libs.androidx.compose.material.icons.extended)
    implementation(libs.androidx.lifecycle.viewmodel.compose)
    implementation(libs.retrofit)
    implementation(libs.retrofit.converter.gson)
    implementation(libs.okhttp)
    implementation(libs.androidx.navigation.compose)

    testImplementation(libs.junit)
    testImplementation(libs.kotlinx.coroutines.test)
    androidTestImplementation(platform(libs.androidx.compose.bom))
    androidTestImplementation(libs.androidx.compose.ui.test.junit4)
    androidTestImplementation(libs.androidx.espresso.core)
    androidTestImplementation(libs.androidx.junit)
    debugImplementation(libs.androidx.compose.ui.test.manifest)
    debugImplementation(libs.androidx.compose.ui.tooling)
}
