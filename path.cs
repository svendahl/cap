using System;
using System.Collections.Generic;

namespace cap
{
	class path
	{



		private readonly bool[] closed;
		private readonly int[] cost;
		private const int cost_tolerance = 10;
		private readonly edge edge;
		private readonly byte[] input;
		private readonly Dictionary<int, state>[] tree;



		public path(byte[] input)
		{
			this.input = input;
			closed = new bool[input.Length + 1];
			cost = new int[input.Length + 1];
			edge = new edge(input);
			tree = new Dictionary<int, state>[input.Length + 1];

			for (int i = 0; i < input.Length + 1; i++)
			{
				closed[i] = false;
				cost[i] = i * 10;
				tree[i] = new Dictionary<int, state>();
			}

			tree[0].Add(0, new state(input));
		}



		public Tuple<int,int>[] output()
		{
			int cpos = 0;

			while (cpos < input.Length)
			{
				int cend = cpos + 1;
				while (cend < input.Length && edge.oe[cend].Length == 0)
				{
					cend++;
				}
				while (cend < input.Length && edge.oe[cend].Length != 0)
				{
					cend++;
				}
				subpath(cpos, ref cend);
				cpos = cend;
			}

			var output = collect();

			if (!verify(input, output))
			{
				throw new Exception("mpath b0rk");
			}

			return output;
		}



		private void subpath(int cpos, ref int cend)
		{
			var queue = new pqueue<int, int>(0, 0);
			var cendqueue = new pqueue<int, int>(0, 0);

			foreach(KeyValuePair<int, state> kvp in tree[cpos])
			{
				queue.enqueue(kvp.Value.bitcost, kvp.Value.position);
			}

			while (!queue.empty)
			{
				cpos = queue.dequeue().Item2;

				if (!closed[cpos])
				{
					foreach (KeyValuePair<int, state> kvp in tree[cpos])
					{
						int roffs = kvp.Key;
						state cstate = kvp.Value;

						if (cstate.bitcost <= cost[cpos] + cost_tolerance)
						{
							foreach (Tuple<int, Tuple<int, int>> ccostedge in edge.edges(cstate))
							{
								int cedgecost = ccostedge.Item1;
								Tuple<int, int> cedge = ccostedge.Item2;

								if (!closed[cstate.position + cedge.Item1] && cstate.bitcost + cedgecost <= cost[cstate.position + cedge.Item1] + cost_tolerance)
								{
									var dstate = new state(cstate, ccostedge);

									int tl = dstate.position;
									while (tl > cstate.position)
									{
										cost[tl] = dstate.bitcost < cost[tl] ? dstate.bitcost : cost[tl];
										tl--;
									}

									if (!tree[dstate.position].ContainsKey(dstate.roffs))
									{
										tree[dstate.position].Add(dstate.roffs, dstate);
									}
									else if (dstate.bitcost < tree[dstate.position][dstate.roffs].bitcost || (dstate.bitcost == tree[dstate.position][dstate.roffs].bitcost && dstate.steps < tree[dstate.position][dstate.roffs].steps))
									{
										tree[dstate.position][dstate.roffs] = dstate;
									}

									if (dstate.position < cend)
									{
										queue.enqueue(dstate.bitcost, dstate.position);
									}
									else if (dstate.position == cend)
									{
										cendqueue.enqueue(dstate.bitcost, dstate.position);
									}
									else if (dstate.position > cend)
									{
										if (!cendqueue.empty)
										{
											var t = cendqueue.dequeue();
											queue.enqueue(t.Item1, t.Item2);
										}

										while (!cendqueue.empty)
										{
											cendqueue.dequeue();
										}

										cend = dstate.position;
										cendqueue.enqueue(dstate.bitcost, dstate.position);
									}
									/*else
									{
										throw new Exception("head blown");
									}*/
								}
							}
						}
					}
					closed[cpos] = true;
				}
			}
		}



		private Tuple<int,int>[] collect()
		{
			int cpos = tree.Length - 1;
			int bsc = int.MaxValue;
			int bss = int.MaxValue;
			int k = 0;

			foreach (KeyValuePair<int, state> kvp in tree[cpos])
			{
				if (kvp.Value.bitcost < bsc || kvp.Value.bitcost == bsc && kvp.Value.steps < bss)
				{
					k = kvp.Key;
					bsc = kvp.Value.bitcost;
					bss = kvp.Value.steps;
				}
			}

			state cs = tree[cpos][k];
			var output = new Tuple<int, int>[cs.steps];

			while (cpos > 0)
			{
				output[cs.steps - 1] = new Tuple<int, int>(cs.length, cs.offset);
				cpos -= cs.length;
				cs = tree[cpos][cs.pkey];
			}

			return output;
		}



		static private bool verify(byte[] input, Tuple<int, int>[] path)
		{
			int i = 0;

			foreach (Tuple<int, int> t in path)
			{
				int length = t.Item1;
				int offset = t.Item2;

				if (offset > 0)
				{
					int ci = i - offset;
					for (int l = 0; l < length; l++)
					{
						if (input[i + l] != input[ci + l])
						{
							return false;
						}
					}
				}
				i += length;
			}

			return true;
		}



	} // class
} // namespace
