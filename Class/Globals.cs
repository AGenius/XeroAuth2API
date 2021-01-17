using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

/// <summary>
///  This is used so the internal listener has somewhere to place the data for the main process to access
/// </summary>
namespace XeroAuth2API
{
    static class Globals
    {
        public static Model.XeroConfiguration config { get; set; } // Hold the First leg of the oAuth2 process
        public static Model.XeroOAuthToken XeroToken { get; set; } // Hold the Final Access and Refresh tokens
    }
 
}
