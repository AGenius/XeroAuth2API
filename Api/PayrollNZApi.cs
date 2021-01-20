using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XeroAuth2API.Api
{
    /// <summary>
    /// Collection of wrapper functions to interact with the PayrollNZ API endpoints
    /// </summary>
    public class PayrollNZ
    {
        Xero.NetStandard.OAuth2.Api.ProjectApi APIClient = new Xero.NetStandard.OAuth2.Api.ProjectApi();
        internal API APICore { get; set; }
       
    }
}
