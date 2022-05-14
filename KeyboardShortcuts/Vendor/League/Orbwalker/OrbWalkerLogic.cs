using KeyboardShortcuts.Commons;
using KeyboardShortcuts.Commons.Enums;
using KeyboardShortcuts.Vendor.Common;
using KeyboardShortcuts.Vendor.League.API;
using KeyboardShortcuts.Vendor.League.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                //Orbwalker switch
                Keys = new List<Key>
                {
                    Key.N
                },
                Text1 = "orb"
            },
            new Shortcut()
            {
                //Attack range switch
                Keys = new List<Key>
                {
                    Key.M
                },
                Text1 = "rng"
            }
        };

        private Key _orbWalkerActivationKey = Key.L;

        private char _showRangeKey = 'C';

        private char _attackChampionOnlyKey = 'Z';

        private char _attackOnCursorKey = 'X';

        private char _moveChampionKey = 'V';

        //move = V
        //Attack on cursor = X
        //Attcak champion only on down = Z

        private Thread _orbWalkerThread;

        private Thread _statScraperThread;

        private bool _gameStarted = false;

        private bool _showRange = false;

        private double _currentPlayerAttackSpeed = 0;

        private double _windupOffset = 300;

        private double _windup = 1500; // in ms

        private string _currentChampion = "";

        public OrbWalkerLogic(KeyInjection keyInjection) : base(keyInjection)
        {
            Log.WriteInfo($"OrbWalker starting...");
            _orbWalkerThread = new Thread(OrbWalk);
            _orbWalkerThread.SetApartmentState(ApartmentState.STA);
            _orbWalkerThread.Start();

            _statScraperThread = new Thread(PlayerStatsScraper);
            _statScraperThread.SetApartmentState(ApartmentState.STA);
            _statScraperThread.Start();

            if (_orbWalkerThread.IsAlive)
            {
                Log.WriteInfo($"OrbWalker started in thread {_orbWalkerThread.ManagedThreadId}");
            }

            if (_statScraperThread.IsAlive)
            {
                Log.WriteInfo($"Player stats scraper started in thread {_statScraperThread.ManagedThreadId}");
            }
        }

        private void OrbWalk()
        {
            var canAttack = true;
            var championAttackOnlyIsToggled = false;

            var sw = new Stopwatch();
            while (true)
            {
                if (_gameStarted)
                {
                    if (Keyboard.IsKeyDown(_orbWalkerActivationKey))
                    {
                        CalculateWindup();

                        if (!championAttackOnlyIsToggled)
                        {
                            KeyInjector.PressKeyAsync(KeyCodeCharWrapper.GetKey(_attackChampionOnlyKey), false);
                            championAttackOnlyIsToggled = true;
                        }

                        if (canAttack)
                        {
                            Log.WriteDebug("Attacking...");

                            TypeKey(_attackOnCursorKey.ToString(), 10000);

                            Thread.Sleep(260);//need to be dynamics

                            canAttack = false;
                            
                            sw.Start();
                        }
                        else
                        {
                            if(sw.ElapsedMilliseconds > _windup - _windupOffset)
                            {
                                Log.WriteDebug("windup passed");
                                canAttack = true;
                                sw.Reset();
                            }
                            TypeKey(_moveChampionKey.ToString(), 10000);

                        }

                        Thread.Sleep(25);
                    }
                    else
                    {
                        if (championAttackOnlyIsToggled)
                        {
                            KeyInjector.PressKeyAsync(KeyCodeCharWrapper.GetKey(_attackChampionOnlyKey), true);
                            championAttackOnlyIsToggled = false;
                        }
                    }
                }
                else
                {
                    Thread.Sleep(1000);
                }
            }
        }

        protected override void Listen()
        {
            //test
            Log.WriteDebug("Current champ name : " + ApiScraper.PlayerChampionName());
            Log.WriteDebug("Current attack speed : " + ApiScraper.PlayerAttackSpeed());

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

        private void CalculateWindup()
        {
            if (_currentPlayerAttackSpeed == 0) return; 
            var cache = 1 / _currentPlayerAttackSpeed;
            _windup = cache * 1000;
        }

        private void PlayerStatsScraper()
        {
            while (true)
            {
                if (_gameStarted)
                {
                    _currentPlayerAttackSpeed = Convert.ToDouble(ApiScraper.PlayerAttackSpeed());
                    Log.WriteDebug($"Current player auto attack speed is {_currentPlayerAttackSpeed}");
                }
                Thread.Sleep(750);
            }
        }

        private void PlayerChampionScraper()
        {
            _currentChampion = ApiScraper.PlayerChampionName();

            _windup = ChampionWindupData.GetChampionWindup(_currentChampion);

            Log.WriteInfo($"Your champion is : {_currentChampion}");
            Log.WriteInfo($"Base windup for {_currentChampion} is {_windup}");
        }

        private string ParseSugarText(string text)
        {
            if (text.Contains("orb"))
            {
                _gameStarted = !_gameStarted;
                Log.WriteInfo($"OrbWalker : game started set to {_gameStarted}");
                if (_gameStarted)
                {
                    PlayerChampionScraper();
                }
            }
            else if (text.Contains("rng"))
            {
                _showRange = !_showRange;
                Log.WriteInfo($"OrbWalker : show range set to {_showRange}");
                KeyInjector.PressKeyAsync(KeyCodeCharWrapper.GetKey(_showRangeKey), !_showRange);
            }
            return text;
        }

    }
}
