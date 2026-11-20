using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace BraveTweaker
{
    internal sealed class SplashForm : Form
    {
        private readonly Image _art;
        private readonly Timer _timer;
        private int _tick;
        private const int TotalTicks = 56;

        public SplashForm()
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            ShowInTaskbar = false;
            TopMost = true;
            DoubleBuffered = true;
            BackColor = Color.FromArgb(13, 14, 18);
            Size = new Size(460, 330);
            Opacity = 0.0;

            _art = LoadEmbeddedLogo();

            try
            {
                Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            }
            catch { }

            _timer = new Timer { Interval = 20 };
            _timer.Tick += OnTimerTick;
            _timer.Start();
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            _tick++;
            if (_tick <= 10)
            {
                Opacity = Math.Min(1.0, _tick / 10.0);
            }
            else if (_tick > TotalTicks - 10 && _tick <= TotalTicks)
            {
                Opacity = Math.Max(0.0, (TotalTicks - _tick) / 10.0);
            }
            else if (_tick > TotalTicks)
            {
                _timer.Stop();
                Close();
                return;
            }
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            // Deep Brave Obsidian gradient
            using (var bg = new LinearGradientBrush(
                ClientRectangle,
                Color.FromArgb(20, 21, 28),
                Color.FromArgb(11, 12, 16),
                LinearGradientMode.Vertical))
            {
                g.FillRectangle(bg, ClientRectangle);
            }

            // Outer border
            using (var pen = new Pen(Color.FromArgb(44, 47, 63), 1))
            {
                g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }

            // Top Brave Lion Orange -> Magenta gradient bar
            using (var bar = new LinearGradientBrush(
                new Rectangle(0, 0, Width, 4),
                Color.FromArgb(255, 85, 0),
                Color.FromArgb(230, 36, 98),
                LinearGradientMode.Horizontal))
            {
                g.FillRectangle(bar, 0, 0, Width, 4);
            }

            // Draw splash art
            if (_art != null)
            {
                int maxW = 185;
                int maxH = 185;
                float scale = Math.Min((float)maxW / _art.Width, (float)maxH / _art.Height);
                int drawW = Math.Max(1, (int)(_art.Width * scale));
                int drawH = Math.Max(1, (int)(_art.Height * scale));
                int x = (Width - drawW) / 2;
                int y = 28 + (maxH - drawH) / 2;
                g.DrawImage(_art, x, y, drawW, drawH);
            }

            // Title
            using (var titleFont = new Font("Segoe UI", 16f, FontStyle.Bold))
            using (var titleBrush = new SolidBrush(Color.FromArgb(244, 245, 248)))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center })
            {
                g.DrawString("BraveTweaker", titleFont, titleBrush, new RectangleF(0, 224, Width, 34), sf);
            }

            // Subtitle
            using (var subFont = new Font("Segoe UI", 9f, FontStyle.Regular))
            using (var subBrush = new SolidBrush(Color.FromArgb(154, 158, 181)))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center })
            {
                g.DrawString("Loading Brave policies, preferences & containers...", subFont, subBrush, new RectangleF(0, 258, Width, 22), sf);
            }

            // Progress bar track
            int barX = 70;
            int barY = 294;
            int barW = Width - 140;
            int barH = 5;
            using (var trackBrush = new SolidBrush(Color.FromArgb(37, 39, 52)))
            {
                g.FillRectangle(trackBrush, barX, barY, barW, barH);
            }

            float progress = Math.Min(1f, (float)_tick / (TotalTicks - 10));
            int fillW = Math.Max(4, (int)(barW * progress));
            using (var fillBrush = new LinearGradientBrush(
                new Rectangle(barX, barY, barW, barH),
                Color.FromArgb(255, 85, 0),
                Color.FromArgb(230, 36, 98),
                LinearGradientMode.Horizontal))
            {
                g.FillRectangle(fillBrush, barX, barY, fillW, barH);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_timer != null) _timer.Dispose();
                if (_art != null) _art.Dispose();
            }
            base.Dispose(disposing);
        }

        public static Image LoadEmbeddedLogo()
        {
            try
            {
                Assembly asm = Assembly.GetExecutingAssembly();
                using (Stream stream = asm.GetManifestResourceStream("BraveTweaker.png"))
                {
                    if (stream != null)
                    {
                        using (Image img = Image.FromStream(stream))
                        {
                            return new Bitmap(img);
                        }
                    }
                }
            }
            catch { }
            return null;
        }
    }
}
