using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XeroAuth2API.Api
{
    /// <summary>
    /// Collection of wrapper functions to interact with the BankFeeds API endpoints
    /// </summary>
    public class BankFeeds
    {
        Xero.NetStandard.OAuth2.Api.BankFeedsApi APIClient = new Xero.NetStandard.OAuth2.Api.BankFeedsApi();
        internal API APICore { get; set; }
 
    }
}
