using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace CashMachine.WPF
{
    using Banknote = CashMachine.Core.Banknote;
    public partial class MainWindow : Window
    {
        private CashMachine.Core.CashMachine _cashMachine;
        private const int InitialCount = 100; // Large number of banknotes for each denomination
        private const int CassetteCapacity = 200; // Example cassette capacity
        Banknote[] _denominations = (Banknote[])Enum.GetValues(typeof(Banknote));
        private readonly Dictionary<Banknote, CheckBox> _checkboxes = new();


        public MainWindow()
        {
            InitializeComponent();
            InitializeCashMachine();
            RenderDenominationIconsAndCheckboxes();
            UpdateBalance();
            WithdrawButton.Click += WithdrawButton_Click;
        }

        // Initialize the cash machine with a large number of banknotes
        private void InitializeCashMachine()
        {
            var capacities = _denominations.ToDictionary(d => d, d => CassetteCapacity);
            _cashMachine = new CashMachine.Core.CashMachine(capacities);
            foreach (var denom in _denominations)
            {
                _cashMachine.Deposit(denom, InitialCount);
            }
        }

        // Render denomination icons and checkboxes
        private void RenderDenominationIconsAndCheckboxes()
        {
            DenominationIconsPanel.Children.Clear();
            var state = _cashMachine.GetState();
            const int rectWidth = 96;
            const int rectHeight = 64;
            for (int i = 0; i < _denominations.Length; i++)
            {
                var denom = _denominations[i];
                // Icon
                var stack = new StackPanel { Orientation = Orientation.Vertical, Margin = (i == 0) ? new Thickness(0) : new Thickness(15, 0, 0, 0) };
                var rect = new System.Windows.Shapes.Rectangle
                {
                    Width = rectWidth,
                    Height = rectHeight,
                    Fill = new SolidColorBrush(Color.FromRgb(220, 220, 220)),
                    Stroke = new SolidColorBrush(Color.FromRgb(100, 100, 100)),
                    StrokeThickness = 2,
                    RadiusX = 8,
                    RadiusY = 8,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                var denomText = new TextBlock
                {
                    Text = denom.ToString(),
                    FontSize = 20,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.Black,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, -40, 0, 0)
                };
                var grid = new Grid { Width = rectWidth, Height = rectHeight };
                grid.Children.Add(rect);
                grid.Children.Add(denomText);
                stack.Children.Add(grid);
                // Count below
                stack.Children.Add(new TextBlock
                {
                    Text = $"x{state[denom]}",
                    FontSize = 15,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = Brushes.DarkSlateGray,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 4, 0, 0)
                });
                // Checkbox for this denomination
                var checkBox = new CheckBox
                {
                    Margin = new Thickness(0, 8, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                stack.Children.Add(checkBox);
                _checkboxes[denom] = checkBox;
                DenominationIconsPanel.Children.Add(stack);
            }
        }

        // Update the displayed balance
        private void UpdateBalance()
        {
            BalanceText.Text = _cashMachine.GetBalance().ToString();
        }

        // Handler for Withdraw button
        private void WithdrawButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(AmountBox.Text, out int amount) || amount <= 0)
            {
                MessageBox.Show("Please enter a valid positive amount.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // Determine selected denominations
            var selectedDenoms = _denominations.Where(d => _checkboxes[d].IsChecked == true).ToList();
            selectedDenoms.Reverse();
            Func<List<Banknote>, Dictionary<Banknote, int>>? selector = null;
            if (selectedDenoms.Count > 0)
            {
                selector = (availableDenoms) =>
                {
                    var result = new Dictionary<Banknote, int>();
                    int remaining = amount;
                    foreach (var denom in selectedDenoms)
                    {
                        int availableNotes = _cashMachine.GetState()[denom];
                        int neededNotes = remaining / (int)denom;
                        int notesToUse = Math.Min(neededNotes, availableNotes);
                        if (notesToUse > 0)
                        {
                            result[denom] = notesToUse;
                            remaining -= notesToUse * (int)denom;
                        }
                    }
                    if (remaining != 0)
                        throw new InvalidOperationException($"Cannot dispense the requested amount: {amount} with selected denominations");
                    return result;
                };
            }
            try
            {
                var stateBefore = _cashMachine.GetState();

                _cashMachine.WithdrawAmount(amount, selector);

                var stateAfter = _cashMachine.GetState();

                var dispensed = stateBefore
                    .Where(kv => stateAfter.ContainsKey(kv.Key))
                    .Select(kv => new { Denom = kv.Key, Count = kv.Value - stateAfter[kv.Key] })
                    .Where(x => x.Count > 0)
                    .ToList();

                string dispensedStr = dispensed.Count > 0
                    ? string.Join(", ", dispensed.Select(x => $"{x.Denom} x {x.Count}"))
                    : "Nothing dispensed";

                MessageBox.Show($"Dispensed: {dispensedStr}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Withdrawal failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            RenderDenominationIconsAndCheckboxes();
            UpdateBalance();
        }
    }
}
