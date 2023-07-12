using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Zlibg;
using System.IO;
using PureCloudPlatform.Client.V2.Model;
using Newtonsoft.Json;
using System.Configuration;
using Konsole;

namespace GenesysInt
{
    class Program
    {
        public static AppConfigurator cfg = new AppConfigurator(System.Reflection.Assembly.GetExecutingAssembly().Location);
        public const string OracleErr_01858 = "ORA-01858";

        static void doInclusionExclusion()
        {
            Logger.Write(Consts.logdelimiter + Properties.Resources.BENCHMRKST + DbEnvironment.context);
            Logger.Flush();
            Oradb persistConn = new Oradb(); //// Performance tuning with persisting connection
            persistConn.InitializePersist(); //// Performance tuning with persisting connection

            List<Genesys.CampaignContact> cca = Oradb.getAllCampaignContact(cfg.GetConfigValue("IncQRY"));
            if (cca.Count < 1)
            {
                Logger.Write(Properties.Resources.ZEROINSDATA + DbEnvironment.context);
                Logger.Flush();
            }
            else
            {
                // One by One Inclusion
                ProgressBar inPb = new ProgressBar(PbStyle.DoubleLine, cca.Count);
                Genesys.CampaignContactList updated_ccl = new Genesys.CampaignContactList();
                long cupCnt = 0;
                foreach (Genesys.CampaignContact cc in cca)
                {
                    cc.contactListId = cfg.GetConfigValue("ContactListID");
                    // One by One Inclusion
                    Genesys.CampaignContactList ccl = new Genesys.CampaignContactList(cc);
                    Genesys.CampaignContactList newOne = Genesys.APIs.AddContacts(ccl); 
                    cupCnt += newOne.cc.Count;
                    foreach(Genesys.CampaignContact c in newOne.cc) // bug fix cga reflection
                    {
                        updated_ccl.cc.Add(c); 
                    }
                    inPb.Refresh((int)cupCnt, "Processing contact in contact list");
                    // End One by One Inclusion
                }
                // Bulk Inclusion
                //Genesys.CampaignContactList ccl = new Genesys.CampaignContactList(cca);
                //Genesys.CampaignContactList updated_ccl = Genesys.APIs.AddContacts(ccl);
                //Logger.Write(" ==> " + updated_ccl.cc.Count + " contacts was loaded to Genesys. ");
                // End Bulk Inclusion

                // One by One Inclusion
                Logger.Write(Consts.logdelimiter + cupCnt + " contacts was loaded to Genesys. ");
                inPb.Refresh((int)cupCnt, "Inclusion Done! ");

                long newIns = 0;
                foreach (Genesys.CampaignContact cc in updated_ccl.cc)
                {
                    if (!persistConn.p_Exists(cc.id)) // Performance tuning with persisting connection
                    {
                        persistConn.p_InsertContact(cc);
                        newIns++;
                    }
                }
                Logger.Write(Consts.logdelimiter + newIns + " new contacts was pushed to local Genesys database. ");
            }

            cca.Clear();
            cca = Oradb.getAllCampaignContact(cfg.GetConfigValue("ExcQRY"));
            if (cca.Count < 1)
            {
                Logger.Write(Properties.Resources.ZEROEXCDATA + DbEnvironment.context);
            }
            else
            {
                ProgressBar exPb = new ProgressBar(PbStyle.DoubleLine, cca.Count);
                long updCnt = 0;
                foreach (Genesys.CampaignContact cc in cca)
                {
                    string contactId = persistConn.p_GetGenesysID(cc.data.subs_id); // Performance tuning with persisting connection

                    cc.contactListId = cfg.GetConfigValue("ContactListID");
                    cc.callable = cfg.GetConfigValue("ExclusionBehavior") == "Inclusion"; // Excluded
                    if (contactId == "")
                    {
                        Logger.Write(Consts.logdelimiter + "No associated contactid for subscriber id " + cc.data.subs_id + " found in Genesys, " + cfg.GetConfigValue("ExclusionBehavior") + " not applied. ");
                    }
                    else
                    {
                        Genesys.CampaignContact gcc = Genesys.APIs.GetContact(cc, contactId, cfg.GetConfigValue("ContactListID"));
                        if(gcc != null)
                        {
                            Genesys.CampaignContact updated_cc = Genesys.APIs.UpdateContact(cc, contactId);
                            updCnt++;
                            exPb.Refresh((int)updCnt, "Excluding contact from contact list");
                        }
                    }
                }
                Logger.Write(Consts.logdelimiter + updCnt + " contacts was " + (cfg.GetConfigValue("ExclusionBehavior") == "Inclusion" ? "included" : "excluded") + " from callable list. ");
                exPb.Refresh((int)updCnt, "Done exclusion!");
            }
            persistConn.DeInitializePersist();
            Logger.Write(Consts.logdelimiter + Properties.Resources.BENCHMRKEN + DbEnvironment.context);
        }

        static void collectAgentData()
        {
            //Genesys.AgentCCList ccAgtList = Genesys.APIs.ListAgentActivitySummary();
            Genesys.AgentCCList ccAgtList = Genesys.APIs.ListByAllPage("ListAgentAct");
            Logger.Write(Consts.logdelimiter + ccAgtList.entities.Count + @" agents activity summary data collected in d:\logs\agtactsum.json");
            File.WriteAllText(@"d:\logs\agtactsum.json", JsonConvert.SerializeObject(ccAgtList, Formatting.Indented));
        }

        static void collectQueueData()
        {
            //Genesys.AgentCCList ccAgtList = Genesys.APIs.ListAgentActivitySummary();
            Genesys.QueueList ccAgtList = JsonConvert.DeserializeObject<Genesys.QueueList>(Genesys.APIs.GenericAPICall("ListQueues"));
            Logger.Write(Consts.logdelimiter + ccAgtList.entities.Count + @" queue(s) data collected in d:\logs\agtactsum.json");
            File.WriteAllText(@"d:\logs\agtactsum.json", JsonConvert.SerializeObject(ccAgtList, Formatting.Indented));
        }

        static void collectQueueDataDetails(string queueId)
        {
            Genesys.Queue ccAgtList = JsonConvert.DeserializeObject<Genesys.Queue>(Genesys.APIs.GenericAPICall2(cfg.GetConfigValue("ListQueues") + "/" + queueId));
            Logger.Write(Consts.logdelimiter + @" queue data details collected in d:\logs\queuedet.json");
            File.WriteAllText(@"d:\logs\queuedet.json", JsonConvert.SerializeObject(ccAgtList, Formatting.Indented));
        }

        static void collectEvaluatorData()
        {
            //Genesys.AgentCCList ccAgtList = Genesys.APIs.ListAgentActivitySummary();
            Genesys.EvaluatorList ccAgtList = JsonConvert.DeserializeObject<Genesys.EvaluatorList>(Genesys.APIs.GenericAPICall("ListEvalAct"));
            Logger.Write(Consts.logdelimiter + ccAgtList.entities.Count + @" evaluator(s) data collected in d:\logs\agtactsum.json");
            File.WriteAllText(@"d:\logs\agtactsum.json", JsonConvert.SerializeObject(ccAgtList, Formatting.Indented));
        }

        static void collectQueueUserData(string queueId)
        {
            //Genesys.AgentCCList ccAgtList = Genesys.APIs.ListAgentActivitySummary();
            Genesys.QueueUserList ccAgtList = JsonConvert.DeserializeObject<Genesys.QueueUserList>(Genesys.APIs.GenericAPICall2(cfg.GetConfigValue("ListQueueUsers_1") + queueId + cfg.GetConfigValue("ListQueueUsers_2")));
            Logger.Write(Consts.logdelimiter + ccAgtList.entities.Count + @" queue user(s) data collected in d:\logs\agtactsum.json");
            File.WriteAllText(@"d:\logs\qusers.json", JsonConvert.SerializeObject(ccAgtList, Formatting.Indented));
        }

        static void deleteContactInList()
        {
            if (Genesys.APIs.ClearContactInList(cfg.GetConfigValue("ContactListID")))
                Logger.Write(Consts.logdelimiter + Properties.Resources.CLEAROK + cfg.GetConfigValue("ContactListID"));
        }

        static void deleteContactInList(string contLstId)
        {
            if (Genesys.APIs.ClearContactInList(contLstId))
                Logger.Write(Consts.logdelimiter + Properties.Resources.CLEAROK + contLstId);
        }

        static void Main(string[] args)
        {
            bool clearContactList = false;
            if (args.Length > 0 && args[0] == cfg.GetConfigValue("CmdlineClearContactList"))
                clearContactList = true;
            Logger.Open(cfg.GetConfigValue("logpath"));
            Logger.Debugging = (cfg.GetConfigValue("logging") == "1");
            LogLevels.Debug = (cfg.GetConfigValue("DebugLog") == "1");

            DbEnvironment.context = cfg.GetConfigValue("Environment");
            
            DbEnvironment.dbCols = cfg.GetConfigValue("RSETORDER").Split(',').ToList<string>();
            if (DbEnvironment.dbCols.Count < 1)
            {
                throw new Genesys.DataExtractionFormatException("No data format definition from Database, empty column list. Please check configuration.");
            }
            ProxyParameters.netCredAuthType = cfg.GetConfigValue("NetCredAuthType");
            ProxyParameters.wProxy = getProxy(LoadProxyConfigs(cfg.GetConfigValue("ApiProxyId")));
            ProxyParameters.behindProxy = (cfg.GetConfigValue("BehindProxy").ToLower() == "yes" ? true : false);
            LoadDbConfigs(DbEnvironment.context);

            if (clearContactList)
            {
                deleteContactInList();
                Console.WriteLine("Contact list id : " + cfg.GetConfigValue("ContactListID") + " was cleared. Please do a fresh inclusion/exclusion to use it. ");
            }
            else
            {
                doInclusionExclusion();
            }
            
            Logger.Flush(); // write all information lines
        }

        static void DisplayMe()
        {
            OrganizationInfo oInfo = GetMe();
            Console.WriteLine("organization-id: " + oInfo.organization.id);
            Console.WriteLine("organization-name: " + oInfo.organization.name);
            Console.WriteLine("home organization-id: " + oInfo.homeOrganization.id);
            Console.WriteLine("home organization-name: " + oInfo.homeOrganization.name);
            Console.WriteLine("OAuth-id: " + oInfo.OAuthClient.id);
            Console.WriteLine("OAuth-name: " + oInfo.OAuthClient.name);
            Console.WriteLine("OAuth organization-id: " + oInfo.OAuthClient.organization.id);
            Console.WriteLine("OAuth organization-name: " + oInfo.OAuthClient.organization.name);
        }

        static void mDisplayMe()
        {
            Genesys.OrganizationInfo oInfo = Genesys.APIs.GetMe();
            Console.WriteLine("organization-id: " + oInfo.organization.id);
            Console.WriteLine("organization-name: " + oInfo.organization.name);
            Console.WriteLine("home organization-id: " + oInfo.homeOrganization.id);
            Console.WriteLine("home organization-name: " + oInfo.homeOrganization.name);
            Console.WriteLine("OAuth-id: " + oInfo.OAuthClient.id);
            Console.WriteLine("OAuth-name: " + oInfo.OAuthClient.name);
            Console.WriteLine("OAuth organization-id: " + oInfo.OAuthClient.organization.id);
            Console.WriteLine("OAuth organization-name: " + oInfo.OAuthClient.organization.name);
        }

        static bool Exists(ContactList cl)
        {
            CLEntity ce = ListContactList();
            if (ce == null) return false;
            foreach (SContactList ecl in ce.entities)
            {
                if (ecl.id == cl.Id || ecl.name == cl.Name)
                    return true;
            }
            return false;
        }

        static void LoadDbConfigs(string context)
        {
            if (!string.IsNullOrEmpty(context))
            {
                DataConnectionSection ds = (DataConnectionSection)ConfigurationManager.GetSection(DataConnectionSection.SectionName);
                if (ds != null)
                {
                    foreach (EndpointElement ele in ds.ConnectionManagerEndpoints)
                    {
                        if (ele.Name == context)
                        {
                            DbEnvironment.DbKey = ele.Address;
                            DbEnvironment.magicKey = ele.Key;
                        }
                    }
                }
            }
        }

        public static ProxyParameters LoadProxyConfigs(string proxyId)
        {
            ProxySection ps = (ProxySection)ConfigurationManager.GetSection(ProxySection.SectionName);
            if (ps != null)
            {
                foreach (ProxyInfo ele in ps.ProxyTables)
                {
                    if (ele.Name == proxyId)
                        return new ProxyParameters(ele.Uri, ele.Domain, ele.User, ele.Password, ele.UrisViaProxy);
                }
            }
            return null;
        }

        public static WebProxy getProxy(ProxyParameters pp)
        {
            if (pp!=null)
            {
                NetworkCredential nc = UseProxy(pp);
                WebProxy proxy = new WebProxy(new Uri(pp.uri));
                proxy.Credentials = nc;
                return proxy;
            }
            return null;
        }

        public static NetworkCredential UseProxy(ProxyParameters pp)
        {
            try
            {
                NetworkCredential myCred = null;
                if (ProxyParameters.netCredAuthType == "Basic")
                {
                    myCred = new NetworkCredential(pp.user, pp.password, pp.domain);
                }
                else if (ProxyParameters.netCredAuthType == "NTLM")
                {
                    myCred = CredentialCache.DefaultNetworkCredentials;
                }

                CredentialCache myCache = new CredentialCache();

                foreach (string gUri in pp.urls)
                    myCache.Add(new Uri(gUri), ProxyParameters.netCredAuthType, myCred);
                return myCred;
            }
            catch (Exception e)
            {
                throw new Exception(Properties.Resources.INVPROXY + e.Message);
            }
        }

        static string CreateContactList()
        {
            WebResponse webResponse;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(cfg.GetConfigValue("CreateContactList"));
                request.Method = "POST";
                if (ProxyParameters.useProxy)
                    request.Proxy = ProxyParameters.wProxy;
                request.Headers.Add("Authorization", "Bearer " + cfg.GetConfigValue("AuthToken"));
                //request.Headers.Add("Content-Type", "application/json");
                request.ContentType = "application/json";
                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    List<string> cols = new List<string>();
                    cols.Add("subscriberID");
                    cols.Add("contractno");
                    cols.Add("phonenumber");

                    List<ContactPhoneNumberColumn> phcol = new List<ContactPhoneNumberColumn>();
                    ContactPhoneNumberColumn c1 = new ContactPhoneNumberColumn("phonenumber", "string");
                    phcol.Add(c1);
                    ContactList cList = new ContactList("CplusMain", 1, cols, phcol);
                    string json = cList.ToJson();
                    streamWriter.Write(json);
                }


                webResponse = request.GetResponse();
                using (Stream webStream = webResponse.GetResponseStream())
                {
                    if (webStream != null)
                    {
                        using (StreamReader responseReader = new StreamReader(webStream))
                        {
                            string response = responseReader.ReadToEnd();
                            Console.Out.WriteLine(response);
                            return response;
                        }
                    }
                }
                throw new Exception("Unknown");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return e.Message;
            }
        }

        private static string LoadUserInformation()
        {
            WebResponse webResponse;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(cfg.GetConfigValue("MainURI") + cfg.GetConfigValue("Me"));
                request.Method = "GET";
                if (ProxyParameters.useProxy)
                    request.Proxy = ProxyParameters.wProxy;
                request.Headers.Add("Authorization", "Bearer " + cfg.GetConfigValue("AuthToken"));
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
                            Console.Out.WriteLine(response);
                        }
                    }
                }
                return response;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return e.Message;
            }
        }

        private static OrganizationInfo GetMe()
        {
            WebResponse webResponse;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(cfg.GetConfigValue("MainURI") + cfg.GetConfigValue("Me"));
                request.Method = "GET";
                if (ProxyParameters.useProxy)
                    request.Proxy = ProxyParameters.wProxy;
                request.Headers.Add("Authorization", "Bearer " + cfg.GetConfigValue("AuthToken"));
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
                Console.WriteLine(e.Message);
                return null;
            }
        }

        private static string CreateContactList(ContactList cList)
        {
            try
            {
                if (Exists(cList))
                    return Properties.Resources.DUPCONTLST;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(cfg.GetConfigValue("CreateContactList"));
                request.Method = "POST";
                if (ProxyParameters.useProxy)
                    request.Proxy = ProxyParameters.wProxy;
                request.Headers.Add("Authorization", "Bearer " + cfg.GetConfigValue("AuthToken"));
                request.ContentType = "application/json";
                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(cList.ToJson());
                }
                WebResponse webResponse = request.GetResponse();
                using (Stream webStream = webResponse.GetResponseStream())
                {
                    if (webStream != null)
                    {
                        using (StreamReader responseReader = new StreamReader(webStream))
                        {
                            string response = responseReader.ReadToEnd();
                            return response;
                        }
                    }
                }
                return "Create contact list was not successful."; // this line should not execute anyway. 
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
        private static string ListContactListTEST()
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(cfg.GetConfigValue("MainURI") + cfg.GetConfigValue("ListContactList"));
                request.Method = "GET";
                if (ProxyParameters.useProxy)
                    request.Proxy = ProxyParameters.wProxy;
                request.Headers.Add("Authorization", "Bearer " + cfg.GetConfigValue("AuthToken"));
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
                            Console.Out.WriteLine(response);
                        }
                    }
                }
                CLEntity cList =
                JsonConvert.DeserializeObject<CLEntity>(response);

                Console.WriteLine(cList.entities[0].phoneColumns[0].columnName, cList.entities[0].phoneColumns[0].type);


                return response;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return e.Message;
            }
        }
        private static CLEntity ListContactList()
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(cfg.GetConfigValue("MainURI") + cfg.GetConfigValue("ListContactList"));
                request.Method = "GET";
                if (ProxyParameters.useProxy)
                    request.Proxy = ProxyParameters.wProxy;
                request.Headers.Add("Authorization", "Bearer " + cfg.GetConfigValue("AuthToken"));
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
                return JsonConvert.DeserializeObject<CLEntity>(response);
            }
            catch (Exception e)
            {
                Logger.Write(e.Message);
                Logger.Flush();
                return null;
            }
        }
        private static string GetTokenTest()
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
                            Console.Out.WriteLine(response);
                            PCToken token = JsonConvert.DeserializeObject<PCToken>(response);
                            return token.access_token;
                        }
                    }
                }
                return Properties.Resources.ERR_GET_TOKEN; // this line should not execute anyway. 
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return e.Message;
            }
        }

        private static string GetToken()
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
                            PCToken token = JsonConvert.DeserializeObject<PCToken>(response);
                            return token.access_token;
                        }
                    }
                }
                return Properties.Resources.ERR_GET_TOKEN; // this line should not execute anyway. 
            }
            catch (Exception e)
            {
                Logger.Write(Properties.Resources.ERR_GET_TOKEN);
                Logger.Write(e.Message);
                Logger.Flush();
                return Properties.Resources.ERR_GET_TOKEN;
            }
        }
    }
}
