<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="startpayment.aspx.cs" Inherits="PGTesterApp.startpayment" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Start Payment :: Asan Pardakht</title>
    <style>
        body *
        {
            font-family:Tahoma;
            width:400px;
            box-sizing:border-box;
            padding:5px 10px;
            padding-bottom:8px;
            margin-bottom:10px;
            resize:none;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <asp:Label ID="lblIPAddress" style="padding-left:0;" runat="server"></asp:Label>
        <br /><br />
        <asp:TextBox ID="txbxMerchantID" placeholder="کد پذیرنده" runat="server"></asp:TextBox>
        <br />
        <asp:TextBox ID="txbxMerchantConfigID" placeholder="کد پیکربندی پذیرنده" runat="server"></asp:TextBox>
        <br />
        <asp:TextBox ID="txbxUsername" placeholder="نام کاربری" runat="server"></asp:TextBox>
        <br />
        <asp:TextBox ID="txbxPassword" placeholder="رمز عبور" runat="server"></asp:TextBox>
        <br />
        <asp:TextBox ID="txbxKey" placeholder="کلید" runat="server"></asp:TextBox>
        <br />
        <asp:TextBox ID="txbxIV" placeholder="وکتور رمزنگاری" runat="server"></asp:TextBox>
        <br />
        <asp:TextBox ID="txbxAmount" placeholder="مبلغ به ریال" runat="server"></asp:TextBox>
        <br />
        <asp:TextBox ID="txbxPreInvoiceID" placeholder="شماره غیر تکراری پیش فاکتور" runat="server"></asp:TextBox>
        <br />
        <asp:Button ID="btnPay" OnClick="btnPay_Click" runat="server" Text="پرداخت" />
        <br />
        <asp:TextBox ID="txbxConsole" TextMode="MultiLine" Rows="10" ReadOnly="true" placeholder="خروجی" runat="server"></asp:TextBox>
    </form>
</body>
</html>
