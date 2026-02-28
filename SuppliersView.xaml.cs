using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
namespace ChemicalWarehouseWPF.Views
{
    public partial class SuppliersView : Page
    {
        private ChemicalWarehouseDBEntities3 _db;
        private ObservableCollection<SupplierViewModel> _list;
        public SuppliersView()
        {
            InitializeComponent();
            _db = new ChemicalWarehouseDBEntities3();
            _list = new ObservableCollection<SupplierViewModel>();
            dgSuppliers.ItemsSource = _list;
            Loaded += (s, e) => LoadSuppliers();
        }
        void LoadSuppliers()
        {
            try
            {
                _list.Clear();
                var data = _db.Suppliers.Select(s => new SupplierViewModel
                {
                    SupplierID = s.SupplierID,
                    SupplierName = s.SupplierName ?? "",
                    ContactPerson = s.ContactPerson ?? "",
                    PhoneNumber = s.PhoneNumber ?? "",
                    Email = s.Email ?? "",
                    INN = s.INN ?? "",
                    KPP = s.KPP ?? "",
                    Address = s.Address ?? "",
                    IsActive = s.IsActive ?? true
                }).ToList();
                foreach (var s in data) _list.Add(s);
                txtCount.Text = _list.Count.ToString();
                txtStatus.Text = _list.Count > 0 ? $"Загружено: {_list.Count}" : "Нет данных";
            }
            catch { txtStatus.Text = "Ошибка"; }
        }
        void BtnAdd_Click(object s, RoutedEventArgs e) => ShowDialog(null);
        void BtnEdit_Click(object s, RoutedEventArgs e)
        {
            if (dgSuppliers.SelectedItem is SupplierViewModel sel)
                ShowDialog(_db.Suppliers.Find(sel.SupplierID));
        }
        void ShowDialog(Suppliers supplier)
        {
            var dlg = new SupplierDialog(_db, supplier) { Owner = Window.GetWindow(this) };
            if (dlg.ShowDialog() == true) { LoadSuppliers(); txtStatus.Text = supplier == null ? "Добавлен" : "Обновлен"; }
        }
        void BtnRefresh_Click(object s, RoutedEventArgs e) { LoadSuppliers(); txtSearch.Text = ""; }
        void TxtSearch_TextChanged(object s, TextChangedEventArgs e) => ApplyFilter();
        void ApplyFilter()
        {
            if (_list.Count == 0) return;
            string text = txtSearch.Text?.ToLower() ?? "";
            if (string.IsNullOrWhiteSpace(text))
            { dgSuppliers.ItemsSource = _list; txtStatus.Text = $"Показано {_list.Count}"; }
            else
            {
                var filtered = _list.Where(s =>
                    s.SupplierName.ToLower().Contains(text) ||
                    s.ContactPerson.ToLower().Contains(text) ||
                    s.Email.ToLower().Contains(text) ||
                    s.INN.ToLower().Contains(text)).ToList();
                dgSuppliers.ItemsSource = filtered;
                txtStatus.Text = $"Найдено {filtered.Count} из {_list.Count}";
            }
        }
        void DgSuppliers_SelectionChanged(object s, SelectionChangedEventArgs e)
        { btnEdit.IsEnabled = dgSuppliers.SelectedItem != null; }
    }
    public class SupplierViewModel
    {
        public int SupplierID { get; set; }
        public string SupplierName { get; set; }
        public string ContactPerson { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string INN { get; set; }
        public string KPP { get; set; }
        public string Address { get; set; }
        public bool IsActive { get; set; }
    }
}