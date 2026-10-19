using System;
using System.Collections.Generic;
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
            RefreshGroup(group);
        }

        private void BeginTileDrag(Button handle, Grid tile, NormalButton item)
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
            var ghost = new Popup {
                PlacementTarget = this, Placement = PlacementMode.Relative,
                AllowsTransparency = true, IsHitTestVisible = false,
                Child = new Border {
                    Opacity = 0.72, CornerRadius = new CornerRadius(7),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(120,167,234)),
                    BorderThickness = new Thickness(1),
                    Child = new Image { Source = bitmap, Width = tile.ActualWidth, Height = tile.ActualHeight }
                }
            };
            DragEventHandler follow = (_, args) => {
                var point = args.GetPosition(this);
                ghost.HorizontalOffset = point.X + 18;
                ghost.VerticalOffset = point.Y + 18;
            };
            GiveFeedbackEventHandler feedback = (_, args) => {
                args.UseDefaultCursors = false;
                Mouse.SetCursor(args.Effects == DragDropEffects.None ? Cursors.No : Cursors.SizeAll);
                args.Handled = true;
            };
            draggedItem = item;
            previewTarget = null;
            tile.Opacity = 0.25;
            var initial = Mouse.GetPosition(this);
            ghost.HorizontalOffset = initial.X + 18;
            ghost.VerticalOffset = initial.Y + 18;
            AddHandler(DragDrop.PreviewDragOverEvent, follow, true);
            handle.GiveFeedback += feedback;
            ghost.IsOpen = true;
            bool committed = false;
            try
            {
                committed = DragDrop.DoDragDrop(handle, new DataObject(typeof(NormalButton), item), DragDropEffects.Move) == DragDropEffects.Move;
            }
            finally
            {
                ghost.IsOpen = false;
                RemoveHandler(DragDrop.PreviewDragOverEvent, follow);
                handle.GiveFeedback -= feedback;
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
