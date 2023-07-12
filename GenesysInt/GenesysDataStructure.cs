using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Genesys
{
    [Serializable()]
    class Token
    {
        public string access_token;
        public string token_type;
        public int expires_in;
        public Token()
        {
            access_token = "";
            token_type = "";
            expires_in = 0;
        }
    }

    [Serializable()]
    class ContactList
    {
        public string id;
        public string name;
        public string dateCreated;
        public int version;
        public string[] columnNames;
        public PhoneColumns[] phoneColumns;
        public string previewModeColumnName;
        public string[] previewModeAcceptedValues;
        public bool automaticTimeZoneMapping;
        public string selfUri;

        [JsonConstructor]
        public ContactList(string mid = "", string mname = "", string dtcr = "", int ver = 0, string[] colNames = null,
            PhoneColumns[] phCols = null, string prevColNames = "", string[] prevAccVals = null, bool autoTZMap = false, string meUri = "")
        {
            id = mid;
            name = mname;
            dateCreated = dtcr;
            version = ver;
            columnNames = (colNames != null ? new List<string>(colNames).ToArray(): colNames);
            phoneColumns = (phCols != null ? new List<PhoneColumns>(phCols).ToArray(): phCols);
            previewModeColumnName = prevColNames;
            previewModeAcceptedValues = (prevAccVals != null ? new List<string>(prevAccVals).ToArray(): prevAccVals);
            automaticTimeZoneMapping = autoTZMap;
            selfUri = meUri;
        }

        public ContactList(ContactList cl)
        {
            id = cl.id;
            name = cl.name;
            dateCreated = cl.dateCreated;
            version = cl.version;
            columnNames = (cl.columnNames != null ? new List<string>(cl.columnNames).ToArray() : cl.columnNames);
            phoneColumns = (cl.phoneColumns != null ? new List<PhoneColumns>(cl.phoneColumns).ToArray() : cl.phoneColumns);
            previewModeColumnName = cl.previewModeColumnName;
            previewModeAcceptedValues = (cl.previewModeAcceptedValues != null ? new List<string>(cl.previewModeAcceptedValues).ToArray() : cl.previewModeAcceptedValues);
            automaticTimeZoneMapping = cl.automaticTimeZoneMapping;
            selfUri = cl.selfUri;
        }
    }

    [Serializable()]
    class ContactListEntity
    {
        public ContactList[] entities = null;
        public int pageSize = 0;
        public int pageNumber = 0;
        public int total = 0;
        public string firstUri = "";
        public string selfUri = "";
        public string previousUri = "";
        public string lastUri = "";
        public int pageCount = 0;
    }

    [Serializable()]
    class PhoneColumns
    {
        public string columnName = "";
        public string type = "";
    }

    [Serializable()]
    class OrgID
    {
        public string id = "";
        public string name = "";
    }

    [Serializable()]
    class OAuthOrgID
    {
        public string id = "";
        public string name = "";
        public OrgID organization = null;
    }

    [Serializable()]
    class OrganizationInfo
    {
        public OrgID organization = null;
        public OrgID homeOrganization = null;
        public OAuthOrgID OAuthClient = null;
    }

    [Serializable()]
    class StandardContact
    {
        public string subscriberid;
        public string contractno;
        public string name;
        public string phonenumber;
        public string smartcardid;
        public StandardContact(string sid = null, string contno = null, string nm=null, string phno=null, string scid = null)
        {
            subscriberid = sid;
            contractno = contno;
            name = nm;
            phonenumber = phno;
            smartcardid = scid;
        }
        public StandardContact(StandardContact sc)
        {
            subscriberid = sc.subscriberid;
            contractno = sc.contractno;
            name = sc.name;
            phonenumber = sc.phonenumber;
            smartcardid = sc.smartcardid;
        }
    }

    [Serializable()]
    class PhoneNumberStatus
    {
        // Empty class because no data definition found yet
        [JsonConstructor]
        public PhoneNumberStatus() { }
        // Empty class because no data definition found yet
        public PhoneNumberStatus(PhoneNumberStatus ps) { }
    }

    [Serializable()]
    class ContactListItemStandardContact
    {
        public string id;
        public string contactListId;
        public StandardContact data;
        public bool callable;
        public PhoneNumberStatus phoneNumberStatus; // Currently place holder

        public ContactListItemStandardContact(string mainid=null, string clid=null, StandardContact sc=null, bool ca=false, PhoneNumberStatus ps = null)
        {
            id = mainid;
            contactListId = clid;
            data = sc;
            callable = ca;
            phoneNumberStatus = ps;
        }
    }

    [Serializable()]
    class Contact
    {
        // Phone number session
        public string mobile1;
        public string mobile2;
        public string work1;
        public string work2;
        public string home1;

        public string campaign;
        public string subs_id;
        public int contract_id;
        public string subs_name;
        public string smartcard;
        public string township;
        public string insert_date;
        public string exclusion_date;
        public string ending_date;
        public string package_type;
        public string timezone;
        public double score_non_reabo;
        public int c_attempt;
        public string call_date;
        public string ending_call_date;
        public string result1;
        public string result2;
        public string result3;


        [JsonConstructor]
        public Contact(string m1=null, string m2=null, string w1=null, string w2=null, string h1=null, string cam=null, 
            string sid=null, int contr_id=1, string sname=null, string scid=null, string tshp=null, string insdt=null, string exdt=null, 
            string endt=null, string pkg=null, string tz= "Indian/Cocos (+6:30)", int catt=0, string cdate="", string ecdate="")
        {
            mobile1 = m1;
            mobile2 = m2;
            work1 = w1;
            work2 = w2;
            home1 = h1;
            campaign = cam;
            subs_id = sid;
            contract_id = contr_id; // Default Contract ID
            subs_name = sname;
            smartcard = scid;
            township = tshp;
            insert_date = insdt;
            exclusion_date = exdt;
            ending_date = endt;
            package_type = pkg;
            timezone = tz;
            c_attempt = catt;
            call_date = cdate;
            ending_call_date = ecdate;
        }

        public Contact(Contact c)
        {
            mobile1 = c.mobile1;
            mobile2 = c.mobile2;
            work1 = c.work1;
            work2 = c.work2;
            home1 = c.home1;
            campaign = c.campaign;
            subs_id = c.subs_id;
            contract_id = c.contract_id; // Contract ID from source to destination
            subs_name = c.subs_name;
            smartcard = c.smartcard;
            township = c.township;
            insert_date = c.insert_date;
            exclusion_date = c.exclusion_date;
            ending_date = c.ending_date;
            package_type = c.package_type;
            timezone = c.timezone;
            c_attempt = c.c_attempt;
            call_date = c.call_date;
            ending_call_date = c.ending_call_date;
        }
        public int SetCallAttemp (int catt) {
            return c_attempt = catt; 
        }
    }

    [Serializable()]
    class CampaignContact
    {
        public string id;
        public string contactListId;
        public Contact data;
        public bool callable;
        public PhoneNumberStatus phoneNumberStatus;

        [JsonConstructor]
        public CampaignContact(string mainid=null, string clid=null, Contact c=null, bool ca=false, PhoneNumberStatus ps = null)
        {
            id = mainid;
            contactListId = clid;
            if(c != null)
            {
                data = new Contact(c);
                data = c;
            }
            else
            {
                data = null;
            }
            callable = ca;
            phoneNumberStatus = ps;
        }
        public CampaignContact(CampaignContact c)
        {
            id = c.id;
            contactListId = c.contactListId;
            data = c.data;
            callable = c.callable;
            phoneNumberStatus = c.phoneNumberStatus;
        }
        /// <summary>
        /// Customized Json formatter since it was considered array in AddContact API function. 
        /// </summary>
        /// <returns>Json formatted string</returns>
        //public string ToJson()
        //{
        //    return "[" + JsonConvert.SerializeObject(this) + "]";
        //}
    }

    [Serializable()]
    class CampaignContactList
    {
        public List<CampaignContact> cc;

        public CampaignContactList(CampaignContact pc = null)
        {
            cc = new List<CampaignContact>(); // bug fix cga reflash
            if (pc != null)
            {
                cc.Add(pc);
            }
        }
        public CampaignContactList(CampaignContact[] ca)
        {
            if (ca != null)
            {
                cc = new List<CampaignContact>();
                for(int i=0; i < ca.Length; i++)
                {
                    cc.Add(ca[i]);
                }
            }
        }
        public CampaignContactList(List<CampaignContact> lst)
        {
            if (lst == null || lst.Count < 1) return;
            cc = new List<CampaignContact>();
            foreach (CampaignContact c in lst)
                cc.Add(c);
        }
        public string ToJson()
        {
            if(cc!=null)
                return JsonConvert.SerializeObject(cc);
            return string.Empty;
        }
        public static string Serialize(CampaignContactList c)
        {
            return c.ToJson();
        }
        public static CampaignContactList Deserialize(string s)
        {
            return new CampaignContactList(JsonConvert.DeserializeObject<List<CampaignContact>>(s));
        }
    }

    [Serializable()]
    class DeletedContactList
    {
        public string id;
        public string selfUri;
        public DeletedContactList(string dcid=null, string meUri = null)
        {
            id = dcid;
            selfUri = meUri;
        }
    }

    [Serializable()]
    class CallablePhoneColumn
    {
        public string columnName;
        public string type;
        public string callablecallableTimeColumn;

        [JsonConstructor]
        public CallablePhoneColumn(string colName="", string t="", string ctCol= "")
        {
            columnName = colName;
            type = t;
            callablecallableTimeColumn = ctCol;
        }
        public CallablePhoneColumn(CallablePhoneColumn c)
        {
            columnName = c.columnName;
            type = c.type;
            callablecallableTimeColumn = c.callablecallableTimeColumn;
        }
    }

    [Serializable()]
    class AttemptLimits
    {
        public string id;
        public string name;
        public string selfUri;

        [JsonConstructor]
        public AttemptLimits(string pid="", string pnm="", string meUri = "")
        {
            id = pid;
            name = pnm;
            selfUri = meUri;
        }
        public AttemptLimits(AttemptLimits a)
        {
            id = a.id;
            name = a.name;
            selfUri = a.selfUri;
        }
    }

    [Serializable()]
    class AttemptLimitsFull : AttemptLimits
    {

    }

    [Serializable()]
    class ContactListUpdate
    {
        public string name;
        public int version;
        public string[] columnNames;
        public CallablePhoneColumn[] phoneColumns;
        public string previewModeColumnName;
        public string[] previewModeAcceptedValues;
        public AttemptLimits attemptLimits;
        public bool automaticTimeZoneMapping;
        public string zipCodeColumnName;

        [JsonConstructor]
        public ContactListUpdate(string pnm ="", int ver=0, string[] colNames=null, CallablePhoneColumn[] cpc=null, string prevColNm="", string[] prevAccVals=null, AttemptLimits als=null, bool atzmap=false, string zipColNm= "")
        {
            name = pnm;
            version = ver;
            if(colNames!=null && colNames.Length > 0)
            {
                columnNames = new string[colNames.Length];
                for(int i=0; i<colNames.Length; i++)
                {
                    columnNames[i] = colNames[i];
                }
            }
            else
                columnNames = null;
            if (cpc != null && cpc.Length>0)
            {
                phoneColumns = new CallablePhoneColumn[cpc.Length];
                for(int i=0; i<cpc.Length; i++)
                {
                    phoneColumns[i] = cpc[i];
                }
            }
            else
                phoneColumns = null;
            previewModeColumnName = prevColNm;
            if (prevAccVals != null && prevAccVals.Length > 0)
            {
                for (int i = 0; i < prevAccVals.Length; i++)
                    previewModeAcceptedValues[i] = prevAccVals[i];
            }
            else
                previewModeAcceptedValues = null;
            if (als != null)
                attemptLimits = als;
            else
                attemptLimits = null;
            automaticTimeZoneMapping = atzmap;
            zipCodeColumnName = zipColNm;
        }
    }

    [Serializable()]
    class ContactUpdate
    {
        public string name;
        public string[] columnNames;
        public CallablePhoneColumn[] phoneColumns;
        public string previewModeColumnName;
        public string[] previewModeAcceptedValues;
        public AttemptLimits attemptLimits;
        public bool automaticTimeZoneMapping;
        public string zipCodeColumnName;

        [JsonConstructor]
        public ContactUpdate(string pnm = "", string[] colNames = null, CallablePhoneColumn[] cpc = null, string prevColNm = "", string[] prevAccVals = null, AttemptLimits als = null, bool atzmap = false, string zipColNm = "")
        {
            name = pnm;
            if (colNames != null && colNames.Length > 0)
            {
                columnNames = new string[colNames.Length];
                for (int i = 0; i < colNames.Length; i++)
                {
                    columnNames[i] = colNames[i];
                }
            }
            else
                columnNames = null;
            if (cpc != null && cpc.Length > 0)
            {
                phoneColumns = new CallablePhoneColumn[cpc.Length];
                for (int i = 0; i < cpc.Length; i++)
                {
                    phoneColumns[i] = cpc[i];
                }
            }
            else
                phoneColumns = null;
            previewModeColumnName = prevColNm;
            if (prevAccVals != null && prevAccVals.Length > 0)
            {
                for (int i = 0; i < prevAccVals.Length; i++)
                    previewModeAcceptedValues[i] = prevAccVals[i];
            }
            else
                previewModeAcceptedValues = null;
            if (als != null)
                attemptLimits = als;
            else
                attemptLimits = null;
            automaticTimeZoneMapping = atzmap;
            zipCodeColumnName = zipColNm;
        }
    }

    // Agent info
    [Serializable()]
    public class Division
    {
        public string id { get; set; }
        public string name { get; set; }
        public string selfUri { get; set; }
    }

    [Serializable()]
    public class Chat
    {
        public string jabberId { get; set; }
    }

    [Serializable()]
    public class PrimaryContactInfo
    {
        public string address { get; set; }
        public string mediaType { get; set; }
        public string type { get; set; }
    }

    [Serializable()]
    public class Agent
    {
        public string id { get; set; }
        public string name { get; set; }
        public Division division { get; set; }
        public Chat chat { get; set; }
        public string department { get; set; }
        public string email { get; set; }
        public List<PrimaryContactInfo> primaryContactInfo { get; set; }
        public List<object> addresses { get; set; } // Generic object needs to update. 
        public string state { get; set; }
        public string title { get; set; }
        public string username { get; set; }
        public int version { get; set; }
        public bool acdAutoAnswer { get; set; }
        public string selfUri { get; set; }
    }

    [Serializable()]
    public class AgentCC
    {
        public Agent agent { get; set; }
        public float numEvaluations { get; set; }
        public float averageEvaluationScore { get; set; }
        public float numCriticalEvaluations { get; set; }
        public float highestEvaluationScore { get; set; }
        public float lowestEvaluationScore { get; set; }

        public AgentCC() { }
        public AgentCC(AgentCC a)
        {
            agent = a.agent;
            numEvaluations = a.numEvaluations;
            numCriticalEvaluations = a.numCriticalEvaluations;
            highestEvaluationScore = a.highestEvaluationScore;
            lowestEvaluationScore = a.lowestEvaluationScore;
        }
    }

    [Serializable()]
    public class AgentCCList
    {
        public List<AgentCC> entities = null;
        public int pageSize;
        public int pageNumber;
        public long total;
        public string firstUri;
        public string selfUri;
        public string nextUri;
        public string lastUri;
        public int pageCount;
    }

    [Serializable()]
    public class ServiceLevel
    {
        public float percentage { get; set; }
        public int durationMs { get; set; }
    }

    [Serializable()]
    public class ServiceLevelByType
    {
        public int alertingTimeoutSeconds { get; set; }
        public ServiceLevel serviceLevel { get; set; }
    }

    [Serializable()]
    public class Call
    {
        public int alertingTimeoutSeconds { get; set; }
        public ServiceLevel serviceLevel { get; set; }
    }

    [Serializable()]
    public class SocialExpression
    {
        public int alertingTimeoutSeconds { get; set; }
        public ServiceLevel serviceLevel { get; set; }
    }

    [Serializable()]
    public class QChat
    {
        public int alertingTimeoutSeconds { get; set; }
        public ServiceLevel serviceLevel { get; set; }
    }

    [Serializable()]
    public class Callback
    {
        public int alertingTimeoutSeconds { get; set; }
        public ServiceLevel serviceLevel { get; set; }
    }

    [Serializable()]
    public class Message
    {
        public int alertingTimeoutSeconds { get; set; }
        public ServiceLevel serviceLevel { get; set; }
    }

    [Serializable()]
    public class VideoComm
    {
        public int alertingTimeoutSeconds { get; set; }
        public ServiceLevel serviceLevel { get; set; }
    }

    [Serializable()]
    public class Email
    {
        public int alertingTimeoutSeconds { get; set; }
        public ServiceLevel serviceLevel { get; set; }
    }

    [Serializable()]
    public class MediaSettings
    {
        public Call call { get; set; }
        public SocialExpression socialExpression { get; set; }
        public QChat chat { get; set; }
        public Callback callback { get; set; }
        public Message message { get; set; }
        public VideoComm videoComm { get; set; }
        public Email email { get; set; }
    }

    [Serializable()]
    public class AcwSettings
    {
        public string wrapupPrompt { get; set; }
        public int timeoutMs { get; set; }
    }

    [Serializable()]
    public class Script
    {
        public string id { get; set; }
        public string selfUri { get; set; }
    }

    [Serializable()]
    public class DefaultScripts
    {
        public Script CALL { get; set; }
    }

    [Serializable()]
    public class Queue
    {
        public string id { get; set; }
        public string name { get; set; }
        public Division division { get; set; }
        public DateTime dateModified { get; set; }
        public string modifiedBy { get; set; }
        public MediaSettings mediaSettings { get; set; }
        public AcwSettings acwSettings { get; set; }
        public string skillEvaluationMethod { get; set; }
        public bool autoAnswerOnly { get; set; }
        public DefaultScripts defaultScripts { get; set; }
        public int memberCount { get; set; }
        public string selfUri { get; set; }
    }

    [Serializable()]
    public class QueueList
    {
        public List<Queue> entities { get; set; }
        public int pageSize { get; set; }
        public int pageNumber { get; set; }
        public int total { get; set; }
        public string firstUri { get; set; }
        public string selfUri { get; set; }
        public string lastUri { get; set; }
        public int pageCount { get; set; }
    }

    [Serializable()]
    public class Image
    {
        public string resolution { get; set; }
        public string imageUri { get; set; }
    }

    [Serializable()]
    public class iEvaluator
    {
        public string id { get; set; }
        public string name { get; set; }
        public Division division { get; set; }
        public Chat chat { get; set; }
        public string department { get; set; }
        public string email { get; set; }
        public List<PrimaryContactInfo> primaryContactInfo { get; set; }
        public List<object> addresses { get; set; }
        public string state { get; set; }
        public string title { get; set; }
        public string username { get; set; }
        public List<Image> images { get; set; }
        public int version { get; set; }
        public bool acdAutoAnswer { get; set; }
        public string selfUri { get; set; }
    }

    [Serializable()]
    public class Evaluator
    {
        public iEvaluator evaluator { get; set; }
        public float numEvaluationsAssigned { get; set; }
        public float numEvaluationsStarted { get; set; }
        public float numEvaluationsCompleted { get; set; }
        public float numCalibrationsAssigned { get; set; }
        public float numCalibrationsStarted { get; set; }
        public float numCalibrationsCompleted { get; set; }
    }

    [Serializable()]
    public class EvaluatorList
    {
        public List<Evaluator> entities { get; set; }
        public int pageSize { get; set; }
        public int pageNumber { get; set; }
        public int total { get; set; }
        public string firstUri { get; set; }
        public string selfUri { get; set; }
        public string nextUri { get; set; }
        public string lastUri { get; set; }
        public int pageCount { get; set; }
    }

    [Serializable()]
    public class User
    {
        public string id { get; set; }
        public string name { get; set; }
        public Division division { get; set; }
        public Chat chat { get; set; }
        public string department { get; set; }
        public string email { get; set; }
        public List<PrimaryContactInfo> primaryContactInfo { get; set; }
        public List<object> addresses { get; set; }
        public string state { get; set; }
        public string title { get; set; }
        public string username { get; set; }
        public List<Image> images { get; set; }
        public int version { get; set; }
    }

    [Serializable()]
    public class QueueUser
    {
        public string id { get; set; }
        public string name { get; set; }
        public User user { get; set; }
        public int ringNumber { get; set; }
        public bool joined { get; set; }
        public string memberBy { get; set; }
    }

    [Serializable()]
    public class QueueUserList
    {
        public List<QueueUser> entities { get; set; }
        public int pageSize { get; set; }
        public int pageNumber { get; set; }
        public int total { get; set; }
        public string firstUri { get; set; }
        public string selfUri { get; set; }
        public string lastUri { get; set; }
        public int pageCount { get; set; }
    }
}