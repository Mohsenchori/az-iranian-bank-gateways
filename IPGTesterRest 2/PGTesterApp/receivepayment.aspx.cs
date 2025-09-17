using PGTesterApp.Business;
using PGTesterApp.RestSample;
using PGTesterApp.RestSample.models.settle;
using PGTesterApp.RestSample.models.verify;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace PGTesterApp
{
    public partial class receivepayment : System.Web.UI.Page
    {

        private void AddToConsole(string txt)
        {
            txbxConsole.Text += "\n" + txt;
        }

        protected void btnVerify_Click(object sender, EventArgs e)
        {
            ulong payGateTranID; ;
            if (ViewState["PayGateTranID"] == null ||
                !ulong.TryParse(ViewState["PayGateTranID"].ToString(),
                out payGateTranID) || payGateTranID < 1)
            {
                AddToConsole("شماره پیگیری تراکنش وجود ندارد");
                return;
            }

            var verifyCommand = new VerifyCommand()
            {
                merchantConfigurationId = int.Parse(ConfigurationManager.AppSettings["MerchantConfigurationId"]),
                payGateTranId = payGateTranID
            };
            var ipgService = new IPGResetService();
            var verifyRes = ipgService.VerifyTrx(verifyCommand).Result;

            AddToConsole("Result: " + verifyRes);

            if (verifyRes.ResCode == 0)
            {
                AddToConsole("وریفای موفق با نتیجه موفق");
                return;
            }
            else
            {
                AddToConsole("VerifyTrx Method Evaluation Result: " + verifyRes.ResMessage.ToString());
                AddToConsole("verifyRes: " + verifyRes);
            }
        }

        protected void btnSettle_Click(object sender, EventArgs e)
        {
            ulong payGateTranID; ;
            if (ViewState["PayGateTranID"] == null || !ulong.TryParse(ViewState["PayGateTranID"].ToString(), out payGateTranID) || payGateTranID < 1)
            {
                AddToConsole("شماره پیگیری تراکنش وجود ندارد");
                return;
            }

            var ipgService = new IPGResetService();
            var settleRes = ipgService.SettleTrx(
                new SettleCommand()
                {
                    merchantConfigurationId = int.Parse(ConfigurationManager.AppSettings["MerchantConfigurationId"]),
                    payGateTranId = payGateTranID
                })
                .Result;

            if (settleRes.ResCode == 0)
            {
                AddToConsole("ستل موفق با نتیجه موفق");
                return;
            }
            else
            {
                AddToConsole("SettleTrx Method Evaluation Result: " + settleRes.ResMessage.ToString());
                AddToConsole("settleRes: " + settleRes);
            }
        }

        protected void btnReversal_Click(object sender, EventArgs e)
        {
            ulong payGateTranID; ;
            if (ViewState["PayGateTranID"] == null || !ulong.TryParse(ViewState["PayGateTranID"].ToString(), out payGateTranID) || payGateTranID < 1)
            {
                AddToConsole("شماره پیگیری تراکنش وجود ندارد");
                return;
            }
            var ipgService = new IPGResetService();
            var reversalRes = ipgService.ReverseTrx(
                new RestSample.models.reverse.ReverseCommand()
                {
                    merchantConfigurationId = int.Parse(ConfigurationManager.AppSettings["MerchantConfigurationId"]),
                    payGateTranId = payGateTranID
                })
                .Result;

            if (reversalRes.ResCode == 0)
            {
                AddToConsole("ریورس موفق با نتیجه موفق");
                return;
            }
            else
            {
                AddToConsole("ReverseTrx Method Evaluation Result: " + reversalRes.ResMessage.ToString());
                AddToConsole("ReverseTrx: " + reversalRes);
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                //بررسی نتیجه پرداخت 

                //روش اول 
                //Call TranResult Rest Api With MerchantConfigurationId & LocalInvoiceId
                //استعلام نتیجه تراکنش با استفاده از متد ترن ریزالت شماره فاکتور محلی ارسالی  همراه
                //آدرس بازگشت  (در هنگام ساخت تراکنش ) به صورت زیر است

                //برای تست روش اول دو خط زیر را از کامنت در آورید و اسپس برنامه را اجرا نمایید
                TranResultInquiryWithLocalInvoiceId();
                return;
                //برای تست روش اول دو خط بالا را از کامنت در آورید و اسپس برنامه را اجرا نمایید


                //روش دوم
                //Decryt ReturningPratams
                // بررسی نتیجه تراکنش با استفاده از گشودن رشته ارسالی نتیجه تراکنش 
                //با نام ریترنینگ پارامز که توسط درگاه پرداخت  آسان پرداخت به صفحه بازگشت پذیرنده ترسال می گردد 
                //این روش  خیلی توصیه نمی گردد

                //برای تست روش دوم دو خط زیر را از کامنت در آورید و سپس برنامه را اجرا نمایید
                // TranResultInquiryWithDecryptReturningParams();
                //return;
                //برای تست روش دوم دو خط زیر را از کامنت در آورید و اسپس برنامه را اجرا نمایید

            }
        }

        private void TranResultInquiryWithDecryptReturningParams() {
            NameValueCollection nvc = Request.Params;
            if (nvc == null)
            {
                AddToConsole("شما به اشتباه وارد این صفحه شده اید. این صفحه بیانگر نتیجه هیچ تراکنشی نیست");
                return;
            }

            string returningParamsString = nvc["ReturningParams"];
            string decryptedReturningParamsString = string.Empty;
            AES2 aesProvider = new AES2(System.Configuration.ConfigurationManager.AppSettings["Key"],
                                        System.Configuration.ConfigurationManager.AppSettings["Iv"]);
            bool decryptionIsSuccessful = aesProvider.Decrypt(returningParamsString, out decryptedReturningParamsString);

            if (!decryptionIsSuccessful)
            {
                AddToConsole("تراکنشی یافت نشد");
                AddToConsole("چنانچه وجهی از حساب شما کسر شده باشد، حداکثر ظرف 72 ساعت به حساب شما بازخواهد گشت");
                return;
            }

            AsanPardakhtPGResultDescriptor trxResultDescriptor = AsanPardakhtPGResultDescriptor.AsanPardakhtTrxResultDescriptorFromString(decryptedReturningParamsString);
            if (trxResultDescriptor == null)
            {
                AddToConsole("تراکنشی یافت نشد");
                AddToConsole("چنانچه وجهی از حساب شما کسر شده باشد، حداکثر ظرف 72 ساعت به حساب شما بازخواهد گشت");
                return;
            }

            int iPreInvoiceID;
            if (!int.TryParse(trxResultDescriptor.PreInvoiceID, out iPreInvoiceID) || iPreInvoiceID < 1)
            {
                AddToConsole("تراکنشی یافت نشد");
                AddToConsole("چنانچه وجهی از حساب شما کسر شده باشد، حداکثر ظرف 72 ساعت به حساب شما بازخواهد گشت");
                return;
            }

            // در این نقطه لازم است شماره پیش فاکتور خود را با نتیجه دریافت شده انطباق دهید
            // آیا چنین شناسه فاکتوری در سیستم شما وجود دارد؟
            // اگر ندارد...                
            //{
            //    AddToConsole("تراکنشی یافت نشد");
            //    AddToConsole("چنانچه وجهی از حساب شما کسر شده باشد، حداکثر ظرف 72 ساعت به حساب شما بازخواهد گشت");
            //    return;
            //}


            // حال لازم است ببینید آیا قبلا نتیجه تراکنش جاری که متناظر با شنایه فاکتوری در سیستم شماست قبلا به شما اعلام شده است
            // اگر قبلا نتیجه این تراکنش را دریافت کرده اید به معنای آن است که باید از درج مجدد آن جلوگیری کنید
            // و احتمالا این اتفاق بدان خاطر رخ داده که کاربر شما صفحه را ریفرش کرده است
            // در چنین شرایطی معمولا بهتر است چنین پیغامی را به کاربر نشان دهید
            //{
            //    AddToConsole("شما قبلا برای این تراکنش وارد صفحه جاری شده اید");
            //    AddToConsole("امکان ورود مجدد به صفحه وجود ندارد");
            //    return;
            //}

            AddToConsole("نتیجه گشودن پاسخ پرداخت از سوی آسان پرداخت به این صفحه به صورت زیر است");
            AddToConsole("iPreInvoiceID: " + iPreInvoiceID);
            AddToConsole("Amount:" + trxResultDescriptor.Amount.ToString());
            AddToConsole("Token: " + trxResultDescriptor.Token);
            AddToConsole("ResCode: " + trxResultDescriptor.ResCode);
            AddToConsole("PayGateTranID: " + trxResultDescriptor.PayGateTranID);
            AddToConsole("MessageText: " + trxResultDescriptor.MessageText);
            AddToConsole("LastFourDigitOfPAN: " + trxResultDescriptor.LastFourDigitOfPAN);
            AddToConsole("RRN: " + trxResultDescriptor.RRN);
            AddToConsole("");
            AddToConsole("");

            // حال لازم است نتیجه تراکنش را در سامانه خود ذخیره کنید. چنانچه مشکلی در ذخیره سازی نتیجه پرداخت وجود داشت لازم است اقدام بیشتر را متوقف کنید
            // و به کاربر اعلام کنید که وجه به حسابش باز خواهد گشت                
            //{
            //    AddToConsole("به دلیل بروز مشکلی امکان ثبت پرداخت شما وجود ندارد");
            //    AddToConsole("چنانچه وجهی از حساب شما کسر شده باشد، حداکثر ظرف 72 ساعت به حساب شما بازخواهد گشت");
            //    return;
            //}

            // آیا کاربر از انجام تراکنش منصرف شده و در صفحه پرداخت دکمه انصراف را زده است؟
            if (trxResultDescriptor.ResCode == "911")
            {
                AddToConsole("شما از انجام تراکنش منصرف شدید");
                AddToConsole("در صورت تمایل می توانید دوباره خرید خود را انجام دهید");
                return;
            }

            // حال لازم است چک نهایی روی شماره پیگیری پرداخت را انجام دهید
            ulong refNumb;
            if (!ulong.TryParse(trxResultDescriptor.PayGateTranID, out refNumb))
            {
                AddToConsole("به دلیل بروز مشکلی امکان ثبت پرداخت شما وجود ندارد");
                AddToConsole("چنانچه وجهی از حساب شما کسر شده باشد، حداکثر ظرف 72 ساعت به حساب شما بازخواهد گشت");
            }

            // در حالت عادی شما بعد از دریافت نتیجه قطعی پرداخت موفق، فرآیندهای تسویه یا عودت وجه را بصورت خودکار انجام می دهید
            // اما در این پروژه آموزشی شماره پیگیری تراکنش ذخیره می شود تا با فشار دادن دکمه های متناظر فرآیند های تکمیلی را بصورت دستی انجام دهید
            ViewState["PayGateTranID"] = refNumb;
        }

        private void TranResultInquiryWithLocalInvoiceId()
        {

            var nvc0 = Request.QueryString;
            if (nvc0 == null)
            {
                AddToConsole("شما به اشتباه وارد این صفحه شده اید. این صفحه بیانگر نتیجه هیچ تراکنشی نیست");
                return;
            }
            string localInvoiceId = nvc0["invoiceid"];
            if (string.IsNullOrEmpty(localInvoiceId))
            {
                AddToConsole("شماره فاکتور محلی ارسالی  همراه آدرس بازگشت  یافت نشد،لطفا مرحله ساخت تراکنش را بررسی فرمایید .");
                return;
            }

            var _ipgService = new IPGResetService();
            //در این قسمت نتیجه تراکنش از درگاه استعلام می گردد
            var paymentResult = _ipgService.TranResult(
               Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["MerchantConfigurationId"]),
               Convert.ToInt64(localInvoiceId)).Result;

            ////در صورتی که فیلد زیر مقدار صفر داشته باشد پرداخت با موفقیت انجام شده است
            if (paymentResult.ResCode == 0)
            {

                // در حالت عادی شما بعد از دریافت نتیجه قطعی پرداخت موفق، فرآیندهای تسویه یا عودت وجه را بصورت خودکار انجام می دهید
                // اما در این پروژه آموزشی شماره پیگیری تراکنش ذخیره می شود تا با فشار دادن دکمه های متناظر فرآیند های تکمیلی را بصورت دستی انجام دهید
                ViewState["PayGateTranID"] = paymentResult.PayGateTranID;

                AddToConsole("پرداخت موفق");
                AddToConsole("نتیجه استعلام تراکنش با شماره فاکتور محلی ذخیره شده در آدرس بازگشت به صورت زیر است");
                AddToConsole("iPreInvoiceID: " + localInvoiceId);
                AddToConsole("Amount:" + paymentResult.Amount.ToString());
                AddToConsole("ResCode: " + paymentResult.ResCode);
                AddToConsole("Token: " + paymentResult.RefId);
                AddToConsole("PayGateTranID: " + paymentResult.PayGateTranID);
                AddToConsole("MessageText: " + paymentResult.ResMessage);
                AddToConsole("LastFourDigitOfPAN: " + paymentResult.CardNumber);
                AddToConsole("RRN: " + paymentResult.Rrn);
                AddToConsole("");
                AddToConsole("");

            }
            else
            {
                AddToConsole("نتیجه استعلام تراکنش با شماره فاکتور محلی ناموفق می باشد");
            }

            return;
        }
    }
}