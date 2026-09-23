package io.github.bigreddog33.homeharmony.ui.screens.login

import io.github.bigreddog33.homeharmony.R
import org.junit.Assert.assertEquals
import org.junit.Assert.assertFalse
import org.junit.Assert.assertNull
import org.junit.Assert.assertTrue
import org.junit.Test

class LoginValidationTest {
    @Test
    fun `empty fields are required`() {
        val errors = validateLogin("", "")

        assertEquals(R.string.email_required, errors.email)
        assertEquals(R.string.password_required, errors.password)
        assertFalse(errors.isValid)
    }

    @Test
    fun `whitespace only fields are required`() {
        val errors = validateLogin(" \t", " \n")

        assertEquals(R.string.email_required, errors.email)
        assertEquals(R.string.password_required, errors.password)
    }

    @Test
    fun `malformed email is rejected`() {
        listOf("not-an-email", "name@", "@example.com", "a@@example.com", "a b@example.com")
            .forEach { email ->
                val errors = validateLogin(email, "password")

                assertEquals(email, R.string.email_invalid, errors.email)
                assertNull(errors.password)
                assertFalse(errors.isValid)
            }
    }

    @Test
    fun `missing password does not invalidate email`() {
        val errors = validateLogin("person@example.com", "")

        assertNull(errors.email)
        assertEquals(R.string.password_required, errors.password)
    }

    @Test
    fun `valid email allows surrounding whitespace and plus addressing`() {
        assertTrue(validateLogin(" person+home@example.com ", "password").isValid)
    }

    @Test
    fun `login does not enforce password creation policy`() {
        assertTrue(validateLogin("person@example.com", "x").isValid)
    }
}
