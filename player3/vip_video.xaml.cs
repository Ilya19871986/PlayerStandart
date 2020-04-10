using System;
using System.Collections.Generic;
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

namespace player3
{
    public partial class vip_video : Window
    {
        private int current_media_index = 0;
        private List<string> list_vip = new List<string>();
        string cur_dir = AppDomain.CurrentDomain.BaseDirectory + @"content\";

        public vip_video(List<string> s)
        {
            InitializeComponent();

            list_vip = s;
            try
            {
                vip.Source = new Uri(cur_dir + "Vip" + @"\" + list_vip[0]);
                vip.Play();
            } catch { };
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // разворачиваем окно
            this.WindowState = WindowState.Maximized;
            this.Topmost = true;
        }

        private void Vip_MediaEnded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (current_media_index < list_vip.Count - 1)
                {
                    vip.Source = new Uri(cur_dir + "Vip" + @"\" + list_vip[current_media_index + 1]);
                    vip.Play();
                    current_media_index++;
                }
                else
                {
                    this.Close();
                }
            }
            catch
            {
                this.Close();
            };
        }

        private void Vip_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            try
            {
                current_media_index++;
                if (current_media_index < list_vip.Count - 1)
                {
                    vip.Source = new Uri(cur_dir + "Vip" + @"\" + list_vip[current_media_index]);
                    vip.Play();
                }
            }
            catch
            {
                this.Close();
            };
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            var animation = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromSeconds(3.0)));
            Storyboard board = new Storyboard();
            board.Children.Add(animation);
            Storyboard.SetTarget(animation, vip_form);
            Storyboard.SetTargetProperty(animation, new PropertyPath("(Opacity)"));
            board.Completed += delegate
            {

            };
            board.Begin();
        }
    }
}
