using KeyboardShortcuts.Commons;
using KeyboardShortcuts.Vendor.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace KeyboardShortcuts.Vendor.League.Orbwalker
{
    public class OrbWalkerLogic : KeyListener
    {
        List<Shortcut> Shortcuts = new List<Shortcut>
        {
            new Shortcut()
            {
                //Orbwaler activator
                Keys = new List<Key>
                {
                    Key.V
                },
                Text1 = "orb"
            },
            new Shortcut(){
                Keys = new List<Key>
                {
                    Key.T
                },
                Text1 = "tst",
                LastKeyNeedToBeUp = false
            }
        };

        //Attack move key = X
        //Display Range = c

        private bool _gameStarted = false;

        public OrbWalkerLogic(KeyInjection keyInjection) : base(keyInjection)
        {
        }

        protected override void Listen()
        {
            Log.WriteInfo("OrbWalker logic start listening");
            while (true)
            {
                //Sleep(100000);
                Thread.Sleep(10);

                WaitingForUpKey();

                foreach (var shortcut in Shortcuts)
                {
                    bool allKeyPressed = false;
                    foreach (var key in shortcut.Keys)
                    {
                        if (!Keyboard.IsKeyDown(key))
                        {
                            allKeyPressed = false;
                            break;
                        }
                        if (shortcut.LastKeyNeedToBeUp)
                        {
                            LastKeyPressed = key;
                        }

                        allKeyPressed = true;
                    }

                    if (allKeyPressed && !WaitForUpKey)
                    {
                        ParseSugarText(shortcut.ToString());
                        WaitForUpKey = true;
                    }
                }
            }
        }

        private string ParseSugarText(string text)
        {
            if (text.Contains("orb"))
            {
                _gameStarted = !_gameStarted;
                Log.WriteInfo($"OrbWalker : game started set to {_gameStarted}");
            }
            else if (text.Contains("tst"))
            {
                Log.WriteInfo($"OrbWalker : test key pressed");
            }
            return text;
        }

    }
}
