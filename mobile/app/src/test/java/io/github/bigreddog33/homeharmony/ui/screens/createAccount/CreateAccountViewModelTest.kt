package io.github.bigreddog33.homeharmony.ui.screens.createAccount

import io.github.bigreddog33.homeharmony.R
import io.github.bigreddog33.homeharmony.data.account.dto.CreateAccountRequest
import io.github.bigreddog33.homeharmony.data.account.dto.CreateAccountResponse
import io.github.bigreddog33.homeharmony.support.AccountApiFake
import io.github.bigreddog33.homeharmony.support.MainDispatcherRule
import kotlinx.coroutines.CancellationException
import kotlinx.coroutines.CompletableDeferred
import kotlinx.coroutines.ExperimentalCoroutinesApi
import kotlinx.coroutines.test.runCurrent
import kotlinx.coroutines.test.runTest
import okhttp3.ResponseBody.Companion.toResponseBody
import org.junit.Assert.*
import org.junit.Rule
import org.junit.Test
import retrofit2.HttpException
import retrofit2.Response
import java.io.IOException
import java.net.SocketTimeoutException

@OptIn(ExperimentalCoroutinesApi::class)
class CreateAccountViewModelTest {
    @get:Rule val mainDispatcher = MainDispatcherRule()

    @Test
    fun invalidInputNeverCallsApiAndCanBeCorrected() = runTest {
        val api = AccountApiFake()
        val vm = CreateAccountViewModel(api)
        vm.createAccount()
        runCurrent()

        assertEquals(R.string.email_required, vm.uiState.emailError)
        assertEquals(R.string.email_required, vm.uiState.emailConfirmError)
        assertEquals(R.string.password_required, vm.uiState.passwordError)
        assertEquals(R.string.password_required, vm.uiState.passwordConfirmError)
        assertTrue(api.creations.isEmpty())
        assertFalse(vm.uiState.isLoading)

        fill(vm)
        vm.createAccount()
        runCurrent()
        assertEquals(1, api.creations.size)
        assertTrue(vm.uiState.isCreateAccountSuccessful)
    }

    @Test
    fun changingAnOriginalFieldRechecksConfirmationOnNextSubmission() = runTest {
        for (change in listOf<(CreateAccountViewModel) -> Unit>(
            { it.onEmailChange("other@example.com") },
            { it.onPasswordChange("Different1!") }
        )) {
            val api = AccountApiFake()
            val vm = validVm(api)
            change(vm)
            vm.createAccount()
            runCurrent()
            assertTrue(api.creations.isEmpty())
            assertTrue(vm.uiState.emailConfirmError != null || vm.uiState.passwordConfirmError != null)

            fill(vm)
            vm.createAccount()
            runCurrent()
            assertTrue(vm.uiState.isCreateAccountSuccessful)
        }
    }

    @Test
    fun editingClearsRelatedErrorsWithoutClearingUnrelatedErrors() {
        val vm = CreateAccountViewModel(AccountApiFake())
        vm.createAccount()
        vm.onEmailChange(email)
        assertNull(vm.uiState.emailError)
        assertNull(vm.uiState.emailConfirmError)
        assertEquals(R.string.password_required, vm.uiState.passwordError)
        assertEquals(R.string.password_required, vm.uiState.passwordConfirmError)
        vm.onPasswordChange(password)
        assertNull(vm.uiState.passwordError)
        assertNull(vm.uiState.passwordConfirmError)
    }

    @Test
    fun bothCreationOutcomesPreserveSubmittedEmailAndPasswordWhitespace() = runTest {
        for (status in listOf("Created", "CreatedConfirmationFailed")) {
            val api = AccountApiFake().apply { create = { CreateAccountResponse(status) } }
            val vm = validVm(api)
            vm.onEmailChange(" $email ")
            vm.onPasswordChange(" $password ")
            vm.onPasswordConfirmChange(" $password ")

            vm.createAccount()
            assertTrue(vm.uiState.isLoading)
            assertFalse(vm.uiState.isCreateAccountSuccessful)
            runCurrent()

            assertEquals(listOf(CreateAccountRequest(email, " $password ")), api.creations)
            assertTrue(vm.uiState.isCreateAccountSuccessful)
            assertEquals(email, vm.uiState.confirmationEmail)
            assertEquals(status, vm.uiState.confirmationStatus)
            assertFalse(vm.uiState.isLoading)
            assertNull(vm.uiState.createAccountError)
            assertNull(vm.uiState.snackbarMessage)
            assertTrue(api.resends.isEmpty())
        }
    }

    @Test
    fun duplicateSubmissionsAreIgnoredWhilePendingAndAfterSuccess() = runTest {
        val pending = CompletableDeferred<CreateAccountResponse>()
        val api = AccountApiFake().apply { create = { pending.await() } }
        val vm = validVm(api)
        vm.createAccount()
        vm.createAccount()
        runCurrent()
        assertEquals(1, api.creations.size)
        assertTrue(vm.uiState.isLoading)
        assertNull(vm.uiState.confirmationEmail)

        // No screen observer is needed for the delayed result to remain available.
        pending.complete(CreateAccountResponse("Created"))
        runCurrent()
        vm.createAccount()
        runCurrent()
        assertEquals(1, api.creations.size)
        assertTrue(vm.uiState.isCreateAccountSuccessful)
        assertEquals(email, vm.uiState.confirmationEmail)
    }

    @Test
    fun invalidSuccessStatusesShowSafeFeedbackAndAllowRetry() = runTest {
        for (status in listOf(null, "", "Unknown", "AlreadyExists")) {
            val api = AccountApiFake().apply { create = { CreateAccountResponse(status) } }
            val vm = validVm(api)
            vm.createAccount()
            runCurrent()
            assertFailed(vm)
            assertEquals(R.string.unexpected_error, vm.uiState.snackbarMessage)
            api.create = { CreateAccountResponse("Created") }
            vm.createAccount()
            assertNull(vm.uiState.snackbarMessage)
            runCurrent()
            assertTrue(vm.uiState.isCreateAccountSuccessful)
        }
    }

    @Test
    fun duplicateResponsesChooseOnlyTheExplicitRecoveryState() = runTest {
        val cases = listOf(
            Triple("""{"emailConfirmed":false}""", false, R.string.createAccount_existing_unconfirmed),
            Triple("""{"emailConfirmed":true}""", true, R.string.createAccount_existing_confirmed),
            Triple("{}", null, R.string.createAccount_email_in_use),
            Triple("""{"emailConfirmed":"false"}""", null, R.string.createAccount_email_in_use),
            Triple("not JSON", null, R.string.createAccount_email_in_use)
        )
        for ((body, confirmed, message) in cases) {
            val api = AccountApiFake().apply { create = { throw httpError(409, body) } }
            val vm = validVm(api)
            vm.onEmailChange(" $email ")
            vm.createAccount()
            runCurrent()
            assertFailed(vm)
            assertEquals(message, vm.uiState.createAccountError)
            assertEquals(email, vm.uiState.existingAccountEmail)
            assertEquals(confirmed, vm.uiState.existingAccountEmailConfirmed)
            assertNull(vm.uiState.snackbarMessage)
            assertTrue(api.resends.isEmpty())
        }
    }

    @Test
    fun everyFieldEditClearsThePreviousRecoveryContext() = runTest {
        val edits = listOf<(CreateAccountViewModel) -> Unit>(
            { it.onEmailChange("new@example.com") },
            { it.onEmailConfirmChange("new@example.com") },
            { it.onPasswordChange("Different1!") },
            { it.onPasswordConfirmChange("Different1!") }
        )
        for (edit in edits) {
            val api = AccountApiFake().apply {
                create = { throw httpError(409, """{"emailConfirmed":false}""") }
            }
            val vm = validVm(api)
            vm.createAccount()
            runCurrent()
            assertEquals(email, vm.uiState.existingAccountEmail)
            edit(vm)
            assertNull(vm.uiState.createAccountError)
            assertNull(vm.uiState.existingAccountEmail)
            assertNull(vm.uiState.existingAccountEmailConfirmed)
        }
    }

    @Test
    fun resubmittingClearsOldRecoveryAndUsesTheNewEmail() = runTest {
        val api = AccountApiFake().apply {
            create = { throw httpError(409, """{"emailConfirmed":false}""") }
        }
        val vm = validVm(api)
        vm.createAccount()
        runCurrent()
        vm.onEmailChange(" new@example.com ")
        vm.onEmailConfirmChange("new@example.com")
        vm.createAccount()
        assertNull(vm.uiState.existingAccountEmail)
        assertNull(vm.uiState.createAccountError)
        runCurrent()
        assertEquals("new@example.com", api.creations.last().email)
        assertEquals("new@example.com", vm.uiState.existingAccountEmail)
    }

    @Test
    fun httpFormErrorsPermitSuccessfulRetry() = runTest {
        for ((code, message) in listOf(
            400 to R.string.createAccount_invalid_request,
            401 to R.string.createAccount_failed,
            403 to R.string.createAccount_failed
        )) {
            val api = AccountApiFake().apply { create = { throw httpError(code) } }
            val vm = validVm(api)
            vm.createAccount()
            runCurrent()
            assertFailed(vm)
            assertEquals(message, vm.uiState.createAccountError)
            assertNull(vm.uiState.snackbarMessage)

            api.create = { CreateAccountResponse("Created") }
            vm.createAccount()
            assertNull(vm.uiState.createAccountError)
            runCurrent()
            assertTrue(vm.uiState.isCreateAccountSuccessful)
            assertEquals(2, api.creations.size)
        }
    }

    @Test
    fun transientErrorsCanBeConsumedAndShownAgain() = runTest {
        val cases = listOf(
            httpError(429) to R.string.too_many_requests,
            httpError(500) to R.string.createAccount_server_error,
            httpError(503) to R.string.createAccount_server_error,
            IOException("Private host") to R.string.network_error,
            SocketTimeoutException("Private timeout") to R.string.network_error,
            IllegalStateException("Private details") to R.string.unexpected_error
        )
        for ((failure, message) in cases) {
            val api = AccountApiFake().apply { create = { throw failure } }
            val vm = validVm(api)
            repeat(2) {
                vm.createAccount()
                runCurrent()
                assertFailed(vm)
                assertEquals(message, vm.uiState.snackbarMessage)
                assertNull(vm.uiState.createAccountError)
                vm.onSnackbarShown()
                assertNull(vm.uiState.snackbarMessage)
            }
        }
    }

    @Test
    fun cancellingPendingCreationClearsLoadingWithoutErrorAndAllowsRetry() = runTest {
        val pending = CompletableDeferred<CreateAccountResponse>()
        val api = AccountApiFake().apply { create = { pending.await() } }
        val vm = validVm(api)
        vm.createAccount()
        runCurrent()
        pending.cancel(CancellationException("Leaving screen"))
        runCurrent()
        assertFailed(vm)
        assertNull(vm.uiState.createAccountError)
        assertNull(vm.uiState.snackbarMessage)

        api.create = { CreateAccountResponse("Created") }
        vm.createAccount()
        runCurrent()
        assertTrue(vm.uiState.isCreateAccountSuccessful)
    }

    private fun assertFailed(vm: CreateAccountViewModel) {
        assertFalse(vm.uiState.isLoading)
        assertFalse(vm.uiState.isCreateAccountSuccessful)
        assertNull(vm.uiState.confirmationEmail)
        assertNull(vm.uiState.confirmationStatus)
    }

    private fun validVm(api: AccountApiFake) = CreateAccountViewModel(api).also(::fill)

    private fun fill(vm: CreateAccountViewModel) {
        vm.onEmailChange(email)
        vm.onEmailConfirmChange(email)
        vm.onPasswordChange(password)
        vm.onPasswordConfirmChange(password)
    }

    private fun httpError(code: Int, body: String = "Private server details") =
        HttpException(Response.error<Unit>(code, body.toResponseBody()))

    private val email = "person@example.com"
    private val password = "Password1!"
}
