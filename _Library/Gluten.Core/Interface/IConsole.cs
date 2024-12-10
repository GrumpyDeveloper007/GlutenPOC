using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gluten.Core.Interface
{
    /// <summary>
    /// Simple abstraction for logging, TODO: Use logging framework?
    /// </summary>
    public interface IConsole
    {
        /// <summary>
        /// Log line
        /// </summary>
        void WriteLine(string line);
        /// <summary>
        /// Log line
        /// </summary>
        void WriteLineRed(string line);
        /// <summary>
        /// Log line
        /// </summary>
        void WriteLineBlue(string line);
        /// <summary>
        /// Log line
        /// </summary>
        void WriteLine(List<string> line);
        /// <summary>
        /// Log line
        /// </summary>
        void WriteLine(int line);
        /// <summary>
        /// Clear recent history
        /// </summary>
        void Clear();
    }
}
