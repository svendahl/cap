namespace aplibsharp
{
    internal static class constant
    {
        internal readonly static int[] prefixcode_literal = new int[1] { 0 };
        internal readonly static int[] prefixcode_normal_match = new int[2] { 1, 0 };
        internal readonly static int[] prefixcode_short_match = new int[3] { 1, 1, 0 };
        internal readonly static int[] prefixcode_byte_match = new int[3] { 1, 1, 1 };

        internal static int bitcost_prefixcode_literal
        {
            get
            {
                return prefixcode_literal.Length;
            }
        }

        internal static int bitcost_prefixcode_normal_match
        {
            get
            {
                return prefixcode_normal_match.Length;
            }
        }

        internal static int bitcost_prefixcode_short_match
        {
            get
            {
                return prefixcode_short_match.Length;
            }
        }

        internal static int bitcost_prefixcode_byte_match
        {
            get
            {
                return prefixcode_byte_match.Length;
            }
        }

        internal const int bitcost_offset_byte_match = 4; //log2(threshold_byte_match_offset)
        internal const int bitcost_offset_short_match = 8; //log2(threshold_short_match_offset) << 1
        internal const int bitcost_literal = 8; //bits per byte
        internal const int threshold_byte_match_offset = 16; //4 bits (0-15)
        internal const int threshold_short_match_offset = 128; //7 bits (0-127)
        internal const int threshold_length2_match_offset = 1280; //11 bits (1-1279)
        internal const int threshold_length3_match_offset = 32000; //14 bits (1-31999)
        internal const int threshold_offset = 65024; //(256-2) << 8
        internal const int threshold_length = 65535; //ushort.MaxLength
        internal const int threshold_gap_length = 64;//increase to approach optimality at the cost of encoding time
        internal const int threshold_greedy_runlength = 64;//increase to approach optimality at the cost of encoding time
    }
}
