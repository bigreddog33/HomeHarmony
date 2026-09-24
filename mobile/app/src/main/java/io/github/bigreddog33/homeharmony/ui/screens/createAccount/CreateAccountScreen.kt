package io.github.bigreddog33.homeharmony.ui.screens.createAccount

import androidx.compose.foundation.Image
import androidx.compose.foundation.gestures.detectTapGestures
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.text.KeyboardActions
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.runtime.remember
import androidx.compose.runtime.rememberUpdatedState
import androidx.compose.runtime.snapshotFlow
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.alpha
import androidx.compose.ui.draw.rotate
import androidx.compose.ui.focus.FocusDirection
import androidx.compose.ui.graphics.luminance
import androidx.compose.ui.input.pointer.pointerInput
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.platform.LocalFocusManager
import androidx.compose.ui.res.painterResource
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.semantics.LiveRegionMode
import androidx.compose.ui.semantics.liveRegion
import androidx.compose.ui.semantics.semantics
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.ImeAction
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.text.input.PasswordVisualTransformation
import androidx.compose.ui.unit.dp
import androidx.lifecycle.Lifecycle
import androidx.lifecycle.compose.LocalLifecycleOwner
import androidx.lifecycle.repeatOnLifecycle
import androidx.lifecycle.viewmodel.compose.viewModel
import io.github.bigreddog33.homeharmony.R
import kotlinx.coroutines.flow.collect
import kotlinx.coroutines.flow.filter

@Composable
fun CreateAccountScreen(
    onCreateAccountSuccess: () -> Unit,
    onBackToLoginClick: () -> Unit,
    viewModel: CreateAccountViewModel = viewModel()
) {
    val state = viewModel.uiState
    val focusManager = LocalFocusManager.current
    val snackbarHostState = remember { SnackbarHostState() }
    val snackbarMessage = state.snackbarMessage?.let { stringResource(it) }
    val lifecycle = LocalLifecycleOwner.current.lifecycle
    val currentOnCreateAccountSuccess by rememberUpdatedState(onCreateAccountSuccess)

    LaunchedEffect(viewModel, lifecycle) {
        lifecycle.repeatOnLifecycle(Lifecycle.State.RESUMED) {
            snapshotFlow { viewModel.uiState.isCreateAccountSuccessful }
                .filter { it }
                .collect { currentOnCreateAccountSuccess() }
        }
    }

    LaunchedEffect(snackbarMessage) {
        snackbarMessage?.let {
            snackbarHostState.showSnackbar(it)
            viewModel.onSnackbarShown()
        }
    }

    Scaffold(snackbarHost = { SnackbarHost(hostState = snackbarHostState) }) { paddingValues ->
        Box(
            modifier = Modifier
                .fillMaxSize()
                .padding(paddingValues)
                .consumeWindowInsets(paddingValues)
                .imePadding()
                .pointerInput(Unit) {
                    detectTapGestures { focusManager.clearFocus() }
                }
        ) {
            CreateAccountBackground()

            Column(
                modifier = Modifier
                    .fillMaxSize()
                    .verticalScroll(rememberScrollState())
                    .padding(horizontal = 24.dp, vertical = 32.dp),
                horizontalAlignment = Alignment.CenterHorizontally,
                verticalArrangement = Arrangement.Center
            ) {
                Column(
                    modifier = Modifier
                        .widthIn(max = 420.dp)
                        .fillMaxWidth(),
                    verticalArrangement = Arrangement.spacedBy(24.dp)
                ) {
                    CreateAccountHeader()
                    CreateAccountForm(
                        state = state,
                        onEmailChange = viewModel::onEmailChange,
                        onEmailConfirmChange = viewModel::onEmailConfirmChange,
                        onPasswordChange = viewModel::onPasswordChange,
                        onPasswordConfirmChange = viewModel::onPasswordConfirmChange,
                        onCreateAccountClick = {
                            focusManager.clearFocus()
                            viewModel.createAccount()
                        },
                        onBackToLoginClick = onBackToLoginClick
                    )
                    CreateAccountFooter()
                }
            }
        }
    }
}

@Composable
private fun CreateAccountBackground() {
    val isDark = MaterialTheme.colorScheme.background.luminance() < 0.5f
    val decorationAlpha = if (isDark) 0.2f else 0.35f

    Box(modifier = Modifier.fillMaxSize()) {
        Image(
            painter = painterResource(R.drawable.flower_logo),
            contentDescription = null,
            contentScale = ContentScale.Fit,
            modifier = Modifier
                .align(Alignment.TopEnd)
                .widthIn(max = 520.dp)
                .fillMaxWidth()
                .aspectRatio(1f)
                .offset(x = 100.dp, y = (-60).dp)
                .rotate(130f)
                .alpha(decorationAlpha)
        )

        Image(
            painter = painterResource(R.drawable.leaves_bottom),
            contentDescription = null,
            contentScale = ContentScale.Fit,
            modifier = Modifier
                .align(Alignment.BottomStart)
                .widthIn(max = 520.dp)
                .fillMaxWidth()
                .aspectRatio(1f)
                .offset(x = (-120).dp, y = 100.dp)
                .rotate(-30f)
                .alpha(decorationAlpha)
        )
    }
}

@Composable
private fun CreateAccountHeader() {
    Column(verticalArrangement = Arrangement.spacedBy(8.dp)) {
        Text(
            text = stringResource(R.string.createAccount_welcome),
            style = MaterialTheme.typography.headlineLarge,
            fontWeight = FontWeight.SemiBold,
            color = MaterialTheme.colorScheme.primary
        )
        Text(
            text = stringResource(R.string.createAccount_subtitle),
            style = MaterialTheme.typography.bodyLarge,
            color = MaterialTheme.colorScheme.onSurfaceVariant
        )
    }
}

@Composable
private fun CreateAccountForm(
    state: CreateAccountUiState,
    onEmailChange: (String) -> Unit,
    onEmailConfirmChange: (String) -> Unit,
    onPasswordChange: (String) -> Unit,
    onPasswordConfirmChange: (String) -> Unit,
    onCreateAccountClick: () -> Unit,
    onBackToLoginClick: () -> Unit
) {
    val focusManager = LocalFocusManager.current

    Column(verticalArrangement = Arrangement.spacedBy(12.dp)) {
        OutlinedTextField(
            value = state.email,
            onValueChange = onEmailChange,
            label = { Text(stringResource(R.string.email_label)) },
            modifier = Modifier.fillMaxWidth(),
            enabled = !state.isLoading,
            isError = state.emailError != null,
            supportingText = state.emailError?.let { error ->
                { Text(stringResource(error)) }
            },
            singleLine = true,
            keyboardOptions = KeyboardOptions(
                autoCorrectEnabled = false,
                keyboardType = KeyboardType.Email,
                imeAction = ImeAction.Next
            ),
            keyboardActions = KeyboardActions(
                onNext = { focusManager.moveFocus(FocusDirection.Down) }
            ),
            shape = MaterialTheme.shapes.medium
        )

        OutlinedTextField(
            value = state.emailConfirm,
            onValueChange = onEmailConfirmChange,
            label = { Text(stringResource(R.string.email_confirm_label)) },
            modifier = Modifier.fillMaxWidth(),
            enabled = !state.isLoading,
            isError = state.emailConfirmError != null,
            supportingText = state.emailConfirmError?.let { error ->
                { Text(stringResource(error)) }
            },
            singleLine = true,
            keyboardOptions = KeyboardOptions(
                autoCorrectEnabled = false,
                keyboardType = KeyboardType.Email,
                imeAction = ImeAction.Next
            ),
            keyboardActions = KeyboardActions(
                onNext = { focusManager.moveFocus(FocusDirection.Down) }
            ),
            shape = MaterialTheme.shapes.medium
        )

        Spacer(modifier = Modifier.height(4.dp))

        OutlinedTextField(
            value = state.password,
            onValueChange = onPasswordChange,
            label = { Text(stringResource(R.string.password_label)) },
            modifier = Modifier.fillMaxWidth(),
            enabled = !state.isLoading,
            isError = state.passwordError != null,
            supportingText = {
                Text(stringResource(state.passwordError ?: R.string.createAccount_password_rules))
            },
            visualTransformation = PasswordVisualTransformation(),
            singleLine = true,
            keyboardOptions = KeyboardOptions(
                autoCorrectEnabled = false,
                keyboardType = KeyboardType.Password,
                imeAction = ImeAction.Next
            ),
            keyboardActions = KeyboardActions(
                onNext = { focusManager.moveFocus(FocusDirection.Down) }
            ),
            shape = MaterialTheme.shapes.medium
        )

        OutlinedTextField(
            value = state.passwordConfirm,
            onValueChange = onPasswordConfirmChange,
            label = { Text(stringResource(R.string.password_confirm_label)) },
            modifier = Modifier.fillMaxWidth(),
            enabled = !state.isLoading,
            isError = state.passwordConfirmError != null,
            supportingText = state.passwordConfirmError?.let { error ->
                { Text(stringResource(error)) }
            },
            visualTransformation = PasswordVisualTransformation(),
            singleLine = true,
            keyboardOptions = KeyboardOptions(
                autoCorrectEnabled = false,
                keyboardType = KeyboardType.Password,
                imeAction = ImeAction.Done
            ),
            keyboardActions = KeyboardActions(onDone = { onCreateAccountClick() }),
            shape = MaterialTheme.shapes.medium
        )

        state.createAccountError?.let { error ->
            Text(
                text = stringResource(error),
                color = MaterialTheme.colorScheme.error,
                style = MaterialTheme.typography.bodyMedium,
                modifier = Modifier.semantics { liveRegion = LiveRegionMode.Polite }
            )
        }

        Spacer(modifier = Modifier.height(6.dp))

        Button(
            onClick = onCreateAccountClick,
            enabled = !state.isLoading,
            modifier = Modifier.fillMaxWidth().heightIn(min = 52.dp),
            shape = MaterialTheme.shapes.medium
        ) {
            if (state.isLoading) {
                CircularProgressIndicator(
                    modifier = Modifier.size(20.dp),
                    color = LocalContentColor.current,
                    strokeWidth = 2.dp
                )
                Spacer(modifier = Modifier.width(8.dp))
            }
            Text(
                text = stringResource(
                    if (state.isLoading) R.string.createAccount_loading else R.string.createAccount_button
                ),
                style = MaterialTheme.typography.titleSmall
            )
        }

        OutlinedButton(
            onClick = onBackToLoginClick,
            enabled = !state.isLoading,
            modifier = Modifier.fillMaxWidth().heightIn(min = 52.dp),
            shape = MaterialTheme.shapes.medium
        ) {
            Text(
                text = stringResource(R.string.createAccount_login),
                style = MaterialTheme.typography.titleSmall
            )
        }
    }
}

@Composable
private fun CreateAccountFooter() {
    Column {
        TextButton(onClick = {}) {
            Text(
                text = stringResource(R.string.policies_terms),
                color = MaterialTheme.colorScheme.onSurfaceVariant
            )
        }
    }
}
