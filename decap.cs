using System;
using System.Collections.Generic;

namespace cap
{
	class decap
	{



		private readonly byte[] input;



		private byte bitbuffer;
		private int bitcount;
		private int inptr;
		private int lwm;
		private int roffs;



		private int getbit()
		{
			if (bitcount-- == 0)
			{
				bitbuffer = getbyte();
				bitcount = 7;
			}

			int bit = (bitbuffer >> 7) & 1;
			bitbuffer <<= 1;

			return bit;
		}



		private byte getbyte()
		{
			if (inptr < 0 && inptr >= input.Length)
			{
				throw new Exception("decap getbyte b0rk");
			}
			return input[inptr++];
		}



		private int getgamma()
		{
			int output = 1;

			do
			{
				output <<= 1;
				output |= getbit();
			}
			while (getbit() == 1);

			return output;
		}



		public decap(byte[] input)
		{
			this.input = input;
		}



		public byte[] depack()
		{
			var output = new List<byte>();

			inptr = 0;
			lwm = 0;
			roffs = 0;

			output.Add(getbyte());
			output.Add(getbyte());

			output.Add(getbyte());

			int offs = 0;
			int len = 0;

			bool done = false;

			while (!done)
			{
				int pfx = 0;
				while (pfx < 3 && getbit() == 1)
				{
					pfx++;
				}
				switch (pfx)
				{
					case 0:
						output.Add(getbyte());
						lwm = 0;
						break;

					case 1:
						offs = getgamma();
						if (lwm == 0 && offs == 2)
						{
							offs = roffs;
							len = getgamma();
							while (len-- > 0)
							{
								output.Add(output[output.Count - offs]);
							}
						}
						else
						{
							offs -= lwm == 0 ? 3 : 2;
							offs <<= 8;
							offs |= getbyte();
							len = getgamma();
							if (offs < 128 || offs >= 32000)
							{
								len += 2;
							}
							else if (offs >= 1280)
							{
								len += 1;
							}
							while (len-- > 0)
							{
								output.Add(output[output.Count - offs]);
							}
							roffs = offs;
						}
						lwm = 1;
						break;

					case 2:
						offs = getbyte();
						len = 2 + (offs & 1);
						offs >>= 1;
						if (offs > 0)
						{
							while (len-- > 0)
							{
								output.Add(output[output.Count - offs]);
							}
						}
						else
						{
							done = true;
						}

						roffs = offs;
						lwm = 1;
						break;

					case 3:
						offs = 0;
						for (int i = 0; i < 4; i++)
						{
							offs <<= 1;
							offs |= getbit();
						}

						if (offs > 0)
						{
							output.Add(output[output.Count - offs]);
						}
						else
						{
							output.Add(0);
						}
						lwm = 0;
						break;

					default:
						throw new Exception("decap depack b0rk");
				}
			}

			return output.ToArray();
		}



	} // class
} // namespace
