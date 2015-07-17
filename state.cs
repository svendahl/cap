using System;

namespace cap
{
	class state
	{



		public readonly int bitcost;
		public readonly byte[] input;
		public readonly int length;
		public readonly int offset;
		public readonly int pkey;
		public readonly int position;
		public readonly int roffs;
		public readonly int steps;



		public int lwm { get { return length < 2 ? 0 : 1; } }
		public bool posis0 { get { return position < input.Length ? input[position] == 0x00 : false; } }



		public state(byte[] input)
		{
			bitcost = 0;
			this.input = input;
			length = 0;
			offset = 0;
			pkey = 0;
			position = 0;
			roffs = 0;
			steps = 0;
		}



		public state(state s, Tuple<int,int> e)
		{
			bitcost = s.bitcost + model.edge_cost(e.Item1, e.Item2, s);
			this.input = s.input;
			length = e.Item1;
			offset = e.Item2;
			pkey = s.roffs;
			position = s.position + e.Item1;
			roffs = e.Item1 > 1 ? e.Item2 : s.roffs;
			steps = s.steps + 1;
		}



		public state(state s, Tuple<int, Tuple<int, int>> ce)
		{
			bitcost = s.bitcost + ce.Item1;
			input = s.input;
			length = ce.Item2.Item1;
			offset = ce.Item2.Item2;
			pkey = s.roffs;
			position = s.position + ce.Item2.Item1;
			roffs = ce.Item2.Item1 > 1 ? ce.Item2.Item2 : s.roffs;
			steps = s.steps + 1;
		}



	} // class
} // namespace
