# BraveTweaker

A standalone, zero-dependency Windows GUI + CLI executable (`BraveTweaker.exe`) to de-bloat Brave Browser, silence annoying prompts, and harden privacy via Windows Registry Policies and Brave profile `Preferences` JSON.

## Features

- **Native Windows GUI** (double-click `BraveTweaker.exe`) with 26 individual checkboxes organized into 3 tabs, a **Containers** tab, live **Active** status badges, and a built-in log viewer.
- **Brave Containers manager:** Reads your current Brave containers from `brave.containers.list`, lets you add/remove new containers (Name, Color, Icon) or load common presets, and only creates containers that do not already exist (never duplicates or overwrites existing ones).
- **Presets:** `Recommended`, `Clean UI`, `No Prompts`, `Strict Privacy`, `Select All`, `Select None`.
- **Export `.reg` file:** Export any selection of policies to a standalone `.reg` file for other PCs.
- **Safe Revert:** Automatically backs up each Brave profile's `Preferences` file (`Preferences.brave-tweaker.bak`) before editing, and lets you remove all policies and restore backups with one click.
- **Headless CLI mode:** Supports `--apply`, `--revert`, `--status`, `--export-reg`, `--preset`, `--user`, `--machine`, and `--close-brave`.

## Included Tweaks (26 Total)

### 1. Brave Bloat & UI (11)
- Disable Brave Rewards (`BraveRewardsDisabled = 1`)
- Hide "Show sidebar" button (`brave.show_side_panel_button = false`, `brave.sidebar.sidebar_show_option = 3`)
- Hide "Brave Wallet" button & disable Wallet (`BraveWalletDisabled = 1`)
- Disable "Brave VPN" button & disable VPN (`BraveVPNDisabled = 1`)
- Disable all AI features — Brave Leo + Chromium GenAI (`BraveAIChatEnabled = 0`, `GenAiDefaultSettings = 2`, etc.)
- Disable Brave News (`BraveNewsDisabled = 1`)
- Disable Brave Talk (`BraveTalkDisabled = 1`)
- Disable Sponsored Images on New Tab Page (`brave.new_tab_page.show_branded_background_image = false`)
- Disable Tor Windows (`TorDisabled = 1`) *(unchecked in Recommended, checked in Select All)*
- Disable Wayback Machine 404 prompts (`brave.wayback_machine_enabled = false`)
- Always show full URLs in address bar (`omnibox.prevent_url_elisions = true`)

### 2. Prompts & Behavior (10)
- Disable password save offers (`PasswordManagerEnabled = 0`)
- Disable credit card save offers (`AutofillCreditCardEnabled = 0`)
- Disable address save offers (`AutofillAddressEnabled = 0`)
- Disable translation offers (`TranslateEnabled = 0`)
- Disable notification prompts (`DefaultNotificationsSetting = 2`)
- Disable location (geolocation) prompts (`DefaultGeolocationSetting = 2`)
- Disable "Set as default browser" prompt (`DefaultBrowserSettingEnabled = 0`)
- Disable background mode (`BackgroundModeEnabled = 0`)
- Disable shopping & price tracking (`ShoppingListEnabled = 0`)
- Disable "Close all tabs?" confirmation popup (`brave.enable_window_closing_confirm = false`)

### 3. Privacy & Telemetry (5)
- Disable Brave P3A & Stats Ping telemetry (`BraveP3AEnabled = 0`, `BraveStatsPingEnabled = 0`)
- Disable Chromium metrics, telemetry & feedback (`MetricsReportingEnabled = 0`, `UrlKeyedAnonymizedDataCollectionEnabled = 0`, `UserFeedbackAllowed = 0`, `FeedbackSurveysEnabled = 0`)
- Prevent WebRTC IP leaks (`WebRtcIPHandling = "disable_non_proxied_udp"`)
- Disable network prediction & link preloading (`NetworkPredictionOptions = 2`)
- Force HTTPS-Only mode (`HttpsOnlyMode = "force_enabled"`) *(unchecked in Recommended, checked in Strict Privacy / Select All)*

## Usage

### GUI Mode
Simply double-click `BraveTweaker.exe`.

### CLI Mode
```powershell
# Apply Recommended preset (Current User, no Admin needed)
.\BraveTweaker.exe --apply --close-brave

# Apply all 25 tweaks machine-wide (run from Administrator prompt)
.\BraveTweaker.exe --apply --preset all --machine --close-brave

# Export a .reg file of the Recommended preset
.\BraveTweaker.exe --export-reg .\BraveTweaks.reg --preset recommended

# Check active status of all tweaks
.\BraveTweaker.exe --status

# Revert all policies & restore Preferences backups
.\BraveTweaker.exe --revert --close-brave
```

## Rebuilding from Source
Run `.\build.ps1` (uses the built-in Windows `.NET Framework` compiler `csc.exe`, no external SDK required).
