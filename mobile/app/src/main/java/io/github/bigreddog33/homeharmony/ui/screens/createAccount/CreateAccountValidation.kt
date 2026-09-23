package io.github.bigreddog33.homeharmony.ui.screens.createAccount

import androidx.annotation.StringRes
import androidx.core.util.PatternsCompat
import io.github.bigreddog33.homeharmony.R

data class CreateAccountValidationErrors(
    @StringRes val email: Int? = null,
    @StringRes val password: Int? = null,
    @StringRes val emailConfirm: Int? = null,
    @StringRes val passwordConfirm: Int? = null
) {
    val isValid: Boolean
        get() = email == null && password == null && emailConfirm == null && passwordConfirm == null
}

fun validateCreateAccount(email: String, password: String, emailConfirm: String, passwordConfirm: String): CreateAccountValidationErrors {
    val emailError = when {
        email.isBlank() -> R.string.email_required
        !PatternsCompat.EMAIL_ADDRESS.matcher(email.trim()).matches() -> R.string.email_invalid
        else -> null
    }

    val emailConfirmError = when {
        emailConfirm.isBlank() -> R.string.email_required
        !PatternsCompat.EMAIL_ADDRESS.matcher(emailConfirm.trim()).matches() -> R.string.email_invalid
        email.trim() != emailConfirm.trim() -> R.string.createAccount_email_not_matching
        else -> null
    }

    val passwordError = when {
        password.isBlank() -> R.string.password_required
        password.length < 8 || !passwordRules.matches(password) -> R.string.createAccount_password_rules
        else -> null
    }

    val passwordConfirmError = when {
        passwordConfirm.isBlank() -> R.string.password_required
        password != passwordConfirm -> R.string.createAccount_password_not_matching
        else -> null
    }

    return CreateAccountValidationErrors(
        email = emailError,
        password = passwordError,
        emailConfirm = emailConfirmError,
        passwordConfirm = passwordConfirmError
    )
}

// Keep aligned with the API's CreateUserRequest password validation.
private val passwordRules = Regex("""(?s)\A(?=.*[0-9])(?=.*[A-Z])(?=.*[\p{P}\p{S}]).*\z""")
