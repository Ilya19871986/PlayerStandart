using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace player3
{
    class MyFtp
    {
        private string User;
        private string Pass;

        public MyFtp()
        {
            User = ConfigurationManager.AppSettings["user"].ToString();
            Pass = ConfigurationManager.AppSettings["pass"].ToString();
        }

        public List<FileInfo> ListDirectory(string path, string port, string folder)
        {
            string content = "";
            try
            {
                FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(path + ':' + port + '/' + folder);

                ftpRequest.Credentials = new NetworkCredential(User, Pass);

                ftpRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

                ftpRequest.EnableSsl = false;

                FtpWebResponse ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();

                StreamReader sr = new StreamReader(ftpResponse.GetResponseStream(), Encoding.UTF8);
                content = sr.ReadToEnd();
                sr.Close();
                ftpResponse.Close();
            }
            catch { };

            return _Parse(content, folder);
        }

        // парсинг ответ 
        public List<FileInfo> _Parse(string s, string folder)
        {
            List<FileInfo> tmp = new List<FileInfo>();
            List<string> A = new List<string>();

            try
            {

                string[] array_content = s.Split('\n');

                // разбиваем общую строку на список строк (файлов)
                foreach (string x in array_content)
                {
                    A.Add(x);
                }

                // парсим
                foreach (string x in A)
                {
                    if ((x.Contains("ftp")) & !x.Contains("xml"))
                    {
                        // тип 
                        string type;
                        type = x.Contains("drwxr") ? "images\\user.png" : "images\\file.png";

                        // имя
                        string name;
                        string[] array_name;
                        array_name = x.Split(' ');
                        name = array_name[array_name.Length - 1];

                        // размер
                        string[] array = x.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        string size = array[4];

                        string result = type + " " + name + " " + size;

                        tmp.Add(new FileInfo(name.Trim(), size.Trim(), type.Trim()));
                    }
                }
            }
            catch { };

            return tmp;
        }

        //метод протокола FTP MKD для создания каталога на FTP-сервере 
        public void CreateDirectory(string path, string folderName, string user, string pass)
        {
            try
            {
                FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(path + '/' + folderName);
                ftpRequest.Credentials = new NetworkCredential(user, pass);
                ftpRequest.EnableSsl = false;
                ftpRequest.Method = WebRequestMethods.Ftp.MakeDirectory;
                FtpWebResponse ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                ftpResponse.Close();
            }
            catch { };
        }

        public void CreatePanel(string path, string PanelName, string user, string pass)
        {
            // создаем папку для нового пользователя
            CreateDirectory(path, PanelName, user, pass);
            // создаем "video"
            CreateDirectory(path, PanelName + '/' + "Видео", user, pass);
            // создаем "объявления"
            CreateDirectory(path, PanelName + '/' + "Объявления", user, pass);
            // создаем "бегущая строка"
            CreateDirectory(path, PanelName + '/' + "Строка", user, pass);
            // создаем "афиша"
            CreateDirectory(path, PanelName + '/' + "Афиша", user, pass);
            // создаем "акции"
            CreateDirectory(path, PanelName + '/' + "Акции", user, pass);

            CreateDirectory(path, PanelName + '/' + "Vip", user, pass);

            CreateDirectory(path, PanelName + '/' + "update", user, pass);
        }

        public void DownloadFile(string path, string port, string folder, string fileName, string save_folder)
        {
            try
            {
                FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(path + ':' + port + '/' + folder);

                ftpRequest.Credentials = new NetworkCredential(User, Pass);

                ftpRequest.Method = WebRequestMethods.Ftp.DownloadFile;

                ftpRequest.EnableSsl = false;

                // запоминаем имя файла, чтобы переименовать
                string fn = fileName;

                
                    Guid g = Guid.NewGuid();
                    //Файлы будут копироваться в кталог save_folder
                    // на время копирование генерируем имя файла чтобы плеер "не забрал" и для закачки боьших файлов. После закачки переименуем.
                    fileName = AppDomain.CurrentDomain.BaseDirectory + @"Content\" + save_folder + "\\" + "TEMP" + g.ToString();// fileName;                    
               

                using (FtpWebResponse ftpResponse = (FtpWebResponse)ftpRequest.GetResponse())
                using (Stream responseStream = ftpResponse.GetResponseStream())
                using (FileStream downloadedFile = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite))
                {
                    //Буфер для считываемых данных
                    byte[] buffer = new byte[1024];

                    int readBytesCount = 0;
                    int totalReadBytesCount = 0;

                    readBytesCount = responseStream.Read(buffer, 0, buffer.Length);
                    while (readBytesCount != 0)
                    {
                        downloadedFile.Write(buffer, 0, readBytesCount);
                        readBytesCount = responseStream.Read(buffer, 0, buffer.Length);
                        totalReadBytesCount += readBytesCount;

                    }
                }
                // переименовываем после копирования               
                 File.Move(fileName, AppDomain.CurrentDomain.BaseDirectory + @"Content\" + save_folder + "\\" + fn);
            }
            catch (Exception ex)
            {
            }

        }
        //метод протокола FTP DELE для удаления файла с FTP-сервера
        public void DeleteFile(string path, string file_name)
        {
            try
            {
                FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(path + '/' + file_name);
                ftpRequest.Credentials = new NetworkCredential(User, Pass);
                ftpRequest.EnableSsl = false;
                ftpRequest.Method = WebRequestMethods.Ftp.DeleteFile;

                FtpWebResponse ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                ftpResponse.Close();
            }
            catch { };
        }
    }

}
