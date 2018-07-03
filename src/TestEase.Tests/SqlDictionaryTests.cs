namespace TestEase.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Diagnostics.CodeAnalysis;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
    [TestClass]
    public class SqlDictionaryTests
    {
        private dynamic results;
        private readonly Dictionary<string, string> testConnections = new Dictionary<string, string>()
                                                                          {
                                                                              {"Test", "Data Source=DESKTOP-RC3RG6H\\SKRAPS;Initial Catalog=master;Integrated Security=True"},
                                                                              {
                                                                                  "BadConnection",
                                                                                  "Data Source=NOWHERE;Initial Catalog=master;Integrated Security=True;Connection Timeout=1"
                                                                              }
                                                                          };

        private readonly TestDataManager testDataManager;

        public SqlDictionaryTests()
        {
            this.testDataManager = new TestDataManager();
            this.testDataManager.Sql.SetupConnections(this.testConnections);
        }

        [TestMethod]
        public void BadConnectionStringTest()
        {
            this.testDataManager.Sql.QueueSql("BadConnection", "select name from sys.databases");

            void ExecuteAction()
            {
                this.testDataManager.Sql.Execute();
            }

            Assert.ThrowsException<SqlException>((Action)ExecuteAction, "A network-related or instance-specific error occurred while establishing a connection to SQL Server");
        }

        [TestMethod]
        public void OverwriteExistingConnectionStrings()
        {
            this.testDataManager.Sql.SetupConnections(new Dictionary<string, string>()
                                                          {
                                                              {"Test", "Data Source=DESKTOP-RC3RG6H\\SKRAPS;Initial Catalog=master;Integrated Security=True"},
                                                              {
                                                                  "BadConnection",
                                                                  "Data Source=NOWHERE;Initial Catalog=master;Integrated Security=True;Connection Timeout=1"
                                                              }
                                                          });

            Assert.IsTrue(this.testDataManager.Sql.GetConnections().Count == 2);
        }

        [TestMethod]
        public void QueueSqlWithNoConnections()
        {
            try
            {
                this.testDataManager.Sql.SetupConnections(new Dictionary<string, string>());
                this.testDataManager.Sql.QueueSql("Test", "select name from sys.databases");
                Assert.Fail("Exception should have been thrown because no connections exist");
            }
            catch (ArgumentException e)
            {
                Assert.IsTrue(e.Message == "This flavor can only be run if you pre-configure the connections.");
            }
        }

        [TestMethod]
        public void QueueLibraryItemWithNoConnections()
        {
            try
            {
                this.testDataManager.Sql.SetupConnections(new Dictionary<string, string>());
                this.testDataManager.Sql.QueueLibraryItem("Test.TestSql");
                Assert.Fail("Exception should have been thrown because no connections exist");
            }
            catch (ArgumentException e)
            {
                Assert.IsTrue(e.Message == "This flavor can only be run if you pre-configure the connections.");
            }
        }

        [TestMethod]
        public void MissingConnectionStringTest()
        {
            this.testDataManager.Sql.QueueSql("MissingConnection", "select name from sys.databases");

            void ExecuteAction()
            {
                this.testDataManager.Sql.Execute();
            }

            Assert.ThrowsException<KeyNotFoundException>((Action)ExecuteAction, "Connections does not contain a key for MISSINGCONNECTION. Keys present: TEST, BADCONNECTION");
        }

        [TestMethod]
        public void ExecuteWithSingleResult()
        {
            this.testDataManager.Sql.QueueSql("Test", "select 1 [Val] from sys.databases where name in ('master')");

            this.results = this.testDataManager.Sql.Execute();

            Assert.AreEqual(this.results[0].Val.ToString(), "1");
        }

        [TestMethod]
        public void ExecuteWithNothingQueued()
        {
            try
            {
                this.results = this.testDataManager.Sql.Execute();
                Assert.Fail("Exception should have been thrown because no sql is queued");
            }
            catch (Exception e)
            {
                Assert.IsTrue(e.Message == "No sql is currently queued");
            }
        }

        [TestMethod]
        public void ExecuteWithSqlError()
        {
            this.testDataManager.Sql.QueueSql("Test", "select 1/0 [Val] from sys.databases where name in ('master')");

            Action executeAction = () =>
                {
                    this.results = this.testDataManager.Sql.Execute();
                };


            Assert.ThrowsException<SqlException>(executeAction);
        }

        [TestMethod]
        public void ExecuteLibraryItemWithSingleResult()
        {
            this.testDataManager.Sql.QueueLibraryItem("Test.TestSql");

            this.results = this.testDataManager.Sql.Execute();

            Assert.AreEqual(this.results[0].Val.ToString(), "1");
        }

        [TestMethod]
        public void ExecuteLibraryItemBadDbType()
        {
            try
            {
                this.testDataManager.Sql.QueueLibraryItem("Test.TestSqlWithBadDbType");
                Assert.Fail("Exception should be thrown because the script contains a db type that does not have a connection configured");
            }
            catch (Exception e)
            {
                Assert.IsTrue(e.GetType() == typeof(ArgumentException));
                Assert.IsTrue(e.Message == "DB type split did not work correctly. Expression used: --DbType \\\\s*=\\\\s* (TEST|BADCONNECTION)");
            }
        }

        [TestMethod]
        public void ExecuteWithMultipleResult()
        {
            this.testDataManager.Sql.QueueSql("Test", "select 1 [Val] from sys.databases where name in ('master')");
            this.testDataManager.Sql.QueueSql("Test", "select 2 [Val] from sys.databases where name in ('master')");
            this.testDataManager.Sql.QueueSql("Test", "select 3 [Val] from sys.databases where name in ('master')");

            this.results = this.testDataManager.Sql.Execute();

            Assert.AreEqual(this.results[0].Val.ToString(), "1");
            Assert.AreEqual(this.results[1].Val.ToString(), "2");
            Assert.AreEqual(this.results[2].Val.ToString(), "3");
        }

        [TestMethod]
        public void ExecuteLibraryItemWithIncludes()
        {
            this.testDataManager.Sql.QueueLibraryItem("Test.TestSqlWithIncludes", new Dictionary<string, object>() { { "BaseValue", 69 } });

            this.results = this.testDataManager.Sql.Execute();

            Assert.AreEqual(this.results[0].Val.ToString(), "22");
            Assert.AreEqual(this.results[1].Val.ToString(), "69");
        }

        [TestMethod]
        public void ExecuteLibraryItemWithIncludesNoReplacements()
        {
            this.testDataManager.Sql.QueueLibraryItem("Test.TestSqlWithIncludes_NoReplacements");

            this.results = this.testDataManager.Sql.Execute();

            Assert.AreEqual(this.results[0].Val.ToString(), "22");
            Assert.AreEqual(this.results[1].Val.ToString(), "1");
        }

        [TestMethod]
        public void ExecuteLibraryItemWithBadIncludes()
        {
            void QueueAction()
            {
                this.testDataManager.Sql.QueueLibraryItem("Test.TestSqlWithBadIncludes", new Dictionary<string, object>() { { "BaseValue", 69 } });
            }

            Assert.ThrowsException<KeyNotFoundException>((Action)QueueAction, "Library item does not exist in the collection: Test.TestSqlWithBadIncludes");
        }

        [TestMethod]
        public void ExecuteLibraryItemWithBadNestedIncludes()
        {
            try
            {
                this.testDataManager.Sql.QueueLibraryItem("Test.TestSqlWithBadInclude", new Dictionary<string, object>() { { "BaseValue", 69 } });
            }
            catch (Exception e)
            {
                Assert.IsTrue(e.Message.Contains("The library item \"Test.TestSqlBad\" that is part of an include statement was not found."));
            }

        }

        [TestMethod]
        public void ExecuteLibraryItemWithBadIncludesNoDefaultValues()
        {
            try
            {
                this.testDataManager.Sql.QueueLibraryItem("Test.TestSqlWithBadInclude");
                Assert.Fail("Exception should have been thrown");
            }
            catch (Exception e)
            {
                Assert.IsTrue(e.Message.Contains("The library item \"Test.TestSqlBad\" that is part of an include statement was not found."));
            }
        }

        [TestMethod]
        public void ExecuteLibraryItemWithIncludesNoDefaultValueForMissingReplacement()
        {
            void QueueAction()
            {
                this.testDataManager.Sql.QueueLibraryItem("Test.TestSqlWithIncludesNoDefaults");
            }

            Assert.ThrowsException<ArgumentException>((Action)QueueAction);
        }
    }
}
