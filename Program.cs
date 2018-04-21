using System;

namespace cap
{
	class Program
	{



		public static void Main(string[] args)
		{
			if (!ValidInput(args))
			{
				PrintUsage();
				System.Environment.Exit(-1);
			}



			string command = args[0];
			string loadname = args[1];
			string savename = args[2];

			var input = LoadFile(loadname);
			var output = new byte[0];

			if (command == "e")
			{
				output = cap.encode(input);
			}
			else if (command == "d")
			{
				output = cap.decode(input);
			}

			SaveFile(output, savename);
		}



		private static byte[] LoadFile(string filename)
		{
			if (!System.IO.File.Exists(filename))
			{
				System.Console.WriteLine("File not found");
				System.Environment.Exit(-1);
			}

			var fs = System.IO.File.OpenRead(filename);

			try
			{
				if (fs.Length <= 2)
				{
					System.Console.WriteLine("File too small");
					System.Environment.Exit(-1);
				}
				else if (fs.Length > 65538)
				{
					System.Console.WriteLine("File too large");
					System.Environment.Exit(-1);
				}
				var data = new byte[fs.Length];
				fs.Read(data, 0, (int)fs.Length);
				return data;
			}
			catch (System.Exception e)
			{
				System.Console.WriteLine("Exception: {0}", e);
			}
			finally
			{
				fs.Close();
			}

			return new byte[0];
		}



		private static void SaveFile(byte[] input, string filename)
		{
			if (filename.Length < 1)
			{
				System.Console.WriteLine("No filename");
				System.Environment.Exit(-1);
			}

			if (input.Length < 1)
			{
				System.Console.WriteLine("No data to save");
				System.Environment.Exit(-1);
			}

			if (System.IO.File.Exists(filename))
			{
				System.IO.File.Delete(filename);
			}


			var fs = System.IO.File.OpenWrite(filename);

			try
			{
				fs.Write(input, 0, input.Length);
			}
			catch (System.Exception e)
			{
				System.Console.WriteLine("Exception: {0}", e);
			}
			finally
			{
				fs.Close();
			}
		}



		private static void PrintUsage()
		{
			System.Console.WriteLine("cap v1.2");
			System.Console.WriteLine();
			System.Console.WriteLine("Usage: cap d|e infile outfile");
		}



		private static bool ValidInput(string[] input)
		{
			return (
				input.Length == 3 &&
				input[0].Length == 1 &&
				(input[0] == "e" || input[0] == "d") &&
				System.IO.File.Exists(input[1]) &&
				//!System.IO.File.Exists(input[2]) &&
				!input[1].Equals(input[2])
			);
		}



	}
}
