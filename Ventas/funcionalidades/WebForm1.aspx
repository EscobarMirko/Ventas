<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WebForm1.aspx.cs" Inherits="Ventas.funcionalidades.WebForm1" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title>Procesar Ventas</title>
    <style type="text/css">
        .table-bordered {
            border: 1px solid #ccc;
            border-collapse: collapse;
        }
        .table-bordered th,
        .table-bordered td {
            border: 1px solid #ccc;
            padding: 8px;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <h1>Procesar los datos</h1>
        </div>

        <!-- Subir archivo Excel -->
        <div>
            <asp:FileUpload ID="FileUpload1" runat="server" />
        </div>

        <!-- Botón para procesar -->
        <div style="margin-top: 10px;">
            <asp:Button ID="btnProcesar" runat="server" Text="Procesar" OnClick="btnProcesar_Click" />
        </div>

        <!-- Gráfico -->
        <div style="margin-top: 20px;">
            <asp:Literal ID="LiteralGrafico" runat="server" />
            <asp:Literal ID="LiteralGraficoVendedores" runat="server" />

        </div>

        <!-- Tabla de datos -->
        <div style="margin-top: 20px;">
            <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="true" CssClass="table table-bordered" Width="920px" />
        </div>
    </form>
</body>
</html>

