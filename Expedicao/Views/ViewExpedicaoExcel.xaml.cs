using CommunityToolkit.Mvvm.ComponentModel;
using Expedicao.Model;
using Microsoft.EntityFrameworkCore;
using Syncfusion.XlsIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Expedicao.Views;

/// <summary>
/// Interação lógica para ViewExpedicaoExcel.xam
/// </summary>
public partial class ViewExpedicaoExcel : UserControl
{
    public string Consulta { get; set; }
    DataBase BaseSettings = DataBase.Instance;

    public ViewExpedicaoExcel(string consulta)
    {
        InitializeComponent();
        DataContext = new ViewExpedicaoExcelViewModel();
        this.Consulta = consulta;
    }

    private async void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            //this.sfmcAprovados.ItemsSource = await Task.Run(() => new AprovadoViewModel().GetAprovados());

            if (DataContext is ViewExpedicaoExcelViewModel vm)
                await vm.GetAprovados();

            this.sfBusyIndicator.IsBusy = false;
            this.principal.Visibility = Visibility.Visible;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }

    private async void btnExcel_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            ExcelEngine excelEngine = new ExcelEngine();
            IApplication excel = excelEngine.Excel;
            excel.DefaultVersion = ExcelVersion.Xlsx;
            IWorkbook workbook = excel.Workbooks.Create(1);
            IWorksheet worksheet = workbook.Worksheets[0];

            await Task.Run(() => Dispatcher.Invoke(() => btnExcel.Visibility = Visibility.Hidden));
            await Task.Run(() => Dispatcher.Invoke(() => sfBusyIndicatorExcel.IsBusy = true));

            object dados = new();

            if (DataContext is ViewExpedicaoExcelViewModel vm)
            {
                //var aprovados = sfmcAprovados.SelectedItems;
                //ObservableCollection<AprovadoModel> aprovados = (ObservableCollection<AprovadoModel>)sfmcAprovados.SelectedItems;
                
                if (Consulta == "ITENS_FALTANTES")
                {
                    dados = await vm.GetItensFaltanteAsync();
                }
                else if (Consulta == "ITENS_CARREGADOS")
                {
                    dados = await vm.GetCarregamentoItensAsync();
                }
                else if (Consulta == "PRE_ITENS_FALTANTES")
                {
                    dados = await vm.GetPreItensFaltanteAsync();
                }
                else if (Consulta == "PRE_ITENS_CONFERIDOS")
                {
                    dados = await vm.GetPreItensShoppAsync();
                }
            }


            var arquivo = @$"{BaseSettings.CaminhoSistema}\Impressos\{Consulta}-{DateTime.Now:fffffff}.xlsx";
            ExcelImportDataOptions importDataOptions = new()
            {
                FirstRow = 1,
                FirstColumn = 1,
                IncludeHeader = true,
                PreserveTypes = true
            };
            worksheet.ImportData((IEnumerable)dados, importDataOptions);
            worksheet.UsedRange.AutofitColumns();
            workbook.SaveAs(arquivo);
            workbook.Close();
            excelEngine.Dispose();

            await Task.Run(() => Dispatcher.Invoke(() => btnExcel.Visibility = Visibility.Visible));
            await Task.Run(() => Dispatcher.Invoke(() => sfBusyIndicatorExcel.IsBusy = false));

            Process.Start(new ProcessStartInfo(arquivo)
            {
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
             MessageBox.Show(ex.Message );
        }
        
    }

}

public partial class ViewExpedicaoExcelViewModel : ObservableObject
{

    [ObservableProperty]
    private ObservableCollection<AprovadoModel> aprovados = [];

    [ObservableProperty]
    private ObservableCollection<object> selectedItems = [];


    public async Task GetAprovados()
    {
        using AppDatabase db = new();
        Aprovados = new ObservableCollection<AprovadoModel>(await db.Aprovados.OrderBy(c => c.SiglaServ).ToListAsync());
    }

    public async Task<ObservableCollection<CarregamentoItenFaltanteModel>> GetItensFaltanteAsync()
    {
        using AppDatabase db = new();
        var siglas = SelectedItems.Cast<AprovadoModel>().Select(x => x.SiglaServ).ToList(); 
        return new ObservableCollection<CarregamentoItenFaltanteModel>(
            await (from f in db.CarregamentoItenFaltantes
                   where siglas.Contains(f.Sigla)
                   select f).ToListAsync());
    }

    public async Task<ObservableCollection<CarregamentoItenShoppModel>> GetCarregamentoItensAsync()
    {
        using AppDatabase db = new();
        var siglas = SelectedItems.Cast<AprovadoModel>().Select(x => x.SiglaServ).ToList();
        return new ObservableCollection<CarregamentoItenShoppModel>(await db.CarregamentoItenShopps
            .Where(s => siglas.Contains(s.Sigla))
            .ToListAsync());
    }

    public async Task<ObservableCollection<PreConferenciaItemFaltanteModel>> GetPreItensFaltanteAsync()
    {
        using AppDatabase db = new();
        var siglas = SelectedItems.Cast<AprovadoModel>().Select(x => x.SiglaServ).ToList();
        return new ObservableCollection<PreConferenciaItemFaltanteModel>(await db.PreConferenciaItemFaltantes
            .Where(s => siglas.Contains(s.sigla))
            .ToListAsync());
    }

    public async Task<IList<PreConferenciaItemShoppModel>> GetPreItensShoppAsync()
    {
        using AppDatabase db = new();
        var siglas = SelectedItems.Cast<AprovadoModel>().Select(x => x.SiglaServ).ToList();
        return new ObservableCollection<PreConferenciaItemShoppModel>(await db.PreConferenciaItemShopps
            .Where(s => siglas.Contains(s.sigla))
            .ToListAsync());
    }
}

