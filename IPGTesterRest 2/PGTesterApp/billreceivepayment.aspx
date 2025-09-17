<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="billreceivepayment.aspx.cs" Inherits="PGTesterApp.billreceivepayment" %>
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
        <div>لطفا قبل از تست روی این صفحه پارامترها را مطابق پیکربندی دریافت کرده تنظیم بفرمایید</div>
    </form>
</body>
</html>

