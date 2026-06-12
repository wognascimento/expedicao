using Dapper;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;

namespace Expedicao.Views
{
    /// <summary>
    /// Interação lógica para ViewExpedicaoProduto.xam
    /// </summary>
    public partial class ViewExpedicaoProduto : UserControl
    {

        private ProdutoExpedidoModel ProdutoExpedido;
        private bool _salvandoColagem;

        public ViewExpedicaoProduto()
        {
            InitializeComponent();
        }

        private async void UserControl_Initialized(object sender, EventArgs e)
        {
            try
            {
                DataContext = new ExpedicaoProdutoViewModel();
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                ExpedicaoProdutoViewModel vm = (ExpedicaoProdutoViewModel)DataContext;
                vm.Aprovados = await vm.GetAprovadosAsync();
                vm.Medidas = await vm.GetMedidasAsync();
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show(ex.Message);
            }
        }

        private async void Aprovados_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AprovadoModel? aprovado = aprovados.SelectedItem as AprovadoModel;
            try
            {
                if (aprovado is null)
                    return;

                ExpedicaoProdutoViewModel vm = (ExpedicaoProdutoViewModel)DataContext;

                this.loadingExped.Visibility = Visibility.Hidden;
                this.loadingDetalhes.Visibility = Visibility.Visible;
                vm.ChkDetails = await vm.GetProdutoExpedidos(aprovado.IdAprovado);
                dataGrid.FilterDescriptors.Clear();
                Exped.FilterDescriptors.Clear();
                vm.Expeds = [];
                this.loadingDetalhes.Visibility = Visibility.Hidden;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void DataGrid_SelectionChanged(object sender, SelectionChangeEventArgs e)
        {
            try
            {

                ExpedicaoProdutoViewModel vm = (ExpedicaoProdutoViewModel)DataContext;

                vm.Exped = new ExpedModel();
                loadingExped.Visibility = Visibility.Visible;
                if (vm.ChkDetail is null)
                    return;

                vm.Expeds = await vm.GetExpedsAsync(vm.ChkDetail.CodDetalhesCompl);
                loadingExped.Visibility = Visibility.Hidden;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Exped_AddingNewDataItem(object sender, GridViewAddingNewEventArgs e)
        {
            ExpedicaoProdutoViewModel vm = (ExpedicaoProdutoViewModel)DataContext;
            e.NewObject = new ExpedModel
            {
                CodDetalhesCompl = vm.ChkDetail?.CodDetalhesCompl
            };
        }

        private void Exped_CopyRow_Click(object sender, RoutedEventArgs e)
        {
            CopiarLinhasExped();
        }

        private async void Exped_Paste_Click(object sender, RoutedEventArgs e)
        {
            await ColarLinhasExpedAsync();
        }

        private void Exped_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var row = FindParent<GridViewRow>(e.OriginalSource as DependencyObject);
            if (row?.Item is not ExpedModel item)
                return;

            Exped.SelectedItem = item;
            Exped.CurrentItem = item;
            row.IsSelected = true;
        }

        private async Task ColarLinhasExpedAsync()
        {
            if (_salvandoColagem)
                return;

            try
            {
                _salvandoColagem = true;
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });

                ExpedicaoProdutoViewModel vm = (ExpedicaoProdutoViewModel)DataContext;
                if (vm.Expeds is null)
                    return;

                var linhasNovas = ObterLinhasExpedDoClipboard(vm);
                if (linhasNovas.Count == 0)
                    return;

                foreach (var linha in linhasNovas)
                {
                    linha.CodDetalhesCompl ??= vm.ChkDetail?.CodDetalhesCompl;
                    PrepararLinhaParaSalvar(linha);

                    if (!LinhaValidaParaSalvar(linha, out var mensagem))
                        throw new InvalidOperationException(mensagem);

                    var salvo = await vm.AddExpedAsync(linha);
                    vm.Expeds.Add(salvo);
                }

                Exped.Rebind();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Colar expedição", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _salvandoColagem = false;
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
        }

        private async void Exped_Deleting(object sender, GridViewDeletingEventArgs e)
        {

            if (MessageBox.Show("Confirma a exclusão do item?", "Excluir", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                    ExpedicaoProdutoViewModel vm = (ExpedicaoProdutoViewModel)DataContext;
                    var items = e.Items.OfType<ExpedModel>().ToList();
                    await vm.DeleteExpedsAsync(items);
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

        private async void Exped_RowValidated(object sender, GridViewRowValidatedEventArgs e)
        {
            try
            {
                if (e.Row.Item is not ExpedModel data)
                    return;

                if (LinhaNovaVazia(data))
                {
                    RemoverLinhaNovaVazia(data);
                    return;
                }

                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                AprovadoModel? aprovado = this.aprovados.SelectedItem as AprovadoModel;
                ExpedicaoProdutoViewModel vm = (ExpedicaoProdutoViewModel)DataContext;
                var novaLinha = !data.CodExped.HasValue || data.CodExped.Value <= 0;
                PrepararLinhaParaSalvar(data);
                ExpedModel expedModel = await vm.AddExpedAsync(data);
                AtualizarLinhaExped(data, expedModel);

                if (vm.Expeds is not null)
                {
                    var index = vm.Expeds.IndexOf(data);
                    if (index >= 0)
                        vm.Expeds[index] = data;
                }

                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });

                if (novaLinha && !_salvandoColagem)
                    PosicionarParaNovaLinhaExped();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
        }

        private void Exped_CellEditEnded(object sender, GridViewCellEditEndedEventArgs e)
        {
            if (e.Cell?.DataContext is not ExpedModel data)
                return;

            if (!TemMedidasInformadas(data))
                return;

            if (!string.IsNullOrWhiteSpace(data.ModeloCaixa) &&
                !string.Equals(data.ModeloCaixa, "CX", StringComparison.OrdinalIgnoreCase))
            {
                data.ModeloCaixa = null;
                Exped.Rebind();
            }
        }

        private void PosicionarParaNovaLinhaExped()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                Exped.Focus();
                Exped.CurrentColumn = Exped.Columns.OfType<GridViewDataColumn>().FirstOrDefault();
                Exped.BeginInsert();
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void CopiarLinhasExped()
        {
            var linhas = Exped.SelectedItems.OfType<ExpedModel>().ToList();
            if (linhas.Count == 0 && Exped.CurrentItem is ExpedModel item)
                linhas.Add(item);

            if (linhas.Count == 0)
                return;

            var texto = new StringBuilder();
            foreach (var linha in linhas)
            {
                texto.AppendLine(string.Join('\t',
                    FormatarClipboard(linha.QtdExpedida),
                    FormatarClipboard(linha.VolExp),
                    FormatarClipboard(linha.VolTotExp),
                    FormatarClipboard(linha.Pl),
                    FormatarClipboard(linha.Pb),
                    FormatarClipboard(linha.Largura),
                    FormatarClipboard(linha.Altura),
                    FormatarClipboard(linha.Profundidade),
                    linha.ModeloCaixa ?? string.Empty,
                    FormatarClipboard(linha.Volume),
                    linha.BaiaVirtual ?? string.Empty));
            }

            Clipboard.SetText(texto.ToString().TrimEnd());
        }

        private List<ExpedModel> ObterLinhasExpedDoClipboard(ExpedicaoProdutoViewModel vm)
        {
            if (!Clipboard.ContainsText())
                return [];

            var linhas = Clipboard.GetText()
                .Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries)
                .Select(l => l.Split('\t'))
                .Where(colunas => colunas.Length >= 8)
                .ToList();

            var ultimoVolume = vm.Expeds?
                .Where(e => e.Volume.HasValue)
                .Select(e => e.Volume!.Value)
                .DefaultIfEmpty(0)
                .Max() ?? 0;

            var resultado = new List<ExpedModel>();
            foreach (var colunas in linhas)
            {
                resultado.Add(new ExpedModel
                {
                    CodDetalhesCompl = vm.ChkDetail?.CodDetalhesCompl,
                    QtdExpedida = ConverterDouble(colunas.ElementAtOrDefault(0)),
                    VolExp = ConverterInt(colunas.ElementAtOrDefault(1)),
                    VolTotExp = ConverterInt(colunas.ElementAtOrDefault(2)),
                    Pl = ConverterDouble(colunas.ElementAtOrDefault(3)),
                    Pb = ConverterDouble(colunas.ElementAtOrDefault(4)),
                    Largura = ConverterDouble(colunas.ElementAtOrDefault(5)),
                    Altura = ConverterDouble(colunas.ElementAtOrDefault(6)),
                    Profundidade = ConverterDouble(colunas.ElementAtOrDefault(7)),
                    ModeloCaixa = NormalizarTexto(colunas.ElementAtOrDefault(8)),
                    Volume = ++ultimoVolume,
                    BaiaVirtual = NormalizarTexto(colunas.ElementAtOrDefault(10))
                });
            }

            return resultado;
        }

        private void PrepararLinhaParaSalvar(ExpedModel data)
        {
            AprovadoModel? aprovado = aprovados.SelectedItem as AprovadoModel;
            data.CodVol = $"{aprovado?.SiglaServ}-{data.Volume}";
        }

        private static string FormatarClipboard(double? valor)
        {
            return valor?.ToString("0.###", CultureInfo.CurrentCulture) ?? string.Empty;
        }

        private static string FormatarClipboard(int? valor)
        {
            return valor?.ToString(CultureInfo.CurrentCulture) ?? string.Empty;
        }

        private static double? ConverterDouble(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return null;

            texto = texto.Trim();
            var culturas = new[]
            {
                CultureInfo.CurrentCulture,
                CultureInfo.GetCultureInfo("pt-BR"),
                CultureInfo.InvariantCulture
            };

            foreach (var cultura in culturas)
            {
                if (double.TryParse(texto, NumberStyles.Number, cultura, out var valor))
                    return valor;
            }

            return null;
        }

        private static int? ConverterInt(string? texto)
        {
            var valor = ConverterDouble(texto);
            return valor.HasValue ? Convert.ToInt32(valor.Value) : null;
        }

        private static string? NormalizarTexto(string? texto)
        {
            return string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
        }

        private bool LinhaValidaParaSalvar(ExpedModel rowData, out string mensagem)
        {
            mensagem = string.Empty;

            ExpedicaoProdutoViewModel vm = (ExpedicaoProdutoViewModel)DataContext;
            if (!rowData.CodDetalhesCompl.HasValue)
                return Falha("Erro ao selecionar a linha.", out mensagem);

            if (!rowData.QtdExpedida.HasValue)
                return Falha("qtd_expedida não pode ser nulo.", out mensagem);

            if (vm.ChkDetail?.Qtd.HasValue == true &&
                Math.Round((double)rowData.QtdExpedida, 2) > Math.Round((double)vm.ChkDetail.Qtd, 2))
                return Falha("qtd_expedida não pode ser maior que qtd do cheklist.", out mensagem);

            if (!rowData.VolExp.HasValue)
                return Falha("vol_exp não pode ser nulo.", out mensagem);

            if (!rowData.VolTotExp.HasValue)
                return Falha("vol_tot_exp não pode ser nulo.", out mensagem);

            if (!rowData.Pl.HasValue)
                return Falha("pl não pode ser nulo.", out mensagem);

            if (!rowData.Pb.HasValue)
                return Falha("pb não pode ser nulo.", out mensagem);

            if (rowData.ModeloCaixa == null)
            {
                if (!rowData.Largura.HasValue || !rowData.Altura.HasValue || !rowData.Profundidade.HasValue)
                    return Falha("Precisa informar uma das formas de medida.", out mensagem);
            }

            if (!string.IsNullOrWhiteSpace(rowData.ModeloCaixa) && rowData.ModeloCaixa != "CX")
            {
                if (TemMedidasInformadas(rowData))
                    return Falha("Quando informar largura, altura e profundidade, só pode selecionar o modelo de caixa CX.", out mensagem);

                if (rowData.Largura.HasValue || rowData.Altura.HasValue || rowData.Profundidade.HasValue)
                    return Falha("Precisa informar apenas tipo da caixa ou as medidas.", out mensagem);
            }

            if (rowData.ModeloCaixa == "CX" &&
                (!rowData.Largura.HasValue || !rowData.Altura.HasValue || !rowData.Profundidade.HasValue))
                return Falha("Com tipo de caixa CX informado, precisa informar as medidas.", out mensagem);

            if (!rowData.Volume.HasValue)
                return Falha("Informe o número do volume.", out mensagem);

            return true;
        }

        private static bool Falha(string texto, out string mensagem)
        {
            mensagem = texto;
            return false;
        }

        private static T? FindParent<T>(DependencyObject? child)
            where T : DependencyObject
        {
            while (child != null)
            {
                if (child is T parent)
                    return parent;

                child = VisualTreeHelper.GetParent(child);
            }

            return null;
        }

        private void Exped_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && e.Key == Key.C)
            {
                CopiarLinhasExped();
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && e.Key == Key.V)
            {
                _ = ColarLinhasExpedAsync();
                e.Handled = true;
                return;
            }

            if (e.Key != Key.Enter && e.Key != Key.Return)
                return;

            e.Handled = true;

            if (!Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) &&
                Exped.CurrentColumn?.UniqueName == "Profundidade")
            {
                Exped.CurrentColumn = Caixas;
                Exped.Focus();
                Exped.BeginEdit();
                return;
            }

            if (Keyboard.FocusedElement is UIElement focusedElement)
            {
                var direction = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift)
                    ? System.Windows.Input.FocusNavigationDirection.Previous
                    : System.Windows.Input.FocusNavigationDirection.Next;

                focusedElement.MoveFocus(new TraversalRequest(direction));
            }
        }

        private static void AtualizarLinhaExped(ExpedModel destino, ExpedModel origem)
        {
            destino.CodExped = origem.CodExped;
            destino.QtdExpedida = origem.QtdExpedida;
            destino.VolExp = origem.VolExp;
            destino.VolTotExp = origem.VolTotExp;
            destino.Pl = origem.Pl;
            destino.Pb = origem.Pb;
            destino.Largura = origem.Largura;
            destino.Altura = origem.Altura;
            destino.Profundidade = origem.Profundidade;
            destino.CodVol = origem.CodVol;
            destino.CadastradoPor = origem.CadastradoPor;
            destino.Quando = origem.Quando;
            destino.BaiaVirtual = origem.BaiaVirtual;
            destino.ModeloCaixa = origem.ModeloCaixa;
            destino.CodDetalhesCompl = origem.CodDetalhesCompl;
            destino.AlteradoPor = origem.AlteradoPor;
            destino.AlteradoQuando = origem.AlteradoQuando;
            destino.InseridoPor = origem.InseridoPor;
            destino.InseridoEm = origem.InseridoEm;
            destino.Operacao = origem.Operacao;
            destino.Volume = origem.Volume;
            destino.nf_emitida = origem.nf_emitida;
        }

        private static bool TemMedidasInformadas(ExpedModel data)
        {
            return data.Largura.HasValue &&
                   data.Altura.HasValue &&
                   data.Profundidade.HasValue;
        }

        private static bool LinhaNovaVazia(ExpedModel data)
        {
            return data.CodExped.GetValueOrDefault() <= 0 &&
                   !data.QtdExpedida.HasValue &&
                   !data.VolExp.HasValue &&
                   !data.VolTotExp.HasValue &&
                   !data.Pl.HasValue &&
                   !data.Pb.HasValue &&
                   !data.Largura.HasValue &&
                   !data.Altura.HasValue &&
                   !data.Profundidade.HasValue &&
                   string.IsNullOrWhiteSpace(data.ModeloCaixa) &&
                   !data.Volume.HasValue &&
                   string.IsNullOrWhiteSpace(data.BaiaVirtual);
        }

        private void RemoverLinhaNovaVazia(ExpedModel data)
        {
            if (DataContext is not ExpedicaoProdutoViewModel vm || vm.Expeds is null)
                return;

            if (vm.Expeds.Contains(data))
            {
                vm.Expeds.Remove(data);
                Exped.Rebind();
            }
        }

        private void Exped_RowValidating(object sender, GridViewRowValidatingEventArgs e)
        {
            if (e.Row.Item is not ExpedModel rowData)
                return;

            if (LinhaNovaVazia(rowData))
                return;

            ExpedicaoProdutoViewModel vm = (ExpedicaoProdutoViewModel)DataContext;
            if (!rowData.CodDetalhesCompl.HasValue)
            {
                AplicarErroLinha(e, "Erro ao selecionar a linha.");
            }
            else
            {
                //decimal? qtdExpedida = (decimal?)rowData.QtdExpedida;
                if (!rowData.QtdExpedida.HasValue)
                {
                    AplicarErroLinha(e, "qtd_expedida não pode ser nulo.", "QtdExpedida");
                }
                else
                {
                    //Math.Round( 2.123455909, 2);
                    //qtdExpedida = (decimal?)rowData.QtdExpedida;
                    //decimal? nullable1 = (decimal?)this.ProdutoExpedido.Qtd;
                    if (Math.Round((double)rowData.QtdExpedida, 2) > Math.Round((double)vm.ChkDetail.Qtd, 2) & rowData.QtdExpedida.HasValue & vm.ChkDetail.Qtd.HasValue)
                    {
                        AplicarErroLinha(e, "qtd_expedida não pode ser maior que qtd do cheklist.", "QtdExpedida");
                    }
                    else
                    {

                        if (!rowData.VolExp.HasValue)
                        {
                            AplicarErroLinha(e, "vol_exp não pode ser nulo.", "VolExp");
                        }
                        else
                        {
                            if (!rowData.VolTotExp.HasValue)
                            {
                                AplicarErroLinha(e, "vol_tot_exp não pode ser nulo.", "VolTotExp");
                            }
                            else
                            {
                                if (!rowData.Pl.HasValue)
                                {
                                    AplicarErroLinha(e, "pl não pode ser nulo.", "Pl");
                                }
                                else
                                {
                                    if (!rowData.Pb.HasValue)
                                    {
                                        AplicarErroLinha(e, "pb não pode ser nulo.", "Pb");
                                    }
                                    else
                                    {
                                        if (rowData.ModeloCaixa == null)
                                        {
                                            if (!rowData.Largura.HasValue)
                                            {
                                                AplicarErroLinha(e, "Precisa informar uma das formas de medida.", "Largura");
                                                return;
                                            }
                                            if (!rowData.Altura.HasValue)
                                            {
                                                AplicarErroLinha(e, "Precisa informar uma das formas de medida.", "Altura");
                                                return;
                                            }
                                            if (!rowData.Profundidade.HasValue)
                                            {
                                                AplicarErroLinha(e, "Precisa informar uma das formas de medida.", "Profundidade");
                                                return;
                                            }
                                        }
                                        if (!string.IsNullOrWhiteSpace(rowData.ModeloCaixa) && rowData.ModeloCaixa != "CX")
                                        {
                                            if (TemMedidasInformadas(rowData))
                                            {
                                                AplicarErroLinha(e, "Quando informar largura, altura e profundidade, só pode selecionar o modelo de caixa CX.", "ModeloCaixa");
                                                return;
                                            }
                                            if (rowData.Largura.HasValue)
                                            {
                                                AplicarErroLinha(e, "Precisa informar apenas tipo da caixa ou as medidas.", "Largura");
                                                return;
                                            }
                                            if (rowData.Altura.HasValue)
                                            {
                                                AplicarErroLinha(e, "Precisa informar apenas tipo da caixa ou as medidas.", "Altura");
                                                return;
                                            }
                                            if (rowData.Profundidade.HasValue)
                                            {
                                                AplicarErroLinha(e, "Precisa informar apenas tipo da caixa ou as medidas.", "Profundidade");
                                                return;
                                            }
                                        }
                                        if (rowData.ModeloCaixa == "CX")
                                        {
                                            if (!rowData.Largura.HasValue)
                                            {
                                                AplicarErroLinha(e, "Com tipo de caixa CX informado, precisa informar as medidas.", "Largura");
                                                return;
                                            }
                                            if (!rowData.Altura.HasValue)
                                            {
                                                AplicarErroLinha(e, "Com tipo de caixa CX informado, precisa informar as medidas.", "Altura");
                                                return;
                                            }
                                            if (!rowData.Profundidade.HasValue)
                                            {
                                                AplicarErroLinha(e, "Com tipo de caixa CX informado, precisa informar as medidas.", "Profundidade");
                                                return;
                                            }

                                        }
                                        if (rowData.Volume.HasValue)
                                            return;
                                        AplicarErroLinha(e, "Informe o número do volume.", "Volume");
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void AplicarErroLinha(GridViewRowValidatingEventArgs e, string mensagem, string propertyName = "")
        {
            e.IsValid = false;
            e.ValidationResults.Add(new GridViewCellValidationResult
            {
                ErrorMessage = mensagem,
                PropertyName = propertyName
            });
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
            var window = new Window();
            var stackPanel = new StackPanel { Orientation = Orientation.Vertical };
            stackPanel.Margin = new Thickness(5, 5, 5, 5);
            TextBox textBox = new();
            textBox.Margin = new Thickness(0, 0, 0, 8);
            textBox.PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter)
                    PerformSearch(textBox.Text);
                else if (e.Key == Key.Escape)
                    window.Close();
            };
            stackPanel.Children.Add(new TextBlock { Text = "Localizar", Margin = new Thickness(0, 0, 0, 4) });
            stackPanel.Children.Add(textBox);
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
                if (DataContext is not ExpedicaoProdutoViewModel vm || vm.ChkDetails is null)
                    return;

                var item = vm.ChkDetails.FirstOrDefault(i =>
                    Contem(i.CodDetalhesCompl?.ToString(), texto) ||
                    Contem(i.ItemMemorial, texto) ||
                    Contem(i.LocalShoppings, texto) ||
                    Contem(i.Planilha, texto) ||
                    Contem(i.DescricaoProduto, texto));

                if (item is null)
                    return;

                dataGrid.SelectedItem = item;
                dataGrid.ScrollIntoView(item);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            


        }

        private static bool Contem(string? valor, string texto)
        {
            return valor?.IndexOf(texto, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }

    public class ExpedicaoProdutoViewModel : INotifyPropertyChanged
    {
        private readonly DataBase BaseSettings = DataBase.Instance;

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


        public event PropertyChangedEventHandler? PropertyChanged;
        public void RaisePropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public async Task<ObservableCollection<AprovadoModel>> GetAprovadosAsync()
        {
            using var conn = CreateConnection();
            var data = await conn.QueryAsync<AprovadoModel>(
                @"
                    SELECT
                        id_aprovado AS ""IdAprovado"",
                        sigla AS ""Sigla"",
                        sigla_serv AS ""SiglaServ"",
                        nome AS ""Nome"",
                        cidade AS ""Cidade"",
                        tema AS ""Tema""
                    FROM producao.t_aprovados
                    ORDER BY sigla_serv;");

            return new ObservableCollection<AprovadoModel>(data);
        }

        public async Task<ObservableCollection<MedidaModel>> GetMedidasAsync()
        {
            using var conn = CreateConnection();
            var data = await conn.QueryAsync<MedidaModel>(
                @"
                    SELECT
                        nomecx AS ""NomeCaixa"",
                        alt AS ""Altura"",
                        larg AS ""Largura"",
                        prof AS ""Profundidade"",
                        m3 AS ""Cubagem""
                    FROM producao.tblmedidas
                    ORDER BY nomecx;");

            return new ObservableCollection<MedidaModel>(data);
        }

        public async Task<ObservableCollection<ProdutoExpedidoModel>> GetProdutoExpedidos(int? IdAprovado)
        {
            using var conn = CreateConnection();
            var data = await conn.QueryAsync<ProdutoExpedidoModel>(
                @"
                    SELECT
                        coddetalhescompl AS ""CodDetalhesCompl"",
                        id_aprovado AS ""IdAprovado"",
                        sigla AS ""Sigla"",
                        local_shoppings AS ""LocalShoppings"",
                        planilha AS ""Planilha"",
                        qtd AS ""Qtd"",
                        descricao_produto AS ""DescricaoProduto"",
                        codcompl AS ""CodCompl"",
                        codcompladicional AS ""CodComplAdicional"",
                        item_memorial AS ""ItemMemorial""
                    FROM expedicao.qry_produto_expedido
                    WHERE id_aprovado = @IdAprovado
                    ORDER BY item_memorial, descricao_produto;",
                new { IdAprovado });

            return new ObservableCollection<ProdutoExpedidoModel>(data);
        }

        public async Task<ObservableCollection<ExpedModel>> GetExpedsAsync(int? CodDetalhesCompl)
        {
            using var conn = CreateConnection();
            var data = await conn.QueryAsync<ExpedModel>(
                $@"
                    SELECT {ExpedSelectColumns}
                    FROM expedicao.t_exped
                    WHERE coddetalhescompl = @CodDetalhesCompl
                    ORDER BY volume;",
                new { CodDetalhesCompl });

            return new ObservableCollection<ExpedModel>(data);
        }

        public async Task<ExpedModel> AddExpedAsync(ExpedModel exped)
        {
            using var conn = CreateConnection();
            await conn.OpenAsync();
            await using var transaction = await conn.BeginTransactionAsync();

            try
            {
                var saved = exped.CodExped.GetValueOrDefault() <= 0
                    ? await InsertExpedAsync(conn, transaction, exped)
                    : await UpdateExpedAsync(conn, transaction, exped);

                await transaction.CommitAsync();
                return saved;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task DeleteExpedAsync(ExpedModel exped)
        {
            await DeleteExpedsAsync([exped]);
        }

        public async Task DeleteExpedsAsync(IEnumerable<ExpedModel> expeds)
        {
            var ids = expeds
                .Where(e => e.CodExped.HasValue && e.CodExped.Value > 0)
                .Select(e => e.CodExped!.Value)
                .ToArray();

            if (ids.Length == 0)
                return;

            using var conn = CreateConnection();
            await conn.OpenAsync();
            await using var transaction = await conn.BeginTransactionAsync();

            try
            {
                await conn.ExecuteAsync(
                    "DELETE FROM expedicao.t_exped WHERE codexped = ANY(@Ids);",
                    new { Ids = ids },
                    transaction);

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private NpgsqlConnection CreateConnection()
        {
            if (string.IsNullOrWhiteSpace(BaseSettings.ConnectionString))
                BaseSettings.RefreshConnectionString();

            return new NpgsqlConnection(BaseSettings.ConnectionString);
        }

        private static async Task<ExpedModel> InsertExpedAsync(
            NpgsqlConnection conn,
            NpgsqlTransaction transaction,
            ExpedModel exped)
        {
            return await conn.QuerySingleAsync<ExpedModel>(
                $@"
                    INSERT INTO expedicao.t_exped
                    (
                        qtd_expedida,
                        vol_exp,
                        vol_tot_exp,
                        pl,
                        pb,
                        largura,
                        altura,
                        profundidade,
                        codvol,
                        cadastrado_por,
                        quando,
                        baia_virtual,
                        modelo_de_cx,
                        coddetalhescompl,
                        alterado_por,
                        alterado_quando,
                        inserido_por,
                        inserido_em,
                        operacao,
                        volume,
                        nf_emitida
                    )
                    VALUES
                    (
                        @QtdExpedida,
                        @VolExp,
                        @VolTotExp,
                        @Pl,
                        @Pb,
                        @Largura,
                        @Altura,
                        @Profundidade,
                        @CodVol,
                        @CadastradoPor,
                        @Quando,
                        @BaiaVirtual,
                        @ModeloCaixa,
                        @CodDetalhesCompl,
                        @AlteradoPor,
                        @AlteradoQuando,
                        @InseridoPor,
                        @InseridoEm,
                        @Operacao,
                        @Volume,
                        @nf_emitida
                    )
                    RETURNING {ExpedSelectColumns};",
                exped,
                transaction);
        }

        private static async Task<ExpedModel> UpdateExpedAsync(
            NpgsqlConnection conn,
            NpgsqlTransaction transaction,
            ExpedModel exped)
        {
            var saved = await conn.QuerySingleOrDefaultAsync<ExpedModel>(
                $@"
                    UPDATE expedicao.t_exped
                    SET
                        qtd_expedida = @QtdExpedida,
                        vol_exp = @VolExp,
                        vol_tot_exp = @VolTotExp,
                        pl = @Pl,
                        pb = @Pb,
                        largura = @Largura,
                        altura = @Altura,
                        profundidade = @Profundidade,
                        codvol = @CodVol,
                        cadastrado_por = @CadastradoPor,
                        quando = @Quando,
                        baia_virtual = @BaiaVirtual,
                        modelo_de_cx = @ModeloCaixa,
                        coddetalhescompl = @CodDetalhesCompl,
                        alterado_por = @AlteradoPor,
                        alterado_quando = @AlteradoQuando,
                        inserido_por = @InseridoPor,
                        inserido_em = @InseridoEm,
                        operacao = @Operacao,
                        volume = @Volume,
                        nf_emitida = @nf_emitida
                    WHERE codexped = @CodExped
                    RETURNING {ExpedSelectColumns};",
                exped,
                transaction);

            return saved ?? throw new InvalidOperationException("Registro de expedição não encontrado para atualização.");
        }

        private const string ExpedSelectColumns = @"
            codexped AS ""CodExped"",
            qtd_expedida AS ""QtdExpedida"",
            vol_exp AS ""VolExp"",
            vol_tot_exp AS ""VolTotExp"",
            pl AS ""Pl"",
            pb AS ""Pb"",
            largura AS ""Largura"",
            altura AS ""Altura"",
            profundidade AS ""Profundidade"",
            codvol AS ""CodVol"",
            cadastrado_por AS ""CadastradoPor"",
            quando AS ""Quando"",
            baia_virtual AS ""BaiaVirtual"",
            modelo_de_cx AS ""ModeloCaixa"",
            coddetalhescompl AS ""CodDetalhesCompl"",
            alterado_por AS ""AlteradoPor"",
            alterado_quando AS ""AlteradoQuando"",
            inserido_por AS ""InseridoPor"",
            inserido_em AS ""InseridoEm"",
            operacao AS ""Operacao"",
            volume AS ""Volume"",
            nf_emitida";
    }

}
