using System;
using System.Collections.Generic;
using System.Linq;

namespace CashMachine.Core
{
    // Represents a banknote cassette in the ATM
    public class BanknoteCassette
    {
        // Denomination of the banknote
        public int Denomination { get; }
        // Maximum number of banknotes the cassette can hold
        public int Capacity { get; }
        // Current number of banknotes in the cassette
        public int Count { get; private set; }

        public BanknoteCassette(int denomination, int capacity)
        {
            Denomination = denomination;
            Capacity = capacity;
            Count = 0;
        }

        // Add banknotes to the cassette
        public int Add(int count)
        {
            int space = Capacity - Count;
            int toAdd = Math.Min(space, count);
            Count += toAdd;
            return toAdd;
        }

        // Remove banknotes from the cassette
        public int Remove(int count)
        {
            int toRemove = Math.Min(Count, count);
            Count -= toRemove;
            return toRemove;
        }
    }

    // Represents the ATM (Cash Machine)
    public class CashMachine
    {
        // Dictionary of denomination to cassette
        private readonly Dictionary<int, BanknoteCassette> _cassettes;

        public CashMachine(Dictionary<int, int> denominationCapacities)
        {
            _cassettes = new Dictionary<int, BanknoteCassette>();
            foreach (var pair in denominationCapacities)
            {
                _cassettes[pair.Key] = new BanknoteCassette(pair.Key, pair.Value);
            }
        }

        // Deposit banknotes into the ATM
        public void Deposit(int denomination, int count)
        {
            if (!_cassettes.ContainsKey(denomination))
                throw new ArgumentException($"ATM does not support denomination {denomination}");
            int added = _cassettes[denomination].Add(count);
            if (added < count)
                throw new InvalidOperationException($"Only {added} banknotes of {denomination} were accepted due to cassette limit.");
        }

        // Withdraw banknotes of a specific denomination
        public int Withdraw(int denomination, int count)
        {
            if (!_cassettes.ContainsKey(denomination))
                throw new ArgumentException($"ATM does not support denomination {denomination}");
            int removed = _cassettes[denomination].Remove(count);
            if (removed < count)
                throw new InvalidOperationException($"Only {removed} banknotes of {denomination} available.");
            return removed;
        }

        // Get the current balance of the ATM
        public int GetBalance()
        {
            return _cassettes.Values.Sum(c => c.Denomination * c.Count);
        }

        // Get the state of the ATM (denominations and their counts)
        public Dictionary<int, int> GetState()
        {
            return _cassettes.ToDictionary(c => c.Key, c => c.Value.Count);
        }

        // Get available denominations
        public List<int> GetAvailableDenominations()
        {
            return _cassettes.Keys.OrderBy(x => x).ToList();
        }
    }
} 