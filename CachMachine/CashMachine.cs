using System;
using System.Collections.Generic;
using System.Linq;

namespace CashMachine.Core
{
    public class BanknoteCassette
    {
        public int Denomination { get; }

        public int Capacity { get; }

        public int Count { get; private set; }

        public BanknoteCassette(int denomination, int capacity)
        {
            Denomination = denomination;
            Capacity = capacity;
            Count = 0;
        }

        public int Add(int count)
        {
            int space = Capacity - Count;
            if (count > space)
            {
                throw new InvalidOperationException($"Cannot add {count} banknotes: only {space} slots available in cassette for denomination {Denomination}.");
            }
            Count += count;
            return count;
        }

        public int Remove(int count)
        {
            if (count > Count)
            {
                throw new InvalidOperationException($"Cannot remove {count} banknotes: only {Count} available in cassette for denomination {Denomination}.");
            }
            Count -= count;
            return count;
        }
    }

    public class CashMachine
    {
        private readonly Dictionary<int, BanknoteCassette> _cassettes;

        public CashMachine(Dictionary<int, int> denominationCapacities)
        {
            _cassettes = new Dictionary<int, BanknoteCassette>();
            foreach (var pair in denominationCapacities)
            {
                _cassettes[pair.Key] = new BanknoteCassette(pair.Key, pair.Value);
            }
        }

        public void Deposit(int denomination, int count)
        {
            if (!_cassettes.ContainsKey(denomination))
            {
                throw new ArgumentException($"Cache machine does not support denomination {denomination}");
            }
            int added = _cassettes[denomination].Add(count);
            if (added < count)
            {
                throw new InvalidOperationException($"Only {added} banknotes of {denomination} were accepted due to cassette limit.");
            }
        }

        public int Withdraw(int denomination, int count)
        {
            if (!_cassettes.ContainsKey(denomination))
            {
                throw new ArgumentException($"ATM does not support denomination {denomination}");
            }
            int removed = _cassettes[denomination].Remove(count);
            if (removed < count)
            {
                throw new InvalidOperationException($"Only {removed} banknotes of {denomination} available.");
            }
            return removed;
        }

        public int GetBalance()
        {
            return _cassettes.Values.Sum(c => c.Denomination * c.Count);
        }

        public Dictionary<int, int> GetState()
        {
            return _cassettes.ToDictionary(c => c.Key, c => c.Value.Count);
        }

        public List<int> GetAvailableDenominations()
        {
            return _cassettes.Keys.OrderBy(x => x).ToList();
        }

        // Withdraw a specific amount using available denominations
        // selector: optional function to choose how to split the amount by denominations
        // Returns a dictionary of denomination to count dispensed
        public Dictionary<int, int> WithdrawAmount(int amount, Func<List<int>, Dictionary<int, int>>? selector = null)
        {
            var denominations = _cassettes.Keys.OrderByDescending(x => x).ToList();
            Dictionary<int, int> toDispense;

            if (selector != null)
            {
                // Use custom selector to determine how to split the amount
                toDispense = selector(denominations);
            }
            else
            {
                // Default: use largest denominations first
                toDispense = new Dictionary<int, int>();
                int remaining = amount;
                foreach (var denom in denominations)
                {
                    int availableNotes = _cassettes[denom].Count;
                    int neededNotes = remaining / denom;
                    int notesToUse = Math.Min(neededNotes, availableNotes);
                    if (notesToUse > 0)
                    {
                        toDispense[denom] = notesToUse;
                        remaining -= notesToUse * denom;
                    }
                }
                if (remaining != 0)
                {
                    throw new InvalidOperationException($"Cannot dispense the requested amount: {amount}");
                }
            }

            foreach (var pair in toDispense)
            {
                _cassettes[pair.Key].Remove(pair.Value);
            }

            return toDispense;
        }
    }
}