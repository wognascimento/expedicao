using CsvHelper;
using Expedicao.Model;
using ClosedXML.Excel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.IO.Ports;
using System.Linq;
using System.Media;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Expedicao.Views
{
    /// <summary>
    /// Interação lógica para ViewExpedicaoColetaDados.xam
    /// </summary>
    public partial class ViewExpedicaoColetaDados : UserControl
    {
        SerialPort port = new();
        int index;
        DateTime DataCarregamento = new DateTime();
        DataBase BaseSettings = DataBase.Instance;
        private readonly List<RomaneioModel> _romaneios = [];
        private readonly List<CarregamentoItenFaltanteModel> _itensFaltantes = [];
        private readonly HashSet<string> _barcodesCarregados = new(StringComparer.OrdinalIgnoreCase);

        public ViewExpedicaoColetaDados()
        {
            InitializeComponent();

            
            port.PortName = "COM1";
            port.BaudRate = 9600;
            port.Handshake = 0;
            port.Parity = 0;
            port.DataBits = 8;
            port.StopBits = (StopBits)1;
            port.DtrEnable = true;
            port.RtsEnable = true;
            port.ReadTimeout = 200;
            port.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
            
        }

        private void AtualizarResumo()
        {
            var siglas = ObterSiglasSelecionadas();
            txtSigla.Content = string.Join("; ", siglas);
            txtPlaca.Content = ObterPlacaSelecionada();
            txtConferente.Content = ObterConferenteSelecionado();
            txtVolumes.Content = ObterItensPendentes().Count;
        }

        private List<string> ObterSiglasSelecionadas()
        {
            return _romaneios
                .Select(r => r.shopping_destino)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct()
                .ToList()!;
        }

        private string ObterPlacaSelecionada()
        {
            return _romaneios
                .Select(r => r.placa_carroceria)
                .FirstOrDefault(p => !string.IsNullOrWhiteSpace(p)) ?? string.Empty;
        }

        private string ObterConferenteSelecionado()
        {
            return _romaneios
                .Select(r => r.nome_conferente)
                .FirstOrDefault(c => !string.IsNullOrWhiteSpace(c)) ?? string.Empty;
        }

        private List<CarregamentoItenFaltanteModel> ObterItensPendentes()
        {
            return _itensFaltantes
                .Where(i => string.IsNullOrWhiteSpace(i.Barcode) || !_barcodesCarregados.Contains(i.Barcode))
                .ToList();
        }

        private void AtualizarGridItens()
        {
            var pendentes = ObterItensPendentes();
            itens.ItemsSource = pendentes;
            txtVolumes.Content = pendentes.Count;
        }

        private void LimparCarregamento()
        {
            _romaneios.Clear();
            _itensFaltantes.Clear();
            _barcodesCarregados.Clear();
            DataCarregamento = new DateTime();
            itens.ItemsSource = null;
            AtualizarResumo();
        }

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            string recieved_data = port.ReadExisting();
            if (_itensFaltantes.Any(i => string.Equals(i.Barcode, recieved_data, StringComparison.OrdinalIgnoreCase)))
            {
                _barcodesCarregados.Add(recieved_data);
                Dispatcher.Invoke((Action)AtualizarGridItens);
                new SoundPlayer(@"sound\success.wav").Play();
            }
            else
            {
                new SoundPlayer(@"sound\error.wav").Play();
                MessageBox.Show("Volume não presente no lookup.");
            }
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                AtualizarGridItens();
                AtualizarResumo();
                loading.Visibility = Visibility.Hidden;
                //port.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void RibbonButton_Click(object sender, RoutedEventArgs e)
        {
            if (_barcodesCarregados.Count == 0)
            {
                LimparCarregamento();
                Window window = new();
                window.Title = "EXPEDIÇÃO ROMANEIOS ";
                window.Content = new ViewExpedicaoRomaneios("CARREGAMENTO");
                window.Height = 600.0;
                window.Width = 680.0;
                window.ResizeMode = ResizeMode.NoResize;
                window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                window.ShowDialog();
                _romaneios.AddRange(((ViewExpedicaoRomaneios)window.Content).Romaneios);
                this.DataCarregamento = _romaneios
                    .Where(r => r.data_carregamento.HasValue)
                    .OrderBy(r => r.data_carregamento)
                    .Select(r => r.data_carregamento!.Value)
                    .FirstOrDefault();
                AtualizarResumo();
            }
            else
            {
                MessageBox.Show("É necessário encerrar o carregamento para selecionar outro(s) cliente(s)");
            }
        }

        private async void RibbonButton_Click_1(object sender, RoutedEventArgs e)
        {
            var siglas = ObterSiglasSelecionadas();
            if (siglas.Count == 0)
            {
                MessageBox.Show("Selecione um romaneio antes de buscar os itens.", "Lookup Itens", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Window window = new();
            window.Title = "EXPEDIÇÃO CAMINHÕES ";
            window.Content = new ViewExpedicaoLookupCaminao(siglas);
            window.Height = 300.0;
            window.Width = 450.0;
            window.ResizeMode = ResizeMode.NoResize;
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            window.ShowDialog();
            try
            {
                loading.Visibility = Visibility.Visible;
                _itensFaltantes.Clear();
                _itensFaltantes.AddRange(((ViewExpedicaoLookupCaminao)window.Content).ItensFaltantes);
                _barcodesCarregados.Clear();
                AtualizarGridItens();
                loading.Visibility = Visibility.Hidden;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void RibbonButton_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                loading.Visibility = Visibility.Visible;
                var conferente = ObterConferenteSelecionado();
                var placa = ObterPlacaSelecionada();

                foreach (var barcode in _barcodesCarregados)
                {
                    ConfCargaGeralModel conf = new ConfCargaGeralModel()
                    {
                        Barcode = barcode,
                        DocaOrigem = "JACAREÍ",
                        Data = DataCarregamento,
                        Resp = conferente,
                        Caminhao = placa
                    };
                    await Task.Run(async () => await new ExpedicaoViewModel().GetAddVolumeCarregado(conf));
                }
                this.loading.Visibility = Visibility.Hidden;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void RibbonButton_Click_3(object sender, RoutedEventArgs e)
        {
            try
            {
                loading.Visibility = Visibility.Visible;

                List<string> siglas = ObterSiglasSelecionadas();
                foreach (var sigla in siglas)
                {
                    List<string> arr = new List<string>() { sigla };
                    var itens = await Task.Run(async () => await new ExpedicaoViewModel().GetCarregamentoItemCaminhaosAsync(arr, Dispatcher.Invoke(() => txtPlaca.Content.ToString())));
                    if (itens.Count == 0)
                    {
                        MessageBox.Show($"Não há itens carregados para o romaneio selecionado, por este motivo o e-mail não será enviado.", "Itens carregados", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }


                IList dados = await Task.Run(async () => await new ExpedicaoViewModel().GetCarregamentoItemCaminhaosSemParemetroAsync(siglas));
                if (dados.Count > 0)
                {
                    MessageBox.Show(
                        "Existe produto(s) sem Preço ou Pesso, por esse motivo não será possivel enviar e-mail para emissão, envia a planilha para cadastro_produto@cipolatti.com.br e tente novamente após todos os produtos parametrizado(s)",
                        "Proto sem paremetro",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    await Task.Run(() => GetProdutosSemParemetrokAsync(dados));
                    loading.Visibility = Visibility.Hidden;
                }
                else
                {

                    await Task.Run(() => GetInformacoesNF());
                    var NExportado =  await Task.Run(GetProdutosNaoExportadosMaticAsync);
                    long codigo = await Task.Run(() => new ExpedicaoViewModel().OrcamentoSequenceAsync(new OrcamentoSequenceModel { Cliente = "CARREGAMNETO" }));
                    await Task.Run(() => CriarOrcamento1TaskAsync(codigo));
                    await Task.Run(() => CriarOrcamento2TaskAsync(codigo));
                    //await Task.Run(CriarOrcamentokAsync);

                    await Task.Run(() => SendMailAsync(NExportado));


                    LimparCarregamento();
   

                    loading.Visibility = Visibility.Hidden;
                    MessageBox.Show("Email enviado para o fiscal");
                }
            }
            catch (Exception ex)
            {
                loading.Visibility = Visibility.Hidden;
                MessageBox.Show(ex.Message);
            }
        }

        private async Task GetProdutosSemParemetrokAsync(IList reports)
        {
            var caminho = Path.Combine(BaseSettings.CaminhoSistema, "Impressos", "ProdutoSemParametro.xlsx");
            ExportarListaParaExcel(reports, caminho, "Produtos");
            Process.Start(new ProcessStartInfo(caminho)
            {
                UseShellExecute = true
            });
            await Task.Delay(2000);
        }


        private static string RemoverAcentos(string texto)
        {
            // Normaliza o texto para separar os acentos das letras
            string textoNormalizado = texto.Normalize(NormalizationForm.FormD);

            // Expressão regular para remover caracteres não-ASCII (acentos)
            Regex regexAcentos = new Regex(@"\p{IsCombiningDiacriticalMarks}+");

            // Remove os acentos
            return regexAcentos.Replace(textoNormalizado, "").Normalize(NormalizationForm.FormC);
        }


        private async Task CriarOrcamentokAsync()
        {
            try
            {
                var siglas = ObterSiglasSelecionadas();
                var itens = await Task.Run(() => new ExpedicaoViewModel().GetCarregamentoItemCaminhaosAsync(siglas, Dispatcher.Invoke(() => txtPlaca.Content.ToString())));
                int tamanhoDoPedaco = 30;
                int arquivo = 1;
                
                var pedacos = itens
                    .Select((value, index) => new { value, index })
                    .GroupBy(x => x.index / tamanhoDoPedaco)
                    .Select(group => group.Select(x => x.value).ToList())
                    .ToList();


                if (Directory.Exists(@"C:\Temp\NF"))
                    Directory.Delete(@"C:\Temp\NF", true);
                Directory.CreateDirectory(@"C:\Temp\NF");

                foreach (var pedaco in pedacos)
                {
                    long codigo = await Task.Run(async () => await new ExpedicaoViewModel().OrcamentoSequenceAsync(new OrcamentoSequenceModel { Cliente = "CARREGAMNETO" }));
                    Directory.CreateDirectory(@$"C:\Temp\NF\ORCAMENTO-{codigo}");
                    StreamWriter sw = new(@$"C:\Temp\NF\ORCAMENTO-{codigo}\ORCAMEN1.FSI");
                    await sw.WriteLineAsync(
                        /*01*/"F210" +
                    /*02*/Convert.ToString(codigo).ToString().PadLeft(6, '0') +
                    /*03*/DateTime.Now.ToString("ddMMyyyy") +
                    /*04*/Convert.ToString("").PadRight(6, '0') +
                    /*05*/Convert.ToString("").PadRight(30) +
                    /*06*/Convert.ToString("").PadRight(30) +
                    /*07*/Convert.ToString("").PadRight(10) +
                    /*08*/Convert.ToString("").PadRight(50) +
                    /*09*/Convert.ToString("").PadRight(30) +
                    /*10*/Convert.ToString("").PadRight(2) +
                    /*11*/Convert.ToString("").PadRight(9) +
                    /*12*/Convert.ToString("").PadRight(50) +
                    /*13*/Convert.ToString("").PadRight(30) +
                    /*14*/Convert.ToString("").PadRight(2) +
                    /*15*/Convert.ToString("").PadRight(9) +
                    /*16*/Convert.ToString("").PadRight(3, '0') +
                    /*17*/Convert.ToString("").PadRight(3, '0') +
                    /*18*/DateTime.Now.ToString("ddMMyyyy") +
                    /*19*/Convert.ToString("").PadRight(4, '0') +
                    /*20*/DateTime.Now.ToString("ddMMyyyy") +
                    /*21*/Convert.ToString("").PadRight(14, '0') +
                    /*22*/Convert.ToString("").PadRight(6, '0') +
                    /*23*/DateTime.Now.ToString("ddMMyyyy") +
                    /*24*/Convert.ToString("A").PadRight(2) +
                    /*25*/Convert.ToString("").PadRight(60) +
                    /*26*/Convert.ToString("").PadRight(60) +
                    /*27*/Convert.ToString("").PadRight(60) +
                    /*28*/Convert.ToString("").PadRight(60) +
                    /*29*/Convert.ToString("").PadRight(60) +
                    /*30*/Convert.ToString("").PadRight(60) +
                    /*31*/Convert.ToString("").PadRight(60) +
                    /*32*/Convert.ToString("").PadRight(60) +
                    /*33*/Convert.ToString("").PadRight(4) +
                    /*34*/Convert.ToString("").PadRight(2) +
                    /*35*/Convert.ToString("").PadRight(50) +
                    /*36*/Convert.ToString("").PadRight(30) +
                    /*37*/Convert.ToString("").PadRight(30) +
                    /*38*/Convert.ToString("A").PadRight(1));
                    sw.Close();


                    sw = new(@$"C:\Temp\NF\ORCAMENTO-{codigo}\ORCAMEN2.FSI");
                    int item = 1;
                    foreach (var p in pedaco)
                    {
                        await sw.WriteLineAsync(
                            /*01*/"F220" +
                        /*02*/Convert.ToString(codigo).ToString().PadLeft(6, '0') +
                        /*03*/Convert.ToString(item).PadRight(14) +
                        /*04*/Convert.ToString(p.CodComplAdicional).PadRight(30) +
                        /*05*/Convert.ToString(RemoverAcentos(p.DescricaoFiscal)).PadRight(60) +
                        /*06*/Convert.ToString("N").PadRight(1) +
                        /*07*/Convert.ToString("").PadRight(60) +
                        /*08*/Convert.ToString("").PadRight(60) +
                        /*09*/Convert.ToString("").PadRight(60) +
                        /*10*/Convert.ToString("").PadRight(60) +
                        /*11*/Convert.ToString("").PadRight(60) +
                        /*12*/Convert.ToString("").PadRight(60) +
                        /*13*/Convert.ToString("").PadRight(60) +
                        /*14*/Convert.ToString("").PadRight(60) +
                        /*15*/Convert.ToString("").PadRight(60) +
                        /*16*/string.Format("{0:000000000000.00}", p.Qtd).Replace(",", null).Replace(".", null) +
                        /*17*/Convert.ToString(p.Unidade).PadRight(3) +
                        /*18*/string.Format("{0:000000000000.00}", p.Custo).Replace(",", null).Replace(".", null) +
                        /*19*/string.Format("{0:000000000000.00}", p.Qtd * p.Custo).Replace(",", null).Replace(".", null) +
                        /*20*/string.Format("{0:000000000000.00}", 0).Replace(",", null).Replace(".", null) +
                        /*21*/string.Format("{0:000000000000.00}", 0).Replace(",", null).Replace(".", null) +
                        /*22*/string.Format("{0:000000000000.00}", 0).Replace(",", null).Replace(".", null) +
                        /*23*/string.Format("{0:000000000000.00}", 0).Replace(",", null).Replace(".", null) +
                        /*24*/string.Format("{0:000000000000.00}", 0).Replace(",", null).Replace(".", null) +
                        /*25*/string.Format("{0:000000000000.00}", 0).Replace(",", null).Replace(".", null) +
                        /*26*/string.Format("{0:000000000000.00}", 0).Replace(",", null).Replace(".", null) +
                        /*27*/string.Format("{0:000000000000.00}", 0).Replace(",", null).Replace(".", null) +
                        /*28*/Convert.ToString("").PadRight(60) +
                        /*29*/Convert.ToString("").PadRight(60) +
                        /*30*/Convert.ToString("").PadRight(60) +
                        /*31*/Convert.ToString("").PadRight(60) +
                        /*32*/Convert.ToString("").PadRight(60) +
                        /*33*/Convert.ToString("").PadRight(60) +
                        /*34*/string.Format("{0:000000000000.00}", 0).Replace(",", null).Replace(".", null) +
                        /*35*/Convert.ToString("A").PadRight(1));

                        item++;
                    }
                    sw.Close();



                    arquivo++;
                }
                File.Delete(@"C:\Temp\ORCAMENTO.zip");
                ZipFile.CreateFromDirectory(@"C:\Temp\NF", @"C:\Temp\ORCAMENTO.zip");
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task CriarOrcamento1TaskAsync(long codigo)
        {
            try
            {
                StreamWriter sw = new StreamWriter("ORCAMEN1.FSI");
                await sw.WriteLineAsync(
                    /*01*/"F210" +
                    /*02*/Convert.ToString(codigo).ToString().PadLeft(6, '0') +
                    /*03*/DateTime.Now.ToString("ddMMyyyy") +
                    /*04*/Convert.ToString("").PadRight(6, '0') +
                    /*05*/Convert.ToString("").PadRight(30) +
                    /*06*/Convert.ToString("").PadRight(30) +
                    /*07*/Convert.ToString("").PadRight(10) +
                    /*08*/Convert.ToString("").PadRight(50) +
                    /*09*/Convert.ToString("").PadRight(30) +
                    /*10*/Convert.ToString("").PadRight(2) +
                    /*11*/Convert.ToString("").PadRight(9) +
                    /*12*/Convert.ToString("").PadRight(50) +
                    /*13*/Convert.ToString("").PadRight(30) +
                    /*14*/Convert.ToString("").PadRight(2) +
                    /*15*/Convert.ToString("").PadRight(9) +
                    /*16*/Convert.ToString("").PadRight(3, '0') +
                    /*17*/Convert.ToString("").PadRight(3, '0') +
                    /*18*/DateTime.Now.ToString("ddMMyyyy") +
                    /*19*/Convert.ToString("").PadRight(4, '0') +
                    /*20*/DateTime.Now.ToString("ddMMyyyy") +
                    /*21*/Convert.ToString("").PadRight(14, '0') +
                    /*22*/Convert.ToString("").PadRight(6, '0') +
                    /*23*/DateTime.Now.ToString("ddMMyyyy") +
                    /*24*/Convert.ToString("A").PadRight(2) +
                    /*25*/Convert.ToString("").PadRight(60) +
                    /*26*/Convert.ToString("").PadRight(60) +
                    /*27*/Convert.ToString("").PadRight(60) +
                    /*28*/Convert.ToString("").PadRight(60) +
                    /*29*/Convert.ToString("").PadRight(60) +
                    /*30*/Convert.ToString("").PadRight(60) +
                    /*31*/Convert.ToString("").PadRight(60) +
                    /*32*/Convert.ToString("").PadRight(60) +
                    /*33*/Convert.ToString("").PadRight(4) +
                    /*34*/Convert.ToString("").PadRight(2) +
                    /*35*/Convert.ToString("").PadRight(50) +
                    /*36*/Convert.ToString("").PadRight(30) +
                    /*37*/Convert.ToString("").PadRight(30) +
                    /*38*/Convert.ToString("A").PadRight(1));
                sw.Close();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                Console.WriteLine("Executing finally block.");
            }
        }

        public async Task CriarOrcamento2TaskAsync(long codigo)
        {
            try
            {
                var siglas = ObterSiglasSelecionadas();
                var itens = await Task.Run(() => new ExpedicaoViewModel().GetCarregamentoItemCaminhaosAsync(siglas, Dispatcher.Invoke(() => txtPlaca.Content.ToString())));

                using StreamWriter sw = new("ORCAMEN2.FSI");
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("ITENS");
                worksheet.Cell(1, 1).Value = "ITEM";
                worksheet.Cell(1, 2).Value = "CODIGO PRODUTO";
                worksheet.Cell(1, 3).Value = "DESCRIÇÃO";
                worksheet.Cell(1, 4).Value = "QUANTIDADE";
                worksheet.Cell(1, 5).Value = "VALOR UNITÁRIO";
                worksheet.Cell(1, 6).Value = "UNIDADE";
                worksheet.Range("A1:F1").Style.Font.Bold = true;

                var item = 0;
                foreach (var registro in itens)
                {
                    item++;
                    await sw.WriteLineAsync(
                        "F220" +
                        Convert.ToString(codigo).PadLeft(6, '0') +
                        Convert.ToString(item).PadRight(14) +
                        Convert.ToString(registro.CodComplAdicional).PadRight(30) +
                        Convert.ToString(RemoverAcentos(registro.DescricaoFiscal)).PadRight(60) +
                        Convert.ToString("N").PadRight(1) +
                        Convert.ToString("").PadRight(60) +
                        Convert.ToString("").PadRight(60) +
                        Convert.ToString("").PadRight(60) +
                        Convert.ToString("").PadRight(60) +
                        Convert.ToString("").PadRight(60) +
                        Convert.ToString("").PadRight(60) +
                        Convert.ToString("").PadRight(60) +
                        Convert.ToString("").PadRight(60) +
                        Convert.ToString("").PadRight(60) +
                        string.Format("{0:000000000000.00}", registro.Qtd).Replace(",", null).Replace(".", null) +
                        Convert.ToString(registro.Unidade).PadRight(3) +
                        string.Format("{0:000000000000.00}", registro.Custo).Replace(",", null).Replace(".", null) +
                        string.Format("{0:000000000000.00}", 0).Replace(",", null).Replace(".", null) +
                        string.Format("{0:000000000000.00}", 0).Replace(",", null).Replace(".", null) +
                        string.Format("{0:000000000000.00}", 0).Replace(",", null).Replace(".", null) +
                        string.Format("{0:000000000000.00}", 0).Replace(",", null).Replace(".", null) +
                        string.Format("{0:000000000000.00}", 0).Replace(",", null).Replace(".", null) +
                        string.Format("{0:000000000000.00}", 0).Replace(",", null).Replace(".", null) +
                        string.Format("{0:000000000000.00}", 0).Replace(",", null).Replace(".", null) +
                        string.Format("{0:000000000000.00}", 0).Replace(",", null).Replace(".", null) +
                        string.Format("{0:000000000000.00}", 0).Replace(",", null).Replace(".", null) +
                        Convert.ToString("").PadRight(60) +
                        Convert.ToString("").PadRight(60) +
                        Convert.ToString("").PadRight(60) +
                        Convert.ToString("").PadRight(60) +
                        Convert.ToString("").PadRight(60) +
                        Convert.ToString("").PadRight(60) +
                        string.Format("{0:000000000000.00}", 0).Replace(",", null).Replace(".", null) +
                        Convert.ToString("A").PadRight(1));

                    var row = item + 1;
                    worksheet.Cell(row, 1).Value = item;
                    worksheet.Cell(row, 2).Value = Convert.ToDouble(registro.CodComplAdicional);
                    worksheet.Cell(row, 3).Value = registro.DescricaoFiscal;
                    worksheet.Cell(row, 4).Value = Convert.ToDouble(registro.Qtd);
                    worksheet.Cell(row, 5).Value = Convert.ToDouble(registro.Custo);
                    worksheet.Cell(row, 6).Value = registro.Unidade;
                }

                worksheet.Columns().AdjustToContents();
                workbook.SaveAs("ITENS.xlsx");

                var volumes = await Task.Run(() => new ExpedicaoViewModel().GetCarregamentoVolumesAsync(siglas, Dispatcher.Invoke(() => txtPlaca.Content.ToString())));
                foreach (var volume in volumes)
                {
                    await Task.Run(() => new ExpedicaoViewModel().GetVolumeCarregado(volume.codexped));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                Console.WriteLine("Executing finally block.");
            }
        }

        private async Task GetInformacoesNF()
        {
            var resumoNotaModelList = await Task.Run(async () => await new ExpedicaoViewModel().GetInformasoesNfAsync(ObterSiglasSelecionadas(), Dispatcher.Invoke(() => txtPlaca.Content.ToString())));
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Informacoes");
            var row = 1;

            foreach (var resumo in resumoNotaModelList)
            {
                worksheet.Range(row, 1, row, 7).Merge().Value = "INFORMAÇÕES COMPLEMENTARES NF SHOPPING";
                worksheet.Range(row, 1, row, 7).Style.Font.Bold = true;
                worksheet.Range(row, 1, row, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Range(row, 1, row, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                row++;

                AdicionarLinhaInformacaoNf(worksheet, ref row, "Shopping", resumo.Shopp);
                AdicionarLinhaInformacaoNf(worksheet, ref row, "Nome", resumo.Nome);
                AdicionarLinhaInformacaoNf(worksheet, ref row, "Caminhão", resumo.Caminhao);
                AdicionarLinhaInformacaoNf(worksheet, ref row, "Data", resumo.Data);
                AdicionarLinhaInformacaoNf(worksheet, ref row, "Transportadora", resumo.NomeTransportadora);
                AdicionarLinhaInformacaoNf(worksheet, ref row, "CNPJ", resumo.Cnpj);
                AdicionarLinhaInformacaoNf(worksheet, ref row, "IE", resumo.Ie);
                AdicionarLinhaInformacaoNf(worksheet, ref row, "Endereço", resumo.Endereco);
                AdicionarLinhaInformacaoNf(worksheet, ref row, "Bairro", resumo.Bairro);
                AdicionarLinhaInformacaoNf(worksheet, ref row, "Cidade", resumo.Cidade);
                AdicionarLinhaInformacaoNf(worksheet, ref row, "CEP", resumo.Cep);
                AdicionarLinhaInformacaoNf(worksheet, ref row, "UF", resumo.Uf);
                AdicionarLinhaInformacaoNf(worksheet, ref row, "Volumes", resumo.volumes);
                AdicionarLinhaInformacaoNf(worksheet, ref row, "Peso Liquido", resumo.Liquido);
                AdicionarLinhaInformacaoNf(worksheet, ref row, "Peso Bruto", resumo.Bruto);
                AdicionarLinhaInformacaoNf(worksheet, ref row, "Preço", resumo.Preco);
                row++;
            }

            worksheet.Columns().AdjustToContents();
            workbook.SaveAs("Informacoes_Complementares.xlsx");
        }

        private async Task<int> GetProdutosNaoExportadosMaticAsync()
        {

            var dados = await Task.Run(async () => await new ExpedicaoViewModel().GetCarregamentoItemCaminhaosNaoExportadoMaticAsync(ObterSiglasSelecionadas()));

            var nomePasta = "NF";
            var nomeArquivo = "Produtos.csv";
            var caminhoArquivo = @"C:\Temp\" + nomePasta;

            if (!Directory.Exists(caminhoArquivo))
                Directory.CreateDirectory(caminhoArquivo);

            //using (var streamWriter = new StreamWriter(Path.Combine(caminhoArquivo, nomeArquivo)))
            using (var streamWriter = new StreamWriter("Produtos.csv"))
            using (var csvWriter = new CsvWriter(streamWriter, new CultureInfo("pt-BR", true)))
            {
                //csvWriter.Context.RegisterClassMap<DadosAnexoMap>();                                               
                csvWriter.WriteRecords(dados);
                streamWriter.Flush();
            }

            return dados.Count;
        }

        private async Task SendMailAsync(int prodNExport)
        {
            string sigla = Dispatcher.Invoke(() => txtSigla.Content.ToString().Split(";")[0]);
            AprovadoModel aprovadoModel = await Task.Run(() => new AprovadoViewModel().GetAprovadoAsync(sigla));
            using MailMessage emailMessage = new();
            emailMessage.From = new MailAddress("envio_relatorio@cipolatti.com.br");
            //emailMessage.To.Add(new MailAddress("wesley_oliveira@cipolatti.com.br"));
            
            emailMessage.To.Add(new MailAddress("grupo_nota_fiscal@cipolatti.com.br"));
            emailMessage.CC.Add(new MailAddress("expedicao@cipolatti.com.br"));
            emailMessage.CC.Add(new MailAddress("operacionalinterno@cipolatti.com.br"));
            emailMessage.CC.Add(new MailAddress("helpdesk@cipolatti.com.br"));
            
            emailMessage.Subject = "Solicitação Nota Fisca Shopping";
            emailMessage.Body = "Em anexo arquivos para emissão da nota fiscal para o cliente " + aprovadoModel.Nome + " - " + aprovadoModel.Sigla + ", caminhão: " + Dispatcher.Invoke(() => txtPlaca.Content.ToString());
            emailMessage.Priority = MailPriority.High;// 2;
            Attachment attachment = new("ITENS.xlsx");
            Attachment attachment1 = new("ORCAMEN1.FSI");
            Attachment attachment2 = new("ORCAMEN2.FSI");
            Attachment attachment3 = new("Informacoes_Complementares.xlsx");
            emailMessage.Attachments.Add(attachment);
            emailMessage.Attachments.Add(attachment1);
            emailMessage.Attachments.Add(attachment2);
            emailMessage.Attachments.Add(attachment3);
            if (prodNExport > 0)
                emailMessage.Attachments.Add(new Attachment("Produtos.csv"));
            using SmtpClient MailClient = new("192.168.0.209", 25);
            MailClient.EnableSsl = false;
            MailClient.Credentials = new NetworkCredential("envio_relatorio@cipolatti.com.br", "@n0dh@n0dh1966");
            await MailClient.SendMailAsync(emailMessage);
        }

        private async void ButtonAdv_Click(object sender, RoutedEventArgs e)
        {
            ViewExpedicaoColetaDados expedicaoColetaDados1 = this;
            try
            {
                if (txtSigla.Content.ToString()?.Split(';', StringSplitOptions.None).Length > 1)
                {
                    MessageBox.Show("Seleciona uma sigla por vez para gerar o packing-list", "Ação Abortada", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    return;
                }

                string sigla = Dispatcher.Invoke(() => txtSigla.Content.ToString().Split(";")[0]);
                AprovadoModel aprovado = await Task.Run(async () => await new AprovadoViewModel().GetAprovadoAsync(sigla));
                IList list = await Task.Run(async () => await new ExpedicaoViewModel().GetPacklistCarregCaminhaoAsync(aprovado.SiglaServ, Dispatcher.Invoke(() => txtPlaca.Content.ToString()), DataCarregamento));

                var caminho = Path.Combine(BaseSettings.CaminhoSistema, "Impressos", "PACKING-LIST-SHOPPING.xlsx");
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Packing List");
                worksheet.Cell(1, 1).Value = aprovado.SiglaServ + " - " + aprovado.Nome;
                worksheet.Range("A1:E1").Merge();
                worksheet.Cell(1, 6).Value = "CAMINHÃO";
                worksheet.Cell(1, 7).Value = Convert.ToString(txtPlaca.Content);
                worksheet.Range("G1:H1").Merge();
                worksheet.Range("A1:H1").Style.Font.Bold = true;
                worksheet.Range("A1:H1").Style.Font.FontSize = 15;

                var headers = new[] { "COD", "local Shoppings", "Nome Caixa", "QTD", "Planilha", "Descrição", "Liquido", "Bruto", "Controlado" };
                for (var i = 0; i < headers.Length; i++)
                {
                    worksheet.Cell(2, i + 1).Value = headers[i];
                }

                InserirDados(worksheet, list, 3, false);
                var usedRange = worksheet.Range(2, 1, Math.Max(list.Count + 2, 2), 9);
                usedRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                usedRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                worksheet.Range("A2:I2").Style.Fill.BackgroundColor = XLColor.FromArgb(255, 174, 33);
                worksheet.Range("A2:I2").Style.Font.Bold = true;
                worksheet.Columns().AdjustToContents();
                worksheet.Column(2).Width = 30;
                worksheet.Column(6).Width = 60;
                worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                worksheet.PageSetup.Margins.Left = 0;
                worksheet.PageSetup.Margins.Right = 0;
                worksheet.PageSetup.Margins.Top = 0;
                worksheet.PageSetup.Margins.Bottom = 0.5;
                workbook.SaveAs(caminho);

                Process.Start(new ProcessStartInfo(caminho)
                {
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show(ex.Message);
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (!port.IsOpen)
                return;
            port.Close();
            port.Dispose();
        }

        private async void AlterarDados_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new();

            try
            {
                dialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "Planilha do Excel|*.xls;*.xlsx"
                };
                bool? result = dialog.ShowDialog();

                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });

                string filename = dialog.FileName;

                if (result == true)
                {
                    var volumes = LerBarcodesDoExcel(filename);

                    if(txtPlaca.Content.ToString().Length == 0)
                        throw new InvalidOperationException("Precisa selecionar o romaneio");

                    await new ExpedicaoViewModel().AtualizarVolumesCarregadosAsync(
                        volumes.Select(v => v.barcode),
                        txtPlaca.Content.ToString(),
                        this.DataCarregamento);

                    Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                    MessageBox.Show("volumes enviados alterado conforme Romaneio selecionado", "Operação Concluída");
                }
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show(ex.Message, "Operação Cancelada");
            }
        }
        private static void ExportarListaParaExcel(IEnumerable dados, string caminho, string worksheetName)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(worksheetName);
            InserirDados(worksheet, dados, 1, true);
            worksheet.Columns().AdjustToContents();
            Directory.CreateDirectory(Path.GetDirectoryName(caminho)!);
            workbook.SaveAs(caminho);
        }

        private static void InserirDados(IXLWorksheet worksheet, IEnumerable dados, int linhaInicial, bool incluirCabecalho)
        {
            var lista = dados.Cast<object>().ToList();
            if (lista.Count == 0)
            {
                return;
            }

            var propriedades = lista[0].GetType().GetProperties();
            var linha = linhaInicial;
            if (incluirCabecalho)
            {
                for (var coluna = 0; coluna < propriedades.Length; coluna++)
                {
                    worksheet.Cell(linha, coluna + 1).Value = propriedades[coluna].Name;
                }
                worksheet.Range(linha, 1, linha, propriedades.Length).Style.Font.Bold = true;
                linha++;
            }

            foreach (var item in lista)
            {
                for (var coluna = 0; coluna < propriedades.Length; coluna++)
                {
                    worksheet.Cell(linha, coluna + 1).Value = XLCellValue.FromObject(propriedades[coluna].GetValue(item));
                }
                linha++;
            }
        }

        private static void AdicionarLinhaInformacaoNf(IXLWorksheet worksheet, ref int row, string label, object? valor)
        {
            worksheet.Range(row, 1, row, 2).Merge().Value = label;
            worksheet.Range(row, 3, row, 7).Merge().Value = valor?.ToString() ?? string.Empty;
            worksheet.Range(row, 1, row, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            worksheet.Range(row, 1, row, 7).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            worksheet.Range(row, 1, row, 2).Style.Font.Bold = true;
            row++;
        }

        private static List<BarcodeModel> LerBarcodesDoExcel(string filename)
        {
            using var workbook = new XLWorkbook(filename);
            var worksheet = workbook.Worksheets.First();
            return worksheet.Column(1)
                .CellsUsed()
                .Select(c => c.GetString())
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(v => new BarcodeModel { barcode = v })
                .ToList();
        }
    }
}




