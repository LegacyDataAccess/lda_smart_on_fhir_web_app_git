using lda_fhir_web_data_access;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace lda_fhir_web_app_unit_tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void GetUser()
        {
            var userSearch = new UserDataAccess();
            try
            {
                var response = userSearch.GetUser(1);
                Assert.IsTrue(response != null);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.InnerException.Message);
            }

        }

        [TestMethod]
        public void GetEhrClient()
        {
            var search = new EhrClientDataAccess();
            try
            {
                var response = search.GetEhrClient("6822a665-f217-481b-a5b6-9609ef6b2746");
                Assert.IsTrue(response != null);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.InnerException.Message);
            }

        }
    }
}
