using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace player3
{
    class FileNameType
    {
        public string FileName { get; set; }
        public int TypeContent { get; set; }

        public FileNameType(string FileName, int TypeContent)
        {
            this.FileName = FileName;
            this.TypeContent = TypeContent;
        }
    }
}
