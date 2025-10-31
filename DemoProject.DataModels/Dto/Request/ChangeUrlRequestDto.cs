using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoProject.DataModels.Dto.Request
{
    public class ChangeUrlRequestDto
    {
        public string OldUrl { get; set; }
        public string NewUrl { get; set; }  
    }
}
