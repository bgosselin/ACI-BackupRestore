using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using System.Xml.Serialization;

namespace ACI_GetTenants.Models
{
    /// <summary>
    /// This is a .NET model based on the ACI object model used to recieve an APIC GET response
    /// 
    /// NOTE: this model is note complete!
    /// Notice that it only handals a request for Tenants, filters and bridge domains!
    /// It also does not return all the attruibutes for these objects!
    /// 
    /// For the sake of this applicaiton this not an issue since we only need to collect tenant names
    /// I welcome collaboration and further development of this model
    /// </summary>
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