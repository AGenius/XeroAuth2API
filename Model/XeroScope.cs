
namespace XeroAuth2API.Model
{
    /// <summary>
    /// The Available scopes for API Access
    /// </summary>
    public enum XeroScope
    {
        // Main
        /// <summary>Include all non read only scopes</summary>
        all,
        /// <summary>Include all read only scopes</summary>
        all_read,
        /// <summary>Enable the offline access - Always present</summary>
        offline_access,
        /// <summary>Your application intends to use the user's identity - Always present</summary>
        openid,
        /// <summary>Will return First name, Last name and xero user id - Always present</summary>
        profile,
        /// <summary>Returns the users email address</summary>
        email,

        // Files
        /// <summary>View and manage the file library</summary>
        files,
        /// <summary>View the file library - Read Only</summary>
        files_read,

        // Accounting API
        /// <summary>Include all accounting non read only scopes</summary>
        accounting_all,
        /// <summary>Include all accounting read only scopes </summary>
        accounting_all_read,
        /// <summary>View and manage business transactions</summary>
        accounting_transactions,
        /// <summary>View business transactions</summary>
        accounting_transactions_read,
        /// <summary>View reports</summary>
        accounting_reports_read,
        // accounting_reports_tenninetynine_read, //View your 1099 reports --- !!!!! US ONLY !!!!!
        /// <summary>View the general ledger</summary>
        accounting_journals_read,
        /// <summary>View and manage organisation settings</summary>
        accounting_settings,
        /// <summary>View organisation settings</summary>
        accounting_settings_read,
        /// <summary>View and manage contacts</summary>
        accounting_contacts,
        /// <summary>View contacts</summary>
        accounting_contacts_read,
        /// <summary>View and manage attachments</summary>
        accounting_attachments,
        /// <summary>View attachments</summary>
        accounting_attachments_read,

        // Assets API 
        /// <summary>View and manage fixed assets</summary>
        assets,
        /// <summary>View fixed assets</summary>
        assets_read,

        // Projects API
        /// <summary>View and manage projects</summary>
        projects,
        /// <summary>View projects</summary>
        projects_read,

    }
}
