using System;
using System.Collections.Generic;

namespace cap
{
	class path
	{



		private readonly byte[] input;
		private readonly esa esa;



		public path(byte[] input)
		{
			this.input = input;
			esa = new esa(input);
		}



		private state cheapest_of(state c1, state c2)
		{
			if (c1 == null || c2.bitcost < c1.bitcost || c2.bitcost == c1.bitcost && c2.steps < c1.steps)
			{
				return c2;
			}
			return c1;
		}



		private void evaluate_mo(ref state[] c_s, int index, int length, int offset)
		{
			if (model.valid(c_s[index], length, offset))
			{
				int dindex = index + length;
				if (c_s[dindex] == null || c_s[index].bitcost < c_s[dindex].bitcost)
				{
					var dstate = new state(c_s[index], new Tuple<int, int>(length, offset), input);
					c_s[dindex] = cheapest_of(c_s[dindex], dstate);
				}
			}
		}



		private void evaluate_ro(ref state[] c_s, ref Tuple<int,int>[] r_c, ref state[] r_s, int index, int length, int offset)
		{
			if (r_s[offset] == null && r_c[offset] == null)
			{
				evaluate_ro_rin_candidate(index, length, offset, ref r_c);
			}
			else if (r_s[offset] != null && r_c[offset] == null)
			{
				int gindex = r_s[offset].index;
				int glength = index - gindex;

				if (glength <= constant.threshold_gap_length)
				{
					var dstate = evaluate_ro_gapstate(r_s[offset], glength, c_s);

					if (dstate.index == index)
					{
						evaluate_ro_rmidrout(ref c_s, ref r_c, ref r_s, dstate, index, length, offset);
					}
					else
					{
						evaluate_ro_rin_candidate(index, length, offset, ref r_c);
						r_s[offset] = null;
					}
				}
				else
				{
					evaluate_ro_rin_candidate(index, length, offset, ref r_c);
					r_s[offset] = null;
				}
			}
			else if (r_s[offset] == null && r_c[offset] != null)
			{
				int gindex = r_c[offset].Item1 + r_c[offset].Item2;
				int glength = index - gindex;

				if (glength <= constant.threshold_gap_length)
				{
					var dstate = evaluate_ro_cheapest_rin(c_s, r_c[offset].Item1, r_c[offset].Item2, offset);

					if (c_s[dstate.index] == null || dstate.bitcost <= c_s[dstate.index].bitcost + constant.repeat_offset_edge_bitcost_maxdeviation)
					{
						dstate = evaluate_ro_gapstate(dstate, glength, c_s);

						if (dstate.index == index)
						{
							evaluate_ro_rmidrout(ref c_s, ref r_c, ref r_s, dstate, index, length, offset);
						}
						else
						{
							evaluate_ro_rin_candidate(index, length, offset, ref r_c);
						}
					}
				}
				else
				{
					evaluate_ro_rin_candidate(index, length, offset, ref r_c);
				}

			}
			else if (r_s[offset] != null && r_c[offset] != null)
			{
				throw new Exception("!WAT!");
			}
			else
			{
				throw new Exception("!WAT!");
			}
		}



		private void evaluate_ro_rin_candidate(int index, int length, int offset, ref Tuple<int,int>[] r_c)
		{
			if (model.valid(length, offset))
			{
				r_c[offset] = new Tuple<int, int>(index, length);
			}
			else
			{
				r_c[offset] = null;
			}
		}
		


		private state evaluate_ro_cheapest_rin(state[] c_s, int index, int length, int offset)
		{
			state state = null;

			while (length > 1)
			{
				if (c_s[index] != null && model.valid(c_s[index], length, offset))
				{
					state = cheapest_of(state, new state(c_s[index], new Tuple<int, int>(length, offset), input));
				}

				index++;
				length--;
			}

			return state;
		}



		private state evaluate_ro_gapstate(state state, int length, state[] c_s)
		{
			int dindex = state.index + length;
			while (state.index < dindex)
			{
				state = new state(state, new Tuple<int, int>(1, esa.l1o[state.index]), input);
				if (c_s[state.index] != null && state.bitcost > c_s[state.index].bitcost + constant.repeat_offset_gap_bitcost_maxdeviation)
				{
					break;
				}
			}

			return state;
		}



		private void evaluate_ro_rmidrout(ref state[] c_s, ref Tuple<int, int>[] r_c, ref state[] r_s, state state, int index, int length, int offset)
		{
			bool do_r_s = true;

			int _length = length;

			while (length > 1)
			{
				var dstate = new state(state, new Tuple<int, int>(length, offset), input);
				if (do_r_s)
				{
					if (model.valid(length, offset))
					{
						var cstate = new state(c_s[index], new Tuple<int, int>(length, offset), input);
						r_s[offset] = cheapest_of(cstate, dstate);
					}
					else
					{
						r_s[offset] = dstate;
					}
					do_r_s = false;
				}
				c_s[dstate.index] = cheapest_of(c_s[dstate.index], dstate);

				length--;
			}

			length = _length;
			if (model.valid(length, offset))
			{
				var rstate = evaluate_ro_cheapest_rin(c_s, index, length, offset);
				c_s[index + length] = cheapest_of(c_s[index + length], rstate);
				r_s[offset] = cheapest_of(r_s[offset], rstate);
			}

			r_c[offset] = null;
		}

	
		
		public Tuple<int,int>[] output()
		{
			var c_s = new state[esa.input.Length + 1];
			c_s[0] = new state();

			var r_c = new Tuple<int, int>[constant.threshold_offset + 1];
			var r_s = new state[constant.threshold_offset + 1];

			for (int i = 0; i < input.Length; i++)
			{
				if (c_s[i] != null)
				{
					foreach(Tuple<bool, int, int> t in esa.tedges(i))
					{
						bool maximal = t.Item1;
						int length = t.Item2;
						int offset = t.Item3;

						if (maximal)
						{
							evaluate_ro(ref c_s, ref r_c, ref r_s, i, length, offset);
						}
						else
						{
							evaluate_mo(ref c_s, i, length, offset);
						}
					}
				}
				else
				{
					throw new Exception("!WAT!");
				}
			}

			state state = c_s[c_s.Length - 1];
			var output = new Tuple<int,int>[state.steps];
			int op = output.Length - 1;
			while (state.parent != null)
			{
				output[op--] = state.edge;
				state = state.parent;
			}

			if (!verify(input, output))
			{
				throw new Exception("path b0rk");
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

	
	
	}
}
