using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyboardShortcuts.Commons
{
    public class Log
    {
        public static void Write(string message)
        {
            Console.WriteLine(message);
        }

        public static void WriteInfo(string message)
        {
            Console.WriteLine(GetTime() + " [INFO] " + message);
        }

        public static void WriteError(string message)
        {
            Console.WriteLine(GetTime() + " [ERROR] " + message);
        }

        public static void WriteWarning(string message)
        {
            Console.WriteLine(GetTime() + " [WARNING] " + message);
        }

        public static void WriteDebug(string message)
        {
#if DEBUG
            Console.WriteLine(GetTime() + " [DEBUG] " + message);
#endif
        }

        public static void WriteException(Exception ex)
        {
            Console.WriteLine(GetTime() + " [EXCEPTION] " + ex.Message);
        }
        
        public static void CarriageReturn()
        {
            Console.WriteLine("");
        }

        private static string GetTime()
        {
            return DateTime.Now.ToString("HH:mm:ss:fff");
        }
    }
}
