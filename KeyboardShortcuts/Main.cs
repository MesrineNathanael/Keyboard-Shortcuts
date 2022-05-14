using KeyboardShortcuts.Vendor.Common;
using KeyboardShortcuts.Commons;
using KeyboardShortcuts.Vendor;
using System;
using System.Threading;
using KeyboardShortcuts.Commons.Enums;
using KeyboardShortcuts.Vendor.League.Orbwalker;

namespace KeyboardShortcuts
{
    public class Main
    {
        public readonly KeyInjection KeyInjector = new KeyInjection();

        public readonly MouseInputs MouseInputs = new MouseInputs();

        private bool _isRunning = true;

        public void Start()
        {
            Console.WriteLine(AsciiTitle.GetRandomTitle());

            Log.CarriageReturn();

            Log.WriteInfo("Made by Noodle Studio with <3 and noodles");

            Log.CarriageReturn();
            Log.WriteInfo("Keyboard Shortcuts starting...");

            // call your Vendor part here
            //LeagueLogic leagueLogic = new LeagueLogic(KeyInjector);
            
            //leagueLogic.Start();

            OrbWalkerLogic orbWalkerLogic = new OrbWalkerLogic(KeyInjector, MouseInputs);

            orbWalkerLogic.Start();
            //

            Log.CarriageReturn();
            Log.WriteInfo("Press escape to exit");
            Log.CarriageReturn();
            
            while (_isRunning)
            {
                Thread.Sleep(50);

                var key = Console.ReadKey();
                if (key.Key == ConsoleKey.Escape)
                {
                    _isRunning = false;
                    Environment.Exit(0);
                }
            }
        }
    }
}
