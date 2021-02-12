using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XeroAuth2API.Api
{
    /// <summary>
    /// API Core Interface 
    /// </summary>
    public interface ICoreAPI
    {
        /// <summary>
        /// Suppress errors in some places where no data is found
        /// </summary>
        bool? RaiseNotFoundErrors { get; set; }
 
    }
}
