using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace PGTesterApp.Business
{
    public class AsanPardakhtProvider
    {
        private int merchantID;
        private int merchantConfigID;
        private string userName;
        private string password;
        private string encryptionKey;
        private string encryptionVector;
        public AsanPardakhtProvider(int merchantID, int merchantConfigID, string userName, string password, string encryptionKey, string encryptionVector)

        {
            this.merchantID = merchantID;
            this.merchantConfigID = merchantConfigID;
            this.userName = userName;
            this.password = password;
            this.encryptionKey = encryptionKey;
            this.encryptionVector = encryptionVector;
        }

        public bool PrepareForPayment(int localInvoiceID, ulong amount, out string result, out string token)
        {

            result = string.Empty;
            token = string.Empty;

            string p1 = "1";
            string p2 = this.userName;
            string p3 = this.password;
            string p4 = localInvoiceID.ToString();
            string p5 = amount.ToString();
            string p6 = GetDateTimeProperFormat();
            string p7 = " ";
            string p8 = HttpContext.Current.Request.Url.ToString().Replace("startpayment", "receivepayment");
            string p9 = " ";
            string toBeEncrypted = p1 + "," + p2 + "," + p3 + "," + p4 + "," + p5 + "," + p6 + "," + p7 + "," + p8 + "," + p9;


            string encryptedString = string.Empty;
            AES2 aesProvider = new AES2(this.encryptionKey, this.encryptionVector);
            bool encryptionIsSuccessful = aesProvider.Encrypt(toBeEncrypted, out encryptedString);

            if (!encryptionIsSuccessful)
                return false;

            try
            {
                AsanPardakhtIPG.merchantservices merchantServices = new AsanPardakhtIPG.merchantservices();
                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => { return true; };
                string res = merchantServices.RequestOperation(this.merchantConfigID, encryptedString);
                string[] splittedArray = res.Split(',');
                merchantServices.Abort();
                merchantServices = null;
                if (splittedArray.Length == 2)
                {
                    result = splittedArray[0];
                    token = splittedArray[1];
                    return true;
                }
                else if (splittedArray.Length == 1)
                {
                    result = splittedArray[0];
                    token = string.Empty;
                    return false;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool VerifyTrx(ulong tranID, out string res)
        {
            try
            {
                res = string.Empty;
                string toBeEncrypted = userName + "," + password;
                string encryptedString = string.Empty;
                AES2 aesProvider = new AES2(this.encryptionKey, this.encryptionVector);
                bool encryptionIsSuccessful = aesProvider.Encrypt(toBeEncrypted, out encryptedString);
                if (encryptionIsSuccessful)
                {
                    AsanPardakhtIPG.merchantservices merchantServices = new AsanPardakhtIPG.merchantservices();
                    ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => { return true; };
                    res = merchantServices.RequestVerification(this.merchantConfigID, encryptedString, tranID);
                    return (res == "500");
                }
                else
                {
                    res = string.Empty;
                    return false;
                }
            }
            catch (Exception ex)
            {
                res = string.Empty;
                return false;
            }
        }

        public bool SettleTrx( ulong tranID, out string res)
        {
            try
            {
                res = string.Empty;
                string toBeEncrypted = userName + "," + password;
                string encryptedString = string.Empty;
                AES2 aesProvider = new AES2(this.encryptionKey, this.encryptionVector);
                bool encryptionIsSuccessful = aesProvider.Encrypt(toBeEncrypted, out encryptedString);
                if (encryptionIsSuccessful)
                {
                    AsanPardakhtIPG.merchantservices merchantServices = new AsanPardakhtIPG.merchantservices();
                    ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => { return true; };
                    res = merchantServices.RequestReconciliation(this.merchantConfigID, encryptedString, tranID);
                    return (res == "600");
                }
                else
                {
                    res = string.Empty;
                    return false;
                }
            }
            catch (Exception ex)
            {
                res = string.Empty;
                return false;
            }
        }

        public bool ReverseTrx( ulong tranID, out string res)
        {
            try
            {
                res = string.Empty;
                string toBeEncrypted = userName + "," + password;
                string encryptedString = string.Empty;
                AES2 aesProvider = new AES2(this.encryptionKey, this.encryptionVector);
                bool encryptionIsSuccessful = aesProvider.Encrypt(toBeEncrypted, out encryptedString);
                if (encryptionIsSuccessful)
                {
                    AsanPardakhtIPG.merchantservices merchantServices = new AsanPardakhtIPG.merchantservices();
                    ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => { return true; };
                    res = merchantServices.RequestReversal(this.merchantConfigID, encryptedString, tranID);
                    return (res == "700");
                }
                else
                {
                    res = string.Empty;
                    return false;
                }
            }
            catch (Exception ex)
            {
                res = string.Empty;
                return false;
            }
        }

        private string GetDateTimeProperFormat()
        {
            DateTime now = DateTime.Now;
            return now.Year.ToString() + now.Month.ToString().PadLeft(2, '0') +
                now.Day.ToString().PadLeft(2, '0') + " " + now.Hour.ToString().PadLeft(2, '0') +
                now.Minute.ToString().PadLeft(2, '0') + now.Second.ToString().PadLeft(2, '0');
        }
    }
}