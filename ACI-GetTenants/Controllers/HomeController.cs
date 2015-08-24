/*
  Blair Gosselin (blair.gosselin@live.com)
 * 
  Licensed under the Apache License, Version 2.0 (the "License");
  you may not use this file except in compliance with the License.
  You may obtain a copy of the License at
 
      http://www.apache.org/licenses/LICENSE-2.0
 
  Unless required by applicable law or agreed to in writing, software
  distributed under the License is distributed on an "AS IS" BASIS,
  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
  See the License for the specific language governing permissions and
  limitations under the License.
 *
 */

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
        /// <summary>
        /// Main Method launched when navigating to the site
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {

            //collection is a data model to be used by the MVC view.
            //In this case collection is returned by the APIC.  In this case the request is for all tenants so collection returns a list of the tenants to be used on the SnapShot page
            var collection = ApicGetRequest("api/node/class/fvTenant.json?");

            //Go to the SnapShot page
            return View("SnapShot", collection);
            
        }

        /// <summary>
        /// SnapShot page 
        /// </summary>
        /// <returns></returns>
        public ActionResult SnapShot()
        {

            //collection is a data model to be used by the MVC view
            //in this case collection is returned by the APIC.  In this case the request is for all tenants so collection returns a list of the tenants to be used on the SnapShot page
            var collection = ApicGetRequest("api/node/class/fvTenant.json?");

            //Go to the SnapShot page
            return View("SnapShot", collection);
        }

        /// <summary>
        /// RollBack Page
        /// </summary>
        /// <returns></returns>
        public ActionResult RollBack()
        {

            //Collect is a model to be used by the VMC view
            var collection = new TenantSnapShots();
            var SnapShotDirectory = GetWebConfigValues("LocalSnapShotDirectory");

            //Get the tenants that have a saved SnapShots
            collection.Tenants = GetTenantList(SnapShotDirectory);

            //Get the SnapShots available for the active tenant
            //In this case, set the first returned tenant as active until the use selects a differnet tenant in the MVC view
            collection.CommentsByTenant = GetSnapShotsByTenant(SnapShotDirectory, collection.Tenants[0].tenantName);//);
            collection.Tenants[0].selected = true;

            //Load RollBack page
            return View(collection);
        }

        /// <summary>
        /// Change Selected Tenant of the RollBack page
        /// </summary>
        /// <param name="selectedTenant"></param>
        /// <returns></returns>
        public ActionResult RollBackReload(string selectedTenant)
        {

            //Similar as RollBack() except the active tenant is defined by user driven input
            var collection = new TenantSnapShots();
            var SnapShotDirectory = GetWebConfigValues("LocalSnapShotDirectory");

            collection.Tenants = GetTenantList(SnapShotDirectory);
            collection.CommentsByTenant = GetSnapShotsByTenant(SnapShotDirectory, selectedTenant);
            foreach (var tenant in collection.Tenants)
            {
                if (tenant.tenantName == selectedTenant)
                {
                    tenant.selected = true;
                }
            }


            return View("RollBack", collection);
        }

        /// <summary>
        /// RollBack the user Selected Tenant
        /// </summary>
        /// <param name="currentTenant"></param>
        /// <param name="selectedSnapShot"></param>
        /// <returns></returns>
        public ActionResult RollBackExecute(string currentTenant, string selectedSnapShot)
        {

            //Same as RollBack() except the selected snapshot is used to trigger a the RollBackTenant method
            var collection = new TenantSnapShots();
            var SnapShotDirectory = GetWebConfigValues("LocalSnapShotDirectory");


            collection.Tenants = GetTenantList(SnapShotDirectory);
            collection.CommentsByTenant = GetSnapShotsByTenant(SnapShotDirectory, currentTenant);
            foreach (var tenant in collection.Tenants)
            {
                if (tenant.tenantName == currentTenant)
                {
                    tenant.selected = true;

                    RollBackTenant(tenant.tenantName, selectedSnapShot, SnapShotDirectory);

                }
            }

            return View("RollBack", collection);
        }

        /// <summary>
        /// This does nothing yet!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        /// Potential feature expansion - providing uses with the ability to delete unwanted SnapShots
        /// </summary>
        /// <param name="selectedTenant"></param>
        /// <param name="selectedSnapShot"></param>
        /// <returns></returns>
        public ActionResult DeleteSnapShot(string selectedTenant, string selectedSnapShot)
        {
            var collection = new TenantSnapShots();
            var SnapShotDirectory = GetWebConfigValues("LocalSnapShotDirectory");
            //var tenantModelFromApic = ApicGetRequest("api/node/class/fvTenant.json?");
            string[] filesToDelete = Directory.GetFiles(SnapShotDirectory, selectedTenant + "----" + selectedSnapShot);
            foreach (var file in filesToDelete)
            {
                if (System.IO.File.Exists(file))
        
                {
                    System.IO.File.Delete(file);
                }
            }


            collection.Tenants = GetTenantList(SnapShotDirectory);
            collection.CommentsByTenant = GetSnapShotsByTenant(SnapShotDirectory, selectedTenant);
            foreach (var tenant in collection.Tenants)
            {
                if (tenant.tenantName == selectedTenant)
                {
                    tenant.selected = true;

                }
            }

            return View("RollBack", collection);
        }

 
        /// <summary>
        /// Returns a list of comments for all the SnapShots of the selected Tenant
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="tenantName"></param>
        /// <returns></returns>
        public List<SnapShotComment> GetSnapShotsByTenant(string directory, string tenantName)
        {
            List<SnapShotComment> commentsByTenant = new List<SnapShotComment>();
            string comment;
            string guid;
            string[] commentFiles = Directory.GetFiles(directory, tenantName+"*.txt");

            foreach (string commentFile in commentFiles)
            {
                //Parse the filenames for the GUID
                guid = commentFile.Replace(directory+@"\"+tenantName+"----", "").Replace(".txt","");
              
                comment = System.IO.File.ReadAllText(commentFile);
                commentsByTenant.Add(new SnapShotComment(guid, comment));
  
            }
            return commentsByTenant;
        }

        /// <summary>
        /// Returns a list of all the tenants which have a SnapShot file in the directory 
        /// This is not the same as a GET request to the APIC for tenants, this method provides only tenants that have a saved SnapShot
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        public List<Tenant> GetTenantList(string directory)
        {

            List<Tenant> Tenants = new List<Tenant>();
            string tenantName;
            string[] commentFiles = Directory.GetFiles(directory, "*.txt");

            foreach (string commentFile in commentFiles)
            {
                //parse the file names for the tenant name
                tenantName = Regex.Replace(commentFile, "----(.*?)(?:$)", "");
                tenantName = tenantName.Replace(directory + @"\", "");

                //If there are is at lease one record in the SnapShot directory than add it
                if (Tenants.Count()>0)
                {
                    //Only add the name of each tenant onece
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


        /// <summary>
        /// SnapShot Page; add new SnapShot
        /// </summary>
        /// <returns></returns>
        public ActionResult CheckInTenant()
        {
            string tenant = Request.Form["tenantSelect"];
            string comments = Request.Form["comments"];
            var SnapShotDirectory = GetWebConfigValues("LocalSnapShotDirectory");
            CreateTenantSnapShot(tenant, comments, SnapShotDirectory);
      

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

            //Build the components for POST call to the APIC
            var snapShotFile = string.Format(@"{0}\{1}----{2}.xml", snapShotDirectory, tenantName, snapShotGuid);
            var readLines = System.IO.File.ReadAllText(snapShotFile);
            var requestUri = string.Format(@"https://{0}/api/node/mo/.xml", apicSettings.url);
            //login
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
                
                    //post the SnapShot for the tenant
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

            //build the GET for the APIC
            string request = string.Format("api/node/mo/uni/tn-{0}.xml?query-target=self&rsp-subtree=full&rsp-prop-include=config-only", tenantName);

            //login
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


                    //GET the config for the specified tenant
                    using (StreamReader streamIn = new StreamReader(httpClient.GetStreamAsync(request).Result))
                    {
                        response = streamIn.ReadToEnd();
                        streamIn.Close();
                    }

                    //modify the config - replace the imdata xml tag with polUni - making the XML usable to for APIC Config REST POSTs
                    string formattedResponse = Regex.Replace(response,"<imdata totalCount(.*?)(?:>|$)","<polUni>");
                    formattedResponse = formattedResponse.Replace("</imdata>", "</polUni>");

                    //build the snapshot file with the date and a GUID for indexing later
                    DateTime today = DateTime.Today;
                    string date = today.ToString();
                    string xmlFile = string.Format(@"{0}\{1}.xml", snapShotDirectory, tenantName+"----"+guidId);
                    XmlDocument xDoc = new XmlDocument();
                    xDoc.LoadXml(formattedResponse);
                    xDoc.Save(xmlFile);

                    //write the SnapShot file
                    string commentFile = string.Format(@"{0}\{1}.txt", snapShotDirectory, tenantName+"----"+guidId);
                    System.IO.File.WriteAllText(commentFile, date + " - " + comments);


                }
            }
         
        }

        /// <summary>
        /// Takes a GET API request and returns the results in a .NET JSON model for ACI responses
        /// </summary>
        /// <param name="apicCall"></param>
        /// <returns></returns>
        public ApicJsonModel ApicGetRequest(string apicCall)
        {
            var apicSettings = new APICSettings();

            //login to the APIC
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

                    //get the results of the call
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

        /// <summary>
        /// Get values from the Web Config file at run time by providing the key
        /// </summary>
        /// <param name="key"></param>
        /// <returns>all values are returned as strings</returns>
        public string GetWebConfigValues(string key)
        {
            string value = null;

            value = System.Configuration.ConfigurationManager.AppSettings[key].ToString();

            return value;
        }
    }
}