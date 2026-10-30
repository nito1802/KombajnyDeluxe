using Apps_Dashboard.Helpers;
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
        private SpatialAppPanel? dragPanel;
        private (int Column, int Row)? dropCell;
        private AppDragAdorner? activeGhost;
        
        

        private sealed class AppDragAdorner : Adorner
        {
            private readonly ImageSource snapshot;
            private readonly Size size;
            public Point Position { get; set; }
            public Rect? Target { get; set; }
            public AppDragAdorner(UIElement owner, ImageSource snapshot, Size size) : base(owner)
            {
                this.snapshot = snapshot; this.size = size;
                IsHitTestVisible = false; Opacity = 0.65;
            }
                        protected override void OnRender(DrawingContext context)
            {
                if (Target is Rect target)
                    context.DrawRoundedRectangle(new SolidColorBrush(Color.FromArgb(45, 217, 154, 66)), new Pen(Brushes.Goldenrod, 2), target, 8, 8);
                context.DrawImage(snapshot, new Rect(Position, size));
            }
        }
        private void Tile_Loaded(object sender, RoutedEventArgs e)
        {
            var tile = (Grid)sender;
            tile.Opacity = ReferenceEquals(tile.DataContext, draggedApp) ? 0.25 : 1;
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
            draggedApp = item; dropCell = null; activeGhost = ghost;
            dragPanel = FindPanel(control);
            if (dragPanel == null) { draggedApp = null; activeGhost = null; return; }
            dragPanel.ExtraRow = true; dragPanel.InvalidateMeasure();

            tile.Opacity = 0.25;

            FollowCursor(null, EventArgs.Empty);
            layer.Add(ghost);
            CompositionTarget.Rendering += FollowCursor;
            GiveFeedback += feedback;
            bool accepted = false;
            (int Column, int Row)? destination = null;
            try
            {
                accepted = DragDrop.DoDragDrop(this, new DataObject(typeof(SingleAppModel), item), DragDropEffects.Move) == DragDropEffects.Move;
                destination = dropCell;
            }
            finally
            {
                CompositionTarget.Rendering -= FollowCursor;
                GiveFeedback -= feedback;
                layer.Remove(ghost);
                dragPanel.ExtraRow = false; dragPanel.InvalidateMeasure();
                draggedApp = null; dragPanel = null; dropCell = null; activeGhost = null;

                tile.Opacity = 1;
                Mouse.SetCursor(null);
            }
            if (accepted && destination is var cell && cell.HasValue)
            {
                var oldColumn = item.LayoutColumn; var oldRow = item.LayoutRow;
                Commit(() => { item.LayoutColumn = cell.Value.Column; item.LayoutRow = cell.Value.Row; },
                    () => { item.LayoutColumn = oldColumn; item.LayoutRow = oldRow; });
                FindPanel(control)?.InvalidateMeasure();
            }
        }
        private static SpatialAppPanel? FindPanel(DependencyObject root)
        {
            if (root is SpatialAppPanel panel) return panel;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
                if (FindPanel(VisualTreeHelper.GetChild(root, i)) is SpatialAppPanel found) return found;
            return null;
        }
        private bool UpdateDrop(ItemsControl control, DragEventArgs e)
        {
            var panel = FindPanel(control);
            bool valid = panel != null && panel == dragPanel && draggedApp != null &&
                ReferenceEquals(e.Data.GetData(typeof(SingleAppModel)), draggedApp);
            dropCell = null;
            if (activeGhost != null) activeGhost.Target = null;
            if (valid)
            {
                var point = e.GetPosition(panel!);
                int column = (int)(point.X / SpatialAppPanel.CellWidth), row = (int)(point.Y / SpatialAppPanel.CellHeight);
                valid = point.X >= 0 && point.Y >= 0 && column < panel!.Columns && point.Y < panel.ActualHeight && !panel.Occupied(column, row, draggedApp!);
                if (valid)
                {
                    dropCell = (column, row);
                    var position = panel!.TranslatePoint(new Point(column * SpatialAppPanel.CellWidth, row * SpatialAppPanel.CellHeight), DashboardRoot);
                    if (activeGhost != null) activeGhost.Target = new Rect(position, new Size(90, 108));
                }
            }
            activeGhost?.InvalidateVisual();
            e.Effects = valid ? DragDropEffects.Move : DragDropEffects.None;
            e.Handled = true;
            return valid;
        }
        private void Apps_DragOver(object sender, DragEventArgs e) { if (movingGroup == null) UpdateDrop((ItemsControl)sender, e); }
        private void Apps_Drop(object sender, DragEventArgs e) { if (movingGroup == null) UpdateDrop((ItemsControl)sender, e); }
    }
}
