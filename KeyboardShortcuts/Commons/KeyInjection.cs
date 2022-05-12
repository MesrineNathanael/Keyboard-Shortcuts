using KeyboardShortcuts.Commons.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KeyboardShortcuts.Commons
{
    public class KeyInjection
    {
        [DllImport("user32.dll")]
        private static extern bool InjectKeyboardInput(ref TagKeyInput input, uint count);

        //Async key injector
        public void TypeKeysAsync(string text, int delay = 1) 
        {
            Task.Run(() =>
            {
                var keys = text.ToCharArray();
                foreach (var key in keys)
                {
                    TypeKey(KeyCodeCharWrapper.GetKey(key), delay);
                }
            });
        }
        
        public void TypeKeyAsync(KeyCode code, int delay = 1)
        {
            if (code == 0)
            {
                Console.WriteLine($"[ERROR] KeyCode <{code}> is not valid");
                return;
            }
            
            Task.Run(() =>
            {
                TypeKey(false, code);
                Thread.Sleep(delay);
                TypeKey(true, code);
            });
        }

        private void TypeKey(KeyCode code, int delay = 1)
        {
            TypeKey(false, code);
            Thread.Sleep(delay);
            TypeKey(true, code);
        }

        private void TypeKey(bool up, KeyCode code)
        {
            TagKeyInput input = new TagKeyInput()
            {
                wScan = code
            };
            if (up)
            {
                input.dwFlags = KeyEventFlags.KEYEVENTF_KEYUP;
            }
            InjectKeyboardInput(ref input, 1);
        }
    }
}
