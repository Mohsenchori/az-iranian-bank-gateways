using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PGTesterApp.RestSample.models.settle
{
    public class SettleVm : IResponseVm
    {
        public int ResCode { get; set; }
        public string ResMessage { get; set; }
    }

}