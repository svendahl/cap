using System;

namespace cap
{
	class state
	{



		public readonly int bitcost;
		public readonly Tuple<int, int> edge;
		public readonly int index;
		public readonly int steps;
		public readonly int roffs;
		public readonly state parent;



		public int lwm { get { return edge == null || edge.Item1 == 1 || edge.Item2 == 0 ? 0 : 1; } }



		public state()
		{
			bitcost = 0;
			edge = null;
			index = 0;
			parent = null;
			roffs = 0;
			steps = 0;
		}



		public state(state state, Tuple<int, int> edge, byte[] input)
		{
			bitcost = state.bitcost + model.cost(state, edge, input);
			this.edge = edge;
			index = state.index + edge.Item1;
			parent = state;
			roffs = edge != null && edge.Item1 > 1 ? edge.Item2 : state.roffs;
			steps = state.steps + 1;
		}



	}
}
