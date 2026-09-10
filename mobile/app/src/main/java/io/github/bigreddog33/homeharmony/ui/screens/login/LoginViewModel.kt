package io.github.bigreddog33.homeharmony.ui.screens.login

import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.setValue
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import io.github.bigreddog33.homeharmony.R
import io.github.bigreddog33.homeharmony.data.remote.ApiClient
import io.github.bigreddog33.homeharmony.data.remote.account.AccountApi
import io.github.bigreddog33.homeharmony.data.remote.account.LoginRequest
import kotlinx.coroutines.CancellationException
import kotlinx.coroutines.launch
import java.io.IOException

class LoginViewModel(
    private val accountApi: AccountApi = ApiClient.accountApi
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

        val request = LoginRequest(email = uiState.email.trim(), password = uiState.password)
        uiState = uiState.copy(isLoading = true)

        viewModelScope.launch {
            val response = try {
                accountApi.login(request)
            } catch (exception: CancellationException) {
                throw exception
            } catch (_: IOException) {
                uiState = uiState.copy(snackbarMessage = R.string.login_network_error)
                return@launch
            } catch (_: Exception) {
                uiState = uiState.copy(snackbarMessage = R.string.login_unexpected_error)
                return@launch
            } finally {
                uiState = uiState.copy(isLoading = false)
            }

            when {
                response.isSuccessful -> {
                    uiState = uiState.copy(isLoginSuccessful = true)
                }
                response.code() == 400 -> {
                    uiState = uiState.copy(loginError = R.string.login_invalid_request)
                }
                response.code() == 401 -> {
                    uiState = uiState.copy(loginError = R.string.login_invalid_credentials)
                }
                else -> {
                    uiState = uiState.copy(snackbarMessage = R.string.login_server_error)
                }
            }
        }
    }
}
