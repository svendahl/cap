using System;
using System.Collections.Generic;

namespace cap
{
	class edge
	{



		private readonly byte[] input;
		private readonly int[] ll1;



		public readonly Tuple<int, int>[][] oe;
		public readonly sae sae;//TODO: move to buildedges()



		public edge(byte[] input)
		{
			this.input = input;
			sae = new sae(input);//TODO: move to buildedges()
			buildedges(ref oe);
			buildbyteedges(ref ll1);
			sae = null;
		}



		private void buildbyteedges(ref int[] ll1)
		{
			ll1 = new int[input.Length];
			var t = new int[256];
			for (int i = 0; i < byte.MaxValue + 1; i++)
			{
				t[i] = -1;
			}
			for (int i = 0; i < input.Length; i++)
			{
				ll1[i] = t[input[i]];
				t[input[i]] = i;
			}
		}



		private void buildedges(ref Tuple<int,int>[][] oe)
		{
			oe = new Tuple<int, int>[input.Length][];
			//TODO: parallelize
			for (int i = 0; i < input.Length; i++)
			{
				oe[i] = edgesof(i);
			}
		}



		private Tuple<int, int>[] edgesof(int index)
		{
			var state = new state(input);
			int offset = int.MaxValue;
			var output = new List<Tuple<int, int>>();

			var rc = edges_sel(edgesll, index);
			var re = rc.GetEnumerator();
			var lc = edges_sel(edgesls, index);
			var le = lc.GetEnumerator();

			var r = edgesnext(ref re);

			if (r.Item1 > 255/*FIXME: magic*/ && r.Item2 == 1)
			{
				return new Tuple<int, int>[1]{r};
			}

			var l = edgesnext(ref le);

			while (l.Item1 > 0 || r.Item1 > 0)
			{
				if (l.Item1 > r.Item1 || l.Item1 == r.Item1 && l.Item2 < r.Item2)
				{
					if (model.edge_valid(l.Item1, l.Item2, state))
					{
						output.Add(l);
					}

					offset = l.Item2;
					if (l.Item1 == r.Item1)
					{
						r = edgesnext(ref re);
					}
					l = edgesnext(ref le);
				}
				else
				{
					if (model.edge_valid(r.Item1, r.Item2, state))
					{
						output.Add(r);
					}

					offset = r.Item2;
					if (r.Item1 == l.Item1)
					{
						l = edgesnext(ref le);
					}
					r = edgesnext(ref re);
				}
				if (offset == 1)
				{
					break;
				}
				while (l.Item2 >= offset)
				{
					l = edgesnext(ref le);
				}
				while (r.Item2 >= offset)
				{
					r = edgesnext(ref re);
				}
			}

			if (!verify(output, index))
			{
				throw new Exception("edge b0rk");
			}
			return output.ToArray();
		}



		private IEnumerable<Tuple<int, int>> edges_sel(Func<int, IEnumerable<Tuple<int, int>>> emethod, int index)
		{
			var ec = emethod(index);
			var ee = ec.GetEnumerator();

			int length = int.MaxValue;
			int offset = int.MaxValue;

			while (length > 0)
			{
				var e = edgesnext(ref ee);
				if (e.Item1 < length && e.Item2 < offset)
				{
					if (length < int.MaxValue)
					{
						yield return new Tuple<int, int>(length, offset);
					}
					length = e.Item1;
					offset = e.Item2;
				}
				else if (e.Item1 < length && e.Item2 > offset)
				{
					yield return new Tuple<int, int>(length, offset);
					length = e.Item1;
				}
				else if (e.Item1 == length && e.Item2 < offset)
				{
					offset = e.Item2;
				}
				/*else if (e.Item1 == length && e.Item2 > offset)
				{//nop
					;
				}
				else
				{
					throw new Exception("head blown");
				}*/
			}
		}



		private Tuple<int, int> edgesnext(ref IEnumerator<Tuple<int, int>> ee)
		{
			return ee.MoveNext() ? ee.Current : new Tuple<int, int>(0, 0);
		}



		private IEnumerable<Tuple<int, int>> edgesls(int index)
		{
			int saindex = sae.isa[index];
			int length = sae.lcp[saindex];
			int offset = int.MaxValue;
			while (saindex > 0 && length > 0 && offset > 1)
			{
				length = Math.Min(length, sae.lcp[saindex]);
				int coffset = index - sae.sa[saindex - 1];

				if (length > 0 && coffset > 0 && coffset < offset)
				{
					offset = coffset;
					yield return new Tuple<int, int>(length, offset);
				}
				saindex--;
			}
		}



		private IEnumerable<Tuple<int, int>> edgesll(int index)
		{
			int saindex = sae.isa[index] + 1;
			if (saindex < sae.sa.Length)
			{
				int length = sae.lcp[saindex];
				int offset = int.MaxValue;
				while (saindex < sae.sa.Length && length > 0 && offset > 1)
				{
					length = Math.Min(length, sae.lcp[saindex]);
					int coffset = index - sae.sa[saindex];

					if (length > 0 && coffset > 0 && coffset < offset)
					{
						offset = coffset;
						yield return new Tuple<int, int>(length, offset);
					}
					saindex++;
				}
			}
		}



		private bool verify(List<Tuple<int, int>> edges, int pos)
		{
			foreach (Tuple<int, int> t in edges)
			{
				for (int i = 0; i < t.Item1; i++)
				{
					if (input[pos + i] != input[pos + i - t.Item2])
					{
						return false;
					}
				}
			}
			return true;
		}



		public IEnumerable<Tuple<int, Tuple<int,int>>> edges(state state)
		{
			var queue = new pqueue<int, Tuple<int, int>>(0, new Tuple<int, int>(-1, -1));//TODO: test cratio

			int ll = int.MaxValue;
			foreach(Tuple<int,int> t in expand(state))
			{
				queue.enqueue(model.edge_cost(t.Item1, t.Item2, state), t);
				ll = t.Item1;
			}

			if (ll != 1)
			{
				yield return new Tuple<int, Tuple<int, int>>(model.edge_cost(1, 0, state), new Tuple<int, int>(1, 0));
			}

			while (queue.count > 0)
			{
				yield return queue.dequeue();
			}
		}



		private IEnumerable<Tuple<int,int>> expand(state state)
		{
			if (state.lwm == 0 && state.roffs > 1 && state.position < input.Length - 2 && input[state.position] == input[state.position - state.roffs] && input[state.position + 1] == input[state.position - state.roffs + 1])
			{
				int rl = 2;
				while (state.position + rl < input.Length && input[state.position + rl] == input[state.position - state.roffs + rl])
				{
					rl++;
				}

				bool yb = oe[state.position].Length > 0 && (oe[state.position][0].Item1 < rl && oe[state.position][0].Item2 != state.roffs);

				while (rl > 1)
				{
					yield return new Tuple<int, int>(rl, state.roffs);
					rl--;
				}

				if (yb)
				{
					yield break;
				}
			}

			if (oe[state.position].Length > 0)
			{
				int current_length = oe[state.position][0].Item1;
				int current_offset = oe[state.position][0].Item2;
				if (current_length > 255/*FIXME: magic*/)
				{
					yield return new Tuple<int, int>(current_length, current_offset);
					yield break;
				}

				foreach (Tuple<int, int> t in oe[state.position])
				{
					while (current_length > t.Item1)
					{
						if (model.edge_valid(current_length, current_offset, state))
						{
							yield return new Tuple<int, int>(current_length, current_offset);
						}
						current_length--;
					}
					current_offset = t.Item2;
				}

				while (current_length > 1)
				{
					if (model.edge_valid(current_length, current_offset, state))
					{
						yield return new Tuple<int, int>(current_length, current_offset);
					}
					current_length--;
				}
			}

			if (state.posis0)
			{
				yield return new Tuple<int, int>(1, 0);
			}
			else if (ll1[state.position] > 0 && model.edge_valid(1, state.position - ll1[state.position], state))
			{
				yield return new Tuple<int, int>(1, state.position - ll1[state.position]);
			}

			if (!(oe[state.position].Length > 0 && oe[state.position][0].Item1 > 32/*FIXME: magic*/) && !state.posis0)
			{
				yield return new Tuple<int, int>(1, 0);
			}
		}



	} // class
} // namespace
