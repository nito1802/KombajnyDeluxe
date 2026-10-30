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
        private static SingleAppModel? Item(UIElement child) => (child as FrameworkElement)?.DataContext as SingleAppModel;
        protected override Size MeasureOverride(Size available)
        {
            Columns = Math.Max(1, (int)(double.IsInfinity(available.Width) ? 13 : available.Width / CellWidth));
            var used = new HashSet<(int, int)>();
            foreach (UIElement child in InternalChildren)
            {
                var app = Item(child);
                if (app?.LayoutColumn is int c && app.LayoutRow is int r) used.Add((c, r));
            }
            int next = 0, rows = 1;
            foreach (UIElement child in InternalChildren)
            {
                var app = Item(child);
                if (app == null) continue;
                if (app.LayoutColumn == null || app.LayoutRow == null)
                {
                    while (used.Contains((next % Columns, next / Columns))) next++;
                    app.LayoutColumn = next % Columns; app.LayoutRow = next / Columns;
                    used.Add((app.LayoutColumn.Value, app.LayoutRow.Value));
                }
                rows = Math.Max(rows, app.LayoutRow.Value + 1);
                child.Measure(new Size(CellWidth, CellHeight));
            }
            return new Size(double.IsInfinity(available.Width) ? Columns * CellWidth : available.Width, (rows + (ExtraRow ? 1 : 0)) * CellHeight);
        }
        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (UIElement child in InternalChildren)
            {
                var app = Item(child);
                if (app == null) continue;
                child.Arrange(new Rect((app.LayoutColumn ?? 0) * CellWidth, (app.LayoutRow ?? 0) * CellHeight, CellWidth, CellHeight));
            }
            return finalSize;
        }
        public bool Occupied(int column, int row, SingleAppModel source) => InternalChildren.Cast<UIElement>().Select(Item).Any(a => a != null && a != source && a.LayoutColumn == column && a.LayoutRow == row);
    }
}
