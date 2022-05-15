using KeyboardShortcuts.Commons;
using KeyboardShortcuts.Commons.Enums;
using KeyboardShortcuts.Vendor.Common;
using KeyboardShortcuts.Vendor.League.API;
using KeyboardShortcuts.Vendor.League.Data;
using KeyboardShortcuts.Vendor.League.PixelBot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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

        PixelSearchArray searchArray = new PixelSearchArray();

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

        private Thread _enemyHpScanner;

        private bool _gameStarted = false;

        private bool _showRange = false;

        private double _currentPlayerAttackSpeed = 1;

        private double _windupOffset = 300;

        private double _windup = 1500; // in ms

        private int _championAnimationPause = 250;

        private string _currentChampion = "";

        public static int offsetX = 0;
        public static int offsetY = 95;

        public OrbWalkerLogic(KeyInjection keyInjection, MouseInputs mouseInputs) : base(keyInjection, mouseInputs)
        {
            Log.WriteInfo($"OrbWalker starting...");
            _orbWalkerThread = new Thread(OrbWalk);
            _orbWalkerThread.SetApartmentState(ApartmentState.STA);
            _orbWalkerThread.Start();

            _statScraperThread = new Thread(PlayerStatsScraper);
            _statScraperThread.SetApartmentState(ApartmentState.STA);
            _statScraperThread.Start();

            _enemyHpScanner = new Thread(ScanEnemyHP);
            _enemyHpScanner.SetApartmentState(ApartmentState.STA);
            _enemyHpScanner.Start();

            if (_orbWalkerThread.IsAlive)
            {
                Log.WriteInfo($"OrbWalker started in thread {_orbWalkerThread.ManagedThreadId}");
            }

            if (_statScraperThread.IsAlive)
            {
                Log.WriteInfo($"Player stats scraper started in thread {_statScraperThread.ManagedThreadId}");
            }

            if (_enemyHpScanner.IsAlive)
            {
                Log.WriteInfo($"Enemy HP scanner started in thread {_enemyHpScanner.ManagedThreadId}");
            }
        }

        private void OrbWalk()
        {
            var canAttack = true;
            var championAttackOnlyIsToggled = false;

            var sw = new Stopwatch();
            var lastmovepos = new Point();
            var moveTime = 3;
            var moveTimeCount = 0;
            while (true)
            {
                if (_gameStarted)
                {
                    if (Keyboard.IsKeyDown(_orbWalkerActivationKey))
                    {
                        CalculateWindup();

                        if (!championAttackOnlyIsToggled)
                        {
                            //KeyInjector.PressKeyAsync(KeyCodeCharWrapper.GetKey(_attackChampionOnlyKey), false);
                            championAttackOnlyIsToggled = true;
                        }

                        if (canAttack)
                        {
                            lastmovepos = MouseInputs.GetPosition();

                            Log.WriteDebug("Attacking...");
                            var enemyPos = new Point();
                            if (searchArray.enemyHPArrayGlobal.Count() > 0)
                            {
                                enemyPos = searchArray.enemyHPArrayGlobal[searchArray.enemyHPArrayGlobal.Count() / 2];
                            }

                            if (searchArray.enemyHPArrayGlobal.Count() > 0)
                            {
                                MouseInputs.SetPosition(enemyPos.X, enemyPos.Y + offsetY);
                                if(searchArray.enemyHPArrayGlobal.Count() < 250)
                                {
                                    Log.WriteWarning("Low enemy array, click can be inacurate");
                                }
                            }

                            TypeKey(_attackOnCursorKey.ToString(), 10000);

                            Thread.Sleep(30);

                            if (lastmovepos.X != 0)
                            {
                                MouseInputs.SetPosition(lastmovepos.X, lastmovepos.Y);
                            }

                            Thread.Sleep(250-30);

                            canAttack = false;
                            
                            sw.Start();
                        }
                        else
                        {
                            if(sw.ElapsedMilliseconds > _windup - _windupOffset)
                            {
                                canAttack = true;
                                sw.Reset();
                            }

                            if (moveTimeCount >= moveTime) 
                            {
                                TypeKey(_moveChampionKey.ToString(), 10000);
                                moveTimeCount = 0;
                            }
                            else
                            {
                                moveTimeCount++;
                            }

                        }

                        Thread.Sleep(25);
                    }
                    else
                    {
                        if (championAttackOnlyIsToggled)
                        {
                            //KeyInjector.PressKeyAsync(KeyCodeCharWrapper.GetKey(_attackChampionOnlyKey), true);
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
                    if (_currentPlayerAttackSpeed == 0)
                    {
                         Log.WriteWarning("Player current attack speed can't be detected.");
                        _currentPlayerAttackSpeed = 1;
                    }
                    var autoA = ApiScraper.PlayerAttackSpeed();

                    if(autoA != "")
                        _currentPlayerAttackSpeed = Convert.ToDouble(autoA);
                    
                    //Log.WriteDebug($"Current player auto attack speed is {_currentPlayerAttackSpeed}");
                }
                Thread.Sleep(500);
            }
        }

        private void PlayerChampionScraper()
        {
            _currentChampion = ApiScraper.PlayerChampionName();

            _championAnimationPause = ChampionWindupData.GetChampionWindup(_currentChampion);

            Log.WriteInfo($"Your champion is : {_currentChampion}");
            Log.WriteInfo($"Base windup for {_currentChampion} is {_championAnimationPause}ms");
        }

        private void ScanEnemyHP()
        {
            var pxbot = new PixelSearch();
            string ENEMYHP = "#a52c21";
            Color ENEMYcolor = ColorTranslator.FromHtml(ENEMYHP);
            while (true)
            {
                if (_gameStarted)
                {
                    Point[] enemyArray = pxbot.Search(new Rectangle(0, 0, 1920, 1080), ENEMYcolor, 0);
                    searchArray.enemyHPArrayGlobal = enemyArray;
                    if(enemyArray.Count() > 0)
                    {
                        //Log.WriteDebug($"Enemy found at {enemyArray[enemyArray.Count() / 2].X};{enemyArray[enemyArray.Count() / 2].Y}");
                    }
                }
                Thread.Sleep(100);
            }
        }

        private string ParseSugarText(string text)
        {
            if (text.Contains("orb"))
            {
                _gameStarted = !_gameStarted;
                Log.WriteInfo($"OrbWalker : game started set to {_gameStarted}");
                if (_gameStarted)
                {
                    //PlayerChampionScraper();
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
