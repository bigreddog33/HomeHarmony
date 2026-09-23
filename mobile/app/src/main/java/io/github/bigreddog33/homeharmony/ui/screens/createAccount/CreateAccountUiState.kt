package io.github.bigreddog33.homeharmony.ui.screens.createAccount

import androidx.annotation.StringRes

data class CreateAccountUiState(
    val email: String = "",
    val emailConfirm: String = "",
    val password: String = "",
    val passwordConfirm: String = "",
    val isLoading: Boolean = false,
    val isCreateAccountSuccessful: Boolean = false,
    @StringRes val emailError: Int? = null,
    @StringRes val emailConfirmError: Int? = null,
    @StringRes val passwordError: Int? = null,
    @StringRes val passwordConfirmError: Int? = null,
    @StringRes val createAccountError: Int? = null,
    @StringRes val snackbarMessage: Int? = null
)
