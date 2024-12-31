using Gluten.Core.Interface;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Console = Colorful.Console;

namespace Frodo.Service
{
    /// <summary>
    /// Simple support for colour console logging
    /// </summary>
    internal class ColorConsoleService : IConsole
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
            Console.BackgroundColor = Color.Blue;
            Console.Write(line, Color.White);
            Console.BackgroundColor = Color.Black;
            Console.WriteLine("");
        }

        public void WriteLineRed(string line)
        {
            Console.BackgroundColor = Color.Red;
            Console.Write(line, Color.White);
            Console.BackgroundColor = Color.Black;
            Console.WriteLine("");
        }
    }
}
