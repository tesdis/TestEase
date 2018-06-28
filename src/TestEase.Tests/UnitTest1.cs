using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestEase.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var tm = new TestDataManager();

            tm.Sql.SetupConnections(new Dictionary<string, string>
            {
                {"WORKSPACE","tester"}
            });
            tm.Sql.QueueSql("Test.Create_CT_Rule");
        }
    }
}
