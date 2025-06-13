using Microsoft.EntityFrameworkCore;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.ScrollAxis;
using Syncfusion.UI.Xaml.TextInputLayout;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Expedicao.Views
{
    /// <summary>
    /// Interação lógica para ViewExpedicaoProduto.xam
    /// </summary>
    public partial class ViewExpedicaoProduto : UserControl
    {

        private ProdutoExpedidoModel ProdutoExpedido;
        RowColumnIndex previousrowColumnIdex = new RowColumnIndex(-1, -1);

        public ViewExpedicaoProduto()
        {
            InitializeComponent();
            try
            {
                //this.dataGrid.SearchHelper = new LocalizarHelperExt(dataGrid);
                //this.dataGrid.SearchHelper = new SearchHelperExtNew(this.dataGrid);
                this.Exped.SelectionController = new GridSelectionControllerExt(Exped);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void UserControl_Initialized(object sender, EventArgs e)
        {
            try
            {
                DataContext = new ExpedicaoProdutoViewModel();
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                ExpedicaoProdutoViewModel vm = (ExpedicaoProdutoViewModel)DataContext;
                vm.Aprovados = await Task.Run(vm.GetAprovadosAsync);
                vm.Medidas = await Task.Run(vm.GetMedidasAsync);
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show(ex.Message);
            }
        }

        private async void Aprovados_SelectionChanged(object sender, Syncfusion.UI.Xaml.Grid.SelectionChangedEventArgs e)
        {
            AprovadoModel? aprovado = ((SfMultiColumnDropDownControl)sender).SelectedItem as AprovadoModel;
            try
            {
                ExpedicaoProdutoViewModel vm = (ExpedicaoProdutoViewModel)DataContext;

                //this.dataGrid.ItemsSource = null;
                this.loadingExped.Visibility = Visibility.Hidden;
                this.loadingDetalhes.Visibility = Visibility.Visible;
                //this.dataGrid.ItemsSource = await Task.Run(async () => await new ExpedicaoViewModel().GetProdutoExpedidos((int)aprovado.IdAprovado));
                vm.ChkDetails = await Task.Run(() => vm.GetProdutoExpedidos(aprovado?.IdAprovado));
                this.loadingDetalhes.Visibility = Visibility.Hidden;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void DataGrid_SelectionChanged(object sender, GridSelectionChangedEventArgs e)
        {
            //[0] = {Syncfusion.UI.Xaml.Grid.GridCellInfo}
            try
            {

                ExpedicaoProdutoViewModel vm = (ExpedicaoProdutoViewModel)DataContext;

                vm.Exped = new ExpedModel();
                loadingExped.Visibility = Visibility.Visible;
                if (e.AddedItems.Count <= 0)
                    return;

                //ProdutoExpedido = (e.AddedItems[0] as GridRowInfo).RowData as ProdutoExpedidoModel;
                //vm.ChkDetail.CodDetalhesCompl

                Exped.ItemsSource = await Task.Run(() => vm.GetExpedsAsync(vm.ChkDetail.CodDetalhesCompl));
                loadingExped.Visibility = Visibility.Hidden;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Exped_AddNewRowInitiating(object sender, AddNewRowInitiatingEventArgs e)
        {
            ExpedicaoProdutoViewModel vm = (ExpedicaoProdutoViewModel)DataContext;
            ((ExpedModel)e.NewObject).CodDetalhesCompl = new long?((long)vm.ChkDetail.CodDetalhesCompl);
        }

        private void Exped_RecordDeleted(object sender, RecordDeletedEventArgs e)
        {

        }

        private async void Exped_RecordDeleting(object sender, RecordDeletingEventArgs e)
        {

            if (MessageBox.Show("Confirma a exclusão do item?", "Excluir", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                    ExpedModel data = (ExpedModel)e.Items[0];
                    ExpedicaoProdutoViewModel vm = (ExpedicaoProdutoViewModel)DataContext;
                    await Task.Run((() => vm.DeleteExpedAsync(data)));
                    e.Cancel = false;
                    Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                }
                catch (Exception ex)
                {
                    e.Cancel = true;
                    int num2 = (int)MessageBox.Show(ex.Message);
                    Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                }
            }
            else
                e.Cancel = true;
        }

        private async void Exped_CurrentCellValueChanged(object sender, CurrentCellValueChangedEventArgs e)
        {
            SfDataGrid grid = (SfDataGrid)sender;
            int columnindex = grid.ResolveToGridVisibleColumnIndex(e.RowColumnIndex.ColumnIndex);
            var column = grid.Columns[columnindex];
            if (column.GetType() == typeof(GridCheckBoxColumn) && column.MappingName == "BaiaVirtual")
            {
                try
                {
                    var rowIndex = grid.ResolveToRecordIndex(e.RowColumnIndex.RowIndex);
                    if (rowIndex > -1) 
                    {
                        var record = (ExpedModel)grid.View.Records[rowIndex].Data;
                        var value = record.BaiaVirtual;
                        Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                        ExpedicaoProdutoViewModel vm = (ExpedicaoProdutoViewModel)DataContext;
                        ExpedModel expedModel = await Task.Run(() => vm.AddExpedAsync(record));
                        Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                }
            }
        }

        private async void Exped_RowValidated(object sender, RowValidatedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                AprovadoModel? aprovado = this.aprovados.SelectedItem as AprovadoModel;
                var grid = ((SfDataGrid)sender);
                ExpedicaoProdutoViewModel vm = (ExpedicaoProdutoViewModel)DataContext;
                ExpedModel data = (ExpedModel)e.RowData;
                data.CodVol = $"{aprovado.SiglaServ}-{data.Volume}";
                ExpedModel expedModel = await Task.Run(() => vm.AddExpedAsync(data));
                //((ExpedModel)e.RowData).CodExped = expedModel.CodExped;
                grid.View.Records[grid.ResolveToRecordIndex(e.RowIndex)].Data = expedModel;
                grid.View.Refresh();

                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
        }

        private void Exped_RowValidating(object sender, RowValidatingEventArgs e)
        {
            ExpedModel rowData = (ExpedModel)e.RowData;
            ExpedicaoProdutoViewModel vm = (ExpedicaoProdutoViewModel)DataContext;
            if (!rowData.CodDetalhesCompl.HasValue)
            {
                e.IsValid = false;
                e.ErrorMessages.Add("QtdExpedida", "Erro ao selecionar a linha.");
                e.ErrorMessages.Add("VolExp", "Erro ao selecionar a linha.");
                e.ErrorMessages.Add("VolTotExp", "Erro ao selecionar a linha.");
                e.ErrorMessages.Add("Pl", "Erro ao selecionar a linha.");
                e.ErrorMessages.Add("Pb", "Erro ao selecionar a linha.");
                e.ErrorMessages.Add("Largura", "Erro ao selecionar a linha.");
                e.ErrorMessages.Add("Altura", "Erro ao selecionar a linha.");
                e.ErrorMessages.Add("Profundidade", "Erro ao selecionar a linha.");
                e.ErrorMessages.Add("ModeloCaixa", "Erro ao selecionar a linha.");
                e.ErrorMessages.Add("Volume", "Erro ao selecionar a linha.");
            }
            else
            {
                //decimal? qtdExpedida = (decimal?)rowData.QtdExpedida;
                if (!rowData.QtdExpedida.HasValue)
                {
                    e.IsValid = false;
                    e.ErrorMessages.Add("QtdExpedida", "qtd_expedida não pode ser nulo.");
                }
                else
                {
                    //Math.Round( 2.123455909, 2);
                    //qtdExpedida = (decimal?)rowData.QtdExpedida;
                    //decimal? nullable1 = (decimal?)this.ProdutoExpedido.Qtd;
                    if (Math.Round((double)rowData.QtdExpedida, 2) > Math.Round((double)vm.ChkDetail.Qtd, 2) & rowData.QtdExpedida.HasValue & vm.ChkDetail.Qtd.HasValue)
                    {
                        e.IsValid = false;
                        e.ErrorMessages.Add("QtdExpedida", "qtd_expedida não pode ser maior que qtd do cheklist.");
                    }
                    else
                    {

                        if (!rowData.VolExp.HasValue)
                        {
                            e.IsValid = false;
                            e.ErrorMessages.Add("VolExp", "vol_exp não pode ser nulo.");
                        }
                        else
                        {
                            if (!rowData.VolTotExp.HasValue)
                            {
                                e.IsValid = false;
                                e.ErrorMessages.Add("VolTotExp", "vol_tot_exp não pode ser nulo.");
                            }
                            else
                            {
                                if (!rowData.Pl.HasValue)
                                {
                                    e.IsValid = false;
                                    e.ErrorMessages.Add("Pl", "pl não pode ser nulo.");
                                }
                                else
                                {
                                    if (!rowData.Pb.HasValue)
                                    {
                                        e.IsValid = false;
                                        e.ErrorMessages.Add("Pb", "pb não pode ser nulo.");
                                    }
                                    else
                                    {
                                        if (rowData.ModeloCaixa == null)
                                        {
                                            if (!rowData.Largura.HasValue)
                                            {
                                                e.IsValid = false;
                                                e.ErrorMessages.Add("Largura", "Precisa informar uma das formas de medida.");
                                                return;
                                            }
                                            if (!rowData.Altura.HasValue)
                                            {
                                                e.IsValid = false;
                                                e.ErrorMessages.Add("Altura", "Precisa informar uma das formas de medida.");
                                                return;
                                            }
                                            if (!rowData.Profundidade.HasValue)
                                            {
                                                e.IsValid = false;
                                                e.ErrorMessages.Add("Profundidade", "Precisa informar uma das formas de medida.");
                                                return;
                                            }
                                        }
                                        if (rowData.ModeloCaixa != null && rowData.ModeloCaixa != "CX")
                                        {
                                            if (rowData.Largura.HasValue)
                                            {
                                                e.IsValid = false;
                                                e.ErrorMessages.Add("Largura", "Precisa informar apenas tipo da caixa ou as medidas.");
                                                return;
                                            }
                                            if (rowData.Altura.HasValue)
                                            {
                                                e.IsValid = false;
                                                e.ErrorMessages.Add("Altura", "Precisa informar apenas tipo da caixa ou as medidas.");
                                                return;
                                            }
                                            if (rowData.Profundidade.HasValue)
                                            {
                                                e.IsValid = false;
                                                e.ErrorMessages.Add("Profundidade", "Precisa informar apenas tipo da caixa ou as medidas.");
                                                return;
                                            }
                                        }
                                        if (rowData.ModeloCaixa == "CX")
                                        {
                                            if (!rowData.Largura.HasValue)
                                            {
                                                e.IsValid = false;
                                                e.ErrorMessages.Add("Largura", "Com tipo de caixa CX informado, precisa informar as medidas.");
                                                return;
                                            }
                                            if (!rowData.Altura.HasValue)
                                            {
                                                e.IsValid = false;
                                                e.ErrorMessages.Add("Altura", "Com tipo de caixa CX informado, precisa informar as medidas.");
                                                return;
                                            }
                                            if (!rowData.Profundidade.HasValue)
                                            {
                                                e.IsValid = false;
                                                e.ErrorMessages.Add("Profundidade", "Com tipo de caixa CX informado, precisa informar as medidas.");
                                                return;
                                            }

                                        }
                                        if (rowData.Volume.HasValue)
                                            return;
                                        e.IsValid = false;
                                        e.ErrorMessages.Add("Volume", "Informe o número do volume.");
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            /* 
                 MessageBox.Show("Ctrl+G detected, NO Alt/Shift/Windows");
            */

            //if ((e.Key == Key.G) && ((e.KeyboardDevice.Modifiers & ModifierKeys.Control) == ModifierKeys.Control))
            //if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && Keyboard.IsKeyDown(Key.L))
            if ((e.KeyboardDevice.Modifiers & ModifierKeys.Control) != ModifierKeys.None && e.Key == Key.L)
                Localizar();


        }

        private void Localizar()
        {
            this.dataGrid.SelectedItems.Clear();
            this.dataGrid.SearchHelper.ClearSearch();
            var window = new Window();
            var stackPanel = new StackPanel { Orientation = Orientation.Vertical };
            stackPanel.Margin = new Thickness(5, 5, 5, 5);
            var inputLayout = new SfTextInputLayout();
            inputLayout.Hint = "Localizar";
            TextBox textBox = new();
            textBox.PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter)
                    PerformSearch(textBox.Text);
                else if (e.Key == Key.Escape)
                    window.Close();
            };
            inputLayout.InputView = textBox;
            stackPanel.Children.Add(inputLayout);
            FocusManager.SetFocusedElement(stackPanel, textBox);
            window.Content = stackPanel;
            window.Title = "Localizar código expedição";
            window.Height = 120;
            window.Width = 350;
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            window.WindowStyle = WindowStyle.ToolWindow;
            window.ResizeMode = ResizeMode.NoResize;
            window.ShowDialog();
        }

        private void PerformSearch(string texto)
        {

            try
            {
                /*
                this.dataGrid.SearchHelper.FindNext(texto);
                var rowIndex = this.dataGrid.SearchHelper.CurrentRowColumnIndex.RowIndex;
                var recordIndex = this.dataGrid.ResolveToRecordIndex(rowIndex);
                this.dataGrid.SelectedIndex = recordIndex;
                */

                this.dataGrid.SearchHelper.FindNext(texto);
                this.dataGrid.SelectionController.MoveCurrentCell(this.dataGrid.SearchHelper.CurrentRowColumnIndex);
                var recored = this.dataGrid.SelectedItem;
                var viewmodel = this.dataGrid.DataContext as ExpedicaoViewModel;
                //if (previousrowColumnIdex != this.dataGrid.SearchHelper.CurrentRowColumnIndex)
                //    viewmodel.SearchItem.Add(recored);
                previousrowColumnIdex = this.dataGrid.SearchHelper.CurrentRowColumnIndex;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            


        }

    }

    public class SearchHelperExtNew : SearchHelper
    {
        public SearchHelperExtNew(SfDataGrid sfDataGrid) : base(sfDataGrid)
        {
        }

        protected override bool SearchCell(DataColumnBase column, object record, bool ApplySearchHighlightBrush)
        {
           if (column == null)
               return false;
           if (column.GridColumn.MappingName == "CodDetalhesCompl")
               return base.SearchCell(column, record, ApplySearchHighlightBrush);
           else
               return false;
        }
        

    }

    public class ExpedicaoProdutoViewModel : INotifyPropertyChanged
    {

        private ObservableCollection<AprovadoModel> aprovados;
        public ObservableCollection<AprovadoModel> Aprovados
        {
            get { return aprovados; }
            set { aprovados = value; RaisePropertyChanged("Aprovados"); }
        }

        private ObservableCollection<MedidaModel> medidas;
        public ObservableCollection<MedidaModel> Medidas
        {
            get { return medidas; }
            set { medidas = value; RaisePropertyChanged("Medidas"); }
        }

        private ProdutoExpedidoModel chkDetail;
        public ProdutoExpedidoModel ChkDetail
        {
            get { return chkDetail; }
            set { chkDetail = value; RaisePropertyChanged("ChkDetail"); }
        }

        private ObservableCollection<ProdutoExpedidoModel> chkDetails;
        public ObservableCollection<ProdutoExpedidoModel> ChkDetails
        {
            get { return chkDetails; }
            set { chkDetails = value; RaisePropertyChanged("ChkDetails"); }
        }

        private ExpedModel exped;
        public ExpedModel Exped
        {
            get { return exped; }
            set { exped = value; RaisePropertyChanged("Exped"); }
        }

        private ObservableCollection<ExpedModel> expeds;
        public ObservableCollection<ExpedModel> Expeds
        {
            get { return expeds; }
            set { expeds = value; RaisePropertyChanged("Expeds"); }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public async Task<ObservableCollection<AprovadoModel>> GetAprovadosAsync()
        {
            try
            {
                using AppDatabase db = new();
                var data = await db.Aprovados.OrderBy(c => c.SiglaServ).ToListAsync();
                return new ObservableCollection<AprovadoModel>(data);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ObservableCollection<MedidaModel>> GetMedidasAsync()
        {
            try
            {
                using AppDatabase db = new();
                var data = await db.Medidas.OrderBy((n => n.NomeCaixa)).ToListAsync();
                return new ObservableCollection<MedidaModel>(data);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ObservableCollection<ProdutoExpedidoModel>> GetProdutoExpedidos(int? IdAprovado)
        {
            try
            {
                using AppDatabase db = new();
                var data = await db.ProdutoExpedidos
                    .Where(n => n.IdAprovado == IdAprovado)
                    .OrderBy(n => n.ItemMemorial)
                    .ThenBy(n => n.DescricaoProduto)
                    .ToListAsync();

                return new ObservableCollection<ProdutoExpedidoModel>(data);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ObservableCollection<ExpedModel>> GetExpedsAsync(int? CodDetalhesCompl)
        {
            IList<ExpedModel> listAsync;
            try
            {
                using AppDatabase db = new();
                listAsync = await db.Expeds
                    .Where(n => n.CodDetalhesCompl == CodDetalhesCompl)
                    .OrderBy(n => n.Volume)
                    .ToListAsync();

                return new ObservableCollection<ExpedModel>(listAsync);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ExpedModel> AddExpedAsync(ExpedModel exped)
        {
            try
            {
                using AppDatabase db = new();
                var expedExistente = await db.Expeds.FindAsync(exped.CodExped);
                if (expedExistente == null)
                {
                    var result = await db.Expeds.AddAsync(exped);
                    await db.SaveChangesAsync();
                    await db.Entry(result.Entity).ReloadAsync();
                    return result.Entity;
                }
                else
                {
                    db.Entry(expedExistente).CurrentValues.SetValues(exped);
                    await db.SaveChangesAsync();
                    await db.Entry(expedExistente).ReloadAsync();
                    return expedExistente;
                }   
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task DeleteExpedAsync(ExpedModel exped)
        {
            try
            {
                using AppDatabase db = new();
                db.Entry<ExpedModel>(exped).State = EntityState.Deleted;
                int num = await db.SaveChangesAsync();
                db.Entry<ExpedModel>(exped).State = EntityState.Detached;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }

}