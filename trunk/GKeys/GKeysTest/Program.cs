using System;
using GKeys;
using LuaInterface;
namespace GKeysTest
{
    class Program
    {
        static GKeyHandler handler;
        static Lua lua;
        static void Main(string[] args)
        {
            lua = new Lua();
            lua.DoFile("Default.lua");
            
            handler = new GKeyHandler(100);
            handler.OnGKeyUp += new OnGKeyUpEventHandler(OutputKey);
            handler.OnGKeyDown += new OnGKeyDownEventHandler(OutputKey);
            handler.OnModeChange += new OnModeChangeEventHandler(OutputMode);
            
            Console.Read();
        }

        static void OutputKey(GKey whichKey)
        {
            if (handler.IsKeyDown((int)whichKey))
            {
                Console.WriteLine("{0} has been pressed.", whichKey);
                lua.GetFunction("onGKeyDown").Call((int)whichKey);
            }
            else
            {
                Console.WriteLine("{0} has been released.", whichKey);
            }
        }

        static void OutputMode(Mode whichMode)
        {
            Console.WriteLine("Mode changed to {0}", whichMode);
        }
    }
}
