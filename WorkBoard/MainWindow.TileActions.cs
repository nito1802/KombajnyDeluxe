using KombajnDoPracy.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using WorkBoard.Consts;
using WorkBoard.Enums;
using WorkBoard.Models;

namespace KombajnDoPracy
{
    public partial class MainWindow
    {
        private List<NormalButton> Group(NormalButton item) =>
            new[] { ButtonsViewModel.LeftButtons, ButtonsViewModel.MiddleButtons,
                ButtonsViewModel.RightButtons, ButtonsViewModel.LinkButtons }.First(g => g.Contains(item));

        private string LinkState() => string.Join(Environment.NewLine,
            ButtonsViewModel.LinkButtons.Select(b => $"{b.Name} --> {b.Path} [{b.LinkType}]"));

        private void Persist(string old)
        {
            foreach (var group in new[] { ButtonsViewModel.LeftButtons, ButtonsViewModel.MiddleButtons,
                ButtonsViewModel.RightButtons, ButtonsViewModel.LinkButtons })
                for (int i = 0; i < group.Count; i++) group[i].Idx = i + 1;
            foreach (var link in ButtonsViewModel.LinkButtons)
            {
                ButtonsViewModel.LinkHistory.RemoveAll(b => b.LinkId == link.LinkId);
                ButtonsViewModel.LinkHistory.Add(Newtonsoft.Json.JsonConvert.DeserializeObject<NormalButton>(
                    Newtonsoft.Json.JsonConvert.SerializeObject(link))!);
            }
            SerializableButtonItemViewModel.Save(ButtonsViewModel);
            if (old != LinkState())
            {
                var path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Consts.DataDirectoryName, Consts.ChangesLogFileName);
                File.AppendAllText(path, $"Stan linków na: {DateTime.Now:dd-MM-yyyy HH:mm}\nOLD>>>>\n{old}\n\n\n\n---------------------------\nNEW>>>>\n{LinkState()}\n\n\n\n---------------------------\n".Replace("\n", Environment.NewLine));
            }
            mainGrid.DataContext = null;
            mainGrid.DataContext = ButtonsViewModel;
        }

        private static Brush TypeBrush(NormalButton b) => b.LinkType switch
        {
            LinkType.Task => Brushes.Gold,
            LinkType.Bug => new SolidColorBrush(Color.FromRgb(235, 112, 112)),
            _ => Brushes.White
        };

        private void Tile_Loaded(object sender, RoutedEventArgs e)
        {
            var panel = (Panel)sender;
            if (panel.Tag != null || panel.DataContext is not NormalButton item) return;
            panel.Tag = true;
            // Overlay actions without changing the existing tile's command or copy gestures.
            var tile = panel.Children.OfType<Button>().FirstOrDefault();
            if (tile == null) return;
            bool link = ButtonsViewModel.LinkButtons.Contains(item);
            if (link) tile.BorderBrush = TypeBrush(item);
            panel.Children.Remove(tile);
            var overlay = new Grid();
            overlay.Children.Add(tile);
            panel.Children.Add(overlay);
            var handle = new Button
            {
                Content = new StackPanel
                {
                    Children = {
                    new System.Windows.Shapes.Ellipse { Width = 3, Height = 3, Fill = Brushes.White, Margin = new Thickness(0, 2, 0, 1) },
                    new System.Windows.Shapes.Ellipse { Width = 3, Height = 3, Fill = Brushes.White, Margin = new Thickness(0, 1, 0, 1) },
                    new System.Windows.Shapes.Ellipse { Width = 3, Height = 3, Fill = Brushes.White, Margin = new Thickness(0, 1, 0, 2) } }
                },
                Width = 24,
                Height = 26,
                FontSize = 21,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 4, 5, 0),
                Opacity = 0,
                Style = (Style)FindResource("TileActionButton"),
                ToolTip = "Akcje • przytrzymaj i przeciągnij"
            };
            overlay.Children.Add(handle);
            overlay.MouseEnter += (_, _) => handle.Opacity = 1;
            overlay.MouseLeave += (_, _) => { if (!handle.IsMouseCaptured) handle.Opacity = 0; };
            Point start = default;
            bool dragged = false;
            handle.PreviewMouseLeftButtonDown += (_, args) => { start = args.GetPosition(handle); dragged = false; };
            handle.PreviewMouseMove += (_, args) =>
            {
                if (args.LeftButton != MouseButtonState.Pressed || dragged) return;
                var pos = args.GetPosition(handle);
                if (Math.Abs(pos.X - start.X) < SystemParameters.MinimumHorizontalDragDistance &&
                    Math.Abs(pos.Y - start.Y) < SystemParameters.MinimumVerticalDragDistance) return;
                dragged = true;
                handle.ReleaseMouseCapture();
                BeginTileDrag(handle, overlay, item);
                args.Handled = true;
            };
            handle.Click += (_, _) => { if (!dragged) ShowTileMenu(handle, item); };
            overlay.AllowDrop = true;
            overlay.DragOver += (_, args) =>
            {
                var source = args.Data.GetData(typeof(NormalButton)) as NormalButton;
                bool valid = source != null && Group(item).Contains(source);
                args.Effects = valid ? DragDropEffects.Move : DragDropEffects.None;
                if (valid && source != item) PreviewTilePosition(source!, item, args.GetPosition(overlay).X >= overlay.ActualWidth / 2);
                args.Handled = true;
            };
            overlay.Drop += (_, args) =>
            {
                if (args.Data.GetData(typeof(NormalButton)) is NormalButton source && Group(item).Contains(source))
                {
                    args.Effects = DragDropEffects.Move;
                    args.Handled = true;
                }
            };
            if (draggedItem == item) overlay.Opacity = 0.25;
        }

        private static MenuItem ActionItem(string title, Action action)
        {
            var menu = new MenuItem { Header = title };
            menu.Click += (_, _) => action();
            return menu;
        }

        private void ShowTileMenu(Button handle, NormalButton item)
        {
            var menu = new ContextMenu { PlacementTarget = handle, Placement = PlacementMode.Bottom };
            menu.Items.Add(ActionItem("Dodaj", () => Insert(item, false)));
            menu.Items.Add(ActionItem("Duplikuj", () => Insert(item, true)));
            menu.Items.Add(ActionItem("Edytuj", () => EditTile(handle, item)));
            if (ButtonsViewModel.LinkButtons.Contains(item))
                menu.Items.Add(ActionItem("Edytuj typ", () =>
                {
                    var row = new StackPanel { Orientation = Orientation.Horizontal };
                    var popup = MakePopup(handle, row);
                    foreach (LinkType type in Enum.GetValues(typeof(LinkType)))
                    {
                        var toggle = new RadioButton
                        {
                            Content = type.ToString(),
                            GroupName = "LinkType",
                            Style = (Style)FindResource("LinkTypeToggle"),
                            IsChecked = item.LinkType == type,
                            Margin = new Thickness(0, 0, 8, 0),
                            Padding = new Thickness(6)
                        };
                        toggle.Checked += (_, _) => { var old = LinkState(); item.LinkType = type; popup.IsOpen = false; Persist(old); };
                        row.Children.Add(toggle);
                    }
                    popup.IsOpen = true;
                }));
            menu.Items.Add(new Separator());
            var delete = ActionItem("Usuń", () =>
            {
                if (MessageBox.Show(this, $"Czy na pewno usunąć „{item.Name}”?", "Usuń kafel",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
                string old = LinkState(); Group(item).Remove(item); Persist(old);
            });
            delete.IsEnabled = item.CanDelete;
            menu.Items.Add(delete);
            menu.IsOpen = true;
        }

        private void Insert(NormalButton item, bool duplicate)
        {
            string old = LinkState();
            var group = Group(item);
            var created = new NormalButton
            {
                Name = duplicate ? item.Name : "Nowy kafel",
                Path = duplicate ? item.Path : "",
                Description = duplicate ? item.Description : "",
                GroupId = item.GroupId,
                LinkType = duplicate ? item.LinkType : LinkType.Inne
            };
            group.Insert(group.IndexOf(item) + 1, created);
            Persist(old);
        }

        private Popup MakePopup(UIElement anchor, UIElement content) => new Popup
        {
            PlacementTarget = anchor,
            Placement = PlacementMode.Bottom,
            StaysOpen = false,
            AllowsTransparency = true,
            PopupAnimation = PopupAnimation.Fade,
            Child = new Border
            {
                Background = new LinearGradientBrush(Color.FromRgb(36, 39, 46), Color.FromRgb(25, 27, 33), 90),
                BorderBrush = new SolidColorBrush(Color.FromRgb(75, 88, 110)),
                CornerRadius = new CornerRadius(10),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(20),
                Child = content
            }
        };

        private void EditTile(Button handle, NormalButton item)
        {
            var fields = new StackPanel { Width = 370 };
            fields.Children.Add(new TextBlock { Text = "Edytuj kafel", FontSize = 18, FontWeight = FontWeights.SemiBold, Foreground = Brushes.White, Margin = new Thickness(0, 0, 0, 12) });
            var popup = MakePopup(handle, fields);
            TextBox Field(string label, string value, bool multiline = false)
            {
                fields.Children.Add(new TextBlock { Text = label, Foreground = Brushes.White, Margin = new Thickness(0, 6, 0, 3) });
                var box = new TextBox
                {
                    Text = value ?? "",
                    AcceptsReturn = multiline,
                    TextWrapping = TextWrapping.Wrap,
                    Background = new SolidColorBrush(Color.FromRgb(22, 25, 31)),
                    Foreground = Brushes.White,
                    BorderBrush = new SolidColorBrush(Color.FromRgb(65, 76, 94)),
                    CaretBrush = Brushes.White,
                    Padding = new Thickness(10, 8, 10, 8),
                    FontSize = 13,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    MaxHeight = 150,
                    MinHeight = multiline ? 70 : 36
                };
                fields.Children.Add(box); return box;
            }
            var name = Field("Tytuł", item.Name);
            var path = Field(ButtonsViewModel.LinkButtons.Contains(item) ? "Link / treść" : "Ścieżka", item.Path, true);
            var description = Field("Opis", item.Description, true);
            var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 10, 0, 0) };
            var save = new Button { Style = (Style)FindResource("PopupButton"), Content = "Zapisz", Padding = new Thickness(10, 4, 10, 4) };
            save.Click += (_, _) =>
            {
                if (string.IsNullOrWhiteSpace(name.Text) || string.IsNullOrWhiteSpace(path.Text)) return;
                string old = LinkState(); item.Name = name.Text.Trim(); item.Path = path.Text.Trim();
                item.Description = description.Text; popup.IsOpen = false; Persist(old);
            };
            var cancel = new Button { Style = (Style)FindResource("PopupButton"), Content = "Anuluj", Margin = new Thickness(8, 0, 0, 0), Padding = new Thickness(10, 4, 10, 4) };
            cancel.Click += (_, _) => popup.IsOpen = false;
            row.Children.Add(save); row.Children.Add(cancel); fields.Children.Add(row);
            popup.IsOpen = true; name.Focus(); name.SelectAll();
        }

        private void ShowOptionsMenu()
        {
            var menu = new ContextMenu { PlacementTarget = btnOptions, Placement = PlacementMode.Bottom };
            var screens = new MenuItem { Header = "Screeny" };
            var tasks = new MenuItem { Header = "Pokaż Taski/Bugi" };
            var periods = new[] { ("Dzisiaj",24), ("Ostatni tydzień",1), ("Ostatnie 2 tygodnie",2),
                ("Ostatni miesiąc",3), ("Ostatni rok",12), ("Wszystkie",-1) };
            foreach (var (label, range) in periods)
            {
                screens.Items.Add(new MenuItem { Header = label, Command = HistoryWindowViewModel.HistoryOfScreensCommand, CommandParameter = range });
                tasks.Items.Add(ActionItem(label, () => ShowTasks(range)));
            }
            menu.Items.Add(screens); menu.Items.Add(tasks);
            var add = new MenuItem { Header = "Dodaj kafel" };
            foreach (var entry in new[] { ("Lewa grupa", ButtonsViewModel.LeftButtons, ButtonGroup.LeftButtons),
                ("Środkowa grupa", ButtonsViewModel.MiddleButtons, ButtonGroup.MiddleButtons),
                ("Prawa grupa", ButtonsViewModel.RightButtons, ButtonGroup.RightButtons),
                ("Linki", ButtonsViewModel.LinkButtons, ButtonGroup.UrlButtons) })
                add.Items.Add(ActionItem(entry.Item1, () =>
                {
                    var old = LinkState();
                    entry.Item2.Add(new NormalButton { Name = "Nowy kafel", Path = "", GroupId = entry.Item3 });
                    Persist(old);
                }));
            menu.Items.Add(add);
            menu.IsOpen = true;
        }

        private void ShowTasks(int range)
        {
            DateTime from = range switch
            {
                24 => DateTime.Today,
                1 => DateTime.Now.AddDays(-7),
                2 => DateTime.Now.AddDays(-14),
                3 => DateTime.Now.AddMonths(-1),
                12 => DateTime.Now.AddYears(-1),
                _ => DateTime.MinValue
            };
            var links = ButtonsViewModel.LinkHistory.Concat(ButtonsViewModel.LinkButtons)
                .GroupBy(b => b.LinkId).Select(g => g.Last())
                .Where(b => b.LinkType != LinkType.Inne && b.AddedAt >= from)
                .OrderByDescending(b => b.AddedAt).ToList();
            var list = new StackPanel { Margin = new Thickness(18) };
            foreach (var link in links)
            {
                var row = new DockPanel { Margin = new Thickness(0, 0, 0, 14) };
                var date = new TextBlock
                {
                    Text = link.AddedAt.ToString("dd.MM.yyyy HH:mm"),
                    FontSize = 11,
                    Foreground = Brushes.Silver,
                    Margin = new Thickness(12, 4, 0, 0)
                };
                DockPanel.SetDock(date, Dock.Right); row.Children.Add(date);
                row.Children.Add(new TextBox
                {
                    Text = $"[{link.LinkType}] {link.Name}\n{link.Path}".Replace("\n", Environment.NewLine),
                    IsReadOnly = true,
                    TextWrapping = TextWrapping.Wrap,
                    BorderThickness = new Thickness(0),
                    Background = Brushes.Transparent,
                    Foreground = TypeBrush(link)
                });
                list.Children.Add(row);
            }
            if (links.Count == 0) list.Children.Add(new TextBlock { Text = "Brak Tasków/Bugów w tym okresie.", Foreground = Brushes.White });
            new Window
            {
                Title = "Taski / Bugi",
                Owner = this,
                Width = 850,
                Height = 550,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Background = new SolidColorBrush(Color.FromRgb(30, 30, 30)),
                Content = new ScrollViewer { Content = list, VerticalScrollBarVisibility = ScrollBarVisibility.Auto }
            }.Show();
        }
    }
}