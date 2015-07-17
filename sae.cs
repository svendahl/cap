namespace cap
{
	class sae
	{



		public readonly int[] isa;
		public readonly int[] lcp;
		public readonly int[] sa; //Suffix Array Induced Sorting Algorithm (sais.cs) from https://sites.google.com/site/yuta256/sais



		public sae(byte[] input)
		{
			sa = SuffixArray.SAIS.sa(input);
			isa = saaux.isa(sa);
			lcp = saaux.lcp(input, sa, isa);
		}



	} // class
} // namespace
