using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Telerik.Windows.Controls;

namespace Expedicao.Views
{
    /// <summary>
    /// Interação lógica para ViewCubagemClienteZerada.xam
    /// </summary>
    public partial class ViewCubagemClienteZerada : UserControl
    {
        public ViewCubagemClienteZerada()
        {
            InitializeComponent();
            DataContext = new ViewCubagemClienteZeradaViewModel();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                ViewCubagemClienteZeradaViewModel vm = (ViewCubagemClienteZeradaViewModel)DataContext;
                vm.Cubagens = await vm.GetItensAsync();
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show(ex.Message);
            }
        }

        private void dataGrid_RowValidating(object sender, GridViewRowValidatingEventArgs e)
        {
            if (e.Row.Item is not CubagemPrevistaClienteModel data) return;
            var vm = (ViewCubagemClienteZeradaViewModel)DataContext;
            ValidatedGridSave.Save(sender, e, () => vm.SaveItemAsync(data));
        }
    }

    public class ViewCubagemClienteZeradaViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public void RaisePropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        private CubagemPrevistaClienteModel cubagem;
        public CubagemPrevistaClienteModel Cubagem
        {
            get { return cubagem; }
            set { cubagem = value; RaisePropertyChanged("Cubagem"); }
        }

        private ObservableCollection<CubagemPrevistaClienteModel> cubagens;
        public ObservableCollection<CubagemPrevistaClienteModel> Cubagens
        {
            get { return cubagens; }
            set { cubagens = value; RaisePropertyChanged("Cubagens"); }
        }

        public async Task<ObservableCollection<CubagemPrevistaClienteModel>> GetItensAsync()
        {
            try
            {
                using DapperDatabase db = new();
                var data = await db.ReadAsync<CubagemPrevistaClienteModel>("\"cubagem\" = 0");
                return new ObservableCollection<CubagemPrevistaClienteModel>(data);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<CubagemPrevistaClienteModel> SaveItemAsync(CubagemPrevistaClienteModel cubagem)
        {
            try
            {
                using DapperDatabase db = new();
                await db.UpdateAsync(cubagem, nameof(CubagemPrevistaClienteModel.cubagem));
                return cubagem;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
