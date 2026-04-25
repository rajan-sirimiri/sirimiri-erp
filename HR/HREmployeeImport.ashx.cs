using System;
using System.IO;
using System.Web;
using ClosedXML.Excel;

namespace HRModule
{
    /// <summary>
    /// Template download handler for employee import.
    /// Follows Session 9 rule: .ashx + .ashx.cs pair, never inline ASHX,
    /// because Plesk precompiled deploy strips inline code.
    /// </summary>
    public class HREmployeeImportHandler : IHttpHandler
    {
        public bool IsReusable => false;

        public void ProcessRequest(HttpContext ctx)
        {
            // Role gate
            string role = ctx.Session?["UserRole"] as string;
            if (role != "Super" && role != "Admin")
            {
                ctx.Response.StatusCode = 401;
                ctx.Response.Write("Unauthorized");
                return;
            }

            string action = (ctx.Request.QueryString["action"] ?? "").ToLowerInvariant();
            if (action == "template")
            {
                SendTemplate(ctx);
                return;
            }

            ctx.Response.StatusCode = 400;
            ctx.Response.Write("Unknown action");
        }

        private void SendTemplate(HttpContext ctx)
        {
            string[] headers = {
                "EmployeeCode","FullName","FatherName","Gender","DOB","DOJ",
                "Department","Designation","EmploymentType",
                "Mobile","AltMobile","Email","Address","City","State","Pincode",
                "Aadhaar","PAN","UAN","PFNo","ESINo",
                "BankAcNo","BankName","IFSC",
                "Basic","HRA","Conveyance","Other"
            };

            using (XLWorkbook wb = new XLWorkbook())
            {
                IXLWorksheet ws = wb.AddWorksheet("Employees");
                for (int i = 0; i < headers.Length; i++)
                    ws.Cell(1, i + 1).Value = headers[i];

                IXLRange hdr = ws.Range(1, 1, 1, headers.Length);
                hdr.Style.Font.Bold = true;
                hdr.Style.Fill.BackgroundColor = XLColor.FromHtml("#F1F3F4");

                // One example row
                ws.Cell(2, 1).Value = "";                       // code auto
                ws.Cell(2, 2).Value = "Lakshmi Narayanan";
                ws.Cell(2, 4).Value = "M";
                ws.Cell(2, 5).Value = new DateTime(1990, 5, 12);
                ws.Cell(2, 6).Value = new DateTime(2024, 4, 1);
                ws.Cell(2, 7).Value = "Production";
                ws.Cell(2, 8).Value = "Operator";
                ws.Cell(2, 9).Value = "Permanent";
                ws.Cell(2, 10).Value = "9876543210";
                ws.Cell(2, 25).Value = 12000;  // Basic
                ws.Cell(2, 26).Value = 4800;   // HRA
                ws.Cell(2, 27).Value = 1600;   // Conveyance
                ws.Cell(2, 28).Value = 1600;   // Other

                ws.Columns().AdjustToContents();

                ctx.Response.Clear();
                ctx.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                ctx.Response.AddHeader("Content-Disposition", "attachment; filename=HR_Employee_Import_Template.xlsx");

                using (MemoryStream ms = new MemoryStream())
                {
                    wb.SaveAs(ms);
                    ctx.Response.BinaryWrite(ms.ToArray());
                }
                ctx.Response.End();
            }
        }
    }
}
