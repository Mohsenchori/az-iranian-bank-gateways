using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PGTesterApp.Business
{
    public class AsanPardakhtPGResultDescriptor
    {
        private string amount;
        private string preInvoiceID;
        private string token;
        private string resCode;
        private string messageText;
        private string payGateTranID;
        private string rrn;
        private string lastFourDigitOfPAN;

        private AsanPardakhtPGResultDescriptor(
          string amount,
         string preInvoiceID,
         string token,
         string resCode,
         string messageText,
         string payGateTranID,
         string rrn,
         string lastFourDigitOfPAN
            )
        {
            this.amount = amount;
            this.preInvoiceID = preInvoiceID;
            this.token = token;
            this.resCode = resCode;
            this.messageText = messageText;
            this.payGateTranID = payGateTranID;
            this.rrn = rrn;
            this.lastFourDigitOfPAN = lastFourDigitOfPAN;
        }

        public static AsanPardakhtPGResultDescriptor AsanPardakhtTrxResultDescriptorFromString(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }

            string[] resArray = str.Split(',');
            if (resArray == null || resArray.Length != 8)
            {
                return null;
            }

            return new AsanPardakhtPGResultDescriptor(resArray[0], resArray[1], resArray[2], resArray[3], resArray[4], resArray[5], resArray[6], resArray[7]);
        }

        #region Public Properties

        public string Amount
        {
            get
            {
                return this.amount;
            }
        }

        public string PreInvoiceID
        {
            get
            {
                return this.preInvoiceID;
            }
        }

        public string Token
        {
            get
            {
                return this.token;
            }
        }

        public string ResCode
        {
            get
            {
                return this.resCode;
            }
        }

        public string MessageText
        {
            get
            {
                return this.messageText;
            }
        }

        public string PayGateTranID
        {
            get
            {
                return this.payGateTranID;
            }
        }

        public string RRN
        {
            get
            {
                return this.rrn;
            }
        }

        public string LastFourDigitOfPAN
        {
            get
            {
                return this.lastFourDigitOfPAN;
            }
        }

        #endregion
    }
}