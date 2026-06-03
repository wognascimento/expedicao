using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
            DataContext = new RomaneioViewModel();
            LocalAberto = localAberto;
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                RomaneioViewModel vm = (RomaneioViewModel)DataContext;
                vm.Romaneios = await Task.Run(vm.GetRomaneiosAsync);
                loadingDetalhes.IsBusy = false;
                loadingDetalhes.Visibility = Visibility.Hidden;

                if (LocalAberto == "PRINCIPAL")
                {
                    BSelecionados.Visibility = Visibility.Visible;
                    itens.SelectionMode = SelectionMode.Single;
                }
                else if (LocalAberto == "CARREGAMENTO")
                {
                    BSelecionadosRomaneio.Visibility = Visibility.Visible;
                    itens.SelectionMode = SelectionMode.Multiple;
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
            foreach (var selectedItem in itens.SelectedItems.Cast<RomaneioModel>())
            {
                Romaneios.Add(selectedItem);
            }

            Window.GetWindow(sender as DependencyObject).DialogResult = true;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });

                RomaneioViewModel vm = (RomaneioViewModel)DataContext;
                var outputPath = Path.Combine(BaseSettings.CaminhoSistema, "Impressos", "ROMANEIOS.xlsx");
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Romaneios");
                    var table = worksheet.Cell(1, 1).InsertTable(vm.Romaneios, "Romaneios", true);
                    table.Theme = XLTableTheme.TableStyleMedium2;
                    worksheet.Columns().AdjustToContents();
                    workbook.SaveAs(outputPath);
                }

                Process.Start(new ProcessStartInfo(outputPath)
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
                RomaneioModel? dataContext = (e.Source as Button)?.DataContext as RomaneioModel;
                if (dataContext is null)
                    return;

                if (LocalAberto == "PRINCIPAL")
                {
                    Window window = new()
                    {
                        Title = "EXPEDIÇÃO ROMANEIO " + dataContext.cod_romaneiro,
                        Content = new ViewExpedicaoRomaneio(dataContext),
                        SizeToContent = SizeToContent.WidthAndHeight,
                        ResizeMode = ResizeMode.NoResize
                    };
                    window.ShowDialog();
                }
                else if (LocalAberto == "CARREGAMENTO")
                {
                    foreach (var selectedItem in itens.SelectedItems.Cast<RomaneioModel>())
                    {
                        Romaneios.Add(selectedItem);
                    }

                    Window.GetWindow(sender as DependencyObject).DialogResult = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
