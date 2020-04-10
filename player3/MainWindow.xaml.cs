using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

namespace player3
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
           InitializeComponent();
        }

        string version = "1.0.1";

        Settings settings = new Settings();
        Db db = new Db(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString);
        // настройки из бд
        SettingsDb settingsDb = new SettingsDb();

        string cur_dir = AppDomain.CurrentDomain.BaseDirectory + @"content\";

        Process ProcRunRext = new Process();

        DispatcherTimer TimerAf = new DispatcherTimer();
        DispatcherTimer TimerAkcii = new DispatcherTimer();
        DispatcherTimer TimerOb = new DispatcherTimer();

        DispatcherTimer timer_vip = new DispatcherTimer();

        int CounterAkcii = 0;
        int CounterAfisha = 0;
        int CounterOb = 0;
        int current_media_index = 0;
        int Flag_start_video = 0;
        // частота смены изображений
        int N = 10;

        private List<string> ListAfisha = new List<string>();
        private List<string> ListOb = new List<string>();
        private List<string> ListAkcii = new List<string>();
        private List<string> list_video = new List<string>();
        private List<string> list_video_vip = new List<string>();
        
        // обновление контента
        private void UpdateContent(object sender, EventArgs e)
        {
            //ListAkcii = File.ReadAllLines(cur_dir + "Акции" + @"\" + "Акции").ToList();
        }

        private int ListingContent(List<string> list, int CounterList, string TypeContent, Image img)
        {
            try
            {
                if (CounterList == list.Count)
                {
                    list = File.ReadAllLines(cur_dir + TypeContent + @"\" + TypeContent).ToList();

                    CounterList = 0;

                    switch (TypeContent)
                    {
                        case "Акции":
                            ListAkcii = list;
                            break;
                        case "Афиша":
                            ListAfisha = list;
                            break;
                        case "Объявления":
                            ListOb = list;
                            break;
                    }
                }

                BitmapImage image = new BitmapImage();
                if ((list.Count != 0) && (File.Exists(cur_dir + TypeContent + @"\" + list[CounterList])) && 
                     (list[CounterList].Contains("jpeg")  || (list[CounterList].Contains("jpg"))))
                {
                    using (var stream = new FileStream(cur_dir + TypeContent + @"\" +
                                list[CounterList], FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.StreamSource = stream;
                        image.EndInit();
                        image.Freeze();
                    };
                    img.Source = image;

                    var animation = new DoubleAnimation(5, 0.1, new Duration(TimeSpan.FromSeconds(N)));
                    Storyboard board = new Storyboard();
                    board.Children.Add(animation);
                    Storyboard.SetTarget(animation, img);
                    Storyboard.SetTargetProperty(animation, new PropertyPath("(Opacity)"));
                    board.Completed += delegate
                    {

                    };
                    board.RepeatBehavior = RepeatBehavior.Forever;
                    board.AutoReverse = true;
                    board.Begin();

                    image = null;
                }
                else
                {
                    img.Source = null;
                    image = null;
                    // не корректный файл
                    // удаляем из файла со списком файлов
                    list.RemoveAt(CounterList);
                    File.WriteAllLines(cur_dir + TypeContent + @"\" + TypeContent, list);
                }
            }
            catch {
                CounterList++;
            };
            CounterList++;
            return CounterList;
        }
        // смена акции
        private void ListingAkcii(object sender, EventArgs e)
        {
            CounterAkcii = ListingContent(ListAkcii, CounterAkcii, "Акции", img_akcii);
            if (Flag_start_video == 0) LoadAndPlayVideo();
        }
        // смена афиша
        private void ListingAf(object sender, EventArgs e)
        {
            CounterAfisha = ListingContent(ListAfisha, CounterAfisha, "Афиша", img_afisha);
        }
        // смена объявления
        private void ListingOb(object sender, EventArgs e)
        {
            CounterOb = ListingContent(ListOb, CounterOb, "Объявления", img_ob);
        }

        // Loaded
        private void LoadPlayer(object sender, RoutedEventArgs e)
        {
            // разворачиваем экран
            Desktop d = new Desktop();
            Desktop.Rotate(1, Desktop.Orientations.DEGREES_CW_270);

            // отображаем заставку
            SplashScreen splash = new SplashScreen(@"img\splash.jpeg");
            splash.Show(true, true);
            Thread.Sleep(5000);

            MyFtp Ftp = new MyFtp();

            /* получаем настройки из БД:
             * если нет подключения к интернету, то по умолчанию бегущая строка скрыта и
             * время показа ВИП каждые 5 минут
            */
            settingsDb = db.GetSettingsDB(settings.UserId, settings.PanelId);
            
            // если не заданы все параметры
            if (!settings.confirmed)
            {
                // если есть интернет предлагаем зарегистрировать панель
                if (settings.StateInternet)
                {
                    // предлагаем ввсети данные
                    LogIn logIn = new LogIn();
                    // если ОК
                    if (logIn.ShowDialog() == true)
                    {
                        try
                        {
                            // создаем папку для панели и все подпапки
                            Ftp.CreatePanel(settings.Adr + ":" + settings.Port, logIn.name_, logIn.user_, logIn.password_);
                            // если панель успешно зарегистрированна, то сохраняем учетные данные
                            settings.SetSettings("user", logIn.user_);
                            settings.SetSettings("pass", logIn.password_);
                            settings.SetSettings("PanelName", logIn.name_);
                            settings.SetSettings("player_version", version);
                            MessageBox.Show("Панель создана на сервере");
                            
                            int IdPanel = 0;
                            int IdUser = 0;
                            // ищем пользователя
                            IdUser = db.CheckUser(logIn.user_, logIn.password_);
                            // если пользователь не найден вставляем в БД
                            if (IdUser == 0)
                               // добавляем нового пользователя и получаем его id для вставки панели
                               IdUser = db.InsertUser(logIn.user_, logIn.password_);
                            // добавляем панель и получаем ее id
                            IdPanel = db.InsertPanel(logIn.name_, settings.player_version, IdUser);
                            // вставляем id панели
                            settings.SetSettings("PanelId", IdPanel.ToString());
                            settings.SetSettings("UserId", IdUser.ToString());
                            MessageBox.Show("Панель добавлена в БД");
                        }
                        catch
                        {
                            MessageBox.Show("Неверные учетные данные или панель с таким именем уже существует.");
                        }
                    }
                    // удаляем из памяти
                    logIn = null;
                }
            }
            // запускаем синхронизацию
            Sync s = new Sync();
            db.PostVersion(settings.UserId, settings.PanelId, version);
            s.StartAsync();
            // запускаем бегущую строку
            if (settingsDb.RunText == 1)
            {
                try
                {
                    ProcRunRext.StartInfo.FileName = AppDomain.CurrentDomain.BaseDirectory + "RunText.exe";
                    ProcRunRext.StartInfo.Arguments = "";
                    ProcRunRext.Start();
                }
                catch { };
            }
            else
            {
                GridLengthConverter gridLengthConverter = new GridLengthConverter();
                MainGrid.RowDefinitions[0].Height = (GridLength)gridLengthConverter.ConvertFrom("80*");
                MainGrid.RowDefinitions[1].Height = (GridLength)gridLengthConverter.ConvertFrom("95*");
                MainGrid.RowDefinitions[2].Height = (GridLength)gridLengthConverter.ConvertFrom("60*");
                MainGrid.RowDefinitions[3].Height = (GridLength)gridLengthConverter.ConvertFrom("0");
            }
            //Thread thread = new Thread(StartText);
            //thread.Start();
            // запуск всех таймеров
            InitTimers();
            LoadAndPlayVideo();
        }
        private void StartText()
        {
            // запускаем бегущую строку
            this.Dispatcher.BeginInvoke(DispatcherPriority.Render,
               (ThreadStart)delegate ()
               {
                   RunText runText = new RunText();
                   runText.Show();
                   runText.Left = -5;
                   runText.Top = SystemParameters.PrimaryScreenHeight - 100;
                   runText.Width = SystemParameters.PrimaryScreenWidth + 20;
                   runText.Height = 110;
               });
        }

        private void InitTimers()
        {
            TimerAkcii.Tick += new EventHandler(ListingAkcii);
            TimerAkcii.Interval = new TimeSpan(0, 0, N);
            TimerAkcii.Start();

            Thread.Sleep(2000);

            TimerAf.Tick += new EventHandler(ListingAf);
            TimerAf.Interval = new TimeSpan(0, 0, N);
            TimerAf.Start();

            Thread.Sleep(1000);

            TimerOb.Tick += new EventHandler(ListingOb);
            TimerOb.Interval = new TimeSpan(0, 0, N);
            TimerOb.Start();

            timer_vip.Tick += new EventHandler(check_vip);
            timer_vip.Interval = new TimeSpan(0, settingsDb.vip, 0);
            timer_vip.Start();
        }

        // закрыть плеер
        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (settingsDb.RunText == 1) ProcRunRext.Kill();
            Application.Current.Shutdown();
        }
        // развернуть экран
        private void Window_Closed(object sender, EventArgs e)
        {
            // разворачиваем экран обратно 
            Desktop d = new Desktop();
            Desktop.Rotate(1, Desktop.Orientations.DEGREES_CW_0);
        }

        private void Video_MediaEnded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (current_media_index < list_video.Count - 1)
                {
                    video.Stop();
                    video.Source = new Uri(cur_dir + "Видео" + @"\" + list_video[current_media_index + 1]);
                    video.Play();
                    current_media_index++;
                }
                else
                {
                    video.Stop();
                    list_video.Clear();
                    list_video = File.ReadAllLines(cur_dir + "Видео" + @"\" + "Видео").ToList();
                    current_media_index = 0;
                    video.Source = new Uri(cur_dir + "Видео" + @"\" + list_video[current_media_index]);
                    video.Play();
                }
            }
            catch { };
        }

        private void Video_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            try
            {
                list_video.Clear();
                video.Stop();

                list_video = File.ReadAllLines(cur_dir + "Видео" + @"\" + "Видео").ToList();

                if ((list_video.Count > 0))
                {
                    Flag_start_video = 1;
                    video.Source = new Uri(cur_dir + "Видео" + @"\" + list_video[0]);
                    video.Play();
                }
                else Flag_start_video = 0;
            }
            catch { }
        }

        private void LoadAndPlayVideo()
        {
            try
            {
                list_video.Clear();
                // video
                list_video = File.ReadAllLines(cur_dir + "Видео" + @"\" + "Видео").ToList();

                for (int i = 0; i <= list_video.Count - 1; i++)
                {
                    if ((File.Exists(cur_dir + "Видео" + @"\" + list_video[i]) && (list_video[i].Contains("mp4")))) 
                    {

                    }
                    else
                    {
                        list_video.RemoveAt(i);
                    }
                }
                //запускаем видео
                if ((list_video.Count > 0) && (File.Exists(cur_dir + "Видео" + @"\" + list_video[0])))
                {
                    // есть видео на воспроизведение
                    Flag_start_video = 1;
                    video.Source = new Uri(cur_dir + "Видео" + @"\" + list_video[0]);
                    video.Play();
                }
            }
            catch { };
        }

        // таймер для запуска vip контента
        private void check_vip(object sender, EventArgs e)
        {
            try
            {
                list_video_vip.Clear();
                list_video_vip = File.ReadAllLines(cur_dir + "Vip" + @"\" + "Vip").ToList();

                for (int i = 0; i <= list_video_vip.Count - 1; i++ )
                {
                   if (File.Exists(cur_dir + "Vip" + @"\" + list_video_vip[i]) && (list_video_vip[i].Contains("mp4")))
                    {

                    }
                   else
                    {
                        list_video_vip.RemoveAt(i);
                    }
                }

                if ((list_video_vip.Count > 0) && (File.Exists(cur_dir + "Vip" + @"\" + list_video_vip[0])))
                {
                    video.Pause();
                    timer_vip.Stop();
                    TimerAf.Stop();
                    TimerAkcii.Stop();
                    TimerOb.Stop();
                    vip_video X = new vip_video(list_video_vip);
                    X.ShowDialog();
                    video.Play();
                    timer_vip.Start();
                    TimerAf.Start();
                    TimerAkcii.Start();
                    TimerOb.Start();
                }
            }
            catch { };
        }
    }
}
