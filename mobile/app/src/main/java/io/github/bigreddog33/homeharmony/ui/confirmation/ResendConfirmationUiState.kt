package io.github.bigreddog33.homeharmony.ui.confirmation

import androidx.annotation.StringRes

data class ResendConfirmationUiState(
    val isLoading: Boolean = false,
    val isSuccessful: Boolean = false,
    @param:StringRes val errorMessage: Int? = null
)
