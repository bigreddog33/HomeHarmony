package io.github.bigreddog33.homeharmony.data.account

import com.google.gson.JsonParseException
import com.google.gson.JsonParser

internal fun parseEmailConfirmed(body: String?): Boolean? {
    if (body.isNullOrBlank()) return null

    return try {
        val json = JsonParser.parseString(body)
        if (!json.isJsonObject) return null

        val value = json.asJsonObject.get("emailConfirmed")
            ?: return null

        if (value.isJsonPrimitive && value.asJsonPrimitive.isBoolean) {
            value.asBoolean
        } else {
            null
        }
    } catch (_: JsonParseException) {
        null
    }
}