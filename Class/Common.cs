using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XeroAuth2API
{
    /// <summary>
    /// Some Common methods
    /// </summary>
    internal static class Common
    {
        // Retrieve the Assemblies physical file location so the default path to store/load files from is usable.
        public static string ApplicationPath = System.IO.Directory.GetParent(System.Reflection.Assembly.GetEntryAssembly().Location).FullName;
        // Handle a list of enums
        internal static string BuildFilterString<T>(string fieldName, List<T> objectList)
        {
            List<string> itemList = objectList.ConvertAll(f => f.ToString());
            string filter = string.Empty;
           
            if (itemList != null)
            {
                foreach (var itm in itemList)
                {
                    if (!string.IsNullOrEmpty(filter)) filter += " || "; //  
                    filter += $"{fieldName}=\"{itm}\"";
                }
                if (itemList.Count > 1)
                {
                    filter = "(" + filter + ")";
                }
            }
            return filter;
        }
        // Single Enum
        internal static string BuildFilterString<T>(string fieldName, T enumItem)
        {
            string filter = string.Empty;
             
            if (enumItem != null)
            {                
                filter += $"{fieldName}=\"{enumItem}\"";
            }
            return filter;
        }
    }
}
