using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace SyncLinks.Models
{
    [DataContract]
    public class GetPackage
    {
        [DataMember]
        public string consumer_key { get; set; }
        [DataMember]
        public string access_token { get; set; }
        [DataMember]
        public int count {get; set;}
        [DataMember]
        public int offset { get; set; }

        string state { get; set; }
        string favorite { get; set; }
        string tag { get; set; }
        string contentType { get; set; }
        string sort { get; set; }
        string detailType { get; set; }
        string search { get; set; }
        string domain { get; set; }
        DateTime since { get; set; }

    }

    [DataContract]
    public class PocketItem
    {
        [DataMember]
        public string item_id { get; set; }
        [DataMember]
        public string resolved_id { get; set; }
        [DataMember]
        public string given_url { get; set; }
        [DataMember]
        public string resolved_url { get; set; }
        [DataMember]
        public string given_title { get; set; }
        [DataMember]
        public string resolved_title { get; set; }
        [DataMember]
        public string favorite { get; set; }
        [DataMember]
        public string status { get; set; }
        [DataMember]
        public string excerpt { get; set; }
        [DataMember]
        public string is_article { get; set; }
        [DataMember]
        public string has_image { get; set; }
        [DataMember]
        public string has_video { get; set; }
        [DataMember]
        public int word_count { get; set; }
        //tags - A JSON object of the user tags associated with the item
        //authors - A JSON object listing all of the authors associated with the item
        //images - A JSON object listing all of the images associated with the item
        //videos - A JSON object listing all of the videos associated with the item
    }

    [DataContract]
    public class AddPocketItem
    {
        [DataMember]
        public string item_id { get; set; }
        [DataMember]
        public string resolved_id { get; set; }
        [DataMember]
        public string normal_url { get; set; }
        [DataMember]
        public string resolved_url { get; set; }
        [DataMember]
        public string title { get; set; }
    }

    [DataContract]
    public class AddResponsePackage
    {
        [DataMember]
        public AddPocketItem item { get; set; }
        [DataMember]
        public int status { get; set; }
    }

    [DataContract]
    public class GetResponsePackage
    {
        [DataMember]
        public int status { get; set; }

        [DataMember]
        public Dictionary<string, PocketItem> list { get; set; }

        public GetResponsePackage()
        {
            list = new Dictionary<string, PocketItem>();
        }
    }

    [DataContract]
    public class TokenRequestPackage
    {
        [DataMember]
        public string consumer_key { get; set; }
        [DataMember]
        public string redirect_uri { get; set; }
    }
}
