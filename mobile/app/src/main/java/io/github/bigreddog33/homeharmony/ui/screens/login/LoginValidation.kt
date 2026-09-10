package io.github.bigreddog33.homeharmony.ui.screens.login

import androidx.annotation.StringRes
import androidx.core.util.PatternsCompat
import io.github.bigreddog33.homeharmony.R

data class LoginValidationErrors(
    @StringRes val email: Int? = null,
    @StringRes val password: Int? = null
) {
    val isValid: Boolean
        get() = email == null && password == null
}

fun validateLogin(email: String, password: String): LoginValidationErrors {
    val emailError = when {
        email.isBlank() -> R.string.login_email_required
        !PatternsCompat.EMAIL_ADDRESS.matcher(email.trim()).matches() ->
            R.string.login_email_invalid
        else -> null
    }

    return LoginValidationErrors(
        email = emailError,
        password = if (password.isBlank()) R.string.login_password_required else null
    )
}
