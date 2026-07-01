using CsvHelper;
using Expedicao.Views;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Telerik.Windows.Controls;

namespace Expedicao
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private DataBase dB = DataBase.Instance;
        DataBase BaseSettings = DataBase.Instance;

        
        public MainWindow()
        {
            InitializeComponent();

            txtUsername.Text = dB.Username;
            txtDataBase.Text = dB.Database;
        }

        private async void OnAtualizarSistemaClick(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            await ((App)Application.Current).CheckForUpdatesAsync(true);
        }

        private void OnSobreSistemaClick(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            var version = ((App)Application.Current).CurrentVersion;
            MessageBox.Show($"Sistema Integrado de Gerenciamento - Expedição\n\nVersão atual: {version}", "Sobre o sistema", MessageBoxButton.OK, MessageBoxImage.Information);
        }
		
        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            await Task.CompletedTask;
        }

        public void adicionarFilho(object filho, string title, string name)
        {
            var paneGroup = documentGroup ?? radDocking.FindChildByType<RadPaneGroup>();
            if (paneGroup == null)
            {
                radDocking.ApplyTemplate();
                radDocking.UpdateLayout();
                paneGroup = radDocking.FindChildByType<RadPaneGroup>();
            }

            if (paneGroup == null)
            {
                MessageBox.Show(
                    "Não foi possível localizar o container de abas do sistema.",
                    "Erro ao abrir tela",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            var pane = FindOpenPane(paneGroup, name, title);
            if (pane != null)
            {
                paneGroup.SelectedItem = pane;
                pane.IsActive = true;
                return;
            }

            var doc = filho as UserControl ?? filho as FrameworkElement;
            if (doc == null)
            {
                return;
            }

            doc.Name = name.ToLower();
            pane = new RadPane
            {
                Header = title,
                Content = doc,
                Tag = name.ToUpperInvariant(),
                CanUserClose = true,
                CanFloat = false
            };

            paneGroup.Items.Add(pane);
            paneGroup.SelectedItem = pane;
            pane.IsActive = true;
        }

        private static RadPane? FindOpenPane(RadPaneGroup paneGroup, string name_, string title)
        {
            var normalizedName = name_.ToUpperInvariant();
            return paneGroup.Items.OfType<RadPane>()
                .FirstOrDefault(p =>
                    string.Equals(p.Tag as string, normalizedName, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(p.Header?.ToString(), title, StringComparison.OrdinalIgnoreCase));
        }


        private void expedProduto_Click(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            adicionarFilho(new ViewExpedicaoProduto(), "EXPEDIÇÃO PRODUTO SHOPPING", "EXPEDICAO_PRODUTO_SHOPPING");
        }

        private void expedImprimirEtiqueta_Click(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            adicionarFilho(new ViewExpedicaoImpressaoEtiqueta(), "EXPEDIÇÃO IMPRESSÃO DE ETIQUETA", "EXPEDICAO_IMPRESSAO_ETIQUETA");
        }

        private void liberarImpressao_Click(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            adicionarFilho(new ViewExpedicaoLiberarImpressao(), "EXPEDIÇÃO LIBERAR IMPRESSÃO DE ETIQUETA", "EXPEDICAO_LIBERAR_IMPRESSAO_ETIQUETA");
        }

        private void expedNovoRomaneio_Click(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            adicionarFilho(new ViewExpedicaoRomaneio(), "EXPEDIÇÃO ROMANEIO", "EXPEDICAO_ROMANEIO");
        }

        private void expedTodosRomaneios_Click(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            adicionarFilho(new ViewExpedicaoRomaneios("PRINCIPAL"), "EXPEDIÇÃO ROMANEIOS", "EXPEDICAO_ROMANEIOS");
        }

        private void expedColetarDados_Click(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            adicionarFilho(new ViewExpedicaoColetaDados(), "EXPEDIÇÃO COLETA DE DADOS", "EXPEDICAO_COLETA_DADOS");
        }

        private void ItensFaltantes_Click(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            adicionarFilho(new ViewExpedicaoExcel("ITENS_FALTANTES"), "EXPEDIÇÃO ITENS FALTANTES", "EXPEDICAO_ITENS_FALTANTES");
        }

        private void ItensCarregados_Click(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            adicionarFilho(new ViewExpedicaoExcel("ITENS_CARREGADOS"), "EXPEDIÇÃO ITENS CARREGADOS", "EXPEDICAO_ITENS_CARREGADOS");
        }

        private async void OnExpedClick(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            await ExportarConsultaAsync(async db => await db.QryExpeds.ToListAsync(), "exped.xlsx");
        }
        private async void OnCaixasEnderecadasClick(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            await ExportarConsultaAsync(async db => await db.CaixasEnderecadas.ToListAsync(), "caixas_enderecadas.xlsx");
        }
        private async void OnSaldoGeralShoppingClick(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            await ExportarConsultaAsync(async db => await db.SaldoGeralShoppings.OrderBy(s => s.sigla).ToListAsync(), "saldo_geral_shopping.xlsx", worksheet =>
            {
                var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;
                if (lastRow <= 1)
                {
                    return;
                }

                var range = worksheet.Range($"A2:K{lastRow}");
                range.AddConditionalFormat().WhenIsTrue("$I2 >= 1").Fill.SetBackgroundColor(XLColor.FromArgb(83, 255, 161));
                range.AddConditionalFormat().WhenIsTrue("$I2 > 0.5").Fill.SetBackgroundColor(XLColor.FromArgb(29, 158, 255));
                range.AddConditionalFormat().WhenIsTrue("$I2 < 0.5").Fill.SetBackgroundColor(XLColor.White);
            });
        }
        private async void OnProdutosExpedidoDataClick(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            await ExportarConsultaAsync(async db => await db.produtosBaiadosData.ToListAsync(), "produtos_expedidos_data.xlsx");
        }
        private async void OnCubagemDiaClick(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            await ExportarConsultaAsync(async db => await db.CubagemDias.ToListAsync(), "cubagem_dia.xlsx");
        }
        private async void OnCubagemSemanaAnosClick(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            await ExportarConsultaAsync(async db => await db.CubagemSemanaAnos.ToListAsync(), "cubagem_ano_atual_ano_anterior.xlsx");
        }
        private async void OnCubagemPrevistaClienteClick(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            await ExportarConsultaAsync(async db => await db.CubagemPrevistaClientes.ToListAsync(), "cubagem_prevista_cliente.xlsx");
        }
        private async void OnCubagemFracionadaCaminao(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            await ExportarConsultaAsync(async db => await db.CubagemEnderecadas.ToListAsync(), "cubagem_fracionada_caminhao.xlsx");
        }
        private async void OnCubagemEfetivaClick(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            await ExportarConsultaAsync(async db => await db.CubagemEnderecadas.ToListAsync(), "cubagem_efetiva.xlsx");
        }
        private async void OnPendenciaExpedicaoClick(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            await ExportarConsultaAsync(async db => await db.PendenciaExpedicaos.ToListAsync(), "pendencias_expedicao.xlsx");
        }
        private async void OnExpedicaoVirtualClick(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            await ExportarConsultaAsync(async db => await db.ControleVirtuals.ToListAsync(), "expedicao_virtual.xlsx");
        }
        private async Task ExportarConsultaAsync<T>(Func<AppDatabase, Task<List<T>>> carregarDados, string nomeArquivo, Action<IXLWorksheet>? configurar = null)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                using AppDatabase db = new();
                var dados = await carregarDados(db);
                var caminho = Path.Combine(BaseSettings.CaminhoSistema, "Impressos", nomeArquivo);

                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Dados");
                if (dados.Count > 0)
                {
                    worksheet.Cell(1, 1).InsertTable(dados);
                }
                else
                {
                    worksheet.Cell(1, 1).Value = "Sem dados";
                }

                worksheet.Columns().AdjustToContents();
                configurar?.Invoke(worksheet);
                Directory.CreateDirectory(Path.GetDirectoryName(caminho)!);
                workbook.SaveAs(caminho);

                Process.Start(new ProcessStartInfo(caminho)
                {
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
        }
        private void OnCubagemZeradaClienteClick(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            adicionarFilho(new ViewCubagemClienteZerada(), "CUBAGEM ZERADA CLIENTE", "CUBAGEM_ZERADA_CLIENTE");
        }

        private void OnPreItensFaltantesClick(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            adicionarFilho(new ViewExpedicaoExcel("PRE_ITENS_FALTANTES"), "EXPEDIÇÃO PRÉ-CONFERENCIA ITENS FALTANTES", "EXPEDICAO_PRE_CONFERENCIA_ITENS_FALTANTES");
        }

        private void OnPreItensCarregadosClick(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            adicionarFilho(new ViewExpedicaoExcel("PRE_ITENS_CONFERIDOS"), "EXPEDIÇÃO PRÉ-CONFERENCIA ITENS CONFERIDOS", "EXPEDICAO_PRE_CONFERENCIA_ITENS_CONFERIDOS");
        }

        private void expedNotaCaminhao(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            adicionarFilho(new ViewSolicitarNotaCaminhao(), "SOLICITA NOTA FISCAL POR CAMINHÃO", "SOLICITA_NOTA_FISCAL_CAMINHAO");
        }

        private void expedNotaCliente_Click(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            adicionarFilho(new ViewSolicitarNotaCliente(), "SOLICITA NOTA FISCAL POR CLIENTE", "SOLICITA_NOTA_FISCAL_CLIENTE");
        }

        private void OnAlterarUsuario(object sender, MouseButtonEventArgs e)
        {
            Login window = new();
            window.ShowDialog();

            try
            {
                txtUsername.Text = dB.Username;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RadWindow.Prompt(new DialogParameters()
            {
                Header = "Ano Sistema",
                Content = "Alterar o Ano do Sistema",
                Closed = (object sender, WindowClosedEventArgs e) =>
                {
                    if (e.PromptResult != null)
                    {
                        dB.Database = e.PromptResult;
                        dB.RefreshConnectionString();
                        txtDataBase.Text = dB.Database;
                        documentGroup.Items.Clear();
                    }
                }
            });
        }

        private async void OnProdutoCSVClick(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            IList listAsync;
            try
            {
                using AppDatabase db = new();
                listAsync = await db.CarregamentoItemCaminhaos
                    .GroupBy(x => new
                    {
                        x.CodComplAdicional,
                        x.DescricaoFiscal,
                        x.Qtd,
                        x.Custo,
                        x.Peso,
                        x.Unidade,
                        x.Ncm
                    })
                    .OrderBy(x => x.Key.DescricaoFiscal)
                    .Select(x => new
                    {
                        IDENTIFICACAO = x.Key.CodComplAdicional,
                        DESCRICAO = x.Key.DescricaoFiscal,
                        NCM = x.Key.Ncm,
                        CODBARRA = "",
                        UNIDADEDECOMPRA = x.Key.Unidade,
                        UNIDADEVENDA = x.Key.Unidade,
                        SITUACAOTRIBUTARIAA = "0",
                        SITUACAOTRIBUTARIAB = "41",
                        CSOSN = "",
                        SITTRIBPIS = "PIS 70 - Operação de Aquisição sem Direito a Crédito",
                        SITTRIBCOFINS = "COFINS 70 - Operação de Aquisição sem Direito a Crédito",
                        SITTRIBIPI = "IPI 99 - Outras saídas",
                        IPI = "0",
                        ICMS = "0",
                        REDUCAOICMS = "0",
                        ALIQCOFINS = "0",
                        ALIQPIS = "",
                        CATEGORIA = "",
                        CEST = "",
                        CFOP = "",
                        CODIGODEBENEFICIOFISCAL = "0",
                        COMISSAODEVENDA = "",
                        CUSTO = "",
                        ESTOQUECOMPRA = "",
                        ESTOQUEMAXIMO = "",
                        ESTOQUEMINIMO = "",
                        FATORUNIDDEVENDA = "1",
                        ATIVO = "Sim",
                        INDICADORDEESCALARELEVANTE = "",
                        CNPJFABRICANTE = "",
                        PESO = "",
                        MATERIAPRIMA = "FALSO",
                        PARAVENDA = "VERDADEIRO",
                        MOEDA = "R$",
                        OBSERVACOES = "",
                        PRECODEVENDA1 = "",
                        PRECODEVENDA2 = "",
                        TIPODOPRODUTO = "3",
                        PRODUTOTERCEIRO = "FALSO",
                        CODTRIBUTACAONOSISTEMA = "7",
                        CODENQUADRAMENTOIPI = "999",
                        OPERACAOFATORCONVERSAO = ""
                    })
                    .ToListAsync();

                var produtos = await db.CarregamentoItemCaminhaos.OrderBy(x => x.DescricaoFiscal).ToListAsync();

                var nomePasta = "NF";
                var nomeArquivo = "Produtos.csv";
                var caminhoArquivo = @"C:\Temp\" + nomePasta;

                if (!Directory.Exists(caminhoArquivo))
                    Directory.CreateDirectory(caminhoArquivo);

                using var streamWriter = new StreamWriter(@"C:\Temp\Produtos.csv");
                using var csvWriter = new CsvWriter(streamWriter, new CultureInfo("pt-BR", true));                                           
                csvWriter.WriteRecords(listAsync);
                streamWriter.Flush();

                StreamWriter sw = new(@"C:\Temp\PRODUTOS.FSI");
                foreach (var produto in produtos)
                {
                    await sw.WriteLineAsync
                        (
                            /*01*/Convert.ToString("F030").ToString().PadRight(4) +
                            /*02*/Convert.ToString(produto.CodComplAdicional).ToString().PadRight(30) +
                            /*03*/Convert.ToString(produto.DescricaoFiscal).ToString().PadRight(60) +
                            /*04*/Convert.ToString("0").ToString().PadLeft(10, '0') +
                            /*05*/Convert.ToString("0").ToString().PadLeft(10, '0') +
                            /*06*/Convert.ToString(produto.Unidade).ToString().PadRight(3) +
                            /*07*/string.Format("{0:000000000000.00000}", 0).Replace(",", null).Replace(".", null) +
                            /*08*/Convert.ToString("1").ToString().PadRight(1) +
                            /*09*/string.Format("{0:000000000000.0000}", 0).Replace(",", null).Replace(".", null) +
                            /*10*/Convert.ToString("0").ToString().PadRight(3) +
                            /*11*/string.Format("{0:0000000.00}", 0).Replace(",", null).Replace(".", null) +
                            /*12*/Convert.ToString("S").ToString().PadRight(1) +
                            /*13*/Convert.ToString("N").ToString().PadRight(1) +
                            /*14*/string.Format("{0:0000000.00}", 0).Replace(",", null).Replace(".", null) +
                            /*15*/string.Format("{0:0000000.00}", 0).Replace(",", null).Replace(".", null) +
                            /*16*/string.Format("{0:0000000.00}", 0).Replace(",", null).Replace(".", null) +
                            /*17*/Convert.ToString("0").ToString().PadLeft(7, '0') +
                            /*18*/Convert.ToString(DateTime.Now.ToString("ddMMyyyy")).ToString().PadRight(8) +
                            /*19*/Convert.ToString("").ToString().PadRight(5) +
                            /*20*/Convert.ToString("").ToString().PadRight(2) +
                            /*21*/Convert.ToString("0").ToString().PadLeft(1, '0') + 
                            /*22*/Convert.ToString("0").ToString().PadLeft(2, '0') +
                            /*23*/string.Format("{0:000000000000.00}", 0).Replace(",", null).Replace(".", null) +
                            /*24*/string.Format("{0:000000000000.00}", 0).Replace(",", null).Replace(".", null) +
                            /*25*/Convert.ToString("N").ToString().PadRight(1) +
                            /*26*/string.Format("{0:000000000000.00}", 0).Replace(",", null).Replace(".", null) +
                            /*27*/string.Format("{0:000000000000.00}", 0).Replace(",", null).Replace(".", null) +
                            /*28*/string.Format("{0:000000000000.00}", 0).Replace(",", null).Replace(".", null) +
                            /*29*/Convert.ToString("").ToString().PadRight(14) +
                            /*30*/string.Format("{0:000000000000.00}", 0).Replace(",", null).Replace(".", null) +
                            /*31*/string.Format("{0:000000000000.0000}", 0).Replace(",", null).Replace(".", null) +
                            /*32*/string.Format("{0:000000000000.0000}", 0).Replace(",", null).Replace(".", null) +
                            /*33*/string.Format("{0:000000000000.0000}", 0).Replace(",", null).Replace(".", null) +
                            /*34*/string.Format("{0:000000000000.0000}", 0).Replace(",", null).Replace(".", null) +
                            /*35*/string.Format("{0:000000000000.00000}", 0).Replace(",", null).Replace(".", null) +
                            /*36*/string.Format("{0:000000000000.00000}", 0).Replace(",", null).Replace(".", null) +
                            /*37*/Convert.ToString("0").ToString().PadLeft(8) +
                            /*38*/Convert.ToString("").ToString().PadRight(50) +
                            /*39*/Convert.ToString("").ToString().PadRight(50) +
                            /*40*/Convert.ToString("").ToString().PadRight(50) +
                            /*41*/Convert.ToString("").ToString().PadRight(50) +
                            /*42*/Convert.ToString("").ToString().PadRight(50) +
                            /*43*/Convert.ToString("").ToString().PadRight(50) +
                            /*44*/Convert.ToString("").ToString().PadRight(50) +
                            /*45*/Convert.ToString("").ToString().PadRight(50) +
                            /*46*/Convert.ToString("").ToString().PadRight(50) +
                            /*47*/Convert.ToString("").ToString().PadRight(50) +
                            /*48*/Convert.ToString("").ToString().PadRight(50) +
                            /*49*/Convert.ToString("").ToString().PadRight(50) +
                            /*50*/Convert.ToString("").ToString().PadRight(50) +
                            /*51*/Convert.ToString("").ToString().PadRight(50) +
                            /*52*/Convert.ToString("").ToString().PadRight(50) +
                            /*53*/Convert.ToString("").ToString().PadRight(140) +
                            /*54*/Convert.ToString(DateTime.Now.ToString("ddMMyyyy")).ToString().PadRight(8) +
                            /*55*/Convert.ToString(DateTime.Now.ToString("ddMMyyyy")).ToString().PadRight(8) +
                            /*56*/Convert.ToString("").ToString().PadRight(18) +
                            /*57*/Convert.ToString("").ToString().PadRight(40) +
                            /*58*/Convert.ToString("").ToString().PadRight(3) +
                            /*59*/Convert.ToString(DateTime.Now.ToString("ddMMyyyy")).ToString().PadRight(8) +
                            /*60*/Convert.ToString("").ToString().PadRight(30) +
                            /*61*/Convert.ToString("").ToString().PadRight(30) + 
                            /*62*/Convert.ToString("0").ToString().PadLeft(2, '0') +
                            /*63*/Convert.ToString("0").ToString().PadLeft(2, '0') +
                            /*64*/Convert.ToString("0").ToString().PadLeft(2, '0') +
                            /*65*/Convert.ToString("0").ToString().PadLeft(3, '0') +
                            /*66*/Convert.ToString("").ToString().PadRight(14) +
                            /*67*/Convert.ToString("").ToString().PadRight(2) +
                            /*68*/Convert.ToString("").ToString().PadRight(30) +
                            /*69*/Convert.ToString("").ToString().PadRight(2) +
                            /*70*/Convert.ToString("").ToString().PadRight(30) +
                            /*71*/Convert.ToString("0").ToString().PadLeft(4, '0') +
                            /*72*/Convert.ToString("").ToString().PadRight(30) +
                            /*73*/Convert.ToString("").ToString().PadRight(5) +
                            /*74*/Convert.ToString("").ToString().PadRight(30) +
                            /*75*/Convert.ToString("").ToString().PadRight(50) +
                            /*76*/Convert.ToString("").ToString().PadRight(50) +
                            /*77*/Convert.ToString("").ToString().PadRight(50) +
                            /*78*/Convert.ToString("").ToString().PadRight(50) +
                            /*79*/Convert.ToString("").ToString().PadRight(50) +
                            /*80*/Convert.ToString("").ToString().PadRight(50) +
                            /*81*/Convert.ToString("").ToString().PadRight(50) +
                            /*82*/Convert.ToString("").ToString().PadRight(50) +
                            /*83*/Convert.ToString("").ToString().PadRight(50) +
                            /*84*/Convert.ToString("").ToString().PadRight(50) +
                            /*85*/Convert.ToString("").ToString().PadRight(50) +
                            /*86*/Convert.ToString("").ToString().PadRight(50) +
                            /*87*/Convert.ToString("").ToString().PadRight(50) +
                            /*88*/Convert.ToString("").ToString().PadRight(50) +
                            /*89*/Convert.ToString("").ToString().PadRight(50) +
                            /*90*/string.Format("{0:000000000000.00}", 0).Replace(",", null).Replace(".", null) +
                            /*91*/string.Format("{0:000000000000.00}", 0).Replace(",", null).Replace(".", null) +
                            /*92*/Convert.ToString("99").ToString().PadRight(2) +
                            /*93*/Convert.ToString("A").ToString().PadRight(1) 
                        );
                }

                MessageBox.Show(@"ARQUIVOS CRIADOS NO DIRETÓRIO 'C:\TEMP'");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            
        }
    }
}



