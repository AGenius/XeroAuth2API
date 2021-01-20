using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace XeroAuth2API.Api
{
    /// <summary>
    /// Collection of wrapper functions to interact with the Project API endpoints
    /// </summary>
    public class ProjectApi
    {
        Xero.NetStandard.OAuth2.Api.ProjectApi APIClient = new Xero.NetStandard.OAuth2.Api.ProjectApi();
        internal API APICore { get; set; }
        /// <summary>
        /// Return a list or Projects
        /// </summary>
        /// <param name="onlypage">Up to 100 records will be returned in a single API call with line items details (optional)</param>
        /// <param name="projectIds">Search for all projects that match a comma separated list of projectIds (optional)</param>
        /// <param name="contactID">Filter for projects for a specific contact (optional)</param>
        /// <param name="states">Filter for projects in a particular state (INPROGRESS or CLOSED) (optional)</param>
        /// <param name="pageSize">Optional, it is set to 50 by default. The number of items to return per page in a paged response - Must be a number between 1 and 500. (optional, default to 50)</param>
        /// <returns>List of Project records</returns>
        public List<Xero.NetStandard.OAuth2.Model.Project.Project> Projects(int? onlypage = null, List<Guid> projectIds = null, Guid? contactID = null, string states = null, int pageSize = 50)
        {
            int? page = 1;
            if (onlypage.HasValue)
            {
                page = onlypage.Value;
            }
            try
            {
                var records = new List<Xero.NetStandard.OAuth2.Model.Project.Project>(); // Hold the records
                int count = pageSize; // This is how many per page - setting this will ensure we check for the first page is a full 100 and loop until all returned  
                while (count == pageSize)
                {
                    if (page == -1) page = null; // This allows a quick first page of records
                    var task = Task.Run(() => APIClient.GetProjectsAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID, projectIds, contactID, states, page, pageSize));
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
        /// <summary>
        /// Return a single Project
        /// </summary>
        /// <param name="projectID">Unique identifier for the record</param>
        /// <returns>Project Record</returns>
        public Xero.NetStandard.OAuth2.Model.Project.Project Project(Guid projectID)
        {
            if (projectID == null)
            {
                throw new ArgumentNullException("Missing Project ID");
            }
            try
            {
                var task = Task.Run(() => APIClient.GetProjectAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID, projectID));
                task.Wait();
                if (task.Result != null)
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
    }
}
