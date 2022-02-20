using System;

namespace aplibsharp
{
    internal class esa
    {
        public readonly byte[] input;
        public readonly int[] sa;
        public readonly int[] isa;
        public readonly int[] lcp;
        public readonly int[] l1o;

        public esa(byte[] input)
        {
            this.input = input;
            if (input.Length > 0)
            {
                sa = SuffixArray.SAIS.sa(input);
                isa = _isa(sa);
                lcp = _lcp(input, sa, isa);
                l1o = _l1o(input);
            }
        }

        static private int[] _isa(int[] sa)
        {
            var output = new int[sa.Length];

            for (int i = 0; i < sa.Length; i++)
            {
                output[sa[i]] = i;
            }

            return output;
        }

        static public int[] _l1o(byte[] input)
        {
            var output = new int[input.Length];

            var t = new int[byte.MaxValue + 1];
            for (int i = 0; i < t.Length; i++)
            {
                t[i] = -1;
            }

            for (int i = 0; i < input.Length; i++)
            {
                int o = i - t[input[i]];
                if (input[i] == 0x00)
                {
                    output[i] = 0;
                }
                else if (t[input[i]] > -1 && o < constant.threshold_byte_match_offset)
                {
                    output[i] = o;
                }
                else
                {
                    output[i] = 0;
                }
                t[input[i]] = i;
            }

            return output;
        }

        //linear-time longest-common-prefix computation in suffix arrays and its applications - toru kasai 2001
        //http://alumni.cs.ucr.edu/~rakthant/cs234/01_KLAAP_Linear%20time%20LCP.PDF - page 6 "algorithm getheight"
        static public int[] _lcp(byte[] input, int[] sa, int[] isa)
        {
            var output = new int[input.Length];

            int h = 0;
            for (int i = 0; i < input.Length; i++)
            {
                int k = isa[i];
                if (k > 0)
                {
                    int j = sa[k - 1];
                    while (i + h < input.Length && j + h < input.Length && input[i + h] == input[j + h])
                    {
                        h++;
                    }
                    output[k] = h;
                }
                else
                {//k=0 undefined
                    output[k] = -1;
                }
                if (h > 0)
                {
                    h--;
                }
            }

            return output;
        }

        internal void edges_left(int i, ValueTuple<int, int>[] edges, ref int ep, ref int runlength, ref int maxlength)
        {
            var pl = lpfl(i);
            int slp = pl.Item1;
            int length = pl.Item2;
            // int slp = isa[i];
            // int length = lcp[slp];
            maxlength = length;
            int offset = 0;

            while (slp >= 0 && length > 1)
            {
                offset = i - sa[slp];
                if (offset > 0 && offset < constant.threshold_offset)
                {
                    edges[ep].Item1 = length;
                    edges[ep].Item2 = offset;
                    ep++;
                    if (offset == 1)
                    {
                        runlength = length;
                    }
                }
                length = Math.Min(length, lcp[slp]);
                slp--;
            }
        }

        internal void edges_right(int i, ValueTuple<int, int>[] edges, ref int ep, ref int runlength, ref int maxlength)
        {
            var pl = lpfr(i);
            int slp = pl.Item1;
            int length = pl.Item2;
            // int slp = isa[i] + 1;
            // int length = slp >= lcp.Length ? 0 : lcp[slp];
            maxlength = length;
            int offset = 0;

            while (length > 1)
            {
                offset = i - sa[slp];
                if (offset > 0 && offset < constant.threshold_offset)
                {
                    edges[ep].Item1 = length;
                    edges[ep].Item2 = offset;
                    ep++;
                    if (offset == 1)
                    {
                        runlength = length;
                    }
                }
                slp++;
                length = slp < lcp.Length ? Math.Min(length, lcp[slp]) : 0;
            }
        }

        internal Tuple<int, int> lpfl(int index)
        {
            int lp = isa[index];
            int ll = lcp[lp];
            int lo = lp >= 0 ? index - sa[lp] : -1;
            while (ll > 1 && lp >= 0 && (lo < 1 || lo >= constant.threshold_offset))
            {
                ll = Math.Min(ll, lcp[lp]);
                lp--;
                lo = lp >= 0 ? index - sa[lp] : -1;
            }
            return new Tuple<int, int>(lp, ll);
        }

        internal Tuple<int, int> lpfr(int index)
        {
            int rp = isa[index] + 1;
            int rl = rp >= lcp.Length ? 0 : lcp[rp];
            int ro = rp < lcp.Length ? index - sa[rp] : -1;
            while (rl > 1 && rp < lcp.Length && (ro < 1 || ro >= constant.threshold_offset))
            {
                rp++;
                rl = rp < lcp.Length ? Math.Min(rl, lcp[rp]) : 0;
                ro = rp < lcp.Length ? index - sa[rp] : -1;
            }
            return new Tuple<int, int>(rp, rl);
        }

        internal int nextpeak(int index, int length)
        {
            int i = index;
            int j = index + length;
            int ir = j;
            while (i <= j && i < input.Length)
            {
                int m = (i + j) / 2;
                int l = Math.Max(1, Math.Max(lpfl(m).Item2, lpfr(m).Item2));
                int mr = m + l;
                if (mr > ir)
                {
                    j = m - 1;
                }
                else
                {
                    i = m + 1;
                }
            }
            return i;
        }

    }
}