using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO.IsolatedStorage;
using System.Security.Cryptography;
using System.IO;

namespace SyncLinks.Models
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

        [XmlIgnore]
        public string AccessToken
        {
            get
            {
                if (IsolatedStorageSettings.ApplicationSettings.Contains("STR_ACCESSTOKEN"))
                {
                    var bytes = IsolatedStorageSettings.ApplicationSettings["STR_ACCESSTOKEN"] as byte[];
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
                IsolatedStorageSettings.ApplicationSettings["STR_ACCESSTOKEN"] = encryptedBytes;
            }
        }

        [XmlIgnore]
        public string RequestToken
        {
            get
            {
                if (IsolatedStorageSettings.ApplicationSettings.Contains("STR_REQUESTTOKEN"))
                {
                    var bytes = IsolatedStorageSettings.ApplicationSettings["STR_REQUESTTOKEN"] as byte[];
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
                IsolatedStorageSettings.ApplicationSettings["STR_REQUESTTOKEN"] = encryptedBytes;
            }
        }

        ///// <summary>
        ///// 从本地加载Preference
        ///// </summary>
        ///// <returns>返回Preference. 若没有存储，则返回null</returns>
        //public static Preference LoadPreference()
        //{
        //    XmlSerializer ser = new XmlSerializer(typeof(Preference));
        //    if (App.store.FileExists("Config.xml"))
        //    {
        //        using (var s = App.store.OpenFile("Config.xml", FileMode.Open, FileAccess.Read, FileShare.Read))
        //            return ((Preference)ser.Deserialize(s));
        //    }
        //    else return null;
        //}

        ///// <summary>
        ///// 保存Preference到本地
        ///// </summary>
        //public void SavePreference()
        //{
        //    XmlSerializer ser = new XmlSerializer(typeof(Preference));
        //    using (var s = App.store.CreateFile("Config.xml"))
        //        ser.Serialize(s, this);
        //}
    }
}
