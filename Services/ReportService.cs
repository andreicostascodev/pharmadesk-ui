using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using PharmaDesk.Data;
using PharmaDesk.Models;
using System.IO;
namespace PharmaDesk.Services;
public class ReportService(PharmaDeskDbContext db, IConfiguration config) : IReportService
{
    private readonly string invoiceFolder = Path.GetFullPath(config["App:InvoiceFolder"] ?? "Invoices");
    public async Task<string> GenerateInvoiceAsync(Order order)
    {
        Directory.CreateDirectory(invoiceFolder);
        var loaded = await db.Orders.Include(x => x.User).Include(x => x.Items).ThenInclude(x => x.Medicine).FirstAsync(x => x.Id == order.Id);
        var path = Path.Combine(invoiceFolder, $"Factura-{loaded.OrderNumber}.pdf");
        using var writer = new PdfWriter(path);
        using var pdf = new PdfDocument(writer);
        using var doc = new Document(pdf);
        doc.Add(new Paragraph("PharmaDesk - Factura").SetFontSize(20));
        doc.Add(new Paragraph($"Comanda: {loaded.OrderNumber}"));
        doc.Add(new Paragraph($"Client: {loaded.User?.FullName} / {loaded.User?.Email}"));
        doc.Add(new Paragraph($"Data: {loaded.OrderDate:dd.MM.yyyy HH:mm}"));
        var table = new Table(4).UseAllAvailableWidth();
        table.AddHeaderCell("Produs"); table.AddHeaderCell("Cantitate"); table.AddHeaderCell("Pret"); table.AddHeaderCell("Total");
        foreach (var item in loaded.Items)
        {
            table.AddCell(item.Medicine?.Name ?? "Produs");
            table.AddCell(item.Quantity.ToString());
            table.AddCell($"{item.UnitPrice:0.00} RON");
            table.AddCell($"{item.TotalPrice:0.00} RON");
        }
        doc.Add(table);
        doc.Add(new Paragraph($"Total produse: {loaded.TotalAmount:0.00} RON"));
        doc.Add(new Paragraph($"Discount: {loaded.Discount:0.00} RON"));
        doc.Add(new Paragraph($"TVA: {loaded.Tax:0.00} RON"));
        doc.Add(new Paragraph($"Total plata: {loaded.GrandTotal:0.00} RON").SetFontSize(16));
        loaded.InvoicePath = path;
        await db.SaveChangesAsync();
        return path;
    }
    public async Task<string> ExportSalesExcelAsync(DateTime from, DateTime to)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        var path = Path.GetFullPath($"Reports/Sales-{from:yyyyMMdd}-{to:yyyyMMdd}.xlsx");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var orders = await OrdersInRange(from, to).ToListAsync();
        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("Vanzari");
        ws.Cells[1, 1].Value = "Numar"; ws.Cells[1, 2].Value = "Client"; ws.Cells[1, 3].Value = "Data"; ws.Cells[1, 4].Value = "Status"; ws.Cells[1, 5].Value = "Total";
        for (var i = 0; i < orders.Count; i++)
        {
            var r = i + 2;
            ws.Cells[r, 1].Value = orders[i].OrderNumber;
            ws.Cells[r, 2].Value = orders[i].User?.FullName;
            ws.Cells[r, 3].Value = orders[i].OrderDate.ToString("dd.MM.yyyy");
            ws.Cells[r, 4].Value = orders[i].Status;
            ws.Cells[r, 5].Value = (double)orders[i].GrandTotal;
        }
        ws.Cells.AutoFitColumns();
        await package.SaveAsAsync(new FileInfo(path));
        return path;
    }
    public async Task<string> ExportSalesPdfAsync(DateTime from, DateTime to)
    {
        var path = Path.GetFullPath($"Reports/Sales-{from:yyyyMMdd}-{to:yyyyMMdd}.pdf");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var orders = await OrdersInRange(from, to).ToListAsync();
        using var writer = new PdfWriter(path);
        using var pdf = new PdfDocument(writer);
        using var doc = new Document(pdf);
        doc.Add(new Paragraph($"Raport vanzari {from:dd.MM.yyyy} - {to:dd.MM.yyyy}").SetFontSize(18));
        var table = new Table(5).UseAllAvailableWidth();
        table.AddHeaderCell("Numar"); table.AddHeaderCell("Client"); table.AddHeaderCell("Data"); table.AddHeaderCell("Status"); table.AddHeaderCell("Total");
        foreach (var order in orders)
        {
            table.AddCell(order.OrderNumber);
            table.AddCell(order.User?.FullName ?? "-");
            table.AddCell(order.OrderDate.ToString("dd.MM.yyyy"));
            table.AddCell(order.Status);
            table.AddCell($"{order.GrandTotal:0.00} RON");
        }
        doc.Add(table);
        doc.Add(new Paragraph($"Total general: {orders.Sum(x => x.GrandTotal):0.00} RON"));
        return path;
    }
    private IQueryable<Order> OrdersInRange(DateTime from, DateTime to) => db.Orders.Include(x => x.User).AsNoTracking().Where(x => x.OrderDate >= from && x.OrderDate <= to.Date.AddDays(1).AddTicks(-1)).OrderByDescending(x => x.OrderDate);
}
