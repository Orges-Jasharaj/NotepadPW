using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoProject.DataModels.Dto.Request
{
    public class ResetPasswordCommandDto
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Code { get; set; }
        [Required]
        public string Password { get; set; }

    }
}
