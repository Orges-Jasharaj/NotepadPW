using DemoProject.API.Services.Interface;
using HTMLQuestPDF.Extensions;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace DemoProject.API.Services.Implementation
{
    public class PdfService : IPdfService
    {
        private static string Content { get; set; }
        private static string Url { get; set; }

        public byte[] GeneratePdf(string content, string url)
        {
            Content = content;
            Url = url;
            var document = Document.Create(c =>
            c.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.MarginLeft(1.5f, Unit.Centimetre);
                page.MarginTop(1f, Unit.Centimetre);
                page.MarginRight(1f, Unit.Centimetre);

                page.Content().Element(ComposeContent);
                page.Footer()
                   .Element(ComposeFooter);
            }));


            var pdf = document.GeneratePdf();

            return pdf;

            //File.WriteAllBytes("C:\\Users\\merov\\Desktop\\test.pdf",pdf);
        }


        public static void ComposeContent(IContainer container)
        {
            // Wrap the whole table in a single container for border/padding
            container
                .Padding(5)
                .Border(0)
                .Element(inner => // use a single child container
                {
                    inner.Table(table =>
                    {
                        // Define 10 relative columns
                        table.ColumnsDefinition(columns =>
                        {
                            for (int i = 0; i < 10; i++)
                                columns.RelativeColumn();
                        });

                        // Add the spanned header cell
                        table.Cell()
                            .Row(1)
                            .RowSpan(3)
                            .Column(0)
                            .ColumnSpan(10)
                            .Element(cell =>
                            {
                                cell
                                    .AlignCenter()   // horizontal center
                                    .AlignMiddle()   // vertical center
                                    .Text("NotePadPw")
                                    .FontSize(32)
                                    .Bold();
                            });

                        // Additional row example
                        table.Cell()
                            .Row(4)
                            .Column(0)
                            .ColumnSpan(10)
                            .HTML(h => { h.SetHtml(Content); });
                    });
                });
        }




        public static void ComposeFooter(IContainer container)
        {
        }
    }
}
