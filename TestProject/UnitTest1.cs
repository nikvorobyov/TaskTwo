using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using CashMachine.Core;

namespace CashMachine.Tests
{
    [TestClass]
    public class CashMachineUnitTests
    {
        private Dictionary<int, int> DefaultCassettes() => new() { { 10, 5 }, { 50, 5 }, { 100, 5 } };

        [TestMethod]
        public void Deposit_InvalidDenomination_Throws()
        {
            var cashMachine = new CashMachine.Core.CashMachine(DefaultCassettes());
            Assert.ThrowsException<ArgumentException>(() => cashMachine.Deposit(20, 1));
            // State should remain unchanged
            var state = cashMachine.GetState();
            Assert.AreEqual(0, state[10]);
            Assert.AreEqual(0, state[50]);
            Assert.AreEqual(0, state[100]);
        }

        [TestMethod]
        public void Deposit_OverCapacity_Throws()
        {
            var cashMachine = new CashMachine.Core.CashMachine(DefaultCassettes());
            cashMachine.Deposit(10, 5);
            Assert.ThrowsException<InvalidOperationException>(() => cashMachine.Deposit(10, 1));
            // State should be full for 10s, empty for others
            var state = cashMachine.GetState();
            Assert.AreEqual(5, state[10]);
            Assert.AreEqual(0, state[50]);
            Assert.AreEqual(0, state[100]);
        }

        [TestMethod]
        public void Withdraw_InvalidDenomination_Throws()
        {
            var cashMachine = new CashMachine.Core.CashMachine(DefaultCassettes());
            Assert.ThrowsException<ArgumentException>(() => cashMachine.Withdraw(20, 1));
            // State should remain unchanged
            var state = cashMachine.GetState();
            Assert.AreEqual(0, state[10]);
            Assert.AreEqual(0, state[50]);
            Assert.AreEqual(0, state[100]);
        }

        [TestMethod]
        public void Withdraw_OverAvailable_Throws()
        {
            var cashMachine = new CashMachine.Core.CashMachine(DefaultCassettes());
            cashMachine.Deposit(10, 2);
            Assert.ThrowsException<InvalidOperationException>(() => cashMachine.Withdraw(10, 3));
            // State should remain unchanged
            var state = cashMachine.GetState();
            Assert.AreEqual(2, state[10]);
            Assert.AreEqual(0, state[50]);
            Assert.AreEqual(0, state[100]);
        }

        [TestMethod]
        public void WithdrawAmount_ImpossibleAmount_Throws()
        {
            var cashMachine = new CashMachine.Core.CashMachine(DefaultCassettes());
            cashMachine.Deposit(10, 1);
            Assert.ThrowsException<InvalidOperationException>(() => cashMachine.WithdrawAmount(15));
            // State should remain unchanged
            var state = cashMachine.GetState();
            Assert.AreEqual(1, state[10]);
            Assert.AreEqual(0, state[50]);
            Assert.AreEqual(0, state[100]);
        }

        [TestMethod]
        public void WithdrawAmount_CustomSelector_Impossible_Throws()
        {
            var cashMachine = new CashMachine.Core.CashMachine(DefaultCassettes());
            cashMachine.Deposit(10, 5);
            cashMachine.Deposit(50, 5);
            cashMachine.Deposit(100, 5);
            // Custom selector: only use 10s
            Assert.ThrowsException<InvalidOperationException>(() =>
                cashMachine.WithdrawAmount(60, denoms => new Dictionary<int, int> { { 10, 6 } })
            );
            // State should remain unchanged
            var state = cashMachine.GetState();
            Assert.AreEqual(5, state[10]);
            Assert.AreEqual(5, state[50]);
            Assert.AreEqual(5, state[100]);
        }

        [TestMethod]
        public void WithdrawAmount_DefaultSelector_Success()
        {
            var cashMachine = new CashMachine.Core.CashMachine(DefaultCassettes());
            cashMachine.Deposit(10, 5);
            cashMachine.Deposit(50, 5);
            cashMachine.Deposit(100, 5);
            var result = cashMachine.WithdrawAmount(160); // Should use 100+50+10
            Assert.AreEqual(1, result[100]);
            Assert.AreEqual(1, result[50]);
            Assert.AreEqual(1, result[10]);
            // State after withdrawal
            var state = cashMachine.GetState();
            Assert.AreEqual(4, state[10]);
            Assert.AreEqual(4, state[50]);
            Assert.AreEqual(4, state[100]);
        }

        [TestMethod]
        public void WithdrawAmount_CustomSelector_Success()
        {
            var cashMachine = new CashMachine.Core.CashMachine(DefaultCassettes());
            cashMachine.Deposit(10, 5);
            cashMachine.Deposit(50, 5);
            cashMachine.Deposit(100, 5);
            // Custom selector: only use 10s
            var result = cashMachine.WithdrawAmount(30, denoms => new Dictionary<int, int> { { 10, 3 } });
            Assert.AreEqual(3, result[10]);

            var state = cashMachine.GetState();
            Assert.AreEqual(2, state[10]);
            Assert.AreEqual(5, state[50]);
            Assert.AreEqual(5, state[100]);
        }

        [TestMethod]
        public void WithdrawAmount_NotEnoughNotes_Throws()
        {
            var cashMachine = new CashMachine.Core.CashMachine(DefaultCassettes());
            cashMachine.Deposit(10, 2);
            Assert.ThrowsException<InvalidOperationException>(() => cashMachine.WithdrawAmount(30));
            var state = cashMachine.GetState();
            Assert.AreEqual(2, state[10]);
            Assert.AreEqual(0, state[50]);
            Assert.AreEqual(0, state[100]);
        }
    }
}
