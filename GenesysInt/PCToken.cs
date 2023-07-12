using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace GenesysInt
{
    [Serializable()]
    class PCToken
    {
        public string access_token;
        public string token_type;
        public int expires_in;
        public PCToken()
        {
            access_token = "";
            token_type = "";
            expires_in = 0;
            
        }
    }

    public class ProxySection : ConfigurationSection
    {

        public const string SectionName = "ProxySection";

        private const string ProxyEntryName = "Proxy";

        [ConfigurationProperty(ProxyEntryName)]
        [ConfigurationCollection(typeof(ProxyCollection), AddItemName = "add")]
        public ProxyCollection ProxyTables { get { return (ProxyCollection)base[ProxyEntryName]; } }
    }

    public class ProxyInfo : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get { return (string)this["name"]; }
            set { this["name"] = value; }
        }

        [ConfigurationProperty("domain", IsRequired = true)]
        public string Domain
        {
            get { return (string)this["domain"]; }
            set { this["domain"] = value; }
        }

        [ConfigurationProperty("uri", IsRequired = true)]
        public string Uri
        {
            get { return (string)this["uri"]; }
            set { this["uri"] = value; }
        }

        [ConfigurationProperty("user", IsRequired = true)]
        public string User
        {
            get { return (string)this["user"]; }
            set { this["user"] = value; }
        }

        [ConfigurationProperty("password", IsRequired = true)]
        public string Password
        {
            get { return (string)this["password"]; }
            set { this["password"] = value; }
        }

        [ConfigurationProperty("urisViaProxy", IsRequired = true)]
        public string UrisViaProxy
        {
            get { return (string)this["urisViaProxy"]; }
            set { this["urisViaProxy"] = value; }
        }
    }

    public class ProxyCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new ProxyInfo();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ProxyInfo)element).Name;
        }
    }

    public class ProxyParameters
    {
        public string uri { get; set; }
        public string user { get; set; }
        public string password { get; set; }
        public List<string> urls { get; set; }
        public string domain { get; set; }

        public static bool behindProxy = false;
        public static WebProxy wProxy = null;
        public static string netCredAuthType { get; set; }

        public static bool useProxy
        {
            get
            {
                return (behindProxy && wProxy != null);
            }
        }

        public ProxyParameters(string puri, string proxyDomain, string proxyUser, string proxyPassword, string urlsViaProxy)
        {
            uri = puri;
            user = proxyUser;
            password = proxyPassword;
            urls = urlsViaProxy.Split(',').ToList<string>();
            domain = proxyDomain;
        }
    }

    public class DataConnectionSection : ConfigurationSection
    {

        public const string SectionName = "DataConnectionSection";

        private const string EndpointCollectionName = "DataEndpoints";

        [ConfigurationProperty(EndpointCollectionName)]
        [ConfigurationCollection(typeof(DataEndpointsCollection), AddItemName = "add")]
        public DataEndpointsCollection ConnectionManagerEndpoints { get { return (DataEndpointsCollection)base[EndpointCollectionName]; } }
    }

    public class DataEndpointsCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new EndpointElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((EndpointElement)element).Name;
        }
    }

    public class EndpointElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get { return (string)this["name"]; }
            set { this["name"] = value; }
        }

        [ConfigurationProperty("address", IsRequired = true)]
        public string Address
        {
            get { return (string)this["address"]; }
            set { this["address"] = value; }
        }

        [ConfigurationProperty("key", IsRequired = false)]
        public string Key
        {
            get { return (string)this["key"]; }
            set { this["key"] = value; }
        }
    }

    public class OraTableCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new OraTableElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((OraTableElement)element).Name;
        }
    }

    public class OraTablesSection : ConfigurationSection
    {

        public const string SectionName = "OracleTablesSection";

        private const string OraTableCollectionName = "OracleTables";

        [ConfigurationProperty(OraTableCollectionName)]
        [ConfigurationCollection(typeof(OraTableCollection), AddItemName = "add")]
        public OraTableCollection OracleTables { get { return (OraTableCollection)base[OraTableCollectionName]; } }
    }

    public class OraTableElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get { return (string)this["name"]; }
            set { this["name"] = value; }
        }

        [ConfigurationProperty("columns", IsRequired = true)]
        public string Columns
        {
            get { return (string)this["columns"]; }
            set { this["columns"] = value; }
        }

        [ConfigurationProperty("keycolumns", IsRequired = true)]
        public string KeyColumns
        {
            get { return (string)this["keycolumns"]; }
            set { this["keycolumns"] = value; }
        }

        [ConfigurationProperty("IsVarchar", IsRequired = false, DefaultValue = false)]
        public bool IsVarcharType
        {
            get { return (bool)this["IsVarchar"]; }
            set { this["IsVarchar"] = value; }
        }
    }

    class PlsqlQueryBuilder
    {
        public const string paramDelimiter = ":";
        public const string selectPrefix = "SELECT 1 FROM ";
        public const string insertPrefix = "INSERT INTO ";
        public const string updatePrefix = "UPDATE ";

        public bool QueryOK { get; private set; }
        public string TableName { get; private set; }
        public List<string> Columns { get; private set; }
        public List<string> KeyColumns { get; private set; }
        private List<string> shadowCols { get; set; }
        public string ExistQry { get; private set; }
        public string InsertQry { get; private set; }
        public string UpdateQry { get; private set; }

        private string PrepareInsQry()
        {
            return InsertQry = insertPrefix + TableName + " (" + string.Join(",", Columns.ToArray()) + ") VALUES (" + string.Join(",", shadowCols.ToArray()) + ")";
        }
        private string PrepareSelQry()
        {
            return ExistQry = selectPrefix + TableName + " WHERE " + whereClause();

        }
        private string PrepareUpdQry()
        {
            return UpdateQry = updatePrefix + TableName + " SET " + updateClause() + " WHERE " + whereClause();
        }
        private string whereClause()
        {
            string clause = KeyColumns[0] + " = " + paramDelimiter + KeyColumns[0];
            for (int i = 1; i < KeyColumns.Count; i++)
            {
                clause += " AND " + KeyColumns[i] + " = " + paramDelimiter + KeyColumns[i];
            }
            return clause;
        }
        bool isKeyColumn(string chkCol)
        {
            foreach (string kCol in KeyColumns)
            {
                if (chkCol == kCol)
                    return true;
            }
            return false;
        }
        private string updateClause()
        {
            //string clause = Columns[0] + " = " + paramDelimiter + Columns[0];
            //for(int i= 1; i<Columns.Count; i++)
            //{
            //    clause += ", " + Columns[i] + " = " + paramDelimiter + Columns[i];
            //}
            // Only non-key columns will be updated 21 Mar 2019
            string clause = "";
            for (int i = 0; i < Columns.Count; i++)
            {
                string updClause = "";
                if (!isKeyColumn(Columns[i]))
                {
                    updClause = Columns[i] + " = " + paramDelimiter + Columns[i];
                }
                if (!string.IsNullOrEmpty(updClause))
                {
                    clause += updClause;
                    if (i != Columns.Count - 1)
                        clause += ", ";
                }

            }
            return clause;
        }

        public PlsqlQueryBuilder(string tblName, string colNames, string keyCols)
        {
            try
            {
                TableName = tblName;
                Columns = colNames.Split(',').ToList<string>();
                KeyColumns = keyCols.Split(',').ToList<string>();
                if (QueryOK = SanityCheck())
                {
                    shadowCols = new List<string>();
                    foreach (string s in Columns)
                        shadowCols.Add(paramDelimiter + s);
                    PrepareSelQry();
                    PrepareInsQry();
                    PrepareUpdQry();
                }
            }
            catch (Exception)
            {
                QueryOK = false;
            }
        }
        private bool SanityCheck()
        {
            int cnt = 0;
            foreach (string s in KeyColumns)
            {
                if (Columns.Contains<string>(s))
                    cnt++;
            }
            return cnt == KeyColumns.Count;
        }
    }
}
