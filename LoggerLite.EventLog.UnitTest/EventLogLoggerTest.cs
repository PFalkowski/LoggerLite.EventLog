using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LoggerLite.EventLog.UnitTest
{
    [TestClass]
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
                try
                {
                    Name = name;
                    Source = source;

                    Logger = new EventLogLogger(Source, Name);

                }
                catch (Exception)
                {
                    Assert.Inconclusive("could not create tested object.");
                }
            }

            public void Dispose()
            {
                System.Diagnostics.EventLog.Delete(Name);
            }

        }


        [TestMethod]
        public void Create()
        {
            try
            {
                var name = nameof(EventLogLogger);
                var source = nameof(Create);
                using (var tested = new TestObjectFactory(name, source))
                {
                    
                    Assert.AreEqual(source, tested.Logger.Source);
                    Assert.AreEqual(tested.Name, tested.Logger.Name);
                    Assert.IsTrue(tested.Logger.MaxSingleMessageLength < 31839);
                    Assert.IsTrue(System.Diagnostics.EventLog.SourceExists(tested.Logger.Source));
                    Assert.AreEqual(System.Diagnostics.EventLog.LogNameFromSourceName(tested.Source, "."), tested.Name);
                    Assert.AreEqual(1, System.Diagnostics.EventLog.GetEventLogs().Count(log => log.Log == name));
                    Assert.AreEqual(0, System.Diagnostics.EventLog.GetEventLogs().First(log => log.Log == name).Entries.Count);
                }
                Assert.IsFalse(System.Diagnostics.EventLog.SourceExists(source));
            }
            catch (Exception ex) when (ex is SecurityException || ex is UnauthorizedAccessException)
            {
                Assert.Inconclusive($"{ex.GetType().Name} {ex.Message} Run tests as administrator.");
            }
        }

        [TestMethod]
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

                    Assert.AreEqual(tested.Source, tested.Logger.Source);
                    Assert.AreEqual(tested.Name, tested.Logger.Name);
                    Assert.IsTrue(tested.Logger.MaxSingleMessageLength < 31839);
                    Assert.IsTrue(System.Diagnostics.EventLog.SourceExists(tested.Logger.Source));
                    Assert.AreEqual(System.Diagnostics.EventLog.LogNameFromSourceName(tested.Source, "."), tested.Name);
                    Assert.AreEqual(1, System.Diagnostics.EventLog.GetEventLogs().Count(log => log.Log == name));
                    Assert.AreEqual(1, System.Diagnostics.EventLog.GetEventLogs().First(log => log.Log == name).Entries.Count);
                    Assert.AreEqual(message, System.Diagnostics.EventLog.GetEventLogs().First(log => log.Log == name).Entries.Cast<EventLogEntry>().FirstOrDefault()?.Message);
                    Assert.AreEqual(EventLogEntryType.Error, System.Diagnostics.EventLog.GetEventLogs().First(log => log.Log == name).Entries.Cast<EventLogEntry>().FirstOrDefault()?.EntryType);
                }
                Assert.IsFalse(System.Diagnostics.EventLog.SourceExists(source));
            }
            catch (Exception ex) when (ex is SecurityException || ex is UnauthorizedAccessException)
            {
                Assert.Inconclusive($"{ex.GetType().Name} {ex.Message} Run tests as administrator.");
            }
        }
        [TestMethod]
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

                    source = tested.Source;

                    Assert.AreEqual(tested.Source, tested.Logger.Source);
                    Assert.AreEqual(tested.Name, tested.Logger.Name);
                    Assert.IsTrue(tested.Logger.MaxSingleMessageLength < 31839);
                    Assert.IsTrue(System.Diagnostics.EventLog.SourceExists(tested.Logger.Source));
                    Assert.AreEqual(System.Diagnostics.EventLog.LogNameFromSourceName(tested.Source, "."), tested.Name);
                    Assert.AreEqual(1, System.Diagnostics.EventLog.GetEventLogs().Count(log => log.Log == name));
                    Assert.AreEqual(1, System.Diagnostics.EventLog.GetEventLogs().First(log => log.Log == name).Entries.Count);
                    var messageActual = System.Diagnostics.EventLog.GetEventLogs().First(log => log.Log == name).Entries
                        .Cast<EventLogEntry>().FirstOrDefault()?.Message;
                    Assert.IsTrue(messageActual.EndsWith(EventLogLogger.TruncateInfo));
                    Assert.AreEqual(EventLogEntryType.Information, System.Diagnostics.EventLog.GetEventLogs().First(log => log.Log == name).Entries.Cast<EventLogEntry>().FirstOrDefault()?.EntryType);
                }
                Assert.IsFalse(System.Diagnostics.EventLog.SourceExists(source));
            }
            catch (Exception ex) when (ex is SecurityException || ex is UnauthorizedAccessException)
            {
                Assert.Inconclusive($"{ex.GetType().Name} {ex.Message} Run tests as administrator.");
            }
        }
        [TestMethod]
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

                    Assert.AreEqual(tested.Source, tested.Logger.Source);
                    Assert.AreEqual(tested.Name, tested.Logger.Name);
                    Assert.IsTrue(tested.Logger.MaxSingleMessageLength < 31839);
                    Assert.IsTrue(System.Diagnostics.EventLog.SourceExists(tested.Logger.Source));
                    Assert.AreEqual(System.Diagnostics.EventLog.LogNameFromSourceName(tested.Source, "."), tested.Name);
                    Assert.AreEqual(1, System.Diagnostics.EventLog.GetEventLogs().Count(log => log.Log == name));
                    Assert.AreEqual(1, System.Diagnostics.EventLog.GetEventLogs().First(log => log.Log == name).Entries.Count);
                    Assert.AreEqual(message, System.Diagnostics.EventLog.GetEventLogs().First(log => log.Log == name).Entries.Cast<EventLogEntry>().FirstOrDefault()?.Message);
                    Assert.AreEqual(EventLogEntryType.Warning, System.Diagnostics.EventLog.GetEventLogs().First(log => log.Log == name).Entries.Cast<EventLogEntry>().FirstOrDefault()?.EntryType);
                }
                Assert.IsFalse(System.Diagnostics.EventLog.SourceExists(source));
            }
            catch (Exception ex) when (ex is SecurityException || ex is UnauthorizedAccessException)
            {
                Assert.Inconclusive($"{ex.GetType().Name} {ex.Message} Run tests as administrator.");
            }
        }
        [TestMethod]
        public void LogAuditSucess()
        {
            try
            {
                var source = Path.GetRandomFileName();
                var name = Path.GetRandomFileName();
                var message = "LogAuditSucess test";
                using (var tested = new TestObjectFactory(name, source))
                {
                    tested.Logger.LogAuditSuccess(message);

                    source = tested.Source;

                    Assert.AreEqual(tested.Source, tested.Logger.Source);
                    Assert.AreEqual(tested.Name, tested.Logger.Name);
                    Assert.IsTrue(tested.Logger.MaxSingleMessageLength < 31839);
                    Assert.IsTrue(System.Diagnostics.EventLog.SourceExists(tested.Logger.Source));
                    Assert.AreEqual(System.Diagnostics.EventLog.LogNameFromSourceName(tested.Source, "."), tested.Name);
                    Assert.AreEqual(1, System.Diagnostics.EventLog.GetEventLogs().Count(log => log.Log == name));
                    Assert.AreEqual(1, System.Diagnostics.EventLog.GetEventLogs().First(log => log.Log == name).Entries.Count);
                    Assert.AreEqual(message, System.Diagnostics.EventLog.GetEventLogs().First(log => log.Log == name).Entries.Cast<EventLogEntry>().FirstOrDefault()?.Message);
                    Assert.AreEqual(EventLogEntryType.SuccessAudit, System.Diagnostics.EventLog.GetEventLogs().First(log => log.Log == name).Entries.Cast<EventLogEntry>().FirstOrDefault()?.EntryType);
                }
                Assert.IsFalse(System.Diagnostics.EventLog.SourceExists(source));
            }
            catch (Exception ex) when (ex is SecurityException || ex is UnauthorizedAccessException)
            {
                Assert.Inconclusive($"{ex.GetType().Name} {ex.Message} Run tests as administrator.");
            }
        }
        [TestMethod]
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

                    Assert.AreEqual(tested.Source, tested.Logger.Source);
                    Assert.AreEqual(tested.Name, tested.Logger.Name);
                    Assert.IsTrue(tested.Logger.MaxSingleMessageLength < 31839);
                    Assert.IsTrue(System.Diagnostics.EventLog.SourceExists(tested.Logger.Source));
                    Assert.AreEqual(System.Diagnostics.EventLog.LogNameFromSourceName(tested.Source, "."), tested.Name);
                    Assert.AreEqual(1, System.Diagnostics.EventLog.GetEventLogs().Count(log => log.Log == name));
                    Assert.AreEqual(1, System.Diagnostics.EventLog.GetEventLogs().First(log => log.Log == name).Entries.Count);
                    Assert.AreEqual(message, System.Diagnostics.EventLog.GetEventLogs().First(log => log.Log == name).Entries.Cast<EventLogEntry>().FirstOrDefault()?.Message);
                    Assert.AreEqual(EventLogEntryType.FailureAudit, System.Diagnostics.EventLog.GetEventLogs().First(log => log.Log == name).Entries.Cast<EventLogEntry>().FirstOrDefault()?.EntryType);
                }
                Assert.IsFalse(System.Diagnostics.EventLog.SourceExists(source));
            }
            catch (Exception ex) when (ex is SecurityException || ex is UnauthorizedAccessException)
            {
                Assert.Inconclusive($"{ex.GetType().Name} {ex.Message} Run tests as administrator.");
            }
        }
    }
}
