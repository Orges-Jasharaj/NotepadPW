using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoProject.DataModels.Dto.Request
{
    public class SetPasswordDto
    {
        public string Url { get; set; }
        public string? Password { get; set; }
        public bool ProtectForMe { get; set; }
    }
}
