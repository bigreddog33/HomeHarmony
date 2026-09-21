package io.github.bigreddog33.homeharmony.data.network

internal sealed interface ApiResult<out T> {
    data class Success<T>(val value: T) : ApiResult<T>
    data class HttpError(val code: Int) : ApiResult<Nothing>
    data object NetworkError : ApiResult<Nothing>
    data object UnexpectedError : ApiResult<Nothing>
}
