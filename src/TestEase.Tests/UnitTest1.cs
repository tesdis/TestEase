using System;
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
            var boom = tm.Sql[""];
        }
    }
}
