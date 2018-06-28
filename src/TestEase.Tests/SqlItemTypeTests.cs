namespace TestEase.Tests
{
    // ReSharper disable ExceptionNotDocumented
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
    [TestClass]
    public class SqlItemTypeTests
    {
        [TestMethod]
        public void TestQueueWithDefaultReplacements()
        {
            var tm = new TestDataManager();

            tm.Sql.SetupConnections(new Dictionary<string, string> { { "WORKSPACE", "tester" } });
            tm.Sql.QueueSql("Test.Create_CT_Rule");

            Assert.IsTrue(tm.Sql.QueuedSql.Count == 1);
            Assert.IsTrue(tm.Sql.QueuedSql.First().Value.ToString() == "\r\n\r\nSelect 1,name,'Boom' from sys.databases\r\n");
        }

        [TestMethod]
        public void TestQueueWithReplacements()
        {
            var tm = new TestDataManager();

            tm.Sql.SetupConnections(new Dictionary<string, string> { { "WORKSPACE", "tester" } });
            tm.Sql.QueueSql("Test.Create_CT_Rule", new Dictionary<string, object> { { "Test_Replace", 2 } });

            Assert.IsTrue(tm.Sql.QueuedSql.Count == 1);
            Assert.IsTrue(tm.Sql.QueuedSql.First().Value.ToString() == "\r\n\r\nSelect 2,name,'Boom' from sys.databases\r\n");
        }

        [TestMethod]
        public void TestQueueWithMultipleReplacements()
        {
            var tm = new TestDataManager();

            tm.Sql.SetupConnections(new Dictionary<string, string> { { "WORKSPACE", "tester" } });
            tm.Sql.QueueSql("Test.Create_CT_Rule", new Dictionary<string, object> { { "Test_Replace", 6 }, { "Test_Replace2", "New Boom" } });

            Assert.IsTrue(tm.Sql.QueuedSql.Count == 1);
            Assert.IsTrue(tm.Sql.QueuedSql.First().Value.ToString() == "\r\n\r\nSelect 6,name,'New Boom' from sys.databases\r\n");
        }
    }
}