using Apps_Dashboard.Models;
using System.Windows;
using System.Windows.Controls;

namespace Apps_Dashboard.Helpers
{
    public sealed class SpatialGroupsPanel : Panel
    {
        public const double Snap=24;
        private static AppsGroupModel? Item(UIElement child)=>(child as FrameworkElement)?.DataContext as AppsGroupModel;
        protected override Size MeasureOverride(Size available)
        {
            double width=double.IsInfinity(available.Width)?1300:available.Width;
            var occupied=new List<Rect>();
            foreach(UIElement child in InternalChildren)
            {
                child.Measure(new Size(double.PositiveInfinity,double.PositiveInfinity));
                if(Item(child) is {LayoutX: not null,LayoutY: not null} group)
                    occupied.Add(new Rect(new Point(Math.Max(0,group.LayoutX.Value),Math.Max(0,group.LayoutY.Value)),child.DesiredSize));
            }
            foreach(UIElement child in InternalChildren)
            {
                if(Item(child) is not AppsGroupModel group) continue;
                if(group.LayoutX==null || group.LayoutY==null)
                {
                    bool placed=false;
                    for(double y=0;!placed;y+=Snap)
                    for(double x=0;x<=Math.Max(0,width-child.DesiredSize.Width);x+=Snap)
                    {
                        var rect=new Rect(new Point(x,y),child.DesiredSize);
                        if(occupied.Any(o=>o.IntersectsWith(rect))) continue;
                        group.LayoutX=x;group.LayoutY=y;occupied.Add(rect);placed=true;break;
                    }
                }
            }
            double right=occupied.Count==0?0:occupied.Max(r=>r.Right), bottom=occupied.Count==0?0:occupied.Max(r=>r.Bottom);
            return new Size(Math.Max(width,right),Math.Max(700,bottom+120));
        }
        protected override Size ArrangeOverride(Size final)
        {
            foreach(UIElement child in InternalChildren)
                if(Item(child) is AppsGroupModel group) child.Arrange(new Rect(new Point(Math.Max(0,group.LayoutX??0),Math.Max(0,group.LayoutY??0)),child.DesiredSize));
            return final;
        }
        public bool CanPlace(AppsGroupModel source,Rect target)
        {
            foreach(UIElement child in InternalChildren)
                if(Item(child) is AppsGroupModel group && group!=source &&
                    target.IntersectsWith(new Rect(new Point(group.LayoutX??0,group.LayoutY??0),child.DesiredSize))) return false;
            return true;
        }
    }
}
