using System;
using System.Diagnostics;
using System.Security;
using Extensions.Standard;

namespace LoggerLite.EventLog
{
    public class EventLogLogger : LoggerBase
    {
        public new const string TruncateInfo = "/truncated";

        public EventLogLogger(string logSource, string logName)
        {
            Source = logSource;
            Name = logName;
            MaxSingleMessageLength = 31837;
            EnsureSourceExists();
        }

        public string Source { get; set; }
        public string Name { get; }

        public override bool FlushAuto => true;
        public override bool IsThreadSafe => true;

        protected override void Log(string message, MessageSeverity severity)
        {
            EnsureSourceExists();
            var eventType = severity switch
            {
                MessageSeverity.Warning => EventLogEntryType.Warning,
                MessageSeverity.Error => EventLogEntryType.Error,
                _ => EventLogEntryType.Information,
            };
            System.Diagnostics.EventLog.WriteEntry(Source, FormatMessage(message), eventType);
        }

        public void LogAuditFailure(string failure)
        {
            EnsureSourceExists();
            System.Diagnostics.EventLog.WriteEntry(Source, FormatMessage(failure), EventLogEntryType.FailureAudit);
        }

        public void LogAuditSuccess(string success)
        {
            EnsureSourceExists();
            System.Diagnostics.EventLog.WriteEntry(Source, FormatMessage(success), EventLogEntryType.SuccessAudit);
        }

        private void EnsureSourceExists()
        {
            if (System.Diagnostics.EventLog.SourceExists(Name)) return;
            try
            {
                System.Diagnostics.EventLog.CreateEventSource(Source, Name);
            }
            catch (Exception ex) when (ex is SecurityException || ex is UnauthorizedAccessException)
            {
                throw new UnauthorizedAccessException(
                    $"Could not create EventLog Source {Source}. The access is denied. Check, whether you have the administrative permission, or permission to write to EventLog.",
                    ex);
            }
        }

        protected internal string FormatMessage(string message)
        {
            return $"{(message.Length > MaxSingleMessageLength ? message.Truncate(MaxSingleMessageLength - TruncateInfo.Length) + TruncateInfo : message)}";
        }
    }
}
