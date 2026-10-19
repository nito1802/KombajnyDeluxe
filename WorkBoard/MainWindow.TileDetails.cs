using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WorkBoard.Models;

namespace KombajnDoPracy
{
    public partial class MainWindow
    {
        private void ShowTileDetails(Button anchor, NormalButton item)
        {
            bool link = ButtonsViewModel.LinkButtons.Contains(item);
            var fields = new StackPanel();
            void Detail(string label, string? value)
            {
                fields.Children.Add(new TextBlock {
                    Text = label, FontSize = 11, Foreground = new SolidColorBrush(Color.FromRgb(157, 170, 190)),
                    Margin = new Thickness(0, 9, 0, 3)
                });
                fields.Children.Add(new TextBox {
                    Text = string.IsNullOrEmpty(value) ? "—" : value,
                    IsReadOnly = true, TextWrapping = TextWrapping.Wrap,
                    Background = Brushes.Transparent, Foreground = Brushes.White,
                    BorderThickness = new Thickness(0), Padding = new Thickness(0),
                    FontSize = 13,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Disabled
                });
            }
            var group = Group(item);
            string groupName = ReferenceEquals(group, ButtonsViewModel.LeftButtons) ? ButtonsViewModel.LeftGroupName :
                ReferenceEquals(group, ButtonsViewModel.MiddleButtons) ? ButtonsViewModel.MiddleGroupName :
                ReferenceEquals(group, ButtonsViewModel.RightButtons) ? ButtonsViewModel.RightGroupName : "Linki";
            Detail("Tytuł", item.Name);
            Detail(link ? "Link / treść" : "Ścieżka", item.Path);
            Detail("Opis", item.Description);
            Detail("Grupa", groupName);
            if (link) Detail("Typ", item.LinkType.ToString());
            Detail("ID", item.LinkId.ToString());
            Detail("Łącznie utworzono", ButtonsViewModel.LastCreatedId.ToString());
            Detail("Data dodania", item.AddedAt.ToString("dd.MM.yyyy HH:mm:ss"));
            Detail("Ostatnia modyfikacja", item.ModifiedAt?.ToString("dd.MM.yyyy HH:mm:ss") ?? "Brak zapisanej daty modyfikacji");
            Detail("Liczba użyć", item.ClickCounter.ToString());
            // WPF also limits transparent popups to a portion of the screen. Constrain
            // the complete layout so the scroll viewport is measured before that limit.
            double maxHeight = Math.Max(120, Math.Min(620, SystemParameters.WorkArea.Height * 0.70) - 42);
            var layout = new Grid { Width = 414, MaxHeight = maxHeight };
            layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            var header = new Grid { Margin = new Thickness(0, 0, 0, 14) };
            header.Children.Add(new TextBlock {
                Text = "Szczegóły kafla", FontSize = 18, FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.White, VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 38, 0)
            });
            var close = new Button {
                Content = "✕", FontSize = 16, Width = 30, Height = 30, ToolTip = "Zamknij",
                Style = (Style)FindResource("TileActionButton"),
                HorizontalAlignment = HorizontalAlignment.Right
            };
            header.Children.Add(close);
            layout.Children.Add(header);
            var content = new ScrollViewer {
                Content = fields, CanContentScroll = false,
                Style = (Style)FindResource("DetailsScrollViewer")
            };
            // Read-only TextBoxes otherwise consume the wheel even when their own scrolling is disabled.
            content.PreviewMouseWheel += (_, args) =>
            {
                content.ScrollToVerticalOffset(content.VerticalOffset - args.Delta);
                args.Handled = true;
            };
            Grid.SetRow(content, 1);
            layout.Children.Add(content);
            var popup = MakePopup(anchor, layout);
            close.Click += (_, _) => popup.IsOpen = false;
            popup.IsOpen = true;
        }
    }
}
