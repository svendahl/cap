using System.IO;
using System.Linq;
namespace cap
{
    internal class cmdflags
    {
        public string cmd = string.Empty;
        public string infile = string.Empty;
        public string outfile = string.Empty;
        public int start = 43121;
        public int cpu = 55;
        public int iflag = 1;
        public bool valid
        {
            get
            {
                return cmd.Length == 1 && infile.Length > 0 && File.Exists(infile) && outfile.Length > 0;
            }
        }

        static public cmdflags DecodeArgs(string[] input)
        {
            var output = new cmdflags();

            foreach (string arg in input)
            {
                if (arg.Length == 1 && output.cmd.Length == 0 && (arg == "d" || arg == "e" || arg == "x"))
                {
                    output.cmd = arg;
                }
                else if (arg.Length > 1 && arg.StartsWith("-"))
                {
                    string[] flag = arg.ToLower().Replace("-", "").Split("=");
                    if (flag.Length == 2)
                    {
                        int value = 0;
                        if (flag[1].ToLower().Contains("x"))
                        {
                            string hexval = flag[1].Split("x")[1];
                            int tvalue = 0;
                            if (int.TryParse(hexval, System.Globalization.NumberStyles.HexNumber, null, out tvalue))
                            {
                                value = tvalue;
                            }
                        }
                        else if (flag[1].All(char.IsDigit))
                        {
                            int tvalue = 0;
                            if (int.TryParse(flag[1], out tvalue))
                            {
                                value = tvalue;
                            }
                        }

                        if (flag[0] == "s")
                        {
                            output.start = value & 0xffff;
                        }
                        else if (flag[0] == "c")
                        {
                            output.cpu = value & 0xff;
                        }
                        else if (flag[0] == "i")
                        {
                            output.iflag = value & 1;
                        }
                    }
                }
                else if (output.infile.Length == 0 && System.IO.File.Exists(arg))
                {
                    output.infile = arg;
                }
                else if (output.outfile.Length == 0)
                {
                    output.outfile = arg;
                    break;
                }
            }

            return output;
        }

    }
}