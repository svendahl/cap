namespace cap
{
	class state
	{



		private int _lwm;
		private int _roffs;



		private int _bitcost;
		private readonly byte[] _input;
		private int _length;
		private int _offset;
		private ulong _pkey;
		private int _position;
		private int _steps;



		public int bitcost { get { return _bitcost; } }
		public byte[] input { get { return _input; } }
		public int length { get { return _length; } }
		public int lwm { get { return _lwm; } }
		public int offset { get { return _offset; } }
		public ulong pkey { get { return _pkey; } }
		public bool posis0 { get { return input[position] == 0x00; } }
		public int position { get { return _position; } }
		public int r0 { get { return _roffs; } }
		public int steps { get { return _steps; } }



		public ulong key
		{
			get
			{
				var key = new ulong[2];
				key[0] = (ulong) _lwm;
				key[1] = (ulong)_roffs;

				ulong output = 1099511628211;
				foreach (ulong k in key)
				{
					output ^= k;
					output *= 9782798678568883157;
					output = (output << 31) | (output >> 33);
					output *= 5545529020109919103;
				}
				return output;
			}
		}



		public state(byte[] input, int position, ulong pkey)
		{
			_input = input;
			_position = position;
			_pkey = pkey;

			_length = 0;
			_offset = 0;

			_lwm = 0;
			_roffs = 0;
		}



		public state(state input)
		{
			_bitcost = input.bitcost;
			_steps = input.steps;

			_input = input.input;
			_position = input.position;
			_pkey = input.key;

			_length = input.length;
			_offset = input.offset;

			_lwm = input.lwm;
			_roffs = input.r0;
		}



		public void add(int length, int offset, int cost)
		{
			add_steps(length, offset);
			add_lwm(length, offset);
			add_roffs(length, offset);
			add_edge(length, offset);
			_bitcost = cost;
		}



		public void add_steps(int length, int offset)
		{
			_steps++;
		}



		public void add_lwm(int length, int offset)
		{
			if (length == 1 && offset < 16)
			{
				_lwm = 0;
			}
			else
			{
				_lwm = 1;
			}
		}



		public void add_roffs(int length, int offset)
		{
			if (length >= 2 && offset > 0)
			{
				_roffs = offset;
			}
		}



		public void add_edge(int length, int offset)
		{
			_length = length;
			_offset = offset;
			_position += _length;
		}



	} // class
} // namespace
