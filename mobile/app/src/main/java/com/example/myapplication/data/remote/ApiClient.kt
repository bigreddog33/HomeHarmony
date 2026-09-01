package com.example.myapplication.data.remote

import com.example.myapplication.data.remote.account.AuthApi
import com.example.myapplication.BuildConfig
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory

object ApiClient {
    private val retrofit = Retrofit.Builder()
        .baseUrl(BuildConfig.API_BASE_URL)
        .addConverterFactory(GsonConverterFactory.create())
        .build()

    val authApi: AuthApi = retrofit.create(AuthApi::class.java)
}