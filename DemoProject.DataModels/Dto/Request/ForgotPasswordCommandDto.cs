using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoProject.DataModels.Dto.Request
{
    public class ForgotPasswordCommandDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }   
    }
}
