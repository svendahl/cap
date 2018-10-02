using System;

namespace cap
{
	static class cap
	{

	
		
		public static byte[] encode(byte[] data)
		{
			var output = new tobinary(data, new path(data).output()).output;

			if (!verify(data, output))
			{
				throw new Exception("verify fail");
			}

			return output;
		}



		public static byte[] decode(byte[] input)
		{
			return new decap(input).depack();
		}



		private static bool verify(byte[] data, byte[] cdata)
		{
			var ddata = new decap(cdata).depack();

			if (data.Length != ddata.Length)
			{
				return false;
			}

			for (int i = 0; i < data.Length; i++)
			{
				if (data[i] != ddata[i])
				{
					return false;
				}
			}

			return true;
		}



	}
}
