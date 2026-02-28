using System;
using System.Linq;
using System.Windows.Controls;

namespace ChemicalWarehouseWPF.Views
{
    public partial class StockView : Page
    {
        private ChemicalWarehouseDBEntities3 _db;
        public StockView()
        {
            InitializeComponent();
            _db = new ChemicalWarehouseDBEntities3();
            Loaded += (s, e) => LoadStock();
        }
        void LoadStock()
        {
            try
            {
                var today = DateTime.Today;
                var data = _db.ReagentBatches.ToList().Select(b => new
                {
                    ReagentName = b.Reagents?.ReagentName ?? "Не указан",
                    BatchNumber = b.BatchNumber,
                    CurrentQuantity = b.CurrentQuantity,
                    UnitSymbol = b.Reagents?.UnitsOfMeasure?.UnitSymbol ?? "шт",
                    LocationCode = b.StorageLocations?.LocationCode ?? "Не указано",
                    ExpirationDate = b.ExpirationDate,
                    Status = b.CurrentQuantity <= 0 ? "Закончилось" :
                            b.ExpirationDate < today ? "Просрочено" :
                            (b.ExpirationDate - today).Days <= 30 ? "Истекает" : "Норма"
                }).ToList();
                dgStock.ItemsSource = data;
                txtTotalItems.Text = data.Count.ToString();
                txtTotalQuantity.Text = data.Sum(s => s.CurrentQuantity).ToString("F2");
                txtNormal.Text = data.Count(s => s.Status == "Норма").ToString();
                txtLowStock.Text = data.Count(s => s.Status == "Истекает").ToString();
                txtExpired.Text = data.Count(s => s.Status == "Просрочено").ToString();
                txtFinished.Text = data.Count(s => s.Status == "Закончилось").ToString();
            }
            catch (Exception ex) { System.Windows.MessageBox.Show($"Ошибка: {ex.Message}"); }
        }
    }
}