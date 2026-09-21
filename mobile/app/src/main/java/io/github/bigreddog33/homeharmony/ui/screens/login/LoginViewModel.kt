package io.github.bigreddog33.homeharmony.ui.screens.login

import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.setValue
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import io.github.bigreddog33.homeharmony.R
import io.github.bigreddog33.homeharmony.data.account.AccountApi
import io.github.bigreddog33.homeharmony.data.account.dto.LoginRequest
import io.github.bigreddog33.homeharmony.data.network.ApiClient
import io.github.bigreddog33.homeharmony.data.network.ApiResult
import io.github.bigreddog33.homeharmony.data.network.apiCall
import kotlinx.coroutines.launch

class LoginViewModel(
    private val accountApi: AccountApi = ApiClient.create(AccountApi::class.java)
) : ViewModel() {

    var uiState by mutableStateOf(LoginUiState())
        private set

    fun onEmailChange(email: String) {
        uiState = uiState.copy(email = email, emailError = null, loginError = null)
    }

    fun onPasswordChange(password: String) {
        uiState = uiState.copy(password = password, passwordError = null, loginError = null)
    }

    fun onSnackbarShown() {
        uiState = uiState.copy(snackbarMessage = null)
    }

    fun login() {
        if (uiState.isLoading || uiState.isLoginSuccessful) return

        val validation = validateLogin(uiState.email, uiState.password)
        uiState = uiState.copy(
            emailError = validation.email,
            passwordError = validation.password,
            loginError = null,
            snackbarMessage = null
        )
        if (!validation.isValid) return

        val email = uiState.email.trim()
        val password = uiState.password
        uiState = uiState.copy(isLoading = true)

        viewModelScope.launch {
            val result = try {
                apiCall { accountApi.login(LoginRequest(email, password)) }
            } finally {
                uiState = uiState.copy(isLoading = false)
            }

            when (result) {
                is ApiResult.Success -> {
                    uiState = uiState.copy(isLoginSuccessful = true)
                }
                is ApiResult.HttpError -> {
                    uiState = when (result.code) {
                        400 -> uiState.copy(loginError = R.string.login_invalid_request)
                        401 -> uiState.copy(loginError = R.string.login_invalid_credentials)
                        else -> uiState.copy(snackbarMessage = R.string.login_server_error)
                    }
                }
                ApiResult.NetworkError -> {
                    uiState = uiState.copy(snackbarMessage = R.string.login_network_error)
                }
                ApiResult.UnexpectedError -> {
                    uiState = uiState.copy(snackbarMessage = R.string.login_unexpected_error)
                }
            }
        }
    }
}
