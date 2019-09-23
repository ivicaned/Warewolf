﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Threading.Tasks;
using Warewolf.Data;
using Warewolf.Logging;
using System;
using Serilog.Events;
using System.Text;
using Newtonsoft.Json;
using Warewolf.Interfaces.Auditing;

namespace Warewolf.Driver.Serilog
{
    public class SeriLogConsumer : ILoggerConsumer
    {
        private readonly ILoggerPublisher _loggerPublisher;

        public SeriLogConsumer(ILoggerPublisher loggerPublisher)
        {
            _loggerPublisher = loggerPublisher;
        }

        public Task<ConsumerResult> Consume(byte[] body)
        {
            try
            {
                var audit = JsonConvert.DeserializeObject<IAudit>(Encoding.UTF8.GetString(body));
                LogMessage(_loggerPublisher, audit);

                return Task.FromResult(ConsumerResult.Success);
            }
            catch (Exception)
            {
                return Task.FromResult(ConsumerResult.Failed);
            }
        }


        private void LogMessage(ILoggerPublisher publisher, IAudit audit)
        {
            var logTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}";

            switch (audit.AuditType)
            {
                case "Information":
                    publisher.Info(logTemplate, DateTime.Now, LogEventLevel.Information, audit);
                    break;
                case "Warning":
                    publisher.Warn(logTemplate, DateTime.Now, LogEventLevel.Warning, audit);
                    break;
                case "Error":
                    publisher.Error(logTemplate, DateTime.Now, LogEventLevel.Error, audit, Environment.NewLine, audit.Exception);
                    break;
                case "Fatal":
                    publisher.Fatal(logTemplate, DateTime.Now, LogEventLevel.Fatal, audit, Environment.NewLine, audit.Exception);
                    break;

                default:
                    break;
            }
        }
    }
}