using System;
using System.Collections.Generic;

namespace cap
{
	class decap
	{



		private readonly byte[] _input;
		private int _inptr;
		private byte _bitbuffer;
		private int _bitcount;



		private int _lwm;
		private int _r0;



		private int getbit()
		{
			if (_bitcount-- == 0)
			{
				_bitbuffer = getbyte();
				_bitcount = 7;
			}

			int bit = (_bitbuffer >> 7) & 1;
			_bitbuffer <<= 1;

			return bit;
		}



		private byte getbyte()
		{
			if (_inptr < 0 && _inptr >= _input.Length)
			{
				throw new Exception("decap getbyte b0rk");
			}
			return _input[_inptr++];
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
			_input = input;
		}



		public byte[] depack()
		{
			//var parse = new List<Tuple<int, int>>();
			var output = new List<byte>();

			_inptr = 0;
			_lwm = 0;
			_r0 = 0;

			// load address
			output.Add(getbyte());
			output.Add(getbyte());

			// first literal
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
						//parse.Add(new Tuple<int, int>(1, 0));
						output.Add(getbyte());
						_lwm = 0;
						break;

					case 1:
						offs = getgamma();
						if (_lwm == 0 && offs == 2)
						{
							offs = _r0;
							len = getgamma();
							//parse.Add(new Tuple<int, int>(len, offs));
							while (len-- > 0)
							{
								output.Add(output[output.Count - offs]);
							}
						}
						else
						{
							offs -= _lwm == 0 ? 3 : 2;
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
							//parse.Add(new Tuple<int, int>(len, offs));
							while (len-- > 0)
							{
								output.Add(output[output.Count - offs]);
							}
							_r0 = offs;
						}
						_lwm = 1;
						break;

					case 2:
						offs = getbyte();
						len = 2 + (offs & 1);
						offs >>= 1;
						//parse.Add(new Tuple<int, int>(len, offs));
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

						_r0 = offs;
						_lwm = 1;
						break;

					case 3:
						offs = 0;
						for (int i = 0; i < 4; i++)
						{
							offs <<= 1;
							offs |= getbit();
						}

						//parse.Add(new Tuple<int, int>(1, offs));

						if (offs > 0)
						{
							output.Add(output[output.Count - offs]);
						}
						else
						{
							output.Add(0);
						}
						_lwm = 0;
						break;

					default:
						throw new Exception("decap depack b0rk");
						break;

				}
			}

			return output.ToArray();
		}



	} // class
} // namespace
