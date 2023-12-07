using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENMService.Models.Request
{
    public partial class RequestENMS
    {
        public int IdConf { get; set; }
        public string? ConnReadConf { get; set; }
        public string? ConfFrom { get; set; }
        public string? ToConf { get; set; }
        public string? SmtpConf { get; set; }
        public long PortConf { get; set; }
        public string? UsernameConf { get; set; }
        public string? PasswordConf { get; set; }
        public int ReadIntervalConf { get; set; }
        public int ResumeIntervalResume { get; set; }
        public DateTime CreationTimeConf { get; set; }
        public DateTime UpdateTimeConf { get; set; }
        public string? ConnInsertConf { get; set; }
        public string? ConnResumeConf { get; set; }
        public int? ReadIntervalResume { get; set; }
    }
}
