using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculate.Worker
{
    public class CalResultEntry:TableEntity
    {
        public string Result { set; get; }
    }
}
