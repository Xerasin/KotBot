using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KotBot
{
    public class Client
    {
        public virtual void Message(string message)
        {

        }
        public virtual string GetName()
        {
            return "";
        }
        public virtual User FindUserByName(string name)
        {
            return null;
        }
        public virtual string GetLocationString()
        {
            return "";
        }
    }
}
