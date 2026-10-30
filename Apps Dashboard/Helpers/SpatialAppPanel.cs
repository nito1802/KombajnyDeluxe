using Apps_Dashboard.Models;
using System.Windows;
using System.Windows.Controls;

namespace Apps_Dashboard.Helpers
{
    public class SpatialAppPanel : Panel
    {
        public const double CellWidth = 94;
        public const double CellHeight = 114;
        public bool ExtraRow { get; set; }
        public int Columns { get; private set; } = 1;
        private double height;
        private static SingleAppModel? Item(UIElement child) => (child as FrameworkElement)?.DataContext as SingleAppModel;
        protected override Size MeasureOverride(Size available)
        {
            var used = new HashSet<int>(); int next = 0; height = 0;
            // Flatten old multi-row layouts and resolve collisions without removing deliberate gaps.
            foreach (UIElement child in InternalChildren)
            {
                var app = Item(child); if (app == null) continue;
                int column = Math.Max(0, app.LayoutColumn ?? next);
                while (used.Contains(column)) column++;
                app.LayoutColumn = column; app.LayoutRow = 0; used.Add(column); next = column + 1;
                child.Measure(new Size(CellWidth, double.PositiveInfinity));
                height = Math.Max(height, child.DesiredSize.Height);
            }
            Columns = Math.Max(1, used.Count == 0 ? 1 : used.Max() + 1) + (ExtraRow ? 1 : 0);
            return new Size(Columns * CellWidth, Math.Max(40, height));
        }
        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (UIElement child in InternalChildren)
                if (Item(child) is SingleAppModel app) child.Arrange(new Rect((app.LayoutColumn ?? 0) * CellWidth, 0, CellWidth, height));
            return finalSize;
        }
        public bool Occupied(int column, int row, SingleAppModel source) => row != 0 || InternalChildren.Cast<UIElement>().Select(Item).Any(a => a != null && a != source && a.LayoutColumn == column);
    }
}
