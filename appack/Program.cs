using System;
using System.IO;
using aplibsharp;
namespace appack
{
    class Program
    {
        static void Main(string[] args)
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
                output = aplib.encode(input);
            }
            else if (command == "d")
            {
                output = aplib.decode(input);
            }

            if (output.Length > 0)
            {
                SaveFile(output, savename);
            }
        }

        private static byte[] LoadFile(string filename)
        {
            var data = new byte[0];

            var fs = System.IO.File.OpenRead(filename);

            try
            {
                if (fs.Length == 0)
                {
                    System.Console.WriteLine("Empty file");
                    System.Environment.Exit(-1);
                }
                data = new byte[fs.Length];
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

            return data;
        }



        private static void SaveFile(byte[] input, string filename)
        {
            if (filename.Length == 0)
            {
                System.Console.WriteLine("No filename");
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
            System.Console.WriteLine("appack 1.3");
            System.Console.WriteLine();
            System.Console.WriteLine("Usage:");
            System.Console.WriteLine("appack d|e infile outfile");
        }
        private static bool ValidInput(string[] input)
        {
            return (
                input.Length == 3 &&
                input[0].Length == 1 &&
                (input[0] == "e" || input[0] == "d") &&
                System.IO.File.Exists(input[1]) &&
                !input[1].Equals(input[2])
            );
        }

    }
}
