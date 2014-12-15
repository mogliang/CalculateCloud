using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Calculate.Worker
{
    public class WorkerRole : RoleEntryPoint
    {
        public override void Run()
        {
            // read Azure storage connection string from Cloud settings
            var connstr = RoleEnvironment.GetConfigurationSettingValue("DataConnection");

            // create Azure Queue Client
            var storageAccount = CloudStorageAccount.Parse(connstr);
            var queueClient = storageAccount.CreateCloudQueueClient();

            // get queue named "caljobqueue", create if it doesn't exist
            var queue = queueClient.GetQueueReference("caljobqueue");
            queue.CreateIfNotExists();

            while (true)
            {
                // get message from queue, if queue is not empty, 
                // it return one message, otherwise, return null
                var msg = queue.GetMessage();
                if (msg != null)
                {
                    // handle job here.
                    var nums = msg.AsString.Split(',');
                    queue.DeleteMessage(msg);

                    double answer = double.Parse(nums[0]) * double.Parse(nums[1]);

                    string result = string.Format("Job handled. {0}*{1}={2}", nums[0], nums[1], answer);

                    AddResultEntry(result);
                    // add applciation log
                    Trace.TraceInformation(result);
                }
                Thread.Sleep(10000);
            }

        }

        // initialize Table on Role start
        CloudTable _resultTable = null;
        public override bool OnStart()
        {
            // read Azure storage connection string from Cloud settings
            var connstr = RoleEnvironment.GetConfigurationSettingValue("DataConnection");

            // create Azure Table Client
            var storageAccount = CloudStorageAccount.Parse(connstr);
            var tableClient = storageAccount.CreateCloudTableClient();

            // get table named "calresulttable", create if it doesn't exist
            _resultTable = tableClient.GetTableReference("calresulttable");
            _resultTable.CreateIfNotExists();

            return base.OnStart();
        }

        void AddResultEntry(string result)
        {
            // PartitionKey+RowKey is table's primary index, must have
            var newentry = new CalResultEntry
            {
                PartitionKey = DateTime.UtcNow.ToString("yyyyMMdd"),
                RowKey = DateTime.UtcNow.Ticks.ToString(),
                Result = result
            };

            var addOp = TableOperation.Insert(newentry);
            _resultTable.Execute(addOp);
        }
    }
}
