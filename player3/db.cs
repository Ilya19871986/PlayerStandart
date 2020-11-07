using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace player3
{
    class Db
    {
        private string ConStr;

        public Db(string s)
        {
            this.ConStr = s;
        }

        // ищем если такой пользователь
        public int CheckUser(string name, string pass)
        {
            string command = "find_user";
            int id = 0;
            try
            {
                using (MySqlConnection conn = new MySqlConnection(ConStr))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand(command, conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        MySqlParameter p1 = new MySqlParameter("login", MySqlDbType.VarChar);
                        MySqlParameter p2 = new MySqlParameter("pass", MySqlDbType.VarChar);

                        p1.Value = name.Trim();
                        p2.Value = pass.Trim();

                        cmd.Parameters.Add(p1);
                        cmd.Parameters.Add(p2);
                        cmd.CommandType = CommandType.StoredProcedure;
                        id = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }
            catch { };
            return id;
        }
        // вставляем пользователя в БД
        public int InsertUser(string name, string pass)
        {
            int id = 0;
            try
            {
                string command = "insert_user";
                using (MySqlConnection conn = new MySqlConnection(ConStr))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand(command, conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        MySqlParameter p1 = new MySqlParameter("login", MySqlDbType.VarChar);
                        MySqlParameter p2 = new MySqlParameter("pass", MySqlDbType.VarChar);

                        p1.Value = name.Trim();
                        p2.Value = pass.Trim();

                        cmd.Parameters.Add(p1);
                        cmd.Parameters.Add(p2);
                        cmd.CommandType = CommandType.StoredProcedure;
                        id = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }
            catch
            {
                MessageBox.Show("Ошибка добавления пользователя в БД");
            }
            return id;
        }
        // вставляем панель
        public int InsertPanel(string PanelName, string version, int UserId)
        {
            int id = 0;
            try
            {
                string command = "insert_panel";
                using (MySqlConnection conn = new MySqlConnection(ConStr))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand(command, conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        MySqlParameter p1 = new MySqlParameter("panel_name", MySqlDbType.VarChar);
                        MySqlParameter p2 = new MySqlParameter("version", MySqlDbType.VarChar);
                        MySqlParameter p3 = new MySqlParameter("user_id", MySqlDbType.Int32);

                        p1.Value = PanelName.Trim();
                        p2.Value = version.Trim();
                        p3.Value = UserId;

                        cmd.Parameters.Add(p1);
                        cmd.Parameters.Add(p2);
                        cmd.Parameters.Add(p3);
                        cmd.CommandType = CommandType.StoredProcedure;
                        id = Convert.ToInt32(cmd.ExecuteScalar());                        
                    }
                }
            }
            catch
            {
                MessageBox.Show("Ошибка добавления панели в БД");
            }
            return id;
        }
        // получаем список файлов из БД, ожидающих загрузку
        public List<string> RequestContent(int UserId, int PanelId, int TypeContent)
        {
            string command = "show_content";
            List<string> list = new List<string>();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(ConStr))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand(command, conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        MySqlParameter p1 = new MySqlParameter("Userid", MySqlDbType.Int32);
                        MySqlParameter p2 = new MySqlParameter("Panelid", MySqlDbType.Int32);
                        MySqlParameter p3 = new MySqlParameter("typecontent", MySqlDbType.Int32);

                        p1.Value = UserId;
                        p2.Value = PanelId;
                        p3.Value = TypeContent;

                        cmd.Parameters.Add(p1);
                        cmd.Parameters.Add(p2);
                        cmd.Parameters.Add(p3);

                        cmd.CommandType = CommandType.StoredProcedure;
                        DataTable dt = new DataTable();
                        MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                        adapter.FillAsync(dt);

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            list.Add(dt.Rows[i][1].ToString());
                        }
                    }
                }
            } catch { };
            return list;
        }
        // после загрузки файла проставляем в БД sync = 1
        public void SetFlagDBDownloadFile(int UserId, int PanelId, int TypeContent, string FileName)
        {
            try
            {
                string command = "update_flag_download";
                using (MySqlConnection conn = new MySqlConnection(ConStr))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand(command, conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        MySqlParameter p1 = new MySqlParameter("UserId", MySqlDbType.Int32);
                        MySqlParameter p2 = new MySqlParameter("PanelId", MySqlDbType.Int32);
                        MySqlParameter p3 = new MySqlParameter("TypeContent", MySqlDbType.Int32);
                        MySqlParameter p4 = new MySqlParameter("FileName", MySqlDbType.VarChar);

                        p1.Value = UserId;
                        p2.Value = PanelId;
                        p3.Value = TypeContent;
                        p4.Value = FileName;

                        cmd.Parameters.Add(p1);
                        cmd.Parameters.Add(p2);
                        cmd.Parameters.Add(p3);
                        cmd.Parameters.Add(p4);

                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.ExecuteNonQuery();
                    }
                }
            } catch { };
        }
        // получаем список файлов из БД, которые надо удалить
        public List<FileNameType> RequestContentToDeleted(int UserId, int PanelId)
        {
            string command = "show_content_deleted";
            List<FileNameType> list = new List<FileNameType>();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(ConStr))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand(command, conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        MySqlParameter p1 = new MySqlParameter("UserId", MySqlDbType.Int32);
                        MySqlParameter p2 = new MySqlParameter("PanelId", MySqlDbType.Int32);

                        p1.Value = UserId;
                        p2.Value = PanelId;

                        cmd.Parameters.Add(p1);
                        cmd.Parameters.Add(p2);

                        cmd.CommandType = CommandType.StoredProcedure;
                        DataTable dt = new DataTable();
                        MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                        adapter.FillAsync(dt);

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            list.Add(new FileNameType(dt.Rows[i][0].ToString(), Int32.Parse(dt.Rows[i][1].ToString())));
                        }
                    }
                }
            } catch { };
            return list;
        }
        // удаляем запись о файле в БД по имени, user_id и panel_id
        public void DeleteFileDB(int UserId, int PanelId, string FileName)
        {
            try
            {
                string command = "delete_file_player";
                using (MySqlConnection conn = new MySqlConnection(ConStr))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand(command, conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        MySqlParameter p1 = new MySqlParameter("UserId", MySqlDbType.Int32);
                        MySqlParameter p2 = new MySqlParameter("PanelId", MySqlDbType.Int32);
                        MySqlParameter p3 = new MySqlParameter("FileName", MySqlDbType.VarChar);

                        p1.Value = UserId;
                        p2.Value = PanelId;
                        p3.Value = FileName;

                        cmd.Parameters.Add(p1);
                        cmd.Parameters.Add(p2);
                        cmd.Parameters.Add(p3);

                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.ExecuteNonQuery();
                    }
                }
            } catch { };
        }
        // отправдяем версию плеера на сервер 
        public void PostVersion(int UserId, int PanelId, string version)
        {
            try
            {
                string command = "post_version";
                using (MySqlConnection conn = new MySqlConnection(ConStr))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand(command, conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        MySqlParameter p1 = new MySqlParameter("UserId", MySqlDbType.Int32);
                        MySqlParameter p2 = new MySqlParameter("PanelId", MySqlDbType.Int32);
                        MySqlParameter p3 = new MySqlParameter("version", MySqlDbType.VarChar);

                        p1.Value = UserId;
                        p2.Value = PanelId;
                        p3.Value = version;

                        cmd.Parameters.Add(p1);
                        cmd.Parameters.Add(p2);
                        cmd.Parameters.Add(p3);

                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch { };
        }
        // отправляем время подключения на сервер
        public void PostConnectTime(int UserId, int PanelId, string time)
        {
            try
            {
                string command = "post_connect_time";
                using (MySqlConnection conn = new MySqlConnection(ConStr))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand(command, conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        MySqlParameter p1 = new MySqlParameter("UserId", MySqlDbType.Int32);
                        MySqlParameter p2 = new MySqlParameter("PanelId", MySqlDbType.Int32);
                        MySqlParameter p3 = new MySqlParameter("t", MySqlDbType.VarChar);

                        p1.Value = UserId;
                        p2.Value = PanelId;
                        p3.Value = time;

                        cmd.Parameters.Add(p1);
                        cmd.Parameters.Add(p2);
                        cmd.Parameters.Add(p3);

                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch { };
        }
        // получаем время через которое включаем vip и  показывать бегущую строку или нет
        public SettingsDb GetSettingsDB(int UserId, int PanelId)
        {
            try
            {
                string command = "get_settings";
                using (MySqlConnection conn = new MySqlConnection(ConStr))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand(command, conn))
                    {
                        MySqlParameter p1 = new MySqlParameter("user_id", MySqlDbType.Int32);
                        MySqlParameter p2 = new MySqlParameter("panel_id", MySqlDbType.Int32);

                        p1.Value = UserId;
                        p2.Value = PanelId;
                        cmd.Parameters.Add(p1);
                        cmd.Parameters.Add(p2);

                        cmd.CommandType = CommandType.StoredProcedure;

                        DataTable dt = new DataTable();
                        MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                        adapter.FillAsync(dt);

                        return new SettingsDb(Int32.Parse(dt.Rows[0][0].ToString()), Int32.Parse(dt.Rows[0][1].ToString()), Int32.Parse(dt.Rows[0][2].ToString()));
                        
                    }
                }
            }
            catch
            {
                return new SettingsDb(0, 5, 0);
            }
        }
    }
}
