using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace player3
{
    class Settings
    {
        public string ConnStr { get; }
        public string Adr { get; set; }
        public string Port { get; set; }
        public string User { get; set; }
        public string Pass { get; set; }
        public string PanelName { get; set; }
        public int UserId { get; set; }
        public int PanelId { get; set; }
        // если все настройки ести, то true
        public bool confirmed { get; set; }
        // если есть интернет, то true
        public bool StateInternet { get; set; }
        // версия плеера
        public string player_version { get; set; }

        public Settings()
        {
            try
            {
                ConnStr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                Adr = ConfigurationManager.AppSettings["address"].ToString();
                Port = ConfigurationManager.AppSettings["port"].ToString();
                User = ConfigurationManager.AppSettings["user"].ToString();
                Pass = ConfigurationManager.AppSettings["pass"].ToString();
                PanelName = ConfigurationManager.AppSettings["PanelName"].ToString();
                if (ConfigurationManager.AppSettings["UserId"].ToString().Length > 0)
                    UserId = Int32.Parse(ConfigurationManager.AppSettings["UserId"].ToString());
                if (ConfigurationManager.AppSettings["PanelId"].ToString().Length > 0)
                    PanelId = Int32.Parse(ConfigurationManager.AppSettings["PanelId"].ToString());
                player_version = ConfigurationManager.AppSettings["player_version"].ToString();

                confirmed = CheckSettings(ConnStr, Adr, Port, User, Pass, PanelName);

                StateInternet = CheckInternet();
            }
            catch { }; 
        }
        // добавляем параметры в конфигурацию
        public void SetSettings(string key, string value)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings[key].Value = value;
            config.Save();
            ConfigurationManager.RefreshSection("appSettings");
        }
        // проверяем настройки
        private bool CheckSettings(string s1, string s2, string s3, string s4, string s5, string s6)
        {
            if ((s1 != "") && (s2 != "") && (s3 != "") && (s4 != "") && (s5 != "") && (s6 != ""))
            {
                return true;
            }
            else return false;
        }
        // проверяем подключение к интернету
        private bool CheckInternet()
        {
            try
            {
                using (WebClient Client = new WebClient())
                {
                    using (Client.OpenRead("http://www.ya.ru/"))
                    {

                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
