<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="OneNorth.FieldMigrator.sitecore.admin.FieldMigrator.Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Field Migrator</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <h1>Field Migrator</h1>
        
        <h3>Run</h3>
        <asp:Button runat="server" ID="MigrateButton" Text="Run Field Migrator" OnClick="MigrateButton_OnClick"/><br/>

    </div>
    </form>
</body>
</html>
