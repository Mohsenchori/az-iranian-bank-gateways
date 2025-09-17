using Newtonsoft.Json;
using PGTesterApp.RestSample.Moldels.Sale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PGTesterApp.RestSample.models.bill
{
    public class BillCommand : SaleCommand
    {
        public BillCommand(int merchantConfigurationId, int serviceTypeId, long orderId, ulong amountInRials, string callbackURL
            , string billId, string payId)
            : base(merchantConfigurationId, serviceTypeId, orderId, amountInRials, callbackURL, JsonConvert.SerializeObject(new
            {
                billId = billId,
                payId = payId
            }))
        {
        }
    }
}