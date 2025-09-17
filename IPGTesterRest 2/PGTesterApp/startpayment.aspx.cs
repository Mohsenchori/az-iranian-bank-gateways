using PGTesterApp.Business;
using PGTesterApp.RestSample;
using PGTesterApp.RestSample.models;
using PGTesterApp.RestSample.models.bill;
using PGTesterApp.RestSample.models.jud;
using PGTesterApp.RestSample.models.sale;
using PGTesterApp.RestSample.models.telecom;
using PGTesterApp.RestSample.Moldels;
using PGTesterApp.RestSample.Moldels.Sale;

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
    public partial class startpayment : System.Web.UI.Page
    {
        //"https://asan.shaparak.ir"
        //"https://localhost:44329/"
        private static readonly string GatewayUrl = "https://localhost:5001/";
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string myHost = System.Net.Dns.GetHostName();
                string myIP = System.Net.Dns.GetHostEntry(myHost).AddressList[1].ToString();
                lblIPAddress.Text = "My Hostname: " + myHost + "<br />My IP: " + myIP + "<br />" +
                    "Server Variable LOCAL_ADDR: " + Request.ServerVariables["LOCAL_ADDR"];
            }
        }

        protected void btnPay_Click(object sender, EventArgs e)
        {
            txbxConsole.Text = string.Empty;

            int localInvoiceID;
            ulong amountInRials;

            if (!int.TryParse(txbxPreInvoiceID.Text.Trim(), out localInvoiceID))
            {
                AddToConsole("شماره فاکتور به درستی وارد نشده است");
                return;
            }

            if (!ulong.TryParse(txbxAmount.Text.Trim(), out amountInRials))
            {
                AddToConsole("مبلغ ریالی به درستی وارد نشده است");
                return;
            }


            var paymentToken = new SaleCommand(Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["MerchantConfigurationId"]),
                                             Convert.ToInt32(ServiceTypeEnum.Sale),
                                             localInvoiceID,
                                             amountInRials,
                                             $"{ConfigurationManager.AppSettings["CallbackEndPoint"] }?invoiceID={localInvoiceID}",
                                             string.Empty
                                            );

            var ipgService = new IPGResetService();
            var result = ipgService.Token<SaleCommand, SaleTokenVm>(paymentToken).Result;

            if (result.ResCode == 0)
            {
                NameValueCollection nvc = new NameValueCollection();
                nvc.Add("RefId", result.RefId);
                nvc.Add("mobileap", txbxMobileNumber.Text);
                RedirectWithPost.PageRedirect(this.Page, GatewayUrl, nvc);
                return;
            }
            else
            {
                AddToConsole("مشکلی در اتصال به درگاه پرداخت وجود دارد");
                AddToConsole(result.ResMessage + string.Format(" ({0})", result.ResCode));
                return;
            }
        }

        private void AddToConsole(string txt)
        {
            txbxConsole.Text += "\n" + txt;
        }

        private void AddToBillConsole(string txt)
        {
            txbxbillConsole.Text += "\n" + txt;
        }
        private void AddToChargeConsole(string txt)
        {
            txtChargeConsole.Text += "\n" + txt;
        }

        protected void btnbillPay_Click(object sender, EventArgs e)
        {
            txbxbillConsole.Text = string.Empty;

            int localInvoiceID;
            ulong amountInRials;
            string billId;
            string payId;



            if (!int.TryParse(txtbillInvoiceID.Text.Trim(), out localInvoiceID))
            {
                AddToBillConsole("شماره فاکتور به درستی وارد نشده است");
                return;
            }

            if (!NumberValidator.IsValid(txtbillId.Text) || txtbillId.Text.Trim().Length > 13)
            {
                AddToBillConsole("شناسه قبض به درستی وارد نشده است");
                return;
            }

            if (!NumberValidator.IsValid(txtpayId.Text) || txtpayId.Text.Trim().Length > 13)
            {
                AddToBillConsole("شناسه پرداخت به درستی وارد نشده است");
                return;
            }


            if (!ulong.TryParse(txtbillAmount.Text.Trim(), out amountInRials))
            {
                AddToBillConsole("مبلغ ریالی به درستی وارد نشده است");
                return;
            }

            billId = txtbillId.Text.Trim();
            payId = txtpayId.Text.Trim();

            //مر حله ساخت تراکنش و دریافت توکن پرداخت از درگاه آسان پرداخت
            var billCommand = new BillCommand(Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["MerchantConfigurationId"]),
                                             Convert.ToInt32(ServiceTypeEnum.Bill),
                                             localInvoiceID,
                                             amountInRials,
                                             //شماره فاکتور محلی را به همراه آدرس بازگشت ارسال می کنیم 
                                             //تا هنگام بازگشت از صفحه پرداخت با استخراج آن بتوانیم نتیجه تراکنش
                                             //را از در گاه اسعلام بگیریم .
                                             $"{ConfigurationManager.AppSettings["BillCallbackEndPoint"]}?invoiceid={localInvoiceID}",
                                             billId,
                                             payId
                                            );

            var ipgService = new IPGResetService();
            var result = ipgService.Token<BillCommand, BillTokenVm>(billCommand).Result;


            if (result.ResCode == 0)
            {
                NameValueCollection nvc = new NameValueCollection();
                nvc.Add("RefId", result.RefId);
                nvc.Add("mobileap", txbxbillMobileNumber.Text);
                RedirectWithPost.PageRedirect(this.Page, GatewayUrl, nvc);
                return;
            }
            else
            {
                AddToBillConsole("مشکلی در اتصال به درگاه پرداخت وجود دارد");
                AddToBillConsole(result.ResMessage);
                return;
            }
        }
        protected void btnChargePay_Click(object sender, EventArgs e)
        {
            txtChargeConsole.Text = string.Empty;

            int localInvoiceID;
            ulong amountInRials;
            string desMobile;
            int productId;



            if (!int.TryParse(txtChargeInvoiceID.Text.Trim(), out localInvoiceID))
            {
                AddToChargeConsole("شماره فاکتور به درستی وارد نشده است");
                return;
            }

            if (!NumberValidator.IsValid(txtChargeDestinationMobile.Text) || txtChargeDestinationMobile.Text.Trim().Length != 11)
            {
                AddToChargeConsole("شماره موبایل شارژ به درستی وارد نشده است");
                return;
            }

            if (!NumberValidator.IsValid(txtChargeProductId.Text))
            {
                AddToChargeConsole("شناسه محصول به درستی وارد نشده است");
                return;
            }


            if (!ulong.TryParse(txtChargeAmount.Text.Trim(), out amountInRials))
            {
                AddToChargeConsole("مبلغ ریالی به درستی وارد نشده است");
                return;
            }

            desMobile = txtChargeDestinationMobile.Text.Trim();
            productId = int.Parse(txtChargeProductId.Text.Trim());

            //مر حله ساخت تراکنش و دریافت توکن پرداخت از درگاه آسان پرداخت
            var command = new TelecomChargeCommand(Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["MerchantConfigurationId"]),
                                             TelecomChargeServiceType.Irancell,
                                             localInvoiceID,
                                             amountInRials,
                                             $"{ConfigurationManager.AppSettings["BillCallbackEndPoint"]}?invoiceid={localInvoiceID}",
                                             desMobile,
                                             productId
                                            );

            var ipgService = new IPGResetService();
            var result = ipgService.Token<TelecomChargeCommand, BillTokenVm>(command).Result;


            if (result.ResCode == 0)
            {
                NameValueCollection nvc = new NameValueCollection();
                nvc.Add("RefId", result.RefId);
                nvc.Add("mobileap", txtChargeMobileNumber.Text);
                RedirectWithPost.PageRedirect(this.Page, GatewayUrl, nvc);
                return;
            }
            else
            {
                AddToChargeConsole("مشکلی در اتصال به درگاه پرداخت وجود دارد");
                AddToChargeConsole(result.ResMessage);
                return;
            }
        }

    }
}