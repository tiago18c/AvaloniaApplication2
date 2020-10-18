using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using SkiaSharp;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace AvaloniaApplication2
{
    internal class OHLC
    {
        internal float O;
        internal float H;
        internal float L;
        internal float C;
        internal float X;
    }

    internal class CustomDrawOperation : ICustomDrawOperation
    {
        public Rect Bounds { get; set; }

        public UserControl1 control;

        public void Dispose()
        {
        }

        public bool Equals([AllowNull] ICustomDrawOperation other)
        {
            return false;
        }

        public bool HitTest(Point p)
        {
            return false;
        }

        public void Render(IDrawingContextImpl context)
        {
            var canvas = (context as ISkiaDrawingContextImpl)?.SkCanvas;

            var baseColor = new SKColor(200, 200, 200);

            canvas.Clear(baseColor);

            if (control.col == null)
            {
                Random r = new Random();
                control.col = new OHLC[2000];
                for (int x = 0; x < 2000; x++)
                {
                    var h = (float)r.NextDouble() * 500;
                    var l = (float)r.NextDouble() * 500;

                    control.col[x] = new OHLC();

                    control.col[x].H = Math.Max(h, l);
                    control.col[x].L = Math.Min(h, l);
                    control.col[x].C = (float)r.NextDouble() * 500;
                    control.col[x].O = (float)r.NextDouble() * 500;
                    control.col[x].X = (float)x * 10f;
                }
            }

            var sw = System.Diagnostics.Stopwatch.StartNew();

            canvas.Save();

            //canvas.

            //canvas.RotateDegrees(180);

            var c = canvas.GetLocalClipBounds(out SKRect clip);

            clip.ToString();

            canvas.Translate(0, clip.Height);

            canvas.Scale(1, -1);



            var red = new SKPaint(); red.Color = new SKColor(200, 0, 0); red.Style = SKPaintStyle.StrokeAndFill; red.StrokeWidth = 2f;
            var green = new SKPaint(); green.Color = new SKColor(0, 200, 0); green.Style = SKPaintStyle.StrokeAndFill; green.StrokeWidth = 2f;

            SKPaint paint;


            var lineP = new SKPaint();
            lineP.Color = new SKColor(0, 0, 0);
            lineP.Style = SKPaintStyle.Stroke;
            canvas.DrawLine(0, 0, 2000, 0, lineP);


            var mouse = canvas.TotalMatrix.MapPoint(control.MouseCoord.ToSKPoint());
            float lo, hi, lo2, hi2;

            foreach (var item in control.col)
            {
                if (item.C > item.O)
                {
                    paint = green;
                    lo = item.O;
                    hi = item.C;
                }
                else
                {
                    paint = red;
                    lo = item.C;
                    hi = item.O;
                }



                canvas.DrawLine(item.X, item.L, item.X, lo, paint);
                canvas.DrawRect(item.X - 3, lo, 6f, hi - lo, paint);
                canvas.DrawLine(item.X, item.H, item.X, hi, paint);
            }

            var path = new SKPath();


            var pts = new SKPoint[2000];


            for (int i = 0; i < 2000; i++)
            {
                pts[i].Y = control.col[i].C;
                pts[i].X = control.col[i].X;
            }
            path.AddPoly(pts, false);

            var paint3 = new SKPaint();
            paint3.Color = new SKColor(0, 0, 255);
            paint3.StrokeWidth = 1;
            paint3.IsAntialias = true;
            paint3.StrokeCap = SKStrokeCap.Butt;
            paint3.StrokeJoin = SKStrokeJoin.Bevel;
            paint3.Style = SKPaintStyle.Stroke;

            canvas.DrawPath(path, paint3);



            var candle = control.selectedCandle;
            if (candle != null)
            {
                if (candle.C > candle.O)
                {
                    paint = green;
                    lo = candle.O;
                    hi = candle.C;
                }
                else
                {
                    paint = red;
                    lo = candle.C;
                    hi = candle.O;
                }
                paint.Color = paint.Color.WithAlpha(150).WithBlue(100).WithGreen(10).WithRed(10);
                canvas.DrawRect(candle.X - 4, lo, 8f, hi - lo, paint);

            }


            canvas.Restore();

            if(control.IsMouseOver && candle != null)
            {
                var paint4 = new SKPaint();
                paint4.Color = new SKColor(0, 0, 0, 50);
                paint4.StrokeWidth = 2;
                paint4.PathEffect = SKPathEffect.CreateDash(new float[] { 5f, 5f }, 10);
                canvas.DrawLine((float)candle.X, 0, (float)candle.X, 20000, paint4);
                canvas.DrawLine(0, (float)control.MouseCoord.Y, 20000, (float)control.MouseCoord.Y, paint4);
            }

            sw.Stop();

            paint = new SKPaint(); paint.Color = new SKColor(255, 0, 0); paint.Style = SKPaintStyle.StrokeAndFill; paint.TextSize = 20f; paint.Typeface = SKTypeface.Default;



            canvas.DrawText("SW " + (sw.ElapsedTicks / (float)System.Diagnostics.Stopwatch.Frequency * 1000) + "ms", 0f, 20f, paint);
            canvas.DrawText($"Mouse X:{mouse.X} Y:{mouse.Y}", 0f, 40f, paint);
            if (candle != null)
            {
                canvas.DrawText($"Candle X:{candle.X} O:{candle.O} H:{candle.H} L:{candle.L} C:{candle.C}", 0f, 60f, paint);
            }

            //canvas.Restore();
        }
    }

    public class UserControl1 : UserControl
    {
        public UserControl1()
        {
            this.InitializeComponent();

            Task.Run(() =>
            {
                while (true)
                {
                    evt.WaitOne();



                    if (IsMouseOver && col != null)
                    {
                        var pt = MouseCoord.ToSKPoint();
                        int lo = 0, hi = col.Length;

                        while (hi - lo > 1)
                        {
                            if (col[lo + (hi - lo) / 2].X > pt.X) hi = hi - (hi - lo) / 2;
                            else lo = lo + (hi - lo) / 2;
                        }

                        if (Math.Abs(col[hi].X - pt.X) < Math.Abs(col[lo].X - pt.X)) selectedCandle = col[hi];
                        else selectedCandle = col[lo];
                    }
                    else
                        selectedCandle = null;
                }
            });

            width = 0;
            height = 0;
        }

        internal OHLC[] col { get; set; }

        internal Point MouseCoord { get; set; }

        internal bool IsMouseOver { get; set; }

        internal OHLC selectedCandle { get; set; }

        internal AutoResetEvent evt { get; set; }
        internal float width { get; set; }
        internal float height { get; set; }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            selectedCandle = null;
            evt = new AutoResetEvent(false);
            this.PropertyChanged += UserControl1_PropertyChanged;
        }

        private void UserControl1_PropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property.Name == "Bounds")
            {
                width = (float)Bounds.Width;
                height = (float)Bounds.Height;
            }
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);

            var custom = new CustomDrawOperation()
            {
                Bounds = this.Bounds,
                control = this
            };

            context.Custom(custom);


        }

        public void mouseMoved(object source, PointerEventArgs e)
        {
            MouseCoord = e.GetPosition(this);
            evt.Set();
            InvalidateVisual();

        }

        public void mouseEnter(object source, PointerEventArgs e)
        {
            IsMouseOver = true;

        }

        public void mouseLeave(object source, PointerEventArgs e)
        {
            IsMouseOver = false;

        }


    }
}
