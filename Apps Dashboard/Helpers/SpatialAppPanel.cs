using Apps_Dashboard.Models;
using System.Windows;
using System.Windows.Controls;
namespace Apps_Dashboard.Helpers
{
    public class SpatialAppPanel : Panel
    {
        public const double CellWidth=94,CellHeight=99;
        public bool ExtraRow{get;set;}
        public int Columns{get;private set;}=1;
        public double RowHeight{get;private set;}=CellHeight;
        private static SingleAppModel? Item(UIElement child)=>(child as FrameworkElement)?.DataContext as SingleAppModel;
        protected override Size MeasureOverride(Size available)
        {
            var used=new HashSet<(int,int)>(); int next=0,maxColumn=0,maxRow=0; RowHeight=CellHeight;
            foreach(UIElement child in InternalChildren)
            {
                var app=Item(child);if(app==null)continue;
                int c=Math.Max(0,app.LayoutColumn??next),r=Math.Max(0,app.LayoutRow??0);
                while(used.Contains((c,r)))c++;
                app.LayoutColumn=c;app.LayoutRow=r;used.Add((c,r));next=c+1;
                maxColumn=Math.Max(maxColumn,c);maxRow=Math.Max(maxRow,r);
                child.Measure(new Size(CellWidth,double.PositiveInfinity));
                RowHeight=Math.Max(RowHeight,child.DesiredSize.Height);
            }
            Columns=maxColumn+1+(ExtraRow?1:0);
            return new Size(Columns*CellWidth,(maxRow+1+(ExtraRow?1:0))*RowHeight);
        }
        protected override Size ArrangeOverride(Size final)
        {
            foreach(UIElement child in InternalChildren)
                if(Item(child)is SingleAppModel app)child.Arrange(new Rect((app.LayoutColumn??0)*CellWidth,(app.LayoutRow??0)*RowHeight,CellWidth,RowHeight));
            return final;
        }
        public bool Occupied(int column,int row,SingleAppModel source)=>InternalChildren.Cast<UIElement>().Select(Item).Any(a=>a!=null && a!=source && a.LayoutColumn==column && a.LayoutRow==row);
    }
}
