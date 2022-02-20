namespace aplibsharp
{
    static public class aplib
    {
        static public byte[] encode(byte[] data, bool eof = true)
        {
            var parse = path.bfs(data);
            var binary = new tobinary(data, parse, eof).output;
            return binary;
        }

        static public byte[] decode(byte[] data)
        {
            return new decode(data).depack();
        }
    }
}
