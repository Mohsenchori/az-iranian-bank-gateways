using Newtonsoft.Json;
using PGTesterApp.RestSample.Moldels.Sale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PGTesterApp.RestSample.models.telecom
{
    public class TelecomChargeCommand : SaleCommand
    {
        public TelecomChargeCommand(int merchantConfigurationId, TelecomChargeServiceType serviceType, long orderId, ulong amountInRials, string callbackURL
            , string destinationMobile, int productId)
            : base(merchantConfigurationId, (int)serviceType, orderId, amountInRials, callbackURL, JsonConvert.SerializeObject(new
            {
                destinationMobile,
                productId
            }))
        {
        }
    }
}