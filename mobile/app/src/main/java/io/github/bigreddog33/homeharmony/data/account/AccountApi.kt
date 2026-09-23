package io.github.bigreddog33.homeharmony.data.account

import io.github.bigreddog33.homeharmony.data.account.dto
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
