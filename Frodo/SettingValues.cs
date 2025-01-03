﻿// Ignore Spelling: SQL CONNECTIONSTRING Api

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frodo
{
    internal class SettingValues
    {
        public required string AZURE_SQL_CONNECTIONSTRING { get; set; }

        public required string AIEndPoint { get; set; }
        public required string AIApiKey { get; set; }
        public required string DbEndpointUri { get; set; }
        public required string DbPrimaryKey { get; set; }

        public required string GroqApiKey { get; set; }
        public required string OpenRouterApiKey { get; set; }
    }

}
