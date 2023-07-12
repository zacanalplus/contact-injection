using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenesysInt
{
    [Serializable()]
    class SContactList
    {
        public string id = "";
        public string name = "";
        public string dateCreated = "";
        public int version = 0;
        public string[] columnNames = null;
        public PhoneColumns[] phoneColumns = null;
        public string previewModeColumnName = "";
        public string[] previewModeAcceptedValues = null;
        public bool automaticTimeZoneMapping = false;
        public string selfUri = "";

    }
    [Serializable()]
    class CLEntity
    {
        public SContactList[] entities = null;
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
}
