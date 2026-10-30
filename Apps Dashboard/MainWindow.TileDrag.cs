using Apps_Dashboard.Models;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Apps_Dashboard
{
    public partial class MainWindow
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct CursorPoint { public int X; public int Y; }
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out CursorPoint point);
        private SingleAppModel? draggedApp;
        private ObservableCollection<SingleAppModel>? previewApps;
        private SingleAppModel? previewTarget;
        private bool previewAfter;

        private sealed class AppDragAdorner : Adorner
        {
            private readonly ImageSource snapshot;
            private readonly Size size;
            public Point Position { get; set; }
            public AppDragAdorner(UIElement owner, ImageSource snapshot, Size size) : base(owner)
            {
                this.snapshot = snapshot; this.size = size;
                IsHitTestVisible = false; Opacity = 0.65;
            }
            protected override void OnRender(DrawingContext context) => context.DrawImage(snapshot, new Rect(Position, size));
        }
        private void Tile_Loaded(object sender, RoutedEventArgs e)
        {
            var tile = (Grid)sender;
            tile.Opacity = ReferenceEquals(tile.DataContext, draggedApp) ? 0.25 : 1;
        }
        private void PreviewPosition(SingleAppModel target, bool after)
        {
            if (previewApps == null || draggedApp == null || (previewTarget == target && previewAfter == after)) return;
            previewTarget = target; previewAfter = after;
            int oldIndex = previewApps.IndexOf(draggedApp);
            int index = previewApps.IndexOf(target) + (after ? 1 : 0);
            if (oldIndex < index) index--;
            if (oldIndex != index) previewApps.Move(oldIndex, index);
        }
        private void BeginTileDrag(Grid tile, SingleAppModel item, Point grabPoint)
        {
            var group = Group(item);
            if (group == null) return;
            DependencyObject? parent = tile;
            while (parent != null && parent is not ItemsControl) parent = VisualTreeHelper.GetParent(parent);
            if (parent is not ItemsControl control) return;
            var layer = AdornerLayer.GetAdornerLayer(DashboardRoot);
            if (layer == null) return;
            var dpi = VisualTreeHelper.GetDpi(tile);
            var bitmap = new RenderTargetBitmap(Math.Max(1, (int)Math.Ceiling(tile.ActualWidth * dpi.DpiScaleX)), Math.Max(1, (int)Math.Ceiling(tile.ActualHeight * dpi.DpiScaleY)), 96 * dpi.DpiScaleX, 96 * dpi.DpiScaleY, PixelFormats.Pbgra32);
            bitmap.Render(tile); bitmap.Freeze();
            var ghost = new AppDragAdorner(DashboardRoot, bitmap, new Size(tile.ActualWidth, tile.ActualHeight));
            void FollowCursor(object? sender, EventArgs args)
            {
                if (!GetCursorPos(out var cursor)) return;
                var point = DashboardRoot.PointFromScreen(new Point(cursor.X, cursor.Y));
                ghost.Position = new Point(point.X - grabPoint.X, point.Y - grabPoint.Y);
                ghost.InvalidateVisual();
            }
            GiveFeedbackEventHandler feedback = (_, args) =>
            {
                args.UseDefaultCursors = false;
                Mouse.SetCursor(args.Effects == DragDropEffects.None ? Cursors.No : Cursors.SizeAll);
                args.Handled = true;
            };
            draggedApp = item; previewTarget = null;
            previewApps = new ObservableCollection<SingleAppModel>(group);
            tile.Opacity = 0.25;
            control.SetCurrentValue(ItemsControl.ItemsSourceProperty, previewApps);
            FollowCursor(null, EventArgs.Empty);
            layer.Add(ghost);
            CompositionTarget.Rendering += FollowCursor;
            GiveFeedback += feedback;
            bool accepted = false;
            int destination = group.IndexOf(item);
            try
            {
                accepted = DragDrop.DoDragDrop(this, new DataObject(typeof(SingleAppModel), item), DragDropEffects.Move) == DragDropEffects.Move;
                destination = previewApps.IndexOf(item);
            }
            finally
            {
                CompositionTarget.Rendering -= FollowCursor;
                GiveFeedback -= feedback;
                layer.Remove(ghost);
                draggedApp = null; previewApps = null; previewTarget = null;
                control.SetCurrentValue(ItemsControl.ItemsSourceProperty, group);
                tile.Opacity = 1;
                Mouse.SetCursor(null);
            }
            int original = group.IndexOf(item);
            if (accepted && original != destination)
                Commit(() => group.Move(original, destination), () => group.Move(destination, original));
        }
    }
}
