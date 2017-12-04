using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KotBot.Modules
{
    public class ModuleInfo : Attribute
    {
        public string name { get; set; }
        public string version { get; set; }
        public string author { get; set; }
        public string description { get; set; }
        public ModuleInfo(string name, string version, string author, string description)
        {
            this.name = name;
            this.version = version;
            this.author = author;
            this.description = description;
        }
    }
}
