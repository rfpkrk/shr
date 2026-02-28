using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ChemicalWarehouseWPF.Models;

namespace ChemicalWarehouseWPF.Views
{
    public partial class ReagentsView : Page
    {
        private ChemicalWarehouseDBEntities3 _db;
        public ReagentsView()
        {
            InitializeComponent();
            _db = new ChemicalWarehouseDBEntities3();
            Loaded += (s, e) => LoadReagents();
        }
        void LoadReagents()
        {
            try
            {
                var data = _db.Reagents.Select(r => new ReagentViewModel
                {
                    ReagentID = r.ReagentID,
                    ReagentCode = r.ReagentCode ?? "",
                    ReagentName = r.ReagentName ?? "",
                    ChemicalFormula = r.ChemicalFormula ?? "",
                    CASNumber = r.CASNumber ?? "",
                    TypeName = r.ReagentTypes.TypeName,
                    HazardClass = r.HazardClasses.ClassName,
                    UnitSymbol = r.UnitsOfMeasure.UnitSymbol,
                    MinStockLevel = r.MinStockLevel,
                    MaxStockLevel = r.MaxStockLevel,
                    IsActive = r.IsActive ?? true
                }).ToList();
                dgReagents.ItemsSource = data;
                txtCount.Text = data.Count.ToString();
                txtStatus.Text = data.Count > 0 ? $"Загружено: {data.Count}" : "Нет данных";
            }
            catch (Exception ex) { MessageBox.Show($"Ошибка: {ex.Message}"); }
        }
        void BtnRefresh_Click(object s, RoutedEventArgs e) => LoadReagents();
        void BtnAdd_Click(object s, RoutedEventArgs e) => ShowDialog(null);
        void BtnEdit_Click(object s, RoutedEventArgs e) => Edit();
        void DgReagents_MouseDoubleClick(object s, MouseButtonEventArgs e) => Edit();
        void Edit()
        {
            if (dgReagents.SelectedItem is ReagentViewModel sel)
                ShowDialog(_db.Reagents.Find(sel.ReagentID));
        }
        void ShowDialog(Reagents reagent)
        {
            var dlg = new ReagentDialog(_db, reagent) { Owner = Window.GetWindow(this) };
            if (dlg.ShowDialog() == true) LoadReagents();
        }
        void BtnDelete_Click(object s, RoutedEventArgs e)
        {
            if (!(dgReagents.SelectedItem is ReagentViewModel sel)) return;
            if (MessageBox.Show($"Удалить '{sel.ReagentName}'?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var r = _db.Reagents.Find(sel.ReagentID);
                if (r == null) return;
                if (_db.ReagentBatches.Any(b => b.ReagentID == sel.ReagentID))
                    r.IsActive = false;
                else
                    _db.Reagents.Remove(r);
                _db.SaveChanges();
                LoadReagents();
            }
        }
        void DgReagents_SelectionChanged(object s, SelectionChangedEventArgs e)
        { btnEdit.IsEnabled = btnDelete.IsEnabled = dgReagents.SelectedItem != null; }
    }
    public class ReagentViewModel
    {
        public int ReagentID { get; set; }
        public string ReagentCode { get; set; }
        public string ReagentName { get; set; }
        public string ChemicalFormula { get; set; }
        public string CASNumber { get; set; }
        public string TypeName { get; set; }
        public string HazardClass { get; set; }
        public string UnitSymbol { get; set; }
        public decimal? MinStockLevel { get; set; }
        public decimal? MaxStockLevel { get; set; }
        public bool IsActive { get; set; }
    }
}