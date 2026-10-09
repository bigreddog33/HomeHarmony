package io.github.bigreddog33.homeharmony.ui.screens.login

import androidx.annotation.StringRes

data class LoginUiState(
    val email: String = "",
    val password: String = "",
    val isLoading: Boolean = false,
    val isLoginSuccessful: Boolean = false,
    @param:StringRes val emailError: Int? = null,
    @param:StringRes val passwordError: Int? = null,
    @param:StringRes val loginError: Int? = null,
    @param:StringRes val snackbarMessage: Int? = null
)
