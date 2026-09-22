using CommunityToolkit.Mvvm.ComponentModel;
using Expedicao.Model;
using Expedicao.Utils;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

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
            if (DataContext is ViewExpedicaoExcelViewModel vm)
            {
                await vm.GetAprovados();
                ConfigurarFiltro(vm.Aprovados);
            }

            this.sfBusyIndicator.IsBusy = false;
            this.principal.Visibility = Visibility.Visible;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }

    private void ConfigurarFiltro(ObservableCollection<AprovadoModel> aprovados)
    {
        ICollectionView view = CollectionViewSource.GetDefaultView(aprovados);
        view.Filter = item =>
        {
            if (item is not AprovadoModel aprovado)
                return false;

            var filtro = txtFiltro.Text?.Trim();
            if (string.IsNullOrWhiteSpace(filtro))
                return true;

            return Contem(aprovado.Sigla, filtro)
                || Contem(aprovado.SiglaServ, filtro)
                || Contem(aprovado.Nome, filtro)
                || Contem(aprovado.Cidade, filtro);
        };
    }

    private static bool Contem(string? valor, string filtro)
    {
        return valor?.IndexOf(filtro, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private void txtFiltro_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (DataContext is ViewExpedicaoExcelViewModel vm)
            CollectionViewSource.GetDefaultView(vm.Aprovados)?.Refresh();
    }

    private async void btnExcel_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            btnExcel.Visibility = Visibility.Hidden;
            sfBusyIndicatorExcel.IsBusy = true;

            object dados = new();

            if (DataContext is ViewExpedicaoExcelViewModel vm)
            {
                vm.SelectedItems = new ObservableCollection<object>(aprovadosList.SelectedItems.Cast<object>());

                if (vm.SelectedItems.Count == 0)
                    throw new InvalidOperationException("Selecione ao menos um cliente.");
                
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


            var arquivo = Path.Combine(BaseSettings.CaminhoSistema, "Impressos", $"{Consulta}-{DateTime.Now:fffffff}.xlsx");
            Directory.CreateDirectory(Path.GetDirectoryName(arquivo)!);

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add(Consulta);
                ExcelExportHelper.WritePlainData(worksheet, (IEnumerable)dados);
                workbook.SaveAs(arquivo);
            }

            btnExcel.Visibility = Visibility.Visible;
            sfBusyIndicatorExcel.IsBusy = false;

            Process.Start(new ProcessStartInfo(arquivo)
            {
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            btnExcel.Visibility = Visibility.Visible;
            sfBusyIndicatorExcel.IsBusy = false;
            MessageBox.Show(ex.Message);
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

