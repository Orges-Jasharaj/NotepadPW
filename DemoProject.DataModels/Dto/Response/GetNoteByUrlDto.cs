using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoProject.DataModels.Dto.Response
{
    public class GetNoteByUrlDto
    {
        public string? Content { get; set; }
        public string Url { get; set; }
        public bool IsSecure { get; set; }
    }
}
