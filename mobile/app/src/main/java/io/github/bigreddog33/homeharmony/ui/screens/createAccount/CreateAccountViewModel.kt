package io.github.bigreddog33.homeharmony.ui.screens.createAccount

import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.setValue
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope

import io.github.bigreddog33.homeharmony.R
import io.github.bigreddog33.homeharmony.R
import io.github.bigreddog33.homeharmony.data.account.AccountApi
import io.github.bigreddog33.homeharmony.data.account.dto.CreateAccountRequest
import io.github.bigreddog33.homeharmony.data.network.ApiClient
import io.github.bigreddog33.homeharmony.data.network.ApiResult
import io.github.bigreddog33.homeharmony.data.network.apiCall

import kotlinx.coroutines.launch

class CreateAccountViewModel(
    private val accountApi: AccountApi = ApiClient.create(AccountApi::class.java)
) : ViewModel() {
    var uiState by mutableStateOf(CreateAccountUiState())
        private set

    fun onEmailChange(email: String) {
        uiState = uiState.copy(email = email, emailError = null, createAccountError = null)
    }

    fun onEmailConfirmChange(emailConfirm: String) {
        uiState = uiState.copy(emailConfirm = email, emailErrorConfirm = null, createAccountError = null)
    }

    fun onPasswordChange(password: String) {
        uiState = uiState.copy(password = password, passwordError = null, createAccountError = null)
    }

    fun onPasswordConfirmChange(password: String) {
        uiState = uiState.copy(passwordConfirm = password, passwordErrorConfirm = null, createAccountError = null)
    }

    fun onSnackbarShown() {
        uiState = uiState.copy(snackbarMessage = null)
    }

    fun createAccount() {
        if (uiState.isLoading || uiState.isCreateAccountSuccessful) return

        val validation = validateLogin(uiState.email, uiState.password, uiState.emailConfirm, uiState.passwordConfirm)
        uiState = uiState.copy(
            emailError = validation.email,
            passwordError = validation.password,
            emailConfirmError = validation.emailConfirm,
            passwordConfirmError = validation.passwordConfirm,
            createAccountError = null,
            snackbarMessage = null
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
                    uiState = uiState.copy(isCreateAccountSuccessful = true)
                }

                is ApiResult.HttpError -> {
                    uiState = when (result.code) {
                        400 -> uiState.copy(createAccountError = R.string.createAccount_invalid_request)
                        401 -> uiState.copy(createAccountError = R.string.createAccount_invalid_credentials)
                        else -> uiState.copy(snackbarMessage = R.string.server_error)
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

    //TODO
    fun goBackLogin() {

    }
}