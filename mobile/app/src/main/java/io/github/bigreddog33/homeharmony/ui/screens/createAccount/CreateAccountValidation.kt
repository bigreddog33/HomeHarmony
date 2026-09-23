package io.github.bigreddog33.homeharmony.ui.screens.createAccount

import androidx.annotation.StringRes
import androidx.core.util.PatternsCompat
import io.github.bigreddog33.homeharmony.R

data class LoginValidationErrors(
    @StringRes val email: Int? = null,
    @StringRes val password: Int? = null,
    @StringRes val emailConfirm: Int? = null,
    @StringRes val passwordConfirm: Int? = null
) {
    val isValid: Boolean
        get() = email == null && password == null && emailConfirm == null && passwordConfirm == null
}

fun validateLogin(email: String, password: String, emailConfirm: String, passwordConfirm: String): LoginValidationErrors {
    val emailError = when {
        email.isBlank() -> R.string.createAccount_email_required
        !PatternsCompat.EMAIL_ADDRESS.matcher(email.trim()).matches() -> R.string.createAccount_email_invalid
        email != emailConfirm -> R.string.createAccount_email_not_matching
        else -> null
    }
    
    val passwordError = when {
        password.isBlank() -> R.string.createAccount_password_required
        password.trim() != "rule from api here" -> R.string.createAccount_password_invalid //TODO
        password != passwordConfirm ->  R.string.createAccount_password_not_matching
        else -> null
    }

    return LoginValidationErrors(
        email = emailError,
        password = passwordError
    )
}
