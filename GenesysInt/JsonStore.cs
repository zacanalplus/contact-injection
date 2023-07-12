using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace GenesysInt
{
    class JsonStore
    {
        private string spath;
        private StreamWriter file;

        public JsonStore(string jsonStorePath)
        {
            Open(jsonStorePath);
            spath = jsonStorePath;
                //throw new JsonStorePathNotFoundException();
        }

        private void Open(string logpath)
        {
            string p = Path.GetDirectoryName(logpath);
            if (!Directory.Exists(p))
            {
                Directory.CreateDirectory(p);
            }
            if (logpath.Length > 0)
            {
                file = new System.IO.StreamWriter(logpath, true);
                file.AutoFlush = true;
            }
        }

        public void SerializeSeq(List<Genesys.CampaignContact> ccl)
        {
            JsonSerializer ser = new JsonSerializer();
            foreach(Genesys.CampaignContact cc in ccl)
            {
                ser.Serialize(file, cc);
            }
        }
        public void Serialize(List<Genesys.CampaignContact> ccl)
        {
            JsonSerializer ser = new JsonSerializer();
            
            ser.Serialize(file, ccl);
        }
        public List<Genesys.CampaignContact> Deserialize()
        {
            return JsonConvert.DeserializeObject<List<Genesys.CampaignContact>>(File.ReadAllText(spath));
        }
    }

    class JsonStorePathNotFoundException : Exception
    {
        public JsonStorePathNotFoundException() : base(Properties.Resources.NOTJSONSTORE) { }
        public JsonStorePathNotFoundException(string msg) : base(msg) { }
        public JsonStorePathNotFoundException(string msg, Exception inner) : base(msg, inner) { }
    }
}
