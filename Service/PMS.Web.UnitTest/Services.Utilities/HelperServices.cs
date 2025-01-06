using Microsoft.VisualStudio.TestTools.UnitTesting;
using PMS.EFCore.Model;
using PMS.EFCore.Services.Utilities;
using PMS.EFCore.Helper;
using System.Collections.Generic;

namespace PMS.Web.UnitTest.Services.Utilities
{
    [TestClass]
    public class HelperServicesTest
    {
        PMSContextBase context = new PMSContextBase(DBContextOption<PMSContextBase>.GetOptions("10.99.16.89", "PMSTEST", "sa", "@dmin54", string.Empty));

        [TestMethod]
        public void GetConfigAll_NullValue_ReturnAll()
        {
            // Arrange
            
            // Act
            IEnumerable<MCONFIG> values = HelperService.GetConfigAll(context);

            // Assert
            //values.GetEnumerator().
            //var val = (List<MCONFIG)values;
            //Assert.AreEqual((<List<MCONFIG>)values), string.Empty);
        }

        [TestMethod]
        public void GetConfigValue_NullValue_ReturnEmpty()
        {
            // Arrange
            string name = "XYZXYZ";

            // Act
            var value = HelperService.GetConfigValue(name, context);

            // Assert
            Assert.AreEqual(value, string.Empty);
        }
    }
}
