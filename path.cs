using System;
using System.Collections.Generic;

namespace cap
{
	static class path
	{



		public static Tuple<int, int>[] dijkstra(byte[] input, SortedDictionary<int, int>[] edges)
		{
			var node = new Dictionary<ulong, state>[input.Length + 1];
			var cost = new int[input.Length + 1];
			for (int i = 0; i < input.Length + 1; i++)
			{
				node[i] = new Dictionary<ulong, state>();
				cost[i] = int.MaxValue;
			}
			cost[0] = 0;

			var current_state = new state(input, 0, 0);
			node[0].Add(current_state.key, current_state);

			var queue = new pqueue<int, state>(0, new state(new byte[0], 0, 0));

			int edgecount = 0;

			while (current_state.position < input.Length)
			{
				if (current_state.bitcost == cost[current_state.position])
				{
					foreach (Tuple<int, int> t in edge.edges(edges[current_state.position], current_state.r0))
					{
						int length = t.Item1;
						int offset = t.Item2;

						edgecount++;

						int edge_dest = current_state.position + length;

						if (offset == current_state.offset && offset > 0 || current_state.bitcost > cost[edge_dest])
						{
							continue;
						}

						int edge_cost = model.edge_cost(length, offset, current_state);

						if (edge_cost > 0 && current_state.bitcost + edge_cost <= cost[edge_dest])
						{
							var dest_state = new state(current_state);
							dest_state.add(length, offset, current_state.bitcost + edge_cost);

							if (dest_state.bitcost < cost[dest_state.position])
							{
								cost[dest_state.position] = dest_state.bitcost;
								node[dest_state.position].Clear();
							}

							if (dest_state.bitcost == cost[dest_state.position])
							{
								if (!node[dest_state.position].ContainsKey(dest_state.key))
								{
									node[edge_dest].Add(dest_state.key, dest_state);
									queue.enqueue(dest_state.bitcost, dest_state);
								}
								else
								{
									if (dest_state.steps < node[dest_state.position][dest_state.key].steps)
									{
										node[dest_state.position][dest_state.key] = dest_state;
									}
								}
							}
						}
					}
				}
				current_state = queue.dequeue();
			}

			var output = new List<Tuple<int, int>>();
			while (current_state.pkey != 0)
			{
				output.Add(new Tuple<int, int>(current_state.length, current_state.offset));
				current_state = node[current_state.position - current_state.length][current_state.pkey];
			}

			output.Reverse();

			if (!verify(input, output))
			{
				throw new Exception("path b0rk");
			}

			return output.ToArray();
		}



		static public bool verify(byte[] input, List<Tuple<int,int>> path)
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
						if (input[i+l] != input[ci+l])
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
