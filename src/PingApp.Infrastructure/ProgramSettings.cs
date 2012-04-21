using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;

namespace PingApp.Infrastructure {
    public class ProgramSettings {
        private static readonly ILoggerFactory loggerFactory;

        public bool Debug { get; private set; }

        public int BatchSize { get; private set; }

        public int RetryAttemptCount { get; private set; }

        public int ParallelDegree { get; set; }

        public string ProxyAddress { get; set; }

        public string LucentDirectory { get; private set; }

        public string MailAddress { get; private set; }

        public string MailUser { get; private set; }

        private ProgramSettings(bool debug, int batchSize, int retryAttemptCount, 
            int parallelDegree, string proxyAddress, string luceneDirectory, 
            string mailAddress, string mailUser) {
            Debug = debug;
            BatchSize = batchSize;
            RetryAttemptCount = retryAttemptCount;
            ParallelDegree = parallelDegree;
            LucentDirectory = luceneDirectory;
            ProxyAddress = proxyAddress;

            MailAddress = mailAddress;
            MailUser = mailUser;
        }

        public static ProgramSettings Current { get; private set; }

        #region Logger Factory Methods

        public static ILogger GetLogger<T>() {
            return loggerFactory.GetLogger<T>();
        }

        public static ILogger GetLogger(Type type) {
            return loggerFactory.GetLogger(type);
        }

        public static ILogger GetLogger(string name) {
            return loggerFactory.GetLogger(name);
        }

        #endregion

        static ProgramSettings() {
            NameValueCollection appSettings = ConfigurationManager.AppSettings;
            Type loggerFactoryType = Type.GetType(appSettings["LoggerFactory"], true);
            loggerFactory = Activator.CreateInstance(loggerFactoryType) as ILoggerFactory;

            if (loggerFactoryType == null) {
                throw new ConfigurationErrorsException("Invalid type of LoggerFactory");
            }

            Current = new ProgramSettings(
                Convert.ToBoolean(appSettings["Debug"]),
                Convert.ToInt32(appSettings["BatchSize"]),
                Convert.ToInt32(appSettings["RetryAttemptCount"]),
                Convert.ToInt32(appSettings["ParallelDegree"]),
                Convert.ToString(appSettings["ProxyAddress"]),
                Convert.ToString(appSettings["LucentDirectory"]),

                Convert.ToString(appSettings["MailAddress"]),
                Convert.ToString(appSettings["MailUser"])
            );
        }
    }
}
