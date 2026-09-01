package com.example.myapplication.ui.screens.login

data class LoginUiState(
    val Email: String = "",
    val Password: String = "",
    val isLoading: Boolean = false,
    val errorMessage: String? = null,
    val loginSucceeded: Boolean = false
)