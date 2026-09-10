package io.github.bigreddog33.homeharmony.ui.previews

import androidx.compose.runtime.Composable
import io.github.bigreddog33.homeharmony.ui.screens.login.LoginScreen
import io.github.bigreddog33.homeharmony.ui.theme.HomeHarmonyTheme

@AppDevicePreviews
@Composable
fun LoginScreenPreview() {
    HomeHarmonyTheme {
        LoginScreen(onLoginSuccess = {})
    }
}