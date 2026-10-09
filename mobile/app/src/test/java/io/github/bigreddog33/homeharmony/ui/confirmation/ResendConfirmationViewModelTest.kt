package io.github.bigreddog33.homeharmony.ui.confirmation

import androidx.lifecycle.SavedStateHandle
import io.github.bigreddog33.homeharmony.R
import io.github.bigreddog33.homeharmony.data.account.dto.ResendConfirmationRequest
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
class ResendConfirmationViewModelTest {
    @get:Rule val mainDispatcher = MainDispatcherRule()

    @Test
    fun blankEmailDoesNotSendAndCanBeCorrected() = runTest {
        val api = AccountApiFake()
        val vm = ResendConfirmationViewModel(SavedStateHandle(), api)
        for (blank in listOf("", " ", "\t\n")) {
            vm.resend(blank)
            runCurrent()
            assertEquals(R.string.email_required, vm.uiState.errorMessage)
            assertFalse(vm.uiState.isLoading)
            assertTrue(api.resends.isEmpty())
        }
        vm.resend(" user@example.com ")
        assertNull(vm.uiState.errorMessage)
        runCurrent()
        assertEquals(listOf(ResendConfirmationRequest("user@example.com")), api.resends)
        assertTrue(vm.uiState.isSuccessful)
        assertTrue(api.creations.isEmpty())
    }

    @Test
    fun pendingAndSuccessfulRequestsCannotBeSubmittedTwice() = runTest {
        val pending = CompletableDeferred<Unit>()
        val api = AccountApiFake().apply { resend = { pending.await() } }
        val vm = ResendConfirmationViewModel(SavedStateHandle(), api)
        vm.resend(email)
        vm.resend(email)
        runCurrent()
        assertTrue(vm.uiState.isLoading)
        assertFalse(vm.uiState.isSuccessful)
        assertEquals(1, api.resends.size)

        pending.complete(Unit)
        runCurrent()
        vm.resend(email)
        runCurrent()
        assertEquals(1, api.resends.size)
        assertTrue(vm.uiState.isSuccessful)
        assertFalse(vm.uiState.isLoading)
        assertNull(vm.uiState.errorMessage)
    }

    @Test
    fun failuresShowSafeFeedbackAndAllowSuccessfulRetry() = runTest {
        val failures = listOf(
            httpError(400) to R.string.confirmation_resend_failed,
            httpError(401) to R.string.confirmation_resend_failed,
            httpError(403) to R.string.confirmation_resend_failed,
            httpError(404) to R.string.confirmation_resend_failed,
            httpError(429) to R.string.too_many_requests,
            httpError(500) to R.string.confirmation_resend_failed,
            httpError(503) to R.string.confirmation_resend_failed,
            IOException("Private host") to R.string.network_error,
            SocketTimeoutException("Private timeout") to R.string.network_error,
            IllegalStateException("Private details") to R.string.unexpected_error
        )
        for ((failure, message) in failures) {
            val handle = SavedStateHandle()
            val api = AccountApiFake().apply { resend = { throw failure } }
            val vm = ResendConfirmationViewModel(handle, api)
            vm.resend(email)
            runCurrent()
            assertEquals(message, vm.uiState.errorMessage)
            assertFalse(vm.uiState.isLoading)
            assertFalse(vm.uiState.isSuccessful)
            assertFalse(ResendConfirmationViewModel(copy(handle), api).uiState.isSuccessful)

            val pending = CompletableDeferred<Unit>()
            api.resend = { pending.await() }
            vm.resend(email)
            assertNull(vm.uiState.errorMessage)
            assertTrue(vm.uiState.isLoading)
            runCurrent()
            pending.complete(Unit)
            runCurrent()
            assertTrue(vm.uiState.isSuccessful)
            assertFalse(vm.uiState.isLoading)
            assertEquals(2, api.resends.size)
        }
    }

    @Test
    fun successIsRestoredFromSavedValuesWithoutSendingAgain() = runTest {
        val handle = SavedStateHandle()
        val api = AccountApiFake()
        val vm = ResendConfirmationViewModel(handle, api)
        vm.resend(email)
        runCurrent()

        // A fresh handle and VM simulate consuming restored values, not Activity recreation.
        val restored = ResendConfirmationViewModel(copy(handle), api)
        assertTrue(restored.uiState.isSuccessful)
        assertFalse(restored.uiState.isLoading)
        assertNull(restored.uiState.errorMessage)
        restored.resend(email)
        runCurrent()
        assertEquals(1, api.resends.size)
    }

    @Test
    fun aDifferentAccountUsesAnIndependentSavedState() = runTest {
        val api = AccountApiFake()
        val first = ResendConfirmationViewModel(SavedStateHandle(), api)
        first.resend(email)
        runCurrent()
        val second = ResendConfirmationViewModel(SavedStateHandle(), api)
        assertFalse(second.uiState.isSuccessful)
        second.resend("other@example.com")
        runCurrent()
        assertEquals(listOf(email, "other@example.com"), api.resends.map { it.email })
    }

    @Test
    fun cancellationClearsLoadingWithoutSavingSuccessOrDisplayingFailure() = runTest {
        val pending = CompletableDeferred<Unit>()
        val handle = SavedStateHandle()
        val api = AccountApiFake().apply { resend = { pending.await() } }
        val vm = ResendConfirmationViewModel(handle, api)
        vm.resend(email)
        runCurrent()
        pending.cancel(CancellationException("Leaving screen"))
        runCurrent()
        assertFalse(vm.uiState.isLoading)
        assertFalse(vm.uiState.isSuccessful)
        assertNull(vm.uiState.errorMessage)
        assertFalse(ResendConfirmationViewModel(copy(handle), api).uiState.isSuccessful)

        api.resend = {}
        vm.resend(email)
        runCurrent()
        assertTrue(vm.uiState.isSuccessful)
    }

    private fun copy(handle: SavedStateHandle) =
        SavedStateHandle(handle.keys().associateWith { handle.get<Any?>(it) })

    private fun httpError(code: Int) =
        HttpException(Response.error<Unit>(code, "Private details".toResponseBody()))

    private val email = "person@example.com"
}
