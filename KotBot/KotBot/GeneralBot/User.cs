using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KotBot
{
    public class User
    {
        public virtual void Message(string message)
        {

        }
        public virtual string GetName()
        {
            return "";
        }
        public virtual string GetUserID()
        {
            return "";
        }
    }
}
