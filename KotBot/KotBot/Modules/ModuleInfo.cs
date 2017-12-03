using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KotBot.Modules
{
    public class ModuleInfo : Attribute
    {
        public string name;
        public string version;
        public string author;
        public string description;
        public ModuleInfo(string name, string version, string author, string description)
        {
            this.name = name;
            this.version = version;
            this.author = author;
            this.description = description;
        }
    }
}
