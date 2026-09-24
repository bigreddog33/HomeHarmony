package io.github.bigreddog33.homeharmony.data.network

import kotlinx.coroutines.CancellationException
import retrofit2.HttpException
import java.io.IOException

// Retrofit services return bodies (or Unit); non-success HTTP responses throw HttpException.
internal suspend fun <T> apiCall(request: suspend () -> T): ApiResult<T> = try {
    ApiResult.Success(request())
} catch (exception: CancellationException) {
    throw exception
} catch (exception: HttpException) {
    ApiResult.HttpError(exception.code())
} catch (_: IOException) {
    ApiResult.NetworkError
} catch (_: Exception) {
    ApiResult.UnexpectedError
}
