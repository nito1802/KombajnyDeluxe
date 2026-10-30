using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Apps_Dashboard.Controls
{
    public sealed class GroupColorPicker : StackPanel
    {
        private readonly Grid canvas = new() { Height = 115, ClipToBounds = true, Margin = new Thickness(0,8,0,6) };
        private readonly Rectangle hueSurface = new();
        private readonly Ellipse marker = new() { Width=12, Height=12, Stroke=Brushes.White, StrokeThickness=2, Fill=Brushes.Transparent, HorizontalAlignment=HorizontalAlignment.Left, VerticalAlignment=VerticalAlignment.Top, IsHitTestVisible=false };
        private readonly Slider hue = new() { Minimum=0, Maximum=360, Margin=new Thickness(0,3,0,3) };
        private readonly Border preview = new() { Height=18, CornerRadius=new CornerRadius(4), Margin=new Thickness(0,4,0,4) };
        private double saturation=1, brightness=1;
        private bool updating;
        public Color SelectedColor { get; private set; }
        public GroupColorPicker(Color initial)
        {
            var palette=new WrapPanel(); Children.Add(palette);
            foreach(var hex in new[]{"#C99045","#EA7272","#E99AC4","#AF8AE8","#748EE8","#62B5E5","#54C5BC","#7CC98A","#C6CC70","#A8B2C1"})
            {
                var c=(Color)ColorConverter.ConvertFromString(hex);
                var b=new Button{Width=29,Height=25,Margin=new Thickness(1),Background=new SolidColorBrush(c),BorderThickness=new Thickness(0)};
                b.Click+=(_,_)=>SetColor(c); palette.Children.Add(b);
            }
            canvas.Children.Add(hueSurface);
            canvas.Children.Add(new Rectangle { Fill=new LinearGradientBrush(Colors.White,Color.FromArgb(0,255,255,255),0) });
            canvas.Children.Add(new Rectangle { Fill=new LinearGradientBrush(Colors.Transparent,Colors.Black,90) });
            canvas.Children.Add(marker); Children.Add(canvas);
            var rainbow=new LinearGradientBrush { StartPoint=new Point(0,.5), EndPoint=new Point(1,.5) };
            for(int i=0;i<=6;i++) rainbow.GradientStops.Add(new GradientStop(FromHsv(i*60,1,1),i/6d));
            Children.Add(new Border { Height=8, Background=rainbow, CornerRadius=new CornerRadius(3) });
            Children.Add(hue); Children.Add(preview);
            canvas.MouseLeftButtonDown+=(_,e)=> { canvas.CaptureMouse(); Pick(e.GetPosition(canvas)); e.Handled=true; };
            canvas.MouseMove+=(_,e)=> { if(canvas.IsMouseCaptured && e.LeftButton==MouseButtonState.Pressed) Pick(e.GetPosition(canvas)); };
            canvas.MouseLeftButtonUp+=(_,_)=>canvas.ReleaseMouseCapture();
            canvas.SizeChanged+=(_,_)=>Refresh();
            hue.ValueChanged+=(_,_)=> { if(!updating) Refresh(); };
            SetColor(initial);
        }
        private void Pick(Point p) { saturation=Math.Clamp(p.X/Math.Max(1,canvas.ActualWidth),0,1); brightness=1-Math.Clamp(p.Y/Math.Max(1,canvas.ActualHeight),0,1); Refresh(); }
        private void SetColor(Color c)
        {
            double r=c.R/255d,g=c.G/255d,b=c.B/255d,max=Math.Max(r,Math.Max(g,b)),min=Math.Min(r,Math.Min(g,b)),delta=max-min;
            double h=delta==0?0:max==r?60*((g-b)/delta%6):max==g?60*((b-r)/delta+2):60*((r-g)/delta+4);
            updating=true; hue.Value=(h+360)%360; updating=false;
            saturation=max==0?0:delta/max; brightness=max; Refresh();
        }
        private void Refresh()
        {
            SelectedColor=FromHsv(hue.Value,saturation,brightness);
            hueSurface.Fill=new SolidColorBrush(FromHsv(hue.Value,1,1)); preview.Background=new SolidColorBrush(SelectedColor);
            marker.Margin=new Thickness(Math.Max(0,saturation*canvas.ActualWidth-6),Math.Max(0,(1-brightness)*canvas.ActualHeight-6),0,0);
        }
        public static Color FromHsv(double hue,double saturation,double value)
        {
            double c=value*saturation,h=(hue%360)/60,x=c*(1-Math.Abs(h%2-1)),m=value-c;
            var (r,g,b)=h switch { <1=>(c,x,0d), <2=>(x,c,0d), <3=>(0d,c,x), <4=>(0d,x,c), <5=>(x,0d,c), _=>(c,0d,x) };
            return Color.FromRgb((byte)Math.Round((r+m)*255),(byte)Math.Round((g+m)*255),(byte)Math.Round((b+m)*255));
        }
    }
}
