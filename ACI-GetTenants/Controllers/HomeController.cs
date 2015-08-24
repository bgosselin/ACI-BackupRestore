using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ACI_GetTenants.Models;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using System.Xml.Linq;
using System.Text;
using System.Text.RegularExpressions;




namespace ACI_GetTenants.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            // main page
            //CreateTenantSnapShot("mytenant", "This is a test", @"C:\");

            //RollBackTenant("Tester-66b5584c-8f07-4df0-900a-c11c0070bae4", @"C:\zzTest");

            var collection = ApicGetRequest("api/node/class/fvTenant.json?");
            //var collection = new ApicJsonModel();
            return View("SnapShot", collection);
            
        }

        public ActionResult SnapShot()
        {

            var collection = ApicGetRequest("api/node/class/fvTenant.json?");
            //var collection = new ApicJsonModel();
            return View(collection);
        }

        public ActionResult RollBack()
        {

            var collection = new TenantSnapShots();
            //var tenantModelFromApic = ApicGetRequest("api/node/class/fvTenant.json?");
            collection.Tenants = GetTenantList(@"C:\zzTest");
            collection.CommentsByTenant = GetSnapShotsByTenant(@"C:\zzTest", collection.Tenants[0].tenantName);//);
            collection.Tenants[0].selected = true;
            return View(collection);
        }

        public ActionResult RollBackReload(string selectedTenant)
        {

            
            var collection = new TenantSnapShots();
            //var tenantModelFromApic = ApicGetRequest("api/node/class/fvTenant.json?");
            collection.Tenants = GetTenantList(@"C:\zzTest");
            collection.CommentsByTenant = GetSnapShotsByTenant(@"C:\zzTest", selectedTenant);
            foreach (var tenant in collection.Tenants)
            {
                if (tenant.tenantName == selectedTenant)
                {
                    tenant.selected = true;
                }
            }


            return View("RollBack", collection);
        }

        public ActionResult RollBackExecute(string currentTenant, string selectedSnapShot, string submitButton)
        {

         
            var collection = new TenantSnapShots();
            //var tenantModelFromApic = ApicGetRequest("api/node/class/fvTenant.json?");
            if (submitButton == "Remove")
            {
                string[] filesToDelete = Directory.GetFiles(@"C:\zzTest", currentTenant + "----" + selectedSnapShot);
                foreach (var file in filesToDelete)
                {
                    if (System.IO.File.Exists(file))
                    {
                        System.IO.File.Delete(file);
                    }
                }
            }

            collection.Tenants = GetTenantList(@"C:\zzTest");
            collection.CommentsByTenant = GetSnapShotsByTenant(@"C:\zzTest", currentTenant);
            foreach (var tenant in collection.Tenants)
            {
                if (tenant.tenantName == currentTenant)
                {
                    tenant.selected = true;
                    //if (submitButton == "RollBack")
                    //{
                        RollBackTenant(tenant.tenantName, selectedSnapShot, @"C:\zzTest");
                    //}
                }
            }

            return View("RollBack", collection);
        }

        /// <summary>
        /// This does nothing yet!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        /// </summary>
        /// <param name="selectedTenant"></param>
        /// <param name="selectedSnapShot"></param>
        /// <returns></returns>
        public ActionResult DeleteSnapShot(string selectedTenant, string selectedSnapShot)
        {


            

            var collection = new TenantSnapShots();
            //var tenantModelFromApic = ApicGetRequest("api/node/class/fvTenant.json?");
            string[] filesToDelete = Directory.GetFiles(@"C:\zzTest", selectedTenant + "----" + selectedSnapShot);
            foreach (var file in filesToDelete)
            {
                if (System.IO.File.Exists(file))
        
                {
                    System.IO.File.Delete(file);
                }
            }


            collection.Tenants = GetTenantList(@"C:\zzTest");
            collection.CommentsByTenant = GetSnapShotsByTenant(@"C:\zzTest", selectedTenant);
            foreach (var tenant in collection.Tenants)
            {
                if (tenant.tenantName == selectedTenant)
                {
                    tenant.selected = true;

                }
            }

            return View("RollBack", collection);
        }

 

        public List<SnapShotComment> GetSnapShotsByTenant(string directory, string tenantName)
        {
            List<SnapShotComment> commentsByTenant = new List<SnapShotComment>();
            string comment;
            string guid;
            string[] commentFiles = Directory.GetFiles(directory, tenantName+"*.txt");

            foreach (string commentFile in commentFiles)
            {
                guid = commentFile.Replace(directory+@"\"+tenantName+"----", "").Replace(".txt","");
              
                comment = System.IO.File.ReadAllText(commentFile);
                commentsByTenant.Add(new SnapShotComment(guid, comment));
  
            }
            return commentsByTenant;
        }

        public List<Tenant> GetTenantList(string directory)
        {
            /*
            int i=0;
            List<Tenant> Tenants = new List<Tenant>(); 
            foreach (string tenantName in collection.imdata.Select(t => t.fvTenant).Select(t => t.attributes).Select(t => t.name))
            {
                Tenants.Add(new Tenant(tenantName, false));

            }*/
            List<Tenant> Tenants = new List<Tenant>();
            string tenantName;
            string[] commentFiles = Directory.GetFiles(directory, "*.txt");

            foreach (string commentFile in commentFiles)
            {

                tenantName = Regex.Replace(commentFile, "----(.*?)(?:$)", "");
                tenantName = tenantName.Replace(directory + @"\", "");
                if (Tenants.Count()>0)
                {
                    if (tenantName != Tenants.Last().tenantName)
                    {
                        Tenants.Add(new Tenant(tenantName, false));
                    }
                }
                else
                {
                    Tenants.Add(new Tenant(tenantName, false));
                }
            }


            return Tenants;
        }
        public ActionResult Tenant()
        {
            
            var collection = ApicGetRequest("api/node/class/fvTenant.json?");
            return View(collection);
        }

        
        public ActionResult CheckInTenant()
        {
            string tenant = Request.Form["tenantSelect"];
            string comments = Request.Form["comments"];
            /*
            XMLDeleteTenant(Request.Form["tenantSelect"]);
            scriptSettings.StaticXMLFileSource(@"DynamicScripts\Delete_Tenant.cfg");
            Result = ExecutePythonScript(scriptSettings);
            var readLines = System.IO.File.ReadAllText(@"C:\ACIDemo\DynamicScripts\DynamicScript_Delete_Tenant.xml");

            ViewBag.xmlOutput = readLines;
             * 
             * 
        */


            //RollBackTenant("5b8c4a44-5a61-4b47-bf93-dd4fc6eebe8e", @"C:\");
            //ViewBag.pageResults = "Done!";


            CreateTenantSnapShot(tenant, comments, @"C:\zzTest"); //**************put this in config file
      

            var collection = ApicGetRequest("api/node/class/fvTenant.json?");
            return View("SnapShot", collection);
        }
        
        /// <summary>
        /// Posts the specified backup tenant config to the APIC
        /// </summary>
        /// <param name="snapShotDirectory">Directory on the server where the xml backup config files are stored</param>
        /// <param name="snapShotGuid">GUID for the xml backup config</param>
        /// <returns></returns>
        public bool RollBackTenant(string tenantName, string snapShotGuid, string snapShotDirectory)
        {
            bool success = false;
            var apicSettings = new APICSettings();
            HttpResponseMessage result;


            var snapShotFile = string.Format(@"{0}\{1}----{2}.xml", snapShotDirectory, tenantName, snapShotGuid);
            var readLines = System.IO.File.ReadAllText(snapShotFile);
            var requestUri = string.Format(@"https://{0}/api/node/mo/.xml", apicSettings.url);

            if (apicSettings.credentialsSet)
            {


                ServicePointManager
                .ServerCertificateValidationCallback +=
                (sender, cert, chain, sslPolicyErrors) => true;

                var APICSessionCookieContainer = new CookieContainer();
                using (var handler = new HttpClientHandler() { CookieContainer = APICSessionCookieContainer, UseCookies = true, })
                using (var httpClient = new HttpClient(handler))
                {
                    httpClient.BaseAddress = new Uri("https://" + apicSettings.url);

                    result = httpClient.PostAsJsonAsync("api/aaaLogin.json", new
                    {
                        aaaUser = new
                        {
                            attributes = new
                            {
                                name = apicSettings.username,
                                pwd = apicSettings.password
                            }
                        }

                    }).Result;

                    //delete current version of the tenant
                    if (result.IsSuccessStatusCode)
                    {
                        var apiContent = String.Format("api/mo/uni/tn-{0}.xml", tenantName);
                        result = httpClient.DeleteAsync(apiContent).Result;
                    }
                
                    if (result.IsSuccessStatusCode)
                    {
                        
                        var httpContent = new StringContent(readLines, Encoding.UTF8, "application/xml");
                        success = httpClient.PostAsync(requestUri, httpContent).Result.IsSuccessStatusCode;
                    }
                }
            }
            return success;
            
        }
       
        /// <summary>
        /// Create a new snap shot of a tenant and save it in the specified server directory along with the comments
        /// </summary>
        /// <param name="tenantName"></param>
        /// <param name="comments"></param>
        /// <param name="snapShotDirectory"></param>
        public void CreateTenantSnapShot(string tenantName, string comments, string snapShotDirectory)
        {
            Guid guid = Guid.NewGuid();
            string guidId = guid.ToString();
            string response = null;
            var apicSettings = new APICSettings();
            string request = string.Format("api/node/mo/uni/tn-{0}.xml?query-target=self&rsp-subtree=full&rsp-prop-include=config-only", tenantName);
            if (apicSettings.credentialsSet)
            
            {


                ServicePointManager
                .ServerCertificateValidationCallback +=
                (sender, cert, chain, sslPolicyErrors) => true;

                var APICSessionCookieContainer = new CookieContainer();
                using (var handler = new HttpClientHandler() { CookieContainer = APICSessionCookieContainer, UseCookies = true, })
                using (var httpClient = new HttpClient(handler))
                {
                    httpClient.BaseAddress = new Uri("https://" + apicSettings.url);

                    var result = httpClient.PostAsJsonAsync("api/aaaLogin.json", new
                    {
                        aaaUser = new
                        {
                            attributes = new
                            {
                                name = apicSettings.username,
                                pwd = apicSettings.password
                            }
                        }

                    }).Result;



                    using (StreamReader streamIn = new StreamReader(httpClient.GetStreamAsync(request).Result))
                    {
                        response = streamIn.ReadToEnd();
                        streamIn.Close();
                    }
                    /////////////////////*********
                    //need to replace IMDATA in the xml!
                    string formattedResponse = Regex.Replace(response,"<imdata totalCount(.*?)(?:>|$)","<polUni>");
                    formattedResponse = formattedResponse.Replace("</imdata>", "</polUni>");

                    DateTime today = DateTime.Today;
                    string date = today.ToString();
                    string xmlFile = string.Format(@"{0}\{1}.xml", snapShotDirectory, tenantName+"----"+guidId);
                    XmlDocument xDoc = new XmlDocument();
                    xDoc.LoadXml(formattedResponse);
                    xDoc.Save(xmlFile);

                    
                    string commentFile = string.Format(@"{0}\{1}.txt", snapShotDirectory, tenantName+"----"+guidId);
                    System.IO.File.WriteAllText(commentFile, date + " - " + comments);


                    //remove
                    //var collection = result.Content.ReadAsAsync<ApicJsonModel>().Result;

                }
            }
         
        }


        public ApicJsonModel ApicGetRequest(string apicCall)
        {
            var apicSettings = new APICSettings();
            if (apicSettings.credentialsSet)
            {


                ServicePointManager
                .ServerCertificateValidationCallback +=
                (sender, cert, chain, sslPolicyErrors) => true;

                var APICSessionCookieContainer = new CookieContainer();
                using (var handler = new HttpClientHandler() { CookieContainer = APICSessionCookieContainer, UseCookies = true, })
                using (var httpClient = new HttpClient(handler))
                {
                    httpClient.BaseAddress = new Uri("https://" + apicSettings.url);

                    var result = httpClient.PostAsJsonAsync("api/aaaLogin.json", new
                    {
                        aaaUser = new
                        {
                            attributes = new
                            {
                                name = apicSettings.username,
                                pwd = apicSettings.password
                            }
                        }

                    }).Result;

                
                    result = httpClient.GetAsync(apicCall).Result;


                    var collection = result.Content.ReadAsAsync<ApicJsonModel>().Result;
                    return collection;
                }
            }
            else
            {
                return new ApicJsonModel();
            }
        }
    }
}