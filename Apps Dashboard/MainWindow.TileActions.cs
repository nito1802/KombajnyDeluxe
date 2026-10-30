using Apps_Dashboard.Models;
using Apps_Dashboard.ViewModels;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Apps_Dashboard
{
    public partial class MainWindow
    {
        private AppsGroupsViewModel ViewModel => (AppsGroupsViewModel)DataContext;
        private ObservableCollection<SingleAppModel>? Group(SingleAppModel item) => ViewModel.AppsGroups.FirstOrDefault(g => g.Apps.Contains(item))?.Apps;
        private Point dragStart;
        private Button? dragHandle;
        private bool dragged;
        private Popup? editor;

        // Keep collection changes and disk state consistent when saving fails.
        private bool Commit(Action apply, Action rollback)
        {
            apply();
            try { ViewModel.Save(); return true; }
            catch (Exception ex)
            {
                rollback();
                MessageBox.Show(this, $"Nie udało się zapisać zmian: {ex.Message}", "Błąd zapisu", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
        private void EditTile_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is SingleAppModel item) EditTile(button, item, false);
            e.Handled = true;
        }
        private void EditTile(Button anchor, SingleAppModel item, bool create, AppsGroupModel? owner = null)
        {
            editor?.SetCurrentValue(Popup.IsOpenProperty, false);
            var group = owner?.Apps ?? Group(item);
            if (group == null) return;
            var fields = new StackPanel { Width = 370 };
            fields.Children.Add(new TextBlock { Text = create ? "Dodaj aplikację" : "Edytuj aplikację", Foreground = Brushes.White, FontSize = 18, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 12) });
            TextBox Field(string label, string value)
            {
                fields.Children.Add(new TextBlock { Text = label, Foreground = Brushes.White, Margin = new Thickness(0, 8, 0, 4) });
                var box = new TextBox { Text = value, Padding = new Thickness(10, 8, 10, 8), FontSize = 13, Background = new SolidColorBrush(Color.FromRgb(22, 25, 31)), Foreground = Brushes.White, CaretBrush = Brushes.White, BorderBrush = new SolidColorBrush(Color.FromRgb(65, 76, 94)) };
                fields.Children.Add(box);
                return box;
            }
            var name = Field("Nazwa", create ? "" : item.DisplayName);
            var path = Field("Ścieżka", create ? "" : item.Path);
            var browse = new Button { Content = "Wybierz plik…", Margin = new Thickness(0, 8, 0, 0), Padding = new Thickness(10, 5, 10, 5), HorizontalAlignment = HorizontalAlignment.Left };
            browse.Click += (_, _) =>
            {
                var dialog = new Microsoft.Win32.OpenFileDialog { Filter = "Programy i skróty|*.exe;*.lnk;*.bat;*.cmd|Wszystkie pliki|*.*" };
                if (dialog.ShowDialog(this) == true) path.Text = dialog.FileName;
            };
            fields.Children.Add(browse);
            var error = new TextBlock { Foreground = Brushes.Salmon, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 8, 0, 0) };
            fields.Children.Add(error);
            var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 12, 0, 0) };
            var cancel = new Button { Content = "Anuluj", Padding = new Thickness(14, 7, 14, 7), Margin = new Thickness(0, 0, 8, 0) };
            var save = new Button { Content = "Zapisz ↵", Style = (Style)FindResource("PrimaryPopupButton") };
            buttons.Children.Add(cancel); buttons.Children.Add(save); fields.Children.Add(buttons);
            var popup = new Popup
            {
                PlacementTarget = anchor, Placement = PlacementMode.Bottom, StaysOpen = false, AllowsTransparency = true, PopupAnimation = PopupAnimation.Fade,
                Child = new Border { Background = new LinearGradientBrush(Color.FromRgb(36, 39, 46), Color.FromRgb(25, 27, 33), 90), BorderBrush = new SolidColorBrush(Color.FromRgb(75, 88, 110)), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(10), Padding = new Thickness(20), Child = fields }
            };
            cancel.Click += (_, _) => popup.IsOpen = false;
            save.Click += (_, _) =>
            {
                if (string.IsNullOrWhiteSpace(name.Text) || string.IsNullOrWhiteSpace(path.Text)) { error.Text = "Podaj nazwę i ścieżkę aplikacji."; return; }
                bool saved;
                if (create)
                {
                    var added = new SingleAppModel { DisplayName = name.Text.Trim(), Path = path.Text.Trim() };
                    saved = Commit(() => group.Insert(group.IndexOf(item) + 1, added), () => group.Remove(added));
                }
                else
                {
                    var oldName = item.DisplayName; var oldPath = item.Path;
                    saved = Commit(() => { item.DisplayName = name.Text.Trim(); item.Path = path.Text.Trim(); }, () => { item.DisplayName = oldName; item.Path = oldPath; });
                }
                if (saved) popup.IsOpen = false;
            };
            fields.PreviewKeyDown += (_, args) =>
            {
                if (args.Key == Key.Escape) { popup.IsOpen = false; args.Handled = true; }
                else if (args.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
                {
                    args.Handled = true;
                    if (!args.IsRepeat) save.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                }
            };
            popup.Opened += (_, _) => { name.Focus(); name.SelectAll(); };
            editor = popup; popup.IsOpen = true;
        }
        private void TileActions_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if (dragged || sender is not Button handle || handle.DataContext is not SingleAppModel item) return;
            var group = Group(item);
            if (group == null) return;
            var menu = new ContextMenu { PlacementTarget = handle, Placement = PlacementMode.Bottom, Background = new SolidColorBrush(Color.FromRgb(36, 39, 46)), Foreground = Brushes.White, BorderBrush = new SolidColorBrush(Color.FromRgb(75, 88, 110)) };
            void Action(string title, Action action)
            {
                var entry = new MenuItem { Header = title, Padding = new Thickness(12, 7, 12, 7) };
                entry.Click += (_, _) => action(); menu.Items.Add(entry);
            }
            Action("Dodaj", () => EditTile(handle, item, true));
            Action("Duplikuj", () =>
            {
                var copy = new SingleAppModel { DisplayName = item.DisplayName, Path = item.Path };
                Commit(() => group.Insert(group.IndexOf(item) + 1, copy), () => group.Remove(copy));
            });
            menu.Items.Add(new Separator());
            Action("Usuń", () =>
            {
                if (MessageBox.Show(this, $"Usunąć „{item.DisplayName}”?", "Usuń aplikację", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
                var index = group.IndexOf(item);
                Commit(() => group.Remove(item), () => group.Insert(index, item));
            });
            handle.ContextMenu = menu; menu.IsOpen = true;
        }
        private void Handle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            dragHandle = (Button)sender; dragStart = e.GetPosition(this); dragged = false;
        }
        private void Handle_MouseMove(object sender, MouseEventArgs e)
        {
            if (sender != dragHandle || e.LeftButton != MouseButtonState.Pressed || dragged || dragHandle.DataContext is not SingleAppModel item) return;
            var position = e.GetPosition(this);
            if (Math.Abs(position.X - dragStart.X) < SystemParameters.MinimumHorizontalDragDistance && Math.Abs(position.Y - dragStart.Y) < SystemParameters.MinimumVerticalDragDistance) return;
            dragged = true;
            var handle = dragHandle;
            handle.ReleaseMouseCapture();
            var tile = (Grid)handle.Parent;
            try { BeginTileDrag(tile, item, e.GetPosition(tile)); }
            finally { dragHandle = null; }
            e.Handled = true;
        }
    }
}


