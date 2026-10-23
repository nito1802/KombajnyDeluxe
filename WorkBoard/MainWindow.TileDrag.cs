using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Documents;
using System.Runtime.InteropServices;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WorkBoard.Models;

namespace KombajnDoPracy
{
    public partial class MainWindow
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct DragCursorPoint { public int X; public int Y; }
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out DragCursorPoint point);

        private sealed class TileDragAdorner : Adorner
        {
            private readonly ImageSource snapshot;
            private readonly Size size;
            public Point Position { get; set; }
            public TileDragAdorner(UIElement owner, ImageSource snapshot, Size size) : base(owner)
            {
                this.snapshot = snapshot; this.size = size;
                IsHitTestVisible = false; Opacity = .72;
            }
            protected override void OnRender(DrawingContext context)
            {
                context.DrawImage(snapshot, new Rect(Position, size));
            }
        }

        private ObservableCollection<NormalButton>? dragItems;
        private NormalButton? draggedItem;
        private NormalButton? previewTarget;
        private bool previewAfter;

        private void RefreshGroup(List<NormalButton> group)
        {
            var control = new[] { LeftGroup, MiddleGroup, RightGroup, LinkGroup }
                .First(c => ReferenceEquals(c.ItemsSource, group));
            control.Items.Refresh();
        }

        private void PreviewTilePosition(NormalButton source, NormalButton target, bool after)
        {
            if (previewTarget == target && previewAfter == after) return;
            previewTarget = target;
            previewAfter = after;
            var group = Group(source);
            int current = group.IndexOf(source);
            int index = group.IndexOf(target) + (after ? 1 : 0);
            if (current < index) index--;
            if (current == index) return;
            group.Remove(source);
            group.Insert(index, source);
            if (dragItems != null) dragItems.Move(current, index);
            else RefreshGroup(group);
        }

        private void BeginTileDrag(Button handle, Grid tile, NormalButton item, Point grabPoint)
        {
            var group = Group(item);
            var original = group.ToList();
            var old = LinkState();
            var dpi = VisualTreeHelper.GetDpi(tile);
            var bitmap = new RenderTargetBitmap(
                Math.Max(1, (int)Math.Ceiling(tile.ActualWidth * dpi.DpiScaleX)),
                Math.Max(1, (int)Math.Ceiling(tile.ActualHeight * dpi.DpiScaleY)),
                96 * dpi.DpiScaleX, 96 * dpi.DpiScaleY, PixelFormats.Pbgra32);
            bitmap.Render(tile);
            bitmap.Freeze();
            var layer = AdornerLayer.GetAdornerLayer(mainGrid);
            if (layer == null) return;
            var ghost = new TileDragAdorner(mainGrid, bitmap, new Size(tile.ActualWidth, tile.ActualHeight));
            void FollowCursor(object? sender, EventArgs args)
            {
                if (!GetCursorPos(out var cursor)) return;
                var point = mainGrid.PointFromScreen(new Point(cursor.X, cursor.Y));
                ghost.Position = new Point(point.X - grabPoint.X, point.Y - grabPoint.Y);
                ghost.InvalidateVisual();
            }
            GiveFeedbackEventHandler feedback = (_, args) => {
                args.UseDefaultCursors = false;
                Mouse.SetCursor(args.Effects == DragDropEffects.None ? Cursors.No : Cursors.SizeAll);
                args.Handled = true;
            };
            draggedItem = item;
            previewTarget = null;
            tile.Opacity = 0.25;
            var control = new[] { LeftGroup, MiddleGroup, RightGroup, LinkGroup }
                .First(c => ReferenceEquals(c.ItemsSource, group));
            dragItems = new ObservableCollection<NormalButton>(group);
            control.SetCurrentValue(ItemsControl.ItemsSourceProperty, dragItems);
            FollowCursor(null, EventArgs.Empty);
            layer.Add(ghost);
            CompositionTarget.Rendering += FollowCursor;
            GiveFeedback += feedback;
            bool committed = false;
            try
            {
                committed = DragDrop.DoDragDrop(this, new DataObject(typeof(NormalButton), item), DragDropEffects.Move) == DragDropEffects.Move;
            }
            finally
            {
                CompositionTarget.Rendering -= FollowCursor;
                layer.Remove(ghost);
                GiveFeedback -= feedback;
                dragItems = null;
                control.SetCurrentValue(ItemsControl.ItemsSourceProperty, group);


                draggedItem = null;
                previewTarget = null;
                Mouse.SetCursor(null);
                if (!committed) { group.Clear(); group.AddRange(original); }
                if (committed)
                {
                    if (original.IndexOf(item) != group.IndexOf(item)) item.ModifiedAt = DateTime.Now;
                    Persist(old);
                }
                else RefreshGroup(group);
            }
        }
    }
}
