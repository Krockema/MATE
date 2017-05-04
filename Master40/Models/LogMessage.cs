using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Master40.Models
{
    public enum MessageType
    {
        success,
        info,
        warning,
        danger
    }
    public class LogMessage
    {
        [Key]
        public int MessageNumber { get; set; }
        public string Message { get; set; }
        public MessageType MessageType { get; set; }
    }
}
