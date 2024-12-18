using Gluten.Core.Interface;
using Gluten.Data.PinCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gluten.Core.LocationProcessing.Service
{
    internal class ChainPinCache(List<ChainPinCache> _pinCache,
        IConsole Console)
    {
        public TopicPinCache? TryGetPin(string? placeName, string country)
        {
            if (placeName == null) return null;
            if (_pinCache == null) return null;
            return null;
        }
    }
}
