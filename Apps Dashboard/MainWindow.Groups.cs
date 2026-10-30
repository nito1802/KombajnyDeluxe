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
            var color = new Apps_Dashboard.Controls.GroupColorPicker((Color)ColorConverter.ConvertFromString(group.Color));
            content.Children.Add(color);
            string? texturePath = group.BackgroundPath;
            var textureLabel = new TextBlock { Text = string.IsNullOrEmpty(texturePath) ? "Brak tekstury" : System.IO.Path.GetFileName(texturePath), Foreground = Brushes.LightGray, TextTrimming = TextTrimming.CharacterEllipsis, Margin = new Thickness(0,8,0,4) };
            var texturePreview = new Image { Height = 48, Stretch = Stretch.UniformToFill, Source = group.Texture, Visibility = group.Texture == null ? Visibility.Collapsed : Visibility.Visible, Effect = new System.Windows.Media.Effects.BlurEffect { Radius = 8 } };
            var textureButtons = new StackPanel { Orientation = Orientation.Horizontal };
            var choose = new Button { Content = "Wczytaj tło…", Padding = new Thickness(10,6,10,6), Margin = new Thickness(0,0,8,0) };
            var remove = new Button { Content = "Usuń tło", Padding = new Thickness(10,6,10,6) };
            choose.Click += (_,_) =>
            {
                var dialog = new Microsoft.Win32.OpenFileDialog { Filter = "Grafiki|*.png;*.jpg;*.jpeg;*.bmp;*.webp;*.tif;*.tiff" };
                if (dialog.ShowDialog(this) != true) return;
                var candidate = new AppsGroupModel { BackgroundPath = dialog.FileName };
                if (candidate.Texture == null) { MessageBox.Show(this,"Nie można wczytać tej grafiki.","Tło grupy"); return; }
                texturePath = dialog.FileName; textureLabel.Text = System.IO.Path.GetFileName(texturePath);
                texturePreview.Source = candidate.Texture; texturePreview.Visibility = Visibility.Visible;
            };
            remove.Click += (_,_) => { texturePath = null; textureLabel.Text = "Brak tekstury"; texturePreview.Source = null; texturePreview.Visibility = Visibility.Collapsed; };
            textureButtons.Children.Add(choose); textureButtons.Children.Add(remove);
            content.Children.Add(textureLabel); content.Children.Add(textureButtons); content.Children.Add(texturePreview);            var error = new TextBlock { Foreground = Brushes.Salmon, TextWrapping = TextWrapping.Wrap }; content.Children.Add(error);
            var actions = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0,8,0,0) };
            var cancel = new Button { Content = "Anuluj", Padding = new Thickness(12,8,12,8), Margin = new Thickness(0,0,8,0) };
            var save = new Button { Content = "Zapisz ↵", Style = (Style)FindResource("PrimaryPopupButton") };
            actions.Children.Add(cancel); actions.Children.Add(save); content.Children.Add(actions);
            var popup = new Popup { PlacementTarget = anchor, Placement = PlacementMode.Bottom, StaysOpen = false, AllowsTransparency = true,
                Child = new Border { Child = content, Background = new SolidColorBrush(Color.FromRgb(30,33,40)), BorderBrush = group.Accent, BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(9), Padding = new Thickness(18) } };
            cancel.Click += (_, _) => popup.IsOpen = false;
            void Save()
            {
                string hex = $"#{color.SelectedColor.R:X2}{color.SelectedColor.G:X2}{color.SelectedColor.B:X2}";
                if (string.IsNullOrWhiteSpace(name.Text)) { error.Text = "Podaj nazwę grupy."; return; }
                var oldName = group.GroupName; var oldColor = group.Color; var oldTexture = group.BackgroundPath;
                bool saved = Commit(() => { group.GroupName = name.Text.Trim(); group.Color = hex; group.BackgroundPath = texturePath; if (create) ViewModel.AppsGroups.Add(group); },
                    () => { if (create) ViewModel.AppsGroups.Remove(group); group.GroupName = oldName; group.Color = oldColor; group.BackgroundPath = oldTexture; });
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
            groupHandle = (Grid)sender; groupStart = e.GetPosition(this); groupHandle.CaptureMouse();
        }
        private void Group_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (groupHandle?.IsMouseCaptured == true) groupHandle.ReleaseMouseCapture();
            groupHandle = null;
        }
        private void Group_MouseMove(object sender, MouseEventArgs e)
        {
            if (sender != groupHandle || e.LeftButton != MouseButtonState.Pressed || movingGroup != null) return;
            var point = e.GetPosition(this);
            if (Math.Abs(point.X - groupStart.X) < SystemParameters.MinimumHorizontalDragDistance && Math.Abs(point.Y - groupStart.Y) < SystemParameters.MinimumVerticalDragDistance) return;
            groupHandle.ReleaseMouseCapture();
            var header = groupHandle!; var card = (Border)((Grid)((StackPanel)header.Parent).Parent).Parent;
            var group = (AppsGroupModel)header.DataContext;
            var layer = AdornerLayer.GetAdornerLayer(DashboardRoot); if (layer == null) return;
            var dpi = VisualTreeHelper.GetDpi(card);
            var bitmap = new RenderTargetBitmap(Math.Max(1,(int)(card.ActualWidth*dpi.DpiScaleX)), Math.Max(1,(int)(card.ActualHeight*dpi.DpiScaleY)),96*dpi.DpiScaleX,96*dpi.DpiScaleY,PixelFormats.Pbgra32); bitmap.Render(card); bitmap.Freeze();
            var ghost = new AppDragAdorner(DashboardRoot, bitmap, new Size(card.ActualWidth,card.ActualHeight));
            var grab = TranslatePoint(groupStart, card); groupGrab = grab; groupSize = new Size(card.ActualWidth + 10, card.ActualHeight + 10); groupDestination = null;
            void Follow(object? o, EventArgs args) { if (GetCursorPos(out var cursor)) { var p = DashboardRoot.PointFromScreen(new Point(cursor.X,cursor.Y)); ghost.Position = new Point(p.X-grab.X,p.Y-grab.Y); ghost.InvalidateVisual(); } }
            movingGroup = group; groupGhost = ghost; card.Opacity = .3; layer.Add(ghost); CompositionTarget.Rendering += Follow;
            try { DragDrop.DoDragDrop(this,new DataObject(typeof(AppsGroupModel),group),DragDropEffects.Move); }
            finally { CompositionTarget.Rendering -= Follow; layer.Remove(ghost); card.Opacity=1; movingGroup=null; groupHandle=null; groupGhost=null; }
            e.Handled=true;
        }
        private Point groupGrab;
        private Size groupSize;
        private Point? groupDestination;
        private Apps_Dashboard.Helpers.SpatialGroupsPanel? BoardPanel()
        {
            Apps_Dashboard.Helpers.SpatialGroupsPanel? Find(DependencyObject root)
            {
                if(root is Apps_Dashboard.Helpers.SpatialGroupsPanel panel)return panel;
                for(int i=0;i<VisualTreeHelper.GetChildrenCount(root);i++)if(Find(VisualTreeHelper.GetChild(root,i)) is {} found)return found;
                return null;
            }
            return Find(GroupsControl);
        }
        private void Board_DragOver(object sender,DragEventArgs e)
        {
            if(movingGroup==null)return;
            var panel=BoardPanel(); groupDestination=null;
            if(panel!=null)
            {
                var p=e.GetPosition(panel);
                double snap=Apps_Dashboard.Helpers.SpatialGroupsPanel.Snap;
                var position=new Point(Math.Max(0,Math.Round((p.X-groupGrab.X)/snap)*snap),Math.Max(0,Math.Round((p.Y-groupGrab.Y)/snap)*snap));
                var rect=new Rect(position,groupSize);
                if(panel.CanPlace(movingGroup,rect))groupDestination=position;
                if(groupGhost!=null)groupGhost.Target=groupDestination.HasValue?new Rect(panel.TranslatePoint(position,DashboardRoot),groupSize):null;
            }
            groupGhost?.InvalidateVisual();e.Effects=groupDestination.HasValue?DragDropEffects.Move:DragDropEffects.None;e.Handled=true;
        }
        private void Board_Drop(object sender,DragEventArgs e)
        {
            if(movingGroup==null)return;
            Board_DragOver(sender,e);
            if(groupDestination is Point p)
            {
                var group=movingGroup;var x=group.LayoutX;var y=group.LayoutY;
                if(!Commit(()=>{group.LayoutX=p.X;group.LayoutY=p.Y;},()=>{group.LayoutX=x;group.LayoutY=y;}))e.Effects=DragDropEffects.None;
                BoardPanel()?.InvalidateMeasure();
            }
        }
    }
}

