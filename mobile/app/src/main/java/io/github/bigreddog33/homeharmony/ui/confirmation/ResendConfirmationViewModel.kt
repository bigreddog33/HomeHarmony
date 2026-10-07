package io.github.bigreddog33.homeharmony.ui.confirmation

import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.setValue
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import io.github.bigreddog33.homeharmony.R
import io.github.bigreddog33.homeharmony.data.account.AccountApi
import io.github.bigreddog33.homeharmony.data.account.dto.ResendConfirmationRequest
import io.github.bigreddog33.homeharmony.data.network.ApiClient
import io.github.bigreddog33.homeharmony.data.network.ApiResult
import io.github.bigreddog33.homeharmony.data.network.apiCall
import kotlinx.coroutines.launch

class ResendConfirmationViewModel(
    private val accountApi: AccountApi = ApiClient.create(AccountApi::class.java)
) : ViewModel() {

    var uiState by mutableStateOf(ResendConfirmationUiState())
        private set

    fun resend(email: String) {
        if (uiState.isLoading || uiState.isSuccessful) return

        val normalizedEmail = email.trim()

        if (normalizedEmail.isEmpty()) {
            uiState = uiState.copy(errorMessage = R.string.email_required)
            return
        }

        uiState = uiState.copy(
            isLoading = true,
            errorMessage = null
        )

        viewModelScope.launch {
            try {
                val result = apiCall {
                    accountApi.resendConfirmation(
                        ResendConfirmationRequest(normalizedEmail)
                    )
                }

                uiState = when (result) {
                    is ApiResult.Success -> uiState.copy(
                        isSuccessful = true
                    )

                    is ApiResult.HttpError -> uiState.copy(
                        errorMessage = when (result.code) {
                            429 -> R.string.too_many_requests
                            else -> R.string.confirmation_resend_failed
                        }
                    )

                    ApiResult.NetworkError -> uiState.copy(
                        errorMessage = R.string.network_error
                    )

                    ApiResult.UnexpectedError -> uiState.copy(
                        errorMessage = R.string.unexpected_error
                    )
                }
            } finally {
                uiState = uiState.copy(isLoading = false)
            }
        }
    }
}