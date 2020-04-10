using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace player3
{
    class Sync
    {
        Settings settings = new Settings();
        // получаем список файлов из БД на удаление и удаляем
        private void DeleteFile()
        {
            Db db = new Db(settings.ConnStr);
            List<FileNameType> ListDelete = new List<FileNameType>();

            ListDelete = db.RequestContentToDeleted(settings.UserId, settings.PanelId);

            DelFile(ListDelete);           
        }
        // удаляем файлы с панели из списка полученного из БД
        private void DelFile(List<FileNameType> files)
        {
            string cur_dir = AppDomain.CurrentDomain.BaseDirectory + @"content\";
            
            Db db = new Db(settings.ConnStr);

            for (int i = 0; i < files.Count; i++)
            {
                switch (files[i].TypeContent)
                {
                    case 1: 
                        File.Delete(cur_dir + @"Акции\" + files[i].FileName);
                        break;
                    case 2:
                        File.Delete(cur_dir + @"Афиша\" + files[i].FileName);
                        break;
                    case 3:
                        File.Delete(cur_dir + @"Объявления\" + files[i].FileName);
                        break;
                    case 4:
                        File.Delete(cur_dir + @"Видео\" + files[i].FileName);
                        break;
                    case 5:
                        File.Delete(cur_dir + @"Строка\" + files[i].FileName);
                        break;
                    case 6:
                        File.Delete(cur_dir + @"Vip\" + files[i].FileName);
                        break;                   
                }
                // удаляем запись из БД
                db.DeleteFileDB(settings.UserId, settings.PanelId, files[i].FileName);
            }
        }

        // загрузка + удаление + update DB по типу контента
        private void Download(string TypeContent, int TypeContentCode)
        {
            try
            {
                List<FileInfo> ListFtp = new List<FileInfo>();
                List<string> ListDb = new List<string>();

                string cur_dir = AppDomain.CurrentDomain.BaseDirectory + @"content\";

                MyFtp myFtp = new MyFtp();
                // получаем список файлов
                ListFtp = myFtp.ListDirectory(settings.Adr, settings.Port, settings.PanelName + '/' + TypeContent);
                Db db = new Db(settings.ConnStr);
                // получаем информацию о файлах, требующих загрузки из БД
                ListDb = db.RequestContent(settings.UserId, settings.PanelId, TypeContentCode);
                for (int i = 0; i < ListFtp.Count; i++)
                {
                    // если файл есть в БД как ожидающий загрузки, то загружаем
                    if (ListDb.Contains(ListFtp[i].Name))
                    {
                        // загружаем файл
                        myFtp.DownloadFile(settings.Adr, settings.Port,
                                           settings.PanelName + '/' + TypeContent + '/' + ListFtp[i].Name, ListFtp[i].Name, TypeContent);
                        // удаляем файл с сервера
                        myFtp.DeleteFile(settings.Adr + ':' + settings.Port + '/' + settings.PanelName + '/' + TypeContent, ListFtp[i].Name);
                        // проставляем sync = 1 в БД для загруженного файла
                        db.SetFlagDBDownloadFile(settings.UserId, settings.PanelId, TypeContentCode, ListFtp[i].Name);
                    }
                };
                // обновляем списки файлов
                if (ListDb.Count > 0)
                    File.WriteAllLines(cur_dir + TypeContent + @"\" + TypeContent, ListDb);
            }
            catch
            {

            }
        }
        // проверка содержимого папки update на наличие новой версии
        public void CheckNewVersion()
        {
            try
            {
                MyFtp myFtp = new MyFtp();
                List<FileInfo> list = new List<FileInfo>();
                list = myFtp.ListDirectory(settings.Adr, settings.Port, settings.PanelName + @"/update");
                // если есть обновления
                if (list.Count > 0)
                {
                    //Version server_version = new Version(list[0].Name);
                    //Version current_version = new Version("1.0.0");
                    //if (server_version > current_version)
                    //{
                        // скачиваем новую версию в папку Content
                        myFtp.DownloadFile(settings.Adr, settings.Port,
                                               settings.PanelName + '/' + "update" + '/' + list[0].Name, "newversion", "");
                        // перемещаем в корень
                        File.Move(AppDomain.CurrentDomain.BaseDirectory + @"content\newversion",
                                    AppDomain.CurrentDomain.BaseDirectory + "newversion");
                        // удаляем файл с сервера
                        myFtp.DeleteFile(settings.Adr + ':' + settings.Port + '/' + settings.PanelName + "/update", list[0].Name);
                        // запускаем updater, который закроет текущий плеер, заменит его на новый и запустит уже новый 
                        Process Proc = new Process();
                        Proc.StartInfo.FileName = AppDomain.CurrentDomain.BaseDirectory + "updater.exe";
                        Proc.StartInfo.Arguments = "";
                        Proc.Start();
                    //}
                }
            }catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
        }

        public void StartAsync()
        {
            using (BackgroundWorker bw = new BackgroundWorker())
            {
                bw.WorkerReportsProgress = true;
                bw.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
                bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
                bw.DoWork += (p, ev) =>
                {
                    while (true) 
                    {
                        try
                        {
                            // проверяем есть ли что на удаление и удаляем
                            DeleteFile();
                            // загружаем весь контент
                            Download("Акции", 1);
                            Download("Афиша", 2);
                            Download("Объявления", 3);
                            Download("Видео", 4);
                            Download("Строка", 5);
                            Download("Vip", 6);
                            Db db = new Db(settings.ConnStr);
                            // отправляем время подключения к серверу на сервер
                            db.PostConnectTime(settings.UserId, settings.PanelId, DateTime.Now.ToString());
                            // проверяем наличие обновлений
                            CheckNewVersion();
                            // делаем задержку после загрузки всего контента
                            Thread.Sleep(1000 * 300);
                        }
                        catch { };
                    };
                };
                bw.RunWorkerAsync();
            }
        }

        private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
