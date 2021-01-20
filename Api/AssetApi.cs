using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XeroAuth2API.Model;
using Xero.NetStandard.OAuth2.Model.Asset;

namespace XeroAuth2API.Api
{
    /// <summary>
    /// Collection of wrapper functions to interact with the Asset API endpoints
    /// </summary>
    public class AssetApi
    {
        Xero.NetStandard.OAuth2.Api.AssetApi APIClient = new Xero.NetStandard.OAuth2.Api.AssetApi();
        internal API APICore { get; set; }

        #region Assets
        /// <summary>
        /// Return a list of fixed assets
        /// </summary>
        /// <param name="status">Required when retrieving a collection of assets. See Asset Status Codes</param>
        /// <param name="onlypage">Up to 100 records will be returned in a single API call with line items details (optional)</param>
        /// <param name="orderBy">Requests can be ordered by AssetType, AssetName, AssetNumber, PurchaseDate and PurchasePrice. If the asset status is DISPOSED it also allows DisposalDate and DisposalPrice. (optional)</param>
        /// <param name="sortDirection">ASC or DESC (optional)</param>
        /// <param name="filterBy">A string that can be used to filter the list to only return assets containing the text. Checks it against the AssetName, AssetNumber, Description and AssetTypeName fields. (optional)</param>
        /// <param name="pageSize">The number of records returned per page. By default the number of records returned is 10. (optional)</param>
        /// <returns>List of Assets</returns>
        public List<Asset> Assets(AssetStatusQueryParam status, int? onlypage = null,
            string orderBy = null, string sortDirection = null, string filterBy = null, int pageSize = 50)
        {
            int? page = 1;
            if (onlypage.HasValue)
            {
                page = onlypage.Value;
            }
            try
            {
                var records = new List<Asset>(); // Hold the records
                int count = pageSize; // This is how many per page - setting this will ensure we check for the first page is a full 100 and loop until all returned  
                while (count == pageSize)
                {
                    if (page == -1) page = null; // This allows a quick first page of records
                    var task = Task.Run(() => APIClient.GetAssetsAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID, status, page, pageSize, orderBy, sortDirection, filterBy));
                    task.Wait();
                    records.AddRange(task.Result.Items); // Add the next page records returned
                    count = task.Result.Items.Count; // Record the number of records returned in this page. if less than 100 then the loop will exit otherwise get the next page full
                    if (page != null) page++;
                    if (onlypage.HasValue) count = -1;
                }

                if (records.Count > 0)
                {
                    return records;
                }
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
            }

            return null;
        }
        public List<AssetType> AssetTypes()
        {
            try
            {
                var task = Task.Run(() => APIClient.GetAssetTypesAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID));
                task.Wait();
                if (task.Result.Count > 0)
                {
                    return task.Result;
                }
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
            }
            return null;
        }
        #endregion

    }
}
