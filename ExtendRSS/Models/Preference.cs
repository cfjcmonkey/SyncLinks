using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO.IsolatedStorage;
using System.Security.Cryptography;

namespace ExtendRSS.Models
{
    public class Preference
    {        
        [XmlIgnore]
        public string Password
        {
            get
            {
                if (IsolatedStorageSettings.ApplicationSettings.Contains("STR_PASSWORD"))
                {
                    var bytes = IsolatedStorageSettings.ApplicationSettings["STR_PASSWORD"] as byte[];
                    var unEncrypteBytes = ProtectedData.Unprotect(bytes, null);
                    return Encoding.UTF8.GetString(unEncrypteBytes, 0, unEncrypteBytes.Length);
                }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
                var encryptedBytes = ProtectedData.Protect(Encoding.UTF8.GetBytes(value), null);
                IsolatedStorageSettings.ApplicationSettings["STR_PASSWORD"] = encryptedBytes;
            }
        }

        [XmlIgnore]
        public string Username
        {
            get
            {
                if (IsolatedStorageSettings.ApplicationSettings.Contains("STR_USERNAME"))
                {
                    var bytes = IsolatedStorageSettings.ApplicationSettings["STR_USERNAME"] as byte[];
                    var unEncrypteBytes = ProtectedData.Unprotect(bytes, null);
                    return Encoding.UTF8.GetString(unEncrypteBytes, 0, unEncrypteBytes.Length);
                }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
                var encryptedBytes = ProtectedData.Protect(Encoding.UTF8.GetBytes(value), null);
                IsolatedStorageSettings.ApplicationSettings["STR_USERNAME"] = encryptedBytes;
            }
        }

        public bool IsSycn;
    }
}
