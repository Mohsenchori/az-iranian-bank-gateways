using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PGTesterApp.RestSample.models.settle
{
    public class SettleCommand
    {
        public int merchantConfigurationId { get; set; }
        public ulong payGateTranId { get; set; }
    }
}