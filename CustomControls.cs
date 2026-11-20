using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;

namespace BraveTweaker
{
    internal static class BraveTheme
    {
        // Official Brave Night / Nala Dark Color Palette
        public static readonly Color Obsidian = Color.FromArgb(13, 14, 18);         // #0D0E12 main canvas
        public static readonly Color SidebarBg = Color.FromArgb(20, 21, 28);        // #14151C left rail
        public static readonly Color TitleBarBg = Color.FromArgb(16, 17, 23);       // #101117 custom title bar
        public static readonly Color Surface = Color.FromArgb(26, 28, 36);          // #1A1C24 cards
        public static readonly Color SurfaceHover = Color.FromArgb(34, 37, 49);     // #222531 card hover
        public static readonly Color InputBg = Color.FromArgb(36, 39, 52);          // #242734 inputs / pills
        public static readonly Color Border = Color.FromArgb(46, 49, 66);           // #2E3142 subtle border
        public static readonly Color Divider = Color.FromArgb(34, 36, 48);          // #222430 divider

        public static readonly Color BraveOrange = Color.FromArgb(255, 85, 0);      // #FF5500 Lion Orange
        public static readonly Color BraveFlame = Color.FromArgb(251, 84, 43);      // #FB542B Brave Flame
        public static readonly Color BraveMagenta = Color.FromArgb(230, 36, 98);    // #E62462 Brave Magenta
        public static readonly Color BraveBlurple = Color.FromArgb(115, 122, 222);  // #737ADE Brave Blurple

        public static readonly Color TextPrimary = Color.FromArgb(244, 245, 248);   // #F4F5F8
        public static readonly Color TextSecondary = Color.FromArgb(154, 158, 181); // #9A9EB5
        public static readonly Color TextMuted = Color.FromArgb(111, 115, 138);     // #6F738A

        public static readonly Color ActiveBg = Color.FromArgb(19, 49, 35);         // #133123
        public static readonly Color ActiveFg = Color.FromArgb(74, 222, 128);       // #4ADE80

        public static readonly Color TerminalBg = Color.FromArgb(10, 11, 15);       // #0A0B0F
        public static readonly Color TerminalFg = Color.FromArgb(204, 212, 226);

        private static string _iconFontFamily;

        public static Font CreateIconFont(float size)
        {
            if (_iconFontFamily == null)
            {
                using (var installed = new InstalledFontCollection())
                {
                    if (installed.Families.Any(f => string.Equals(f.Name, "Segoe Fluent Icons", StringComparison.OrdinalIgnoreCase)))
                    {
                        _iconFontFamily = "Segoe Fluent Icons";
                    }
                    else if (installed.Families.Any(f => string.Equals(f.Name, "Segoe MDL2 Assets", StringComparison.OrdinalIgnoreCase)))
                    {
                        _iconFontFamily = "Segoe MDL2 Assets";
                    }
                    else
                    {
                        _iconFontFamily = "Segoe UI Symbol";
                    }
                }
            }
            return new Font(_iconFontFamily, size, FontStyle.Regular);
        }

        public static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            if (d <= 0)
            {
                path.AddRectangle(bounds);
                return path;
            }
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    internal static class TweakIcons
    {
        // Segoe MDL2 Assets / Segoe Fluent Icons glyphs (built into Windows 10 & 11)
        public const string NavBloat = "\uE71D";      // AllApps / UI
        public const string NavPrompts = "\uEA8F";    // Ringer / Notifications
        public const string NavPrivacy = "\uE72E";    // Lock / Shield
        public const string NavContainers = "\uE74C"; // Layers / Cubes

        public static string GetGlyph(string tweakId)
        {
            switch (tweakId)
            {
                case "brave_rewards": return "\uE734";               // Star
                case "hide_sidebar_btn": return "\uE8A0";            // Side panel
                case "brave_wallet": return "\uE8C7";                // Payment / Wallet
                case "brave_vpn": return "\uE705";                   // VPN
                case "disable_ai": return "\uE945";                  // Sparkle / AI
                case "brave_news": return "\uF57E";                  // News
                case "brave_talk": return "\uE714";                  // Video
                case "ntp_sponsored": return "\uEB9F";               // Picture
                case "disable_tor": return "\uE727";                 // Private / Incognito
                case "wayback_machine": return "\uE81C";             // History
                case "full_urls": return "\uE71B";                   // Link
                case "show_downloads_button": return "\uE896";       // Download
                case "hide_tab_groups_bookmarks_bar": return "\uE728"; // Bookmark
                case "ultra_dark_theme": return "\uE708";            // Moon / Quiet hours
                case "mv2_ublock": return "\uEA86";                  // Puzzle / Extension

                case "password_offers": return "\uE8D7";             // Key / Permissions
                case "credit_card_offers": return "\uE8C7";          // Credit Card
                case "address_offers": return "\uE80F";              // Home / Address
                case "translate_offers": return "\uE774";            // Globe
                case "notification_offers": return "\uEA8F";         // Bell
                case "geolocation_offers": return "\uE707";          // Map Pin
                case "default_browser_prompt": return "\uE774";      // Browser Globe
                case "background_mode": return "\uE7E8";             // Power
                case "shopping_list": return "\uE7BF";               // Shopping Cart
                case "close_all_confirm": return "\uE8BB";           // Close / Dismiss
                case "start_new_tab": return "\uE710";               // Add / New Tab
                case "fullscreen_reminder": return "\uE740";         // FullScreen
                case "energy_saver_off": return "\uE83E";            // Battery / Saver
                case "search_google": return "\uE721";               // Search
                case "disable_auto_update": return "\uE777";         // Sync / Update

                case "brave_telemetry": return "\uE9D9";             // Bar Chart
                case "chromium_telemetry": return "\uE9D2";          // Diagnostics
                case "webrtc_leak": return "\uE968";                 // Network / Radio
                case "network_prediction": return "\uE701";          // Send / Preload
                case "shields_aggressive": return "\uEA18";          // Defender / Shield
                case "diagnostic_reports": return "\uE9F5";          // Bug / Report
                case "https_only": return "\uE72E";                  // Lock
                default: return "\uE713";                            // Settings gear
            }
        }

        public static Color GetAccent(TweakCategory category)
        {
            switch (category)
            {
                case TweakCategory.BraveUI: return BraveTheme.BraveFlame;
                case TweakCategory.PromptsBehavior: return Color.FromArgb(245, 158, 11); // Warm Amber
                case TweakCategory.PrivacyTelemetry: return BraveTheme.BraveBlurple;
                default: return BraveTheme.BraveFlame;
            }
        }

        public static Color GetBadgeBg(TweakCategory category)
        {
            switch (category)
            {
                case TweakCategory.BraveUI: return Color.FromArgb(46, 26, 26);
                case TweakCategory.PromptsBehavior: return Color.FromArgb(46, 36, 20);
                case TweakCategory.PrivacyTelemetry: return Color.FromArgb(28, 31, 54);
                default: return BraveTheme.InputBg;
            }
        }
    }

    internal sealed class BraveToggleSwitch : Control
    {
        private bool _checked;

        public event EventHandler CheckedChanged;

        public bool Checked
        {
            get { return _checked; }
            set
            {
                if (_checked != value)
                {
                    _checked = value;
                    Invalidate();
                    if (CheckedChanged != null) CheckedChanged(this, EventArgs.Empty);
                }
            }
        }

        public BraveToggleSwitch()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Size = new Size(40, 22);
            Cursor = Cursors.Hand;
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            Checked = !Checked;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new Rectangle(1, 1, Width - 3, Height - 3);
            using (GraphicsPath track = BraveTheme.RoundedRect(rect, rect.Height / 2))
            {
                if (_checked)
                {
                    using (var grad = new LinearGradientBrush(rect, BraveTheme.BraveOrange, BraveTheme.BraveMagenta, LinearGradientMode.Horizontal))
                    {
                        g.FillPath(grad, track);
                    }
                }
                else
                {
                    using (var offBrush = new SolidBrush(Color.FromArgb(48, 51, 68)))
                    using (var pen = new Pen(BraveTheme.Border, 1))
                    {
                        g.FillPath(offBrush, track);
                        g.DrawPath(pen, track);
                    }
                }
            }

            int knobSize = Height - 8;
            int knobX = _checked ? (Width - knobSize - 4) : 4;
            int knobY = 4;
            using (var knobBrush = new SolidBrush(Color.White))
            {
                g.FillEllipse(knobBrush, knobX, knobY, knobSize, knobSize);
            }
        }
    }

    internal sealed class BraveGradientButton : Control
    {
        private bool _hovered;
        private bool _pressed;

        public BraveGradientButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Cursor = Cursors.Hand;
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            ForeColor = Color.White;
            Size = new Size(148, 38);
        }

        protected override void OnMouseEnter(EventArgs e) { _hovered = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hovered = false; _pressed = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { _pressed = true; Invalidate(); base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e) { _pressed = false; Invalidate(); base.OnMouseUp(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            Color c1 = _pressed ? Color.FromArgb(204, 62, 0) : (_hovered ? Color.FromArgb(255, 102, 26) : BraveTheme.BraveOrange);
            Color c2 = _pressed ? Color.FromArgb(180, 26, 76) : (_hovered ? Color.FromArgb(242, 52, 114) : BraveTheme.BraveMagenta);

            using (GraphicsPath path = BraveTheme.RoundedRect(rect, 7))
            using (var grad = new LinearGradientBrush(rect, c1, c2, LinearGradientMode.Horizontal))
            {
                g.FillPath(grad, path);
            }

            TextRenderer.DrawText(g, Text, Font, rect, ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
        }
    }

    internal sealed class BraveSegmentedScope : Control
    {
        private bool _isMachine;
        private readonly bool _machineEnabled;

        public event EventHandler ScopeChanged;

        public bool IsMachine
        {
            get { return _isMachine; }
            set
            {
                if (value && !_machineEnabled) return;
                if (_isMachine != value)
                {
                    _isMachine = value;
                    Invalidate();
                    if (ScopeChanged != null) ScopeChanged(this, EventArgs.Empty);
                }
            }
        }

        public BraveSegmentedScope(bool isAdmin)
        {
            _machineEnabled = isAdmin;
            _isMachine = isAdmin;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Size = new Size(206, 30);
            Cursor = Cursors.Hand;
            Font = new Font("Segoe UI", 8.25f, FontStyle.Bold);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.X < Width / 2)
            {
                if (_machineEnabled) IsMachine = true;
            }
            else
            {
                IsMachine = false;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var outer = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath path = BraveTheme.RoundedRect(outer, 6))
            using (var bg = new SolidBrush(BraveTheme.InputBg))
            using (var border = new Pen(BraveTheme.Border, 1))
            {
                g.FillPath(bg, path);
                g.DrawPath(border, path);
            }

            int half = Width / 2;
            var activeRect = _isMachine
                ? new Rectangle(2, 2, half - 3, Height - 5)
                : new Rectangle(half + 1, 2, Width - half - 4, Height - 5);

            using (GraphicsPath activePath = BraveTheme.RoundedRect(activeRect, 5))
            using (var activeBg = new SolidBrush(Color.FromArgb(46, 26, 26)))
            using (var activeBorder = new Pen(BraveTheme.BraveFlame, 1))
            {
                g.FillPath(activeBg, activePath);
                g.DrawPath(activeBorder, activePath);
            }

            var leftRect = new Rectangle(0, 0, half, Height);
            var rightRect = new Rectangle(half, 0, Width - half, Height);

            Color leftColor = !_machineEnabled ? BraveTheme.TextMuted : (_isMachine ? BraveTheme.TextPrimary : BraveTheme.TextSecondary);
            Color rightColor = !_isMachine ? BraveTheme.TextPrimary : BraveTheme.TextSecondary;

            TextRenderer.DrawText(g, "HKLM (All)", Font, leftRect, leftColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
            TextRenderer.DrawText(g, "HKCU (User)", Font, rightRect, rightColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
        }
    }

    internal sealed class BraveWindowButton : Control
    {
        private bool _hovered;
        private readonly bool _isClose;
        private readonly string _glyph;

        public BraveWindowButton(string glyph, bool isClose)
        {
            _glyph = glyph;
            _isClose = isClose;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Size = new Size(44, 34);
            Cursor = Cursors.Hand;
        }

        protected override void OnMouseEnter(EventArgs e) { _hovered = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hovered = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Color bg = _hovered
                ? (_isClose ? Color.FromArgb(220, 42, 68) : BraveTheme.SurfaceHover)
                : BraveTheme.TitleBarBg;

            using (var brush = new SolidBrush(bg))
            {
                g.FillRectangle(brush, ClientRectangle);
            }

            Color fg = _hovered ? Color.White : BraveTheme.TextSecondary;
            using (var font = BraveTheme.CreateIconFont(9.5f))
            {
                TextRenderer.DrawText(g, _glyph, font, ClientRectangle, fg,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
            }
        }
    }

    /// <summary>
    /// Custom dark scrollable container with a thin dark pill scrollbar (no native white Win32 scrollbar).
    /// </summary>
    internal sealed class BraveScrollPanel : Panel
    {
        private readonly Panel _content;
        private readonly Panel _scrollTrack;
        private int _scrollY;
        private int _contentHeight;
        private bool _draggingThumb;
        private int _dragStartMouseY;
        private int _dragStartScrollY;
        private bool _thumbHovered;
        private const int TrackWidth = 10;

        public Panel ContentHost { get { return _content; } }

        public BraveScrollPanel()
        {
            DoubleBuffered = true;
            BackColor = BraveTheme.Obsidian;

            _content = new Panel
            {
                Location = new Point(0, 0),
                BackColor = BraveTheme.Obsidian
            };

            _scrollTrack = new Panel
            {
                Dock = DockStyle.Right,
                Width = TrackWidth,
                BackColor = BraveTheme.Obsidian,
                Cursor = Cursors.Hand
            };
            _scrollTrack.Paint += OnPaintScrollTrack;
            _scrollTrack.MouseDown += OnTrackMouseDown;
            _scrollTrack.MouseMove += OnTrackMouseMove;
            _scrollTrack.MouseUp += (s, e) => { _draggingThumb = false; };
            _scrollTrack.MouseEnter += (s, e) => { _thumbHovered = true; _scrollTrack.Invalidate(); };
            _scrollTrack.MouseLeave += (s, e) => { _thumbHovered = false; _scrollTrack.Invalidate(); };

            Controls.Add(_content);
            Controls.Add(_scrollTrack);

            MouseWheel += OnWheel;
            _content.MouseWheel += OnWheel;
            Resize += (s, e) => LayoutCards();
            VisibleChanged += (s, e) => { if (Visible) LayoutCards(); };
            HandleCreated += (s, e) => LayoutCards();
        }

        public void RegisterChildForWheel(Control ctrl)
        {
            ctrl.MouseWheel += OnWheel;
            foreach (Control child in ctrl.Controls)
            {
                RegisterChildForWheel(child);
            }
        }

        public void LayoutCards()
        {
            if (ClientSize.Width < 100) return;
            int usableW = ClientSize.Width - TrackWidth - 8;
            int y = 2;
            foreach (Control c in _content.Controls)
            {
                if (!c.Visible) continue;
                c.Location = new Point(0, y);
                c.Width = usableW;
                y += c.Height + 8;
            }
            _contentHeight = y;
            _content.Size = new Size(usableW, Math.Max(ClientSize.Height, _contentHeight));
            ClampScroll();
            _content.Top = -_scrollY;
            _scrollTrack.Invalidate();
        }

        private void OnWheel(object sender, MouseEventArgs e)
        {
            int delta = -e.Delta / 2;
            _scrollY += delta;
            ClampScroll();
            _content.Top = -_scrollY;
            _scrollTrack.Invalidate();
        }

        private void ClampScroll()
        {
            int maxScroll = Math.Max(0, _contentHeight - ClientSize.Height);
            if (_scrollY < 0) _scrollY = 0;
            if (_scrollY > maxScroll) _scrollY = maxScroll;
        }

        private Rectangle GetThumbRect()
        {
            int trackH = _scrollTrack.Height;
            if (_contentHeight <= ClientSize.Height || trackH <= 0)
            {
                return Rectangle.Empty;
            }

            int maxScroll = _contentHeight - ClientSize.Height;
            int thumbH = Math.Max(32, (int)((float)ClientSize.Height / _contentHeight * trackH));
            int thumbY = (int)((float)_scrollY / maxScroll * (trackH - thumbH));
            return new Rectangle(2, thumbY, TrackWidth - 4, thumbH);
        }

        private void OnPaintScrollTrack(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle thumb = GetThumbRect();
            if (thumb.IsEmpty) return;

            Color thumbColor = (_draggingThumb || _thumbHovered)
                ? Color.FromArgb(94, 100, 128)
                : Color.FromArgb(52, 56, 74);

            using (GraphicsPath path = BraveTheme.RoundedRect(thumb, 3))
            using (var brush = new SolidBrush(thumbColor))
            {
                e.Graphics.FillPath(brush, path);
            }
        }

        private void OnTrackMouseDown(object sender, MouseEventArgs e)
        {
            Rectangle thumb = GetThumbRect();
            if (thumb.IsEmpty) return;

            if (thumb.Contains(e.Location))
            {
                _draggingThumb = true;
                _dragStartMouseY = e.Y;
                _dragStartScrollY = _scrollY;
            }
            else
            {
                int maxScroll = Math.Max(1, _contentHeight - ClientSize.Height);
                float ratio = (float)e.Y / Math.Max(1, _scrollTrack.Height);
                _scrollY = (int)(ratio * maxScroll);
                ClampScroll();
                _content.Top = -_scrollY;
                _scrollTrack.Invalidate();
            }
        }

        private void OnTrackMouseMove(object sender, MouseEventArgs e)
        {
            if (!_draggingThumb) return;
            Rectangle thumb = GetThumbRect();
            int scrollableTrack = Math.Max(1, _scrollTrack.Height - thumb.Height);
            int maxScroll = Math.Max(1, _contentHeight - ClientSize.Height);
            int dy = e.Y - _dragStartMouseY;
            _scrollY = _dragStartScrollY + (int)((float)dy / scrollableTrack * maxScroll);
            ClampScroll();
            _content.Top = -_scrollY;
            _scrollTrack.Invalidate();
        }
    }

    /// <summary>
    /// Custom dark monospace log viewer with a sleek dark scrollbar (no native white scrollbar).
    /// </summary>
    internal sealed class BraveDarkLogBox : Control
    {
        private readonly List<string> _lines = new List<string>();
        private int _scrollOffset;
        private const int LineHeight = 16;

        public BraveDarkLogBox()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = BraveTheme.TerminalBg;
            ForeColor = BraveTheme.TerminalFg;
            Font = new Font("Consolas", 8.5f, FontStyle.Regular);
        }

        public void Clear()
        {
            _lines.Clear();
            _scrollOffset = 0;
            Invalidate();
        }

        public void AppendLine(string line)
        {
            if (line == null) return;
            foreach (string part in line.Replace("\r\n", "\n").Split('\n'))
            {
                _lines.Add(part);
            }
            int visibleLines = Math.Max(1, (Height - 12) / LineHeight);
            _scrollOffset = Math.Max(0, _lines.Count - visibleLines);
            Invalidate();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            int visibleLines = Math.Max(1, (Height - 12) / LineHeight);
            int maxOffset = Math.Max(0, _lines.Count - visibleLines);
            _scrollOffset -= Math.Sign(e.Delta) * 2;
            if (_scrollOffset < 0) _scrollOffset = 0;
            if (_scrollOffset > maxOffset) _scrollOffset = maxOffset;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (var bg = new SolidBrush(BraveTheme.TerminalBg))
            using (var border = new Pen(BraveTheme.Border, 1))
            {
                g.FillRectangle(bg, ClientRectangle);
                g.DrawRectangle(border, 0, 0, Width - 1, Height - 1);
            }

            int visibleLines = Math.Max(1, (Height - 12) / LineHeight);
            int y = 6;
            for (int i = _scrollOffset; i < _lines.Count && i < _scrollOffset + visibleLines; i++)
            {
                string text = _lines[i];
                Color color = BraveTheme.TerminalFg;
                if (text.StartsWith("[OK]")) color = BraveTheme.ActiveFg;
                else if (text.StartsWith("[ERROR]")) color = Color.FromArgb(248, 113, 113);
                else if (text.StartsWith("[WARN]")) color = Color.FromArgb(251, 191, 36);
                else if (text.StartsWith("===")) color = BraveTheme.BraveBlurple;

                TextRenderer.DrawText(g, text, Font,
                    new Rectangle(10, y, Width - 22, LineHeight),
                    color, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
                y += LineHeight;
            }

            // Custom thin dark scrollbar thumb if needed
            if (_lines.Count > visibleLines)
            {
                int trackH = Height - 10;
                int maxOffset = _lines.Count - visibleLines;
                int thumbH = Math.Max(20, (int)((float)visibleLines / _lines.Count * trackH));
                int thumbY = 5 + (int)((float)_scrollOffset / maxOffset * (trackH - thumbH));
                var thumb = new Rectangle(Width - 8, thumbY, 4, thumbH);
                using (GraphicsPath path = BraveTheme.RoundedRect(thumb, 2))
                using (var brush = new SolidBrush(Color.FromArgb(60, 64, 84)))
                {
                    g.FillPath(brush, path);
                }
            }
        }
    }
}
