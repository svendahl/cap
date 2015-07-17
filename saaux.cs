//using System;
//using System.Collections.Generic;

namespace cap
{
	static class saaux
	{



		public static int[] lcp(byte[] input, int[] sa, int[] isa)//http://www.cs.ucdavis.edu/~gusfield/cs224f09/lcp09.pdf
		{
			var lcp = new int[input.Length];
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
					lcp[k] = h;
				}

				if (h > 0)
				{
					h--;
				}
			}
			return lcp;
		}



		public static int[] isa(int[] sa)
		{
			var output = new int[sa.Length];

			for (int i = 0; i < sa.Length; i++)
			{
				output[sa[i]] = i;
			}

			return output;
		}



		////longest previous factor
		//public static void lpf(int[] sa, int[] isa, int[] lcp, ref int[] ref_lpf, ref int[] ref_prevocc)
		//{
		//    var psv = new int[sa.Length];
		//    var nsv = new int[sa.Length];

		//    var s1 = new Stack<int>();
		//    var s2 = new Stack<int>();

		//    for (int i = sa.Length - 1; i >= 0; i--)
		//    {
		//        while (s1.Count > 0 && sa[s1.Peek()] > sa[i])
		//        {
		//            psv[s1.Peek()] = i;
		//            s1.Pop();
		//        }
		//        s1.Push(i);
		//    }

		//    for (int i = 0; i < sa.Length; i++)
		//    {
		//        while (s2.Count > 0 && sa[s2.Peek()] > sa[i])
		//        {
		//            nsv[s2.Peek()] = i;
		//            s2.Pop();
		//        }
		//        s2.Push(i);
		//    }

		//    while (s1.Count > 0)
		//    {
		//        psv[s1.Peek()] = -1;
		//        s1.Pop();
		//    }

		//    while (s2.Count > 0)
		//    {
		//        nsv[s2.Peek()] = -1;
		//        s2.Pop();
		//    }

		//    for (int i = 0; i < sa.Length; i++)
		//    {
		//        int p = psv[i] == -1 ? 0 : lcpq(lcp, isa, sa[psv[i]], sa[i]);
		//        int n = nsv[i] == -1 ? 0 : lcpq(lcp, isa, sa[nsv[i]], sa[i]);
		//        ref_lpf[sa[i]] = (p > n) ? p : n;
		//        ref_prevocc[sa[i]] = ((p > n) ? p == 0 : n == 0) ? -1 : sa[(p > n) ? psv[i] : nsv[i]];
		//    }
		//}



		//// LCP query
		//public static int lcpq(int[] lcp, int[] isa, int i, int j)
		//{
		//    if (i < 0 || j < 0)
		//    {
		//        return 0;
		//    }

		//    if (i==j)
		//    {
		//        return lcp.Length-i;
		//    }

		//    int l=isa[i];
		//    int r=isa[j];
		//    if (l>r)
		//    {
		//        int temp=l; l=r; r=temp;
		//    }
		//    return lcp[rmq(lcp,l+1,r)];
		//}



		//// Longest common subsequence of two strings LCS query
		//public static int lcsq(int[] lcp, int[] isa, int i, int j)
		//{
		//    return lcpq(lcp, isa, lcp.Length - i - 1, lcp.Length - j - 1);
		//}



		//// Range minimum query
		//public static int rmq(int[] lcp, int i, int j)
		//{
		//    int min = i;

		//    if (i > j)
		//    {
		//        int temp = i;
		//        i = j;
		//        j = temp;
		//    }

		//    for (int k = i + 1; k <= j; k++)
		//    {
		//        if (lcp[k] < lcp[min])
		//        {
		//            min = k;
		//        }
		//    }

		//    return min;
		//}



	} // class
} // namespace
