package io.github.bigreddog33.homeharmony.data.account

import io.github.bigreddog33.homeharmony.data.account.dto.CreateAccountRequest
import io.github.bigreddog33.homeharmony.data.account.dto.CreateAccountResponse
import io.github.bigreddog33.homeharmony.data.account.dto.LoginRequest
import io.github.bigreddog33.homeharmony.data.account.dto.ResendConfirmationRequest
import retrofit2.http.Body
import retrofit2.http.POST

interface AccountApi {
    @POST("api/account/login")
    suspend fun login(
        @Body request: LoginRequest
    )
    
    @POST("api/account/createuser")
    suspend fun createAccount(
        @Body request: CreateAccountRequest
    ): CreateAccountResponse
    
    @POST("api/account/resendconfirmation")
    suspend fun resendConfirmation(
        @Body request: ResendConfirmationRequest
    )
}
