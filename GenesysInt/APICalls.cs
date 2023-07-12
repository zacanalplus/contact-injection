using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using Zlibg;
using PureCloudPlatform.Client.V2.Model;
using System.Text;
using GenesysInt;

namespace Genesys
{
    class APIs
    {
        static AppConfigurator cfg = new AppConfigurator(System.Reflection.Assembly.GetExecutingAssembly().Location);
        static string AccessToken
        {
            get;set;
        }
        static APIs()
        {
            AccessToken = GetToken();
        }

        public static OrganizationInfo GetMe()
        {
            WebResponse webResponse;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(cfg.GetConfigValue("MainURI") + cfg.GetConfigValue("Me"));
                request.Method = "GET";
                if (ProxyParameters.useProxy)
                    request.Proxy = ProxyParameters.wProxy;
                request.Headers.Add("Authorization", "Bearer " + AccessToken); //cfg.GetConfigValue("AuthToken"));
                request.ContentType = "application/json";

                webResponse = request.GetResponse();
                string response = "";
                using (Stream webStream = webResponse.GetResponseStream())
                {
                    if (webStream != null)
                    {
                        using (StreamReader responseReader = new StreamReader(webStream))
                        {
                            response = responseReader.ReadToEnd();
                        }
                    }
                }
                return JsonConvert.DeserializeObject<OrganizationInfo>(response);
            }
            catch (Exception e)
            {
                Logger.Write(GenesysInt.Properties.Resources.FAILCHKSELFINFO + e.Message);
                return null;
            }
        }

        public static ContactListEntity ListContactList(int pageNumber=1)
        {
            try
            {
                HttpWebRequest request =
                pageNumber ==1?
                    (HttpWebRequest)WebRequest.Create(cfg.GetConfigValue("MainURI") + cfg.GetConfigValue("ListContactList"))
                    : (HttpWebRequest)WebRequest.Create(cfg.GetConfigValue("MainURI") + cfg.GetConfigValue("ListContactList")+ cfg.GetConfigValue("CListPageNumber") + pageNumber.ToString());
                request.Method = "GET";
                if (ProxyParameters.useProxy)
                    request.Proxy = ProxyParameters.wProxy;
                request.Headers.Add("Authorization", "Bearer " + AccessToken); //cfg.GetConfigValue("AuthToken"));
                request.ContentType = "application/json";

                WebResponse webResponse = request.GetResponse();
                string response = "";
                using (Stream webStream = webResponse.GetResponseStream())
                {
                    if (webStream != null)
                    {
                        using (StreamReader responseReader = new StreamReader(webStream))
                        {
                            response = responseReader.ReadToEnd();
                        }
                    }
                }
                ContactListEntity ce = JsonConvert.DeserializeObject<ContactListEntity>(response);
                // If ContactList page is 1 only
                if ((ce.pageCount == 1 && ce.total == ce.entities.Length) || pageNumber>1)
                    return ce;
                // If ContactList page is more than 1
                else
                {
                    for(int i=2; i<=ce.pageCount; i++)
                    {
                        ContactListEntity ince = ListContactList(i);
                        int origEntyLen = ce.entities.Length;
                        Array.Resize<ContactList>(ref ce.entities, origEntyLen + ince.entities.Length);
                        Array.Copy(ince.entities, ce.entities, ince.entities.Length);
                    }
                    return ce;
                }
            }
            catch (WebException e)
            {
                Genesys.GenesysErrorResponse er = GetErrorResponse(e);
                throw new Genesys.GenesysException(er);
            }
            catch (Exception e)
            {
                Logger.Write(GenesysInt.Properties.Resources.FAILCONTLST + e.Message);
                Logger.Flush();
                return null;
            }
        }

        public static T ListByPages<T>(string cdApiURI, int pageNumber = 1)
        {
            try
            {
                HttpWebRequest request =
                pageNumber == 1 ?
                    (HttpWebRequest)WebRequest.Create(cfg.GetConfigValue("MainURI") + cdApiURI )
                    : (HttpWebRequest)WebRequest.Create(cfg.GetConfigValue("MainURI") + cdApiURI + cfg.GetConfigValue("CListPageNumber") + pageNumber.ToString());
                request.Method = "GET";
                if (ProxyParameters.useProxy)
                    request.Proxy = ProxyParameters.wProxy;
                request.Headers.Add("Authorization", "Bearer " + AccessToken);
                request.ContentType = "application/json";
                WebResponse webResponse = request.GetResponse();
                string response = "";
                using (Stream webStream = webResponse.GetResponseStream())
                {
                    if (webStream != null)
                    {
                        using (StreamReader responseReader = new StreamReader(webStream))
                        {
                            response = responseReader.ReadToEnd();
                        }
                    }
                    return JsonConvert.DeserializeObject<T>(response);
                }
            }
            catch (WebException e)
            {
                Genesys.GenesysErrorResponse er = GetErrorResponse(e);
                throw new Genesys.GenesysException(er);
            }
            catch (GenesysException ge)
            {
                Logger.Write(GenesysInt.Properties.Resources.FAILGENERICLST + ge.Message + " Details : " + ge.errResponse.message);
                Logger.Flush();
                return default(T);
            }
            catch (Exception e)
            {
                Logger.Write(GenesysInt.Properties.Resources.FAILGENERICLST + e.Message);
                Logger.Flush();
                return default(T);
            }
        }

        public static AgentCCList ListByAllPage ()
        {
            AgentCCList accl = ListByPages<AgentCCList>(cfg.GetConfigValue("ListAgentAct"));
            for(int i=2; i<=accl.pageCount; i++)
            {
                AgentCCList inn = ListByPages<AgentCCList>(cfg.GetConfigValue("ListAgentAct"), i);
                foreach (AgentCC a in inn.entities)
                {
                    accl.entities.Add(new AgentCC(a));
                }
            }
            return accl;
        }

        public static AgentCCList ListByAllPage(string apiUriCfg)
        {
            AgentCCList accl = ListByPages<AgentCCList>(cfg.GetConfigValue(apiUriCfg));
            for (int i = 2; i <= accl.pageCount; i++)
            {
                AgentCCList inn = ListByPages<AgentCCList>(cfg.GetConfigValue(apiUriCfg), i);
                foreach (AgentCC a in inn.entities)
                {
                    accl.entities.Add(new AgentCC(a));
                }
            }
            return accl;
        }

        public static string GenericAPICall(string cdApiURI, int pageNumber = 1)
        {
            try
            {
                HttpWebRequest request =
                pageNumber == 1 ?
                    (HttpWebRequest)WebRequest.Create(cfg.GetConfigValue("MainURI") + cfg.GetConfigValue(cdApiURI))
                    : (HttpWebRequest)WebRequest.Create(cfg.GetConfigValue("MainURI") + cfg.GetConfigValue(cdApiURI) + cfg.GetConfigValue("CListPageNumber") + pageNumber.ToString());
                request.Method = "GET";
                if (ProxyParameters.useProxy)
                    request.Proxy = ProxyParameters.wProxy;
                request.Headers.Add("Authorization", "Bearer " + AccessToken);
                request.ContentType = "application/json";
                WebResponse webResponse = request.GetResponse();
                string response = "";
                using (Stream webStream = webResponse.GetResponseStream())
                {
                    if (webStream != null)
                    {
                        using (StreamReader responseReader = new StreamReader(webStream))
                        {
                            response = responseReader.ReadToEnd();
                        }
                    }
                    return response;
                }
            }
            catch (WebException e)
            {
                Genesys.GenesysErrorResponse er = GetErrorResponse(e);
                throw new Genesys.GenesysException(er);
            }
            catch (GenesysException ge)
            {
                Logger.Write(GenesysInt.Properties.Resources.FAILGENERICCALL + ge.Message + " Details : " + ge.errResponse.message);
                Logger.Flush();
                return "";
            }
            catch (Exception e)
            {
                Logger.Write(GenesysInt.Properties.Resources.FAILGENERICCALL + e.Message);
                Logger.Flush();
                return "";
            }
        }

        public static string GenericAPICall2(string cdApiURI, int pageNumber = 1)
        {
            try
            {
                HttpWebRequest request =
                pageNumber == 1 ?
                    (HttpWebRequest)WebRequest.Create(cfg.GetConfigValue("MainURI") + cdApiURI)
                    : (HttpWebRequest)WebRequest.Create(cfg.GetConfigValue("MainURI") + cdApiURI + cfg.GetConfigValue("CListPageNumber") + pageNumber.ToString());
                request.Method = "GET";
                if (ProxyParameters.useProxy)
                    request.Proxy = ProxyParameters.wProxy;
                request.Headers.Add("Authorization", "Bearer " + AccessToken);
                request.ContentType = "application/json";
                WebResponse webResponse = request.GetResponse();
                string response = "";
                using (Stream webStream = webResponse.GetResponseStream())
                {
                    if (webStream != null)
                    {
                        using (StreamReader responseReader = new StreamReader(webStream))
                        {
                            response = responseReader.ReadToEnd();
                        }
                    }
                    return response;
                }
            }
            catch (WebException e)
            {
                Genesys.GenesysErrorResponse er = GetErrorResponse(e);
                throw new Genesys.GenesysException(er);
            }
            catch (GenesysException ge)
            {
                Logger.Write(GenesysInt.Properties.Resources.FAILGENERICCALL + ge.Message + " Details : " + ge.errResponse.message);
                Logger.Flush();
                return "";
            }
            catch (Exception e)
            {
                Logger.Write(GenesysInt.Properties.Resources.FAILGENERICCALL + e.Message);
                Logger.Flush();
                return "";
            }
        }

        public static string ListByPages_tmpl(string cdApiURI, int pageNumber = 1)
        {
            try
            {
                HttpWebRequest request =
                pageNumber == 1 ?
                    (HttpWebRequest)WebRequest.Create(cfg.GetConfigValue("MainURI") + cfg.GetConfigValue(cdApiURI))
                    : (HttpWebRequest)WebRequest.Create(cfg.GetConfigValue("MainURI") + cfg.GetConfigValue(cdApiURI) + cfg.GetConfigValue("CListPageNumber") + pageNumber.ToString());
                request.Method = "GET";
                if (ProxyParameters.useProxy)
                    request.Proxy = ProxyParameters.wProxy;
                request.Headers.Add("Authorization", "Bearer " + AccessToken);
                request.ContentType = "application/json";
                WebResponse webResponse = request.GetResponse();
                string response = "";
                using (Stream webStream = webResponse.GetResponseStream())
                {
                    if (webStream != null)
                    {
                        using (StreamReader responseReader = new StreamReader(webStream))
                        {
                            response = responseReader.ReadToEnd();
                        }
                    }
                    return response;
                }
            }
            catch (WebException e)
            {
                Genesys.GenesysErrorResponse er = GetErrorResponse(e);
                throw new Genesys.GenesysException(er);
            }
            catch(GenesysException ge)
            {
                Logger.Write(GenesysInt.Properties.Resources.FAILGENERICLST + ge.Message + " Details : " + ge.errResponse.message);
                Logger.Flush();
                return null;
            }
            catch (Exception e)
            {
                Logger.Write(GenesysInt.Properties.Resources.FAILGENERICLST + e.Message);
                Logger.Flush();
                return null;
            }
        }

        public static AgentCCList ListAgentActivitySummary(int pageNumber = 1)
        {
            try
            {
                HttpWebRequest request =
                pageNumber == 1 ?
                    (HttpWebRequest)WebRequest.Create(cfg.GetConfigValue("MainURI") + cfg.GetConfigValue("ListAgentAct"))
                    : (HttpWebRequest)WebRequest.Create(cfg.GetConfigValue("MainURI") + cfg.GetConfigValue("ListAgentAct") + cfg.GetConfigValue("CListPageNumber") + pageNumber.ToString());
                request.Method = "GET";
                if (ProxyParameters.useProxy)
                    request.Proxy = ProxyParameters.wProxy;
                request.Headers.Add("Authorization", "Bearer " + AccessToken); //cfg.GetConfigValue("AuthToken"));
                request.ContentType = "application/json";

                WebResponse webResponse = request.GetResponse();
                string response = "";
                using (Stream webStream = webResponse.GetResponseStream())
                {
                    if (webStream != null)
                    {
                        using (StreamReader responseReader = new StreamReader(webStream))
                        {
                            response = responseReader.ReadToEnd();
                        }
                    }
                }
                AgentCCList ce = JsonConvert.DeserializeObject<AgentCCList>(response);
                // If Agent Activity Summary List page is 1 only
                if ((ce.pageCount == 1 && ce.total == ce.entities.Count) || pageNumber > 1)
                    return ce;
                // If Agent Activity Summary List page is more than 1
                else
                {
                    for (int i = 2; i <= ce.pageCount; i++)
                    {
                        AgentCCList ince = ListAgentActivitySummary(i);
                        foreach(AgentCC a in ince.entities)
                        {
                            ce.entities.Add(new AgentCC(a));
                        }
                    }
                    return ce;
                }
            }
            catch (WebException e)
            {
                Genesys.GenesysErrorResponse er = GetErrorResponse(e);
                throw new Genesys.GenesysException(er);
            }
            catch (Exception e)
            {
                Logger.Write(GenesysInt.Properties.Resources.FAILAGNTACTS + e.Message);
                Logger.Flush();
                return null;
            }
        }

        public static bool Exists(PureCloudPlatform.Client.V2.Model.ContactList cl)
        {
            ContactListEntity ce = ListContactList();
            if (ce == null) return false;
            foreach (Genesys.ContactList ecl in ce.entities)
            {
                if (ecl.id == cl.Id || ecl.name == cl.Name)
                    return true;
            }
            return false;
        }

        public static bool Exists(ContactList cl)
        {
            ContactListEntity ce = ListContactList();
            if (ce == null) return false;
            foreach (Genesys.ContactList ecl in ce.entities)
            {
                if (ecl.id == cl.id || ecl.name == cl.name)
                    return true;
            }
            return false;
        }

        public static bool Exists(string contactListID)
        {
            ContactListEntity ce = ListContactList();
            if (ce == null) return false;
            foreach (Genesys.ContactList ecl in ce.entities)
            {
                if (ecl.id == contactListID)
                    return true;
            }
            return false;
        }

        public static bool ClearContactInList(string clid)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(cfg.GetConfigValue("MainURI") + cfg.GetConfigValue("ClearContacts_1") + clid + cfg.GetConfigValue("ClearContacts_2"));
                request.Method = "POST";
                request.Headers.Add("Authorization", "Bearer " + AccessToken); //cfg.GetConfigValue("AuthToken"));
                request.ContentType = "application/json";
                if (ProxyParameters.useProxy)
                    request.Proxy = ProxyParameters.wProxy;

                WebResponse webResponse = request.GetResponse();
                string response = "";
                using (Stream webStream = webResponse.GetResponseStream())
                {
                    if (webStream != null)
                    {
                        using (StreamReader responseReader = new StreamReader(webStream))
                        {
                            response = responseReader.ReadToEnd();
                        }
                    }
                }
                return true;
            }
            catch (ContactListNotFoundException)
            {
                throw;
            }
            catch (System.Net.WebException e)
            {
                WebResponse webResponse = e.Response;
                string response = "";
                using (Stream webStream = webResponse.GetResponseStream())
                {
                    if (webStream != null)
                    {
                        using (StreamReader responseReader = new StreamReader(webStream))
                        {
                            response = responseReader.ReadToEnd();
                        }
                    }
                }
                try
                {
                    GenesysErrorResponse obj = JsonConvert.DeserializeObject<GenesysErrorResponse>(response);
                    if (obj.code == cfg.GetConfigValue("JsonDTresNFound") || obj.code == cfg.GetConfigValue("JsonContactListNotFound"))
                    {
                        ContactListNotFoundException clnf = new ContactListNotFoundException();
                        clnf.errResponse = obj;
                        throw new ContactListNotFoundException(obj.message);
                    }
                    return false;
                }
                catch (ContactListNotFoundException clnfe)
                {
                    throw clnfe;
                }
                catch (Exception)
                {
                    throw e;
                }
            }
            //return false;
        }

        public static DeletedContactList DeleteContactList(string clid)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(cfg.GetConfigValue("MainURI") + cfg.GetConfigValue("DelContactList") + clid);
                request.Method = "DELETE";
                request.Headers.Add("Authorization", "Bearer " + AccessToken); //cfg.GetConfigValue("AuthToken"));
                request.ContentType = "application/json";
                if (ProxyParameters.useProxy)
                    request.Proxy = ProxyParameters.wProxy;

                WebResponse webResponse = request.GetResponse();
                string response = "";
                using (Stream webStream = webResponse.GetResponseStream())
                {
                    if (webStream != null)
                    {
                        using (StreamReader responseReader = new StreamReader(webStream))
                        {
                            response = responseReader.ReadToEnd();
                        }
                    }
                }
                return JsonConvert.DeserializeObject<DeletedContactList>(response);
            }
            catch (ContactListNotFoundException)
            {
                throw;
            }
            catch(System.Net.WebException e)
            {
                WebResponse webResponse = e.Response;
                string response = "";
                using (Stream webStream = webResponse.GetResponseStream())
                {
                    if (webStream != null)
                    {
                        using (StreamReader responseReader = new StreamReader(webStream))
                        {
                            response = responseReader.ReadToEnd();
                        }
                    }
                }
                try
                {
                    GenesysErrorResponse obj = JsonConvert.DeserializeObject<GenesysErrorResponse>(response);
                    if (obj.code == cfg.GetConfigValue("JsonDTresNFound"))
                    {
                        ContactListNotFoundException clnf = new ContactListNotFoundException();
                        clnf.errResponse = obj;
                        throw new ContactListNotFoundException(obj.message);
                    }
                    return null;
                }
                catch (ContactListNotFoundException clnfe)
                {
                    throw clnfe;
                }
                catch (Exception)
                {
                    throw e;
                }
            }
            //return false;
        }

        public static GenesysErrorResponse GetErrorResponse(WebException we)
        {
            try
            {
                WebResponse webResponse = we.Response;
                using (Stream webStream = webResponse.GetResponseStream())
                {
                    if (webStream != null)
                    {
                        using (StreamReader responseReader = new StreamReader(webStream))
                        {
                            string response = responseReader.ReadToEnd();
                            return JsonConvert.DeserializeObject<GenesysErrorResponse>(response);
                        }
                    }
                }
                return new GenesysErrorResponse(-101, "Unknown", "Unknown/Genesys not-defined Error Response", "");
            }
            catch(Exception)
            {
                throw we;
            }
        }

        public static CampaignContactList AddContacts(CampaignContactList ccl)
        {
            try
            {
                // Get first contact contact list ID as contact list ID
                string contactListID = ccl.cc[0].contactListId;
                if (!Exists(contactListID))
                    throw new ContactListNotFoundException(GenesysInt.Properties.Resources.NFNDCONTLIST + contactListID);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(cfg.GetConfigValue("MainURI") + cfg.GetConfigValue("AddContact2List_1") + contactListID + cfg.GetConfigValue("AddContact2List_2"));
                request.Method = "POST";
                if (ProxyParameters.useProxy)
                    request.Proxy = ProxyParameters.wProxy;
                request.Headers.Add("Authorization", "Bearer " + AccessToken); //cfg.GetConfigValue("AuthToken"));
                request.ContentType = "application/json";
                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    string jsonData = ccl.ToJson();
                    streamWriter.Write(jsonData);
                }
                WebResponse webResponse = request.GetResponse();
                string response = "";
                using (Stream webStream = webResponse.GetResponseStream())
                {
                    if (webStream != null)
                    {
                        using (StreamReader responseReader = new StreamReader(webStream))
                        {
                            response = responseReader.ReadToEnd();
                        }
                    }
                }
                return CampaignContactList.Deserialize(response);
            }
            catch (WebException we)
            {
                GenesysErrorResponse ger = GetErrorResponse(we);
                DisplayGenesysException(ger);
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static void DisplayGenesysException(GenesysErrorResponse e)
        {
            Console.WriteLine("Genesys Error Code: " + e.code);
            Console.WriteLine("Genesys Error Status: " + e.status);
            Console.WriteLine("Genesys Error Message: " + e.message);
        }

        public static void DisplayContactList(PureCloudPlatform.Client.V2.Model.ContactList cl)
        {
            Console.WriteLine("ID : " + cl.Id);
            Console.WriteLine("Name : " + cl.Name);
            Console.WriteLine("SelfUri : " + cl.SelfUri);
            Console.WriteLine("Size : " + cl.Size);
            Console.WriteLine("Version : " + cl.Version);
            Console.WriteLine("Date Created : " + cl.DateCreated);
            Console.WriteLine("Date Modified : " + cl.DateModified);
            Console.WriteLine("Preview Mode Column Name : " + cl.PreviewModeColumnName);
            Console.WriteLine("Zip Code Column Name : " + cl.ZipCodeColumnName);
        }

        public static ContactList CreateContactList(PureCloudPlatform.Client.V2.Model.ContactList cList)
        {
            try
            {
                if (Exists(cList))
                    throw new DuplicateContactListException();
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(cfg.GetConfigValue("CreateContactList"));
                request.Method = "POST";
                if (ProxyParameters.useProxy)
                    request.Proxy = ProxyParameters.wProxy;
                request.Headers.Add("Authorization", "Bearer " + AccessToken); //cfg.GetConfigValue("AuthToken"));
                request.ContentType = "application/json";
                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(cList.ToJson());
                }
                WebResponse webResponse = request.GetResponse();
                string response = "";
                using (Stream webStream = webResponse.GetResponseStream())
                {
                    if (webStream != null)
                    {
                        using (StreamReader responseReader = new StreamReader(webStream))
                        {   
                            response = responseReader.ReadToEnd();
                        }
                    }
                }
                return JsonConvert.DeserializeObject<ContactList>(response);
            }
            catch (DuplicateContactListException)
            {
                Logger.Write(GenesysInt.Properties.Resources.DUPCONTLST + cList.Name);
                throw;
            }
            catch (Exception e)
            {
                Logger.Write(GenesysInt.Properties.Resources.FAILCRCONTLST + e.Message);
                Logger.Flush();
                return null;
            }
        }

        public static PureCloudPlatform.Client.V2.Model.ContactList UpdateContactList(string contlstID, ContactListUpdate cu)
        {
            try
            {
                if (!Exists(contlstID))
                    throw new ContactListNotFoundException(GenesysInt.Properties.Resources.NFNDCONTLIST + contlstID);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(cfg.GetConfigValue("MainURI") + cfg.GetConfigValue("UpdContactList") + contlstID);
                request.Method = "PUT";
                if (ProxyParameters.useProxy)
                    request.Proxy = ProxyParameters.wProxy;
                request.Headers.Add("Authorization", "Bearer " + AccessToken); //cfg.GetConfigValue("AuthToken"));
                request.ContentType = "application/json";
                cu.version++;  // Auto Increase contact version
                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(JsonConvert.SerializeObject(cu));
                }
                WebResponse webResponse = request.GetResponse();
                string response = "";
                using (Stream webStream = webResponse.GetResponseStream())
                {
                    if (webStream != null)
                    {
                        using (StreamReader responseReader = new StreamReader(webStream))
                        {
                            response = responseReader.ReadToEnd();
                        }
                    }
                }
                return JsonConvert.DeserializeObject<PureCloudPlatform.Client.V2.Model.ContactList>(response);
            }
            catch (ContactListNotFoundException)
            {
                throw;
            }
            catch(WebException e)
            {
                throw new GenesysException(GetErrorResponse(e));
            }
        }

        public static CampaignContact UpdateContact(CampaignContact cc, string contactID)
        {
            try
            {
                if (!Exists(cc.contactListId))
                    throw new ContactListNotFoundException(GenesysInt.Properties.Resources.NFNDCONTLIST + cc.contactListId);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(cfg.GetConfigValue("MainURI") + cfg.GetConfigValue("UpdateContact_1") + cc.contactListId + cfg.GetConfigValue("UpdateContact_2") + contactID);
                request.Method = "PUT";
                if (ProxyParameters.useProxy)
                    request.Proxy = ProxyParameters.wProxy;
                request.Headers.Add("Authorization", "Bearer " + AccessToken);//cfg.GetConfigValue("AuthToken"));
                request.ContentType = "application/json";

                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(JsonConvert.SerializeObject(cc));
                }
                WebResponse webResponse = request.GetResponse();
                string response = "";
                using (Stream webStream = webResponse.GetResponseStream())
                {
                    if (webStream != null)
                    {
                        using (StreamReader responseReader = new StreamReader(webStream))
                        {
                            response = responseReader.ReadToEnd();
                        }
                    }
                }
                return JsonConvert.DeserializeObject<CampaignContact>(response);
            }
            catch (ContactListNotFoundException)
            {
                throw;
            }
            catch (WebException e)
            {
                throw new GenesysException(GetErrorResponse(e));
            }
        }

        public static CampaignContact GetContact(CampaignContact cc, string contactID, string contactListId)
        {
            try
            {
                if (!Exists(contactListId))
                    throw new ContactListNotFoundException(GenesysInt.Properties.Resources.NFNDCONTLIST + contactListId);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(cfg.GetConfigValue("MainURI") + cfg.GetConfigValue("RetrieveContact_1") + contactListId + cfg.GetConfigValue("RetrieveContact_2") + contactID);
                request.Method = "GET";
                if (ProxyParameters.useProxy)
                    request.Proxy = ProxyParameters.wProxy;
                request.Headers.Add("Authorization", "Bearer " + AccessToken);//cfg.GetConfigValue("AuthToken"));
                request.ContentType = "application/json";

                //using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                //{
                //    streamWriter.Write(JsonConvert.SerializeObject(cc));
                //}
                WebResponse webResponse = request.GetResponse();
                string response = "";
                using (Stream webStream = webResponse.GetResponseStream())
                {
                    if (webStream != null)
                    {
                        using (StreamReader responseReader = new StreamReader(webStream))
                        {
                            response = responseReader.ReadToEnd();
                        }
                    }
                }
                return JsonConvert.DeserializeObject<CampaignContact>(response);
            }
            catch (ContactListNotFoundException)
            {
                throw;
            }
            catch (WebException e)
            {
                GenesysException ge = new GenesysException(GetErrorResponse(e));
                if(ge.errResponse.code== cfg.GetConfigValue("ContactIDNotFound"))
                {
                    Logger.Write(GenesysInt.Consts.logdelimiter + contactID + " was not found in Contact List ID: " + contactListId + ". No update can be done in Genesys.");
                    return null; 
                }
                throw ge;
            }
        }

        public static string GetToken()
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(cfg.GetConfigValue("AuthURI"));
                request.Method = "POST";
                if (ProxyParameters.useProxy)
                    request.Proxy = ProxyParameters.wProxy;
                string authenData = System.Convert.ToBase64String(
                                    System.Text.Encoding.GetEncoding("ISO-8859-1")
                                      .GetBytes(cfg.GetConfigValue("ClientID") + ":" + cfg.GetConfigValue("Secret"))
                                    );
                request.Headers.Add("Authorization", cfg.GetConfigValue("AuthenHead") + authenData);
                request.ContentType = "application/x-www-form-urlencoded";

                StringBuilder postString = new StringBuilder();
                postString.Append("grant_type=" + cfg.GetConfigValue("grant_type"));
                postString.Append("&accept=" + cfg.GetConfigValue("accept"));
                byte[] postData = new ASCIIEncoding().GetBytes(postString.ToString());
                //request.ContentLength = postData.Length;

                Stream reqStream = request.GetRequestStream();
                reqStream.Write(postData, 0, postData.Length);
                reqStream.Close();

                WebResponse webResponse = request.GetResponse();
                using (Stream webStream = webResponse.GetResponseStream())
                {
                    if (webStream != null)
                    {
                        using (StreamReader responseReader = new StreamReader(webStream))
                        {
                            string response = responseReader.ReadToEnd();
                            Token token = JsonConvert.DeserializeObject<Token>(response);
                            return token.access_token;
                        }
                    }
                }
                return GenesysInt.Properties.Resources.ERR_GET_TOKEN; // this line should not execute anyway. 
            }
            catch (Exception e)
            {
                Logger.Write(GenesysInt.Properties.Resources.ERR_GET_TOKEN);
                Logger.Write(e.Message);
                Logger.Flush();
                return GenesysInt.Properties.Resources.ERR_GET_TOKEN;
            }
        }
    }
}