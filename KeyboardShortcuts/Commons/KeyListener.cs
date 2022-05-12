using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace KeyboardShortcuts.Commons
{
    public abstract class KeyListener
    {
        protected Thread ListenerThread;

        protected KeyInjection KeyInjector;

        protected Key LastKeyPressed;

        protected bool WaitForUpKey = false;

        public KeyListener(KeyInjection keyInjection)
        {
            ListenerThread = new Thread(Listen);
            KeyInjector = keyInjection;
        }

        public void Start()
        {
            Log.WriteInfo("Key Listener starting...");
            
            ListenerThread.SetApartmentState(ApartmentState.STA);
            ListenerThread.Start();

            if (ListenerThread.IsAlive)
            {
                Log.WriteInfo("Key Listener started");
            }
        }

        protected virtual void Listen()
        {
            //This is the base piece of code that will be called on a new thread.
            //Don't call base.Listen().
            //Just copy this code and modify it as you need in your override method.
            while (true)
            {
                Thread.Sleep(10);

                WaitingForUpKey();
            }
        }

        protected void SetAndTypeKeys(Key key, string text)
        {
            WaitForUpKey = true;
            LastKeyPressed = key;
            Log.WriteDebug($"Attempt writing [{text.Replace("\r", "\\r")}]");
            KeyInjector.TypeKeysAsync(text);
        }

        protected void WaitingForUpKey()
        {
            if (WaitForUpKey)
            {
                Thread.Sleep(100);
                if (Keyboard.IsKeyUp(LastKeyPressed))
                {
                    WaitForUpKey = false;
                }
            }
        }
    }
}
