using System;
using System.Collections.Generic;
using System.Linq;

namespace CashMachine.Core
{
    public enum Banknote
    {
        Rub10 = 10,
        Rub50 = 50,
        Rub100 = 100,
        Rub500 = 500,
        Rub1000 = 1000,
        Rub2000 = 2000,
        Rub5000 = 5000
    }

    public class BanknoteCassette
    {
        public Banknote Denomination { get; }
        public int Capacity { get; }
        public int Count { get; private set; }

        public BanknoteCassette(Banknote denomination, int capacity)
        {
            Denomination = denomination;
            Capacity = capacity;
            Count = 0;
        }

        public void Add(int count)
        {
            int space = Capacity - Count;
            if (count > space)
            {
                throw new InvalidOperationException($"Cannot add {count} banknotes: only {space} slots available in cassette for denomination {Denomination}.");
            }
            Count += count;
        }

        public void Remove(int count)
        {
            if (count > Count)
            {
                throw new InvalidOperationException($"Cannot remove {count} banknotes: only {Count} available in cassette for denomination {Denomination}.");
            }
            Count -= count;
        }
    }

    public class CashMachine
    {
        private readonly Dictionary<Banknote, BanknoteCassette> _cassettes;

        public CashMachine(Dictionary<Banknote, int> denominationCapacities)
        {
            _cassettes = new Dictionary<Banknote, BanknoteCassette>();
            foreach (var pair in denominationCapacities)
            {
                _cassettes[pair.Key] = new BanknoteCassette(pair.Key, pair.Value);
            }
        }

        public void Deposit(Banknote denomination, int count)
        {
            if ((int)denomination <= 0)
                throw new ArgumentException("Denomination must be positive.", nameof(denomination));
            if (count <= 0)
                throw new ArgumentException("Count must be positive.", nameof(count));
            if (!_cassettes.ContainsKey(denomination))
            {
                throw new ArgumentException($"Cache machine does not support denomination {denomination}");
            }
            _cassettes[denomination].Add(count);
        }

        public void Withdraw(Banknote denomination, int count)
        {
            if ((int)denomination <= 0)
                throw new ArgumentException("Denomination must be positive.", nameof(denomination));
            if (count <= 0)
                throw new ArgumentException("Count must be positive.", nameof(count));
            if (!_cassettes.ContainsKey(denomination))
            {
                throw new ArgumentException($"ATM does not support denomination {denomination}");
            }
            _cassettes[denomination].Remove(count);
        }

        public int GetBalance()
        {
            return _cassettes.Values.Sum(c => (int)c.Denomination * c.Count);
        }

        public Dictionary<Banknote, int> GetState()
        {
            return _cassettes.ToDictionary(c => c.Key, c => c.Value.Count);
        }

        public List<Banknote> GetAvailableDenominations()
        {
            return _cassettes.Keys.OrderBy(x => (int)x).ToList();
        }

        // Withdraw an amount with an optional custom selector for denomination distribution
        public void WithdrawAmount(int amount, Func<List<Banknote>, Dictionary<Banknote, int>>? selector = null)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be positive.", nameof(amount));
            // If chosen plan is invalid, exception is thrown
            Dictionary<Banknote, int> plan = selector == null
                ? GetGreedyDispensePlan(amount)
                : GetCustomDispensePlan(amount, selector);

            // Remove notes according to the plan
            foreach (var pair in plan)
            {
                _cassettes[pair.Key].Remove(pair.Value);
            }
        }

        // Greedy algorithm for dispensing the amount
        private Dictionary<Banknote, int> GetGreedyDispensePlan(int amount)
        {
            var denominations = _cassettes.Keys.OrderByDescending(x => (int)x).ToList();
            var plan = new Dictionary<Banknote, int>();
            int remaining = amount;
            foreach (var denom in denominations)
            {
                int availableNotes = _cassettes[denom].Count;
                int neededNotes = remaining / (int)denom;
                int notesToUse = Math.Min(neededNotes, availableNotes);
                if (notesToUse > 0)
                {
                    plan[denom] = notesToUse;
                    remaining -= notesToUse * (int)denom;
                }
            }
            if (remaining != 0)
                throw new InvalidOperationException($"Cannot dispense the requested amount: {amount}");
            return plan;
        }

        // Check and get the user's plan for dispensing
        private Dictionary<Banknote, int> GetCustomDispensePlan(int amount, Func<List<Banknote>, Dictionary<Banknote, int>> selector)
        {
            var denominations = _cassettes.Keys.OrderByDescending(x => (int)x).ToList();
            var plan = selector(denominations);
            if (!IsDispensePlanValid(amount, plan))
                throw new InvalidOperationException("Custom dispense plan is invalid or not possible.");
            return plan;
        }

        // Check the validity of the user's plan
        private bool IsDispensePlanValid(int amount, Dictionary<Banknote, int> plan)
        {
            if (plan == null || plan.Count == 0)
                return false;
            int sum = 0;
            foreach (var pair in plan)
            {
                if (!_cassettes.ContainsKey(pair.Key))
                    return false;
                if (pair.Value <= 0 || pair.Value > _cassettes[pair.Key].Count)
                    return false;
                sum += (int)pair.Key * pair.Value;
            }
            return sum == amount;
        }
    }
}