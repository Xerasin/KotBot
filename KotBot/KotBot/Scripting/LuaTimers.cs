using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using NLua;

namespace KotBot.Scripting
{
    class LuaTimer
    {
        public static Dictionary<object, LuaTimer> timers = new Dictionary<object, LuaTimer>();
        [RegisterLuaFunction("timer.Create")]
        public static LuaTimer Create(object name, int interval, int times, LuaFunction func)
        {
            LuaTimer timer = new LuaTimer(interval);
            timer.SetInterval(interval);
            timer.SetTimes(times);
            timer.SetFunction(func);
            timer.name = name;
            if (timers.ContainsKey(name))
            {
                timers[name].Destroy();
            }
            timers[name] = timer;
            return timer;
        }
        static Random rand = new Random();
        [RegisterLuaFunction("timer.Simple")]
        public static LuaTimer Simple(int interval, LuaFunction func)
        {
            object name = "TIMER SIMPLE" + rand.Next(0, 20000) + DateTime.Now.ToString();
            LuaTimer timer = new LuaTimer(interval);
            timer.SetInterval(interval);
            timer.SetTimes(1);
            timer.SetFunction(func);
            timer.name = name;
            if (timers.ContainsKey(name))
            {
                timers[name].Destroy();
            }
            timers[name] = timer;
            return timer;
        }
        [RegisterLuaFunction("timer.Get")]
        public static LuaTimer GetTimer(object name)
        {
            if (timers.ContainsKey(name))
            {
                return timers[name];
            }
            return null;
        }

        #region class
        public LuaFunction func = null;
        public Timer timer = null;
        public int times = 0;
        public object name = null;

        public LuaTimer(double interval)
        {
            this.timer = new Timer(interval);
            this.times = 0;
        }
        public void SetFunction(LuaFunction func)
        {
            if (timer != null)
            {
                this.func = func;
                timer = new Timer(timer.Interval);
                
                timer.Elapsed += new ElapsedEventHandler((object e, ElapsedEventArgs args) => {

                    if (times != 0)
                    {
                        try
                        {
                            func.Call(new object[] { this, args.SignalTime });
                        }
                        catch (NLua.Exceptions.LuaException exp)
                        {
                            Log.Error(exp.Message);
                            this.SetTimes(0);
                        }
                        catch (ArgumentNullException exp)
                        {
                            Log.Error(exp.Message);
                        }
                        if (times > 0)
                        {
                            times = times - 1;
                            if (times == 0)
                            {
                                timer.Stop();
                            }
                        }
                    }
                } );
                timer.Start();
            }
        }
        public LuaFunction GetFunction()
        {
            return func;
        }

        public void SetInterval(double interval)
        {
            if (timer != null)
            {
                timer.Interval = interval;
            }
        }

        public double GetInterval()
        {
            if (timer != null)
            {
                return timer.Interval;
            }
            return 0;
        }

        public void SetTimes(int times)
        {
            this.times = times;
            if (times > 0)
            {
                timer.Start();
            }
        }

        public int GetTimes()
        {
            return this.times;
        }

        public void Destroy()
        {
            timer.Dispose();
            timers.Remove(this.name);
            this.timer = null;
        }

        #endregion

    }
}
