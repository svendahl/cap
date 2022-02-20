using System;

namespace aplibsharp
{
    internal class cstates
    {
        private readonly dstates d;
        private readonly state[] l;
        private readonly state[] r;
        private readonly int lc;
        private readonly int rc;
        private int lp;
        private int rp;

        public cstates(dstates d, state[] l, state[] r, int lc, int rc)
        {
            this.d = d;
            this.l = l;
            this.r = r;
            this.lc = lc;
            this.rc = rc;
            lp = 0;
            rp = 0;
        }

        public void go()
        {
            state tstate = null;

            if (getnextlength(out tstate))
            {
                state next = tstate;
                state current = null;
                int length = tstate.length;
                while (getnextlength(out tstate))
                {
                    current = next;
                    next = tstate;
                    if (d[current.index] == null || state.cheapest(d[current.index], current) == current)
                    {
                        d[current.index] = current;
                    }
                    length = current.length - 1;
                    while (length > next.length && model.valid(current.parent, length, current.offset))
                    {
                        int cost = model.cost(current.parent, length, current.offset);
                        if (d[current.parent.index + length] == null || state.cheaper(d[current.parent.index + length], current.parent, length, current.offset, cost))
                        {
                            state nstate = new state(current.parent, length, current.offset, cost);
                            d[nstate.index] = nstate;
                        }
                        length--;
                    }
                    length = next.length;
                    if (model.valid(current.parent, length, current.offset))
                    {
                        int cost = model.cost(current.parent, length, current.offset);
                        if (state.cheaper(next, current.parent, length, current.offset, cost))
                        {
                            next = new state(current.parent, length, current.offset, cost);
                        }
                    }
                }
                current = next;
                if (d[current.index] == null || state.cheapest(d[current.index], current) == current)
                {
                    d[current.index] = current;

                }
                length = current.length - 1;
                while (length > 1 && model.valid(current.parent, length, current.offset))
                {
                    int cost = model.cost(current.parent, length, current.offset);
                    if (d[current.parent.index + length] == null || state.cheaper(d[current.parent.index + length], current.parent, length, current.offset, cost))
                    {
                        state nstate = new state(current.parent, length, current.offset, cost);
                        d[nstate.index] = nstate;
                    }
                    length--;
                }
            }
        }

        private bool getnextlength(out state ostate)
        {
            bool output = false;

            int length = getmaxlength();

            state tstate = null;
            state cstate = null;
            if (lp < lc && peekl(out tstate) && tstate.length == length)
            {
                cstate = tstate;
            }
            else if (rp < rc && peekr(out tstate) && tstate.length == length)
            {
                cstate = tstate;
            }

            while (lp < lc && peekl(out tstate) && tstate.length == length)
            {
                cstate = state.cheapest(cstate, tstate);
                l[lp++] = null;
            }
            while (rp < rc && peekr(out tstate) && tstate.length == length)
            {
                cstate = state.cheapest(cstate, tstate);
                r[rp++] = null;
            }
            output = cstate != null;
            ostate = cstate;
            return output;
        }

        private int getmaxlength()
        {
            int output = -1;
            state tstate;
            if (peekl(out tstate))
            {
                output = Math.Max(output, tstate.length);
            }
            if (peekr(out tstate))
            {
                output = Math.Max(output, tstate.length);
            }
            return output;
        }

        private bool peekl(out state state)
        {
            state = null;
            bool output = peek(l, lc, ref lp, out state);
            return output;
        }

        private bool peekr(out state state)
        {
            state = null;
            bool output = peek(r, rc, ref rp, out state);
            return output;
        }

        static private bool peek(state[] array, int stop, ref int p, out state state)
        {
            bool output = false;
            while (p < stop && array[p] == null)
            {
                p++;
            }
            state = null;
            if (p < stop)
            {
                state = array[p];
                output = true;
            }
            return output;
        }

    }
}
