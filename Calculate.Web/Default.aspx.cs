using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Calculate.Web
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            
        }

        // Submit calculation job.
        protected void Button1_Click(object sender, EventArgs e)
        {
            // read Azure storage connection string from Cloud settings
            var connstr = RoleEnvironment.GetConfigurationSettingValue("DataConnection");

            // create Azure Queue Client
            var storageAccount = CloudStorageAccount.Parse(connstr);
            var queueClient = storageAccount.CreateCloudQueueClient();

            // get queue named "caljobqueue"
            var queue = queueClient.GetQueueReference("caljobqueue");
            queue.CreateIfNotExists();

            // warp user's input into queue message, add to queue.
            string msgstr = string.Format("{0},{1}",
                TextBox1.Text,
                TextBox2.Text);

            queue.AddMessage(
                new CloudQueueMessage(msgstr));

            // all done. Write application log
            System.Diagnostics.Trace.TraceInformation("Message added. " + msgstr);
        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            var connstr = RoleEnvironment.GetConfigurationSettingValue("DataConnection");
            var storageAccount = CloudStorageAccount.Parse(connstr);
            var tableClient = storageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference("calresulttable");
            table.CreateIfNotExists();

            // get today's result form table
            var results = from en in table.CreateQuery<Calculate.Worker.CalResultEntry>()
                          where en.PartitionKey == DateTime.UtcNow.ToString("yyyyMMdd")
                          select en;

            // display result on page
            string resStr = "";
            foreach (var en in results)
            {
                resStr += en.Result + "<br>";
            }
            Label1.Text = resStr;
        }
    }
}