using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace updater
{
    class Program
    {
        static void Main(string[] args)
        { 
            try
            {
                Process[] listprosecc = Process.GetProcesses();
                foreach (Process oneproc in listprosecc)
                {
                    string ProsessName = oneproc.ProcessName;

                    ProsessName = ProsessName.ToLower();
                    if (ProsessName.Equals("player3") || (ProsessName.Equals("runtext")))
                    {
                        oneproc.Kill();
                        oneproc.WaitForExit();
                    }
                }
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine("Обновление...");
                Thread.Sleep(3000);

                File.Delete(AppDomain.CurrentDomain.BaseDirectory + "player3.exe");

                File.Move(AppDomain.CurrentDomain.BaseDirectory + "newversion", AppDomain.CurrentDomain.BaseDirectory + "player3.exe");
                
                Console.WriteLine("Обновление завершено. Перезапуск...");
                Thread.Sleep(3000);

                Process ProcRun = new Process();
                ProcRun.StartInfo.FileName = AppDomain.CurrentDomain.BaseDirectory + "player3.exe";
                ProcRun.StartInfo.Arguments = "";
                ProcRun.Start();
            }
            catch (Exception ex)
            {
               // MessageBox.Show(ex.Message);
            }
            //Console.ReadKey();
        }
    }
}
