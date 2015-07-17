using System;

namespace cap
{
	static class model
	{



		public static int edge_cost(int length, int offset, state state)
		{
			int output = 0;

			if (length == 1)
			{
				if (offset >= constant.threshold_bytematch_offset)
				{
					throw new Exception("WAT!");
				}
				else if (state.posis0)
				{
					if (state.position == 0)
					{
						output += constant.bitcost_prefixcode_literal + constant.bitcost_literal;
					}
					else
					{
						output += constant.bitcost_prefixcode_bytematch + constant.bitcost_offset_bytematch;
					}
				}
				else if (offset > 0 && offset < constant.threshold_bytematch_offset && !state.posis0)
				{
					output += constant.bitcost_prefixcode_bytematch + constant.bitcost_offset_bytematch;
				}
				else if (offset == 0)
				{
					output += constant.bitcost_prefixcode_literal + constant.bitcost_literal;
				}
			}
			else if (length >= 2 && offset == state.roffs && state.lwm == 0)
			{
				output += constant.bitcost_prefixcode_normalmatch;
				output += togammacount(2);
				output += togammacount(length);
			}
			else if (length >= 2 && length <= 3 && offset > 0 && offset < constant.threshold_shortmatch_offset)
			{
				output += constant.bitcost_prefixcode_shortmatch;
				output += constant.bitcost_literal;
			}
			else if (
				(length == 2 && offset >= constant.threshold_shortmatch_offset && offset < constant.threshold_length2match_offset) ||
				(length == 3 && offset >= constant.threshold_shortmatch_offset && offset < constant.threshold_length3match_offset) ||
				(length >= 4 && offset >= 1 && offset < constant.threshold_offset)
				)
			{
				output += constant.bitcost_prefixcode_normalmatch;

				int offset_hi = (offset >> 8);
				offset_hi += 2;
				if (state.lwm == 0)
				{
					offset_hi += 1;
				}
				output += togammacount(offset_hi);
				output += 8;

				int ldo = 0;
				if (offset < 128 || offset >= constant.threshold_length3match_offset)
				{
					ldo = 2;
				}
				else if (offset >= constant.threshold_length2match_offset)
				{
					ldo = 1;
				}
				output += togammacount(length - ldo);
			}
			else
			{
				throw new Exception("WAT!");
			}

			return output;
		}



		public static bool edge_valid(int length, int offset, state state)
		{
			bool output = false;

			if (length == 1)
			{
				if (offset == 0)
				{
					output = true;
				}
				else if (offset > -1 && offset < constant.threshold_bytematch_offset)
				{
					if (state.lwm == 0 || (state.lwm == 1 && offset != state.roffs))
					{
						output = true;
					}
				}
			}
			else if (length > 1)
			{
				if (offset == state.roffs && state.lwm == 0 && state.roffs != 0 && offset != state.offset)
				{
					output = true;
				}
				else if (offset != state.offset)
				{
					if (length == 2 && offset > 0 && offset < constant.threshold_length2match_offset)
					{
						output = true;
					}
					else if (length == 3 && offset > 0 && offset < constant.threshold_length3match_offset)
					{
						output = true;
					}
					else if (length > 3 && offset > 0 && offset < constant.threshold_offset)
					{
						if (!(offset == state.roffs && state.lwm == 1))//redundant
						{
							output = true;
						}
					}
				}
			}

			return output;
		}



		public static Tuple<int, int>[] parse(byte[] input)
		{
			return new path(input).output();
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
