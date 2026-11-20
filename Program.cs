using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace BraveTweaker
{
    internal static class Program
    {
        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        [DllImport("kernel32.dll")]
        private static extern bool AttachConsole(int dwProcessId);

        private const int ATTACH_PARENT_PROCESS = -1;

        [STAThread]
        private static int Main(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                try { SetProcessDPIAware(); } catch { }
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                using (var splash = new SplashForm())
                {
                    Application.Run(splash);
                }
                Application.Run(new MainForm());
                return 0;
            }

            // Attach to parent console when invoked with CLI flags from a terminal
            AttachConsole(ATTACH_PARENT_PROCESS);
            return RunCli(args);
        }

        private static int RunCli(string[] args)
        {
            bool modeApply = false;
            bool modeRevert = false;
            bool modeStatus = false;
            string exportRegPath = null;
            string preset = "recommended";
            bool useMachineScope = false;
            bool forceUserScope = false;
            bool closeBrave = false;

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i].Trim().ToLowerInvariant();
                switch (arg)
                {
                    case "--apply":
                    case "-a":
                        modeApply = true;
                        break;
                    case "--revert":
                    case "-r":
                        modeRevert = true;
                        break;
                    case "--status":
                    case "-s":
                        modeStatus = true;
                        break;
                    case "--export-reg":
                    case "-e":
                        if (i + 1 >= args.Length)
                        {
                            Console.WriteLine("[ERROR] --export-reg requires a file path.");
                            return 1;
                        }
                        exportRegPath = args[++i];
                        break;
                    case "--preset":
                    case "-p":
                        if (i + 1 >= args.Length)
                        {
                            Console.WriteLine("[ERROR] --preset requires a value: recommended, ui, prompts, privacy, all.");
                            return 1;
                        }
                        preset = args[++i].Trim().ToLowerInvariant();
                        break;
                    case "--machine":
                    case "--hklm":
                        useMachineScope = true;
                        break;
                    case "--user":
                    case "--hkcu":
                        forceUserScope = true;
                        break;
                    case "--close-brave":
                        closeBrave = true;
                        break;
                    case "--help":
                    case "-h":
                    case "/?":
                        PrintHelp();
                        return 0;
                    default:
                        Console.WriteLine("[ERROR] Unknown argument: " + args[i]);
                        PrintHelp();
                        return 1;
                }
            }

            bool isAdmin = TweakEngine.IsRunningAsAdministrator();
            if (!useMachineScope && !forceUserScope)
            {
                useMachineScope = isAdmin;
            }

            List<TweakItem> allTweaks = TweakCatalog.GetAll();
            HashSet<string> selected = ResolvePreset(allTweaks, preset);

            if (!string.IsNullOrEmpty(exportRegPath))
            {
                try
                {
                    TweakEngine.ExportRegFile(allTweaks, selected, useMachineScope, exportRegPath);
                    Console.WriteLine("[OK] Exported registry policies to: " + Path.GetFullPath(exportRegPath));
                    return 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[ERROR] Export failed: " + ex.Message);
                    return 1;
                }
            }

            if (modeStatus)
            {
                Dictionary<string, bool> states = TweakEngine.GetAppliedStates(allTweaks, useMachineScope);
                Console.WriteLine("BraveTweaker Status (Scope: {0})", useMachineScope ? "HKLM" : "HKCU");
                Console.WriteLine();
                foreach (TweakItem item in allTweaks)
                {
                    bool active = states.ContainsKey(item.Id) && states[item.Id];
                    Console.WriteLine("  [{0}] {1}", active ? "ACTIVE  " : "inactive", item.Title);
                }
                return 0;
            }

            if (modeApply)
            {
                bool ok = TweakEngine.ApplySelected(allTweaks, selected, useMachineScope, closeBrave, Console.WriteLine);
                return ok ? 0 : 1;
            }

            if (modeRevert)
            {
                bool ok = TweakEngine.RevertAll(allTweaks, closeBrave, Console.WriteLine);
                return ok ? 0 : 1;
            }

            PrintHelp();
            return 0;
        }

        private static HashSet<string> ResolvePreset(List<TweakItem> allTweaks, string preset)
        {
            var ids = new HashSet<string>();
            foreach (TweakItem item in allTweaks)
            {
                bool include = false;
                switch (preset)
                {
                    case "recommended":
                        include = item.DefaultChecked;
                        break;
                    case "ui":
                        include = item.Category == TweakCategory.BraveUI && item.Id != "disable_tor";
                        break;
                    case "prompts":
                        include = item.Category == TweakCategory.PromptsBehavior;
                        break;
                    case "privacy":
                        include = item.Category == TweakCategory.PrivacyTelemetry;
                        break;
                    case "all":
                        include = true;
                        break;
                    default:
                        include = item.DefaultChecked;
                        break;
                }

                if (include)
                {
                    ids.Add(item.Id);
                }
            }
            return ids;
        }

        private static void PrintHelp()
        {
            Console.WriteLine("BraveTweaker.exe - GUI & CLI Brave Browser Tweaker");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("  BraveTweaker.exe                              Launch Windows GUI");
            Console.WriteLine("  BraveTweaker.exe --apply [options]            Apply tweaks headlessly");
            Console.WriteLine("  BraveTweaker.exe --revert [options]           Remove policies & restore Preferences backup");
            Console.WriteLine("  BraveTweaker.exe --status                     Show active status of all tweaks");
            Console.WriteLine("  BraveTweaker.exe --export-reg <path.reg>      Export selected preset policies to a .reg file");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  --preset <recommended|ui|prompts|privacy|all> Preset to apply/export (default: recommended)");
            Console.WriteLine("  --user / --hkcu                               Write policies to Current User (no Admin needed)");
            Console.WriteLine("  --machine / --hklm                            Write policies to Local Machine (requires Admin)");
            Console.WriteLine("  --close-brave                                 Close Brave before editing Preferences");
        }
    }
}
