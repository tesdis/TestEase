using NUnit.Framework;

namespace TestEase.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Diagnostics.CodeAnalysis;

    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
    public class SqlDictionaryTests
    {
        private dynamic results;
        private readonly Dictionary<string, string> testConnections = new Dictionary<string, string>()
                                                                          {
                                                                              {"Test", "Data Source=.\\ct;Initial Catalog=master;Integrated Security=True"},
                                                                              {
                                                                                  "BadConnection",
                                                                                  "Data Source=NOWHERE;Initial Catalog=master;Integrated Security=True;Connection Timeout=1"
                                                                              }
                                                                          };
        private TestDataManager testDataManager { get; set; }

        public SqlDictionaryTests()
        {
            testDataManager = new TestDataManager();
            testDataManager.Sql.SetupConnections(testConnections);
        }

        private void SetupDataManager()
        {
            testDataManager = new TestDataManager();
            testDataManager.Sql.SetupConnections(testConnections);
        }

        [Test]
        public void BadConnectionStringTest()
        {
            SetupDataManager();
            testDataManager.Sql.QueueSql("BadConnection", "select name from sys.databases");

            void ExecuteAction()
            {
                testDataManager.Sql.Execute();
            }

            Assert.Throws<SqlException>(ExecuteAction, "A network-related or instance-specific error occurred while establishing a connection to SQL Server");
        }

        [Test]
        public void OverwriteExistingConnectionStrings()
        {
            SetupDataManager();
            testDataManager.Sql.SetupConnections(new Dictionary<string, string>()
                                                          {
                                                              {"Test", "Data Source=DESKTOP-RC3RG6H\\SKRAPS;Initial Catalog=master;Integrated Security=True"},
                                                              {
                                                                  "BadConnection",
                                                                  "Data Source=NOWHERE;Initial Catalog=master;Integrated Security=True;Connection Timeout=1"
                                                              }
                                                          });

            Assert.IsTrue(testDataManager.Sql.GetConnections().Count == 2);
        }

        [Test]
        public void QueueSqlWithNoConnections()
        {
            SetupDataManager();
            try
            {
                testDataManager.Sql.SetupConnections(new Dictionary<string, string>());
                testDataManager.Sql.QueueSql("Test", "select name from sys.databases");
                Assert.Fail("Exception should have been thrown because no connections exist");
            }
            catch (ArgumentException e)
            {
                Assert.IsTrue(e.Message == "This flavor can only be run if you pre-configure the connections.");
            }
        }

        [Test]
        public void QueueLibraryItemWithNoConnections()
        {
            SetupDataManager();
            try
            {
                testDataManager.Sql.SetupConnections(new Dictionary<string, string>());
                testDataManager.Sql.QueueLibraryItem("Test.TestSql");
                Assert.Fail("Exception should have been thrown because no connections exist");
            }
            catch (ArgumentException e)
            {
                Assert.IsTrue(e.Message == "This flavor can only be run if you pre-configure the connections.");
            }
        }

        [Test]
        public void MissingConnectionStringTest()
        {
            SetupDataManager();
            testDataManager.Sql.QueueSql("MissingConnection", "select name from sys.databases");

            void ExecuteAction()
            {
                testDataManager.Sql.Execute();
            }

            Assert.Throws<KeyNotFoundException>(ExecuteAction, "Connections does not contain a key for MISSINGCONNECTION. Keys present: TEST, BADCONNECTION");
        }

        [Test]
        public void ExecuteWithSingleResult()
        {
            SetupDataManager();
            testDataManager.Sql.QueueSql("Test", "select 1 [Val] from sys.databases where name in ('master')");

            results = testDataManager.Sql.Execute();

            Assert.AreEqual(results[0].Val.ToString(), "1");
        }

        [Test]
        public void ExecuteWithNothingQueued()
        {
            SetupDataManager();
            try
            {
                results = testDataManager.Sql.Execute();
                Assert.Fail("Exception should have been thrown because no sql is queued");
            }
            catch (Exception e)
            {
                Assert.IsTrue(e.Message == "No sql is currently queued");
            }
        }


        [Test]
        public void ExecuteWithSqlError()
        {
            SetupDataManager();
            testDataManager.Sql.QueueSql("Test", "select 1/0 [Val] from sys.databases where name in ('master')");

            TestDelegate executeAction = () =>
                {
                    results = testDataManager.Sql.Execute();
                };


            Assert.Throws<SqlException>(executeAction);
        }

        [Test]
        public void ExecuteLibraryItemWithSingleResult()
        {
            SetupDataManager();
            testDataManager.Sql.QueueLibraryItem("Test.TestSql");

            results = testDataManager.Sql.Execute();

            Assert.AreEqual(results[0].Val.ToString(), "1");
        }

        [Test]
        public void ExecuteLibraryItemBadDbType()
        {
            SetupDataManager();
            try
            {
                testDataManager.Sql.QueueLibraryItem("sql.TestSqlWithBadDbType");
                Assert.Fail("Exception should be thrown because the script contains a db type that does not have a connection configured");
            }
            catch (Exception e)
            {
                Assert.IsTrue(e.GetType() == typeof(ArgumentException));
                Assert.IsTrue(e.Message == "DB type split did not work correctly. Expression used: --DbType \\\\s*=\\\\s* (TEST|BADCONNECTION)");
            }
        }

        [Test]
        public void ExecuteWithMultipleResult()
        {
            SetupDataManager();
            testDataManager.Sql.QueueSql("Test", "select 1 [Val] from sys.databases where name in ('master')");
            testDataManager.Sql.QueueSql("Test", "select 2 [Val] from sys.databases where name in ('master')");
            testDataManager.Sql.QueueSql("Test", "select 3 [Val] from sys.databases where name in ('master')");

            results = testDataManager.Sql.Execute();

            Assert.AreEqual(results[0].Val.ToString(), "1");
            Assert.AreEqual(results[1].Val.ToString(), "2");
            Assert.AreEqual(results[2].Val.ToString(), "3");
        }

        [Test]
        public void ExecuteLibraryItemWithIncludes()
        {
            SetupDataManager();
            testDataManager.Sql.QueueLibraryItem("sql.TestSqlWithIncludes", new Dictionary<string, object>() { { "BaseValue", 69 } });

            results = testDataManager.Sql.Execute();

            foreach (var li in results)
            {
                foreach (var property in (IDictionary<String, Object>)li)
                {
                    Console.WriteLine(property.Key + ": " + property.Value);
                }
            }

            var result1 = results[0];
            var result2 = results[1];
            var prop1 = (IDictionary<String, Object>) result1;
            var prop2 = (IDictionary<String, Object>)result2;




            Assert.AreEqual(prop1["FirstCol"].ToString(), "22");
            Assert.AreEqual(prop2["PassedInCol"].ToString(), "69");
        }

        [Test]
        public void ExecuteLibraryItemWithIncludesNoReplacements()
        {
            SetupDataManager();
            testDataManager.Sql.QueueLibraryItem("Test.TestSqlWithIncludes_NoReplacements");

            results = testDataManager.Sql.Execute();

            Assert.AreEqual(results[0].Val.ToString(), "22");
            Assert.AreEqual(results[1].Val.ToString(), "1");
        }

        [Test]
        public void ExecuteLibraryItemWithBadIncludes()
        {
            SetupDataManager();
            void QueueAction()
            {
                testDataManager.Sql.QueueLibraryItem("Test.TestSqlWithBadIncludes", new Dictionary<string, object>() { { "BaseValue", 69 } });
            }

            Assert.Throws<KeyNotFoundException>(QueueAction, "Library item does not exist in the collection: Test.TestSqlWithBadIncludes");
        }

        [Test]
        public void ExecuteLibraryItemWithBadNestedIncludes()
        {
            SetupDataManager();
            try
            {
                testDataManager.Sql.QueueLibraryItem("sql.TestSqlBadNestedIncludes", new Dictionary<string, object>() { { "BaseValue", 69 } });
            }
            catch (Exception e)
            {
                Assert.IsTrue(e.Message.Contains("The library item \"Test.TestSqlBad\" that is part of an include statement was not found."));
            }

        }

        [Test]
        public void ExecuteLibraryItemWithBadIncludesNoDefaultValues()
        {
            SetupDataManager();
            try
            {
                testDataManager.Sql.QueueLibraryItem("sql.TestSqlWithBadIncludes");
                Assert.Fail("Exception should have been thrown");
            }
            catch (Exception e)
            {
                Assert.IsTrue(e.Message.Contains("The library item \"Test.TestSqlBad\" that is part of an include statement was not found."));
            }
        }

        [Test]
        public void ExecuteLibraryItemWithIncludesNoDefaultValueForMissingReplacement()
        {
            SetupDataManager();
            void QueueAction()
            {
                testDataManager.Sql.QueueLibraryItem("Test.TestSqlWithIncludesNoDefaults");
            }

            Assert.Throws<ArgumentException>((QueueAction));
        }
    }
}
