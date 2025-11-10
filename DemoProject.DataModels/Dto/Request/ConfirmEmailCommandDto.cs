using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoProject.DataModels.Dto.Request
{
    public class ConfirmEmailCommandDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;
        [Required]
        public string Code { get; set; } = string.Empty;
    }
}
