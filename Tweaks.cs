using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using Microsoft.Win32;

namespace BraveTweaker
{
    internal enum TweakCategory
    {
        BraveUI,
        PromptsBehavior,
        PrivacyTelemetry
    }

    internal sealed class PolicyEntry
    {
        public string SubKeyPath { get; set; }
        public string Name { get; set; }
        public object Value { get; set; }
        public RegistryValueKind Kind { get; set; }

        public string EffectiveSubKeyPath
        {
            get { return string.IsNullOrEmpty(SubKeyPath) ? TweakEngine.PolicySubKeyPath : SubKeyPath; }
        }

        public static PolicyEntry DWord(string name, int value, string subKeyPath = null)
        {
            return new PolicyEntry { SubKeyPath = subKeyPath, Name = name, Value = value, Kind = RegistryValueKind.DWord };
        }

        public static PolicyEntry Str(string name, string value, string subKeyPath = null)
        {
            return new PolicyEntry { SubKeyPath = subKeyPath, Name = name, Value = value, Kind = RegistryValueKind.String };
        }
    }

    internal sealed class PrefEntry
    {
        public string[] Path { get; set; }
        public object Value { get; set; }
        public bool IsArrayAppend { get; set; }

        public PrefEntry(object value, params string[] path)
        {
            Value = value;
            Path = path;
            IsArrayAppend = false;
        }

        public static PrefEntry ArrayAppend(string itemValue, params string[] path)
        {
            return new PrefEntry(itemValue, path) { IsArrayAppend = true };
        }
    }

    internal sealed class TweakItem
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public TweakCategory Category { get; set; }
        public bool DefaultChecked { get; set; }
        public List<PolicyEntry> Policies { get; set; }
        public List<PrefEntry> Prefs { get; set; }
        public List<PrefEntry> LocalStatePrefs { get; set; }

        public TweakItem()
        {
            Policies = new List<PolicyEntry>();
            Prefs = new List<PrefEntry>();
            LocalStatePrefs = new List<PrefEntry>();
        }
    }

    internal sealed class ContainerItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Icon { get; set; }
        public int BackgroundColor { get; set; }
        public bool IsExisting { get; set; }
    }

    internal static class TweakCatalog
    {
        public static List<TweakItem> GetAll()
        {
            return new List<TweakItem>
            {
                // ========================
                // 1. Brave Bloat & UI
                // ========================
                new TweakItem
                {
                    Id = "brave_rewards",
                    Title = "Disable Brave Rewards",
                    Description = "Turns off Brave Rewards and hides its toolbar icon and menus.",
                    Category = TweakCategory.BraveUI,
                    DefaultChecked = true,
                    Policies = { PolicyEntry.DWord("BraveRewardsDisabled", 1) }
                },
                new TweakItem
                {
                    Id = "hide_sidebar_btn",
                    Title = "Hide \"Show sidebar\" button",
                    Description = "Hides the sidebar toggle button on the toolbar and sets sidebar display to Never.",
                    Category = TweakCategory.BraveUI,
                    DefaultChecked = true,
                    Prefs =
                    {
                        new PrefEntry(false, "brave", "show_side_panel_button"),
                        new PrefEntry(3, "brave", "sidebar", "sidebar_show_option")
                    }
                },
                new TweakItem
                {
                    Id = "brave_wallet",
                    Title = "Hide \"Brave Wallet\" button & disable Wallet",
                    Description = "Disables the built-in crypto wallet and removes its toolbar icon.",
                    Category = TweakCategory.BraveUI,
                    DefaultChecked = true,
                    Policies = { PolicyEntry.DWord("BraveWalletDisabled", 1) },
                    Prefs = { new PrefEntry(false, "brave", "wallet", "show_wallet_icon_on_toolbar") }
                },
                new TweakItem
                {
                    Id = "brave_vpn",
                    Title = "Disable \"Brave VPN\" button & disable VPN",
                    Description = "Disables Brave Firewall + VPN and hides the VPN toolbar button.",
                    Category = TweakCategory.BraveUI,
                    DefaultChecked = true,
                    Policies = { PolicyEntry.DWord("BraveVPNDisabled", 1) },
                    Prefs = { new PrefEntry(false, "brave", "brave_vpn", "show_button") }
                },
                new TweakItem
                {
                    Id = "disable_ai",
                    Title = "Disable all AI features (Brave Leo & Chromium GenAI)",
                    Description = "Disables Brave Leo AI chat, toolbar button, autocomplete, and Chromium generative AI tools.",
                    Category = TweakCategory.BraveUI,
                    DefaultChecked = true,
                    Policies =
                    {
                        PolicyEntry.DWord("BraveAIChatEnabled", 0),
                        PolicyEntry.DWord("GenAiDefaultSettings", 2),
                        PolicyEntry.DWord("DevToolsGenAiSettings", 2),
                        PolicyEntry.DWord("HistorySearchSettings", 2),
                        PolicyEntry.DWord("TabCompareSettings", 2),
                        PolicyEntry.DWord("HelpMeWriteSettings", 2),
                        PolicyEntry.DWord("CreateThemesSettings", 2),
                        PolicyEntry.DWord("TabOrganizerSettings", 2),
                        PolicyEntry.DWord("AutofillPredictionSettings", 2)
                    },
                    Prefs =
                    {
                        new PrefEntry(false, "brave", "ai_chat", "show_toolbar_button"),
                        new PrefEntry(false, "brave", "ai_chat", "autocomplete_provider_enabled")
                    }
                },
                new TweakItem
                {
                    Id = "brave_news",
                    Title = "Disable Brave News",
                    Description = "Removes the Brave News feed from the New Tab Page.",
                    Category = TweakCategory.BraveUI,
                    DefaultChecked = true,
                    Policies = { PolicyEntry.DWord("BraveNewsDisabled", 1) }
                },
                new TweakItem
                {
                    Id = "brave_talk",
                    Title = "Disable Brave Talk",
                    Description = "Removes Brave Talk video call integration and prompts.",
                    Category = TweakCategory.BraveUI,
                    DefaultChecked = true,
                    Policies = { PolicyEntry.DWord("BraveTalkDisabled", 1) }
                },
                new TweakItem
                {
                    Id = "ntp_sponsored",
                    Title = "Disable Sponsored Images on New Tab Page",
                    Description = "Hides sponsored ad background images on new tabs.",
                    Category = TweakCategory.BraveUI,
                    DefaultChecked = true,
                    Prefs = { new PrefEntry(false, "brave", "new_tab_page", "show_branded_background_image") }
                },
                new TweakItem
                {
                    Id = "disable_tor",
                    Title = "Disable Tor Windows",
                    Description = "Removes the \"New Private Window with Tor\" feature.",
                    Category = TweakCategory.BraveUI,
                    DefaultChecked = false,
                    Policies = { PolicyEntry.DWord("TorDisabled", 1) }
                },
                new TweakItem
                {
                    Id = "wayback_machine",
                    Title = "Disable Wayback Machine 404 prompts",
                    Description = "Stops Brave from showing the Wayback Machine banner on 404 pages.",
                    Category = TweakCategory.BraveUI,
                    DefaultChecked = true,
                    Prefs = { new PrefEntry(false, "brave", "wayback_machine_enabled") }
                },
                new TweakItem
                {
                    Id = "full_urls",
                    Title = "Always show full URLs in address bar",
                    Description = "Displays https:// and www. instead of hiding them in the omnibox.",
                    Category = TweakCategory.BraveUI,
                    DefaultChecked = true,
                    Prefs = { new PrefEntry(true, "omnibox", "prevent_url_elisions") }
                },
                new TweakItem
                {
                    Id = "show_downloads_button",
                    Title = "Pin Downloads button to toolbar",
                    Description = "Brings back and pins the Downloads button on the browser toolbar.",
                    Category = TweakCategory.BraveUI,
                    DefaultChecked = true,
                    Prefs = { PrefEntry.ArrayAppend("kActionShowDownloads", "toolbar", "pinned_actions") }
                },
                new TweakItem
                {
                    Id = "hide_tab_groups_bookmarks_bar",
                    Title = "Hide tab groups in bookmarks bar",
                    Description = "Turns off \"Show tab groups in bookmarks bar\" (bookmark_bar.show_tab_groups = false).",
                    Category = TweakCategory.BraveUI,
                    DefaultChecked = true,
                    Prefs = { new PrefEntry(false, "bookmark_bar", "show_tab_groups") }
                },
                new TweakItem
                {
                    Id = "ultra_dark_theme",
                    Title = "Enable Ultra dark theme",
                    Description = "Switches Brave to Dark mode and enables the Ultra dark / Darker theme.",
                    Category = TweakCategory.BraveUI,
                    DefaultChecked = true,
                    Prefs =
                    {
                        new PrefEntry(2, "browser", "color_scheme2"),
                        new PrefEntry(true, "brave", "darker_theme_enabled")
                    },
                    LocalStatePrefs =
                    {
                        new PrefEntry(1, "brave", "dark_mode"),
                        PrefEntry.ArrayAppend("brave-ultra-dark-theme@1", "browser", "enabled_labs_experiments")
                    }
                },
                new TweakItem
                {
                    Id = "mv2_ublock",
                    Title = "Enable Manifest V2 & uBlock Origin",
                    Description = "Enables MV2 extensions policy and adds uBlock Origin to ExtensionInstallForcelist (also check brave://settings/extensions/v2).",
                    Category = TweakCategory.BraveUI,
                    DefaultChecked = true,
                    Policies =
                    {
                        PolicyEntry.DWord("ExtensionManifestV2Availability", 2),
                        PolicyEntry.Str(
                            "1",
                            "cjpalhdlnbpafiamejdnhcphjbkeiagm;https://clients2.google.com/service/update2/crx",
                            TweakEngine.PolicySubKeyPath + @"\ExtensionInstallForcelist")
                    }
                },

                // ========================
                // 2. Prompts & Behavior
                // ========================
                new TweakItem
                {
                    Id = "password_offers",
                    Title = "Disable password save offers",
                    Description = "Disables the built-in password manager save prompts.",
                    Category = TweakCategory.PromptsBehavior,
                    DefaultChecked = true,
                    Policies = { PolicyEntry.DWord("PasswordManagerEnabled", 0) },
                    Prefs =
                    {
                        new PrefEntry(false, "credentials_enable_service"),
                        new PrefEntry(false, "profile", "password_manager_enabled")
                    }
                },
                new TweakItem
                {
                    Id = "credit_card_offers",
                    Title = "Disable credit card save offers",
                    Description = "Stops Brave from offering to save or autofill payment cards.",
                    Category = TweakCategory.PromptsBehavior,
                    DefaultChecked = true,
                    Policies = { PolicyEntry.DWord("AutofillCreditCardEnabled", 0) },
                    Prefs = { new PrefEntry(false, "autofill", "credit_card_enabled") }
                },
                new TweakItem
                {
                    Id = "address_offers",
                    Title = "Disable address save offers",
                    Description = "Stops Brave from offering to save street addresses and contact info.",
                    Category = TweakCategory.PromptsBehavior,
                    DefaultChecked = true,
                    Policies = { PolicyEntry.DWord("AutofillAddressEnabled", 0) },
                    Prefs = { new PrefEntry(false, "autofill", "profile_enabled") }
                },
                new TweakItem
                {
                    Id = "translate_offers",
                    Title = "Disable translation offers",
                    Description = "Stops the translate page popup from appearing on foreign-language sites.",
                    Category = TweakCategory.PromptsBehavior,
                    DefaultChecked = true,
                    Policies = { PolicyEntry.DWord("TranslateEnabled", 0) },
                    Prefs = { new PrefEntry(false, "translate", "enabled") }
                },
                new TweakItem
                {
                    Id = "notification_offers",
                    Title = "Disable notification prompts",
                    Description = "Blocks all websites from asking to send desktop notifications.",
                    Category = TweakCategory.PromptsBehavior,
                    DefaultChecked = true,
                    Policies = { PolicyEntry.DWord("DefaultNotificationsSetting", 2) },
                    Prefs = { new PrefEntry(2, "profile", "default_content_setting_values", "notifications") }
                },
                new TweakItem
                {
                    Id = "geolocation_offers",
                    Title = "Disable location (geolocation) prompts",
                    Description = "Blocks websites from asking to access your physical location.",
                    Category = TweakCategory.PromptsBehavior,
                    DefaultChecked = true,
                    Policies = { PolicyEntry.DWord("DefaultGeolocationSetting", 2) },
                    Prefs = { new PrefEntry(2, "profile", "default_content_setting_values", "geolocation") }
                },
                new TweakItem
                {
                    Id = "default_browser_prompt",
                    Title = "Disable \"Set as default browser\" prompt",
                    Description = "Stops Brave from asking to become your default browser.",
                    Category = TweakCategory.PromptsBehavior,
                    DefaultChecked = true,
                    Policies = { PolicyEntry.DWord("DefaultBrowserSettingEnabled", 0) }
                },
                new TweakItem
                {
                    Id = "background_mode",
                    Title = "Disable background mode",
                    Description = "Prevents Brave from running in the system tray after you close all windows.",
                    Category = TweakCategory.PromptsBehavior,
                    DefaultChecked = true,
                    Policies = { PolicyEntry.DWord("BackgroundModeEnabled", 0) }
                },
                new TweakItem
                {
                    Id = "shopping_list",
                    Title = "Disable shopping & price tracking",
                    Description = "Turns off Chromium shopping list and price tracking UI.",
                    Category = TweakCategory.PromptsBehavior,
                    DefaultChecked = true,
                    Policies = { PolicyEntry.DWord("ShoppingListEnabled", 0) }
                },
                new TweakItem
                {
                    Id = "close_all_confirm",
                    Title = "Disable \"Close all tabs?\" confirmation popup",
                    Description = "Turns off the warning prompt when closing a window with multiple tabs.",
                    Category = TweakCategory.PromptsBehavior,
                    DefaultChecked = true,
                    Prefs = { new PrefEntry(false, "brave", "enable_window_closing_confirm") }
                },
                new TweakItem
                {
                    Id = "start_new_tab",
                    Title = "Start with New Tab page",
                    Description = "Always opens a fresh New Tab page when Brave launches.",
                    Category = TweakCategory.PromptsBehavior,
                    DefaultChecked = true,
                    Policies = { PolicyEntry.DWord("RestoreOnStartup", 5) },
                    Prefs = { new PrefEntry(5, "session", "restore_on_startup") }
                },
                new TweakItem
                {
                    Id = "fullscreen_reminder",
                    Title = "Disable \"Show full screen reminder\"",
                    Description = "Turns off the 'Press Esc to exit full screen' reminder banner.",
                    Category = TweakCategory.PromptsBehavior,
                    DefaultChecked = true,
                    Prefs = { new PrefEntry(false, "brave", "show_fullscreen_reminder") }
                },
                new TweakItem
                {
                    Id = "energy_saver_off",
                    Title = "Turn off Energy Saver",
                    Description = "Disables Chromium/Brave Energy Saver throttling.",
                    Category = TweakCategory.PromptsBehavior,
                    DefaultChecked = true,
                    Policies = { PolicyEntry.DWord("BatterySaverModeAvailability", 0) },
                    Prefs = { new PrefEntry(0, "performance_tuning", "battery_saver_mode", "state") }
                },
                new TweakItem
                {
                    Id = "search_google",
                    Title = "Set Search Engine to Google (Normal & Private)",
                    Description = "Sets Google as the default search engine for both normal and private windows.",
                    Category = TweakCategory.PromptsBehavior,
                    DefaultChecked = true,
                    Policies =
                    {
                        PolicyEntry.DWord("DefaultSearchProviderEnabled", 1),
                        PolicyEntry.Str("DefaultSearchProviderName", "Google"),
                        PolicyEntry.Str("DefaultSearchProviderKeyword", "google.com"),
                        PolicyEntry.Str("DefaultSearchProviderSearchURL", "https://www.google.com/search?q={searchTerms}"),
                        PolicyEntry.Str("DefaultSearchProviderSuggestURL", "https://www.google.com/complete/search?output=chrome&q={searchTerms}")
                    },
                    Prefs = { new PrefEntry(false, "brave", "use_alternate_private_search_engine") }
                },
                new TweakItem
                {
                    Id = "disable_auto_update",
                    Title = "Disable Brave Auto-Update",
                    Description = "Sets BraveSoftware\\Update policies to stop automatic background updates.",
                    Category = TweakCategory.PromptsBehavior,
                    DefaultChecked = true,
                    Policies =
                    {
                        PolicyEntry.DWord("AutoUpdateCheckPeriodMinutes", 0, TweakEngine.UpdatePolicySubKeyPath),
                        PolicyEntry.DWord("UpdateDefault", 0, TweakEngine.UpdatePolicySubKeyPath)
                    }
                },

                // ========================
                // 3. Privacy & Telemetry
                // ========================
                new TweakItem
                {
                    Id = "brave_telemetry",
                    Title = "Disable Brave P3A & Stats Ping telemetry",
                    Description = "Turns off Brave's anonymous product analytics and daily/monthly usage pings.",
                    Category = TweakCategory.PrivacyTelemetry,
                    DefaultChecked = true,
                    Policies =
                    {
                        PolicyEntry.DWord("BraveP3AEnabled", 0),
                        PolicyEntry.DWord("BraveStatsPingEnabled", 0)
                    },
                    Prefs = { new PrefEntry(false, "brave", "p3a", "enabled") }
                },
                new TweakItem
                {
                    Id = "chromium_telemetry",
                    Title = "Disable Chromium metrics, telemetry & feedback",
                    Description = "Disables usage metrics, URL-keyed anonymized data, feedback dialogs, and surveys.",
                    Category = TweakCategory.PrivacyTelemetry,
                    DefaultChecked = true,
                    Policies =
                    {
                        PolicyEntry.DWord("MetricsReportingEnabled", 0),
                        PolicyEntry.DWord("UrlKeyedAnonymizedDataCollectionEnabled", 0),
                        PolicyEntry.DWord("UserFeedbackAllowed", 0),
                        PolicyEntry.DWord("FeedbackSurveysEnabled", 0)
                    }
                },
                new TweakItem
                {
                    Id = "webrtc_leak",
                    Title = "Prevent WebRTC IP leaks",
                    Description = "Sets WebRtcIPHandling to disable_non_proxied_udp to protect your IP behind VPNs.",
                    Category = TweakCategory.PrivacyTelemetry,
                    DefaultChecked = true,
                    Policies = { PolicyEntry.Str("WebRtcIPHandling", "disable_non_proxied_udp") }
                },
                new TweakItem
                {
                    Id = "network_prediction",
                    Title = "Disable network prediction & link preloading",
                    Description = "Stops the browser from pre-resolving DNS and preloading pages you haven't clicked.",
                    Category = TweakCategory.PrivacyTelemetry,
                    DefaultChecked = true,
                    Policies = { PolicyEntry.DWord("NetworkPredictionOptions", 2) }
                },
                new TweakItem
                {
                    Id = "shields_aggressive",
                    Title = "Shields: Trackers & Ads blocking -> Aggressive",
                    Description = "Sets Brave Shields trackers & ads blocking to Aggressive globally.",
                    Category = TweakCategory.PrivacyTelemetry,
                    DefaultChecked = true,
                    Prefs =
                    {
                        new PrefEntry(2, "profile", "content_settings", "exceptions", "cosmeticFiltering", "*,*", "setting"),
                        new PrefEntry(2, "brave", "shields", "ads_ctl"),
                        new PrefEntry(2, "brave", "shields", "trackers_ctl")
                    }
                },
                new TweakItem
                {
                    Id = "diagnostic_reports",
                    Title = "Turn off automatically send diagnostic reports",
                    Description = "Disables automatic crash and diagnostic report uploads in Brave.",
                    Category = TweakCategory.PrivacyTelemetry,
                    DefaultChecked = true,
                    Policies = { PolicyEntry.DWord("MetricsReportingEnabled", 0) },
                    LocalStatePrefs = { new PrefEntry(false, "user_experience_metrics", "reporting_enabled") }
                },
                new TweakItem
                {
                    Id = "https_only",
                    Title = "Shields: Upgrade connections to HTTPS -> Strict",
                    Description = "Enforces HTTPS-Only mode (upgrades all connections to HTTPS and warns on HTTP).",
                    Category = TweakCategory.PrivacyTelemetry,
                    DefaultChecked = true,
                    Policies = { PolicyEntry.Str("HttpsOnlyMode", "force_enabled") }
                }
            };
        }
    }

    internal static class TweakEngine
    {
        public const string PolicySubKeyPath = @"SOFTWARE\Policies\BraveSoftware\Brave";
        public const string UpdatePolicySubKeyPath = @"SOFTWARE\Policies\BraveSoftware\Update";
        private const string BackupSuffix = ".brave-tweaker.bak";

        private static readonly string[] BraveChannelDirs =
        {
            @"BraveSoftware\Brave-Browser\User Data",
            @"BraveSoftware\Brave-Browser-Beta\User Data",
            @"BraveSoftware\Brave-Browser-Nightly\User Data"
        };

        public static bool IsRunningAsAdministrator()
        {
            try
            {
                using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
                {
                    var principal = new WindowsPrincipal(identity);
                    return principal.IsInRole(WindowsBuiltInRole.Administrator);
                }
            }
            catch
            {
                return false;
            }
        }

        public static int GetRunningBraveProcessCount()
        {
            return Process.GetProcessesByName("brave").Length;
        }

        public static bool CloseBraveProcesses(Action<string> log)
        {
            Process[] procs = Process.GetProcessesByName("brave");
            if (procs.Length == 0)
            {
                return true;
            }

            log(string.Format("Closing {0} running Brave process(es)...", procs.Length));
            foreach (Process p in procs)
            {
                try { p.CloseMainWindow(); } catch { }
            }

            for (int i = 0; i < 20; i++)
            {
                if (Process.GetProcessesByName("brave").Length == 0)
                {
                    return true;
                }
                Thread.Sleep(250);
            }

            foreach (Process p in Process.GetProcessesByName("brave"))
            {
                try
                {
                    p.Kill();
                    p.WaitForExit(3000);
                }
                catch { }
            }

            return Process.GetProcessesByName("brave").Length == 0;
        }

        public static bool ApplySelected(
            IList<TweakItem> allTweaks,
            HashSet<string> selectedIds,
            bool useMachineScope,
            bool closeBrave,
            Action<string> log,
            IList<ContainerItem> containersToCreate = null)
        {
            if (useMachineScope && !IsRunningAsAdministrator())
            {
                log("[ERROR] HKLM (All Users) requires running as Administrator.");
                return false;
            }

            RegistryKey root = useMachineScope ? Registry.LocalMachine : Registry.CurrentUser;
            string hiveName = useMachineScope ? "HKLM" : "HKCU";

            int appliedPolicies = 0;
            int removedPolicies = 0;

            log(string.Format("=== Registry Policies ({0}) ===", hiveName));
            try
            {
                var groupedPolicies = new Dictionary<string, List<KeyValuePair<bool, PolicyEntry>>>(StringComparer.OrdinalIgnoreCase);
                foreach (TweakItem item in allTweaks)
                {
                    bool enabled = selectedIds.Contains(item.Id);
                    foreach (PolicyEntry policy in item.Policies)
                    {
                        string path = policy.EffectiveSubKeyPath;
                        List<KeyValuePair<bool, PolicyEntry>> bucket;
                        if (!groupedPolicies.TryGetValue(path, out bucket))
                        {
                            bucket = new List<KeyValuePair<bool, PolicyEntry>>();
                            groupedPolicies[path] = bucket;
                        }
                        bucket.Add(new KeyValuePair<bool, PolicyEntry>(enabled, policy));
                    }
                }

                foreach (var kvp in groupedPolicies)
                {
                    bool anyEnabledInSubKey = kvp.Value.Any(p => p.Key);
                    using (RegistryKey key = anyEnabledInSubKey
                        ? root.CreateSubKey(kvp.Key)
                        : root.OpenSubKey(kvp.Key, writable: true))
                    {
                        if (key == null)
                        {
                            continue;
                        }

                        foreach (var entry in kvp.Value)
                        {
                            bool enabled = entry.Key;
                            PolicyEntry policy = entry.Value;
                            if (enabled)
                            {
                                key.SetValue(policy.Name, policy.Value, policy.Kind);
                                appliedPolicies++;
                            }
                            else if (key.GetValue(policy.Name) != null)
                            {
                                key.DeleteValue(policy.Name, throwOnMissingValue: false);
                                removedPolicies++;
                            }
                        }
                    }
                }
                log(string.Format("[OK] Applied {0} policy value(s), cleared {1} unchecked policy value(s).", appliedPolicies, removedPolicies));
            }
            catch (Exception ex)
            {
                log("[ERROR] Registry update failed: " + ex.Message);
                return false;
            }

            // Collect selected Preferences & Local State JSON edits
            var prefsToApply = new List<PrefEntry>();
            var localStatePrefsToApply = new List<PrefEntry>();
            foreach (TweakItem item in allTweaks)
            {
                if (selectedIds.Contains(item.Id))
                {
                    if (item.Prefs.Count > 0)
                    {
                        prefsToApply.AddRange(item.Prefs);
                    }
                    if (item.LocalStatePrefs.Count > 0)
                    {
                        localStatePrefsToApply.AddRange(item.LocalStatePrefs);
                    }
                }
            }

            bool hasContainers = containersToCreate != null && containersToCreate.Count > 0;
            if (prefsToApply.Count == 0 && localStatePrefsToApply.Count == 0 && !hasContainers)
            {
                log("=== Profile Preferences ===");
                log("No profile JSON edits needed for the selected tweaks.");
                return true;
            }

            log("=== Brave Profile Preferences, Local State & Containers ===");
            List<string> prefFiles = FindBravePreferenceFiles();
            List<string> localStateFiles = FindBraveLocalStateFiles();
            if (prefFiles.Count == 0 && localStateFiles.Count == 0)
            {
                log("[WARN] No Brave profile files found in %LOCALAPPDATA%\\BraveSoftware.");
                log("Launch Brave once to create a profile, then re-run to apply UI preference tweaks and containers.");
                return true;
            }

            if (GetRunningBraveProcessCount() > 0)
            {
                if (closeBrave)
                {
                    if (!CloseBraveProcesses(log))
                    {
                        log("[WARN] Brave is still running. Skipped Preferences JSON edits so Brave won't overwrite them.");
                        return true;
                    }
                }
                else
                {
                    log("[WARN] Brave is currently running. Close Brave and re-apply to update profile Preferences.");
                    return true;
                }
            }

            int updatedProfiles = 0;
            bool hadErrors = false;
            if (prefsToApply.Count > 0 || hasContainers)
            {
                foreach (string prefPath in prefFiles)
                {
                    try
                    {
                        int addedContainers;
                        int skippedContainers;
                        PatchPreferencesFile(prefPath, prefsToApply, containersToCreate, out addedContainers, out skippedContainers);
                        if (hasContainers)
                        {
                            log(string.Format("[OK] Updated profile: {0} (Containers: +{1} created, {2} already existed)",
                                prefPath, addedContainers, skippedContainers));
                        }
                        else
                        {
                            log("[OK] Updated profile: " + prefPath);
                        }
                        updatedProfiles++;
                    }
                    catch (Exception ex)
                    {
                        log("[ERROR] Failed to update " + prefPath + ": " + ex.Message);
                        hadErrors = true;
                    }
                }
            }

            if (localStatePrefsToApply.Count > 0)
            {
                foreach (string localStatePath in localStateFiles)
                {
                    try
                    {
                        int unused1, unused2;
                        PatchPreferencesFile(localStatePath, localStatePrefsToApply, null, out unused1, out unused2);
                        log("[OK] Updated Local State: " + localStatePath);
                    }
                    catch (Exception ex)
                    {
                        log("[ERROR] Failed to update " + localStatePath + ": " + ex.Message);
                        hadErrors = true;
                    }
                }
            }

            log(string.Format("Done! Updated {0} profile(s). Open brave://policy in Brave to verify.", updatedProfiles));
            return !hadErrors;
        }

        public static bool ApplyContainersOnly(
            IList<ContainerItem> containersToCreate,
            bool closeBrave,
            Action<string> log)
        {
            if (containersToCreate == null || containersToCreate.Count == 0)
            {
                log("[INFO] No new containers to create.");
                return true;
            }

            log("=== Creating Brave Containers ===");
            List<string> prefFiles = FindBravePreferenceFiles();
            if (prefFiles.Count == 0)
            {
                log("[WARN] No Brave profile Preferences files found in %LOCALAPPDATA%\\BraveSoftware.");
                return false;
            }

            if (GetRunningBraveProcessCount() > 0)
            {
                if (closeBrave)
                {
                    if (!CloseBraveProcesses(log))
                    {
                        log("[WARN] Brave is still running. Close Brave before creating containers.");
                        return false;
                    }
                }
                else
                {
                    log("[WARN] Brave is currently running. Close Brave and retry.");
                    return false;
                }
            }

            var emptyPrefs = new List<PrefEntry>();
            bool hadErrors = false;
            foreach (string prefPath in prefFiles)
            {
                try
                {
                    int added;
                    int skipped;
                    PatchPreferencesFile(prefPath, emptyPrefs, containersToCreate, out added, out skipped);
                    log(string.Format("[OK] {0}: +{1} created, {2} skipped (already existed)", prefPath, added, skipped));
                }
                catch (Exception ex)
                {
                    log("[ERROR] Failed to update " + prefPath + ": " + ex.Message);
                    hadErrors = true;
                }
            }

            return !hadErrors;
        }

        public static List<ContainerItem> LoadExistingContainers()
        {
            var result = new List<ContainerItem>();
            var seenNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            List<string> prefFiles = FindBravePreferenceFiles();
            if (prefFiles.Count == 0)
            {
                return result;
            }

            JavaScriptSerializer serializer = CreateSerializer();
            foreach (string prefPath in prefFiles)
            {
                try
                {
                    string json = File.ReadAllText(prefPath, Encoding.UTF8);
                    var root = serializer.DeserializeObject(json) as Dictionary<string, object>;
                    if (root == null)
                    {
                        continue;
                    }

                    object listObj = GetNestedValue(root, new[] { "brave", "containers", "list" });
                    object[] items = listObj as object[];
                    if (items == null)
                    {
                        continue;
                    }

                    foreach (object raw in items)
                    {
                        var dict = raw as Dictionary<string, object>;
                        if (dict == null)
                        {
                            continue;
                        }

                        string name = dict.ContainsKey("name") && dict["name"] != null ? dict["name"].ToString() : string.Empty;
                        if (string.IsNullOrEmpty(name) || !seenNames.Add(name))
                        {
                            continue;
                        }

                        string id = dict.ContainsKey("id") && dict["id"] != null ? dict["id"].ToString() : string.Empty;
                        int icon = 0;
                        if (dict.ContainsKey("icon") && dict["icon"] != null)
                        {
                            int.TryParse(dict["icon"].ToString(), out icon);
                        }

                        int bgColor = unchecked((int)0xFF488CFF);
                        if (dict.ContainsKey("background_color") && dict["background_color"] != null)
                        {
                            int.TryParse(dict["background_color"].ToString(), out bgColor);
                        }

                        result.Add(new ContainerItem
                        {
                            Id = id,
                            Name = name,
                            Icon = icon,
                            BackgroundColor = bgColor,
                            IsExisting = true
                        });
                    }
                }
                catch { }
            }

            return result;
        }

        public static bool RevertAll(IList<TweakItem> allTweaks, bool closeBrave, Action<string> log)
        {
            log("=== Removing Registry Policies ===");
            RemovePoliciesFromHive(Registry.CurrentUser, "HKCU", allTweaks, log);
            if (IsRunningAsAdministrator())
            {
                RemovePoliciesFromHive(Registry.LocalMachine, "HKLM", allTweaks, log);
            }
            else
            {
                log("[INFO] Not elevated; skipped HKLM cleanup (only needed if you previously applied with All Users).");
            }

            log("=== Restoring Brave Profile Preferences & Local State Backups ===");
            List<string> prefFiles = FindBravePreferenceFiles();
            List<string> localStateFiles = FindBraveLocalStateFiles();
            var allBackupTargets = new List<string>();
            allBackupTargets.AddRange(prefFiles);
            allBackupTargets.AddRange(localStateFiles);

            if (allBackupTargets.Count == 0)
            {
                log("No Brave profiles found.");
                return true;
            }

            if (GetRunningBraveProcessCount() > 0)
            {
                if (closeBrave)
                {
                    if (!CloseBraveProcesses(log))
                    {
                        log("[WARN] Brave is still running. Could not restore Preferences backups.");
                        return false;
                    }
                }
                else
                {
                    log("[WARN] Brave is currently running. Close Brave and retry Revert.");
                    return false;
                }
            }

            int restored = 0;
            foreach (string filePath in allBackupTargets)
            {
                string backupPath = filePath + BackupSuffix;
                if (File.Exists(backupPath))
                {
                    try
                    {
                        File.Copy(backupPath, filePath, overwrite: true);
                        File.Delete(backupPath);
                        log("[OK] Restored backup: " + filePath);
                        restored++;
                    }
                    catch (Exception ex)
                    {
                        log("[ERROR] Failed to restore " + filePath + ": " + ex.Message);
                    }
                }
                else
                {
                    log("[--] No backup found for: " + filePath);
                }
            }

            log(string.Format("Revert complete! Restored {0} backup(s).", restored));
            return true;
        }

        public static void ExportRegFile(
            IList<TweakItem> allTweaks,
            HashSet<string> selectedIds,
            bool useMachineScope,
            string filePath)
        {
            string hiveFull = useMachineScope ? "HKEY_LOCAL_MACHINE" : "HKEY_CURRENT_USER";
            var sb = new StringBuilder();
            sb.AppendLine("Windows Registry Editor Version 5.00");
            sb.AppendLine();
            sb.AppendLine("; Generated by BraveTweaker");

            var grouped = new Dictionary<string, List<KeyValuePair<string, PolicyEntry>>>(StringComparer.OrdinalIgnoreCase);
            foreach (TweakItem item in allTweaks)
            {
                if (!selectedIds.Contains(item.Id) || item.Policies.Count == 0)
                {
                    continue;
                }

                foreach (PolicyEntry policy in item.Policies)
                {
                    string path = policy.EffectiveSubKeyPath;
                    List<KeyValuePair<string, PolicyEntry>> bucket;
                    if (!grouped.TryGetValue(path, out bucket))
                    {
                        bucket = new List<KeyValuePair<string, PolicyEntry>>();
                        grouped[path] = bucket;
                    }
                    bucket.Add(new KeyValuePair<string, PolicyEntry>(item.Title, policy));
                }
            }

            foreach (var kvp in grouped)
            {
                sb.AppendLine();
                sb.AppendLine(string.Format("[{0}\\{1}]", hiveFull, kvp.Key));
                string lastTitle = null;
                foreach (var entry in kvp.Value)
                {
                    if (entry.Key != lastTitle)
                    {
                        sb.AppendLine("; " + entry.Key);
                        lastTitle = entry.Key;
                    }

                    PolicyEntry policy = entry.Value;
                    if (policy.Kind == RegistryValueKind.DWord)
                    {
                        int val = Convert.ToInt32(policy.Value);
                        sb.AppendLine(string.Format("\"{0}\"=dword:{1:x8}", policy.Name, val));
                    }
                    else
                    {
                        string escaped = (policy.Value ?? string.Empty).ToString()
                            .Replace("\\", "\\\\")
                            .Replace("\"", "\\\"");
                        sb.AppendLine(string.Format("\"{0}\"=\"{1}\"", policy.Name, escaped));
                    }
                }
            }

            File.WriteAllText(filePath, sb.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }

        public static Dictionary<string, bool> GetAppliedStates(IList<TweakItem> allTweaks, bool useMachineScope)
        {
            var states = new Dictionary<string, bool>();
            RegistryKey primary = useMachineScope ? Registry.LocalMachine : Registry.CurrentUser;
            RegistryKey fallback = useMachineScope ? Registry.CurrentUser : Registry.LocalMachine;

            Dictionary<string, object> firstPrefRoot = null;
            List<string> prefFiles = FindBravePreferenceFiles();
            if (prefFiles.Count > 0)
            {
                try
                {
                    string json = File.ReadAllText(prefFiles[0], Encoding.UTF8);
                    firstPrefRoot = CreateSerializer().DeserializeObject(json) as Dictionary<string, object>;
                }
                catch { }
            }

            foreach (TweakItem item in allTweaks)
            {
                bool active = false;
                if (item.Policies.Count > 0)
                {
                    active = true;
                    foreach (PolicyEntry p in item.Policies)
                    {
                        object val = ReadRegistryValue(primary, p.EffectiveSubKeyPath, p.Name);
                        if (val == null)
                        {
                            val = ReadRegistryValue(fallback, p.EffectiveSubKeyPath, p.Name);
                        }

                        if (!ValuesEqual(val, p.Value))
                        {
                            active = false;
                            break;
                        }
                    }
                }
                else if (item.Prefs.Count > 0 && firstPrefRoot != null)
                {
                    active = true;
                    foreach (PrefEntry pref in item.Prefs)
                    {
                        object val = GetNestedValue(firstPrefRoot, pref.Path);
                        if (pref.IsArrayAppend)
                        {
                            if (!ArrayContainsString(val, (pref.Value ?? string.Empty).ToString()))
                            {
                                active = false;
                                break;
                            }
                        }
                        else if (!ValuesEqual(val, pref.Value))
                        {
                            active = false;
                            break;
                        }
                    }
                }

                states[item.Id] = active;
            }

            return states;
        }

        private static object ReadRegistryValue(RegistryKey root, string subKeyPath, string name)
        {
            try
            {
                using (RegistryKey key = root.OpenSubKey(subKeyPath, writable: false))
                {
                    return key != null ? key.GetValue(name) : null;
                }
            }
            catch
            {
                return null;
            }
        }

        public static List<string> FindBraveLocalStateFiles()
        {
            var results = new List<string>();
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (string.IsNullOrEmpty(localAppData))
            {
                return results;
            }

            foreach (string relativeDir in BraveChannelDirs)
            {
                string localStatePath = Path.Combine(localAppData, relativeDir, "Local State");
                if (File.Exists(localStatePath))
                {
                    results.Add(localStatePath);
                }
            }

            return results;
        }

        public static List<string> FindBravePreferenceFiles()
        {
            var results = new List<string>();
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (string.IsNullOrEmpty(localAppData))
            {
                return results;
            }

            foreach (string relativeDir in BraveChannelDirs)
            {
                string userDataDir = Path.Combine(localAppData, relativeDir);
                if (!Directory.Exists(userDataDir))
                {
                    continue;
                }

                try
                {
                    foreach (string subDir in Directory.GetDirectories(userDataDir))
                    {
                        string name = Path.GetFileName(subDir);
                        if (string.Equals(name, "System Profile", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(name, "Guest Profile", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        string prefPath = Path.Combine(subDir, "Preferences");
                        if (File.Exists(prefPath))
                        {
                            results.Add(prefPath);
                        }
                    }
                }
                catch { }
            }

            return results;
        }

        private static void RemovePoliciesFromHive(
            RegistryKey root,
            string hiveName,
            IList<TweakItem> allTweaks,
            Action<string> log)
        {
            var byPath = new Dictionary<string, List<PolicyEntry>>(StringComparer.OrdinalIgnoreCase);
            foreach (TweakItem item in allTweaks)
            {
                foreach (PolicyEntry p in item.Policies)
                {
                    string path = p.EffectiveSubKeyPath;
                    List<PolicyEntry> bucket;
                    if (!byPath.TryGetValue(path, out bucket))
                    {
                        bucket = new List<PolicyEntry>();
                        byPath[path] = bucket;
                    }
                    bucket.Add(p);
                }
            }

            int totalRemoved = 0;
            foreach (var kvp in byPath)
            {
                try
                {
                    using (RegistryKey key = root.OpenSubKey(kvp.Key, writable: true))
                    {
                        if (key == null)
                        {
                            continue;
                        }

                        foreach (PolicyEntry p in kvp.Value)
                        {
                            if (key.GetValue(p.Name) != null)
                            {
                                key.DeleteValue(p.Name, throwOnMissingValue: false);
                                totalRemoved++;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    log(string.Format("[ERROR] Could not clean {0}\\{1}: {2}", hiveName, kvp.Key, ex.Message));
                }
            }

            if (totalRemoved > 0)
            {
                log(string.Format("[OK] Removed {0} policy value(s) from {1}.", totalRemoved, hiveName));
            }
            else
            {
                log(string.Format("[--] No managed policy values found in {0}.", hiveName));
            }
        }

        private static void PatchPreferencesFile(
            string prefPath,
            IList<PrefEntry> prefsToApply,
            IList<ContainerItem> containersToCreate,
            out int addedContainers,
            out int skippedContainers)
        {
            addedContainers = 0;
            skippedContainers = 0;

            string backupPath = prefPath + BackupSuffix;
            if (!File.Exists(backupPath))
            {
                File.Copy(prefPath, backupPath, overwrite: false);
            }

            string json = File.ReadAllText(prefPath, Encoding.UTF8);
            JavaScriptSerializer serializer = CreateSerializer();
            var root = serializer.DeserializeObject(json) as Dictionary<string, object>;
            if (root == null)
            {
                throw new InvalidDataException("Preferences file did not contain a valid JSON object.");
            }

            foreach (PrefEntry entry in prefsToApply)
            {
                if (entry.IsArrayAppend)
                {
                    AppendNestedStringArray(root, (entry.Value ?? string.Empty).ToString(), entry.Path);
                }
                else
                {
                    SetNestedValue(root, entry.Value, entry.Path);
                }
            }

            if (containersToCreate != null && containersToCreate.Count > 0)
            {
                MergeContainersIntoRoot(root, containersToCreate, out addedContainers, out skippedContainers);
            }

            string updatedJson = serializer.Serialize(root);
            string tempPath = prefPath + ".tmp";
            File.WriteAllText(tempPath, updatedJson, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            File.Copy(tempPath, prefPath, overwrite: true);
            File.Delete(tempPath);
        }

        private static void AppendNestedStringArray(Dictionary<string, object> root, string valueToAdd, string[] path)
        {
            object existingObj = GetNestedValue(root, path);
            var list = new List<object>();
            bool alreadyPresent = false;

            object[] existingArr = existingObj as object[];
            if (existingArr != null)
            {
                foreach (object item in existingArr)
                {
                    list.Add(item);
                    if (item != null && string.Equals(item.ToString(), valueToAdd, StringComparison.OrdinalIgnoreCase))
                    {
                        alreadyPresent = true;
                    }
                }
            }

            if (!alreadyPresent)
            {
                list.Add(valueToAdd);
            }

            SetNestedValue(root, list, path);
        }

        private static bool ArrayContainsString(object arrayObj, string target)
        {
            object[] arr = arrayObj as object[];
            if (arr == null)
            {
                return false;
            }

            foreach (object item in arr)
            {
                if (item != null && string.Equals(item.ToString(), target, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static void MergeContainersIntoRoot(
            Dictionary<string, object> root,
            IList<ContainerItem> desired,
            out int addedCount,
            out int skippedCount)
        {
            addedCount = 0;
            skippedCount = 0;

            var mergedList = new List<object>();
            var existingNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var existingIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            object existingListObj = GetNestedValue(root, new[] { "brave", "containers", "list" });
            object[] existingArray = existingListObj as object[];
            if (existingArray != null)
            {
                foreach (object raw in existingArray)
                {
                    mergedList.Add(raw);
                    var dict = raw as Dictionary<string, object>;
                    if (dict != null)
                    {
                        if (dict.ContainsKey("name") && dict["name"] != null)
                        {
                            existingNames.Add(dict["name"].ToString().Trim());
                        }
                        if (dict.ContainsKey("id") && dict["id"] != null)
                        {
                            existingIds.Add(dict["id"].ToString().Trim());
                        }
                    }
                }
            }

            foreach (ContainerItem item in desired)
            {
                string trimmedName = (item.Name ?? string.Empty).Trim();
                if (string.IsNullOrEmpty(trimmedName))
                {
                    continue;
                }

                bool alreadyExists = existingNames.Contains(trimmedName) ||
                    (!string.IsNullOrEmpty(item.Id) && existingIds.Contains(item.Id.Trim()));

                if (alreadyExists)
                {
                    skippedCount++;
                    continue;
                }

                string newId = string.IsNullOrEmpty(item.Id) ? Guid.NewGuid().ToString("D") : item.Id;
                var newDict = new Dictionary<string, object>
                {
                    { "background_color", item.BackgroundColor },
                    { "icon", item.Icon },
                    { "id", newId },
                    { "name", trimmedName }
                };

                mergedList.Add(newDict);
                existingNames.Add(trimmedName);
                existingIds.Add(newId);
                addedCount++;
            }

            SetNestedValue(root, true, new[] { "brave", "containers", "enabled" });
            SetNestedValue(root, mergedList, new[] { "brave", "containers", "list" });
        }

        private static void SetNestedValue(Dictionary<string, object> root, object value, string[] path)
        {
            Dictionary<string, object> current = root;
            for (int i = 0; i < path.Length - 1; i++)
            {
                string segment = path[i];
                object nextObj;
                if (!current.TryGetValue(segment, out nextObj) || !(nextObj is Dictionary<string, object>))
                {
                    var child = new Dictionary<string, object>();
                    current[segment] = child;
                    current = child;
                }
                else
                {
                    current = (Dictionary<string, object>)nextObj;
                }
            }

            current[path[path.Length - 1]] = value;
        }

        private static object GetNestedValue(Dictionary<string, object> root, string[] path)
        {
            Dictionary<string, object> current = root;
            for (int i = 0; i < path.Length - 1; i++)
            {
                object nextObj;
                if (!current.TryGetValue(path[i], out nextObj))
                {
                    return null;
                }

                current = nextObj as Dictionary<string, object>;
                if (current == null)
                {
                    return null;
                }
            }

            object result;
            return current.TryGetValue(path[path.Length - 1], out result) ? result : null;
        }

        private static bool ValuesEqual(object actual, object expected)
        {
            if (actual == null || expected == null)
            {
                return false;
            }

            if (expected is int)
            {
                int actualInt;
                return int.TryParse(actual.ToString(), out actualInt) && actualInt == (int)expected;
            }

            if (expected is bool)
            {
                bool actualBool;
                return bool.TryParse(actual.ToString(), out actualBool) && actualBool == (bool)expected;
            }

            return string.Equals(actual.ToString(), expected.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        private static JavaScriptSerializer CreateSerializer()
        {
            return new JavaScriptSerializer
            {
                MaxJsonLength = int.MaxValue,
                RecursionLimit = 100
            };
        }
    }
}
