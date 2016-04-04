using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BExIS.Dlm.Test
{
    /// <summary>
    /// This test class is designed to be used in the build automation scenarios only.
    /// It must be deleted or altered to be a real unit test class when the build automation is stablized.
    /// Javad 3.3.2016
    /// </summary>
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            Assert.AreEqual("It should fail.", "It always fails.");
        }

        [TestMethod]
        public void TestMethod2()
        {
            Assert.AreEqual("It should pass.", "It should pass.");
        }
    }
}
