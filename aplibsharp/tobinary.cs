using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace aplibsharp
{
    class tobinary
    {
        private readonly byte[] input;
        private int bitbuffer;
        private int bitbuffer_ptr;
        private int input_ptr;
        private List<byte> result;
        private state state;
        public byte[] output { get { return result.ToArray(); } }

        public tobinary(byte[] input, Tuple<int, int>[] path, bool eof = true)
        {
            this.input = input;
            result = new List<byte>();
            if (input.Length > 0 && path.Length > 0)
            {
                result.Add(input[0]);
                input_ptr = 1;

                bitbuffer = 1;
                bitbuffer_ptr = result.Count;
                result.Add(0);

                state = new state();
                state = new state(state, path[0].Item1, path[0].Item2, 0);

                for (int i = 1; i < path.Length; i++)
                {
                    encodestep(path[i]);
                }

                if (eof)
                {
                    putbits(constant.prefixcode_short_match);
                    result.Add(0);
                }

                while (bitbuffer < 256)
                {
                    bitbuffer <<= 1;
                }
                result[bitbuffer_ptr] = (byte)(bitbuffer & 0xff);
            }
        }

        private void encodestep(Tuple<int, int> edge)
        {
            Contract.Assert(edge.Item1 > 0 && edge.Item1 < constant.threshold_length && edge.Item2 >= 0 && edge.Item2 < constant.threshold_offset);

            int length = edge.Item1;
            int offset = edge.Item2;

            if (length == 1)
            {
                if (offset == 0 && input[state.index] != 0x00)
                {
                    putbits(constant.prefixcode_literal);
                    result.Add(input[input_ptr]);
                }
                else
                {
                    putbits(constant.prefixcode_byte_match);
                    for (int s = 3; s >= 0; s--)
                    {
                        putbit(((offset >> s) & 1));
                    }
                }
            }
            else if (length >= 2 && offset == state.roffs && state.lwm == 0)
            {
                putbits(constant.prefixcode_normal_match);
                putgbits(2);
                putgbits(length);
            }
            else if (length >= 2 && length <= 3 && offset > 0 && offset < constant.threshold_short_match_offset)
            {
                putbits(constant.prefixcode_short_match);
                result.Add((byte)(offset << 1 | (length & 1)));
            }
            else
            {
                putbits(constant.prefixcode_normal_match);
                putgbits((offset >> 8) + (3 - state.lwm));
                result.Add((byte)(offset & 0xff));
                putgbits(length - (offset < constant.threshold_short_match_offset || offset >= constant.threshold_length3_match_offset ? 2 : offset >= constant.threshold_length2_match_offset ? 1 : 0));
            }
            state = new state(state, edge.Item1, edge.Item2, 0);
            input_ptr += length;
        }

        private void putbit(int bit)
        {
            if (bitbuffer > 0xff)
            {
                bitbuffer &= 0xff;
                result[bitbuffer_ptr] = (byte)bitbuffer;
                bitbuffer_ptr = result.Count;
                result.Add(0);
                bitbuffer = 1;
            }

            bitbuffer <<= 1;
            bitbuffer |= bit;
        }

        private void putbits(int[] bits)
        {
            foreach (int bit in bits)
            {
                putbit(bit);
            }
        }

        private void putgbits(int input)
        {
            int msb = 15;
            while (input >> msb-- == 0) { }

            while (msb >= 0)
            {
                int bit = (input >> msb) & 1;
                putbit(bit);
                msb--;
                putbit(msb >= 0 ? 1 : 0);
            }
        }

    }
}
