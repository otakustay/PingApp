using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;

namespace PingApp.Schedule.Infrastructure {
    sealed class ProgramSettings {
        public bool Debug { get; private set; }

        public int BatchSize { get; private set; }

        public int RetryAttemptCount { get; private set; }

        public string LucentDirectory { get; private set; }

        public string MailAddress { get; private set; }

        public string MailUser { get; private set; }

        public string MailServerHost { get; private set; }

        public int MailServerPort { get; private set; }

        private ProgramSettings(bool debug, int batchSize, int retryAttemptCount, string luceneDirectory,
            string mailAddress, string mailUser, string mailServerHost, int mailServerPort) {
            Debug = debug;
            BatchSize = batchSize;
            RetryAttemptCount = retryAttemptCount;
            LucentDirectory = luceneDirectory;

            MailAddress = mailAddress;
            MailUser = mailUser;
            MailServerHost = mailServerHost;
            MailServerPort = mailServerPort;
        }

        public static ProgramSettings Current { get; private set; }

        static ProgramSettings() {
            NameValueCollection appSettings = ConfigurationManager.AppSettings;
            Current = new ProgramSettings(
                Convert.ToBoolean(appSettings["Debug"]),
                Convert.ToInt32(appSettings["BatchSize"]),
                Convert.ToInt32(appSettings["RetryAttemptCount"]),
                Convert.ToString(appSettings["LucentDirectory"]),

                Convert.ToString(appSettings["MailAddress"]),
                Convert.ToString(appSettings["MailUser"]),
                Convert.ToString(appSettings["MailServerHost"]),
                Convert.ToInt32(appSettings["MailServerPort"])
            );
        }
    }
}
