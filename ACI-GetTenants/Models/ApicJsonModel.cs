using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using System.Xml.Serialization;

namespace ACI_GetTenants.Models
{
    public class ApicJsonModel
    {
        public ApicJsonModel()
        {
            imdata = new List<imdata>();
        }
        
        [JsonProperty]
        public List<imdata> imdata {get; set;}
    }

    public class imdata 
    {
        [JsonProperty("fvBD")]
        public _fvBD fvBD {get; set;}

        [JsonProperty("vzFilter")]
        public vzFilter vzFilter { get; set; }

        [JsonProperty("fvTenant")]
        public _fvTenant fvTenant { get; set; }

    }

    public class _fvTenant
    {
        [JsonProperty]
        public attributes attributes { get; set; }
    }

    public class vzFilter
    {
        [JsonProperty]
        public attributes attributes { get; set; }
    }
    
    public class _fvBD
    {

        [JsonProperty]
        public attributes attributes {get; set;}
    }

    public class attributes
    {

        [JsonProperty]
        public string dn {get; set;}

        [JsonProperty]
        public string name { get; set; }
    }
}