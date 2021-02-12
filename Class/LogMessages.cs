using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XeroAuth2API
{
    /// <summary>
    /// Event status codes
    /// </summary>
    public enum XeroEventStatus
    {
        /// <summary>Begin Login</summary>
        Login,
        /// <summary>Login was sucessfull</summary>
        Success,
        /// <summary>Token was Refreshed</summary>
        Refreshed,
        /// <summary>Failed to perform action (auth or refresh)</summary>
        Failed,
        /// <summary>Timeout waiting for Auth</summary>
        Timeout,
        /// <summary>Log Message</summary>
        Log,
    }
    /// <summary>
    /// Log event
    /// </summary>
    public class LogMessage
    {
        /// <summary>
        /// Message / Event
        /// </summary>
        public string MessageText { get; set; }
        /// <summary>
        /// The event status
        /// </summary>
        public XeroEventStatus Status { get; set; }
    }

}
