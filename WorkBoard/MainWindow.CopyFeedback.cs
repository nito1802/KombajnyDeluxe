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
        private Border? copiedSnack;
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

        private void ShowCopiedSnack()
        {
            if (copiedSnack == null)
            {
                var row = new StackPanel { Orientation = Orientation.Horizontal };
                row.Children.Add(new TextBlock { Text = "✓", Foreground = new SolidColorBrush(Color.FromRgb(118, 231, 176)),
                    FontSize = 20, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 10, 0) });
                row.Children.Add(new TextBlock { Text = "Skopiowano", Foreground = Brushes.White, FontSize = 14,
                    VerticalAlignment = VerticalAlignment.Center });
                copiedSnack = new Border {
                    Background = new SolidColorBrush(Color.FromRgb(33, 44, 43)),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(79, 137, 113)), BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(12), Padding = new Thickness(20, 12, 20, 12),
                    HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Bottom,
                    Margin = new Thickness(0, 0, 0, 20), IsHitTestVisible = false,
                    RenderTransform = new TranslateTransform(), Child = row
                };
                Panel.SetZIndex(copiedSnack, 1000);
                mainGrid.Children.Add(copiedSnack);
                snackTimer.Tick += (_, _) => HideCopiedSnack();
            }
            snackVersion++;
            snackTimer.Stop();
            copiedSnack.Visibility = Visibility.Visible;
            copiedSnack.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(180)));
            copiedSnack.RenderTransform.BeginAnimation(TranslateTransform.YProperty,
                new DoubleAnimation(14, 0, TimeSpan.FromMilliseconds(260)) {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                });
            snackTimer.Interval = TimeSpan.FromSeconds(2);
            snackTimer.Start();
        }

        private void HideCopiedSnack()
        {
            snackTimer.Stop();
            if (copiedSnack == null) return;
            int version = snackVersion;
            var fade = new DoubleAnimation(0, TimeSpan.FromMilliseconds(220));
            fade.Completed += (_, _) => {
                if (version == snackVersion) copiedSnack.Visibility = Visibility.Collapsed;
            };
            copiedSnack.BeginAnimation(OpacityProperty, fade);
            copiedSnack.RenderTransform.BeginAnimation(TranslateTransform.YProperty,
                new DoubleAnimation(8, TimeSpan.FromMilliseconds(220)));
        }
    }
}
