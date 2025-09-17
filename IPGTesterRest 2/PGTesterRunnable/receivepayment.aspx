<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="receivepayment.aspx.cs" Inherits="PGTesterApp.receivepayment" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Receive Payment :: Asan Pardakht</title>
    <style>
        body * {
            font-family: Tahoma;
            width: 400px;
            box-sizing: border-box;
            padding: 5px 10px;
            padding-bottom: 8px;
            margin-bottom: 10px;
            resize: none;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <asp:TextBox ID="txbxConsole" TextMode="MultiLine" Rows="20" ReadOnly="true" placeholder="خروجی" runat="server"></asp:TextBox>
        <br />
        <asp:Button ID="btnVerify" runat="server" Text="ارسال تاییدیه دریافت یا وریفای" OnClick="btnVerify_Click" />
        <br />
        <asp:Button ID="btnSettle" runat="server" OnClick="btnSettle_Click" Text="ارسال ریکانسیل یا تسویه" />
        <br />
        <asp:Button ID="btnReversal" runat="server" OnClick="btnReversal_Click" Text="ارسال عودت وجه یا ریورسال" />
        <br />
        <div>لطفا قبل از تست روی این صفحه پارامترها را مطابق پیکربندی دریافت کرده تنظیم بفرمایید</div>
    </form>
</body>
</html>
