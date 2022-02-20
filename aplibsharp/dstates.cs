using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace aplibsharp
{
    internal class dstates
    {
        private readonly Dictionary<int, state> d;

        public state this[int key]
        {
            get
            {
                state output = null;
                d.TryGetValue(key, out output);
                return output;
            }
            set
            {
                state dstate = null;
                if (d.TryGetValue(key, out dstate))
                {
                    Contract.Assert(state.cheapest(dstate, value) == value);
                    d[key] = value;
                }
                else
                {
                    d.Add(key, value);
                }
            }
        }

        public dstates()
        {
            d = new Dictionary<int, state>();
        }

        public void clear()
        {
            d.Clear();
        }

        public void delete(int key)
        {
            d.Remove(key);
        }

    }
}