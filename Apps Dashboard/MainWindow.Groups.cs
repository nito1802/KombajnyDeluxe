using Apps_Dashboard.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Documents;

namespace Apps_Dashboard
{
    public partial class MainWindow
    {
        private Point groupStart;
        private Grid? groupHandle;
        private AppsGroupModel? movingGroup;
        private AppDragAdorner? groupGhost;
        private void EditGroup_Click(object sender, RoutedEventArgs e) { GroupEditor((Button)sender, (AppsGroupModel)((Button)sender).DataContext, false); e.Handled = true; }
        private void NewGroup_Click(object sender, RoutedEventArgs e) => GroupEditor((Button)sender, new AppsGroupModel(), true);
        private void AddGroupApp_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            EditTile(button, new SingleAppModel(), true, (AppsGroupModel)button.DataContext); e.Handled = true;
        }
        private void GroupEditor(Button anchor, AppsGroupModel group, bool create)
        {
            editor?.SetCurrentValue(Popup.IsOpenProperty, false);
            var content = new StackPanel { Width = 310 };
            content.Children.Add(new TextBlock { Text = create ? "Nowa grupa" : "Edytuj grupę", Foreground = Brushes.White, FontSize = 18, Margin = new Thickness(0,0,0,12) });
            var name = new TextBox { Text = group.GroupName, Padding = new Thickness(8), Margin = new Thickness(0,0,0,12) }; content.Children.Add(name);
            var palette = new WrapPanel(); content.Children.Add(palette);
            var color = new TextBox { Text = group.Color, Padding = new Thickness(8), Margin = new Thickness(0,8,0,8) };
            foreach (string hex in new[] { "#C99045", "#EA7272", "#E99AC4", "#AF8AE8", "#748EE8", "#62B5E5", "#54C5BC", "#7CC98A", "#C6CC70", "#A8B2C1" })
            {
                var brush = (Brush)new BrushConverter().ConvertFromString(hex)!;
                var swatch = new Button { Width = 29, Height = 29, Margin = new Thickness(1), Background = brush, BorderThickness = new Thickness(0), ToolTip = hex };
                swatch.Click += (_, _) => color.Text = hex; palette.Children.Add(swatch);
            }
            content.Children.Add(new TextBlock { Text = "Własny kolor HEX (#RRGGBB)", Foreground = Brushes.White, Margin = new Thickness(0,10,0,0) }); content.Children.Add(color);
            var error = new TextBlock { Foreground = Brushes.Salmon, TextWrapping = TextWrapping.Wrap }; content.Children.Add(error);
            var actions = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0,8,0,0) };
            var cancel = new Button { Content = "Anuluj", Padding = new Thickness(12,8,12,8), Margin = new Thickness(0,0,8,0) };
            var save = new Button { Content = "Zapisz ↵", Style = (Style)FindResource("PrimaryPopupButton") };
            actions.Children.Add(cancel); actions.Children.Add(save); content.Children.Add(actions);
            var popup = new Popup { PlacementTarget = anchor, Placement = PlacementMode.Bottom, StaysOpen = false, AllowsTransparency = true,
                Child = new Border { Child = content, Background = new SolidColorBrush(Color.FromRgb(30,33,40)), BorderBrush = group.Accent, BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(9), Padding = new Thickness(18) } };
            cancel.Click += (_, _) => popup.IsOpen = false;
            void Save()
            {
                string hex = color.Text.Trim();
                if (string.IsNullOrWhiteSpace(name.Text) || !System.Text.RegularExpressions.Regex.IsMatch(hex, "^#[0-9a-fA-F]{6}$")) { error.Text = "Podaj nazwę i kolor w formacie #RRGGBB."; return; }
                var oldName = group.GroupName; var oldColor = group.Color;
                bool saved = Commit(() => { group.GroupName = name.Text.Trim(); group.Color = hex; if (create) ViewModel.AppsGroups.Add(group); },
                    () => { if (create) ViewModel.AppsGroups.Remove(group); group.GroupName = oldName; group.Color = oldColor; });
                if (saved) popup.IsOpen = false;
            }
            save.Click += (_, _) => Save();
            content.PreviewKeyDown += (_, args) => { if (args.Key == Key.Enter) { args.Handled = true; if (!args.IsRepeat) Save(); } else if (args.Key == Key.Escape) { popup.IsOpen = false; args.Handled = true; } };
            popup.Opened += (_, _) => { name.Focus(); name.SelectAll(); }; editor = popup; popup.IsOpen = true;
        }
        private void Group_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Buttons edit/add; the remaining header is a drag handle.
            DependencyObject? hit = e.OriginalSource as DependencyObject;
            while (hit != null && hit != sender) { if (hit is Button) return; hit = VisualTreeHelper.GetParent(hit); }
            groupHandle = (Grid)sender; groupStart = e.GetPosition(this);
        }
        private void Group_MouseMove(object sender, MouseEventArgs e)
        {
            if (sender != groupHandle || e.LeftButton != MouseButtonState.Pressed || movingGroup != null) return;
            var point = e.GetPosition(this);
            if (Math.Abs(point.X - groupStart.X) < SystemParameters.MinimumHorizontalDragDistance && Math.Abs(point.Y - groupStart.Y) < SystemParameters.MinimumVerticalDragDistance) return;
            var header = groupHandle!; var card = (Border)((StackPanel)header.Parent).Parent;
            var group = (AppsGroupModel)header.DataContext;
            var layer = AdornerLayer.GetAdornerLayer(DashboardRoot); if (layer == null) return;
            var dpi = VisualTreeHelper.GetDpi(card);
            var bitmap = new RenderTargetBitmap(Math.Max(1,(int)(card.ActualWidth*dpi.DpiScaleX)), Math.Max(1,(int)(card.ActualHeight*dpi.DpiScaleY)),96*dpi.DpiScaleX,96*dpi.DpiScaleY,PixelFormats.Pbgra32); bitmap.Render(card); bitmap.Freeze();
            var ghost = new AppDragAdorner(DashboardRoot, bitmap, new Size(card.ActualWidth,card.ActualHeight));
            var grab = e.GetPosition(card);
            void Follow(object? o, EventArgs args) { if (GetCursorPos(out var cursor)) { var p = DashboardRoot.PointFromScreen(new Point(cursor.X,cursor.Y)); ghost.Position = new Point(p.X-grab.X,p.Y-grab.Y); ghost.InvalidateVisual(); } }
            movingGroup = group; groupGhost = ghost; card.Opacity = .3; layer.Add(ghost); CompositionTarget.Rendering += Follow;
            try { DragDrop.DoDragDrop(this,new DataObject(typeof(AppsGroupModel),group),DragDropEffects.Move); }
            finally { CompositionTarget.Rendering -= Follow; layer.Remove(ghost); card.Opacity=1; movingGroup=null; groupHandle=null; groupGhost=null; }
            e.Handled=true;
        }
        private void Group_DragOver(object sender, DragEventArgs e)
        {
            if (movingGroup == null) return;
            var card=(Border)sender;
            e.Effects = card.DataContext is AppsGroupModel target && target != movingGroup ? DragDropEffects.Move : DragDropEffects.None;
            if (groupGhost != null)
            {
                groupGhost.Target = e.Effects == DragDropEffects.Move ? new Rect(card.TranslatePoint(new Point(), DashboardRoot), new Size(card.ActualWidth, card.ActualHeight)) : null;
                groupGhost.InvalidateVisual();
            }
            e.Handled=true;
        }
        private void Group_Drop(object sender, DragEventArgs e)
        {
            if (movingGroup == null || ((Border)sender).DataContext is not AppsGroupModel target || target == movingGroup) return;
            int from=ViewModel.AppsGroups.IndexOf(movingGroup), to=ViewModel.AppsGroups.IndexOf(target);
            e.Effects=Commit(()=>ViewModel.AppsGroups.Move(from,to),()=>ViewModel.AppsGroups.Move(to,from)) ? DragDropEffects.Move : DragDropEffects.None; e.Handled=true;
        }
    }
}
