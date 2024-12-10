using Gluten.Core.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gluten.Core.Service
{
    /// <summary>
    /// Simple, default logger
    /// </summary>
    public class DummyConsole : IConsole
    {
        public void Clear()
        {
            Console.Clear();
        }

        public void WriteLine(string line)
        {
            Console.WriteLine(line);
        }

        public void WriteLine(List<string> line)
        {
            Console.WriteLine(line);
        }

        public void WriteLine(int line)
        {
            Console.WriteLine(line);
        }

        public void WriteLineBlue(string line)
        {
            Console.WriteLine(line);
        }

        public void WriteLineRed(string line)
        {
            Console.WriteLine(line);
        }
    }
}
