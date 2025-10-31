using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoProject.DataModels.Dto.Response
{
    public class RefreshTokenDto
    {
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime RefreshTokenExipirityDate { get; set; }
    }
}
