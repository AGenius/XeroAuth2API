using System;

namespace XeroAuth2API.Model
{
    /// <summary>
    /// Holds the Tenant information
    /// </summary>
    public class Tenant
    {
        /// <summary>
        /// Internal Xero ID for this tenant
        /// </summary>
        public Guid id { get; set; }
        /// <summary>
        /// Unsure
        /// </summary>
        public Guid? authEventId { get; set; }
        /// <summary>
        /// The Tenant ID
        /// </summary>
        public Guid TenantId { get; set; }
        /// <summary>
        /// The Type of Tenant?
        /// </summary>
        public string TenantType { get; set; }
        /// <summary>
        /// The Name as seen in Xero
        /// </summary>
        public string TenantName { get; set; }
        /// <summary>
        /// Tenant created Date Time
        /// </summary>
        public DateTime CreatedDateUtc { get; set; }
        /// <summary>
        /// Tenant updated Date Time
        /// </summary>
        public DateTime UpdatedDateUtc { get; set; }
    }
}
