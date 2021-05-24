using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C9Native
{
    public
    class PrivilegeAndAttributes
    {
        private PrivilegeInformation _information;
        private int _attributes;

        private const int ENABLED_BY_DEFAULT = 1;
        private const int ENABLED = 2;

        public PrivilegeAndAttributes(LUID id, int attributes)
        {
            _information = new PrivilegeInformation(id);
            _attributes = attributes;
        }

        public PrivilegeAndAttributes(string name, int attributes)
        {
            _information = new PrivilegeInformation(name);
            _attributes = attributes;
        }

        public PrivilegeAndAttributes(LUID_AND_ATTRIBUTES data)
        {
            _information = new PrivilegeInformation(data.Luid);
            _attributes = (int)data.Attributes;
        }

        public bool IsDefault
        {
            get
            {
                var masked = _attributes & ENABLED;
                return masked != 0;
            }
        }

        public bool IsEnabled
        {
            get
            {
                var masked = _attributes & ENABLED_BY_DEFAULT;
                return masked != 0;
            }
        }

        public string Name
        {
            get => _information.name;
        }

        public string Display
        {
            get => _information.display;
        }

        public LUID Luid
        {
            get => _information.luid;
        }

        public string Describe()
        {
            string luid = _information.Describe();
            string state = (IsEnabled ? "enabled" : "disabled");
            string isdefault = (IsDefault ? "default" : "");

            return $"{luid} {state} {isdefault}";
        }
    }
}
