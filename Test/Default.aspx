<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Test.Default" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
    <script src="Scripts/jquery-1.10.2.min.js"></script>
    <script src="Scripts/jquery.yuuko-0.9.9.2.min.js"></script>
    <script src="yuuko/js"></script>
</head>
<body>
    <div data-detail="User" data-method="edit">
        <p>
            <input data-field="User.ID" type="text" />
            <input data-field="User.Username" type="text" />
            <input data-field="User.Password" type="text" />
            <input data-field="User.nab" type="text" />
            <a href="javascript:EditPort('User')">Save</a>
        </p>
    </div>
    <script>
        Detail.User = "admin";
        var YuukoToken = "<%=Session["YuukoToken"]%>";
    </script>
</body>
</html>
