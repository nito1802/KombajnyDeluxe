using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using WorkBoard.Models;

namespace KombajnDoPracy
{
    public partial class MainWindow
    {
        [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "GetWindowLongW")]
        private static extern int GetWindowLong(IntPtr hwnd, int index);
        [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "SetWindowLongW")]
        private static extern int SetWindowLong(IntPtr hwnd, int index, int value);

        private Border? copiedSnack;
        private Window? copiedSnackWindow;
        private TextBlock? copiedSnackText;
        private readonly DispatcherTimer snackTimer = new DispatcherTimer();
        private int snackVersion;

        private void AddCopyButton(Grid overlay, NormalButton item)
        {
            var icon = new Grid { Width = 16, Height = 17, IsHitTestVisible = false };
            icon.Children.Add(new Border { Width = 10, Height = 12, CornerRadius = new CornerRadius(2),
                BorderBrush = Brushes.White, BorderThickness = new Thickness(1.3),
                HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top });
            icon.Children.Add(new Border { Width = 10, Height = 12, CornerRadius = new CornerRadius(2),
                Background = new SolidColorBrush(Color.FromRgb(35, 38, 44)),
                BorderBrush = Brushes.White, BorderThickness = new Thickness(1.3),
                HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Bottom });
            var scale = new ScaleTransform(1, 1);
            var copy = new Button {
                Content = icon, Width = 26, Height = 26,
                HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(8, 4, 0, 0), Opacity = 0, ToolTip = "Kopiuj link",
                Style = (Style)FindResource("TileActionButton"),
                RenderTransformOrigin = new Point(0.5, 0.5), RenderTransform = scale
            };
            void AnimateScale(double value, int milliseconds, bool bounce = false)
            {
                var animation = new DoubleAnimation(value, TimeSpan.FromMilliseconds(milliseconds)) {
                    EasingFunction = bounce ? new BackEase { Amplitude = 0.4, EasingMode = EasingMode.EaseOut }
                        : new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                scale.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
                scale.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
            }
            copy.PreviewMouseLeftButtonDown += (_, _) => AnimateScale(0.82, 80);
            copy.PreviewMouseLeftButtonUp += (_, _) => AnimateScale(1, 220, true);
            copy.LostMouseCapture += (_, _) => AnimateScale(1, 220, true);
            copy.Click += (_, _) => item.CopyPath.Execute(null);
            overlay.Children.Add(copy);
            overlay.MouseEnter += (_, _) => copy.Opacity = 1;
            overlay.MouseLeave += (_, _) => { if (!copy.IsMouseCaptured) copy.Opacity = 0; };
            copy.LostMouseCapture += (_, _) => { if (!overlay.IsMouseOver) copy.Opacity = 0; };
        }

        private async void ShowCopiedSnack(string title)
        {
            if (copiedSnack == null)
            {
                var row = new StackPanel { Orientation = Orientation.Horizontal };
                row.Children.Add(new TextBlock { Text = "✓", Foreground = new SolidColorBrush(Color.FromRgb(118, 231, 176)),
                    FontSize = 20, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 10, 0) });
                copiedSnackText = new TextBlock { Foreground = Brushes.White, FontSize = 14,
                    TextWrapping = TextWrapping.Wrap, MaxWidth = 560, VerticalAlignment = VerticalAlignment.Center };
                row.Children.Add(copiedSnackText);
                copiedSnack = new Border {
                    Background = new SolidColorBrush(Color.FromRgb(33, 44, 43)),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(79, 137, 113)), BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(12), Padding = new Thickness(20, 12, 20, 12),
                    HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch,
                    Margin = new Thickness(0), IsHitTestVisible = false,
                    RenderTransform = new TranslateTransform(), Child = row
                };
                Panel.SetZIndex(copiedSnack, 1000);
                copiedSnackWindow = new Window
                {
                    WindowStyle = WindowStyle.None, ResizeMode = ResizeMode.NoResize,
                    AllowsTransparency = true, Background = Brushes.Transparent,
                    ShowInTaskbar = false, ShowActivated = false, Topmost = true,
                    SizeToContent = SizeToContent.Manual, Content = copiedSnack, Opacity = 0,
                    IsHitTestVisible = false, Focusable = false
                };
                copiedSnackWindow.SourceInitialized += (_, _) =>
                {
                    var hwnd = new System.Windows.Interop.WindowInteropHelper(copiedSnackWindow).Handle;
                    SetWindowLong(hwnd, -20, GetWindowLong(hwnd, -20) | 0x08000000 | 0x00000020 | 0x00000080);
                };
                Closed += (_, _) => { snackTimer.Stop(); copiedSnackWindow.Close(); };
                snackTimer.Tick += (_, _) => HideCopiedSnack();
            }
            int version = ++snackVersion;
            snackTimer.Stop();
            var toast = copiedSnackWindow!;
            // Keep the native window transparent until text, layout and position are ready.
            toast.BeginAnimation(OpacityProperty, null);
            toast.Opacity = 0;
            copiedSnackText!.Text = $"Skopiowano treść {title}";
            var area = SystemParameters.WorkArea;
            copiedSnackText.MaxWidth = Math.Max(80, Math.Min(560, area.Width - 120));
            copiedSnack!.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            toast.Width = Math.Ceiling(copiedSnack.DesiredSize.Width);
            toast.Height = Math.Ceiling(copiedSnack.DesiredSize.Height);
            toast.Left = area.Left + (area.Width - toast.Width) / 2;
            toast.Top = area.Bottom - toast.Height - 24;
            toast.Show();
            await Dispatcher.InvokeAsync(() => toast.UpdateLayout(), DispatcherPriority.Loaded);
            if (version != snackVersion || !toast.IsVisible) return;
            toast.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(180)));
            snackTimer.Interval = TimeSpan.FromSeconds(2);
            snackTimer.Start();
        }
        private void HideCopiedSnack()
        {
            snackTimer.Stop();
            if (copiedSnackWindow == null) return;
            int version = snackVersion;
            var fade = new DoubleAnimation(0, TimeSpan.FromMilliseconds(220));
            fade.Completed += (_, _) => {
                if (version == snackVersion) copiedSnackWindow.Hide();
            };
            copiedSnackWindow.BeginAnimation(OpacityProperty, fade);
        }
    }
}