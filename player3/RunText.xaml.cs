using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace player3
{
    /// <summary>
    /// Логика взаимодействия для RunText.xaml
    /// </summary>
    public partial class RunText : Window
    {
        public RunText()
        {
            InitializeComponent();
        }

        DoubleAnimation animation;
        double speed = 10;   // (10 px за 0.1 секунду)

        private void Play()
        {
            // Центрируем строку в канвасе
            Canvas.SetLeft(_runningText, (canvas.ActualWidth - _runningText.ActualWidth) / 2);

            animation = new DoubleAnimation();
            animation.Duration = TimeSpan.FromSeconds(0.08);

            // При завершении анимации, запускаем функцию MyAnimation снова           
            animation.Completed += myanim_Completed;

            MyAnimation(Canvas.GetLeft(_runningText), Canvas.GetLeft(_runningText) - speed);
        }

        private void MyAnimation(double from, double to)
        {
            // Если строка вышла за пределы канваса (отриц. Canvas.Left)
            // то возвращаем с другой стороны           
            if (Canvas.GetLeft(_runningText) + _runningText.ActualWidth <= 0)
            {
                animation.From = canvas.ActualWidth;
                animation.To = canvas.ActualWidth - speed;
                _runningText.BeginAnimation(Canvas.LeftProperty, animation);
            }
            else
            {
                animation.From = from;
                animation.To = to;
                _runningText.BeginAnimation(Canvas.LeftProperty, animation);
            }
        }

        private void myanim_Completed(object sender, EventArgs e)
        {
            MyAnimation(Canvas.GetLeft(_runningText), Canvas.GetLeft(_runningText) - speed);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Play();
        }
    }
}
