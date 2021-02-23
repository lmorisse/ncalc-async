using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using NCalcAsync.Domain;

namespace NCalcAsync.Tests
{
    [TestClass]
    public class SpecificTests
    {
        [TestMethod]
        public async Task StepTest()
        {
            var e = new Expression("Step(2,3)");

            Assert.AreEqual(0F, await e.EvaluateAsync(2));
            Assert.AreEqual(2F, await e.EvaluateAsync(3));
            Assert.AreEqual(2F, await e.EvaluateAsync(4));
        }
        [TestMethod]
        public async Task ComplexTest()
        {
            var e = new Expression("if(Step(2,3)==0 && Step(2,3)==0,0,1)");

            Assert.AreEqual(0, await e.EvaluateAsync(2));
            Assert.AreEqual(1, await e.EvaluateAsync(4));
        }
    }
}

