using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TongTongAdmin.Services.Interfaces;

namespace TongTongAdmin.Common
{
    public class DatabaseAndUidPageParams
    {
        public IFirebaseDatabaseService Database { get; set; }

        public string Uid { get; set; }
    }
}