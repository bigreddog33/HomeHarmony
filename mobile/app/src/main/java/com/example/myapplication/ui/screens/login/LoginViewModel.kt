package com.example.myapplication.ui.screens.login

import android.util.Log
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.setValue
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.example.myapplication.data.remote.ApiClient
import com.example.myapplication.data.remote.auth.LoginRequest
import kotlinx.coroutines.launch
import java.io.IOException

class LoginViewModel : ViewModel() {

    var uiState by mutableStateOf(LoginUiState())
        private set

    fun onUsernameChange(Email: String) {
        uiState = uiState.copy(Email = Email)
    }

    fun onPasswordChange(Password: String) {
        uiState = uiState.copy(Password = Password)
    }

    fun login() {
        if (uiState.Email.isBlank() || uiState.Password.isBlank()) {
            uiState = uiState.copy(
                errorMessage = "Username and password are required."
            )
            return
        }

        viewModelScope.launch {
            uiState = uiState.copy(
                isLoading = true,
                errorMessage = null
            )

            try {
                val response = ApiClient.authApi.login(
                    LoginRequest(
                        Email = uiState.Email,
                        Password = uiState.Password
                    )
                )

                when {
                    response.isSuccessful -> {
                        uiState = uiState.copy(
                            isLoading = false,
                            loginSucceeded = true
                        )
                    }

                    response.code() == 401 -> {
                        uiState = uiState.copy(
                            isLoading = false,
                            errorMessage = "Invalid username or password."
                        )
                    }

                    else -> {
                        uiState = uiState.copy(
                            isLoading = false,
                            errorMessage = "Something went wrong."
                        )
                    }
                }
            } catch (e: IOException) {
                Log.e("LOGIN_API", "Login request failed", e)

                uiState = uiState.copy(
                    isLoading = false,
                    errorMessage =
                        "${e.javaClass.simpleName}: ${e.message}\n" +
                                "Cause: ${e.cause?.javaClass?.simpleName}: ${e.cause?.message}"
                )

            } catch (e: Exception) {
                Log.e("LOGIN_API", "Login request failed", e)

                uiState = uiState.copy(
                    isLoading = false,
                    errorMessage =
                        "${e.javaClass.simpleName}: ${e.message}\n" +
                                "Cause: ${e.cause?.javaClass?.simpleName}: ${e.cause?.message}"
                )

            }
        }
    }
}