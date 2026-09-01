package com.example.myapplication.data.remote.account

import com.example.myapplication.data.remote.auth.LoginRequest
import retrofit2.Response
import retrofit2.http.Body
import retrofit2.http.POST

interface AuthApi {
    @POST("api/account/login")
    suspend fun login(
        @Body request: LoginRequest
    ): Response<LoginResponse>
}