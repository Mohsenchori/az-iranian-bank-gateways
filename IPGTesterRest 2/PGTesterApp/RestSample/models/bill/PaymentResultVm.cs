using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PGTesterApp.RestSample.models.bill
{
    public class PaymentResultVm : IResponseVm
    {
        public string CardNumber { get; set; }
        public string Rrn { get; set; } 
        public string RefId { get; set; }
        public decimal Amount { get; set; }
        public long? PayGateTranID { get; set; }
        public int ResCode { get; set; }
        public string ResMessage { get; set; }
    }
}