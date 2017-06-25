﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MohawkCollege.Util.Console.Parameters;

namespace DisconnectedClient
{
    /// <summary>
    /// Console parameters
    /// </summary>
    public class ConsoleParameters
    {

        /// <summary>
        /// Starts the IMS in debug mode
        /// </summary>
        [Parameter("debug")]
        public bool Debug { get; set; }
    }
}
