package io.github.bigreddog33.homeharmony.ui.previews

import android.content.res.Configuration
import androidx.compose.ui.tooling.preview.Preview

@Target(
    AnnotationTarget.FUNCTION,
    AnnotationTarget.ANNOTATION_CLASS
)
@Retention(AnnotationRetention.BINARY)

@Preview(
    name = "Compact phone - Light",
    device = "spec:width=320dp,height=640dp,dpi=420",
    showSystemUi = true,
    uiMode = Configuration.UI_MODE_NIGHT_NO
)

@Preview(
    name = "Compact phone - Dark",
    device = "spec:width=320dp,height=640dp,dpi=420",
    showSystemUi = true,
    uiMode = Configuration.UI_MODE_NIGHT_YES
)

@Preview(
    name = "Phone - Light",
    device = "spec:width=411dp,height=891dp,dpi=420",
    showSystemUi = true,
    uiMode = Configuration.UI_MODE_NIGHT_NO
)

@Preview(
    name = "Phone - Dark",
    device = "spec:width=411dp,height=891dp,dpi=420",
    showSystemUi = true,
    uiMode = Configuration.UI_MODE_NIGHT_YES
)

@Preview(
    name = "Tablet - Light",
    device = "spec:width=1280dp,height=800dp,dpi=240,orientation=portrait",
    showSystemUi = true,
    uiMode = Configuration.UI_MODE_NIGHT_NO
)

@Preview(
    name = "Tablet - Dark",
    device = "spec:width=1280dp,height=800dp,dpi=240,orientation=portrait",
    showSystemUi = true,
    uiMode = Configuration.UI_MODE_NIGHT_YES
)

annotation class AppDevicePreviews