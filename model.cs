using System;
using System.Collections.Generic;

namespace cap
{
	static class model
	{



		public const int rlethreshold = byte.MaxValue;
		public const int maxlength = ushort.MaxValue;



		public static int edge_cost(int length, int offset, state state)
		{
			const int _bitcost_prefixcode_literal = 1;
			const int _bitcost_prefixcode_normalmatch = 2;
			const int _bitcost_prefixcode_shortmatch = 3;
			const int _bitcost_prefixcode_bytematch = 3;
			const int _bitcost_offset_bytematch = 4;

			int output = 0;

			if (length == 1)
			{
				if (offset > 15)
				{
					output = -1; //noncodable edge
				}
				else if (state.posis0) // #111 - byte match 0
				{
					output += _bitcost_prefixcode_bytematch + _bitcost_offset_bytematch;
				}
				else if (offset > 0 && offset < 16 && !state.posis0) // #111 - byte match offset 1-15
				{
					output += _bitcost_prefixcode_bytematch + _bitcost_offset_bytematch;
				}
				else if (offset == 0) // #0 - literal
				{
					output += _bitcost_prefixcode_literal + 8;
				}
			}
			else if (length >= 2 && offset == state.r0 && state.lwm == 0) // #10 repeated offset
			{
				output += _bitcost_prefixcode_normalmatch;
				output += togammacount(2);
				output += togammacount(length);
			}
			else if (length >= 2 && length <= 3 && offset > 0 && offset <= 127)//#110 short match
			{
				output += _bitcost_prefixcode_shortmatch;
				output += 8;
			}
			else if (
				(length == 2 && offset >= 128 && offset <= 511) || // #10 length 2 offset 128-511
				(length == 3 && offset >= 128 && offset <= 31999) || // #10 length 3 offset 128-31999
				(length >= 4 && offset >= 1 && offset <= 65023) //#10 length > 3 offset <= 65023
				)
			{
				output += _bitcost_prefixcode_normalmatch;

				int offset_hi = (offset >> 8);
				offset_hi += 2;
				if (state.lwm == 0)
				{
					offset_hi += 1;
				}
				output += togammacount(offset_hi);
				output += 8;

				int ldo = 0;
				{
					if (offset < 128 || offset >= 32000)
					{
						ldo = 2;
					}
					else if (offset >= 1280)
					{
						ldo = 1;
					}
				}
				output += togammacount(length - ldo);
			}
			else
			{
				output = -1; //noncodable edge
			}

			return output;
		}



		private static int[] itog(int input)
		{
			var output = new List<int>();
			int msb = 15;
			while (input >> msb-- == 0) { }

			while (true)
			{
				int bit = (input >> msb) & 1;
				output.Add(bit);
				msb--;
				if (msb >= 0)
				{
					output.Add(1);
				}
				else
				{
					output.Add(0);
					break;
				}
			}
			return output.ToArray();
		}



		public static Tuple<int, int>[] parse(byte[] input)
		{
			var edges = new SortedDictionary<int, int>[input.Length + 1];

			for (int i = 0; i < input.Length + 1; i++)
			{
				edges[i] = new SortedDictionary<int, int>();
			}
			edge.findedges(input, ref edges, rlethreshold, maxlength);

			return path.dijkstra(input, edges);
		}



		private static void putbit(int bit, ref int bitbuffer, ref int bitbuffer_ptr, ref List<byte> output)
		{
			if (bitbuffer >= 256)
			{
				bitbuffer &= 0xff;
				output[bitbuffer_ptr] = (byte)bitbuffer;
				bitbuffer_ptr = output.Count;
				output.Add(123);
				bitbuffer = 1;
			}

			bitbuffer <<= 1;
			bitbuffer |= bit;
		}



		private static void putbits(int[] bits, ref int bitbuffer, ref int bitbuffer_ptr, ref List<byte> output)
		{
			foreach(int bit in bits)
			{
				putbit(bit, ref bitbuffer, ref bitbuffer_ptr, ref output);
			}
		}



		public static byte[] tobinary(byte[] input, Tuple<int, int>[] path, ushort loadaddress)
		{
			//prefix codes
			var prefixcode_literal = new int[1] { 0 };
			var prefixcode_normal_match = new int[2] { 1, 0 };
			var prefixcode_short_match = new int[3] { 1, 1, 0 };
			var prefixcode_byte_match = new int[3] { 1, 1, 1 };

			var state = new state(input, 0, 0); //NOTE: bitcost will not be correct

			var output = new List<byte>();

			//load address
			output.Add((byte)(loadaddress & 0xff));
			output.Add((byte)(loadaddress >> 8));

			//first literal
			output.Add(input[0]);

			//init bitbuffer
			int bitbuffer = 1;
			int bitbuffer_ptr = output.Count;
			output.Add(123);

			int input_ptr = 0;

			int tuplecount = 0;
			foreach (Tuple<int, int> t in path)
			{
				//skip first literal
				if (t == path[0])
				{
					state.add(t.Item1, t.Item2, 0); //NOTE: bitcost will not be correct
					input_ptr++;
					continue;
				}

				int length = t.Item1;
				int offset = t.Item2;

				if (length == 1)
				{
					if (offset == 0 && input[input_ptr] != 0x00)
					{
							putbits(prefixcode_literal, ref bitbuffer, ref bitbuffer_ptr, ref output);
							output.Add(input[input_ptr]);
					}
					else if (offset < 16 || input[input_ptr] == 0x00) // #111 - byte match
					{
						if (input[input_ptr] == 0x00)
						{
							offset = 0;
						}

						putbits(prefixcode_byte_match, ref bitbuffer, ref bitbuffer_ptr, ref output);
						for (int s = 3; s >= 0; s--)
						{
							putbit(((offset >> s) & 1), ref bitbuffer, ref bitbuffer_ptr, ref output);
						}
					}
					else
					{
						throw new Exception("uncodable edge");
					}
				}
				else if (length >= 2 && offset == state.r0 && state.lwm == 0)//#10 repeat offset match
				{
					//flag #10
					putbits(prefixcode_normal_match, ref bitbuffer, ref bitbuffer_ptr, ref output);
					putbits(itog(2), ref bitbuffer, ref bitbuffer_ptr, ref output);
					putbits(itog(length), ref bitbuffer, ref bitbuffer_ptr, ref output);

				}
				else if (length >= 2 && length <= 3 && offset > 0 && offset <= 127)//#110 short match
				{
					//flag #110
					putbits(prefixcode_short_match, ref bitbuffer, ref bitbuffer_ptr, ref output);
					output.Add((byte)(offset << 1 | (length & 1)));
				}
				else if (
				(length == 2 && offset >= 128 && offset <= 511) || // #10 length 2 offset 128-511
				(length == 3 && offset >= 128 && offset <= 31999) || // #10 length 3 offset 128-31999
				(length >= 4 && offset >= 1 && offset <= 65023) //#10 length > 3 offset <= 65023
				)
				{
					//flag #10
					putbits(prefixcode_normal_match, ref bitbuffer, ref bitbuffer_ptr, ref output);

					int offset_hi = (offset >> 8) + 2;
					if (state.lwm == 0)
					{
						offset_hi += 1;
					}

					putbits(itog(offset_hi), ref bitbuffer, ref bitbuffer_ptr, ref output);
					output.Add((byte)(offset & 0xff));

					int ldo = 0;
					{
						if (offset < 128 || offset >= 32000)
						{
							ldo = 2;
						}
						else if (offset >= 1280)
						{
							ldo = 1;
						}
					}
					putbits(itog(length - ldo), ref bitbuffer, ref bitbuffer_ptr, ref output);
				}
				else
				{
					throw new Exception("uncodable edge");
				}
				state.add(t.Item1, t.Item2, 0); //NOTE: bitcost will not be correct
				input_ptr += length;

				tuplecount++;
			}

			//eof
			//#110
			putbits(prefixcode_short_match, ref bitbuffer, ref bitbuffer_ptr, ref output);
			//offset 0
			output.Add(0);

			//flush bitbuffer
			while (bitbuffer < 256)
			{
				bitbuffer <<= 1;
			}
			output[bitbuffer_ptr] = (byte)(bitbuffer & 0xff);

			return output.ToArray();
		}



		private static int togammacount(int input)
		{
			if (input < 2 || input > ushort.MaxValue)
			{
				throw new Exception("model togammacount b0rk @ " + input);
			}

			int output = 1;
			while (input >> output > 1)
			{
				output++;
			}
			output <<= 1;
			return output;
		}



	} // class
} // namespace
