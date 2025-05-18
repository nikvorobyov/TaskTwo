using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using CashMachine.Core;

namespace CashMachine.Tests
{
    [TestClass]
    public class CashMachineUnitTests
    {
        private static Dictionary<Banknote, int> DefaultCassettes() => new() { { Banknote.Rub10, 5 }, { Banknote.Rub50, 5 }, { Banknote.Rub100, 5 } };


        [TestMethod]
        public void Deposit_OverCapacity_Throws()
        {
            var cashMachine = new CashMachine.Core.CashMachine(DefaultCassettes());
            cashMachine.Deposit(Banknote.Rub10, 5);
            Assert.ThrowsException<InvalidOperationException>(() => cashMachine.Deposit(Banknote.Rub10, 1));
            // State should be full for 10s, empty for others
            var state = cashMachine.GetState();
            Assert.AreEqual(5, state[Banknote.Rub10]);
            Assert.AreEqual(0, state[Banknote.Rub50]);
            Assert.AreEqual(0, state[Banknote.Rub100]);
        }

        [TestMethod]
        public void Withdraw_OverAvailable_Throws()
        {
            var cashMachine = new CashMachine.Core.CashMachine(DefaultCassettes());
            cashMachine.Deposit(Banknote.Rub10, 2);
            Assert.ThrowsException<InvalidOperationException>(() => cashMachine.Withdraw(Banknote.Rub10, 3));
            // State should remain unchanged
            var state = cashMachine.GetState();
            Assert.AreEqual(2, state[Banknote.Rub10]);
            Assert.AreEqual(0, state[Banknote.Rub50]);
            Assert.AreEqual(0, state[Banknote.Rub100]);
        }

        [TestMethod]
        public void WithdrawAmount_ImpossibleAmount_Throws()
        {
            var cashMachine = new CashMachine.Core.CashMachine(DefaultCassettes());
            cashMachine.Deposit(Banknote.Rub10, 1);
            Assert.ThrowsException<InvalidOperationException>(() => cashMachine.WithdrawAmount(15));
            // State should remain unchanged
            var state = cashMachine.GetState();
            Assert.AreEqual(1, state[Banknote.Rub10]);
            Assert.AreEqual(0, state[Banknote.Rub50]);
            Assert.AreEqual(0, state[Banknote.Rub100]);
        }

        [TestMethod]
        public void WithdrawAmount_CustomSelector_Impossible_Throws()
        {
            var cashMachine = new CashMachine.Core.CashMachine(DefaultCassettes());
            cashMachine.Deposit(Banknote.Rub10, 5);
            cashMachine.Deposit(Banknote.Rub50, 5);
            cashMachine.Deposit(Banknote.Rub100, 5);
            // Custom selector: only use 10s
            Assert.ThrowsException<InvalidOperationException>(() =>
                cashMachine.WithdrawAmount(60, denoms => new Dictionary<Banknote, int> { { Banknote.Rub10, 6 } })
            );
            // State should remain unchanged
            var state = cashMachine.GetState();
            Assert.AreEqual(5, state[Banknote.Rub10]);
            Assert.AreEqual(5, state[Banknote.Rub50]);
            Assert.AreEqual(5, state[Banknote.Rub100]);
        }

        [TestMethod]
        public void WithdrawAmount_DefaultSelector_Success()
        {
            var cashMachine = new CashMachine.Core.CashMachine(DefaultCassettes());
            cashMachine.Deposit(Banknote.Rub10, 5);
            cashMachine.Deposit(Banknote.Rub50, 5);
            cashMachine.Deposit(Banknote.Rub100, 5);

            cashMachine.WithdrawAmount(160); // Should use 100+50+10
            // State after withdrawal
            var state = cashMachine.GetState();
            Assert.AreEqual(4, state[Banknote.Rub10]);
            Assert.AreEqual(4, state[Banknote.Rub50]);
            Assert.AreEqual(4, state[Banknote.Rub100]);
        }

        [TestMethod]
        public void WithdrawAmount_CustomSelector_Success()
        {
            var cashMachine = new CashMachine.Core.CashMachine(DefaultCassettes());
            cashMachine.Deposit(Banknote.Rub10, 5);
            cashMachine.Deposit(Banknote.Rub50, 5);
            cashMachine.Deposit(Banknote.Rub100, 5);
            // Custom selector: only use 10s
            cashMachine.WithdrawAmount(30, denoms => new Dictionary<Banknote, int> { { Banknote.Rub10, 3 } });
        
            var state = cashMachine.GetState();
            Assert.AreEqual(2, state[Banknote.Rub10]);
            Assert.AreEqual(5, state[Banknote.Rub50]);
            Assert.AreEqual(5, state[Banknote.Rub100]);
        }

        [TestMethod]
        public void WithdrawAmount_NotEnoughNotes_Throws()
        {
            var cashMachine = new CashMachine.Core.CashMachine(DefaultCassettes());
            cashMachine.Deposit(Banknote.Rub10, 2);
            Assert.ThrowsException<InvalidOperationException>(() => cashMachine.WithdrawAmount(30));
            var state = cashMachine.GetState();
            Assert.AreEqual(2, state[Banknote.Rub10]);
            Assert.AreEqual(0, state[Banknote.Rub50]);
            Assert.AreEqual(0, state[Banknote.Rub100]);
        }
    }
}
