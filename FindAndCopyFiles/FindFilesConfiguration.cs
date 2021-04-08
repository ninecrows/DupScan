using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;

namespace FindAndCopyFiles
{
    [JsonObject(MemberSerialization.OptIn)]
    class FindFilesConfiguration
    {
        [JsonProperty]
        private
        List<string> _from = new List<string>();

        [JsonProperty]
        private
        List<string> _to = new List<string>();

        public IReadOnlyList<string> sources => _from;
        public IReadOnlyList<string> destinations => _to;

        public
            FindFilesConfiguration()
        {
        }

        public
            FindFilesConfiguration(
                List<string> aFrom,
                List<string> aTo
            )
        {
            _from = aFrom;
            _to = aTo;
        }

        public
            FindFilesConfiguration(
                string[] aFrom,
                string[] aTo
            )
        {
            _from.AddRange(aFrom);
            _to.AddRange(aTo);
        }
    }
}
