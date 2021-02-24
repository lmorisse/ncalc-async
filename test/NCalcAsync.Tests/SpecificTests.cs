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
        #region Step
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
        #endregion

        #region Ramp
        [TestMethod]
        public async Task RampTest()
        {
            var e = new Expression("RAMP(20, -7)");
            Assert.AreEqual(0F, await e.EvaluateAsync(20));
            Assert.AreEqual(-70F, await e.EvaluateAsync(30));
        }
        [TestMethod]
        public async Task RampTest1()
        {
            var e = new Expression("RAMP(20, 1)");
            Assert.AreEqual(10F, await e.EvaluateAsync(30));
        }
        [TestMethod]
        public async Task RampTest2()
        {
            var e = new Expression("RAMP(1, -7)");
            Assert.AreEqual(0F, await e.EvaluateAsync(1));
        }
        #endregion

        #region Pulse
        /// <summary>
        /// DeltaTime = 1 / default
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task PulseTest()
        {
            var e = new Expression("Pulse(20, 1)");

            Assert.AreEqual(0F, await e.EvaluateAsync(null,0));
            Assert.AreEqual(20F, await e.EvaluateAsync(null,1));
            Assert.AreEqual(0F, await e.EvaluateAsync(null,2));
        }
        /// <summary>
        /// DeltaTime = 1 / default
        /// With Interval
        /// </summary>
        [TestMethod]
        public async Task PulseTest1()
        {
            var e = new Expression("Pulse(20, 12, 5)");

            Assert.AreEqual(0F, await e.EvaluateAsync(null,10));
            Assert.AreEqual(20F, await e.EvaluateAsync(null, 12));
            Assert.AreEqual(0F, await e.EvaluateAsync(null, 15));
            Assert.AreEqual(20F, await e.EvaluateAsync(null, 17));
        }
        /// <summary>
        /// DeltaTime = 1 / default
        /// With Interval = 0
        /// </summary>
        [TestMethod]
        public async Task PulseTest2()
        {
            var e = new Expression("Pulse(20, 12, 0)");

            Assert.AreEqual(0F, await e.EvaluateAsync(null, 10));
            Assert.AreEqual(20F, await e.EvaluateAsync(null, 12));
            Assert.AreEqual(0F, await e.EvaluateAsync(null, 15));
            Assert.AreEqual(0F, await e.EvaluateAsync(null, 17));
        }
        /// <summary>
        /// DeltaTime = 0.5 / default
        /// With Interval 
        /// </summary>
        [TestMethod]
        public async Task PulseTest23()
        {
            var e = new Expression("Pulse(20, 12, 5)");

            Assert.AreEqual(0F, await e.EvaluateAsync(null, 23,0.5F));
            Assert.AreEqual(20F, await e.EvaluateAsync(null, 24,0.5F));
            Assert.AreEqual(0F, await e.EvaluateAsync(null, 25,0.5F));
            Assert.AreEqual(0F, await e.EvaluateAsync(null, 27,0.5F));
        }
        #endregion

        #region Dt & Time
        [TestMethod]
        public async Task TimeTest()
        {
            var e = new Expression("Step(Dt,Time)");
            Assert.AreEqual(1F, await e.EvaluateAsync(2,2,1));
        }


        #endregion

        #region Normal

        [DataRow("Normal(0, 0)")]// without seed
        [DataRow("Normal(0, 0,1)")]// with seed
        [TestMethod]
        public async Task NormalTest(string function)
        {
            var e = new Expression(function);
            Assert.AreEqual(0F, await e.EvaluateAsync());
        }


        #endregion

        #region Smth  
        [DataRow("SMTH1(5+Step(10,3),5,5)")] //With Initial value
        [DataRow("SMTH1(5+Step(10,3),5)")] //Without Initial value
        [DataRow("SMTHN(5+Step(10,3),5,1,5)")] //With Initial value
        [DataRow("SMTHN(5+Step(10,3),5,1)")] //Without Initial value
        [TestMethod]
        public async Task Smth1Test(string function)
        {
            var e = new Expression(function);
            // TO test Smth without initial value, we need to evaluate at step 0 first to add the initial value
            Assert.AreEqual(5F, await e.EvaluateAsync(0, 0, 1));
            Assert.AreEqual(5F, await e.EvaluateAsync(2, 2, 1));
            Assert.AreEqual(7F, await e.EvaluateAsync(4, 4, 1));
        }
        [DataRow("SMTH3(5+Step(10,3),5,5)")] //With Initial value
        [DataRow("SMTH3(5+Step(10,3),5)")] //Without Initial value
        [DataRow("SMTHN(5+Step(10,3),5,3,5)")] //With Initial value
        [DataRow("SMTHN(5+Step(10,3),5,3)")] //Without Initial value
        [TestMethod]
        public async Task Smth3Test(string function)
        {
            var e = new Expression(function);
            Assert.AreEqual(5F, await e.EvaluateAsync(0, 0, 1));
            Assert.AreEqual(5F, await e.EvaluateAsync(2, 2, 1));
            Assert.IsTrue(5F < (float)await e.EvaluateAsync(4, 4, 1));
        }
        #endregion
    }
}

