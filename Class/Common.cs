using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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

        /// <summary>
        /// Handle a list of enums
        /// </summary>
        /// <typeparam name="T">Entity Type</typeparam>
        /// <param name="fieldName">The FieldName to include in the filter string  e.g.  ContactStatus</param>
        /// <param name="objectList">List of Items (Enums)</param>
        /// <returns>Concatenated string list of results</returns>
        public static string BuildFilterString<T>(string fieldName, List<T> objectList)
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
        /// <summary>
        /// Handle a Single Enum list - Builds a string of the enum names
        /// </summary>
        /// <typeparam name="T">Entity Type (Enum)</typeparam>
        /// <param name="fieldName">The FieldName to include in the filter string  e.g.  ContactStatus</param>
        /// <param name="enumItem">The Enum</param>
        /// <returns>Concatenated string list of results</returns>
        public static string BuildFilterString<T>(string fieldName, T enumItem)
        {
            string filter = string.Empty;

            if (enumItem != null)
            {
                filter += $"{fieldName}=\"{enumItem}\"";
            }
            return filter;
        }

        /// <summary>
        /// Decode and convert a JSON Web Token string to a JSON object string
        /// </summary>
        /// <param name="JWTTokenString">The JWT token to be decoded</param>
        /// <returns>string containing the JSON object</returns>
        public static string JWTtoJSON(string JWTTokenString)
        {
            var jwtHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            string jsonResult = string.Empty;

            //Check if readable token (string is in a JWT format)            
            if (jwtHandler.CanReadToken(JWTTokenString))
            {
                var token = jwtHandler.ReadJwtToken(JWTTokenString);

                //Extract the payload of the JWT                
                string payload = "{";
                foreach (var item in token.Payload)
                {
                    if (item.Value.GetType().Name == "JArray")
                    {
                        payload += '"' + item.Key + "\":" + item.Value + ",";
                    }
                    else
                    {
                        payload += '"' + item.Key + "\":\"" + item.Value + "\",";
                    }
                }
                payload += "}";
                return Newtonsoft.Json.Linq.JToken.Parse(payload).ToString(Newtonsoft.Json.Formatting.Indented);
            }
            return null;
        }
        #region JSON Serialization methods
        public static string SerializeObject<TENTITY>(TENTITY objectRecord)
        {
            string serialVersion = Newtonsoft.Json.JsonConvert.SerializeObject(objectRecord, Newtonsoft.Json.Formatting.Indented, new Newtonsoft.Json.JsonSerializerSettings()
            {
                ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Error
            });
            return serialVersion;
        }
        public static TENTITY DeSerializeObject<TENTITY>(string serializedString)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<TENTITY>(serializedString);
        }
        #endregion

        /// <summary>Read the contents of a text file into a string </summary>
        /// <param name="filepath">File to read</param>
        /// <returns>files contents</returns>
        public static string ReadTextFile(string filepath)
        {
            try
            {
                string test = Path.GetPathRoot(filepath);

                if (String.IsNullOrEmpty(test) || (test.StartsWith(@"\") && !test.StartsWith(@"\\")))
                {

                    // No Full path supplied so start from Application root
                    if (test.StartsWith(@"\"))
                    {
                        filepath = ApplicationPath + filepath;
                    }
                    else
                    {
                        filepath = $"{ApplicationPath}\\{filepath}";
                    }
                }

                if (File.Exists(filepath).Equals(true))
                {
                    using (StreamReader reader = new StreamReader(filepath))
                    {
                        string contents = reader.ReadToEnd();
                        return contents;
                    }
                }
            }
            catch (Exception)
            {
            }

            return null;
        }
        /// <summary>Write the contents of a string to a file </summary>
        /// <param name="filepath">File to write to</param>
        /// <param name="contents">The file contents to write</param>
        /// <returns>Returns the full file path (if filename was only passed the result will include the full path based on the location of the calling application)</returns>
        public static string WriteTextFile(string filepath, string contents)
        {
            try
            {
                string test = Path.GetPathRoot(filepath);

                if (String.IsNullOrEmpty(test) || (test.StartsWith(@"\") && test.Substring(1, 1) != @"\"))
                {

                    // No Full path supplied so start from Application root
                    if (test.StartsWith(@"\"))
                    {
                        filepath = ApplicationPath + filepath;
                    }
                    else
                    {
                        filepath = $"{ApplicationPath}\\{filepath}";
                    }
                }
                using (StreamWriter sw = new StreamWriter(filepath))
                {
                    sw.WriteLine(contents);
                }
                return filepath;
            }
            catch (Exception)
            {
            }
            return null;
        }
        /// <summary>
        /// Return a DateTime derived from a Unix Epoch time (seconds from 01/01/1970
        /// </summary>
        /// <param name="unixTime">The long value representing the Unix Time</param>
        /// <returns>Date Time value <see cref="DateTime"/></returns>
        public static DateTime DateTimeFromUnixTime(long unixTime)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(unixTime);
        }
    }
}
