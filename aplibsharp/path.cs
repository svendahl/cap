using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace aplibsharp
{
    static internal class path
    {
        static public Tuple<int, int>[] bfs(byte[] input)
        {
            var esa = new esa(input);

            var cslarr = new state[constant.threshold_offset];
            var csrarr = new state[constant.threshold_offset];
            var rsarr = new state[constant.threshold_offset];

            var edgesl = new ValueTuple<int, int>[constant.threshold_offset];
            var edgesr = new ValueTuple<int, int>[constant.threshold_offset];
            for (int i = 0; i < edgesr.Length; i++)
            {
                edgesl[i] = new ValueTuple<int, int>(0, 0);
                edgesr[i] = new ValueTuple<int, int>(0, 0);
            }

            int runlength_index = 0;
            int runlength_length = 0;
            int minlength = 0;

            var dstates = new dstates();
            var state = new state();

            while (state.index < input.Length)
            {
                int edgeslcount = 0;
                int edgesrcount = 0;

                int maxlength = edges(state.index, esa, edgesl, edgesr, ref edgeslcount, ref edgesrcount, ref runlength_index, ref runlength_length, ref minlength);
                if (maxlength <= constant.threshold_length)
                {
                    expand(state, esa, edgesl, edgesr, edgeslcount, edgesrcount, cslarr, csrarr, rsarr);
                    new cstates(dstates, cslarr, csrarr, edgeslcount, edgesrcount).go();
                    state nstate = length1(state, esa);
                    if (dstates[nstate.index] == null || state.cheapest(dstates[nstate.index], nstate) == nstate)
                    {
                        dstates[nstate.index] = nstate;
                    }
                    dstates.delete(state.index);
                    state = dstates[state.index + 1];
                    if (runlength_length >= constant.threshold_greedy_runlength)
                    {
                        state = runlength(dstates, ref runlength_index, ref runlength_length, ref minlength);
                    }
                }
                else
                {
                    int offset = edgeslcount > 0 && edgesl[0].Item1 == maxlength ? edgesl[0].Item2 : edgesr[0].Item2;
                    state = largeedge(state, esa, maxlength, offset);
                    dstates.clear();
                }
            }

            if (!verifyparse(input, state))
            {
                System.Console.WriteLine("Verifying parse failed, exiting");
                Environment.Exit(-1);
            }

            return extractpath(state);
        }

        static private int edges(int i, esa esa, ValueTuple<int, int>[] edgesl, ValueTuple<int, int>[] edgesr, ref int edgeslcount, ref int edgesrcount, ref int runlength_index, ref int runlength_length, ref int minlength)
        {
            int edgeslc = 0;
            int edgesrc = 0;
            int runlength = 0;
            int maxlengthl = 0;
            int maxlengthr = 0;
            Parallel.Invoke
            (
                () => esa.edges_left(i, edgesl, ref edgeslc, ref runlength, ref maxlengthl),
                () => esa.edges_right(i, edgesr, ref edgesrc, ref runlength, ref maxlengthr)
            );
            if (runlength > constant.threshold_greedy_runlength)
            {
                runlength_index = i;
                runlength_length = runlength;
                minlength = Math.Max(esa.nextpeak(i, runlength) - i, 1);
            }
            else
            {
                minlength = 1;
            }
            edgeslcount = edgeslc;
            edgesrcount = edgesrc;
            return Math.Max(maxlengthl, maxlengthr);
        }

        static private void expand(state state, esa esa, ValueTuple<int, int>[] edgesl, ValueTuple<int, int>[] edgesr, int edgeslcount, int edgesrcount, state[] cslarr, state[] csrarr, state[] rsarr)
        {
            Parallel.For(0, edgeslcount, i =>
            {
                int length = edgesl[i].Item1;
                int offset = edgesl[i].Item2;
                expand(state, i, length, offset, esa, ref cslarr[i], ref rsarr[offset]);
            });
            Parallel.For(0, edgesrcount, i =>
            {
                int length = edgesr[i].Item1;
                int offset = edgesr[i].Item2;
                expand(state, i, length, offset, esa, ref csrarr[i], ref rsarr[offset]);
            });
        }

        static private void expand(state state, int edgep, int length, int offset, esa esa, ref state cstate, ref state rstate)
        {
            bool valid = model.valid(state, length, offset);
            if (valid)
            {
                int edgecost = model.cost(state, length, offset);
                cstate = new state(state, length, offset, edgecost);
            }
            else
            {
                cstate = null;
            }

            if (rstate != null)
            {
                if (rstate.index < state.index)
                {
                    int gap = state.index - rstate.index;
                    int bestcasenewrstatecost = rstate.bitcost + (gap * (constant.bitcost_prefixcode_byte_match + constant.bitcost_offset_byte_match)) + model.repcost(length);
                    if ((valid && bestcasenewrstatecost <= cstate.bitcost) || (!valid && gap <= constant.threshold_gap_length))
                    {
                        state nrstate = rstate;
                        while (nrstate.index < state.index)
                        {
                            int l1edgecost = model.l1cost(nrstate, esa.l1o[nrstate.index], esa.input);
                            nrstate = new state(nrstate, 1, esa.l1o[nrstate.index], l1edgecost);
                        }
                        int edgecost = model.cost(nrstate, length, offset);
                        nrstate = new state(nrstate, length, offset, edgecost);

                        if (valid)
                        {
                            nrstate = state.cheapest(cstate, nrstate);
                        }
                        cstate = nrstate;
                        rstate = nrstate;
                    }
                    else if (valid)
                    {
                        rstate = cstate;
                    }
                    else
                    {
                        rstate = null;
                    }
                }
                else if (valid && state.cheapest(rstate, cstate) == cstate)
                {
                    rstate = cstate;
                }
            }
            else if (valid)
            {
                rstate = cstate;
            }
        }

        static private Tuple<int, int>[] extractpath(state state)
        {
            var output = new Tuple<int, int>[state.steps];

            while (state.parent != null)
            {
                output[state.parent.steps] = new Tuple<int, int>(state.length, state.offset);
                state = state.parent;
            }

            return output;
        }

        static private state largeedge(state state, esa esa, int length, int offset)
        {
            int edgecost;
            while (length > constant.threshold_length)
            {
                edgecost = model.cost(state, constant.threshold_length, offset);
                state = new state(state, constant.threshold_length, offset, edgecost);
                length -= constant.threshold_length;
                state = length1(state, esa);
                length--;
            }
            if (length > 1)
            {
                edgecost = model.cost(state, length, offset);
                state = new state(state, length, offset, edgecost);
            }
            else if (length == 1)
            {
                state = length1(state, esa);
            }
            return state;
        }

        static private state length1(state state, esa esa)
        {
            int l1o = model.l1valid(state, esa.l1o[state.index]) ? esa.l1o[state.index] : 0;
            int l1c = model.l1cost(state, l1o, esa.input);
            state output = new state(state, 1, l1o, l1c);
            return output;
        }

        static private state runlength(dstates dstates, ref int runlength_index, ref int runlength_length, ref int minlength)
        {
            runlength_index++;
            runlength_length--;
            minlength--;

            state state = dstates[runlength_index];
            int runlength_stop = runlength_index + runlength_length;
            while (minlength > 0)
            {
                if (state != null && model.valid(state, runlength_length, 1))
                {
                    int cost = model.cost(state, runlength_length, 1);
                    if (dstates[state.index + runlength_length] == null || state.cheaper(dstates[state.index + runlength_length], state, runlength_length, 1, cost))
                    {
                        state nstate = new state(state, runlength_length, 1, cost);
                        dstates[nstate.index] = nstate;
                    }
                }
                dstates.delete(runlength_index);
                runlength_index++;
                runlength_length--;
                minlength--;
                state = dstates[runlength_index];
            }
            runlength_index = 0;
            runlength_length = 0;
            minlength = 0;

            return state;
        }

        static private bool verifyparse(byte[] input, state state)
        {
            return verifyparse1(input, state) && verifyparse2(input, state);
        }

        static private bool verifyparse1(byte[] input, state state)//length & offset verification
        {
            while (state.length > 0)
            {
                int offset = state.offset;
                if (offset > 0)
                {
                    int index = state.parent.index;
                    int length = state.length;
                    int src = index - offset;
                    for (int l = 0; l < length; l++)
                    {
                        if (input[index + l] != input[src + l])
                        {
                            System.Console.WriteLine(index + ":" + length + ":" + offset);
                            return false;
                        }
                    }
                }
                state = state.parent;
            }

            return true;
        }

        static private bool verifyparse2(byte[] input, state state)//states & bitcost verification
        {
            var array = new state[0];
            {
                var s = new Stack<state>();
                state ss = state;
                while (ss != null)
                {
                    s.Push(ss);
                    ss = ss.parent;
                }
                array = s.ToArray();
            }

            for (int i = 0; i < array.Length - 1; i++)
            {
                state = array[i];
                int length = array[i + 1].length;
                int offset = array[i + 1].offset;
                if (length > 1)
                {
                    if (!model.valid(state, length, offset))
                    {
                        System.Console.WriteLine(state.index + ":" + length + ":" + offset);
                        return false;
                    }
                }
                int expectedbitcost = array[i + 1].bitcost - array[i].bitcost;
                int evaluatedbitcost = length > 1 ? model.cost(state, length, offset) : model.l1cost(state, offset, input);
                if (evaluatedbitcost != expectedbitcost)
                {
                    System.Console.WriteLine("bitcost");
                    return false;
                }
            }
            return true;
        }

    }
}
