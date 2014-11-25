using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Geometrie
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const double _pointHeight = 6;
        const double _pointWidth = 6;
        const double _lineThickness = 0.5;
        Point _center;
        Polygon poly;
        Point _point;
        int _timpAnimatie;
        int _indexRes;
        PointCollection _pointCollection;
        Queue<UIElement> _toAnimate;
        Queue<UIElement> _outQueue;
        DispatcherTimer timer;
        int i;
        bool calculate;
        Ellipse _toFind;
        public MainWindow()
        {
            InitializeComponent();
            _center = new Point();
            poly = new Polygon();
            _point = new Point();

            _timpAnimatie = 30;
            _indexRes = 0;

            poly.Stroke = Brushes.Black;
            poly.StrokeThickness = _lineThickness;
            poly.Fill = Brushes.Yellow;
            _pointCollection = new PointCollection();
            _toAnimate = new Queue<UIElement>();
            _outQueue = new Queue<UIElement>();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(300);
            timer.Tick += timer_Tick;
            i = 0;
            calculate = false;
            timer.Start();
            _toFind = new Ellipse();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            if (_toAnimate.Count > 0)
            {
                AnimateIn();
            }
            else
            {
                if (!ok)
                {
                    if (calculate)
                    {
                        if (i < _pointCollection.Count() - 1)
                        {
                            while (_outQueue.Count != 0)
                            {
                                _canvas.Children.Remove(_outQueue.Dequeue());
                            }
                            IsInsidePolygon(i++, (i == _pointCollection.Count - 2));
                        }
                        else
                        {
                            MessageBox.Show("Punctul nu a fost gasit in interiorul poligonului.");
                            timer.Stop();
                        }
                    }
                }
                else
                {
                    while (_outQueue.Count > 3)
                    {
                        _canvas.Children.Remove(_outQueue.Dequeue());
                    }
                    MessageBox.Show("Punctul a fost gasit in interiorul poligonului.");
                    timer.Stop();
                    
                }
                if (!ok)
                {

                    while (_outQueue.Count!=0)
                    {
                        _canvas.Children.Remove(_outQueue.Dequeue());
                    }
                }
            }
        }

        void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _center = new Point(_canvas.ActualWidth / 2, _canvas.ActualHeight / 2);
        }

        public void AnimateIn(double toOpactity = 100)
        {
            UIElement control = _toAnimate.Dequeue();
            if (_canvas.Children.Contains(control))
            {
                return;
            }
            control.Opacity = 0;
            if (_canvas.Children.Contains(_toFind))
            {
                _canvas.Children.Insert(_canvas.Children.IndexOf(_toFind) - 1, control);
            }
            else
            {
                _canvas.Children.Add(control);
            }
            if (control is Polygon)
            {
                if ((control as Polygon).Tag != null)
                {
                    toOpactity = int.Parse((control as Polygon).Tag.ToString());
                    _outQueue.Enqueue(control);
                }
            }
            DoubleAnimation doubleAnimation = new DoubleAnimation(0, toOpactity, new Duration(new TimeSpan(0, 0, 0, _timpAnimatie)), FillBehavior.HoldEnd);

            Storyboard sb = new Storyboard();
            LayoutRoot.Resources.Add(_indexRes++, sb);
            sb.Children.Add(doubleAnimation);
            Storyboard.SetTarget(doubleAnimation, control);
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath("(UIElement.Opacity)"));

            sb.Begin(this);
            //control.BeginAnimation(UIElement.OpacityProperty, doubleAnimation);

            //Thread.Sleep(500);
            //control.BeginAnimation(UIElement.OpacityProperty, doubleAnimation)
            //            trigger.WaitOne();



        }

        public void AnimateOut(double toOpactity = 100)
        {
            UIElement control = _outQueue.Dequeue();
            if (!_canvas.Children.Contains(control))
            {
                return;
            }

            if (control is Polygon)
            {
                if ((control as Polygon).Tag != null)
                    toOpactity = int.Parse((control as Polygon).Tag.ToString());
            }
            DoubleAnimation doubleAnimation = new DoubleAnimation(toOpactity, 0, new Duration(new TimeSpan(0, 0, 0, _timpAnimatie)), FillBehavior.HoldEnd);

            Storyboard sb = new Storyboard();
            LayoutRoot.Resources.Add(_indexRes++, sb);
            sb.Children.Add(doubleAnimation);
            Storyboard.SetTarget(doubleAnimation, control);
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath("(UIElement.Opacity)"));


            sb.Begin(this);
            //control.BeginAnimation(UIElement.OpacityProperty, doubleAnimation);

            //Thread.Sleep(500);
            //control.BeginAnimation(UIElement.OpacityProperty, doubleAnimation)
            //            trigger.WaitOne();



        }

        public void CalculatePoly()
        {

            PointCollection coll = new PointCollection();
            StreamReader f = new StreamReader(Directory.GetCurrentDirectory() + "\\input.in");
            int n = int.Parse(f.ReadLine());
            while (n > 0)
            {
                Point p = new Point();
                string[] splited = f.ReadLine().Split(new char[] { ' ' });
                p.X = double.Parse(splited[0]) * 10;
                p.Y = double.Parse(splited[1]) * 10;
                _pointCollection.Add(p);
                _toAnimate.Enqueue(CreatePoint(p.X, p.Y));
                Centerize(ref p);
                coll.Add(p);
                n--;

            }
            var sp = f.ReadLine().Split(new char[] { ' ' });
            _point.X = double.Parse(sp[0]) * 10;
            _point.Y = double.Parse(sp[1]) * 10;

            poly.Points = coll;
        }

        private void btnDraw_Click(object sender, RoutedEventArgs e)
        {
            CalculatePoly();
            DrawPolygon();
        }

        private void DrawPolygon()
        {
            _toAnimate.Enqueue(poly);
            int len = poly.Points.Count / 2;
            Ellipse p = CreatePoint((_pointCollection[0].X + _pointCollection[len].X) / 2, (_pointCollection[0].Y + _pointCollection[len].Y) / 2);
            _toAnimate.Enqueue(p);// MessageBox.Show((_pointCollection[0].X + _pointCollection[len].X) / 2 + " " + (_pointCollection[0].Y + _pointCollection[len].Y)/2));
        }

        public void Centerize(ref Point p)
        {
            p.X = _center.X + (p.X);
            p.Y = _center.Y - (p.Y);
        }

        private void btnPoint_Click(object sender, RoutedEventArgs e)
        {
            _toFind = CreatePoint(_point.X, _point.Y);
            _toAnimate.Enqueue(_toFind);
        }

        double arie(Point A, Point B, Point C) // functia pentru arie( de fapt, calculeaza aria *2 (pt ca formula de arie era cu rezultatul returnat de functie / 2, dar am facut eu niste smecherii si e bine)
        {
            return Math.Abs(B.X * C.Y + C.X * A.Y + A.X * B.Y - A.Y * B.X - B.Y * C.X - C.Y * A.X);
        }

        bool Ecuatie(Point A, Point B, Point C)
        {
            if ((A.X == B.X && C.X == B.X) || (A.Y == B.Y && C.Y == B.Y)) return true;
            if ((A.X == B.X && C.X != B.X) || (A.Y == B.Y && C.Y != B.Y)) return false;
            int a, b, c;
            b = (int)(-B.Y + A.Y);
            a = (int)(B.X - A.X);
            c = (int)(-A.Y * (B.X - A.X) + A.X * (B.Y - A.Y));
            if (a * C.X + b * C.X + c == 0) return true;
            return false;
        }

        public Ellipse CreatePoint(double x, double y)
        {
            Ellipse myEllipse = new Ellipse();
            SolidColorBrush mySolidColorBrush = new SolidColorBrush();
            mySolidColorBrush.Color = Colors.Blue;
            myEllipse.Fill = mySolidColorBrush;
            myEllipse.StrokeThickness = 0;
            myEllipse.Stroke = Brushes.White;
            myEllipse.Width = _pointWidth;
            myEllipse.Height = _pointHeight;
            Canvas.SetTop(myEllipse, _center.X - (_pointHeight / 2) - y);
            Canvas.SetLeft(myEllipse, _center.Y - (_pointHeight / 2) + x);
            return myEllipse;
        }

        private Polygon CreateTriangle(Point A, Point B, Point C, Color color)
        {
            SolidColorBrush culoare = new SolidColorBrush(color);
            Polygon polygon = new Polygon();
            polygon.Name = "sadas";
            Point A1 = new Point(A.X, A.Y);
            Point B1 = new Point(B.X, B.Y);
            Point C1 = new Point(C.X, C.Y);
            PointCollection coll = new PointCollection();

            Centerize(ref A1);
            Centerize(ref B1);
            Centerize(ref C1);

            coll.Add(A1);
            coll.Add(B1);
            coll.Add(C1);

            polygon.StrokeThickness = _lineThickness;
            polygon.Fill = culoare;
            polygon.Stroke = culoare;

            polygon.Points = coll;

            return polygon;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            calculate = true;
        }

        bool ok = false;
        private void IsInsidePolygon(int i, bool last = false)
        {
            // ok=false => Q nu apartine interiorului, ok=true => Q apartine inetriorului sau se afla pe laturile poligonului
            int n = _pointCollection.Count;
            Point M = new Point((_pointCollection[0].X + _pointCollection[n / 2].X) / 2, (_pointCollection[0].Y + _pointCollection[n / 2].Y) / 2);
            if (!last)
            {
                if (Ecuatie(_pointCollection[i], _pointCollection[i + 1], _point))
                {
                    ok = true;
                    // g << "Punctul se afla pe latura P" << i << "P" << i + 1; //  verifica daca Q apartine dreptei PiP(i+1)
                }
                else
                {
                    Point P1 = _pointCollection[i + 1];
                    Point P2 = _pointCollection[i];
                    Polygon triunghi1, triunghi2, triunghi3, triunghiTotal;

                    triunghiTotal = CreateTriangle(M, P1, P2, Colors.Green);
                    triunghi1 = CreateTriangle(M, _point, P2, Colors.Red);
                    triunghi2 = CreateTriangle(M, _point, P1, Colors.Gray);
                    triunghi3 = CreateTriangle(_point, P1, P2, Colors.Magenta);

                    triunghiTotal.Tag = 50;
                    triunghi1.Tag = 50;
                    triunghi2.Tag = 50;
                    triunghi3.Tag = 50;

                    _toAnimate.Enqueue(triunghiTotal);
                    _toAnimate.Enqueue(triunghi1);
                    _toAnimate.Enqueue(triunghi2);
                    _toAnimate.Enqueue(triunghi3);

                    if (arie(M, P2, P1) == arie(M, _point, P2) + arie(M, _point, P1) + arie(_point, P2, P1)) // verifica daca Q apartine interiorului triunghiului MPiP(i+1)
                    {
                        ok = true;
                        //timer.Stop();
                        //while (_toAnimate.Count != 0)
                        //{
                        //    AnimateIn();
                        //}
                        //MessageBox.Show("Punctul se afla in interiorul poligonului !");
                    }
                }
            }
            else
            {

                if (!ok) //  testez ultimul triunghi P(n-1)P0M - l-am pus aici sa nu mai complic forul - e aceeasi chestie ca in for doar ca e personalizat si am scos break-urile
                {
                    Point P1 = _pointCollection[n - 1];
                    Point P2 = _pointCollection[0];
                    Polygon triunghi1, triunghi2, triunghi3, triunghiTotal;

                    triunghiTotal = CreateTriangle(M, P1, P2, Colors.Green);
                    triunghi1 = CreateTriangle(M, _point, P2, Colors.Red);
                    triunghi2 = CreateTriangle(M, _point, P1, Colors.Gray);
                    triunghi3 = CreateTriangle(_point, P1, P2, Colors.Magenta);

                    triunghiTotal.Tag = 50;
                    triunghi1.Tag = 50;
                    triunghi2.Tag = 50;
                    triunghi3.Tag = 50;

                    _toAnimate.Enqueue(triunghiTotal);
                    _toAnimate.Enqueue(triunghi1);
                    _toAnimate.Enqueue(triunghi2);
                    _toAnimate.Enqueue(triunghi3);

                    if (Ecuatie(P1, P2, _point))
                    {
                        ok = true;
                        //timer.Stop();
                        //while (_toAnimate.Count != 0)
                        //{
                        //    AnimateIn();
                        //}
                        //MessageBox.Show("Punctul se afla in interiorul poligonului !");
                    }
                    else
                    {
                        if (arie(M, P1, P2) == arie(M, _point, P1) + arie(M, _point, P2) + arie(_point, P1, P2))
                        {
                            ok = true;
                            //            timer.Stop();
                            //            while (_toAnimate.Count != 0)
                            //            {
                            //                AnimateIn();
                            //            }
                            //            MessageBox.Show("Punctul se afla in interiorul poligonului !");

                        }
                    }
                }
            }
        }

        private void btnAbout_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow aw = new AboutWindow();
            aw.ShowDialog();
        }
    }
}
