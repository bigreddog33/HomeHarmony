package com.example.myapplication.ui.previews

import androidx.compose.runtime.Composable
import com.example.myapplication.ui.screens.login.LoginScreen
import com.example.myapplication.ui.theme.MyApplicationTheme

@AppDevicePreviews
@Composable
fun LoginScreenPreview() {
    MyApplicationTheme {
        LoginScreen(onLoginSuccess = {})
    }
}