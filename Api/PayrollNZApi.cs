using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XeroAuth2API.Api
{
    /// <summary>
    /// Collection of wrapper functions to interact with the PayrollNz API endpoints
    /// </summary>
    public class PayrollNzApi : ICoreAPI
    {
        Xero.NetStandard.OAuth2.Api.PayrollNzApi APIClient;
        internal API APICore { get; set; }
        
        /// <summary>
        /// Throw errors for Items not found
        /// </summary>
        public bool? RaiseNotFoundErrors { get; set; }
        /// <summary>
        /// Default 'ctor
        /// </summary>
        public PayrollNzApi()
        {
            APIClient = new Xero.NetStandard.OAuth2.Api.PayrollNzApi();
        }
        /// <summary>
        /// 'ctor - pass Parent API class
        /// </summary>
        /// <param name="parentAPI">ref to the parent API object</param>
        public PayrollNzApi(API parentAPI)
        {
            this.APICore = parentAPI;
            Xero.NetStandard.OAuth2.Client.Configuration confg = new Xero.NetStandard.OAuth2.Client.Configuration();
            confg.UserAgent = "XeroAuth2API-" + APICore.Version;
            APIClient = new Xero.NetStandard.OAuth2.Api.PayrollNzApi(confg);
        }
    }
}
