using System.Collections.Generic;
using System.Windows.Input;

namespace KeyboardShortcuts.Vendor.Common
{
    public class Shortcut
    {
        public List<Key> Keys;

        public string Text1 = "";

        public string Text2 = "";        

        public string Var1 = "";
        
        public string Var2 = "";

        /// <summary>
        /// Return a the result in format "{Text1} {Var1} {Text2} {Var2}"
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{Text1} {Var1} {Text2} {Var2}";
        }
    }
}
