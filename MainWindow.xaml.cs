using System;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace TrabalhoC1
{
    public partial class MainWindow : Window
    {
        private const double StageW = 1000;
        private const double StageH = 420;

        private const double WheelRadius = 40.0;   // roda 80x80
        private const double CrankRadius = 20.0;
        private const double WheelAngularPeriodSec = 2.0;

        private const double RailTopY = 354;

        private DateTime _t0;
        private Storyboard? _moveStoryboard;
        private Storyboard? _rotStoryboard;

        private readonly DispatcherTimer _smokeTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(160) };
        private readonly Random _rng = new Random();

        public MainWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            SizeChanged += (_, __) => RebuildAnimations(); // Viewbox cuida do scale; só refaz range.
        }

        private void OnLoaded(object? sender, RoutedEventArgs e)
        {
            DrawSleepers();
            AlignLocoOnTopRail();   // roda encostando no trilho
            BuildRotationStoryboard();
            BuildMoveStoryboard();
            CompositionTarget.Rendering += OnFrame;

            _smokeTimer.Tick += (_, __) => EmitSmoke();
            _smokeTimer.Start();
        }

        private void DrawSleepers()
        {
            TrackCanvas.Children.Clear();

            var sleeperBrush = new SolidColorBrush(Color.FromRgb(0x6B, 0x5B, 0x53));
            double spacing = 24;
            double sleeperWidth = 20;
            double sleeperHeight = 50;
            double topMargin = RailTopY - 4 - 50; 

            for (double x = 0; x < StageW + spacing; x += spacing)
            {
                var sleeper = new Rectangle
                {
                    Width = sleeperWidth,
                    Height = sleeperHeight,
                    RadiusX = 3,
                    RadiusY = 3,
                    Fill = sleeperBrush
                };
                Canvas.SetLeft(sleeper, x);
                Canvas.SetTop(sleeper, topMargin);
                TrackCanvas.Children.Add(sleeper);
            }
        }

        private void AlignLocoOnTopRail()
        {
            double wheelBottomLocal = 100 + (WheelRadius * 2); // 180
            double desiredTop = RailTopY - wheelBottomLocal;

            Canvas.SetTop(LocoCanvas, desiredTop);
        }

        private void BuildRotationStoryboard()
        {
            _rotStoryboard?.Stop();
            _rotStoryboard = new Storyboard();

            var rot1 = new DoubleAnimation { From = 0, To = 360, Duration = TimeSpan.FromSeconds(WheelAngularPeriodSec), RepeatBehavior = RepeatBehavior.Forever };
            var rot2 = new DoubleAnimation { From = 180, To = 540, Duration = TimeSpan.FromSeconds(WheelAngularPeriodSec), RepeatBehavior = RepeatBehavior.Forever };

            Storyboard.SetTarget(rot1, Wheel1Rotate);
            Storyboard.SetTargetProperty(rot1, new PropertyPath(RotateTransform.AngleProperty));
            Storyboard.SetTarget(rot2, Wheel2Rotate);
            Storyboard.SetTargetProperty(rot2, new PropertyPath(RotateTransform.AngleProperty));

            _rotStoryboard.Children.Add(rot1);
            _rotStoryboard.Children.Add(rot2);
            _rotStoryboard.Begin(this, true);
        }

        private void BuildMoveStoryboard()
        {
            _moveStoryboard?.Stop();
            _moveStoryboard = new Storyboard();

            double margin = 40;
            double maxX = Math.Max(0, StageW - LocoCanvas.Width - margin * 2);

            var move = new DoubleAnimation
            {
                From = margin,
                To = margin + maxX,
                Duration = TimeSpan.FromSeconds(6),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };

            Storyboard.SetTarget(move, LocoCanvas);
            Storyboard.SetTargetProperty(move, new PropertyPath("(Canvas.Left)"));

            _moveStoryboard.Children.Add(move);
            _moveStoryboard.Begin(this, true);
        }

        private void RebuildAnimations()
        {
            BuildMoveStoryboard();
        }

        private void OnFrame(object? sender, EventArgs e)
        {
            var t = (DateTime.Now - _t0).TotalSeconds;
            var angle1 = 2 * Math.PI * (t / WheelAngularPeriodSec);
            var angle2 = angle1 + Math.PI;

            var c1 = new Point(Canvas.GetLeft(Wheel1) + WheelRadius, Canvas.GetTop(Wheel1) + WheelRadius);
            var c2 = new Point(Canvas.GetLeft(Wheel2) + WheelRadius, Canvas.GetTop(Wheel2) + WheelRadius);

            var p1 = new Point(c1.X + CrankRadius * Math.Cos(angle1), c1.Y + CrankRadius * Math.Sin(angle1));
            var p2 = new Point(c2.X + CrankRadius * Math.Cos(angle2), c2.Y + CrankRadius * Math.Sin(angle2));

            SideRod.X1 = p1.X; SideRod.Y1 = p1.Y;
            SideRod.X2 = p2.X; SideRod.Y2 = p2.Y;

            CrankArm1.X1 = c1.X; CrankArm1.Y1 = c1.Y;
            CrankArm1.X2 = p1.X; CrankArm1.Y2 = p1.Y;

            CrankArm2.X1 = c2.X; CrankArm2.Y1 = c2.Y;
            CrankArm2.X2 = p2.X; CrankArm2.Y2 = p2.Y;
        }

        private void EmitSmoke()
        {
            var chimneyLocal = new Point(80, 10);
            var toStage = LocoCanvas.TransformToVisual(EffectsLayer);
            var p = toStage.Transform(chimneyLocal);

            double size = 10 + _rng.NextDouble() * 10;
            var puff = new Ellipse
            {
                Width = size,
                Height = size,
                Fill = new SolidColorBrush(Color.FromArgb(180, 255, 255, 255)),
                Stroke = new SolidColorBrush(Color.FromArgb(120, 200, 200, 200)),
                StrokeThickness = 1,
                RenderTransformOrigin = new Point(0.5, 0.5)
            };

            var tg = new TransformGroup();
            var tt = new TranslateTransform(p.X - size / 2, p.Y - size / 2);
            var st = new ScaleTransform(1, 1);
            tg.Children.Add(st);
            tg.Children.Add(tt);
            puff.RenderTransform = tg;

            EffectsLayer.Children.Add(puff);

            double driftX = (_rng.NextDouble() - 0.5) * 40;
            double rise = 70 + _rng.NextDouble() * 40;
            double dur = 1.4 + _rng.NextDouble() * 0.6;

            var animX = new DoubleAnimation(tt.X, tt.X + driftX, TimeSpan.FromSeconds(dur));
            var animY = new DoubleAnimation(tt.Y, tt.Y - rise, TimeSpan.FromSeconds(dur));
            var animScale = new DoubleAnimation(1, 1.8, TimeSpan.FromSeconds(dur));
            var animOpacity = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(dur));
            animOpacity.Completed += (_, __) => EffectsLayer.Children.Remove(puff);

            puff.BeginAnimation(OpacityProperty, animOpacity);
            st.BeginAnimation(ScaleTransform.ScaleXProperty, animScale);
            st.BeginAnimation(ScaleTransform.ScaleYProperty, animScale);
            tt.BeginAnimation(TranslateTransform.XProperty, animX);
            tt.BeginAnimation(TranslateTransform.YProperty, animY);
        }
    }
}
