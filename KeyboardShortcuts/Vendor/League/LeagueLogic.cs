using KeyboardShortcuts.Commons;
using KeyboardShortcuts.Vendor.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Input;

namespace KeyboardShortcuts.Vendor
{
    public class LeagueLogic : KeyListener
    {
        public List<Shortcut> Shortcuts = new List<Shortcut>
        {
            new Shortcut
            {
                Text1 = "\rTOP GET FLASH AT",
                Var1 = "%5",
                Var2 = "\r",
                Keys = new List<Key>
                {
                    Key.I,
                    Key.D7
                }
            },
            new Shortcut
            {
                Text1 = "\rJGL GET FLASH AT",
                Var1 = "%5",
                Var2 = "\r",
                Keys = new List<Key>
                {
                    Key.I,
                    Key.D8
                }
            },
            new Shortcut
            {
                Text1 = "\rMID GET FLASH AT",
                Var1 = "%5",
                Var2 = "\r",
                Keys = new List<Key>
                {
                    Key.I,
                    Key.D9
                }
            },
            new Shortcut
            {
                Text1 = "\rADC GET FLASH AT",
                Var1 = "%5",
                Var2 = "\r",
                Keys = new List<Key>
                {
                    Key.I,
                    Key.D0
                }
            },
            new Shortcut
            {
                Text1 = "\rUWU",
                Var2 = "\r",
                Keys = new List<Key>
                {
                    Key.I,
                    Key.U
                }
            }

        };

        private DateTime _startTime;

        public LeagueLogic(KeyInjection keyInjection) : base(keyInjection)
        {
            
        }

        protected override void Listen()
        {
            while (true)
            {
                Thread.Sleep(10);

                WaitingForUpKey();
                WaitingNewGame();
                
                int orbwalkKey = VkKeyScan('h');
                short keyStateTemp = GetAsyncKeyState(orbwalkKey);
                bool keyIsPressed = ((keyStateTemp >> 15) & 0x0001) == 0x0001;
                if (keyIsPressed) Log.WriteDebug("H is pressed");

                foreach (var shortcut in Shortcuts)
                {
                    bool allKeyPressed = false;
                    foreach(var key in shortcut.Keys)
                    {
                        if (!Keyboard.IsKeyDown(key))
                        {
                            allKeyPressed = false;
                            break;
                        }
                        allKeyPressed = true;
                    }

                    if (allKeyPressed)
                    {
                        SetAndTypeKeys(shortcut.Keys.Last(), ParseSugarText(shortcut.ToString()));
                    }
                }
            }
        }

        private void WaitingNewGame()
        {

            if (Keyboard.IsKeyDown(Key.N) && Keyboard.IsKeyDown(Key.G))
            {
                StartNewGame();
            }            
        }

        private void StartNewGame()
        {
            _startTime = DateTime.Now;
            Log.WriteInfo("New game of League Of Legends started at " + _startTime);
        }

        private string ParseSugarText(string text)
        {
            if (text.Contains("%5"))
            {
                var time = DateTime.Now - _startTime;
                var flashTime = time.Add(new TimeSpan(0,5,0));
                text = text.Replace("%5", flashTime.ToString("mm\\:ss"));
            }
            return text;
        }
    }
}
