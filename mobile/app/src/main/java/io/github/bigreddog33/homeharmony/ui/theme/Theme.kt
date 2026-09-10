package io.github.bigreddog33.homeharmony.ui.theme

import android.os.Build
import androidx.compose.foundation.isSystemInDarkTheme
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.darkColorScheme
import androidx.compose.material3.dynamicDarkColorScheme
import androidx.compose.material3.dynamicLightColorScheme
import androidx.compose.material3.lightColorScheme
import androidx.compose.runtime.Composable
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.platform.LocalContext

private val DarkColorScheme = darkColorScheme(
    primary = Indigo300,        // Vibrant Light Indigo for dark mode
    secondary = Lilac300,      // Soft Lilac for dark mode
    tertiary = Indigo400,
    background = DeepSea900,    // Deep Sea Blue (Professional Dark)
    surface = DeepSea800,       // Slightly lighter Deep Sea
    onPrimary = DeepSea950,     // Deep Contrast on buttons
    onSecondary = DeepSea950,
    onBackground = Slate50,     // Soft white for text
    onSurface = Slate100,
    primaryContainer = Indigo900,
    onPrimaryContainer = Indigo300,
    outline = Slate300
)

private val LightColorScheme = lightColorScheme(
    primary = Indigo700,        // Deep Indigo
    secondary = Lilac700,      // Professional Purple
    tertiary = Indigo600,
    background = Slate50,       // Very light blue-grey background
    surface = Color.White,
    onPrimary = Color.White,
    onSecondary = Color.White,
    onBackground = DeepSea900,
    onSurface = DeepSea900,
    primaryContainer = Indigo300,
    onPrimaryContainer = Indigo900,
    outline = Slate300
)

@Composable
fun HomeHarmonyTheme(
    darkTheme: Boolean = isSystemInDarkTheme(),
    // Keep the HomeHarmony palette unless system colors are explicitly requested.
    dynamicColor: Boolean = false,
    content: @Composable () -> Unit
) {
    val colorScheme = when {
        dynamicColor && Build.VERSION.SDK_INT >= Build.VERSION_CODES.S -> {
            val context = LocalContext.current
            if (darkTheme) dynamicDarkColorScheme(context) else dynamicLightColorScheme(context)
        }

        darkTheme -> DarkColorScheme
        else -> LightColorScheme
    }

    MaterialTheme(
        colorScheme = colorScheme,
        typography = Typography,
        content = content
    )
}
