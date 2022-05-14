using KeyboardShortcuts.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyboardShortcuts.Vendor.League.Data
{
    public class ChampionWindupData
    {
        private static Dictionary<string, int> Windups = new Dictionary<string, int>
        {
            {"Vayne", 260 },
            {"Kindred", 260 },
            {"Kayle", 240 },
            {"Gangplank", 240 }
        };

        public static int GetChampionWindup(string name)
        {
            var result = 0;

            try
            {
                result = Windups[name];
            }
            catch(Exception e) 
            {
                Log.WriteError("Your champion does not exist in database.");
            }
            return result;
        }
    }
}
