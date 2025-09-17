<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="startpayment.aspx.cs" Inherits="PGTesterApp.startpayment" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Start Payment :: Asan Pardakht</title>
    <style>
        body * {
            direction: rtl;
            text-align: right;
            font-family: Tahoma;
            box-sizing: border-box;
            padding: 5px 10px;
            padding-bottom: 8px;
            margin-bottom: 10px;
            resize: none;
        }

        body {
            direction: rtl;
            text-align: right;
        }

        fieldset {
            height: 550px;
        }

        .form-control {
            width: 49%;
        }

        .md-5 {
            width: 40%;
            float: right;
            display: inline-block
        }
    </style>
</head>
<body>
    <h3 style="color: red">لطفا قبل از تست روی این صفحه پارامترها را مطابق پیکربندی دریافت کرده و در فایل کانفیگ تنظیم بفرمایید</h3>
    <asp:Label ID="lblIPAddress" Style="padding-left: 0; color: green" runat="server"></asp:Label>

    <form id="form1" runat="server">
        <fieldset class="md-5">
            <legend>تست فرآیند خرید</legend>

            <br />
            <br />
            <asp:TextBox ID="txbxAmount" placeholder="مبلغ به ریال" CssClass="form-control" runat="server"></asp:TextBox>
            <br />
            <asp:TextBox ID="txbxPreInvoiceID" placeholder="شماره غیر تکراری پیش فاکتور" CssClass="form-control" runat="server"></asp:TextBox>
            <br />
            <asp:TextBox ID="txbxMobileNumber" CssClass="form-control" placeholder="شماره موبایل ارسالی به صفحه ی پرداخت" runat="server"></asp:TextBox>
            <br />
            <asp:Button ID="btnPay" OnClick="btnPay_Click" runat="server" Text="پرداخت" />
            <br />
            <asp:TextBox ID="txbxConsole" TextMode="MultiLine" Rows="10" ReadOnly="true" CssClass="form-control"
                placeholder="خروجی" runat="server"></asp:TextBox>
        </fieldset>

        <fieldset class="md-5">
            <legend>تست فرآیند پرداخت قبض</legend>
            <asp:Label ID="Label1" Style="padding-left: 0;" CssClass="form-control" runat="server"></asp:Label>
            <br />
            <br />
            <asp:TextBox ID="txtbillId" placeholder="شناسه قبض" CssClass="form-control" runat="server"></asp:TextBox>
            <br />
            <asp:TextBox ID="txtpayId" placeholder="شناسه پرداخت" CssClass="form-control" runat="server"></asp:TextBox>
            <br />
            <asp:TextBox ID="txtbillAmount" placeholder="مبلغ قبض به ریال" CssClass="form-control" runat="server"></asp:TextBox>
            <br />
            <asp:TextBox ID="txtbillInvoiceID" placeholder="شماره غیر تکراری پیش فاکتور" CssClass="form-control" runat="server"></asp:TextBox>
            <br />
            <asp:TextBox ID="txbxbillMobileNumber" placeholder="شماره موبایل ارسالی به صفحه ی پرداخت" CssClass="form-control" runat="server"></asp:TextBox>
            <br />
            <asp:Button ID="btnbillPay" OnClick="btnbillPay_Click" runat="server" Text="پرداخت قبض" />
            <br />
            <asp:TextBox ID="txbxbillConsole" TextMode="MultiLine" Rows="10" CssClass="form-control" ReadOnly="true" placeholder="خروجی" runat="server"></asp:TextBox>
        </fieldset>

        <fieldset class="md-5">
            <legend>تست فرآیند خرید شارژ</legend>
            <asp:Label ID="lblCharge" Style="padding-left: 0;" CssClass="form-control" runat="server"></asp:Label>
            <br />
            <br />
            <asp:TextBox ID="txtChargeDestinationMobile" placeholder="شماره موبایل" CssClass="form-control" runat="server"></asp:TextBox>
            <br />
            <asp:TextBox ID="txtChargeProductId" placeholder="شناسه محصول" CssClass="form-control" runat="server"></asp:TextBox>
            <br />
            <asp:TextBox ID="txtChargeAmount" placeholder="مبلغ به ریال" CssClass="form-control" runat="server"></asp:TextBox>
            <br />
            <asp:TextBox ID="txtChargeInvoiceID" placeholder="شماره غیر تکراری پیش فاکتور" CssClass="form-control" runat="server"></asp:TextBox>
            <br />
            <asp:TextBox ID="txtChargeMobileNumber" placeholder="شماره موبایل ارسالی به صفحه ی پرداخت" CssClass="form-control" runat="server"></asp:TextBox>
            <br />
            <asp:Button ID="btnChargePay" OnClick="btnChargePay_Click" runat="server" Text="خرید شارژ" />
            <br />
            <asp:TextBox ID="txtChargeConsole" TextMode="MultiLine" Rows="10" CssClass="form-control" ReadOnly="true" placeholder="خروجی" runat="server"></asp:TextBox>
        </fieldset>


    </form>
    <fieldset class="md-5">
        <legend>تست فرآیند استعلام قوه</legend>

        <form action="https://localhost:44329/Jud" method="post">
            <input type="text" name="Orderid" id="Orderid" placeholder="شناسه قوه" />
            <br />
            <input type="text" name="Backurl" id="Backurl" placeholder="آدرس بازگشت" value="http://localhost:1402/receivepayment.aspx" />
            <br />
            <button type="submit">پرداخت</button>
        </form>
    </fieldset>
    <script>
        document.getElementById('txbxPrice').value = 1000;
        document.getElementById('txbxInvoiceID').value = (new Date()).getTime() % 100000000;
    </script>
</body>
</html>
