using BookLibrary.Functions.Core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Services
{
    public class AmazonService
    {
        private string _key;
        private string _secret;
        private const string DESTINATION = "ecs.amazonaws.com";
        private const string NAMESPACE = "http://webservices.amazon.com/AWSECommerceService/2009-03-31";

        public AmazonService()
        {
            _key = ConfigurationManager.AppSettings["AmazonKey"];
            _secret = ConfigurationManager.AppSettings["AmazonSecret"];
        }
        public object SearchAmazon(string ItemId, string IdType)
        {
            SignedRequestHelper helper = new SignedRequestHelper(_key, _secret, DESTINATION);

            IDictionary<string, string> r1 = new Dictionary<string, String>();
            r1["Service"] = "AWSECommerceService";
            r1["Version"] = "2009-03-31";
            r1["Operation"] = "ItemLookup";
            r1["IdType"] = IdType;
            if(IdType == "ISBN")
                r1["SearchIndex"] = "Books";
            r1["ItemId"] = ItemId;
            r1["ResponseGroup"] = "Large";
            r1["AssociateTag"] = "gmancoder-20";

            string requestUrl = helper.Sign(r1);

            return Core.PostRemoteData(requestUrl, "", method: "GET");
        }
    }
}
