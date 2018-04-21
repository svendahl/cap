using System;
using System.Collections.Generic;

namespace cap
{
	class esa
	{



		public readonly byte[] input;



		public readonly int[] sa;
		public readonly int[] isa;
		public readonly int[] lcp;



		public readonly int[] l1o;
		public readonly interval lit;
		public readonly interval[] litarray;



		public readonly int[] snsv;
		public readonly int[] spsv;
		public readonly int[] snlv;
		public readonly int[] splv;



		public esa (byte[] input)
		{
			this.input = input;

			sa = SuffixArray.SAIS.sa(input);
			isa = _isa(sa);
			lcp = _lcp(input, sa, isa);
			l1o = _l1o(input);
			lit = _lit(lcp);
			litarray = _litarray(lit);
			spsv = _psv(sa);
			snsv = _nsv(sa);
			splv = _plv(sa);
			snlv = _nlv(sa);
		}



		public static IEnumerable<interval> _dfs(interval interval, interval blocked = null)
		{
			var stack = new Stack<Tuple<interval, int>>();
			stack.Push(new Tuple<interval, int>(interval, 0));

			while (stack.Count > 0)
			{
				var si = stack.Pop();

				if (si.Item1 != blocked)
				{
					if (si.Item2 < si.Item1.children.Count)
					{
						stack.Push(new Tuple<interval, int>(si.Item1, si.Item2 + 1));
						stack.Push(new Tuple<interval, int>(si.Item1.children[si.Item2], 0));
					}
					else
					{
						yield return si.Item1;
					}
				}
			}

			yield break;
		}



		static public int[] _isa(int[] sa)
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
				else if (t[input[i]] > -1 && o < constant.threshold_bytematch_offset)
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



		//lcp interval tree
		static private interval _lit(int[] lcp)
		{
			interval interval = null;

			var stack = new Stack<interval>();
			stack.Push(new interval(0, 0, null));

			for (int i = 1; i < lcp.Length; i++)
			{
				int left = i - 1;

				while (lcp[i] < stack.Peek().length)
				{
					interval = new interval(stack.Pop(), i - 1);
					//process
					left = interval.left;
					if (lcp[i] <= stack.Peek().length)
					{
						stack.Peek().add(interval);
						interval = null;

					}
				}
				if (lcp[i] > stack.Peek().length)
				{
					stack.Push(new interval(lcp[i], left, interval));
					interval = null;
				}
			}

			while (stack.Count > 1)
			{
				interval = stack.Pop();
				interval = new interval(interval, lcp.Length - 1);
				//process
				if (interval.length > stack.Peek().length)
				{
					stack.Peek().add(interval);
				}
			}

			interval = new interval(stack.Pop(), lcp.Length - 1);
			return interval;
		}



		static public interval[] _litarray(interval lit)
		{
			var output = new interval[lit.right + 1];

			foreach (interval interval in _dfs(lit))
			{
				foreach (int n in interval.indexes())
				{
					output[n] = interval;
				}
			}

			return output;
		}



		//computing longest previous factor in linear time and applications - maxime crochemore 20071024
		//https://hal.inria.fr/hal-00619691/document - page 6 "compute_lpf_using_lcp"
		static private int[] _lpf(int[] sa, int[] lcp)
		{
			var lpf = new int[sa.Length];
			//var prevocc = new int[sa.Length];

			var stack = new Stack<Tuple<int, int>>();//Item1 = pos, Item2 = len
			stack.Push(new Tuple<int, int>(sa[0], 0));

			for (int i = 1; i <= lpf.Length; i++)
			{
				int lcpi = i < lpf.Length ? lcp[i] : 0;
				int sai = i < lpf.Length ? sa[i] : -1;

				while (stack.Count > 0 && sai < stack.Peek().Item1)
				{
					var ti = stack.Pop();
					lpf[ti.Item1] = Math.Max(ti.Item2, lcpi);
					lcpi = Math.Min(ti.Item2, lcpi);

					/*if (lpf[ti.Item1] == 0)
					{
						prevocc[ti.Item1] = -1;
					}
					else if (ti.Item2 > lcpi)
					{
						prevocc[ti.Item1] = stack.Peek().Item1;
					}
					else
					{
						prevocc[ti.Item1] = sai;
					}*/
				}
				if (i < sa.Length)
				{
					stack.Push(new Tuple<int, int>(sai, lcpi));
				}
			}

			return lpf;
		}



		static public int[] _nlv(int[] input)
		{
			var nlv = new int[input.Length];
			var s = new Stack<int>();

			for (int i = 0; i < input.Length; i++)
			{
				while (s.Count > 0 && input[s.Peek()] < input[i])
				{
					nlv[s.Peek()] = i;
					s.Pop();
				}
				s.Push(i);
			}

			while (s.Count > 0)
			{
				nlv[s.Peek()] = -1;
				s.Pop();
			}

			return nlv;
		}



		//simpler and faster lempel ziv factorization - keisuke goto & hideo bannai
		//https://pdfs.semanticscholar.org/87f4/f4a4f5a6508a20f356fab3ab97aa7062b0d5.pdf - page 7 algorithm 4
		static public int[] _nsv(int[] input)
		{
			var nsv = new int[input.Length];

			var s = new Stack<int>();

			for (int i = 0; i < input.Length; i++)
			{
				int x = input[i];
				while (s.Count > 0 && input[s.Peek()] > x)
				{
					nsv[s.Peek()] = i;
					s.Pop();
				}
				s.Push(i);
			}

			while (s.Count > 0)
			{
				nsv[s.Peek()] = -1;
				s.Pop();
			}

			return nsv;
		}



		static public int[] _plv(int[] input)
		{
			var plv = new int[input.Length];
			var s = new Stack<int>();

			for (int i = input.Length - 1; i >= 0; i--)
			{
				while (s.Count > 0 && input[s.Peek()] < input[i])
				{
					plv[s.Peek()] = i;
					s.Pop();
				}
				s.Push(i);
			}

			while (s.Count > 0)
			{
				plv[s.Peek()] = -1;
				s.Pop();
			}

			return plv;
		}



		static public int[] _psv(int[] input)
		{
			var psv = new int[input.Length];
			var s = new Stack<int>();

			for (int i = input.Length - 1; i >= 0; i--)
			{
				while (s.Count > 0 && input[s.Peek()] > input[i])
				{
					psv[s.Peek()] = i;
					s.Pop();
				}
				s.Push(i);
			}

			while (s.Count > 0)
			{
				psv[s.Peek()] = -1;
				s.Pop();
			}

			return psv;
		}



		public IEnumerable<Tuple<bool, int, int>> tedges(int i)
		{
			interval lcpinterval = litarray[isa[i]];
			int length = lcpinterval.length;

			int slp = spsv[isa[i]];
			int srp = snsv[isa[i]];

			int minoffset = int.MaxValue;

			bool this_rle = isrle(i);
			bool former_rle = isrle(i - 1);
			bool yieldminimaloffsetedges = true;

			while (lcpinterval.length > 1)
			{
				while (slp > -1 && slp >= lcpinterval.left)
				{
					int offset = i - sa[slp];
					minoffset = Math.Min(minoffset, offset);
					int src = i - offset;
					bool leftmaximal = src == 0 || input[i - 1] != input[src - 1];

					if (leftmaximal)
					{
						yield return new Tuple<bool, int, int>(leftmaximal, lcpinterval.length, offset);
					}

					if (splv[slp] > -1 && sa[splv[slp]] > i && (slp > 0 && spsv[slp] != -1 && splv[spsv[slp]] == spsv[slp]))
					{
						slp = spsv[slp];
					}
					else if (spsv[slp] < 0 || slp - splv[slp] < slp - spsv[slp])
					{
						slp = splv[slp];
						while (slp > -1 && sa[slp] > i)
						{
							slp = spsv[slp];
						}
					}
					else
					{
						slp = spsv[slp];
					}
				}

				while (srp > -1 && srp <= lcpinterval.right)
				{
					int offset = i - sa[srp];
					minoffset = Math.Min(minoffset, offset);
					int src = i - offset;
					bool leftmaximal = src == 0 || input[i - 1] != input[src - 1];

					if (leftmaximal)
					{
						yield return new Tuple<bool, int, int>(leftmaximal, lcpinterval.length, offset);
					}

					if (snlv[srp] > -1 && sa[snlv[srp]] > i && (srp > 0 && snsv[srp] != -1 && snlv[snsv[srp]] == snsv[srp]))
					{
						srp = snsv[srp];
					}
					else if (snsv[srp] < 0 || snlv[srp] - srp < snsv[srp] - srp)
					{
						srp = snlv[srp];
						while (srp > -1 && srp < sa.Length && sa[srp] > i)
						{
							srp = snsv[srp];
						}
					}
					else
					{
						srp = snsv[srp];
					}
				}

				lcpinterval = lcpinterval.parent;

				if (minoffset < int.MaxValue)
				{
					while (length > lcpinterval.length && yieldminimaloffsetedges)
					{
						yield return new Tuple<bool, int, int>(false, length, minoffset);
						length--;
						yieldminimaloffsetedges = !(this_rle && former_rle);
					}
				}
				else
				{
					length = lcpinterval.length;
				}

				if (length > constant.threshold_greedy_length)
				{
					break;
				}

			}

			if (minoffset < int.MaxValue)
			{
				while (length > 1 && yieldminimaloffsetedges)
				{
					yield return new Tuple<bool, int, int>(false, length, minoffset);
					length--;
					if (length > constant.threshold_greedy_length)
					{
						break;
					}
				}
			}

			yield return new Tuple<bool, int, int>(false, 1, l1o[i]);

			yield break;
		}



		private bool isrle(int i)
		{
			bool output = false;
			if (i > 1)
			{
				interval lcpinterval = litarray[isa[i]];
				output = i > 1 && isa[i] >= lcpinterval.left && isa[i - 1] >= lcpinterval.left && isa[i] <= lcpinterval.right && isa[i - 1] <= lcpinterval.right;
			}
			return output;
		}



	}
}
