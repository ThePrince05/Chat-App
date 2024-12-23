using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client__.Net_.MVVM.Model
{
    internal class ServerModel
    {
        public string ServerUrl { get; set; }
        public string ServerPort { get; set; }
        public string SupabaseApiKey { get; set; }
        public string SupabaseProjectURL { get; set; }
    }
}
