using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Functions
{
    public class FieldParser
    {
        public FieldParser() { }

        public bool ParseGuid(object inValue, string fieldName, out Guid outField, out Exception ex)
        {
            ex = null;
            
            if (!Guid.TryParse(inValue.ToString(), out outField))
            {
                ex = new Exception(String.Format("{0} not a valid GUID for field {1}", inValue.ToString(), fieldName));
                return false;
            }
            return true;
        }

        public bool ParseInt(object inValue, string fieldName, out int outField, out Exception ex)
        {
            ex = null;

            if (!Int32.TryParse(inValue.ToString(), out outField))
            {
                ex = new Exception(String.Format("{0} not a valid integer for field {1}", inValue.ToString(), fieldName));
                return false;
            }
            return true;
        }
        public bool ParseDate(object inValue, string fieldName, out DateTime outField, out Exception ex)
        {
            ex = null;

            if (!DateTime.TryParse(inValue.ToString(), out outField))
            {
                ex = new Exception(String.Format("{0} not a valid date/time for field {1}", inValue.ToString(), fieldName));
                return false;
            }
            return true;
        }
        public bool ParseBoolean(object inValue, string fieldName, out bool outField, out Exception ex)
        {
            ex = null;

            if (!Boolean.TryParse(inValue.ToString(), out outField))
            {
                ex = new Exception(String.Format("{0} not a valid boolean for field {1}", inValue.ToString(), fieldName));
                return false;
            }
            return true;
        }
        public bool ParseUri(object inValue, string fieldName, out Uri outField, out Exception ex)
        {
            ex = null;

            if (!Uri.TryCreate(inValue.ToString(), UriKind.RelativeOrAbsolute, out outField))
            {
                ex = new Exception(String.Format("{0} not a valid uri for field {1}", inValue.ToString(), fieldName));
                return false;
            }
            return true;
        }
    }
}
