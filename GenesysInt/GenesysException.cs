using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Genesys
{
    [Serializable()]
    class GenesysErrorResponse
    {
        public int status;
        public string code;
        public string message;
        public string messageWithParams;
        public MessageParameters messageParams;
        public string contextId;
        public string[] details;
        public string[] errors;

        [JsonConstructor]
        public GenesysErrorResponse()
        {
            status = 0;
            code = null;
            message = null;
            messageWithParams = null;
            messageParams = null;
            contextId = null;
            details = null;
            errors = null;
        }
        public GenesysErrorResponse(GenesysErrorResponse ger)
        {
            status = ger.status;
            code = ger.code;
            message = ger.message;
            messageWithParams = ger.messageWithParams;
            messageParams = ger.messageParams;
            contextId = ger.contextId;
            if (ger.details != null && ger.details.Length > 0)
            {
                details = new string[ger.details.Length];
                for (int i = 0; i < ger.details.Length; i++)
                    details[i] = ger.details[i];
            }
            else
                details = null;
            if (ger.errors != null && ger.errors.Length > 0)
            {
                errors = new string[ger.errors.Length];
                for (int i = 0; i < ger.errors.Length; i++)
                    errors[i] = ger.errors[i];
            }
            else
                errors = null;
        }

        //[JsonConstructor]
        public GenesysErrorResponse(int stat = 0, string cd = null, string msg = null, string msgWithParas = null, MessageParameters mp = null, string ctxid = null, string[] dtls = null, string[] errs = null)
        {
            status = stat;
            code = cd;
            message = msg;
            messageWithParams = msgWithParas;
            messageParams = mp;
            contextId = ctxid;
            details = new List<string>(dtls).ToArray();
            errors = new List<string>(errs).ToArray();
        }
        public GenesysErrorResponse(NotFoundResponse nfr)
        {
            status = nfr.status;
            code = nfr.code;
            message = nfr.message;
            messageWithParams = nfr.messageWithParams;
            messageParams = nfr.messageParams;
            contextId = nfr.contextId;
            details = new List<string>(nfr.details).ToArray();
            errors = new List<string>(nfr.errors).ToArray();
        }
    }

    class GenesysException : Exception
    {
        public GenesysException() : base(GenesysInt.Properties.Resources.GENESYSGENERICERRMSG) { }
        public GenesysException(string msg) : base(msg) { }
        public GenesysException(string msg, Exception inner) : base(msg, inner) { }
        public GenesysException(GenesysErrorResponse ger) { errResponse = ger; }
        public GenesysErrorResponse errResponse { get; set; }
    }

    class DuplicateContactListException : GenesysException
    {
        public DuplicateContactListException() : base(GenesysInt.Properties.Resources.DUPCONTLST) { }
        public DuplicateContactListException(string msg) : base(msg) { }
        public DuplicateContactListException(string msg, Exception inner) : base(msg, inner) { }
        public DuplicateContactListException(GenesysErrorResponse ger) : base(ger) { }
    }

    class ContactListNotFoundException : GenesysException
    {
        public ContactListNotFoundException() : base(GenesysInt.Properties.Resources.NFNDCONTLIST) { }
        public ContactListNotFoundException(string msg) : base(msg) { }
        public ContactListNotFoundException(string msg, Exception inner) : base(msg, inner) { }
        public ContactListNotFoundException(GenesysErrorResponse ger) : base(ger) { }
    }

    class DataExtractionFormatException : GenesysException
    {
        public DataExtractionFormatException() : base(GenesysInt.Properties.Resources.DATAFORMATERR) { }
        public DataExtractionFormatException(string msg) : base(msg) { }
        public DataExtractionFormatException(string msg, Exception inner) : base(msg, inner) { }
        public DataExtractionFormatException(GenesysErrorResponse ger) : base(ger) { }
    }

    [Serializable()]
    class MessageParameters
    {
        public string id;
        public string entity;

        [JsonConstructor]
        public MessageParameters()
        {
            id = null;
            entity = null;
        }

        public MessageParameters(string mid=null, string ent=null)
        {
            id = mid;
            entity = ent;
        }
        public MessageParameters(MessageParameters mp){
            id = mp.id;
            entity = mp.entity;
        }
    }

    [Serializable()]
    class NotFoundResponse
    {
        public int status;
        public string code;
        public string message;
        public string messageWithParams;
        public MessageParameters messageParams;
        public string contextId;
        public string[] details;
        public string[] errors;

        [JsonConstructor]
        public NotFoundResponse()
        {
            status = 0;
            code = null;
            message = null;
            messageWithParams = null;
            messageParams = null;
            contextId = null;
            details = null;
            errors = null;
        }

        //[JsonConstructor]
        public NotFoundResponse(int stat=0, string cd=null, string msg=null, string msgWithParas=null, MessageParameters mp=null, string ctxid=null, string[] dtls=null, string[] errs = null)
        {
            status = stat;
            code = cd;
            message = msg;
            messageWithParams = msgWithParas;
            messageParams = mp;
            contextId = ctxid;
            details = new List<string>(dtls).ToArray();
            errors = new List<string>(errs).ToArray();
        }
        public NotFoundResponse(NotFoundResponse nfr)
        {
            status = nfr.status;
            code = nfr.code;
            message = nfr.message;
            messageWithParams = nfr.messageWithParams;
            messageParams = nfr.messageParams;
            contextId = nfr.contextId;
            details = new List<string>(nfr.details).ToArray();
            errors = new List<string>(nfr.errors).ToArray();
        }
    }
}