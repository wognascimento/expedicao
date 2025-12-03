using Syncfusion.UI.Xaml.Grid;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Expedicao.Views
{
    /// <summary>
    /// Interação lógica para ViewExpedicaoRomaneios.xam
    /// </summary>
    public partial class ViewExpedicaoRomaneios : UserControl
    {
        public List<RomaneioModel> Romaneios = [];
        DataBase BaseSettings = DataBase.Instance;

        public string LocalAberto { get; set; }
        public RomaneioModel Romaneio { get; set; }

        public ViewExpedicaoRomaneios(string localAberto)
        {
            InitializeComponent();
            this.DataContext = new RomaneioViewModel();
            LocalAberto = localAberto;
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                RomaneioViewModel vm = (RomaneioViewModel)DataContext;
                vm.Romaneios = await Task.Run(vm.GetRomaneiosAsync);
                if (LocalAberto == "PRINCIPAL")
                {
                    BSelecionados.Visibility = Visibility.Visible;
                    loadingDetalhes.Visibility = Visibility.Hidden;
                    itens.SelectionMode = GridSelectionMode.Single;
                }
                else if(LocalAberto == "CARREGAMENTO")
                {
                    loadingDetalhes.Visibility = Visibility.Hidden;
                    BSelecionadosRomaneio.Visibility = Visibility.Visible;
                    itens.SelectionMode = GridSelectionMode.Multiple;
                    acao.Width = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            
           foreach (RomaneioModel selectedItem in itens.SelectedItems.Cast<RomaneioModel>())
           {
               Romaneios.Add(selectedItem);
           }

           Window.GetWindow(sender as DependencyObject).DialogResult = new bool?(true);
           
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            /*
            foreach (RomaneioModel selectedItem in itens.SelectedItems.Cast<RomaneioModel>())
            {
                Romaneios.Add(selectedItem);
            }

            Window.GetWindow(sender as DependencyObject).DialogResult = new bool?(true);
            */

            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });

                ExcelEngine excelEngine = new();
                IApplication excel = excelEngine.Excel;
                excel.DefaultVersion = ExcelVersion.Xlsx;
                IWorkbook workbook = excel.Workbooks.Create(1);
                IWorksheet worksheet = workbook.Worksheets[0];


                RomaneioViewModel vm = (RomaneioViewModel)DataContext;

                ExcelImportDataOptions importDataOptions = new ExcelImportDataOptions()
                {
                    FirstRow = 1,
                    FirstColumn = 1,
                    IncludeHeader = true,
                    PreserveTypes = true
                };
                worksheet.ImportData(vm.Romaneios, importDataOptions);
                worksheet.UsedRange.AutofitColumns();
                workbook.SaveAs(@$"{BaseSettings.CaminhoSistema}\Impressos\ROMANEIOS.xlsx");
                workbook.Close();
                excelEngine.Dispose();


                Process.Start(new ProcessStartInfo(@$"{BaseSettings.CaminhoSistema}\Impressos\ROMANEIOS.xlsx")
                {
                    UseShellExecute = true
                });

                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });

            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show(ex.Message);
            }

            
        }

    

        private void btnAcao_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RomaneioModel dataContext = (e.Source as Button).DataContext as RomaneioModel;
                if (this.LocalAberto == "PRINCIPAL")
                {
                    Window window = new Window();
                    window.Title = "EXPEDIÇÃO ROMANEIO " + dataContext.cod_romaneiro.ToString();
                    window.Content = new ViewExpedicaoRomaneio(dataContext);
                    window.SizeToContent = SizeToContent.WidthAndHeight;
                    window.ResizeMode = ResizeMode.NoResize;
                    window.ShowDialog();
                }
                else
                {
                    if (!(this.LocalAberto == "CARREGAMENTO"))
                        return;
                    foreach (RomaneioModel selectedItem in (Collection<object>)this.itens.SelectedItems)
                        this.Romaneios.Add(selectedItem);
                    Window.GetWindow(sender as DependencyObject).DialogResult = new bool?(true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
