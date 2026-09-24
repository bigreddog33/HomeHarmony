package io.github.bigreddog33.homeharmony.data.account

import io.github.bigreddog33.homeharmony.data.account.dto.CreateAccountRequest
import io.github.bigreddog33.homeharmony.data.account.dto.LoginRequest
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
    )
}
