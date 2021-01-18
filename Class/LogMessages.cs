using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XeroAuth2API
{
    public enum XeroEventStatus
    {
        Login,
        Success,
        Refreshed,
        Failed,
        Timeout,
        Log,
    }
    public class LogMessage
    {
        public string MessageText { get; set; }
        public XeroEventStatus Status { get; set; }
    }

}
