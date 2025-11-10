using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoProject.DataModels.Dto.System
{
    public class EmailOptions
    {
        [Required]
        public string SmtpHost { get; set; } = string.Empty;

        [Range(1, 65535)]
        public int SmtpPort { get; set; }

        [Required]
        [EmailAddress]
        public string FromEmail { get; set; } = string.Empty;

        [Required]
        public string FromName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        [Required]
        public bool EnableSsl { get; set; }

    }
}
