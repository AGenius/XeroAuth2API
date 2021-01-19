using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XeroAuth2API
{
    public enum XeroScope
    {
        // Main
        all, // Include all non read only scopes
        all_read, // Include all read only scopes
        offline_access, // always present
        openid, // your application intends to use the user's identity - always present
        profile, // first name, last name and xero user id - always present
        email, // email address
        
        // Files
        files, // View and manage the file library
        files_read, // View the file library

        // Accounting API
        accounting_all, // Include all accounting non read only scopes
        accounting_all_read, // Include all accounting read only scopes 
        accounting_transactions, // View and manage business transactions
        accounting_transactions_read, // View business transactions
        accounting_reports_read, // View reports
        accounting_reports_tenninetynine_read, //View your 1099 reports
        accounting_journals_read, // View the general ledger
        accounting_settings, // View and manage organisation settings
        accounting_settings_read, // View organisation settings
        accounting_contacts, // View and manage contacts
        accounting_contacts_read, // View contacts
        accounting_attachments, // View and manage attachments
        accounting_attachments_read, // View attachments
        
        // Assets API 
        assets, // View and manage fixed assets
        assets_read, // View fixed assets

        // Projects API
        projects, // View and manage projects
        projects_read, // View projects

    }
}
