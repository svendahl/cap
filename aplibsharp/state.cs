using System.Diagnostics.Contracts;

namespace aplibsharp
{
    internal class state
    {
        public int bitcost { get; set; }
        public int index { get; set; }
        public int length { get; set; }
        public int offset { get; set; }
        public int steps { get; set; }
        public int roffs { get; set; }
        public state parent { get; set; }

        public int lwm
        {
            get
            {
                return length > 1 ? 1 : 0;
            }
        }

        public state()
        {
            bitcost = 0;
            index = 0;
            length = 0;
            offset = 0;
            parent = null;
            roffs = 0;
            steps = 0;
        }

        public state(state state, int length, int offset, int edgecost)
        {
            bitcost = state.bitcost + edgecost;
            index = state.index + length;
            this.length = length;
            this.offset = offset;
            parent = state;
            roffs = length > 1 ? offset : state.roffs;
            steps = state.steps + 1;
        }

        public static bool cheaper(state estate, state state, int length, int offset, int edgecost)
        {
            Contract.Assert(estate != null && state != null && estate.index == state.index + length);

            int newbitcost = state.bitcost + edgecost;
            int newsteps = state.steps + 1;
            int newlwm = length > 1 ? 1 : 0;
            int newroffs = length > 1 ? offset : state.roffs;

            return newbitcost < estate.bitcost ||
                    newbitcost == estate.bitcost && newsteps < estate.steps ||
                    newbitcost == estate.bitcost && newsteps == estate.steps && newlwm == 1 && estate.lwm == 0 ||
                    newbitcost == estate.bitcost && newsteps == estate.steps && newlwm == 1 && estate.lwm == 1 && newroffs < estate.roffs;//anally retentive
        }

        public static state cheapest(state s1, state s2)
        {
            Contract.Assert(s1 != null && s2 != null && s1.index == s2.index);

            state output = null;

            if (s1.bitcost < s2.bitcost ||
                s1.bitcost == s2.bitcost && s1.steps < s2.steps ||
                s1.bitcost == s2.bitcost && s1.steps == s2.steps && s1.lwm == 1 && s2.lwm == 0 ||
                s1.bitcost == s2.bitcost && s1.steps == s2.steps && s1.lwm == 1 && s2.lwm == 1 && s1.roffs < s2.roffs)//anally retentive
            {
                output = s1;
            }
            else
            {
                output = s2;
            }

            return output;
        }

    }
}
