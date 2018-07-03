using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestEase.Tests
{
    using System.Diagnostics.CodeAnalysis;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using TestEase.LibraryItemDictionaries;

    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
    [TestClass]
    public class ItemDictionaryTests
    {

        [TestMethod]
        public void DuplicateRegistrationAndNoOverrideTest()
        {
            var tm = new TestDataManager();

            void RegisterAction()
            {
                tm.Dictionaries.Register<SqlItemDictionary>();
            }

            Assert.ThrowsException<ArgumentException>((Action)RegisterAction, "Dictionary already registered and override is disabled. Dictionary type: .sql");
        }

        [TestMethod]
        public void OverrideRegistrationTest()
        {
            var tm = new TestDataManager();
            var newDic = new SqlItemDictionary();

            tm.Dictionaries.Register<SqlItemDictionary>(newDic, true);

            Assert.AreSame(newDic, tm.Sql);
        }

        [TestMethod]
        public void RegistrationWithInstanceAndNoOverrideTest()
        {
            var tm = new TestDataManager();
            var newDic = new SqlItemDictionary();


            void RegisterAction()
            {
                tm.Dictionaries.Register<SqlItemDictionary>(newDic);
            }


            Assert.ThrowsException<ArgumentException>((Action)RegisterAction, "Dictionary already registered and override is disabled. Dictionary type: .sql");
        }

        [TestMethod]
        public void RegistrationWithInstanceTest()
        {
            var tm = new TestDataManager();
            var newDic = new SqlItemDictionary();

            tm.Dictionaries.Clear();
            tm.Dictionaries.ExtensionMappings.Clear();
            tm.Dictionaries.Register<SqlItemDictionary>(newDic);


            Assert.AreSame(newDic, tm.Sql);
        }
    }
}
