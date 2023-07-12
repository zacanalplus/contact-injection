using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;

namespace GenesysInt
{
    class Consts
    {
        public static string logdelimiter = " => ";
    }
    class LogLevels
    {
        public static bool Debug { get; set; }
    }
    class DbEnvironment
    {
        public static string context { get; set; }
        public static string DbKey { get; set; }
        public static string magicKey
        {
            /*get
            {
                return context == "Production" ? Properties.Resources.PKEY : Properties.Resources.TKEY;
            }*/
            get; set;
        }
        public static List<string> dbCols;
    }
    class ColumnInfo
    {
        public string name { get; private set; }
        public string type { get; private set; }
        public ColumnInfo(string cname, string ctype)
        {
            name = cname;
            type = ctype;
        }
        public bool isString()
        {
            return type.ToLower().Contains("string");
        }
        public bool isInt()
        {
            return type.ToLower().Contains("int32") || type.ToLower().Contains("int64") || type.ToLower().Contains("int16");
        }
        public bool isDecimal()
        {
            return type.ToLower().Contains("decimal");
        }
    }
    class TableInfo
    {
        private List<ColumnInfo> columns;
        public bool OK { get; private set; }
        public TableInfo(DataTable dt)
        {
            LoadTableInfo(dt);
        }
        private void LoadTableInfo(DataTable dt)
        {
            if (dt == null || dt.Rows.Count < 1)
                return;
            columns = new List<ColumnInfo>();
            foreach (DataRow dr in dt.Rows)
            {
                columns.Add(new ColumnInfo(dr["ColumnName"].ToString(), dr["ColumnType"].ToString()));
            }
        }
        public bool exists(string columnName)
        {
            foreach(ColumnInfo ci in columns)
            {
                if (ci.name.ToLower() == columnName.ToLower())
                    return true;
            }
            return false;
        }
        public ColumnInfo getColumn(string columnName)
        {
            foreach (ColumnInfo ci in columns)
            {
                if (ci.name.ToLower() == columnName.ToLower())
                    return ci;
            }
            return null;
        }
        public bool isTypeString(string columnName)
        {
            foreach (ColumnInfo ci in columns)
            {
                if (ci.name.ToLower() == columnName.ToLower() && ci.isString())
                    return true;
            }
            return false;
        }
        public bool isTypeInteger(string columnName)
        {
            foreach (ColumnInfo ci in columns)
            {
                if (ci.name.ToLower() == columnName.ToLower() && ci.isInt())
                    return true;
            }
            return false;
        }
        public bool isTypeDouble(string columnName)
        {
            foreach (ColumnInfo ci in columns)
            {
                if (ci.name.ToLower() == columnName.ToLower() && ci.isDecimal())
                    return true;
            }
            return false;
        }
        public TableInfo(OracleDataReader rd)
        {
            try
            {
                if (rd != null && rd.FieldCount > 0 && rd.HasRows)
                {
                    LoadTableInfo(rd.GetSchemaTable());
                }
            }
            catch (Exception)
            {
                OK = false;
            }
        }
    }
    class MetaData
    {
        public bool OK { get; private set; }
        private TableInfo tInf;
        
        public MetaData(OracleDataReader rd)
        {
            try
            {
                if (rd != null && rd.FieldCount > 0 && rd.HasRows)
                {
                    tInf = new TableInfo(rd.GetSchemaTable());
                }
            }
            catch (Exception)
            {
                OK = false;
            }
        }
        public string type(string columnName)
        {
            return tInf.getColumn(columnName).type;
        }
    }
    class Oradb
    {
        private OracleConnection pcon = null; // Persist connection for definite period

        public bool InitializePersist()
        {
            pcon = new OracleConnection(ConnectionString);
            return pcon.State == System.Data.ConnectionState.Open;
        }
        public bool ReInitializePersist()
        {
            bool connected = false;
            if (pcon.State != System.Data.ConnectionState.Open) // re-initialize persist
            {
                pcon.Open();
                connected = (pcon.State == System.Data.ConnectionState.Open);
            }
            return connected;
        }
        public bool p_Exists(string contactId)
        {
            try
            {
                ReInitializePersist();
                // Execute a SQL SELECT
                OracleCommand cmd = pcon.CreateCommand();
                cmd.CommandText = GenesysInt.Properties.Resources.CONTIDQRY;
                cmd.Parameters.Add("genesys_id", contactId);
                cmd.Parameters.Add("contactlistid", Program.cfg.GetConfigValue("ContactListID"));
                OracleDataReader reader = cmd.ExecuteReader();

                int val = 0;
                while (reader.Read())
                    val += reader.GetInt16(0);

                // Clean up
                reader.Dispose();
                return val == 1 ? true : false;
            }
            catch (Exception e)
            {
                throw new DataLoadException("Error in checking contact id " + contactId + " " + e.Message + " : Environment " + DbEnvironment.context);
            }
        }

        public int p_InsertContact(Genesys.CampaignContact d)
        {
            OracleCommand updCmd = null;
            try
            {
                ReInitializePersist();
                string cmdTxt = Properties.Resources.INSGENID;

                updCmd = new OracleCommand(cmdTxt, pcon);

                updCmd.Parameters.Add(new OracleParameter("genesys_id", d.id));
                updCmd.Parameters.Add(new OracleParameter("numabo", d.data.subs_id));
                updCmd.Parameters.Add(new OracleParameter("contactlistid", d.contactListId));

                return updCmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw new DbException(DbEnvironment.context + " - " + e.Message + " * Genesys ID = " + d.id + ", Contact List ID = " + d.contactListId);
                //return 0;
            }
        }

        public int p_InsertAgentActSummary(Genesys.AgentCC c)
        {
            OracleCommand updCmd = null;
            try
            {
                ReInitializePersist();
                string cmdTxt = Properties.Resources.INSGENID;

                updCmd = new OracleCommand(cmdTxt, pcon);

                updCmd.Parameters.Add(new OracleParameter("genesys_id", c.agent.id));
                updCmd.Parameters.Add(new OracleParameter("numabo", c.agent.name));
                updCmd.Parameters.Add(new OracleParameter("contactlistid", c.agent.email));

                return updCmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw new DbException(DbEnvironment.context + " - " + e.Message);
                //return 0;
            }
        }

        public string p_GetGenesysID(string subscriberID)
        {
            // Connect
            try
            {
                ReInitializePersist();
                // Execute a SQL SELECT
                OracleCommand cmd = pcon.CreateCommand();
                cmd.CommandText = GenesysInt.Properties.Resources.GENIDQRY;
                cmd.Parameters.Add("numabo", subscriberID);
                cmd.Parameters.Add("contactlistid", Program.cfg.GetConfigValue("ContactListID"));
                OracleDataReader reader = cmd.ExecuteReader();

                string val = "";
                while (reader.Read())
                    val = reader.GetString(0);

                // Clean up
                reader.Dispose();
                cmd.Dispose();

                return val;
            }
            catch (Exception e)
            {
                throw new DataLoadException("Error in getting Genesys ID for subscriber id " + subscriberID + " " + e.Message + " : Environment " + DbEnvironment.context);
            }
        }

        public void DeInitializePersist()
        {
            if (pcon.State != System.Data.ConnectionState.Closed)
                pcon.Close();
            pcon.Dispose();
        }

        public static string ConnectionString
        {
            get
            {
                //if (DbEnvironment.context != "Production")
                //{
                //    return DevConnectionString;
                //}
                Simpenc eng = new Simpenc();
                //return eng.decode("S2J1bylVdsKBeGtnPy1KR1RHVk1QYVdTT0YrS05KV1FcWkswXllQW1hHXVJKXkNbMTRSV1pWSTk+Pi44NzRAMzc9MTJVW1RcPz1APT0wLy5MVVBaUE9YZk5GVENEM1RIWV5ORUdkU0tWRkJXwoBieHB4eG9taHUyLC87VXNxciFvbkNaSUNYV0lTQXNqeXzChXd8ZEd1bjbCg3tkcsKALHo4Zz5lYFRzRw==", Properties.Resources.PKEY);
                return eng.decode(DbEnvironment.DbKey, DbEnvironment.magicKey);
            }
        }
        public static string DevConnectionString
        {
            get
            {
                Simpenc eng = new Simpenc();
                return eng.decode("UmfCgGQhU3x6eXBzSihRUV5NYE5WVEtVVEc1SUhPVEdgYEs1UFdaXlJDUkxLV1BRMTVUWGFhPTxDQDM6NDQ9Nzw9KShQT15ZSjQ3ODsrLytPWlxTUkpXX1FDVUJLLFlQWF9SRkdrTklOS0hXwoRrbm1seDIvLz5ff2xyInZxSF1NTlpgR1o9cGF9eXt1fWtAT3sqdnktYD8=", Properties.Resources.TKEY);
            }
        }
        public static int DbOK
        {
            get; set;
        }

        public static List<Genesys.CampaignContact> getAllCampaignContact(string viewName)
        {
            List<Genesys.CampaignContact> dList = new List<Genesys.CampaignContact>();

            // Connect
            try
            {
                OracleConnection con = new OracleConnection(Oradb.ConnectionString);
                con.Open();

                // Execute a SQL SELECT
                OracleCommand cmd = con.CreateCommand();
                cmd.CommandText = GenesysInt.Properties.Resources.ALLQRY + " " + viewName;
                OracleDataReader reader = cmd.ExecuteReader();
                //TableInfo tInf = new TableInfo(reader);

                while (reader.Read())
                {
                    Genesys.Contact d = new Genesys.Contact();

                    for(int i=0; i < DbEnvironment.dbCols.Count; i++)
                    {
                        if (!reader.IsDBNull(reader.GetOrdinal(DbEnvironment.dbCols[i])))
                        {
                            switch (DbEnvironment.dbCols[i].ToLower())
                            {
                                case "mobile1":
                                    {
                                        //if(tInf.isTypeString(DbEnvironment.dbCols[i]))
                                            d.mobile1 = reader.GetString(reader.GetOrdinal(DbEnvironment.dbCols[i]));
                                        break;
                                    }
                                case "mobile2":
                                    {
                                        //if (tInf.isTypeString(DbEnvironment.dbCols[i]))
                                            d.mobile2 = reader.GetString(reader.GetOrdinal(DbEnvironment.dbCols[i]));
                                        break;
                                    }
                                case "work1":
                                    {
                                        //if (tInf.isTypeString(DbEnvironment.dbCols[i]))
                                            d.work1 = reader.GetString(reader.GetOrdinal(DbEnvironment.dbCols[i]));
                                        break;
                                    }
                                case "work2":
                                    {
                                        //if (tInf.isTypeString(DbEnvironment.dbCols[i]))
                                            d.work2 = reader.GetString(reader.GetOrdinal(DbEnvironment.dbCols[i]));
                                        break;
                                    }
                                case "home1":
                                    {
                                        //if (tInf.isTypeString(DbEnvironment.dbCols[i]))
                                            d.home1 = reader.GetString(reader.GetOrdinal(DbEnvironment.dbCols[i]));
                                        break;
                                    }
                                case "campaign":
                                    {
                                        //if (tInf.isTypeString(DbEnvironment.dbCols[i]))
                                            d.campaign = reader.GetString(reader.GetOrdinal(DbEnvironment.dbCols[i]));
                                        break;
                                    }
                                case "subs_id":
                                    {
                                        //if (tInf.isTypeString(DbEnvironment.dbCols[i]))
                                            d.subs_id = reader.GetInt32(reader.GetOrdinal(DbEnvironment.dbCols[i])).ToString();
                                        break;
                                    }
                                case "contract_id":
                                    {
                                        //if (tInf.isTypeString(DbEnvironment.dbCols[i]))
                                            d.contract_id = reader.GetInt32(reader.GetOrdinal(DbEnvironment.dbCols[i]));
                                        break;
                                    }
                                case "subs_name":
                                    {
                                        //if (tInf.isTypeString(DbEnvironment.dbCols[i]))
                                            d.subs_name = reader.GetString(reader.GetOrdinal(DbEnvironment.dbCols[i]));
                                        break;
                                    }
                                case "smartcard":
                                    {
                                        //if (tInf.isTypeString(DbEnvironment.dbCols[i]))
                                            d.smartcard = reader.GetString(reader.GetOrdinal(DbEnvironment.dbCols[i]));
                                        break;
                                    }
                                case "township":
                                    {
                                        //if (tInf.isTypeString(DbEnvironment.dbCols[i]))
                                            d.township = reader.GetString(reader.GetOrdinal(DbEnvironment.dbCols[i]));
                                        break;
                                    }
                                case "insert_date":
                                    {
                                        //if (tInf.isTypeString(DbEnvironment.dbCols[i]))
                                            d.insert_date = reader.GetString(reader.GetOrdinal(DbEnvironment.dbCols[i]));
                                        break;
                                    }
                                case "exclusion_date":
                                    {
                                        //if (tInf.isTypeString(DbEnvironment.dbCols[i]))
                                            d.exclusion_date = reader.GetString(reader.GetOrdinal(DbEnvironment.dbCols[i]));
                                        break;
                                    }
                                case "ending_date":
                                    {
                                        //if (tInf.isTypeString(DbEnvironment.dbCols[i]))
                                            d.ending_date = reader.GetString(reader.GetOrdinal(DbEnvironment.dbCols[i]));
                                        break;
                                    }
                                case "package_type":
                                    {
                                        //if (tInf.isTypeString(DbEnvironment.dbCols[i]))
                                            d.package_type = reader.GetString(reader.GetOrdinal(DbEnvironment.dbCols[i]));
                                        break;
                                    }
                                case "timezone":
                                    {
                                        //if (tInf.isTypeString(DbEnvironment.dbCols[i]))
                                            d.timezone = reader.GetString(reader.GetOrdinal(DbEnvironment.dbCols[i]));
                                        break;
                                    }
                                case "score_non_reabo":
                                    {
                                        //if (tInf.isTypeString(DbEnvironment.dbCols[i]))
                                            d.score_non_reabo = (double)reader.GetDecimal(reader.GetOrdinal(DbEnvironment.dbCols[i]));
                                        break;
                                    }
                                case "c_attempt":
                                    {
                                        //if (tInf.isTypeString(DbEnvironment.dbCols[i]))
                                            d.c_attempt = Int32.Parse(reader.GetString(reader.GetOrdinal(DbEnvironment.dbCols[i])));
                                        break;
                                    }
                                case "call_date":
                                    {
                                        //if (tInf.isTypeString(DbEnvironment.dbCols[i]))
                                            d.call_date = reader.GetString(reader.GetOrdinal(DbEnvironment.dbCols[i]));
                                        break;
                                    }
                                case "ending_call_date":
                                    {
                                        //if (tInf.isTypeString(DbEnvironment.dbCols[i]))
                                            d.ending_call_date = reader.GetString(reader.GetOrdinal(DbEnvironment.dbCols[i]));
                                        break;
                                    }
                                case "result1":
                                    {
                                        d.result1 = reader.GetString(reader.GetOrdinal(DbEnvironment.dbCols[i]));
                                        break;
                                    }
                                case "result2":
                                    {
                                        d.result2 = reader.GetString(reader.GetOrdinal(DbEnvironment.dbCols[i]));
                                        break;
                                    }
                                case "result3":
                                    {
                                        d.result3 = reader.GetString(reader.GetOrdinal(DbEnvironment.dbCols[i]));
                                        break;
                                    }
                            }
                        }
                    }

                    //if (!reader.IsDBNull(reader.GetOrdinal(DbEnvironment.dbCols[0])))
                    //    d.mobile1 = reader.GetString(reader.GetOrdinal(DbEnvironment.dbCols[0]));
                    //if (!reader.IsDBNull(reader.GetOrdinal(DbEnvironment.dbCols[1])))
                    //    d.mobile2 = reader.GetString(reader.GetOrdinal(DbEnvironment.dbCols[1]));
                    //if (!reader.IsDBNull(reader.GetOrdinal(DbEnvironment.dbCols[2])))
                    //    d.work1 = reader.GetString(reader.GetOrdinal(DbEnvironment.dbCols[2]));
                    //if (!reader.IsDBNull(reader.GetOrdinal(DbEnvironment.dbCols[3])))
                    //    d.work2 = reader.GetString(reader.GetOrdinal(DbEnvironment.dbCols[3]));
                    //if (!reader.IsDBNull(reader.GetOrdinal(DbEnvironment.dbCols[4])))
                    //    d.home1 = reader.GetString(reader.GetOrdinal(DbEnvironment.dbCols[4]));
                    //if (!reader.IsDBNull(reader.GetOrdinal(DbEnvironment.dbCols[5])))
                    //    d.campaign = reader.GetString(reader.GetOrdinal(DbEnvironment.dbCols[5]));
                    //if (!reader.IsDBNull(reader.GetOrdinal(DbEnvironment.dbCols[6])))
                    //    d.subs_id = reader.GetInt32(reader.GetOrdinal(DbEnvironment.dbCols[6])).ToString();
                    //if (!reader.IsDBNull(reader.GetOrdinal(DbEnvironment.dbCols[7])))
                    //    d.contract_id = reader.GetInt32(reader.GetOrdinal(DbEnvironment.dbCols[7]));
                    //if (!reader.IsDBNull(reader.GetOrdinal(DbEnvironment.dbCols[8])))
                    //    d.subs_name = reader.GetString(reader.GetOrdinal(DbEnvironment.dbCols[8]));
                    //if (!reader.IsDBNull(reader.GetOrdinal(DbEnvironment.dbCols[9])))
                    //    d.smartcard = reader.GetString(reader.GetOrdinal(DbEnvironment.dbCols[9]));
                    //if (!reader.IsDBNull(reader.GetOrdinal(DbEnvironment.dbCols[10])))
                    //    d.township = reader.GetString(reader.GetOrdinal(DbEnvironment.dbCols[10]));
                    //if (!reader.IsDBNull(reader.GetOrdinal(DbEnvironment.dbCols[11])))
                    //    d.insert_date = reader.GetString(reader.GetOrdinal(DbEnvironment.dbCols[11]));
                    //if (!reader.IsDBNull(reader.GetOrdinal(DbEnvironment.dbCols[12])))
                    //    d.exclusion_date = reader.GetString(reader.GetOrdinal(DbEnvironment.dbCols[12]));
                    //if (!reader.IsDBNull(reader.GetOrdinal(DbEnvironment.dbCols[13])))
                    //    d.ending_date = reader.GetString(reader.GetOrdinal(DbEnvironment.dbCols[13]));
                    //if (!reader.IsDBNull(reader.GetOrdinal(DbEnvironment.dbCols[14])))
                    //    d.package_type = reader.GetString(reader.GetOrdinal(DbEnvironment.dbCols[14]));
                    //if (!reader.IsDBNull(reader.GetOrdinal(DbEnvironment.dbCols[15])))
                    //    d.timezone = reader.GetString(reader.GetOrdinal(DbEnvironment.dbCols[15]));
                    //if (!reader.IsDBNull(reader.GetOrdinal(DbEnvironment.dbCols[16])))
                    //    d.score_non_reabo = reader.GetDouble(reader.GetOrdinal(DbEnvironment.dbCols[16]));
                    //if (!reader.IsDBNull(reader.GetOrdinal(DbEnvironment.dbCols[17])))
                    //    d.c_attempt = Int32.Parse(reader.GetString(reader.GetOrdinal(DbEnvironment.dbCols[17])));
                    //if (!reader.IsDBNull(reader.GetOrdinal(DbEnvironment.dbCols[18])))
                    //    d.call_date = reader.GetString(reader.GetOrdinal(DbEnvironment.dbCols[18]));
                    //if (!reader.IsDBNull(reader.GetOrdinal(DbEnvironment.dbCols[19])))
                    //    d.ending_call_date = reader.GetString(reader.GetOrdinal(DbEnvironment.dbCols[19]));

                    Genesys.CampaignContact cc = new Genesys.CampaignContact("", "", d, true, null);
                    dList.Add(cc);
                }

                // Clean up
                reader.Dispose();
                cmd.Dispose();
                con.Dispose();
                // return 1 means OK
                DbOK = 1;
                return dList;
            }
            catch (InvalidCastException)
            {
                throw new Genesys.DataExtractionFormatException();
            }
            catch (Exception e)
            {
                // Nah... something bad about connecting Oracle
                if (e.Message.Contains(Program.OracleErr_01858))
                    throw new Genesys.DataExtractionFormatException("View name " + viewName + " is not correct column format/order. It need to comply current inclusion/exclusion format " + Program.cfg.GetConfigValue("RSETORDER"));
                DbOK = 0;
                throw new NoDataFoundException(GenesysInt.Properties.Resources.EMPTYDATA + DbEnvironment.context + " -> " + e.Message);
                //return e.Message;
            }
        }

        public static void ConnectivityTest()
        {
            // Connect
            try
            {
                OracleConnection con = new OracleConnection(Oradb.ConnectionString);
                con.Open();

                // Execute a SQL SELECT
                OracleCommand cmd = con.CreateCommand();
                cmd.CommandText = "select 1 from dual";
                OracleDataReader reader = cmd.ExecuteReader();

                int val = 0;
                while (reader.Read())
                    val += reader.GetInt16(0);

                // Clean up
                reader.Dispose();
                cmd.Dispose();
                con.Dispose();
                // return 1 means OK
                DbOK = 1;
                
            }
            catch (Exception e)
            {
                // Nah... something bad about connecting Oracle
                DbOK = 0;
                throw new DataLoadException("Error in loading data, " + e.Message + " : Environment " + DbEnvironment.context);
                //throw new DbNotConnectedException(GenesysInt.Properties.Resources.DBNOTCONNECTED + " : Environment " + DbEnvironment.context);
                //return e.Message;
            }
        }

        public static bool Exists(string contactId)
        {
            // Connect
            try
            {
                OracleConnection con = new OracleConnection(Oradb.ConnectionString);
                con.Open();

                // Execute a SQL SELECT
                OracleCommand cmd = con.CreateCommand();
                cmd.CommandText = GenesysInt.Properties.Resources.CONTIDQRY;
                cmd.Parameters.Add("genesys_id", contactId);
                cmd.Parameters.Add("contactlistid", Program.cfg.GetConfigValue("ContactListID"));
                OracleDataReader reader = cmd.ExecuteReader();

                int val = 0;
                while (reader.Read())
                    val += reader.GetInt16(0);

                // Clean up
                reader.Dispose();
                cmd.Dispose();
                con.Dispose();
                
                return val==1?true:false;

            }
            catch (Exception e)
            {
                throw new DataLoadException("Error in checking contact id " + contactId + " " + e.Message + " : Environment " + DbEnvironment.context);
            }
        }

        public static string GetGenesysID(string subscriberID)
        {
            // Connect
            try
            {
                OracleConnection con = new OracleConnection(Oradb.ConnectionString);
                con.Open();

                // Execute a SQL SELECT
                OracleCommand cmd = con.CreateCommand();
                cmd.CommandText = GenesysInt.Properties.Resources.GENIDQRY;
                cmd.Parameters.Add("numabo", subscriberID);
                cmd.Parameters.Add("contactlistid", Program.cfg.GetConfigValue("ContactListID"));
                OracleDataReader reader = cmd.ExecuteReader();

                string val = "";
                while (reader.Read())
                    val = reader.GetString(0);

                // Clean up
                reader.Dispose();
                cmd.Dispose();
                con.Dispose();

                return val;

            }
            catch (Exception e)
            {
                throw new DataLoadException("Error in getting Genesys ID for subscriber id " + subscriberID + " " + e.Message + " : Environment " + DbEnvironment.context);
            }
        }

        public static int InsertContact(Genesys.CampaignContact d)
        {
            OracleCommand updCmd = null;
            OracleConnection con = null;
            try
            {
                string cmdTxt = Properties.Resources.INSGENID;
                con = new OracleConnection(Oradb.ConnectionString);
                con.Open();

                updCmd = new OracleCommand(cmdTxt, con);

                updCmd.Parameters.Add(new OracleParameter("genesys_id", d.id));
                updCmd.Parameters.Add(new OracleParameter("numabo", d.data.subs_id));
                updCmd.Parameters.Add(new OracleParameter("contactlistid", d.contactListId));

                return updCmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw new DbException(DbEnvironment.context + " - " + e.Message);
                //return 0;
            }
            finally
            {
                con.Close();
                con.Dispose();
            }
        }

        public static int InsertGQueue(Genesys.Queue d, string insQry)
        {
            OracleCommand updCmd = null;
            OracleConnection con = null;
            try
            {
                string cmdTxt = insQry;
                con = new OracleConnection(Oradb.ConnectionString);
                con.Open();

                updCmd = new OracleCommand(cmdTxt, con);

                updCmd.Parameters.Add(new OracleParameter("id", d.id));
                updCmd.Parameters.Add(new OracleParameter("name", d.name));
                updCmd.Parameters.Add(new OracleParameter("modifiedBy", d.modifiedBy));
                updCmd.Parameters.Add(new OracleParameter("dateModified", d.dateModified));
                updCmd.Parameters.Add(new OracleParameter("memberCount", d.memberCount));

                return updCmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw new DbException(DbEnvironment.context + " - " + e.Message);
                //return 0;
            }
            finally
            {
                con.Close();
                con.Dispose();
            }
        }


        public static List<Genesys.Contact> getAllData(string viewName)
        {
            List<Genesys.Contact> dList = new List<Genesys.Contact>();

            // Connect
            try
            {
                OracleConnection con = new OracleConnection(Oradb.ConnectionString);
                con.Open();

                // Execute a SQL SELECT
                OracleCommand cmd = con.CreateCommand();
                cmd.CommandText = GenesysInt.Properties.Resources.ALLQRY + " " + viewName;
                OracleDataReader reader = cmd.ExecuteReader();


                while (reader.Read())
                {
                    Genesys.Contact d = new Genesys.Contact();
                    if (!reader.IsDBNull(0))
                        d.mobile1 = reader.GetString(0);
                    if(!reader.IsDBNull(1))
                        d.mobile2 = reader.GetString(1);
                    if (!reader.IsDBNull(2))
                        d.work1 = reader.GetString(2);
                    if (!reader.IsDBNull(3))
                        d.work2 = reader.GetString(3);
                    if (!reader.IsDBNull(4))
                        d.home1 = reader.GetString(4);
                    if (!reader.IsDBNull(5))
                        d.campaign = reader.GetString(5);
                    if (!reader.IsDBNull(6))
                        d.subs_id = reader.GetString(6);
                    if (!reader.IsDBNull(7))
                        d.subs_name = reader.GetString(7);
                    if (!reader.IsDBNull(8))
                        d.smartcard = reader.GetString(8);
                    if (!reader.IsDBNull(9))
                        d.township = reader.GetString(9);
                    if (!reader.IsDBNull(10))
                        d.insert_date = reader.GetString(10);
                    if (!reader.IsDBNull(11))
                        d.exclusion_date = reader.GetString(11);
                    if (!reader.IsDBNull(12))
                        d.ending_date = reader.GetString(12);
                    if (!reader.IsDBNull(13))
                        d.package_type = reader.GetString(13);
                    if (!reader.IsDBNull(14))
                        d.timezone = reader.GetString(14);
                    if (!reader.IsDBNull(15))
                        d.c_attempt = reader.GetInt32(15);
                    if (!reader.IsDBNull(16))
                        d.call_date = reader.GetString(16);
                    if (!reader.IsDBNull(17))
                        d.ending_call_date = reader.GetString(17);
                    dList.Add(d);
                }


                // Clean up
                reader.Dispose();
                cmd.Dispose();
                con.Dispose();
                // return 1 means OK
                DbOK = 1;
                return dList;
            }
            catch (Exception)
            {
                // Nah... something bad about connecting Oracle
                DbOK = 0;
                throw new NoDataFoundException(GenesysInt.Properties.Resources.EMPTYDATA + DbEnvironment.context);
                //return e.Message;
            }
            //return dList;
        }
    }

    class Simpenc
    {
        private int minValue = 0;
        private int maxValue = 15;
        public string magic { get; set; }
        public string gen_magic(int len)
        {
            string mgstr = "";
            Random rnd = new Random(DateTime.Now.Millisecond);
            for (int i = 0; i < len; i++)
            {
                mgstr += rnd.Next(minValue, maxValue).ToString("X");
            }
            return mgstr;
        }
        public string code(string text)
        {
            if (text == "") return "";
            magic = gen_magic(text.Length);
            string etxt = "";
            for (int i = 0; i < text.Length; i++)
            {
                int mchar = HexToInt(magic[i]);
                if ((char.MaxValue - text[i]) < mchar)
                    throw new CharOverflowException();
                etxt += (char)(text[i] + mchar);
            }
            byte[] btmp = Encoding.UTF8.GetBytes(etxt);
            return Convert.ToBase64String(btmp);
        }
        public string code(string text, string magic)
        {
            if (text == "") return "";

            string etxt = "";
            for (int i = 0; i < text.Length; i++)
            {
                int mchar = HexToInt(magic[i]);
                if ((char.MaxValue - text[i]) < mchar)
                    throw new CharOverflowException();
                etxt += (char)(text[i] + mchar);
            }
            byte[] btmp = Encoding.UTF8.GetBytes(etxt);
            return Convert.ToBase64String(btmp);
        }
        public int HexToInt(char hexChar)
        {
            hexChar = char.ToUpper(hexChar);

            return (int)hexChar < (int)'A' ?
                ((int)hexChar - (int)'0') :
                10 + ((int)hexChar - (int)'A');
        }
        public string decode(string text)
        {
            byte[] btmp = Convert.FromBase64String(text);
            string ttext = Encoding.UTF8.GetString(btmp);
            string dtxt = "";
            for (int i = 0; i < ttext.Length; i++)
            {
                int mchar = HexToInt(magic[i]);
                if ((char.MinValue + mchar) > ttext[i])
                    throw new CharUnderflowException();
                dtxt += (char)(ttext[i] - mchar);
            }
            return dtxt;
        }
        public string decode(string text, string magic)
        {
            byte[] btmp = Convert.FromBase64String(text);
            string ttext = Encoding.UTF8.GetString(btmp);
            string dtxt = "";
            for (int i = 0; i < ttext.Length; i++)
            {
                int mchar = HexToInt(magic[i]);
                if ((char.MinValue + mchar) > ttext[i])
                    throw new CharUnderflowException();
                dtxt += (char)(ttext[i] - mchar);
            }
            return dtxt;
        }
    }
    class CharOverflowException : Exception
    {
        public CharOverflowException() : base(Properties.Resources.CHAROVRFLOW) { }
        public CharOverflowException(string msg) : base(msg) { }
        public CharOverflowException(string msg, Exception inner) : base(msg, inner) { }
    }
    class CharUnderflowException : Exception
    {
        public CharUnderflowException() : base(Properties.Resources.CHARUNDFLOW) { }
        public CharUnderflowException(string msg) : base(msg) { }
        public CharUnderflowException(string msg, Exception inner) : base(msg, inner) { }
    }

    class DbException : Exception
    {
        public DbException() : base(Properties.Resources.DBEXCEPTION) { }
        public DbException(string msg) : base(Properties.Resources.DBEXCEPTION + msg) { }
        public DbException(string msg, Exception inner) : base(msg, inner) { }
    }

    class DbNotConnectedException : DbException
    {
        public DbNotConnectedException() : base(Properties.Resources.DBNOTCONNECTED) { }
        public DbNotConnectedException(string msg) : base(msg) { }
        public DbNotConnectedException(string msg, Exception inner) : base(msg, inner) { }
    }

    class NoDataFoundException : DbException
    {
        public NoDataFoundException() : base(Properties.Resources.EMPTYDATA) { }
        public NoDataFoundException(string msg) : base(msg) { }
        public NoDataFoundException(string msg, Exception inner) : base(msg, inner) { }
    }

    class DataLoadException : DbException
    {
        public DataLoadException() : base(Properties.Resources.EMPTYDATA) { }
        public DataLoadException(string msg) : base(msg) { }
        public DataLoadException(string msg, Exception inner) : base(msg, inner) { }
    }
}
