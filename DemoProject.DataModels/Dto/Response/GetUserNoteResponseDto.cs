using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoProject.DataModels.Dto.Response
{

    public class GetUserNoteResponseDto
    {
        public List<UserNoteDto> UserNoteDtos { get; set; } = new();
    }
    public class UserNoteDto
    {
        public string Url { get; set; }
        public string ShortDescription { get; set; }
    }

}
