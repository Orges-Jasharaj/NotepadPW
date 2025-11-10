namespace DemoProject.API.Services.Interface
{
    public interface IPdfService
    {

        byte[] GeneratePdf(string content, string url);

    }
}
