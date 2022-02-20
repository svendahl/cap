using System;
using aplibsharp;
namespace cap
{
    class Program
    {
        static void Main(string[] args)
        {
            var dargs = cmdflags.DecodeArgs(args);
            if (!dargs.valid)
            {
                PrintUsage();
                System.Environment.Exit(-1);
            }
            var alldata = LoadFile(dargs.infile);
            var data = new byte[alldata.Length - 2];
            int destination = alldata[1] << 8 | alldata[0];
            Array.Copy(alldata, 2, data, 0, data.Length);
            alldata = null;

            var output = new byte[0];
            if (dargs.cmd == "d" || dargs.cmd == "e")
            {
                var tdata = dargs.cmd == "d" ? aplib.decode(data) : aplib.encode(data);
                output = new byte[tdata.Length + 2];
                output[0] = (byte)(destination & 0xff);
                output[1] = (byte)(destination >> 8);
                Array.Copy(tdata, 0, output, 2, tdata.Length);
            }
            else if (dargs.cmd == "x")
            {
                var tdata = aplibsharp.aplib.encode(data, true);
                output = executable.make(tdata, destination, dargs.start, dargs.cpu, dargs.iflag);
            }

            if (output.Length > 0)
            {
                SaveFile(output, dargs.outfile);
            }
        }

        private static byte[] LoadFile(string filename)
        {
            var fs = System.IO.File.OpenRead(filename);
            var data = new byte[0];

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
                data = new byte[fs.Length];
                fs.Read(data, 0, (int)fs.Length);
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
            System.Console.WriteLine();
            System.Console.WriteLine("cap 1.3");
            System.Console.WriteLine();
            System.Console.WriteLine("cap <command> [-s=0xfce2 -p=0x37 -i=1] <infile> <outfile>");
            System.Console.WriteLine();
            System.Console.WriteLine("commands:");
            System.Console.WriteLine();
            System.Console.WriteLine("d - decode file");
            System.Console.WriteLine("e - encode file");
            System.Console.WriteLine("x - encode executable (handles $0200-$fff?)");
            System.Console.WriteLine();
            System.Console.WriteLine("executable flags:");
            System.Console.WriteLine();
            System.Console.WriteLine("-s - start address");
            System.Console.WriteLine("-c - cpu i/o port");
            System.Console.WriteLine("-i - interrupt flag");
            System.Console.WriteLine();
        }
    }
}
