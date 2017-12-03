using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KotBot.Scripting
{
    public static class LuaTask
    {
        [RegisterLuaFunction("Task.Create")]
        public static Task TaskCreate(NLua.LuaFunction func, params object[] objs)
        {
            Task newTask = Task.Factory.StartNew(() =>
            {
                if (objs.Length == 0)
                {
                    func.Call();
                }
                else
                {
                    func.Call(objs);
                }
                
            });
            return newTask;
        }
    }
}
