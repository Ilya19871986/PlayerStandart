using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace player3
{
    
    class SettingsDb
    {
        public int vip { get; set; }
        public int RunText { get; set; }

        public SettingsDb(int RunText, int vip)
        {
            this.vip = vip;
            this.RunText = RunText;
        }

        public SettingsDb()
        {
           
        }
    }
}
