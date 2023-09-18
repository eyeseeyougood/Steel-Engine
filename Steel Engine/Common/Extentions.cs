using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steel_Engine.Common
{
    public static class Extentions
    {
        public static bool ContainsAnInstanceEqualTo<T>(this List<T> list, T example)
        {
            bool found = false;

            foreach (T element in list)
            {
                if (example.Equals(element))
                {
                    found = true;
                    break;
                }
            }

            return found;
        }
    }
}
