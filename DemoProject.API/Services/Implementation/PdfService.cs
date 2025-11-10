using DemoProject.API.Services.Interface;
using HTMLQuestPDF.Extensions;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace DemoProject.API.Services.Implementation
{
    public class PdfService : IPdfService
    {
		private static string? Content { get; set; }
		private static string? Url { get; set; }

        public byte[] GeneratePdf(string content, string url)
        {
            Content = content;
            Url = "http://padzy.runasp.net/"+ url;
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
			// Page header: centered title "Padzy" with a right-aligned link
			container
				.Padding(10)
				.Border(0)
				.Element(root =>
				{
					root.Column(col =>
					{
						// Header row
						col.Item().Table(table =>
						{
							table.ColumnsDefinition(columns =>
							{
								columns.RelativeColumn();   // left spacer
								columns.RelativeColumn(2);  // center title
								columns.RelativeColumn();   // right link
							});

							// Left spacer (empty)
							table.Cell().Column(1).AlignLeft().Text("");

							// Centered Title
							table.Cell().Column(2).AlignCenter().Element(cell =>
							{
								cell.Text("Padzy")
									.FontSize(28)
									.SemiBold()
									.FontColor(Colors.Black);
							});

							// Right-aligned link
							table.Cell().Column(3).AlignRight().Element(cell =>
							{
								var linkText = string.IsNullOrWhiteSpace(Url) ? "Open" : "Open Link";
								cell.Hyperlink(Url ?? string.Empty).Text(linkText)
									.FontSize(10)
									.Underline()
									.FontColor(Colors.Blue.Medium);
							});
						});

						// Small separator space
						col.Item().PaddingTop(5);

						// Content area: center the HTML content and limit width for readability
						col.Item().Element(contentArea =>
						{
							contentArea
								.AlignCenter()
								.MaxWidth(450)
								.PaddingTop(10)
								.HTML(h => { h.SetHtml(Content ?? string.Empty); });
						});
					});
				});
		}




        public static void ComposeFooter(IContainer container)
        {
        }
    }
}
