using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace TestEase.Tests
{
    using System.Diagnostics.CodeAnalysis;

    using LibraryItemDictionaries;

    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
    public class ItemDictionaryTests
    {

        [Test]
        public void DuplicateRegistrationAndNoOverrideTest()
        {
            var tm = new TestDataManager();

            void RegisterAction()
            {
                tm.Dictionaries.Register<SqlItemDictionary>();
            }

            Assert.Throws<ArgumentException>((TestDelegate) RegisterAction, "Dictionary already registered and override is disabled. Dictionary type: .sql");
        }

        [Test]
        public void OverrideRegistrationTest()
        {
            var tm = new TestDataManager();
            var newDic = new SqlItemDictionary();

            tm.Dictionaries.Register<SqlItemDictionary>(newDic, true);

            Assert.AreSame(newDic, tm.Sql);
        }

        [Test]
        public void RegistrationWithInstanceAndNoOverrideTest()
        {
            var tm = new TestDataManager();
            var newDic = new SqlItemDictionary();


            void RegisterAction()
            {
                tm.Dictionaries.Register<SqlItemDictionary>(newDic);
            }


            Assert.Throws<ArgumentException>(RegisterAction, "Dictionary already registered and override is disabled. Dictionary type: .sql");
        }

        [Test]
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
