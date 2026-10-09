package io.github.bigreddog33.homeharmony.ui.screens.createAccount

import androidx.annotation.StringRes

data class CreateAccountUiState(
    val email: String = "",
    val emailConfirm: String = "",
    val password: String = "",
    val passwordConfirm: String = "",
    val isLoading: Boolean = false,
    val isCreateAccountSuccessful: Boolean = false,
    val confirmationStatus: String? = null,
    val confirmationEmail: String? = null,
    val existingAccountEmail: String? = null,
    val existingAccountEmailConfirmed: Boolean? = null,
    
    @param:StringRes val emailError: Int? = null,
    @param:StringRes val emailConfirmError: Int? = null,
    @param:StringRes val passwordError: Int? = null,
    @param:StringRes val passwordConfirmError: Int? = null,
    @param:StringRes val createAccountError: Int? = null,
    @param:StringRes val snackbarMessage: Int? = null
)
