package io.github.bigreddog33.homeharmony.ui.screens.createAccount

import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.setValue
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope

import io.github.bigreddog33.homeharmony.R
import io.github.bigreddog33.homeharmony.data.account.AccountApi
import io.github.bigreddog33.homeharmony.data.account.dto.CreateAccountRequest
import io.github.bigreddog33.homeharmony.data.network.ApiClient
import io.github.bigreddog33.homeharmony.data.network.ApiResult
import io.github.bigreddog33.homeharmony.data.network.apiCall
import io.github.bigreddog33.homeharmony.data.account.parseEmailConfirmed

import kotlinx.coroutines.launch

class CreateAccountViewModel(
    private val accountApi: AccountApi = ApiClient.create(AccountApi::class.java)
) : ViewModel() {
    var uiState by mutableStateOf(CreateAccountUiState())
        private set

    fun onEmailChange(email: String) {
        uiState = uiState.copy(email = email, emailError = null, emailConfirmError = null, createAccountError = null, existingAccountEmail = null, existingAccountEmailConfirmed = null)
    }

    fun onEmailConfirmChange(emailConfirm: String) {
        uiState = uiState.copy(emailConfirm = emailConfirm, emailConfirmError = null, createAccountError = null, existingAccountEmail = null, existingAccountEmailConfirmed = null)
    }

    fun onPasswordChange(password: String) {
        uiState = uiState.copy(password = password, passwordError = null, passwordConfirmError = null, createAccountError = null, existingAccountEmail = null, existingAccountEmailConfirmed = null)
    }

    fun onPasswordConfirmChange(passwordConfirm: String) {
        uiState = uiState.copy(passwordConfirm = passwordConfirm, passwordConfirmError = null, createAccountError = null, existingAccountEmail = null, existingAccountEmailConfirmed = null)
    }

    fun onSnackbarShown() {
        uiState = uiState.copy(snackbarMessage = null)
    }

    fun createAccount() {
        if (uiState.isLoading || uiState.isCreateAccountSuccessful) return

        val validation = validateCreateAccount(uiState.email, uiState.password, uiState.emailConfirm, uiState.passwordConfirm)
        uiState = uiState.copy(
            emailError = validation.email,
            passwordError = validation.password,
            emailConfirmError = validation.emailConfirm,
            passwordConfirmError = validation.passwordConfirm,
            createAccountError = null,
            snackbarMessage = null, 
            existingAccountEmail = null, 
            existingAccountEmailConfirmed = null
        )
        if (!validation.isValid) return

        val email = uiState.email.trim()
        val password = uiState.password
        uiState = uiState.copy(isLoading = true)

        viewModelScope.launch {
            val result = try {
                apiCall { accountApi.createAccount(CreateAccountRequest(email, password)) }
            } finally {
                uiState = uiState.copy(isLoading = false)
            }

            when (result) {
                is ApiResult.Success -> {
                    val status = result.value.status
                    
                    uiState = when (status) { 
                        "Created", "CreatedConfirmationFailed" -> uiState.copy(isCreateAccountSuccessful=true, confirmationStatus=status, confirmationEmail = email)
                        else -> uiState.copy(snackbarMessage = R.string.unexpected_error)
                    }
                }

                is ApiResult.HttpError -> {
                    uiState = when (result.code) {
                        400 -> uiState.copy(createAccountError = R.string.createAccount_invalid_request)
                        401, 403 -> uiState.copy(createAccountError = R.string.createAccount_failed)
                        409 -> {
                                val emailConfirmed = parseEmailConfirmed(result.body)

                                uiState.copy(
                                    createAccountError = when (emailConfirmed) {
                                        false -> R.string.createAccount_existing_unconfirmed
                                        true -> R.string.createAccount_existing_confirmed
                                        null -> R.string.createAccount_email_in_use
                                    },
                                    existingAccountEmail = email,
                                    existingAccountEmailConfirmed = emailConfirmed
                                )
                            }
                        429 -> uiState.copy(snackbarMessage = R.string.too_many_requests)
                        else -> uiState.copy(snackbarMessage = R.string.createAccount_server_error)
                    }
                }

                ApiResult.NetworkError -> {
                    uiState = uiState.copy(snackbarMessage = R.string.network_error)
                }

                ApiResult.UnexpectedError -> {
                    uiState = uiState.copy(snackbarMessage = R.string.unexpected_error)
                }
            }
        }
    }
}
