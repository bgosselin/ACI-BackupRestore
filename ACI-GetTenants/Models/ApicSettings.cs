using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ACI_GetTenants.Models
{
    public class APICSettings
    {
        public string username { get; set; }
        public string password { get; set; }
        public string url { get; set; }
        public bool credentialsSet { get; set; }

        public APICSettings()
        {
            username = "admin";
            password = "C1sco12345";
            url = "198.18.133.200";
            credentialsSet = true;
            /*
             * This is code to grab APIC creds from the session rather than hardcoding
             * Use it when using the login page
             * 
            username = (string)(Session["username"]);
            password = (string)(Session["password"]);
            url = (string)(Session["apicUrl"]);
            credentialsSet = true;


            if (username == null || username == "" || password == null || password == "" || url == null || url == "")
            {
                username = "";
                password = "";
                url = "";
                credentialsSet = false;
            }
             * */
        }

    }
}