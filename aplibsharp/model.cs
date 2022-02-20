using System.Diagnostics.Contracts;
using System.Numerics;

namespace aplibsharp
{
    static internal class model
    {

        static public bool l1valid(state state, int offset)
        {
            Contract.Assert(state != null && offset >= 0 && offset < constant.threshold_byte_match_offset);
            return offset == 0 || offset >= 0 && offset < constant.threshold_byte_match_offset && (state.lwm == 0 || state.lwm == 1 && offset != state.roffs);
        }

        static public bool valid(state state, int length, int offset)
        {
            Contract.Assert(state != null);
            return offset != state.offset &&
                (
                    (offset == state.roffs && state.lwm == 0 && state.roffs != 0) ||
                    (length > 3 && length <= constant.threshold_length && offset > 0 && offset < constant.threshold_offset) ||
                    (length == 2 && offset > 0 && offset < constant.threshold_length2_match_offset) ||
                    (length == 3 && offset > 0 && offset < constant.threshold_length3_match_offset)
                );
        }

        static public int l1cost(state state, int offset, byte[] input)
        {
            Contract.Assert(state != null && input != null && offset >= 0 && offset < constant.threshold_byte_match_offset);
            int output = 0;

            if (offset > 0 && offset < constant.threshold_byte_match_offset || offset == 0 && input[state.index] == 0x00 && state.index > 0)
            {
                output += constant.bitcost_prefixcode_byte_match + constant.bitcost_offset_byte_match;
            }
            else/* if (edge.offset == 0)*/
            {
                output += constant.bitcost_prefixcode_literal + constant.bitcost_literal;
            }

            return output;
        }

        static public int cost(state state, int length, int offset)
        {
            Contract.Assert(state != null && length > 1 && length <= constant.threshold_length && offset > 0 && offset < constant.threshold_offset);
            int output = 0;

            if (length >= 2 && offset == state.roffs && state.lwm == 0)
            {//repeat offset / gapped match
                output += constant.bitcost_prefixcode_normal_match;
                output += 2;//gammabits(2);
                output += gammabits(length);
            }
            else if (length >= 2 && length <= 3 && offset > 0 && offset < constant.threshold_short_match_offset)
            {//short
                output += constant.bitcost_prefixcode_short_match;
                output += constant.bitcost_literal;
            }
            else if (
                (length == 2 && offset >= constant.threshold_short_match_offset && offset < constant.threshold_length2_match_offset) ||
                (length == 3 && offset >= constant.threshold_short_match_offset && offset < constant.threshold_length3_match_offset) ||
                (length >= 4 && length <= constant.threshold_length && offset >= 1 && offset < constant.threshold_offset)
            )
            {//normal
                output += constant.bitcost_prefixcode_normal_match;

                int offset_hi = (3 - state.lwm) + (offset >> 8);
                output += gammabits(offset_hi);
                output += 8;

                int ldo = 0;
                if (offset < constant.threshold_short_match_offset || offset >= constant.threshold_length3_match_offset)
                {
                    ldo = 2;
                }
                else if (offset >= constant.threshold_length2_match_offset)
                {
                    ldo = 1;
                }
                output += gammabits(length - ldo);
            }

            return output;
        }

        public static int gammabits(int input)
        {
            Contract.Assert(input > 1 && input <= constant.threshold_length);
            return BitOperations.Log2((uint)input) * 2;
        }

        public static int repcost(int input)
        {
            Contract.Assert(input > 1 && input <= constant.threshold_length);

            int output = constant.bitcost_prefixcode_normal_match;
            output += 2;//gammabits(2);
            output += gammabits(input);
            return output;
        }

    }
}
