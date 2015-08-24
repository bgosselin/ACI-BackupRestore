using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;





namespace ACI_GetTenants.Models
{
    /// <summary>
    /// Data model for the APIC creds needed 
    /// </summary>
    public class APICSettings
    {
        public string username { get; set; }
        public string password { get; set; }
        public string url { get; set; }
        public bool credentialsSet { get; set; }

        public APICSettings()
        {
            username = System.Configuration.ConfigurationManager.AppSettings["apicUsername"].ToString();
            password = System.Configuration.ConfigurationManager.AppSettings["apicPassword"].ToString();
            url = System.Configuration.ConfigurationManager.AppSettings["apicUrl"].ToString();
            credentialsSet = true;


            if (username == null || username == "" || password == null || password == "" || url == null || url == "")
            {
                username = "";
                password = "";
                url = "";
                credentialsSet = false;
            }
            
        }

    }
}