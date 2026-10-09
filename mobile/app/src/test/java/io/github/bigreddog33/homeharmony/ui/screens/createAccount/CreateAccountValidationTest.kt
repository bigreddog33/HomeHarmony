package io.github.bigreddog33.homeharmony.ui.screens.createAccount

import io.github.bigreddog33.homeharmony.R
import org.junit.Assert.*
import org.junit.Test

class CreateAccountValidationTest {
    @Test
    fun emptyOrWhitespaceFieldsAreRequiredIndependently() {
        for (blank in listOf("", "   ", "\t\n")) {
            assertEquals(CreateAccountValidationErrors(email = R.string.email_required,
                emailConfirm = R.string.createAccount_email_not_matching),
                validateCreateAccount(blank, password, email, password))
            assertEquals(CreateAccountValidationErrors(emailConfirm = R.string.email_required),
                validateCreateAccount(email, password, blank, password))
            assertEquals(CreateAccountValidationErrors(
                password = R.string.password_required,
                passwordConfirm = R.string.createAccount_password_not_matching),
                validateCreateAccount(email, blank, email, password))
            assertEquals(CreateAccountValidationErrors(passwordConfirm = R.string.password_required),
                validateCreateAccount(email, password, email, blank))
        }
    }

    @Test
    fun malformedEmailsAreRejectedInBothFields() {
        for (invalid in listOf("not-an-email", "name@", "@example.com",
            "name@@example.com", "first last@example.com")) {
            val result = validateCreateAccount(invalid, password, invalid, password)
            assertEquals(invalid, R.string.email_invalid, result.email)
            assertEquals(invalid, R.string.email_invalid, result.emailConfirm)
            assertFalse(result.isValid)
        }
    }

    @Test
    fun emailComparisonTrimsSurroundingWhitespaceAndAcceptsPlusAddressing() {
        assertTrue(validateCreateAccount(" user+home@example.com ", password,
            "user+home@example.com", password).isValid)
        assertEquals(R.string.createAccount_email_not_matching,
            validateCreateAccount(email, password, "other@example.com", password).emailConfirm)
    }

    @Test
    fun eachPasswordRequirementIsEnforcedEvenWhenConfirmationMatches() {
        for (invalid in listOf("Aa1!aaa", "password1!", "Password!!", "Password12",
            "Password1 ", "Password1\n")) {
            val result = validateCreateAccount(email, invalid, email, invalid)
            assertEquals(invalid, R.string.createAccount_password_rules, result.password)
            assertNull(result.passwordConfirm)
            assertFalse(result.isValid)
        }
    }

    @Test
    fun validBoundaryAndUnicodePasswordsDoNotRequireLowercase() {
        for (valid in listOf("Aa1!aaaa", "PASSWORD1!", "Password1€", " Password1! ")) {
            assertTrue(valid, validateCreateAccount(email, valid, email, valid).isValid)
        }
    }

    @Test
    fun passwordConfirmationUsesExactValueIncludingWhitespace() {
        for (confirmation in listOf("Different1!", " Password1!", "Password1! ")) {
            assertEquals(R.string.createAccount_password_not_matching,
                validateCreateAccount(email, password, email, confirmation).passwordConfirm)
        }
    }

    private val email = "person@example.com"
    private val password = "Password1!"
}
