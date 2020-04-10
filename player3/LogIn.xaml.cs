using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace player3
{
    /// <summary>
    /// Логика взаимодействия для LogIn.xaml
    /// </summary>
    public partial class LogIn : Window
    {
        public string user_ { get; set; }
        public string password_ { get; set; }
        public string name_ { get; set; }

        public LogIn()
        {
            InitializeComponent();
        }
        // ok
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            user_ = user.Text.Trim(); ;
            password_ = password.Password.Trim();
            name_ = folder.Text.Trim().Replace(' ', '_');

            this.DialogResult = true;
        }
        // открыть сетевые настройки
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Process.Start("control.exe", "netconnections");
        }
    }
}
