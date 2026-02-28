using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ChemicalWarehouseWPF.Views
{
    public partial class ExpiringView : Page
    {
        private ChemicalWarehouseDBEntities3 _context;

        public ExpiringView()
        {
            InitializeComponent();
            _context = new ChemicalWarehouseDBEntities3();
            this.Loaded += ExpiringView_Loaded;
        }

        private void ExpiringView_Loaded(object sender, RoutedEventArgs e)
        {
            cmbDays.SelectedIndex = 2; 
            LoadExpiring();
        }
        private void LoadExpiring()
        {
            try
            {
                if (_context == null)
                {
                    System.Diagnostics.Debug.WriteLine("Ошибка: контекст не инициализирован");
                    return;
                }
                int days = GetSelectedDays();
                var today = DateTime.Today;
                var threshold = today.AddDays(days);

                var expiring = _context.ReagentBatches
                    .Where(b => b.CurrentQuantity > 0)
                    .ToList()
                    .Where(b => b.ExpirationDate <= threshold && b.ExpirationDate >= today)
                    .OrderBy(b => b.ExpirationDate)
                    .Select(b => new
                    {
                        ReagentName = b.Reagents?.ReagentName ?? "Не указан",
                        BatchNumber = b.BatchNumber,
                        CurrentQuantity = b.CurrentQuantity,
                        UnitSymbol = b.Reagents?.UnitsOfMeasure?.UnitSymbol ?? "шт",
                        ExpirationDate = b.ExpirationDate,
                        DaysLeft = (b.ExpirationDate - today).Days,
                        LocationCode = b.StorageLocations?.LocationCode ?? "Не указано"
                    })
                    .ToList();

                dgExpiring.ItemsSource = expiring;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка: {ex.Message}");
            }
        }
        private int GetSelectedDays()
        {
            if (cmbDays == null || cmbDays.SelectedIndex < 0) return 30;

            switch (cmbDays.SelectedIndex)
            {
                case 0: return 7;
                case 1: return 14;
                case 2: return 30;
                case 3: return 60;
                default: return 30;
            }
        }
        private void CmbDays_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadExpiring();
        }
    }
}