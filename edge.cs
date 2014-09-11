using System;
using System.Collections.Generic;

namespace cap
{
	static class edge
	{



		static public IEnumerable<Tuple<int, int>> edges(SortedDictionary<int, int> edges, int r0)
		{
			yield return new Tuple<int, int>(1, 0);

			int l = 1;
			foreach (KeyValuePair<int, int> kvp in edges)
			{
				if (kvp.Value == r0)
				{
					l = 1;
				}
				while (l <= kvp.Key)
				{
					yield return new Tuple<int, int>(l, kvp.Value);
					l++;
				}
			}
		}



		static public int[] maxlengths(SortedDictionary<int, int>[] edges)
		{
			var output = new int[edges.Length];

			for (int i = 0; i < edges.Length; i++ )
			{
				foreach (KeyValuePair<int, int> kvp in edges[i])
				{
					if (kvp.Key > output[i])
					{
						output[i] = kvp.Key;
					}
				}
			}

			return output;
		}




		static public void findedges(byte[] input, ref SortedDictionary<int,int>[] edges_out, int rlethreshold = 64, int maxlength = ushort.MaxValue)
		{
			const ulong hash_seed = 1099511628211;
			var hashes = new ulong[input.Length];
			for (int i = 0; i < hashes.Length; i++)
			{
				hashes[i] = hash_seed;
			}

			rle(input, ref edges_out, ref hashes, rlethreshold, maxlength);
			match(input, ref edges_out, ref hashes, rlethreshold, maxlength);
			prune(input, ref edges_out, ref hashes, rlethreshold, maxlength);

			if (!verify(input, edges_out))
			{
				throw new Exception("edge b0rk");
			}
		}



		private static void prune(byte[] input, ref SortedDictionary<int, int>[] edges_out, ref ulong[] hashes, int rlethreshold, int maxlength)
		{
			var ml = maxlengths(edges_out);

			for (int i = input.Length - 1; i > 0; i--)
			{
				if (ml[i - 1] == ml[i] + 1 &&
					edges_out[i - 1].ContainsKey(ml[i - 1]) &&
					edges_out[i].ContainsKey(ml[i]) &&
					edges_out[i - 1][ml[i - 1]] == edges_out[i][ml[i]] &&
					ml[i] > 32 //FIXME: Ugly duckling prunes surprisingly well
					)
				{
					edges_out[i].Clear();
				}
			}
		}



		private static void rle(byte[] input, ref SortedDictionary<int, int>[] edges_out, ref ulong[] hashes, int rlethreshold, int maxlength)
		{
			if (rlethreshold >= maxlength)
			{
				return;
			}

			int i = 1;
			while (i < input.Length)
			{
				while (i < input.Length && input[i] != input[i - 1])
				{
					i++;
				}

				int rl = 0;
				while (i + rl < input.Length && input[i - 1] == input[i + rl])
				{
					rl++;
				}

				if (rl >= rlethreshold)
				{
					hashes[i - 1] = 0;
					while (rl >= maxlength)
					{
						edges_out[i].Add(maxlength, 1);
						hashes[i] = 0;
						rl--;
						i++;
					}

					while (rl >= rlethreshold)
					{
						edges_out[i].Add(rl, 1);
						hashes[i] = 0;
						rl--;
						i++;
					}

					while (rl > 0)
					{
						edges_out[i].Add(rl, 1);
						rl--;
						i++;
					}
				}
				else
				{
					i += rl;
				}
			}
		}



		private static void match(byte[] input, ref SortedDictionary<int, int>[] edges_out, ref ulong[] hashes, int rlethreshold, int maxlength = 255)
		{
			int length = 0;
			bool iterate = true;
			Dictionary<ulong, int> dict;
			while (iterate && length < maxlength)
			{
				iterate = false;
				dict = new Dictionary<ulong, int>();
				for (int i = 0; i < input.Length; i++)
				{
					if (hashes[i] != 0 && i + length < input.Length)
					{
						hashes[i] ^= input[i + length];
						hashes[i] *= 9782798678568883157;
						hashes[i]  = (hashes[i] << 31) | (hashes[i] >> 33);
						hashes[i] *= 5545529020109919103;

						if (!dict.ContainsKey(hashes[i]))
						{
							dict.Add(hashes[i], i);
							hashes[i] = 0; //deactivate index
						}
						else
						{
							iterate = true;
							hashes[dict[hashes[i]]] = hashes[i]; //reactivate index
							int o = i - dict[hashes[i]];

							if (edges_out[i].ContainsKey(length) && edges_out[i][length] == o)
							{
								edges_out[i].Remove(length);
							}

							if (!edges_out[i].ContainsKey(length + 1))
							{
								edges_out[i].Add(length + 1, o);
							}
							dict[hashes[i]] = i;
						}
					}
				}
				length++;
			}
		}



		private static bool verify(byte[] input, SortedDictionary<int, int>[] e)
		{
			for (int i = 0; i < input.Length; i++)
			{
				foreach (KeyValuePair<int, int> kvp in e[i])
				{
					int length = kvp.Key;
					int offset = kvp.Value;
					int ci = i - offset;

					int l = 0;
					while (l < length)
					{
						if (input[i + l] != input[ci + l])
						{
							return false;
						}
						l++;
					}
				}
			}

			return true;
		}



	} // class
} // namespace
