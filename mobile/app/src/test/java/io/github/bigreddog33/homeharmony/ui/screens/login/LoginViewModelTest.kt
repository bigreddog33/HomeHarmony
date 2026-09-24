package io.github.bigreddog33.homeharmony.ui.screens.login

import io.github.bigreddog33.homeharmony.R
import io.github.bigreddog33.homeharmony.data.account.AccountApi
import io.github.bigreddog33.homeharmony.data.account.dto.CreateAccountRequest
import io.github.bigreddog33.homeharmony.data.account.dto.LoginRequest
import kotlinx.coroutines.CancellationException
import kotlinx.coroutines.CompletableDeferred
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.ExperimentalCoroutinesApi
import kotlinx.coroutines.test.StandardTestDispatcher
import kotlinx.coroutines.test.resetMain
import kotlinx.coroutines.test.runCurrent
import kotlinx.coroutines.test.runTest
import kotlinx.coroutines.test.setMain
import okhttp3.ResponseBody.Companion.toResponseBody
import org.junit.After
import org.junit.Assert.assertEquals
import org.junit.Assert.assertFalse
import org.junit.Assert.assertNull
import org.junit.Assert.assertTrue
import org.junit.Before
import org.junit.Test
import retrofit2.HttpException
import retrofit2.Response
import java.io.IOException

@OptIn(ExperimentalCoroutinesApi::class)
class LoginViewModelTest {
    @Before
    fun setUp() {
        Dispatchers.setMain(StandardTestDispatcher())
    }

    @After
    fun tearDown() {
        Dispatchers.resetMain()
    }

    @Test
    fun `invalid input shows field errors without calling API`() = runTest {
        val api = FakeAccountApi()
        val viewModel = LoginViewModel(api)

        viewModel.login()
        runCurrent()

        assertTrue(api.requests.isEmpty())
        assertFalse(viewModel.uiState.isLoginSuccessful)
        assertEquals(R.string.email_required, viewModel.uiState.emailError)
        assertEquals(R.string.password_required, viewModel.uiState.passwordError)
        assertNull(viewModel.uiState.loginError)
        assertNull(viewModel.uiState.snackbarMessage)
        assertFalse(viewModel.uiState.isLoading)
    }

    @Test
    fun `malformed email prevents API request`() = runTest {
        val api = FakeAccountApi()
        val viewModel = LoginViewModel(api)
        viewModel.onEmailChange("not-an-email")
        viewModel.onPasswordChange("password")

        viewModel.login()
        runCurrent()

        assertTrue(api.requests.isEmpty())
        assertEquals(R.string.email_invalid, viewModel.uiState.emailError)
        assertFalse(viewModel.uiState.isLoginSuccessful)
        assertNull(viewModel.uiState.passwordError)
    }

    @Test
    fun `editing clears only the relevant field error`() {
        val viewModel = LoginViewModel(FakeAccountApi())
        viewModel.login()

        viewModel.onEmailChange("person@example.com")

        assertNull(viewModel.uiState.emailError)
        assertEquals(R.string.password_required, viewModel.uiState.passwordError)

        viewModel.onPasswordChange("x")
        assertNull(viewModel.uiState.passwordError)
    }

    @Test
    fun `HTTP success needs no body and preserves password whitespace`() = runTest {
        val api = FakeAccountApi { Unit }
        val viewModel = LoginViewModel(api)
        viewModel.onEmailChange(" person@example.com ")
        viewModel.onPasswordChange(" x ")

        viewModel.login()

        assertTrue(viewModel.uiState.isLoading)
        assertFalse(viewModel.uiState.isLoginSuccessful)
        runCurrent()

        assertEquals(listOf(LoginRequest("person@example.com", " x ")), api.requests)
        assertTrue(viewModel.uiState.isLoginSuccessful)
        assertFalse(viewModel.uiState.isLoading)
        assertNull(viewModel.uiState.loginError)
        assertNull(viewModel.uiState.snackbarMessage)
    }

    @Test
    fun `duplicate submissions are ignored while loading`() = runTest {
        val pendingResponse = CompletableDeferred<Unit>()
        val api = FakeAccountApi { pendingResponse.await() }
        val viewModel = validViewModel(api)

        viewModel.login()
        viewModel.login()
        runCurrent()

        assertTrue(viewModel.uiState.isLoading)
        assertEquals(1, api.requests.size)

        pendingResponse.complete(Unit)
        runCurrent()

        assertTrue(viewModel.uiState.isLoginSuccessful)
        assertFalse(viewModel.uiState.isLoading)
    }

    @Test
    fun `delayed success remains in state for a screen that returns later`() = runTest {
        val pendingResponse = CompletableDeferred<Unit>()
        val api = FakeAccountApi { pendingResponse.await() }
        val viewModel = validViewModel(api)

        viewModel.login()
        runCurrent()
        assertFalse(viewModel.uiState.isLoginSuccessful)

        // The request completes without a screen callback or a UI observer.
        pendingResponse.complete(Unit)
        runCurrent()

        assertTrue(viewModel.uiState.isLoginSuccessful)
        viewModel.login()
        runCurrent()

        assertTrue(viewModel.uiState.isLoginSuccessful)
        assertEquals(1, api.requests.size)
    }

    @Test
    fun `invalid credentials show form error and editing clears it`() = runTest {
        val viewModel = validViewModel(FakeAccountApi {
            throw HttpException(Response.error<Unit>(401, "Unauthorized".toResponseBody()))
        })

        viewModel.login()
        runCurrent()

        assertEquals(R.string.login_invalid_credentials, viewModel.uiState.loginError)
        assertFalse(viewModel.uiState.isLoginSuccessful)
        assertNull(viewModel.uiState.emailError)
        assertNull(viewModel.uiState.passwordError)
        assertNull(viewModel.uiState.snackbarMessage)
        assertFalse(viewModel.uiState.isLoading)

        viewModel.onPasswordChange("corrected-password")
        assertNull(viewModel.uiState.loginError)
    }

    @Test
    fun `server validation failure shows safe form feedback`() = runTest {
        val viewModel = validViewModel(FakeAccountApi {
            throw HttpException(Response.error<Unit>(400, "Technical validation details".toResponseBody()))
        })

        viewModel.login()
        runCurrent()

        assertEquals(R.string.login_invalid_request, viewModel.uiState.loginError)
        assertFalse(viewModel.uiState.isLoginSuccessful)
        assertNull(viewModel.uiState.snackbarMessage)
        assertFalse(viewModel.uiState.isLoading)
    }

    @Test
    fun `server failure shows transient feedback and can be consumed repeatedly`() = runTest {
        val viewModel = validViewModel(FakeAccountApi {
            throw HttpException(Response.error<Unit>(500, "Internal technical details".toResponseBody()))
        })

        repeat(2) {
            viewModel.login()
            runCurrent()

            assertEquals(R.string.login_server_error, viewModel.uiState.snackbarMessage)
            assertFalse(viewModel.uiState.isLoginSuccessful)
            assertNull(viewModel.uiState.loginError)
            assertFalse(viewModel.uiState.isLoading)

            viewModel.onSnackbarShown()
            assertNull(viewModel.uiState.snackbarMessage)
        }
    }

    @Test
    fun `network failure shows safe transient feedback`() = runTest {
        val viewModel = validViewModel(FakeAccountApi { throw IOException("Private host details") })

        viewModel.login()
        runCurrent()

        assertEquals(R.string.network_error, viewModel.uiState.snackbarMessage)
        assertFalse(viewModel.uiState.isLoginSuccessful)
        assertNull(viewModel.uiState.loginError)
        assertFalse(viewModel.uiState.isLoading)
    }

    @Test
    fun `unexpected failure shows safe transient feedback`() = runTest {
        val viewModel = validViewModel(FakeAccountApi {
            throw IllegalStateException("Private exception details")
        })

        viewModel.login()
        runCurrent()

        assertEquals(R.string.unexpected_error, viewModel.uiState.snackbarMessage)
        assertFalse(viewModel.uiState.isLoginSuccessful)
        assertNull(viewModel.uiState.loginError)
        assertFalse(viewModel.uiState.isLoading)
    }

    @Test
    fun `cancellation resets loading without showing a failure`() = runTest {
        val viewModel = validViewModel(FakeAccountApi { throw CancellationException() })

        viewModel.login()
        runCurrent()

        assertFalse(viewModel.uiState.isLoginSuccessful)
        assertFalse(viewModel.uiState.isLoading)
        assertNull(viewModel.uiState.loginError)
        assertNull(viewModel.uiState.snackbarMessage)
    }

    private fun validViewModel(api: AccountApi) = LoginViewModel(api).apply {
        onEmailChange("person@example.com")
        onPasswordChange("password")
    }

    private class FakeAccountApi(
        private val result: suspend () -> Unit = { Unit }
    ) : AccountApi {
        val requests = mutableListOf<LoginRequest>()

        override suspend fun login(request: LoginRequest): Unit {
            requests += request
            return result()
        }

        override suspend fun createAccount(request: CreateAccountRequest): Unit {
            error("Account creation is not used by login tests.")
        }
    }
}
