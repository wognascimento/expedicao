using CsvHelper;
using Expedicao.DataBaseLocal;
using Expedicao.Model;
using Microsoft.EntityFrameworkCore;
using Syncfusion.XlsIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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

        private async void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            string recieved_data = port.ReadExisting();
            if (await Task.Run(async () => await new ViewModelLocal().GetVolume(recieved_data)))
            {
                await Task.Run(async () => await new ViewModelLocal().GetAddItemCarregado(recieved_data));
                await Task.Run(async () => await new ViewModelLocal().GetRemoveItemFaltante(recieved_data));
                IList itensFaltantes = await Task.Run(async () => await new ViewModelLocal().Getaitens());
                int volumesFaltantes = await Task.Run(async () => await new ViewModelLocal().GetVolumes());
                Dispatcher.Invoke((Action)(() => itens.ItemsSource = itensFaltantes));
                Dispatcher.Invoke((Action)(() => txtVolumes.Content = volumesFaltantes));
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
                var dados = await Task.Run(async () => await new ViewModelLocal().Getaitens());
                txtSigla.Content = string.Join("; ", await Task.Run(async () => await new ViewModelLocal().GetRomaneios()));
                txtPlaca.Content = await new ViewModelLocal().GetPlaca();
                txtConferente.Content = await new ViewModelLocal().GetConferente();
                itens.ItemsSource = dados;
                txtVolumes.Content = dados.Count; //await Task.Run(async () => await new ViewModelLocal().GetVolumes());
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
            if ((await Task.Run(async () => await new ViewModelLocal().ItemCarregadosAsync())).Count == 0)
            {
                await Task.Run(async () => await new ViewModelLocal().GetRemoveAllItemFaltante());
                await Task.Run(async () => await new ViewModelLocal().GetRemoveAllItemCarregado());
                await Task.Run((async () => await new ViewModelLocal().GetRemoveAllIRomaneios()));
                itens.ItemsSource = await Task.Run(async () => await new ViewModelLocal().Getaitens());
                Window window = new();
                window.Title = "EXPEDIÇÃO ROMANEIOS ";
                window.Content = new ViewExpedicaoRomaneios("CARREGAMENTO");
                window.Height = 600.0;
                window.Width = 680.0;
                window.ResizeMode = ResizeMode.NoResize;
                window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                window.ShowDialog();
                foreach (RomaneioModel romaneio in ((ViewExpedicaoRomaneios)window.Content).Romaneios)
                    await new ViewModelLocal().GetAdicionarRomaneio(new Romaneio()
                    {
                        id_aprovado = 0,
                        placa = romaneio.PlacaCarroceria,
                        sigla = romaneio.ShoppingDestino,
                        conferente = romaneio.NomeConferente,
                        data = (DateTime)romaneio.DataCarregamento
                    });
                Label label = txtSigla;
                label.Content = string.Join("; ", await new ViewModelLocal().GetRomaneios());
                label = txtPlaca;
                label.Content = await new ViewModelLocal().GetPlaca();
                label = txtConferente;
                label.Content = await new ViewModelLocal().GetConferente();

                this.DataCarregamento =  await new ViewModelLocal().GetDataCarregamento();
            }
            else
            {
                MessageBox.Show("É necessário encerrar o carregamento para selecionar outro(s) cliente(s)");
            }
        }

        private async void RibbonButton_Click_1(object sender, RoutedEventArgs e)
        {
            Window window = new();
            window.Title = "EXPEDIÇÃO CAMINHÕES ";
            window.Content = new ViewExpedicaoLookupCaminao();
            window.Height = 300.0;
            window.Width = 450.0;
            window.ResizeMode = ResizeMode.NoResize;
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            window.ShowDialog();
            try
            {
                var dados = await Task.Run(async () => await new ViewModelLocal().Getaitens());
                loading.Visibility = Visibility.Visible;
                itens.ItemsSource = dados;
                txtVolumes.Content = dados.Count; //await Task.Run(async () => await new ViewModelLocal().GetVolumes());
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
                Romaneio romaneio = new Romaneio();
                romaneio.conferente = await new ViewModelLocal().GetConferente();
                romaneio.data = await new ViewModelLocal().GetDataCarregamento();
                romaneio.placa = await new ViewModelLocal().GetPlaca();

                foreach (ItemCarregado itemCarregado in await Task.Run(async () => await new ViewModelLocal().ItemCarregadosAsync()))
                {
                    ConfCargaGeralModel conf = new ConfCargaGeralModel()
                    {
                        Barcode = itemCarregado.barcode,
                        DocaOrigem = "JACAREÍ",
                        Data = romaneio.data,
                        Resp = romaneio.conferente,
                        Caminhao = romaneio.placa
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

                List<string> siglas = await Task.Run(async () => await new ViewModelLocal().GetRomaneios());
                //IAsyncEnumerable<string> siglas = (IAsyncEnumerable<string>)await Task.Run(async () => await new ViewModelLocal().GetRomaneios());

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


                IList dados = await Task.Run(async () => await new ExpedicaoViewModel().GetCarregamentoItemCaminhaosSemParemetroAsync(await Task.Run(async () => await new ViewModelLocal().GetRomaneios())));
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


                    await Task.Run(() => new ViewModelLocal().GetRemoveAllItemFaltante());
                    await Task.Run(() => new ViewModelLocal().GetRemoveAllItemCarregado());
                    await Task.Run(() => new ViewModelLocal().GetRemoveAllIRomaneios());

                    //itens.ItemsSource = await Task.Run(() => new ViewModelLocal().Getaitens());
   

                    loading.Visibility = Visibility.Hidden;
                    MessageBox.Show("Email enviado para o fiscal");
                    txtSigla.Content = null;
                    txtPlaca.Content = null;
                    txtConferente.Content = null;
                    txtVolumes.Content = 0;
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
            using ExcelEngine excelEngine = new ExcelEngine();
            IApplication excel = excelEngine.Excel;
            excel.DefaultVersion = (ExcelVersion)3;
            IWorkbook iworkbook = excel.Workbooks.Create(1);
            iworkbook.Worksheets[0].ImportData(reports, 1, 1, true);
            iworkbook.SaveAs("ProdutoSemParametro.xlsx");
            iworkbook.Close();
            excelEngine.Dispose();
            Process.Start(new ProcessStartInfo("ProdutoSemParametro.xlsx")
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
                var siglas = await Task.Run(() =>  new ViewModelLocal().GetRomaneios());
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
                //Dispatcher.Invoke(() => txtPlaca.Content.ToString());

                var siglas = await Task.Run(() => new ViewModelLocal().GetRomaneios());
                var itens = await Task.Run(() => new ExpedicaoViewModel().GetCarregamentoItemCaminhaosAsync(siglas, Dispatcher.Invoke(() => txtPlaca.Content.ToString())));  //await new ExpedicaoViewModel().GetCarregamentoItemCaminhaosAsync(siglas, "");
                StreamWriter sw = new StreamWriter("ORCAMEN2.FSI");
                int item = 0;
                for (int i = 0; i < itens.Count; ++i)
                {
                    ++item;
                    decimal valorFormatado = (decimal)(itens[i].Custo * 1000);
                    await sw.WriteLineAsync(
                        /*01*/"F220" +
                        /*02*/Convert.ToString(codigo).ToString().PadLeft(6, '0') +
                        /*03*/Convert.ToString(item).PadRight(14) +
                        /*04*/Convert.ToString(itens[i].CodComplAdicional).PadRight(30) +
                        /*05*/Convert.ToString(RemoverAcentos(itens[i].DescricaoFiscal)).PadRight(60) +
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
                        /*16*/string.Format("{0:000000000000.00}", itens[i].Qtd).Replace(",", null).Replace(".", null) +
                        /*17*/Convert.ToString(itens[i].Unidade).PadRight(3) +
                        /*18*/string.Format("{0:000000000000.00}", itens[i].Custo).Replace(",", null).Replace(".", null) +
                        /*19*/string.Format("{0:000000000000.00}", 0).Replace(",", null).Replace(".", null) +
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
                }
                sw.Close();

                var volumes = await Task.Run(() => new ExpedicaoViewModel().GetCarregamentoVolumesAsync(siglas, Dispatcher.Invoke(() => txtPlaca.Content.ToString())));
                
                foreach (var volume in volumes)
                {
                    await Task.Run(() => new ExpedicaoViewModel().GetVolumeCarregado(volume.codexped));
                    //GetVolumeCarregado
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
            IApplication excel = new ExcelEngine().Excel;
            excel.DefaultVersion = ExcelVersion.Xlsx;
            IWorkbook workbook = excel.Workbooks.Create(1);
            IWorksheet worksheet = workbook.Worksheets[0];
            worksheet.IsGridLinesVisible = false;
            this.index = 0;
            List<ResumoNotaModel> resumoNotaModelList = await Task.Run(async () => await new ExpedicaoViewModel().GetInformasoesNfAsync(await Task.Run(async () => await new ViewModelLocal().GetRomaneios()), Dispatcher.Invoke(() => txtPlaca.Content.ToString())));
            worksheet.Range["A1"].Text = "INFORMAÇOES COMPLEMENTARES NF SHOPPING";
            for (int index = 0; index < resumoNotaModelList.Count; ++index)
            {
                this.index = this.index + index + 1;
                worksheet.Range["A" + this.index.ToString()].Text = "INFORMAÇOES COMPLEMENTARES NF SHOPPING";
                worksheet.Range["A" + this.index.ToString()].CellStyle.Font.Bold = true;
                worksheet.Range["A" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.HorizontalAlignment = (ExcelHAlign)2;
                worksheet.Range["A" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.VerticalAlignment = (ExcelVAlign)1;
                worksheet.Range["A" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["A" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["A" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["A" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["A" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].Color = (ExcelKnownColors)0;
                worksheet.Range["A" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].Color = (ExcelKnownColors)0;
                worksheet.Range["A" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].Color = (ExcelKnownColors)0;
                worksheet.Range["A" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].Color = (ExcelKnownColors)0;
                worksheet.Range["A" + this.index.ToString() + ":G" + this.index.ToString()].Merge();
                this.index += 2;
                worksheet.Range["A" + this.index.ToString()].Text = "Sigla";
                worksheet.Range["A" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["A" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["A" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["A" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["A" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].Color = (ExcelKnownColors)0;
                worksheet.Range["A" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].Color = (ExcelKnownColors)0;
                worksheet.Range["A" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].Color = (ExcelKnownColors)0;
                worksheet.Range["A" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].Color = (ExcelKnownColors)0;
                worksheet.Range["B" + this.index.ToString()].Text = resumoNotaModelList[index].Shopp;
                worksheet.Range["B" + this.index.ToString() + ":C" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["B" + this.index.ToString() + ":C" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["B" + this.index.ToString() + ":C" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["B" + this.index.ToString() + ":C" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["B" + this.index.ToString() + ":C" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].Color = (ExcelKnownColors)0;
                worksheet.Range["B" + this.index.ToString() + ":C" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].Color = (ExcelKnownColors)0;
                worksheet.Range["B" + this.index.ToString() + ":C" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].Color = (ExcelKnownColors)0;
                worksheet.Range["B" + this.index.ToString() + ":C" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].Color = (ExcelKnownColors)0;
                worksheet.Range["B" + this.index.ToString() + ":C" + this.index.ToString()].Merge();
                worksheet.Range["D" + this.index.ToString()].Text = resumoNotaModelList[index].Nome;
                worksheet.Range["D" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["D" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["D" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["D" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["D" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].Color = (ExcelKnownColors)0;
                worksheet.Range["D" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].Color = (ExcelKnownColors)0;
                worksheet.Range["D" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].Color = (ExcelKnownColors)0;
                worksheet.Range["D" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].Color = (ExcelKnownColors)0;
                worksheet.Range["D" + this.index.ToString() + ":G" + this.index.ToString()].Merge();
                ++this.index;
                worksheet.Range["A" + this.index.ToString()].Text = "Caminhão";
                worksheet.Range["A" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["A" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["A" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["A" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["A" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].Color = (ExcelKnownColors)0;
                worksheet.Range["A" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].Color = (ExcelKnownColors)0;
                worksheet.Range["A" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].Color = (ExcelKnownColors)0;
                worksheet.Range["A" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].Color = (ExcelKnownColors)0;
                worksheet.Range["B" + this.index.ToString()].Text = resumoNotaModelList[index].Caminhao;
                worksheet.Range["B" + this.index.ToString() + ":C" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["B" + this.index.ToString() + ":C" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["B" + this.index.ToString() + ":C" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["B" + this.index.ToString() + ":C" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["B" + this.index.ToString() + ":C" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].Color = (ExcelKnownColors)0;
                worksheet.Range["B" + this.index.ToString() + ":C" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].Color = (ExcelKnownColors)0;
                worksheet.Range["B" + this.index.ToString() + ":C" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].Color = (ExcelKnownColors)0;
                worksheet.Range["B" + this.index.ToString() + ":C" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].Color = (ExcelKnownColors)0;
                worksheet.Range["B" + this.index.ToString() + ":C" + this.index.ToString()].Merge();
                worksheet.Range["D" + this.index.ToString()].Text = "Data Scanner";
                worksheet.Range["D" + this.index.ToString() + ":E" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["D" + this.index.ToString() + ":E" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["D" + this.index.ToString() + ":E" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["D" + this.index.ToString() + ":E" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["D" + this.index.ToString() + ":E" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].Color = (ExcelKnownColors)0;
                worksheet.Range["D" + this.index.ToString() + ":E" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].Color = (ExcelKnownColors)0;
                worksheet.Range["D" + this.index.ToString() + ":E" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].Color = (ExcelKnownColors)0;
                worksheet.Range["D" + this.index.ToString() + ":E" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].Color = (ExcelKnownColors)0;
                worksheet.Range["F" + this.index.ToString()].Text = resumoNotaModelList[index].Data.Value.ToShortDateString();
                worksheet.Range["F" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["F" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["F" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["F" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["F" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].Color = (ExcelKnownColors)0;
                worksheet.Range["F" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].Color = (ExcelKnownColors)0;
                worksheet.Range["F" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].Color = (ExcelKnownColors)0;
                worksheet.Range["F" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].Color = (ExcelKnownColors)0;
                worksheet.Range["F" + this.index.ToString() + ":G" + this.index.ToString()].Merge();
                ++this.index;
                worksheet.Range["A" + this.index.ToString()].Text = "Transpor.";
                worksheet.Range["A" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["A" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["A" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["A" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["A" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].Color = (ExcelKnownColors)0;
                worksheet.Range["A" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].Color = (ExcelKnownColors)0;
                worksheet.Range["A" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].Color = (ExcelKnownColors)0;
                worksheet.Range["A" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].Color = (ExcelKnownColors)0;
                worksheet.Range["B" + this.index.ToString()].Text = resumoNotaModelList[index].NomeTransportadora;
                worksheet.Range["B" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["B" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["B" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["B" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["B" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].Color = (ExcelKnownColors)0;
                worksheet.Range["B" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].Color = (ExcelKnownColors)0;
                worksheet.Range["B" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].Color = (ExcelKnownColors)0;
                worksheet.Range["B" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].Color = (ExcelKnownColors)0;
                worksheet.Range["B" + this.index.ToString() + ":G" + this.index.ToString()].Merge();
               ++this.index;
                worksheet.Range["A" + this.index.ToString()].Text = "CNPJ";
                worksheet.Range["A" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["A" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["A" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["A" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["A" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].Color = (ExcelKnownColors)0;
                worksheet.Range["A" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].Color = (ExcelKnownColors)0;
                worksheet.Range["A" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].Color = (ExcelKnownColors)0;
                worksheet.Range["A" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].Color = (ExcelKnownColors)0;
                worksheet.Range["B" + this.index.ToString()].Text = resumoNotaModelList[index].Cnpj;
                worksheet.Range["B" + this.index.ToString() + ":D" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["B" + this.index.ToString() + ":D" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["B" + this.index.ToString() + ":D" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["B" + this.index.ToString() + ":D" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["B" + this.index.ToString() + ":D" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].Color = (ExcelKnownColors)0;
                worksheet.Range["B" + this.index.ToString() + ":D" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].Color = (ExcelKnownColors)0;
                worksheet.Range["B" + this.index.ToString() + ":D" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].Color = (ExcelKnownColors)0;
                worksheet.Range["B" + this.index.ToString() + ":D" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].Color = (ExcelKnownColors)0;
                worksheet.Range["B" + this.index.ToString() + ":D" + this.index.ToString()].Merge();
                worksheet.Range["E" + this.index.ToString()].Text = "IE";
                worksheet.Range["E" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["E" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["E" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["E" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["E" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].Color = (ExcelKnownColors)0;
                worksheet.Range["E" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].Color = (ExcelKnownColors)0;
                worksheet.Range["E" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].Color = (ExcelKnownColors)0;
                worksheet.Range["E" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].Color = (ExcelKnownColors)0;
                worksheet.Range["F" + this.index.ToString()].Text = resumoNotaModelList[index].Ie;
                worksheet.Range["F" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["F" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["F" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["F" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["F" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].Color = (ExcelKnownColors)0;
                worksheet.Range["F" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].Color = (ExcelKnownColors)0;
                worksheet.Range["F" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].Color = (ExcelKnownColors)0;
                worksheet.Range["F" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].Color = (ExcelKnownColors)0;
                worksheet.Range["F" + this.index.ToString() + ":G" + this.index.ToString()].Merge();
                ++this.index;
                worksheet.Range["A" + this.index.ToString()].Text = "Endereço";
                worksheet.Range["A" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["A" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["A" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["A" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["A" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].Color = (ExcelKnownColors)0;
                worksheet.Range["A" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].Color = (ExcelKnownColors)0;
                worksheet.Range["A" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].Color = (ExcelKnownColors)0;
                worksheet.Range["A" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].Color = (ExcelKnownColors)0;
                worksheet.Range["B" + this.index.ToString()].Text = resumoNotaModelList[index].Endereco;
                worksheet.Range["B" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["B" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["B" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["B" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["B" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].Color = (ExcelKnownColors)0;
                worksheet.Range["B" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].Color = (ExcelKnownColors)0;
                worksheet.Range["B" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].Color = (ExcelKnownColors)0;
                worksheet.Range["B" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].Color = (ExcelKnownColors)0;
                worksheet.Range["B" + this.index.ToString() + ":G" + this.index.ToString()].Merge();
                ++this.index;
                worksheet.Range["A" + this.index.ToString()].Text = "Bairro";
                worksheet.Range["A" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["A" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["A" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["A" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["A" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].Color = (ExcelKnownColors)0;
                worksheet.Range["A" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].Color = (ExcelKnownColors)0;
                worksheet.Range["A" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].Color = (ExcelKnownColors)0;
                worksheet.Range["A" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].Color = (ExcelKnownColors)0;
                worksheet.Range["B" + this.index.ToString()].Text = resumoNotaModelList[index].Bairro;
                worksheet.Range["B" + this.index.ToString() + ":D" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["B" + this.index.ToString() + ":D" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["B" + this.index.ToString() + ":D" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["B" + this.index.ToString() + ":D" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["B" + this.index.ToString() + ":D" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].Color = (ExcelKnownColors)0;
                worksheet.Range["B" + this.index.ToString() + ":D" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].Color = (ExcelKnownColors)0;
                worksheet.Range["B" + this.index.ToString() + ":D" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].Color = (ExcelKnownColors)0;
                worksheet.Range["B" + this.index.ToString() + ":D" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].Color = (ExcelKnownColors)0;
                worksheet.Range["B" + this.index.ToString() + ":D" + this.index.ToString()].Merge();
                worksheet.Range["E" + this.index.ToString()].Text = "Cidade";
                worksheet.Range["E" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["E" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["E" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["E" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["E" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].Color = (ExcelKnownColors)0;
                worksheet.Range["E" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].Color = (ExcelKnownColors)0;
                worksheet.Range["E" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].Color = (ExcelKnownColors)0;
                worksheet.Range["E" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].Color = (ExcelKnownColors)0;
                worksheet.Range["F" + this.index.ToString()].Text = resumoNotaModelList[index].Cidade;
                worksheet.Range["F" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["F" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["F" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["F" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["F" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].Color = (ExcelKnownColors)0;
                worksheet.Range["F" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].Color = (ExcelKnownColors)0;
                worksheet.Range["F" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].Color = (ExcelKnownColors)0;
                worksheet.Range["F" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].Color = (ExcelKnownColors)0;
                worksheet.Range["F" + this.index.ToString() + ":G" + this.index.ToString()].Merge();
                ++this.index;
                worksheet.Range["A" + this.index.ToString()].Text = "CEP";
                worksheet.Range["A" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["A" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["A" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["A" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["A" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].Color = (ExcelKnownColors)0;
                worksheet.Range["A" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].Color = (ExcelKnownColors)0;
                worksheet.Range["A" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].Color = (ExcelKnownColors)0;
                worksheet.Range["A" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].Color = (ExcelKnownColors)0;
                worksheet.Range["B" + this.index.ToString()].Text = resumoNotaModelList[index].Cep;
                worksheet.Range["B" + this.index.ToString() + ":D" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["B" + this.index.ToString() + ":D" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["B" + this.index.ToString() + ":D" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["B" + this.index.ToString() + ":D" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["B" + this.index.ToString() + ":D" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].Color = (ExcelKnownColors)0;
                worksheet.Range["B" + this.index.ToString() + ":D" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].Color = (ExcelKnownColors)0;
                worksheet.Range["B" + this.index.ToString() + ":D" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].Color = (ExcelKnownColors)0;
                worksheet.Range["B" + this.index.ToString() + ":D" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].Color = (ExcelKnownColors)0;
                worksheet.Range["B" + this.index.ToString() + ":D" + this.index.ToString()].Merge();
                worksheet.Range["E" + this.index.ToString()].Text = "UF";
                worksheet.Range["E" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["E" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["E" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["E" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["E" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].Color = (ExcelKnownColors)0;
                worksheet.Range["E" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].Color = (ExcelKnownColors)0;
                worksheet.Range["E" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].Color = (ExcelKnownColors)0;
                worksheet.Range["E" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].Color = (ExcelKnownColors)0;
                worksheet.Range["F" + this.index.ToString()].Text = resumoNotaModelList[index].Uf;
                worksheet.Range["F" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["F" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["F" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["F" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["F" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].Color = (ExcelKnownColors)0;
                worksheet.Range["F" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].Color = (ExcelKnownColors)0;
                worksheet.Range["F" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].Color = (ExcelKnownColors)0;
                worksheet.Range["F" + this.index.ToString() + ":G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].Color = (ExcelKnownColors)0;
                worksheet.Range["F" + this.index.ToString() + ":G" + this.index.ToString()].Merge();
                ++this.index;
                worksheet.Range["F" + this.index.ToString()].Text = "Volumes";
                worksheet.Range["E" + this.index.ToString() + ":F" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["E" + this.index.ToString() + ":F" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["E" + this.index.ToString() + ":F" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["E" + this.index.ToString() + ":F" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["E" + this.index.ToString() + ":F" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].Color = (ExcelKnownColors)0;
                worksheet.Range["E" + this.index.ToString() + ":F" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].Color = (ExcelKnownColors)0;
                worksheet.Range["E" + this.index.ToString() + ":F" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].Color = (ExcelKnownColors)0;
                worksheet.Range["E" + this.index.ToString() + ":F" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].Color = (ExcelKnownColors)0;
                worksheet.Range["E" + this.index.ToString() + ":F" + this.index.ToString()].Merge();
                worksheet.Range["G" + this.index.ToString()].Number = (double)resumoNotaModelList[index].volumes;
                worksheet.Range["G" + this.index.ToString()].NumberFormat = "#,##0.00";
                worksheet.Range["G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].Color = (ExcelKnownColors)0;
                worksheet.Range["G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].Color = (ExcelKnownColors)0;
                worksheet.Range["G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].Color = (ExcelKnownColors)0;
                worksheet.Range["G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].Color = (ExcelKnownColors)0;
                ++this.index;
                worksheet.Range["F" + this.index.ToString()].Text = "Peso Liquido";
                worksheet.Range["E" + this.index.ToString() + ":F" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["E" + this.index.ToString() + ":F" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["E" + this.index.ToString() + ":F" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["E" + this.index.ToString() + ":F" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["E" + this.index.ToString() + ":F" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].Color = (ExcelKnownColors)0;
                worksheet.Range["E" + this.index.ToString() + ":F" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].Color = (ExcelKnownColors)0;
                worksheet.Range["E" + this.index.ToString() + ":F" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].Color = (ExcelKnownColors)0;
                worksheet.Range["E" + this.index.ToString() + ":F" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].Color = (ExcelKnownColors)0;
                worksheet.Range["E" + this.index.ToString() + ":F" + this.index.ToString()].Merge();
                worksheet.Range["G" + this.index.ToString()].Number = (double)resumoNotaModelList[index].Liquido;
                worksheet.Range["G" + this.index.ToString()].NumberFormat = "#,##0.00";
                worksheet.Range["G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].Color = (ExcelKnownColors)0;
                worksheet.Range["G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].Color = (ExcelKnownColors)0;
                worksheet.Range["G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].Color = (ExcelKnownColors)0;
                worksheet.Range["G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].Color = (ExcelKnownColors)0;
                ++this.index;
                worksheet.Range["F" + this.index.ToString()].Text = "Peso Bruto";
                worksheet.Range["E" + this.index.ToString() + ":F" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["E" + this.index.ToString() + ":F" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["E" + this.index.ToString() + ":F" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["E" + this.index.ToString() + ":F" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["E" + this.index.ToString() + ":F" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].Color = (ExcelKnownColors)0;
                worksheet.Range["E" + this.index.ToString() + ":F" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].Color = (ExcelKnownColors)0;
                worksheet.Range["E" + this.index.ToString() + ":F" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].Color = (ExcelKnownColors)0;
                worksheet.Range["E" + this.index.ToString() + ":F" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].Color = (ExcelKnownColors)0;
                worksheet.Range["E" + this.index.ToString() + ":F" + this.index.ToString()].Merge();
                worksheet.Range["G" + this.index.ToString()].Number = (double)resumoNotaModelList[index].Bruto;
                worksheet.Range["G" + this.index.ToString()].NumberFormat = "#,##0.00";
                worksheet.Range["G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].Color = (ExcelKnownColors)0;
                worksheet.Range["G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].Color = (ExcelKnownColors)0;
                worksheet.Range["G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].Color = (ExcelKnownColors)0;
                worksheet.Range["G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].Color = (ExcelKnownColors)0;
                ++this.index;
                worksheet.Range["F" + this.index.ToString()].Text = "Preço";
                worksheet.Range["E" + this.index.ToString() + ":F" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["E" + this.index.ToString() + ":F" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["E" + this.index.ToString() + ":F" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["E" + this.index.ToString() + ":F" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["E" + this.index.ToString() + ":F" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].Color = (ExcelKnownColors)0;
                worksheet.Range["E" + this.index.ToString() + ":F" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].Color = (ExcelKnownColors)0;
                worksheet.Range["E" + this.index.ToString() + ":F" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].Color = (ExcelKnownColors)0;
                worksheet.Range["E" + this.index.ToString() + ":F" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].Color = (ExcelKnownColors)0;
                worksheet.Range["E" + this.index.ToString() + ":F" + this.index.ToString()].Merge();
                worksheet.Range["G" + this.index.ToString()].Number = (double)resumoNotaModelList[index].Preco;
                worksheet.Range["G" + this.index.ToString()].NumberFormat = "#,##0.00";
                worksheet.Range["G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].LineStyle = (ExcelLineStyle)1;
                worksheet.Range["G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)8].Color = (ExcelKnownColors)0;
                worksheet.Range["G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)9].Color = (ExcelKnownColors)0;
                worksheet.Range["G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)7].Color = (ExcelKnownColors)0;
                worksheet.Range["G" + this.index.ToString()].CellStyle.Borders[(ExcelBordersIndex)10].Color = (ExcelKnownColors)0;
            }
            workbook.SaveAs("Informacoes_Complementares.xlsx");
        }
        private async Task<int> GetProdutosNaoExportadosMaticAsync()
        {
            /*
            IWorkbook workbook;
            IWorksheet worksheet;
            using (ExcelEngine excelEngine = new ExcelEngine())
            {
                var dados = await Task.Run(async () => await new ExpedicaoViewModel().GetCarregamentoItemCaminhaosNaoExportadoMaticAsync(await Task.Run(async () => await new ViewModelLocal().GetRomaneios())));
                
                IApplication excel = excelEngine.Excel;
                excel.DefaultVersion = ExcelVersion.Xlsx;
                workbook = excel.Workbooks.Create(1);
                worksheet = workbook.Worksheets[0];
                worksheet.ImportData(dados, 1, 1, true);
                workbook.SaveAs("ProdutosNaoExportadosMatic.xlsx");
                workbook.Close();
                excelEngine.Dispose();
                
                return dados.Count;
            }
            */

            var dados = await Task.Run(async () => await new ExpedicaoViewModel().GetCarregamentoItemCaminhaosNaoExportadoMaticAsync(await Task.Run(async () => await new ViewModelLocal().GetRomaneios())));

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
            Attachment attachment1 = new("ORCAMEN1.FSI");
            Attachment attachment2 = new("ORCAMEN2.FSI");
            //Attachment attachment = new(@"C:\Temp\ORCAMENTO.zip");
            Attachment attachment3 = new("Informacoes_Complementares.xlsx");
            emailMessage.Attachments.Add(attachment1);
            emailMessage.Attachments.Add(attachment2);
            //emailMessage.Attachments.Add(attachment);
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
                ViewExpedicaoColetaDados expedicaoColetaDados = expedicaoColetaDados1;
                IWorkbook workbook;
                IWorksheet worksheet;
                IStyle headerStyle;
                IStyle bodyStyle;
                using (ExcelEngine excelEngine = new ExcelEngine())
                {
                    IApplication excel = excelEngine.Excel;
                    excel.DefaultVersion = ExcelVersion.Xlsx;
                    workbook = excel.Workbooks.Create(1);
                    worksheet = workbook.Worksheets[0];
                    if (txtSigla.Content.ToString()?.Split(';', StringSplitOptions.None).Length > 1)
                    {
                        MessageBox.Show("Seleciona uma sigla por vez para gerar o packing-list", "Ação Abortada", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                        return;
                    }
                    //AprovadoModel aprovado = await Task.Run<AprovadoModel>(new Func<Task<AprovadoModel>>(expedicaoColetaDados1.\u003CButtonAdv_Click\u003Eb__17_0));
                    string sigla = Dispatcher.Invoke(() => txtSigla.Content.ToString().Split(";")[0]);
                    AprovadoModel aprovado = await Task.Run(async () => await new AprovadoViewModel().GetAprovadoAsync(sigla));

                    worksheet.Range["A1"].Text = aprovado.SiglaServ + " - " + aprovado.Nome;
                    worksheet.Range["A1"].CellStyle.Font.Size = 15.0;
                    worksheet.Range["A1"].CellStyle.Font.Bold = true;
                    worksheet.Range["A1:E1"].Merge();
                    worksheet.Range["F1"].Text = "CAMINHÃO";
                    worksheet.Range["F1"].CellStyle.Font.Size = 15.0;
                    worksheet.Range["F1"].CellStyle.Font.Bold = true;
                    worksheet.Range["F1"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight; //(ExcelHAlign)3;
                    worksheet.Range["G1"].Text = (string)expedicaoColetaDados1.txtPlaca.Content;
                    worksheet.Range["G1"].CellStyle.Font.Size = 15.0;
                    worksheet.Range["G1"].CellStyle.Font.Bold = true;
                    worksheet.Range["G1:H1"].Merge();
                    worksheet.Range["A2"].Text = "COD";
                    worksheet.Range["B2"].Text = "local Shoppings";
                    worksheet.Range["C2"].Text = "Nome Caixa";
                    worksheet.Range["D2"].Text = "QTD";
                    worksheet.Range["E2"].Text = "Planilha";
                    worksheet.Range["F2"].Text = "Descrição";
                    worksheet.Range["G2"].Text = "Liquido";
                    worksheet.Range["H2"].Text = "Bruto";
                    worksheet.Range["I2"].Text = "Controlado";
                    workbook.SetPaletteColor(8, Color.FromArgb((int)byte.MaxValue, 174, 33));
                    headerStyle = workbook.Styles.Add("HeaderStyle");
                    headerStyle.BeginUpdate();
                    headerStyle.Color = Color.FromArgb((int)byte.MaxValue, 174, 33);
                    headerStyle.Font.Bold = true;
                    headerStyle.Borders[ExcelBordersIndex.EdgeLeft].LineStyle = ExcelLineStyle.Thin;
                    headerStyle.Borders[ExcelBordersIndex.EdgeRight].LineStyle = ExcelLineStyle.Thin;
                    headerStyle.Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                    headerStyle.Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Thin;
                    headerStyle.EndUpdate();
                    workbook.SetPaletteColor(9, Color.FromArgb(239, 243, 247));
                    bodyStyle = workbook.Styles.Add("BodyStyle");
                    bodyStyle.BeginUpdate();
                    bodyStyle.Borders[ExcelBordersIndex.EdgeLeft].LineStyle = ExcelLineStyle.Thin;
                    bodyStyle.Borders[ExcelBordersIndex.EdgeRight].LineStyle = ExcelLineStyle.Thin;
                    bodyStyle.Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                    bodyStyle.Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Thin;
                    bodyStyle.WrapText = true;
                    bodyStyle.EndUpdate();

                    IList list = await Task.Run(async () => await new ExpedicaoViewModel().GetPacklistCarregCaminhaoAsync(aprovado.SiglaServ, Dispatcher.Invoke(()=>txtPlaca.Content.ToString()), this.DataCarregamento));
                    IRange range = worksheet.Range;

                    //DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(4, 1);
                    //interpolatedStringHandler.AppendLiteral("A3:H");
                    //interpolatedStringHandler.AppendFormatted<int>(list.Count + 2);
                    //string stringAndClear = interpolatedStringHandler.ToStringAndClear();

                    range[$"A3:I{list.Count + 2}"].CellStyle = bodyStyle;
                    worksheet.Rows[1].CellStyle = headerStyle;
                    worksheet.ImportData(list, 3, 1, false);
                    worksheet.PageSetup.PrintTitleColumns = "$A:$I";
                    worksheet.PageSetup.PrintTitleRows = "$1:$2";
                    worksheet.AutofitColumn(1);
                    worksheet.SetColumnWidth(2, 30.0);
                    worksheet.AutofitColumn(3);
                    worksheet.AutofitColumn(4);
                    worksheet.AutofitColumn(5);
                    worksheet.SetColumnWidth(6, 60.0);
                    worksheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                    worksheet.PageSetup.LeftMargin = 0.0;
                    worksheet.PageSetup.RightMargin = 0.0;
                    worksheet.PageSetup.TopMargin = 0.0;
                    worksheet.PageSetup.BottomMargin = 0.5;
                    worksheet.PageSetup.RightFooter = "&P";
                    workbook.SaveAs("PACKING-LIST-SHOPPING.xlsx");
                    workbook.Close();
                    excelEngine.Dispose();
                    Process.Start(new ProcessStartInfo("PACKING-LIST-SHOPPING.xlsx")
                    {
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                int num = (int)MessageBox.Show(ex.Message);
            }
        }

        private void itens_ItemsSourceChanged(object sender, Syncfusion.UI.Xaml.Grid.GridItemsSourceChangedEventArgs e)
        {

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

            //ConfCargaGerals
            using AppDatabase db = new();
            try
            {
                dialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "Planilha do Excel|*.xls;*.xlsx"
                };
                bool? result = dialog.ShowDialog();

                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });

                string filename = dialog.FileName;

                using ExcelEngine excelEngine = new();
                IApplication application = excelEngine.Excel;
                application.DefaultVersion = ExcelVersion.Xlsx;
                IWorkbook workbook = application.Workbooks.OpenReadOnly(filename);
                IWorksheet worksheet = workbook.Worksheets[0];

                if (result == true)
                {
                    List<BarcodeModel> dadosExcel = worksheet.ExportData<BarcodeModel>(1, 1, 5000, 1);
                    var volumes = dadosExcel.Where(x => x.barcode != null).ToList();

                    if(txtPlaca.Content.ToString().Length == 0)
                        throw new InvalidOperationException("Precisa selecionar o romaneio");

                    var strategy = db.Database.CreateExecutionStrategy();
                     await strategy.ExecuteAsync(async () => 
                     {
                         using var transaction = db.Database.BeginTransaction();
                         try
                         {
                             foreach (var volume in volumes)
                             {
                                 var vol = await db.ConfCargaGerals.FirstOrDefaultAsync(x=> x.Barcode == volume.barcode) ?? throw new InvalidOperationException("Existe código não carregado na lista enviada");
                                 vol.Caminhao = txtPlaca.Content.ToString();
                                 vol.Data = this.DataCarregamento;
                                 vol.DataAltera = DateTime.Now.Date;
                                 vol.AlteradoPor = Environment.UserName;
                                 db.ConfCargaGerals.Update(vol);
                                 await db.SaveChangesAsync();  
                             }
                             transaction.Commit();
                             Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                             MessageBox.Show("volumes enviados alterado conforme Romaneio selecionado", "Operação Concluída");
                         }
                         catch (DbUpdateException ex)
                         {
                             Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                             transaction.Rollback();
                             MessageBox.Show(ex?.InnerException?.Message, "Operação Cancelada");
                         }
                         catch(Exception ex)
                         {
                             Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                             transaction.Rollback();
                             MessageBox.Show(ex?.Message, "Operação Cancelada");
                         }

                     });
                }
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show(ex.Message, "Operação Cancelada");
            }
        }
    }
}
