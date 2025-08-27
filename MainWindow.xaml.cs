using System;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Controls;
using System.Windows.Media;

namespace TrabalhoC1
{
    public partial class MainWindow : Window
    {
        private const double WheelRadius = 40.0;
        private const double CrankRadius = 20.0;
        private const double WheelAngularPeriodSec = 2.0;

        private DateTime _t0;
        private Storyboard? _moveStoryboard;
        private Storyboard? _rotStoryboard;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            SizeChanged += (_, __) => BuildMoveStoryboard();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _t0 = DateTime.Now;
            BuildRotationStoryboard();
            BuildMoveStoryboard();
            CompositionTarget.Rendering += OnFrame;
        }

        private void BuildRotationStoryboard()
        {
            _rotStoryboard?.Stop();
            _rotStoryboard = new Storyboard();

            var rot1 = new DoubleAnimation
            {
                From = 0,
                To = 360,
                Duration = TimeSpan.FromSeconds(WheelAngularPeriodSec),
                RepeatBehavior = RepeatBehavior.Forever
            };
            var rot2 = new DoubleAnimation
            {
                From = 180,
                To = 540,
                Duration = TimeSpan.FromSeconds(WheelAngularPeriodSec),
                RepeatBehavior = RepeatBehavior.Forever
            };

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

            const double margin = 20;
            var maxX = Math.Max(0, ActualWidth - LocoCanvas.Width - margin * 2);

            var move = new DoubleAnimation
            {
                From = margin,
                To = margin + maxX,
                Duration = TimeSpan.FromSeconds(6),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };

            Storyboard.SetTarget(move, LocoTranslate);
            Storyboard.SetTargetProperty(move, new PropertyPath(TranslateTransform.XProperty));

            _moveStoryboard.Children.Add(move);
            _moveStoryboard.Begin(this, true);
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

            Canvas.SetLeft(Pin1, p1.X - Pin1.Width / 2);
            Canvas.SetTop(Pin1, p1.Y - Pin1.Height / 2);
            Canvas.SetLeft(Pin2, p2.X - Pin2.Width / 2);
            Canvas.SetTop(Pin2, p2.Y - Pin2.Height / 2);
        }
    }
}
