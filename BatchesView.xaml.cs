using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ChemicalWarehouseWPF.Views
{
    public partial class BatchesView : Page
    {
        private ChemicalWarehouseDBEntities3 _db;
        public BatchesView()
        {
            InitializeComponent();
            _db = new ChemicalWarehouseDBEntities3();
            LoadBatches();
        }
        void LoadBatches()
        {
            try
            {
                var today = DateTime.Today;
                var data = _db.ReagentBatches.Select(b => new
                {
                    b.BatchID,
                    b.BatchNumber,
                    ReagentName = b.Reagents.ReagentName,
                    SupplierName = b.Suppliers.SupplierName,
                    b.ArrivalDate,
                    b.ExpirationDate,
                    b.CurrentQuantity,
                    UnitSymbol = b.Reagents.UnitsOfMeasure.UnitSymbol,
                    LocationCode = b.StorageLocations.LocationCode,
                    b.IsAvailable
                }).ToList();
                dgBatches.ItemsSource = data.Select(b => new
                {
                    b.BatchID,
                    b.BatchNumber,
                    b.ReagentName,
                    b.SupplierName,
                    b.ArrivalDate,
                    b.ExpirationDate,
                    b.CurrentQuantity,
                    b.UnitSymbol,
                    b.LocationCode,
                    Status = b.IsAvailable == false ? "Списано" :
                            b.ExpirationDate < today ? "Просрочено" :
                            (b.ExpirationDate - today).Days <= 30 ? "Истекает" :
                            b.CurrentQuantity <= 0 ? "Закончилось" : "Активна"
                }).ToList();
                txtStatus.Text = $"Загружено: {data.Count} партий";
            }
            catch (Exception ex) { MessageBox.Show($"Ошибка: {ex.Message}"); }
        }
        void BtnAdd_Click(object s, RoutedEventArgs e) => ShowDialog(null);
        void BtnEdit_Click(object s, RoutedEventArgs e)
        {
            if (dgBatches.SelectedItem == null)
            { MessageBox.Show("Выберите партию"); return; }
            dynamic sel = dgBatches.SelectedItem;
            ShowDialog(_db.ReagentBatches.Find((int)sel.BatchID));
        }
        void ShowDialog(ReagentBatches batch)
        {
            var dlg = new BatchDialog(_db, batch) { Owner = Window.GetWindow(this) };
            if (dlg.ShowDialog() == true) { LoadBatches(); txtStatus.Text = batch == null ? "Партия добавлена" : "Партия обновлена"; }
        }
        void BtnDelete_Click(object s, RoutedEventArgs e)
        {
            if (dgBatches.SelectedItem == null) return;
            dynamic sel = dgBatches.SelectedItem;
            if (MessageBox.Show($"Списать '{sel.BatchNumber}'?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var batch = _db.ReagentBatches.Find((int)sel.BatchID);
                if (batch != null) { batch.IsAvailable = false; _db.SaveChanges(); LoadBatches(); txtStatus.Text = "Списано"; }
            }
        }
        void BtnRefresh_Click(object s, RoutedEventArgs e) { LoadBatches(); txtSearch.Text = ""; }
        void TxtSearch_TextChanged(object s, TextChangedEventArgs e)
        {
            if (dgBatches.ItemsSource == null) return;
            string text = txtSearch.Text.ToLower();
            var view = System.Windows.Data.CollectionViewSource.GetDefaultView(dgBatches.ItemsSource);
            if (string.IsNullOrWhiteSpace(text)) { view.Filter = null; txtStatus.Text = $"Всего: {((System.Collections.ICollection)dgBatches.ItemsSource).Count}"; }
            else
            {
                view.Filter = item =>
                {
                    dynamic b = item;
                    return b.BatchNumber.ToLower().Contains(text) || b.ReagentName.ToLower().Contains(text) || b.SupplierName.ToLower().Contains(text);
                };
                int cnt = 0; foreach (var item in view) cnt++;
                txtStatus.Text = $"Найдено: {cnt}";
            }
        }
        void DgBatches_SelectionChanged(object s, SelectionChangedEventArgs e)
        { btnEdit.IsEnabled = btnDelete.IsEnabled = dgBatches.SelectedItem != null; }
    }
}