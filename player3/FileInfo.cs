using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace player3
{
    public class FileInfo
    {
        public string Type { get; set; }

        public string Size { get; set; }

        public string Name { get; set; }

        public FileInfo(string _name, string _size, string _type)
        {
            Name = _name;
            Size = _size;
            Type = _type;
        }
    }
}
