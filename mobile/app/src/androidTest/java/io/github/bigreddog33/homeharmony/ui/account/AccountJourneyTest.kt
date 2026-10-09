package io.github.bigreddog33.homeharmony.ui.account

import androidx.activity.ComponentActivity
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.requiredSize
import androidx.compose.runtime.CompositionLocalProvider
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalDensity
import androidx.compose.ui.semantics.SemanticsProperties
import androidx.compose.ui.text.AnnotatedString
import androidx.compose.ui.unit.Density
import androidx.compose.ui.unit.dp
import androidx.compose.ui.test.*
import androidx.compose.ui.test.junit4.createAndroidComposeRule
import androidx.lifecycle.Lifecycle
import androidx.lifecycle.createSavedStateHandle
import androidx.lifecycle.viewmodel.initializer
import androidx.lifecycle.viewmodel.viewModelFactory
import androidx.test.ext.junit.runners.AndroidJUnit4
import io.github.bigreddog33.homeharmony.R
import io.github.bigreddog33.homeharmony.data.account.AccountApi
import io.github.bigreddog33.homeharmony.data.account.dto.CreateAccountRequest
import io.github.bigreddog33.homeharmony.data.account.dto.CreateAccountResponse
import io.github.bigreddog33.homeharmony.data.account.dto.LoginRequest
import io.github.bigreddog33.homeharmony.data.account.dto.ResendConfirmationRequest
import io.github.bigreddog33.homeharmony.ui.confirmation.ResendConfirmationViewModel
import io.github.bigreddog33.homeharmony.ui.navigation.AppNavigation
import io.github.bigreddog33.homeharmony.ui.screens.createAccount.CreateAccountViewModel
import io.github.bigreddog33.homeharmony.ui.screens.login.LoginViewModel
import io.github.bigreddog33.homeharmony.ui.theme.HomeHarmonyTheme
import kotlinx.coroutines.CompletableDeferred
import okhttp3.ResponseBody.Companion.toResponseBody
import org.junit.Assert.*
import org.junit.Rule
import org.junit.Test
import org.junit.runner.RunWith
import retrofit2.HttpException
import retrofit2.Response
import java.io.IOException

@RunWith(AndroidJUnit4::class)
class AccountJourneyTest {
    @get:Rule val compose = createAndroidComposeRule<ComponentActivity>()
    private val api = FakeApi()

    @Test
    fun requiredFieldsBlockSubmissionThenCorrectionSucceedsWithMaskedPasswords() {
        openRegistration()
        click(R.string.createAccount_button)
        compose.onAllNodesWithText(text(R.string.email_required)).assertCountEquals(2)
        compose.onAllNodesWithText(text(R.string.password_required)).assertCountEquals(2)
        compose.runOnIdle { assertTrue(api.creations.isEmpty()) }

        fill()
        field(R.string.password_label).assert(
            SemanticsMatcher.keyIsDefined(SemanticsProperties.Password))
        field(R.string.password_confirm_label).assert(
            SemanticsMatcher.keyIsDefined(SemanticsProperties.Password))
        click(R.string.createAccount_button)
        node(R.string.confirmation_sent).assertExists()
    }

    @Test
    fun keyboardSubmitAndRepeatedClickSendOnceAndDisableTheFormWhilePending() {
        val pending = CompletableDeferred<CreateAccountResponse>()
        api.create = { pending.await() }
        openRegistration()
        fill()
        field(R.string.password_confirm_label).performImeAction()
        node(R.string.createAccount_loading).assertIsNotEnabled().performClick()
        fieldLabels.forEach { field(it).assertIsNotEnabled() }
        node(R.string.createAccount_login).assertIsNotEnabled()
        compose.runOnIdle {
            assertEquals(1, api.creations.size)
            pending.complete(CreateAccountResponse("Created"))
        }
        node(R.string.confirmation_sent).assertExists()
        compose.runOnIdle { assertEquals(1, api.creations.size) }
    }

    @Test
    fun createdNavigatesWithEncodedEmailAndReturningToLoginDropsTheOldForm() {
        openRegistration()
        fill()
        click(R.string.createAccount_button)
        compose.onNodeWithText(compose.activity.getString(R.string.confirmation_email, email))
            .assertExists()
        node(R.string.confirmation_sent).assertExists()
        node(R.string.confirmation_resend_button).assertDoesNotExist()

        // System back must go straight to login, not the submitted form.
        compose.runOnUiThread { compose.activity.onBackPressedDispatcher.onBackPressed() }
        node(R.string.login_welcome).assertExists()
        click(R.string.createAccount_button)
        field(R.string.email_label).assert(
            SemanticsMatcher.expectValue(SemanticsProperties.EditableText, AnnotatedString("")))
        compose.runOnIdle { assertEquals(1, api.creations.size) }
    }

    @Test
    fun confirmationFailureStillMeansCreatedAndResendChangesInstructions() {
        api.create = { CreateAccountResponse("CreatedConfirmationFailed") }
        openRegistration()
        fill()
        click(R.string.createAccount_button)
        node(R.string.confirmation_account_created).assertExists()
        node(R.string.confirmation_failed).assertExists()
        node(R.string.confirmation_sent).assertDoesNotExist()

        click(R.string.confirmation_resend_button)
        node(R.string.confirmation_resend_success).assertExists()
        node(R.string.confirmation_failed).assertDoesNotExist()
        node(R.string.confirmation_sent).assertExists()
        node(R.string.confirmation_resend_button).assertDoesNotExist()
        compose.runOnIdle {
            assertEquals(listOf(ResendConfirmationRequest(email)), api.resends)
            assertEquals(1, api.creations.size)
        }
        click(R.string.confirmation_go_to_login)
        node(R.string.login_welcome).assertExists()
    }

    @Test
    fun duplicateUnconfirmedResendsWithoutRegisteringAndEditingRemovesOldFeedback() {
        api.create = { throw conflict("false") }
        val pending = CompletableDeferred<Unit>()
        api.resend = { pending.await() }
        openRegistration()
        fill()
        click(R.string.createAccount_button)
        node(R.string.createAccount_existing_unconfirmed).assertExists()
        node(R.string.reset_password).assertDoesNotExist()

        click(R.string.confirmation_resend_button)
        node(R.string.confirmation_resending).assertIsNotEnabled().performClick()
        node(R.string.createAccount_button).assertIsNotEnabled().performClick()
        fieldLabels.forEach { field(it).assertIsNotEnabled() }
        node(R.string.createAccount_login).assertIsNotEnabled()
        compose.runOnIdle {
            assertEquals(1, api.resends.size)
            assertEquals(1, api.creations.size)
            pending.complete(Unit)
        }
        node(R.string.confirmation_resend_success).assertExists()
        node(R.string.confirmation_resend_button).assertDoesNotExist()

        replace(R.string.email_label, "new@example.com")
        node(R.string.confirmation_resend_success).assertDoesNotExist()
        node(R.string.createAccount_existing_unconfirmed).assertDoesNotExist()
        replace(R.string.email_confirm_label, "new@example.com")
        click(R.string.createAccount_button)
        click(R.string.confirmation_resend_button)
        compose.runOnIdle {
            assertEquals(listOf(email, "new@example.com"), api.resends.map { it.email })
            assertEquals(2, api.creations.size)
        }
    }

    @Test
    fun duplicateConfirmedShowsDisabledResetAndNoResend() {
        api.create = { throw conflict("true") }
        openRegistration()
        fill()
        click(R.string.createAccount_button)
        node(R.string.createAccount_existing_confirmed).assertExists()
        node(R.string.reset_password).assertIsNotEnabled()
        node(R.string.reset_password_unavailable).assertExists()
        node(R.string.confirmation_resend_button).assertDoesNotExist()
        compose.runOnIdle { assertTrue(api.resends.isEmpty()) }
    }

    @Test
    fun unknownDuplicateStateDoesNotOfferAnAssumedRecoveryAction() {
        api.create = { throw conflict("null") }
        openRegistration()
        fill()
        click(R.string.createAccount_button)
        node(R.string.createAccount_email_in_use).assertExists()
        node(R.string.reset_password).assertDoesNotExist()
        node(R.string.confirmation_resend_button).assertDoesNotExist()
        compose.runOnIdle { assertTrue(api.resends.isEmpty()) }
    }

    @Test
    fun recoveryResendFailureAllowsRetryAndClearsItsError() {
        api.create = { throw conflict("false") }
        api.resend = { throw IOException("Private host") }
        openRegistration()
        fill()
        click(R.string.createAccount_button)
        click(R.string.confirmation_resend_button)
        node(R.string.network_error).assertExists()
        node(R.string.confirmation_resend_button).assertIsEnabled()
        compose.runOnIdle { api.resend = {} }
        click(R.string.confirmation_resend_button)
        node(R.string.network_error).assertDoesNotExist()
        node(R.string.confirmation_resend_success).assertExists()
        node(R.string.createAccount_welcome).assertExists()
        compose.runOnIdle {
            assertEquals(1, api.creations.size)
            assertEquals(2, api.resends.size)
        }
    }

    @Test
    fun successPageResendFailureRetainsInstructionsUntilRetryCompletes() {
        api.create = { CreateAccountResponse("CreatedConfirmationFailed") }
        api.resend = { throw IOException("Private host") }
        openRegistration()
        fill()
        click(R.string.createAccount_button)
        click(R.string.confirmation_resend_button)
        node(R.string.network_error).assertExists()
        node(R.string.confirmation_failed).assertExists()

        val pending = CompletableDeferred<Unit>()
        compose.runOnIdle { api.resend = { pending.await() } }
        click(R.string.confirmation_resend_button)
        node(R.string.network_error).assertDoesNotExist()
        node(R.string.confirmation_resending).assertIsNotEnabled().performClick()
        node(R.string.confirmation_failed).assertExists()
        compose.runOnIdle {
            assertEquals(2, api.resends.size)
            pending.complete(Unit)
        }
        node(R.string.confirmation_sent).assertExists()
        node(R.string.confirmation_resend_button).assertDoesNotExist()
    }

    @Test
    fun resultWhileBackgroundedNavigatesOnResumeWithoutRepeatingRegistration() {
        val pending = CompletableDeferred<CreateAccountResponse>()
        api.create = { pending.await() }
        openRegistration()
        fill()
        click(R.string.createAccount_button)
        compose.activityRule.scenario.moveToState(Lifecycle.State.CREATED)
        pending.complete(CreateAccountResponse("Created"))
        compose.activityRule.scenario.moveToState(Lifecycle.State.RESUMED)
        node(R.string.confirmation_sent).assertExists()

        compose.activityRule.scenario.moveToState(Lifecycle.State.CREATED)
        compose.activityRule.scenario.moveToState(Lifecycle.State.RESUMED)
        node(R.string.confirmation_sent).assertExists()
        compose.runOnIdle { assertEquals(1, api.creations.size) }
        click(R.string.confirmation_go_to_login)
        node(R.string.login_welcome).assertExists()
    }

    @Test
    fun backToLoginDoesNotSubmitRegistration() {
        openRegistration()
        click(R.string.createAccount_login)
        node(R.string.login_welcome).assertExists()
        compose.runOnIdle {
            assertTrue(api.creations.isEmpty())
            assertTrue(api.resends.isEmpty())
        }
    }

    @Test
    fun compactLayoutWithLargeTextCanSubmitAndReachLoginFromSuccess() {
        openRegistration(compact = true)
        fill()
        click(R.string.createAccount_button)
        node(R.string.confirmation_sent).performScrollTo().assertIsDisplayed()
        click(R.string.confirmation_go_to_login)
        node(R.string.login_welcome).assertExists()
    }

    private fun openRegistration(compact: Boolean = false) {
        val factory = viewModelFactory {
            initializer { LoginViewModel(api) }
            initializer { CreateAccountViewModel(api) }
            initializer { ResendConfirmationViewModel(createSavedStateHandle(), api) }
        }
        compose.setContent {
            val density = LocalDensity.current
            CompositionLocalProvider(
                LocalDensity provides Density(density.density, if (compact) 1.5f else density.fontScale)
            ) {
                HomeHarmonyTheme {
                    Box(if (compact) Modifier.requiredSize(320.dp, 480.dp) else Modifier) {
                        AppNavigation(viewModelFactory = factory)
                    }
                }
            }
        }
        click(R.string.createAccount_button)
        node(R.string.createAccount_welcome).assertExists()
    }

    private fun fill() {
        replace(R.string.email_label, " $email ")
        replace(R.string.email_confirm_label, email)
        replace(R.string.password_label, "Password1!")
        replace(R.string.password_confirm_label, "Password1!")
    }

    private fun replace(label: Int, value: String) {
        field(label).performScrollTo().performTextReplacement(value)
    }

    private fun field(label: Int) =
        compose.onNode(hasSetTextAction() and hasText(text(label)))

    private fun click(label: Int) { node(label).performScrollTo().performClick() }
    private fun node(label: Int) = compose.onNodeWithText(text(label))
    private fun text(id: Int) = compose.activity.getString(id)

    private fun conflict(flag: String) = HttpException(Response.error<Unit>(
        409, """{"emailConfirmed":$flag}""".toResponseBody()))

    private val email = "user+home@example.com"
    private val fieldLabels = listOf(R.string.email_label, R.string.email_confirm_label,
        R.string.password_label, R.string.password_confirm_label)

    // No HTTP client or email delivery is used by these journeys.
    private class FakeApi : AccountApi {
        val creations = mutableListOf<CreateAccountRequest>()
        val resends = mutableListOf<ResendConfirmationRequest>()
        var create: suspend () -> CreateAccountResponse = { CreateAccountResponse("Created") }
        var resend: suspend () -> Unit = {}

        override suspend fun createAccount(request: CreateAccountRequest): CreateAccountResponse {
            creations += request
            return create()
        }

        override suspend fun resendConfirmation(request: ResendConfirmationRequest) {
            resends += request
            resend()
        }

        override suspend fun login(request: LoginRequest) {
            error("These journeys must not submit login.")
        }
    }
}
