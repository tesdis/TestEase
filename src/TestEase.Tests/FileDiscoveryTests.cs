// --------------------------------------------------------------------------------------------------------------------
// <copyright company="" file="FileDiscoveryTests.cs">
//   boom boom
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable ExceptionNotDocumented
namespace TestEase.Tests
{
    using System.Diagnostics.CodeAnalysis;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
    [TestClass]
    public class FileDiscoveryTests
    {
        [TestMethod]
        public void TestJsonDiscovery()
        {
            var tm = new TestDataManager();

            Assert.IsTrue(tm.Json.Keys.Count > 0);
        }

        [TestMethod]
        public void TestSqlDiscovery()
        {
            var tm = new TestDataManager();

            Assert.IsTrue(tm.Sql.Keys.Count > 0);
        }

        [TestMethod]
        public void TestTextDiscovery()
        {
            var tm = new TestDataManager();

            Assert.IsTrue(tm.Text.Keys.Count > 0);
        }

        [TestMethod]
        public void TestXmlDiscovery()
        {
            var tm = new TestDataManager();

            Assert.IsTrue(tm.Xml.Keys.Count > 0);
        }
    }
}