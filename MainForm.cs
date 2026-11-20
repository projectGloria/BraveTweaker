using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace BraveTweaker
{
    internal sealed class MainForm : Form
    {
        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;
        private const int WM_NCHITTEST = 0x84;
        private const int ResizeBorder = 6;

        private readonly List<TweakItem> _tweaks;
        private readonly Dictionary<string, BraveToggleSwitch> _toggles = new Dictionary<string, BraveToggleSwitch>();
        private readonly Dictionary<string, Label> _statusBadges = new Dictionary<string, Label>();
        private readonly Dictionary<string, Panel> _tweakCards = new Dictionary<string, Panel>();
        private readonly List<BraveScrollPanel> _scrollPanels = new List<BraveScrollPanel>();
        private readonly List<ContainerItem> _containers = new List<ContainerItem>();
        private readonly bool _isAdmin;

        private readonly List<Panel> _navItems = new List<Panel>();
        private readonly List<Label> _navLabels = new List<Label>();
        private readonly List<Label> _navIcons = new List<Label>();
        private readonly List<Control> _pages = new List<Control>();
        private readonly List<string> _pageTitles = new List<string>();
        private int _activeNavIndex;
        private Label _containersNavLabel;

        private Label _lblPageTitle;
        private Label _lblSummary;
        private TextBox _txtSearch;
        private Panel _searchWrap;
        private BraveSegmentedScope _scopeControl;
        private BraveToggleSwitch _swCloseBrave;
        private BraveDarkLogBox _logBox;
        private Image _brandLogo;

        // Containers page controls
        private ListView _lvContainers;
        private TextBox _txtContainerName;
        private ComboBox _cmbContainerColor;
        private ComboBox _cmbContainerIcon;
        private Panel _pnlColorSwatch;
        private Color _selectedContainerColor = Color.FromArgb(unchecked((int)0xFF488CFF));

        private static readonly KeyValuePair<string, Color>[] ContainerColorPalette =
        {
            new KeyValuePair<string, Color>("Blue", Color.FromArgb(unchecked((int)0xFF488CFF))),
            new KeyValuePair<string, Color>("Red", Color.FromArgb(unchecked((int)0xFFFF3C36))),
            new KeyValuePair<string, Color>("Purple", Color.FromArgb(unchecked((int)0xFF9973F8))),
            new KeyValuePair<string, Color>("Green", Color.FromArgb(unchecked((int)0xFF2FAC5F))),
            new KeyValuePair<string, Color>("Orange", Color.FromArgb(unchecked((int)0xFFFF7A00))),
            new KeyValuePair<string, Color>("Yellow", Color.FromArgb(unchecked((int)0xFFF5B800))),
            new KeyValuePair<string, Color>("Teal", Color.FromArgb(unchecked((int)0xFF00A8A8))),
            new KeyValuePair<string, Color>("Pink", Color.FromArgb(unchecked((int)0xFFEC4899)))
        };

        private static readonly string[] ContainerIconNames =
        {
            "0 - Personal",
            "1 - Work",
            "2 - Shopping",
            "3 - Social",
            "4 - Events",
            "5 - Messenger",
            "6 - Gaming",
            "7 - Banking",
            "8 - Education",
            "9 - News",
            "10 - Entertainment",
            "11 - Travel",
            "12 - Fitness",
            "13 - Food",
            "14 - Pet",
            "15 - Default"
        };

        public MainForm()
        {
            _tweaks = TweakCatalog.GetAll();
            _isAdmin = TweakEngine.IsRunningAsAdministrator();
            _brandLogo = SplashForm.LoadEmbeddedLogo();

            Text = "BraveTweaker";
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(960, 680);
            Size = new Size(1060, 770);
            BackColor = BraveTheme.Obsidian;
            ForeColor = BraveTheme.TextPrimary;
            Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
            DoubleBuffered = true;
            Padding = new Padding(1); // 1px crisp dark outer border

            try
            {
                Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            }
            catch { }

            BuildLayout();
            ApplyPreset("recommended");
            LoadContainersFromBrave(keepPending: false);
            RefreshStatusBadges();
            SelectNav(0);
            Shown += (s, e) =>
            {
                foreach (BraveScrollPanel sp in _scrollPanels)
                {
                    sp.LayoutCards();
                }
                RefreshStatusBadges();
            };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (var pen = new Pen(BraveTheme.Border, 1))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_NCHITTEST && WindowState == FormWindowState.Normal)
            {
                Point pos = PointToClient(new Point(m.LParam.ToInt32()));
                bool left = pos.X <= ResizeBorder;
                bool right = pos.X >= Width - ResizeBorder;
                bool top = pos.Y <= ResizeBorder;
                bool bottom = pos.Y >= Height - ResizeBorder;

                if (top && left) { m.Result = (IntPtr)13; return; }     // HTTOPLEFT
                if (top && right) { m.Result = (IntPtr)14; return; }    // HTTOPRIGHT
                if (bottom && left) { m.Result = (IntPtr)16; return; }  // HTBOTTOMLEFT
                if (bottom && right) { m.Result = (IntPtr)17; return; } // HTBOTTOMRIGHT
                if (left) { m.Result = (IntPtr)10; return; }            // HTLEFT
                if (right) { m.Result = (IntPtr)11; return; }           // HTRIGHT
                if (top) { m.Result = (IntPtr)12; return; }             // HTTOP
                if (bottom) { m.Result = (IntPtr)15; return; }          // HTBOTTOM
            }
            base.WndProc(ref m);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _brandLogo != null)
            {
                _brandLogo.Dispose();
                _brandLogo = null;
            }
            base.Dispose(disposing);
        }

        private void DragWindow()
        {
            ReleaseCapture();
            SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
        }

        private void BuildLayout()
        {
            // Top 3px Brave brand gradient stripe
            var topAccent = new Panel { Dock = DockStyle.Top, Height = 3 };
            topAccent.Paint += (s, e) =>
            {
                using (var grad = new LinearGradientBrush(
                    topAccent.ClientRectangle,
                    BraveTheme.BraveOrange,
                    BraveTheme.BraveMagenta,
                    LinearGradientMode.Horizontal))
                {
                    e.Graphics.FillRectangle(grad, topAccent.ClientRectangle);
                }
            };

            // Custom frameless title bar
            var titleBar = BuildCustomTitleBar();

            var shell = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = BraveTheme.Obsidian,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            shell.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 240));
            shell.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

            var mainWorkspace = BuildMainWorkspace();
            var sidebar = BuildSidebar();

            shell.Controls.Add(sidebar, 0, 0);
            shell.Controls.Add(mainWorkspace, 1, 0);

            Controls.Add(shell);
            Controls.Add(titleBar);
            Controls.Add(topAccent);
        }

        private Control BuildCustomTitleBar()
        {
            var bar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 36,
                BackColor = BraveTheme.TitleBarBg
            };
            bar.Paint += (s, e) =>
            {
                using (var pen = new Pen(BraveTheme.Divider, 1))
                {
                    e.Graphics.DrawLine(pen, 0, bar.Height - 1, bar.Width, bar.Height - 1);
                }
            };
            bar.MouseDown += (s, e) => { if (e.Button == MouseButtons.Left) DragWindow(); };
            bar.DoubleClick += (s, e) => ToggleMaximize();

            int x = 12;
            if (_brandLogo != null)
            {
                var pic = new PictureBox
                {
                    Image = _brandLogo,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Size = new Size(18, 18),
                    Location = new Point(x, 9),
                    BackColor = Color.Transparent
                };
                pic.MouseDown += (s, e) => { if (e.Button == MouseButtons.Left) DragWindow(); };
                bar.Controls.Add(pic);
                x += 26;
            }

            var lbl = new Label
            {
                Text = "BraveTweaker",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = BraveTheme.TextSecondary,
                AutoSize = true,
                Location = new Point(x, 9),
                UseMnemonic = false
            };
            lbl.MouseDown += (s, e) => { if (e.Button == MouseButtons.Left) DragWindow(); };
            lbl.DoubleClick += (s, e) => ToggleMaximize();
            bar.Controls.Add(lbl);

            var btnClose = new BraveWindowButton("\uE8BB", isClose: true) { Dock = DockStyle.Right };
            btnClose.Click += (s, e) => Close();

            var btnMax = new BraveWindowButton("\uE922", isClose: false) { Dock = DockStyle.Right };
            btnMax.Click += (s, e) => ToggleMaximize();

            var btnMin = new BraveWindowButton("\uE921", isClose: false) { Dock = DockStyle.Right };
            btnMin.Click += (s, e) => { WindowState = FormWindowState.Minimized; };

            bar.Controls.Add(btnMin);
            bar.Controls.Add(btnMax);
            bar.Controls.Add(btnClose);
            return bar;
        }

        private void ToggleMaximize()
        {
            if (WindowState == FormWindowState.Maximized)
            {
                WindowState = FormWindowState.Normal;
            }
            else
            {
                MaximumSize = Screen.FromHandle(Handle).WorkingArea.Size;
                WindowState = FormWindowState.Maximized;
            }
        }

        private Control BuildSidebar()
        {
            var rail = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = BraveTheme.SidebarBg,
                Padding = new Padding(14, 16, 14, 14)
            };
            rail.Paint += (s, e) =>
            {
                using (var pen = new Pen(BraveTheme.Divider, 1))
                {
                    e.Graphics.DrawLine(pen, rail.Width - 1, 0, rail.Width - 1, rail.Height);
                }
            };

            // Brand block
            var brandPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 62,
                BackColor = BraveTheme.SidebarBg
            };

            int titleX = 0;
            if (_brandLogo != null)
            {
                var pic = new PictureBox
                {
                    Image = _brandLogo,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Size = new Size(44, 44),
                    Location = new Point(0, 4),
                    BackColor = Color.Transparent
                };
                brandPanel.Controls.Add(pic);
                titleX = 54;
            }

            brandPanel.Controls.Add(new Label
            {
                Text = "BraveTweaker",
                Font = new Font("Segoe UI", 12.5f, FontStyle.Bold),
                ForeColor = BraveTheme.TextPrimary,
                AutoSize = true,
                Location = new Point(titleX, 4),
                UseMnemonic = false
            });

            brandPanel.Controls.Add(new Label
            {
                Text = _isAdmin ? "ADMINISTRATOR" : "STANDARD USER",
                Font = new Font("Segoe UI", 7.25f, FontStyle.Bold),
                ForeColor = _isAdmin ? BraveTheme.BraveFlame : BraveTheme.TextMuted,
                BackColor = _isAdmin ? Color.FromArgb(46, 24, 22) : BraveTheme.InputBg,
                AutoSize = true,
                Padding = new Padding(6, 2, 6, 2),
                Location = new Point(titleX + 1, 30),
                UseMnemonic = false
            });

            // Navigation items with icons
            var navPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = BraveTheme.SidebarBg,
                Padding = new Padding(0, 12, 0, 8)
            };

            navPanel.Controls.Add(CreateSidebarHeading("SECTIONS"));

            int uiCount = _tweaks.Count(t => t.Category == TweakCategory.BraveUI);
            int promptCount = _tweaks.Count(t => t.Category == TweakCategory.PromptsBehavior);
            int privacyCount = _tweaks.Count(t => t.Category == TweakCategory.PrivacyTelemetry);

            AddNavItem(navPanel, TweakIcons.NavBloat, string.Format("Brave Bloat & UI ({0})", uiCount), 0);
            AddNavItem(navPanel, TweakIcons.NavPrompts, string.Format("Prompts & Behavior ({0})", promptCount), 1);
            AddNavItem(navPanel, TweakIcons.NavPrivacy, string.Format("Privacy & Telemetry ({0})", privacyCount), 2);
            _containersNavLabel = AddNavItem(navPanel, TweakIcons.NavContainers, "Containers", 3);

            // Quick Presets
            var presetSection = new Panel
            {
                Dock = DockStyle.Top,
                Height = 142,
                BackColor = BraveTheme.SidebarBg,
                Padding = new Padding(0, 10, 0, 0)
            };
            var presetTitle = CreateSidebarHeading("QUICK PRESETS");
            presetTitle.Dock = DockStyle.Top;

            var presetFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                BackColor = BraveTheme.SidebarBg,
                Padding = new Padding(0, 6, 0, 0)
            };
            presetFlow.Controls.Add(CreatePillButton("Recommended", () => ApplyPreset("recommended")));
            presetFlow.Controls.Add(CreatePillButton("Clean UI", () => ApplyPreset("ui")));
            presetFlow.Controls.Add(CreatePillButton("No Prompts", () => ApplyPreset("prompts")));
            presetFlow.Controls.Add(CreatePillButton("Strict Privacy", () => ApplyPreset("privacy")));
            presetFlow.Controls.Add(CreatePillButton("Select All", () => ApplyPreset("all")));
            presetFlow.Controls.Add(CreatePillButton("Select None", () => ApplyPreset("none")));

            presetSection.Controls.Add(presetFlow);
            presetSection.Controls.Add(presetTitle);

            // Bottom Policy Scope + Auto-Close
            var bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 122,
                BackColor = BraveTheme.SidebarBg,
                Padding = new Padding(0, 10, 0, 0)
            };
            bottomPanel.Paint += (s, e) =>
            {
                using (var pen = new Pen(BraveTheme.Divider, 1))
                {
                    e.Graphics.DrawLine(pen, 0, 0, bottomPanel.Width, 0);
                }
            };

            var scopeTitle = CreateSidebarHeading("POLICY SCOPE");
            scopeTitle.Location = new Point(0, 12);
            bottomPanel.Controls.Add(scopeTitle);

            _scopeControl = new BraveSegmentedScope(_isAdmin)
            {
                Location = new Point(0, 32),
                Size = new Size(210, 30)
            };
            _scopeControl.ScopeChanged += (s, e) => RefreshStatusBadges();
            bottomPanel.Controls.Add(_scopeControl);

            _swCloseBrave = new BraveToggleSwitch
            {
                Checked = true,
                Location = new Point(0, 80)
            };
            var lblClose = new Label
            {
                Text = "Auto-close Brave",
                ForeColor = BraveTheme.TextSecondary,
                Font = new Font("Segoe UI", 8.75f, FontStyle.Regular),
                AutoSize = true,
                Location = new Point(48, 83),
                Cursor = Cursors.Hand,
                UseMnemonic = false
            };
            lblClose.Click += (s, e) => { _swCloseBrave.Checked = !_swCloseBrave.Checked; };

            bottomPanel.Controls.Add(_swCloseBrave);
            bottomPanel.Controls.Add(lblClose);

            rail.Controls.Add(presetSection);
            rail.Controls.Add(navPanel);
            rail.Controls.Add(brandPanel);
            rail.Controls.Add(bottomPanel);
            return rail;
        }

        private Label CreateSidebarHeading(string text)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = BraveTheme.TextMuted,
                AutoSize = true,
                Margin = new Padding(2, 0, 0, 6),
                UseMnemonic = false
            };
        }

        private Label AddNavItem(FlowLayoutPanel parent, string iconGlyph, string text, int index)
        {
            var row = new Panel
            {
                Width = 210,
                Height = 38,
                BackColor = BraveTheme.SidebarBg,
                Margin = new Padding(0, 0, 0, 4),
                Cursor = Cursors.Hand
            };

            var lblIcon = new Label
            {
                Text = iconGlyph,
                Font = BraveTheme.CreateIconFont(11f),
                ForeColor = BraveTheme.TextSecondary,
                BackColor = Color.Transparent,
                Size = new Size(26, 38),
                Location = new Point(12, 0),
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand,
                UseMnemonic = false
            };

            var lblText = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9.25f, FontStyle.Regular),
                ForeColor = BraveTheme.TextSecondary,
                BackColor = Color.Transparent,
                AutoSize = false,
                Size = new Size(168, 38),
                Location = new Point(40, 0),
                TextAlign = ContentAlignment.MiddleLeft,
                Cursor = Cursors.Hand,
                UseMnemonic = false
            };

            row.Paint += (s, e) =>
            {
                if (_activeNavIndex == index)
                {
                    using (var bar = new LinearGradientBrush(
                        new Rectangle(0, 5, 4, row.Height - 10),
                        BraveTheme.BraveOrange,
                        BraveTheme.BraveMagenta,
                        LinearGradientMode.Vertical))
                    {
                        e.Graphics.FillRectangle(bar, 0, 5, 4, row.Height - 10);
                    }
                }
            };

            EventHandler onClick = (s, e) => SelectNav(index);
            EventHandler onEnter = (s, e) => { if (_activeNavIndex != index) row.BackColor = BraveTheme.Surface; };
            EventHandler onLeave = (s, e) => { if (_activeNavIndex != index) row.BackColor = BraveTheme.SidebarBg; };

            row.Click += onClick;
            lblIcon.Click += onClick;
            lblText.Click += onClick;
            row.MouseEnter += onEnter;
            lblIcon.MouseEnter += onEnter;
            lblText.MouseEnter += onEnter;
            row.MouseLeave += onLeave;
            lblIcon.MouseLeave += onLeave;
            lblText.MouseLeave += onLeave;

            row.Controls.Add(lblIcon);
            row.Controls.Add(lblText);

            _navItems.Add(row);
            _navIcons.Add(lblIcon);
            _navLabels.Add(lblText);
            parent.Controls.Add(row);
            return lblText;
        }

        private Button CreatePillButton(string text, Action onClick)
        {
            var btn = new Button
            {
                Text = text,
                AutoSize = true,
                Height = 27,
                FlatStyle = FlatStyle.Flat,
                BackColor = BraveTheme.InputBg,
                ForeColor = BraveTheme.TextPrimary,
                Font = new Font("Segoe UI", 8.25f, FontStyle.Regular),
                Margin = new Padding(0, 0, 6, 6),
                Padding = new Padding(6, 0, 6, 0),
                Cursor = Cursors.Hand,
                UseMnemonic = false
            };
            btn.FlatAppearance.BorderColor = BraveTheme.Border;
            btn.FlatAppearance.MouseOverBackColor = BraveTheme.SurfaceHover;
            btn.Click += (s, e) => onClick();
            return btn;
        }

        private Control BuildMainWorkspace()
        {
            var main = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                BackColor = BraveTheme.Obsidian,
                Padding = new Padding(20, 14, 18, 14)
            };
            main.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            main.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            main.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
            main.RowStyles.Add(new RowStyle(SizeType.Absolute, 108));

            // 1. Top Bar
            var topBar = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = BraveTheme.Obsidian
            };

            _lblPageTitle = new Label
            {
                Text = "Brave Bloat & UI",
                Font = new Font("Segoe UI", 14.5f, FontStyle.Bold),
                ForeColor = BraveTheme.TextPrimary,
                AutoSize = true,
                Location = new Point(0, 6),
                UseMnemonic = false
            };

            _lblSummary = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 8.75f, FontStyle.Bold),
                ForeColor = BraveTheme.BraveBlurple,
                BackColor = Color.FromArgb(28, 31, 52),
                Padding = new Padding(8, 4, 8, 4),
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                UseMnemonic = false
            };

            _searchWrap = new Panel
            {
                Width = 225,
                Height = 30,
                BackColor = BraveTheme.InputBg,
                Padding = new Padding(28, 5, 8, 4),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            _searchWrap.Paint += (s, e) =>
            {
                ControlPaint.DrawBorder(e.Graphics, _searchWrap.ClientRectangle, BraveTheme.Border, ButtonBorderStyle.Solid);
                using (var iconFont = BraveTheme.CreateIconFont(9.5f))
                {
                    TextRenderer.DrawText(e.Graphics, "\uE721", iconFont,
                        new Rectangle(7, 0, 18, _searchWrap.Height),
                        BraveTheme.TextMuted, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
                }
            };

            _txtSearch = new TextBox
            {
                Dock = DockStyle.Fill,
                BackColor = BraveTheme.InputBg,
                ForeColor = BraveTheme.TextPrimary,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular)
            };
            _txtSearch.TextChanged += (s, e) => FilterCurrentTweakCards();

            var lblSearchHint = new Label
            {
                Text = "Filter tweaks...",
                ForeColor = BraveTheme.TextMuted,
                BackColor = BraveTheme.InputBg,
                Font = new Font("Segoe UI", 8.75f, FontStyle.Italic),
                AutoSize = true,
                Location = new Point(28, 6),
                Cursor = Cursors.IBeam,
                UseMnemonic = false
            };
            lblSearchHint.Click += (s, e) => _txtSearch.Focus();
            _txtSearch.TextChanged += (s, e) => lblSearchHint.Visible = string.IsNullOrEmpty(_txtSearch.Text);
            _txtSearch.GotFocus += (s, e) => lblSearchHint.Visible = false;
            _txtSearch.LostFocus += (s, e) => lblSearchHint.Visible = string.IsNullOrEmpty(_txtSearch.Text);

            _searchWrap.Controls.Add(lblSearchHint);
            _searchWrap.Controls.Add(_txtSearch);
            lblSearchHint.BringToFront();

            topBar.Controls.Add(_lblPageTitle);
            topBar.Controls.Add(_searchWrap);
            topBar.Controls.Add(_lblSummary);
            topBar.Resize += (s, e) =>
            {
                _lblSummary.Location = new Point(topBar.Width - _lblSummary.Width, 9);
                _searchWrap.Location = new Point(_lblSummary.Left - _searchWrap.Width - 12, 7);
            };

            // 2. Content Host
            var contentHost = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = BraveTheme.Obsidian,
                Margin = new Padding(0, 4, 0, 10)
            };

            RegisterPage(contentHost, "Brave Bloat & UI", BuildCategoryPage(TweakCategory.BraveUI));
            RegisterPage(contentHost, "Prompts & Behavior", BuildCategoryPage(TweakCategory.PromptsBehavior));
            RegisterPage(contentHost, "Privacy & Telemetry", BuildCategoryPage(TweakCategory.PrivacyTelemetry));
            RegisterPage(contentHost, "Tab Containers", BuildContainersPage());

            // 3. Action Bar
            var actionBar = BuildActionBar();

            // 4. Custom Dark Log Box
            _logBox = new BraveDarkLogBox { Dock = DockStyle.Fill };
            _logBox.AppendLine("Ready. Toggle tweaks above and click 'Apply Selected'.");

            main.Controls.Add(topBar, 0, 0);
            main.Controls.Add(contentHost, 0, 1);
            main.Controls.Add(actionBar, 0, 2);
            main.Controls.Add(_logBox, 0, 3);

            return main;
        }

        private void RegisterPage(Panel host, string title, Control page)
        {
            page.Dock = DockStyle.Fill;
            page.Visible = false;
            _pageTitles.Add(title);
            _pages.Add(page);
            host.Controls.Add(page);
        }

        private void SelectNav(int index)
        {
            _activeNavIndex = index;
            for (int i = 0; i < _navItems.Count; i++)
            {
                bool active = i == index;
                _navItems[i].BackColor = active ? BraveTheme.Surface : BraveTheme.SidebarBg;
                _navLabels[i].ForeColor = active ? BraveTheme.TextPrimary : BraveTheme.TextSecondary;
                _navLabels[i].Font = new Font("Segoe UI", 9.25f, active ? FontStyle.Bold : FontStyle.Regular);
                _navIcons[i].ForeColor = active ? BraveTheme.BraveFlame : BraveTheme.TextSecondary;
                _navItems[i].Invalidate();
                _pages[i].Visible = active;
            }

            if (index >= 0 && index < _pageTitles.Count)
            {
                _lblPageTitle.Text = _pageTitles[index];
            }

            if (_searchWrap != null)
            {
                _searchWrap.Visible = (index < 3);
            }

            if (index >= 0 && index < _scrollPanels.Count)
            {
                _scrollPanels[index].LayoutCards();
            }
        }

        private Control BuildCategoryPage(TweakCategory category)
        {
            List<TweakItem> items = _tweaks.Where(t => t.Category == category).ToList();
            var scroller = new BraveScrollPanel();

            foreach (TweakItem item in items)
            {
                Panel card = CreateTweakCard(item);
                _tweakCards[item.Id] = card;
                scroller.ContentHost.Controls.Add(card);
                scroller.RegisterChildForWheel(card);
            }

            scroller.LayoutCards();
            _scrollPanels.Add(scroller);
            return scroller;
        }

        private Panel CreateTweakCard(TweakItem item)
        {
            var card = new Panel
            {
                Width = 720,
                Height = 64,
                BackColor = BraveTheme.Surface,
                Tag = item,
                Cursor = Cursors.Hand
            };

            string glyph = TweakIcons.GetGlyph(item.Id);
            Color iconAccent = TweakIcons.GetAccent(item.Category);
            Color iconBg = TweakIcons.GetBadgeBg(item.Category);

            card.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Card border
                using (var pen = new Pen(BraveTheme.Border, 1))
                {
                    g.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                }

                // Rounded 36x36 icon badge on the left
                var iconRect = new Rectangle(14, 14, 36, 36);
                using (GraphicsPath path = BraveTheme.RoundedRect(iconRect, 8))
                using (var bgBrush = new SolidBrush(iconBg))
                {
                    g.FillPath(bgBrush, path);
                }
                using (var iconFont = BraveTheme.CreateIconFont(13f))
                {
                    TextRenderer.DrawText(g, glyph, iconFont, iconRect, iconAccent,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
                }
            };

            var lblTitle = new Label
            {
                Text = item.Title,
                Font = new Font("Segoe UI", 9.75f, FontStyle.Bold),
                ForeColor = BraveTheme.TextPrimary,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(64, 11),
                Cursor = Cursors.Hand,
                UseMnemonic = false
            };

            var lblDesc = new Label
            {
                Text = item.Description,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = BraveTheme.TextSecondary,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(64, 34),
                Cursor = Cursors.Hand,
                UseMnemonic = false
            };

            var toggle = new BraveToggleSwitch();
            toggle.CheckedChanged += (s, e) => UpdateSelectionSummary();

            var badge = new Label
            {
                Text = "ACTIVE",
                Font = new Font("Segoe UI", 7.25f, FontStyle.Bold),
                BackColor = BraveTheme.ActiveBg,
                ForeColor = BraveTheme.ActiveFg,
                AutoSize = true,
                Padding = new Padding(7, 3, 7, 3),
                Visible = false,
                UseMnemonic = false
            };

            Action positionCardElements = () =>
            {
                toggle.Location = new Point(card.Width - toggle.Width - 16, 21);
                badge.Location = new Point(toggle.Left - badge.Width - 12, 22);
                int maxTextW = Math.Max(140, badge.Left - 76);
                lblTitle.MaximumSize = new Size(maxTextW, 22);
                lblDesc.MaximumSize = new Size(maxTextW, 20);
            };

            Action toggleAction = () => { toggle.Checked = !toggle.Checked; };
            card.Click += (s, e) => toggleAction();
            lblTitle.Click += (s, e) => toggleAction();
            lblDesc.Click += (s, e) => toggleAction();

            EventHandler onEnter = (s, e) => { card.BackColor = BraveTheme.SurfaceHover; };
            EventHandler onLeave = (s, e) => { card.BackColor = BraveTheme.Surface; };
            card.MouseEnter += onEnter;
            card.MouseLeave += onLeave;
            lblTitle.MouseEnter += onEnter;
            lblTitle.MouseLeave += onLeave;
            lblDesc.MouseEnter += onEnter;
            lblDesc.MouseLeave += onLeave;

            card.Resize += (s, e) => positionCardElements();
            badge.VisibleChanged += (s, e) => positionCardElements();

            card.Controls.Add(lblTitle);
            card.Controls.Add(lblDesc);
            card.Controls.Add(badge);
            card.Controls.Add(toggle);
            positionCardElements();

            _toggles[item.Id] = toggle;
            _statusBadges[item.Id] = badge;
            return card;
        }

        private void FilterCurrentTweakCards()
        {
            string q = (_txtSearch.Text ?? string.Empty).Trim();
            foreach (TweakItem item in _tweaks)
            {
                Panel card;
                if (!_tweakCards.TryGetValue(item.Id, out card)) continue;

                bool match = string.IsNullOrEmpty(q) ||
                    item.Title.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    item.Description.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0;
                card.Visible = match;
            }

            foreach (BraveScrollPanel sp in _scrollPanels)
            {
                sp.LayoutCards();
            }
        }

        private Control BuildContainersPage()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                BackColor = BraveTheme.Obsidian
            };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            layout.Controls.Add(new Label
            {
                Text = "Existing Brave containers are loaded automatically. Only containers that do not exist yet will be created.",
                ForeColor = BraveTheme.TextSecondary,
                Font = new Font("Segoe UI", 8.75f, FontStyle.Regular),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 10),
                UseMnemonic = false
            }, 0, 0);

            var addBar = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                WrapContents = true,
                BackColor = BraveTheme.Surface,
                Padding = new Padding(10, 8, 10, 8),
                Margin = new Padding(0, 0, 0, 10)
            };
            addBar.Paint += (s, e) =>
            {
                ControlPaint.DrawBorder(e.Graphics, addBar.ClientRectangle, BraveTheme.Border, ButtonBorderStyle.Solid);
            };

            addBar.Controls.Add(new Label { Text = "Name:", AutoSize = true, Margin = new Padding(0, 6, 4, 0), ForeColor = BraveTheme.TextSecondary });

            _txtContainerName = new TextBox
            {
                Width = 160,
                BackColor = BraveTheme.InputBg,
                ForeColor = BraveTheme.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 3, 10, 0)
            };
            addBar.Controls.Add(_txtContainerName);

            addBar.Controls.Add(new Label { Text = "Color:", AutoSize = true, Margin = new Padding(0, 6, 4, 0), ForeColor = BraveTheme.TextSecondary });

            _pnlColorSwatch = new Panel
            {
                Width = 20,
                Height = 20,
                BackColor = _selectedContainerColor,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 4, 4, 0)
            };
            addBar.Controls.Add(_pnlColorSwatch);

            _cmbContainerColor = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                BackColor = BraveTheme.InputBg,
                ForeColor = BraveTheme.TextPrimary,
                Width = 90,
                Margin = new Padding(0, 3, 4, 0)
            };
            foreach (var kvp in ContainerColorPalette)
            {
                _cmbContainerColor.Items.Add(kvp.Key);
            }
            _cmbContainerColor.SelectedIndex = 0;
            _cmbContainerColor.SelectedIndexChanged += (s, e) =>
            {
                if (_cmbContainerColor.SelectedIndex >= 0 && _cmbContainerColor.SelectedIndex < ContainerColorPalette.Length)
                {
                    _selectedContainerColor = ContainerColorPalette[_cmbContainerColor.SelectedIndex].Value;
                    _pnlColorSwatch.BackColor = _selectedContainerColor;
                }
            };
            addBar.Controls.Add(_cmbContainerColor);

            var btnCustomColor = CreatePillButton("...", () =>
            {
                using (var dlg = new ColorDialog { Color = _selectedContainerColor, FullOpen = true })
                {
                    if (dlg.ShowDialog(this) == DialogResult.OK)
                    {
                        _selectedContainerColor = dlg.Color;
                        _pnlColorSwatch.BackColor = _selectedContainerColor;
                    }
                }
            });
            btnCustomColor.Width = 32;
            btnCustomColor.Margin = new Padding(0, 2, 10, 0);
            addBar.Controls.Add(btnCustomColor);

            addBar.Controls.Add(new Label { Text = "Icon:", AutoSize = true, Margin = new Padding(0, 6, 4, 0), ForeColor = BraveTheme.TextSecondary });

            _cmbContainerIcon = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                BackColor = BraveTheme.InputBg,
                ForeColor = BraveTheme.TextPrimary,
                Width = 126,
                Margin = new Padding(0, 3, 10, 0)
            };
            _cmbContainerIcon.Items.AddRange(ContainerIconNames);
            _cmbContainerIcon.SelectedIndex = 0;
            addBar.Controls.Add(_cmbContainerIcon);

            var btnAdd = new BraveGradientButton
            {
                Text = "+ Add",
                Size = new Size(76, 28),
                Font = new Font("Segoe UI", 8.75f, FontStyle.Bold),
                Margin = new Padding(0, 1, 0, 0)
            };
            btnAdd.Click += OnAddContainerClicked;
            addBar.Controls.Add(btnAdd);

            layout.Controls.Add(addBar, 0, 1);

            _lvContainers = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = false,
                HideSelection = false,
                OwnerDraw = true,
                BackColor = BraveTheme.Surface,
                ForeColor = BraveTheme.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 0, 0, 10)
            };
            _lvContainers.Columns.Add("Status", 130);
            _lvContainers.Columns.Add("Name", 180);
            _lvContainers.Columns.Add("Color", 100);
            _lvContainers.Columns.Add("Icon", 125);
            _lvContainers.Columns.Add("ID", 220);

            _lvContainers.DrawColumnHeader += (s, e) =>
            {
                using (var bg = new SolidBrush(BraveTheme.SidebarBg))
                using (var border = new Pen(BraveTheme.Border))
                using (var f = new Font("Segoe UI", 8.5f, FontStyle.Bold))
                {
                    e.Graphics.FillRectangle(bg, e.Bounds);
                    e.Graphics.DrawLine(border, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
                    e.Graphics.DrawLine(border, e.Bounds.Right - 1, e.Bounds.Top, e.Bounds.Right - 1, e.Bounds.Bottom);
                    TextRenderer.DrawText(e.Graphics, e.Header.Text, f,
                        new Rectangle(e.Bounds.X + 8, e.Bounds.Y, e.Bounds.Width - 10, e.Bounds.Height),
                        BraveTheme.TextMuted, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
                }
            };
            _lvContainers.DrawItem += (s, e) => { };
            _lvContainers.DrawSubItem += (s, e) =>
            {
                Color rowBg = e.Item.Selected ? BraveTheme.SurfaceHover : BraveTheme.Surface;
                Color cellBg = e.ColumnIndex == 2 ? e.SubItem.BackColor : rowBg;
                Color cellFg = e.ColumnIndex == 2 ? e.SubItem.ForeColor
                    : (e.ColumnIndex == 0 ? e.Item.ForeColor : BraveTheme.TextPrimary);

                using (var bg = new SolidBrush(cellBg))
                using (var sep = new Pen(BraveTheme.Divider))
                {
                    e.Graphics.FillRectangle(bg, e.Bounds);
                    e.Graphics.DrawLine(sep, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
                }

                Font font = e.ColumnIndex == 0 ? e.Item.Font : _lvContainers.Font;
                TextRenderer.DrawText(e.Graphics, e.SubItem.Text, font,
                    new Rectangle(e.Bounds.X + 8, e.Bounds.Y, e.Bounds.Width - 12, e.Bounds.Height),
                    cellFg, TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
            };

            layout.Controls.Add(_lvContainers, 0, 2);

            var btnBar = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = BraveTheme.Obsidian
            };
            btnBar.Controls.Add(CreatePillButton("Remove Selected (New Only)", OnRemoveSelectedContainerClicked));
            btnBar.Controls.Add(CreatePillButton("+ Add Common Presets", OnAddCommonContainersClicked));
            btnBar.Controls.Add(CreatePillButton("Create Pending Containers Now", OnApplyContainersOnlyClicked));

            layout.Controls.Add(btnBar, 0, 3);
            return layout;
        }

        private Control BuildActionBar()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = BraveTheme.Obsidian,
                Padding = new Padding(0, 4, 0, 4)
            };

            var btnApply = new BraveGradientButton
            {
                Text = "Apply Selected",
                Size = new Size(150, 38),
                Location = new Point(0, 4)
            };
            btnApply.Click += OnApplyClicked;

            var btnRevert = CreateSecondaryActionButton("Revert All", 160, 108, OnRevertClicked);
            var btnExport = CreateSecondaryActionButton("Export .reg...", 276, 118, OnExportClicked);
            var btnRefresh = CreateSecondaryActionButton("Refresh", 402, 96, (s, e) =>
            {
                RefreshStatusBadges();
                LoadContainersFromBrave(keepPending: true);
                AppendLog("Refreshed status and containers from Registry & Brave Preferences.");
            });

            panel.Controls.Add(btnApply);
            panel.Controls.Add(btnRevert);
            panel.Controls.Add(btnExport);
            panel.Controls.Add(btnRefresh);
            return panel;
        }

        private Button CreateSecondaryActionButton(string text, int x, int width, EventHandler onClick)
        {
            var btn = new Button
            {
                Text = text,
                Width = width,
                Height = 38,
                Location = new Point(x, 4),
                FlatStyle = FlatStyle.Flat,
                BackColor = BraveTheme.Surface,
                ForeColor = BraveTheme.TextPrimary,
                Font = new Font("Segoe UI", 9.25f, FontStyle.Regular),
                Cursor = Cursors.Hand,
                UseMnemonic = false
            };
            btn.FlatAppearance.BorderColor = BraveTheme.Border;
            btn.FlatAppearance.MouseOverBackColor = BraveTheme.SurfaceHover;
            btn.Click += onClick;
            return btn;
        }

        private void ApplyPreset(string preset)
        {
            foreach (TweakItem item in _tweaks)
            {
                bool check = false;
                switch (preset)
                {
                    case "recommended": check = item.DefaultChecked; break;
                    case "ui": check = item.Category == TweakCategory.BraveUI && item.Id != "disable_tor"; break;
                    case "prompts": check = item.Category == TweakCategory.PromptsBehavior; break;
                    case "privacy": check = item.Category == TweakCategory.PrivacyTelemetry; break;
                    case "all": check = true; break;
                    case "none": check = false; break;
                }

                BraveToggleSwitch sw;
                if (_toggles.TryGetValue(item.Id, out sw))
                {
                    sw.Checked = check;
                }
            }
            UpdateSelectionSummary();
        }

        private HashSet<string> GetSelectedIds()
        {
            var selected = new HashSet<string>();
            foreach (var kvp in _toggles)
            {
                if (kvp.Value.Checked) selected.Add(kvp.Key);
            }
            return selected;
        }

        private void UpdateSelectionSummary()
        {
            int count = _toggles.Values.Count(sw => sw.Checked);
            _lblSummary.Text = string.Format("{0} / {1} selected", count, _tweaks.Count);
            if (_lblSummary.Parent != null)
            {
                _lblSummary.Location = new Point(_lblSummary.Parent.Width - _lblSummary.Width, 9);
                if (_searchWrap != null)
                {
                    _searchWrap.Location = new Point(_lblSummary.Left - _searchWrap.Width - 12, 7);
                }
            }
        }

        private void RefreshStatusBadges()
        {
            bool useMachine = _scopeControl != null && _scopeControl.IsMachine;
            Dictionary<string, bool> states = TweakEngine.GetAppliedStates(_tweaks, useMachine);
            foreach (var kvp in states)
            {
                Label badge;
                BraveToggleSwitch toggle;
                if (_statusBadges.TryGetValue(kvp.Key, out badge))
                {
                    badge.Visible = kvp.Value;
                    if (_toggles.TryGetValue(kvp.Key, out toggle))
                    {
                        badge.Location = new Point(toggle.Left - badge.Width - 12, 22);
                    }
                }
            }
        }

        private void LoadContainersFromBrave(bool keepPending)
        {
            List<ContainerItem> pending = keepPending
                ? _containers.Where(c => !c.IsExisting).ToList()
                : new List<ContainerItem>();

            _containers.Clear();
            List<ContainerItem> existing = TweakEngine.LoadExistingContainers();
            var existingNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (ContainerItem c in existing)
            {
                _containers.Add(c);
                existingNames.Add(c.Name);
            }

            foreach (ContainerItem p in pending)
            {
                if (!existingNames.Contains(p.Name))
                {
                    _containers.Add(p);
                    existingNames.Add(p.Name);
                }
            }

            RenderContainersListView();
        }

        private void RenderContainersListView()
        {
            if (_lvContainers == null) return;

            _lvContainers.BeginUpdate();
            _lvContainers.Items.Clear();

            foreach (ContainerItem item in _containers)
            {
                Color c = Color.FromArgb(item.BackgroundColor);
                string hex = string.Format("#{0:X2}{1:X2}{2:X2}", c.R, c.G, c.B);
                string iconLabel = (item.Icon >= 0 && item.Icon < ContainerIconNames.Length)
                    ? ContainerIconNames[item.Icon]
                    : "Icon #" + item.Icon;

                var lvi = new ListViewItem(item.IsExisting ? "Existing in Brave" : "New (to create)")
                {
                    Tag = item,
                    UseItemStyleForSubItems = false,
                    ForeColor = item.IsExisting ? BraveTheme.ActiveFg : BraveTheme.BraveFlame,
                    Font = new Font("Segoe UI", 8.75f, FontStyle.Bold)
                };

                lvi.SubItems.Add(item.Name);

                var colorSub = lvi.SubItems.Add(hex);
                colorSub.BackColor = c;
                int brightness = (c.R * 299 + c.G * 587 + c.B * 114) / 1000;
                colorSub.ForeColor = brightness > 140 ? Color.Black : Color.White;

                lvi.SubItems.Add(iconLabel);
                lvi.SubItems.Add(string.IsNullOrEmpty(item.Id) ? "(generated on apply)" : item.Id);

                _lvContainers.Items.Add(lvi);
            }

            _lvContainers.EndUpdate();

            if (_containersNavLabel != null)
            {
                int pendingCount = _containers.Count(x => !x.IsExisting);
                _containersNavLabel.Text = pendingCount > 0
                    ? string.Format("Containers ({0}, +{1})", _containers.Count, pendingCount)
                    : string.Format("Containers ({0})", _containers.Count);
            }
        }

        private void OnAddContainerClicked(object sender, EventArgs e)
        {
            string name = (_txtContainerName.Text ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Please enter a container name.", "BraveTweaker", MessageBoxButtons.OK, MessageBoxIcon.Information);
                _txtContainerName.Focus();
                return;
            }

            if (_containers.Any(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show(
                    string.Format("Container \"{0}\" already exists and will not be duplicated.", name),
                    "Already Exists",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            _containers.Add(new ContainerItem
            {
                Name = name,
                Icon = Math.Max(0, _cmbContainerIcon.SelectedIndex),
                BackgroundColor = _selectedContainerColor.ToArgb(),
                IsExisting = false
            });

            _txtContainerName.Clear();
            _txtContainerName.Focus();
            RenderContainersListView();
        }

        private void OnRemoveSelectedContainerClicked()
        {
            if (_lvContainers.SelectedItems.Count == 0) return;

            int removed = 0;
            bool triedExisting = false;
            foreach (ListViewItem lvi in _lvContainers.SelectedItems)
            {
                var item = lvi.Tag as ContainerItem;
                if (item == null) continue;

                if (item.IsExisting) triedExisting = true;
                else { _containers.Remove(item); removed++; }
            }

            if (removed > 0) RenderContainersListView();

            if (triedExisting)
            {
                MessageBox.Show(
                    "Existing Brave containers are protected and cannot be deleted here. Manage them inside Brave under brave://settings/braveContent.",
                    "Existing Container Protected",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        private void OnAddCommonContainersClicked()
        {
            var presets = new[]
            {
                new ContainerItem { Name = "Personal", Icon = 0, BackgroundColor = Color.FromArgb(unchecked((int)0xFF488CFF)).ToArgb() },
                new ContainerItem { Name = "Work", Icon = 1, BackgroundColor = Color.FromArgb(unchecked((int)0xFFFF7A00)).ToArgb() },
                new ContainerItem { Name = "Banking", Icon = 7, BackgroundColor = Color.FromArgb(unchecked((int)0xFF2FAC5F)).ToArgb() },
                new ContainerItem { Name = "Shopping", Icon = 2, BackgroundColor = Color.FromArgb(unchecked((int)0xFFF5B800)).ToArgb() },
                new ContainerItem { Name = "Social", Icon = 3, BackgroundColor = Color.FromArgb(unchecked((int)0xFF9973F8)).ToArgb() }
            };

            int added = 0;
            int skipped = 0;
            foreach (ContainerItem p in presets)
            {
                if (_containers.Any(c => string.Equals(c.Name, p.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    skipped++;
                }
                else
                {
                    p.IsExisting = false;
                    _containers.Add(p);
                    added++;
                }
            }

            RenderContainersListView();
            AppendLog(string.Format("Container presets: queued {0} new, skipped {1} already present.", added, skipped));
        }

        private void OnApplyContainersOnlyClicked()
        {
            List<ContainerItem> pending = _containers.Where(c => !c.IsExisting).ToList();
            if (pending.Count == 0)
            {
                MessageBox.Show("No new containers pending to create.", "BraveTweaker", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            bool closeBrave = _swCloseBrave.Checked;
            if (!closeBrave && TweakEngine.GetRunningBraveProcessCount() > 0)
            {
                DialogResult res = MessageBox.Show(
                    "Brave is currently running. Close Brave now so new containers can be saved safely?",
                    "Brave is Running",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                if (res == DialogResult.Yes) closeBrave = true;
            }

            _logBox.Clear();
            bool ok = TweakEngine.ApplyContainersOnly(pending, closeBrave, AppendLog);
            LoadContainersFromBrave(keepPending: !ok);

            if (ok)
            {
                MessageBox.Show("New containers created! Reopen Brave to use them.", "BraveTweaker", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void OnApplyClicked(object sender, EventArgs e)
        {
            _logBox.Clear();
            HashSet<string> selected = GetSelectedIds();
            bool useMachine = _scopeControl.IsMachine;
            bool closeBrave = _swCloseBrave.Checked;

            if (!closeBrave && TweakEngine.GetRunningBraveProcessCount() > 0)
            {
                DialogResult res = MessageBox.Show(
                    "Brave is currently running. Profile Preferences edits will be overwritten unless Brave is closed first.\n\nClose Brave now?",
                    "Brave is Running",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                if (res == DialogResult.Yes) closeBrave = true;
            }

            List<ContainerItem> pendingContainers = _containers.Where(c => !c.IsExisting).ToList();
            bool ok = TweakEngine.ApplySelected(_tweaks, selected, useMachine, closeBrave, AppendLog, pendingContainers);
            RefreshStatusBadges();
            LoadContainersFromBrave(keepPending: !ok);

            if (ok)
            {
                MessageBox.Show(
                    "Tweaks applied successfully!\n\nRestart Brave (or open brave://policy and click 'Reload policies') to verify.",
                    "BraveTweaker",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        private void OnRevertClicked(object sender, EventArgs e)
        {
            DialogResult confirm = MessageBox.Show(
                "This will remove all BraveTweaker registry policies and restore your backed-up Brave Preferences file(s).\n\nContinue?",
                "Confirm Revert",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            _logBox.Clear();
            TweakEngine.RevertAll(_tweaks, _swCloseBrave.Checked, AppendLog);
            RefreshStatusBadges();
            LoadContainersFromBrave(keepPending: false);
        }

        private void OnExportClicked(object sender, EventArgs e)
        {
            HashSet<string> selected = GetSelectedIds();
            bool useMachine = _scopeControl.IsMachine;

            using (var dlg = new SaveFileDialog())
            {
                dlg.Title = "Export Brave Policies to .reg File";
                dlg.Filter = "Registry Files (*.reg)|*.reg|All Files (*.*)|*.*";
                dlg.FileName = "BraveTweaks.reg";

                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        TweakEngine.ExportRegFile(_tweaks, selected, useMachine, dlg.FileName);
                        AppendLog("[OK] Exported registry policies to: " + dlg.FileName);
                        AppendLog("[INFO] Note: UI-only Preferences tweaks & Containers require running BraveTweaker.exe directly.");
                    }
                    catch (Exception ex)
                    {
                        AppendLog("[ERROR] Export failed: " + ex.Message);
                    }
                }
            }
        }

        private void AppendLog(string message)
        {
            if (_logBox != null)
            {
                _logBox.AppendLine(message);
            }
        }
    }
}
