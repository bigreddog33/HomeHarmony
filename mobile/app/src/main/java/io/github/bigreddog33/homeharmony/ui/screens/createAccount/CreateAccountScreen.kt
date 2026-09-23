package io.github.bigreddog33.homeharmony.ui.screens.createAccount

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier

@Composable
fun CreateAccountScreen(
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

        LaunchedEffect(snackbarMessage) {
            snackbarMessage?.let {
                snackbarHostState.showSnackbar(it)
                viewModel.onSnackbarShown()
            }
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
                            viewModel.login()
                        }
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