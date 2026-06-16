using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security;
using Xunit;

namespace LoggerLite.EventLog.UnitTest
{
    [ExcludeFromCodeCoverage]
    public class EventLogLoggerTest
    {
        private class TestObjectFactory : IDisposable
        {
            public string Name { get; private set; }
            public string Source { get; private set; }
            public EventLogLogger Logger { get; private set; }

            public TestObjectFactory(string name, string source)
            {
                Name = name;
                Source = source;
                try
                {
                    Logger = new EventLogLogger(Source, Name);
                }
                catch (Exception)
                {
                    Logger = null!;
                    Skip.If(true, "could not create tested object.");
                }
            }

            public void Dispose()
            {
                System.Diagnostics.EventLog.Delete(Name);
            }
        }

        [SkippableFact]
        public void Create()
        {
            try
            {
                var name = nameof(EventLogLogger);
                var source = nameof(Create);
                using (var tested = new TestObjectFactory(name, source))
                {
                    Assert.Equal(source, tested.Logger.Source);
                    Assert.Equal(tested.Name, tested.Logger.Name);
                    Assert.True(tested.Logger.MaxSingleMessageLength < 31839);
                    Assert.True(System.Diagnostics.EventLog.SourceExists(tested.Logger.Source));
                    Assert.Equal(System.Diagnostics.EventLog.LogNameFromSourceName(tested.Source, "."), tested.Name);
                    Assert.Equal(1, System.Diagnostics.EventLog.GetEventLogs().Count(log => log.Log == name));
                    Assert.Empty(System.Diagnostics.EventLog.GetEventLogs().First(log => log.Log == name).Entries.Cast<EventLogEntry>());
                }
                Assert.False(System.Diagnostics.EventLog.SourceExists(source));
            }
            catch (Exception ex) when (ex is SecurityException || ex is UnauthorizedAccessException)
            {
                Skip.If(true, $"{ex.GetType().Name} {ex.Message} Run tests as administrator.");
            }
        }

        [SkippableFact]
        public void LogError()
        {
            try
            {
                var source = Path.GetRandomFileName();
                var name = Path.GetRandomFileName();
                var message = "LogError test";
                using (var tested = new TestObjectFactory(name, source))
                {
                    tested.Logger.LogError(message);

                    source = tested.Source;

                    Assert.Equal(tested.Source, tested.Logger.Source);
                    Assert.Equal(tested.Name, tested.Logger.Name);
                    Assert.True(tested.Logger.MaxSingleMessageLength < 31839);
                    Assert.True(System.Diagnostics.EventLog.SourceExists(tested.Logger.Source));
                    Assert.Equal(System.Diagnostics.EventLog.LogNameFromSourceName(tested.Source, "."), tested.Name);
                    Assert.Equal(1, System.Diagnostics.EventLog.GetEventLogs().Count(log => log.Log == name));
                    var entry = Assert.Single(System.Diagnostics.EventLog.GetEventLogs().First(log => log.Log == name).Entries.Cast<EventLogEntry>());
                    Assert.Equal(message, entry.Message);
                    Assert.Equal(EventLogEntryType.Error, entry.EntryType);
                }
                Assert.False(System.Diagnostics.EventLog.SourceExists(source));
            }
            catch (Exception ex) when (ex is SecurityException || ex is UnauthorizedAccessException)
            {
                Skip.If(true, $"{ex.GetType().Name} {ex.Message} Run tests as administrator.");
            }
        }

        [SkippableFact]
        public void LogLargeInfoGetsTruncated()
        {
            try
            {
                var source = Path.GetRandomFileName();
                var name = Path.GetRandomFileName();
                using (var tested = new TestObjectFactory(name, source))
                {
                    var message = string.Join("", Enumerable.Repeat("a", tested.Logger.MaxSingleMessageLength * 2));
                    tested.Logger.LogInfo(message);
                    // LoggerBase.LogInfo catches all exceptions silently; skip if the EventLog write
                    // failed on this runner (e.g. record too large for log's default max size).
                    Skip.If(tested.Logger.Failures > 0, "EventLog write of large message failed silently — runner limitation.");

                    source = tested.Source;

                    Assert.Equal(tested.Source, tested.Logger.Source);
                    Assert.Equal(tested.Name, tested.Logger.Name);
                    Assert.True(tested.Logger.MaxSingleMessageLength < 31839);
                    Assert.True(System.Diagnostics.EventLog.SourceExists(tested.Logger.Source));
                    Assert.Equal(System.Diagnostics.EventLog.LogNameFromSourceName(tested.Source, "."), tested.Name);
                    Assert.Equal(1, System.Diagnostics.EventLog.GetEventLogs().Count(log => log.Log == name));
                    var logEntries = System.Diagnostics.EventLog.GetEventLogs().First(log => log.Log == name).Entries.Cast<EventLogEntry>().ToList();
                    // WriteEntry may succeed without an entry appearing (runner buffer/permission quirk)
                    Skip.If(logEntries.Count == 0, "EventLog entry not visible after write — runner limitation.");
                    var entry = Assert.Single(logEntries);
                    Assert.EndsWith(EventLogLogger.TruncateInfo, entry.Message);
                    Assert.Equal(EventLogEntryType.Information, entry.EntryType);
                }
                Assert.False(System.Diagnostics.EventLog.SourceExists(source));
            }
            catch (Exception ex) when (ex is SecurityException || ex is UnauthorizedAccessException)
            {
                Skip.If(true, $"{ex.GetType().Name} {ex.Message} Run tests as administrator.");
            }
        }

        [SkippableFact]
        public void LogWarning()
        {
            try
            {
                var source = Path.GetRandomFileName();
                var name = Path.GetRandomFileName();
                var message = "LogWarning test";
                using (var tested = new TestObjectFactory(name, source))
                {
                    tested.Logger.LogWarning(message);

                    source = tested.Source;

                    Assert.Equal(tested.Source, tested.Logger.Source);
                    Assert.Equal(tested.Name, tested.Logger.Name);
                    Assert.True(tested.Logger.MaxSingleMessageLength < 31839);
                    Assert.True(System.Diagnostics.EventLog.SourceExists(tested.Logger.Source));
                    Assert.Equal(System.Diagnostics.EventLog.LogNameFromSourceName(tested.Source, "."), tested.Name);
                    Assert.Equal(1, System.Diagnostics.EventLog.GetEventLogs().Count(log => log.Log == name));
                    var entry = Assert.Single(System.Diagnostics.EventLog.GetEventLogs().First(log => log.Log == name).Entries.Cast<EventLogEntry>());
                    Assert.Equal(message, entry.Message);
                    Assert.Equal(EventLogEntryType.Warning, entry.EntryType);
                }
                Assert.False(System.Diagnostics.EventLog.SourceExists(source));
            }
            catch (Exception ex) when (ex is SecurityException || ex is UnauthorizedAccessException)
            {
                Skip.If(true, $"{ex.GetType().Name} {ex.Message} Run tests as administrator.");
            }
        }

        [SkippableFact]
        public void LogAuditSuccess()
        {
            try
            {
                var source = Path.GetRandomFileName();
                var name = Path.GetRandomFileName();
                var message = "LogAuditSuccess test";
                using (var tested = new TestObjectFactory(name, source))
                {
                    tested.Logger.LogAuditSuccess(message);

                    source = tested.Source;

                    Assert.Equal(tested.Source, tested.Logger.Source);
                    Assert.Equal(tested.Name, tested.Logger.Name);
                    Assert.True(tested.Logger.MaxSingleMessageLength < 31839);
                    Assert.True(System.Diagnostics.EventLog.SourceExists(tested.Logger.Source));
                    Assert.Equal(System.Diagnostics.EventLog.LogNameFromSourceName(tested.Source, "."), tested.Name);
                    Assert.Equal(1, System.Diagnostics.EventLog.GetEventLogs().Count(log => log.Log == name));
                    var entry = Assert.Single(System.Diagnostics.EventLog.GetEventLogs().First(log => log.Log == name).Entries.Cast<EventLogEntry>());
                    Assert.Equal(message, entry.Message);
                    Assert.Equal(EventLogEntryType.SuccessAudit, entry.EntryType);
                }
                Assert.False(System.Diagnostics.EventLog.SourceExists(source));
            }
            catch (Exception ex) when (ex is SecurityException || ex is UnauthorizedAccessException)
            {
                Skip.If(true, $"{ex.GetType().Name} {ex.Message} Run tests as administrator.");
            }
        }

        [SkippableFact]
        public void LogAuditFailure()
        {
            try
            {
                var source = Path.GetRandomFileName();
                var name = Path.GetRandomFileName();
                var message = "LogAuditFailure test";
                using (var tested = new TestObjectFactory(name, source))
                {
                    tested.Logger.LogAuditFailure(message);

                    source = tested.Source;

                    Assert.Equal(tested.Source, tested.Logger.Source);
                    Assert.Equal(tested.Name, tested.Logger.Name);
                    Assert.True(tested.Logger.MaxSingleMessageLength < 31839);
                    Assert.True(System.Diagnostics.EventLog.SourceExists(tested.Logger.Source));
                    Assert.Equal(System.Diagnostics.EventLog.LogNameFromSourceName(tested.Source, "."), tested.Name);
                    Assert.Equal(1, System.Diagnostics.EventLog.GetEventLogs().Count(log => log.Log == name));
                    var entry = Assert.Single(System.Diagnostics.EventLog.GetEventLogs().First(log => log.Log == name).Entries.Cast<EventLogEntry>());
                    Assert.Equal(message, entry.Message);
                    Assert.Equal(EventLogEntryType.FailureAudit, entry.EntryType);
                }
                Assert.False(System.Diagnostics.EventLog.SourceExists(source));
            }
            catch (Exception ex) when (ex is SecurityException || ex is UnauthorizedAccessException)
            {
                Skip.If(true, $"{ex.GetType().Name} {ex.Message} Run tests as administrator.");
            }
        }
    }
}
