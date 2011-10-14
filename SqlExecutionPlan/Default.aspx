<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="SqlExecutionPlan.Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>SQL Execution Plan</title>

    <script type="text/javascript" src="http://www.google.com/jsapi"></script>
    <script type="text/javascript">
        google.load('visualization', '1', { packages: ['orgchart'] });
    </script>
</head>
<body>
    <form id="sqlForm" runat="server">
    <div>
        <fieldset>
            <asp:Label ID="lblError" runat="server" ForeColor="Red" Visible="False"></asp:Label>
            <legend>Sql</legend>
            <asp:TextBox ID="txtSql" runat="server" Width="100%" Height="100px" TextMode="MultiLine"></asp:TextBox>
            <asp:Button ID="btnExecute" runat="server" Text="Execute" OnClick="btnExecute_Click" />
        </fieldset>
        <fieldset id="fldLegend" runat="server" visible="false">
            <legend>Execution Plan</legend>
            <div id="chartDiv"></div>
        </fieldset>
    </div>
    </form>
</body>
</html>
