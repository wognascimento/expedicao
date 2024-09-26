using CsvHelper;
using Expedicao.DataBaseLocal;
using Expedicao.Model;
using Microsoft.EntityFrameworkCore;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Expedicao.Views
{
    /// <summary>
    /// Interação lógica para ViewExpedicaoImpressaoEtiqueta.xam
    /// </summary>
    public partial class ViewExpedicaoImpressaoEtiqueta : UserControl
    {
        public ViewExpedicaoImpressaoEtiqueta()
        {
            InitializeComponent();
        }

        private async void UserControl_Initialized(object sender, EventArgs e)
        {
            try
            {
                itens.ItemsSource = await Task.Run(new ExpedicaoViewModel().GetEtiquetaVolumesAsync);
                loadingDetalhes.Visibility = Visibility.Hidden;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
    }

    public static class ContextMenuCommands
    {
        private const string IPAdress = "192.168.0.191"; //http://192.168.0.191/
        private const int Port = 9100;
        private static BaseCommand? print;
        private static BaseCommand? printAll;
        private static BaseCommand? criarArquivo;
        private static BaseCommand? criarArquivoCliente;
        private static StreamWriter? streamWriter1;// = new StreamWriter(@"C:\TEMP\ETIQUETA.TXT");

        public static BaseCommand Print
        {
            get
            {
                /*if (ContextMenuCommands.print == null)
                    ContextMenuCommands.print = new BaseCommand(new Action<object>(ContextMenuCommands.OnPrintClicked));*/

                ContextMenuCommands.print ??= new BaseCommand(new Action<object>(ContextMenuCommands.OnPrintClicked));

                return ContextMenuCommands.print;
            }
        }

        public static BaseCommand PrintAll
        {
            get
            {
                /*if (ContextMenuCommands.printAll == null)
                    ContextMenuCommands.printAll = new BaseCommand(new Action<object>(ContextMenuCommands.OnPrintAllClicked));*/

                ContextMenuCommands.printAll ??= new BaseCommand(new Action<object>(ContextMenuCommands.OnPrintAllClicked));
                return ContextMenuCommands.printAll;
            }
        }

        public static BaseCommand CriarArquivo
        {
            get
            {
                ContextMenuCommands.criarArquivo ??= new BaseCommand(new Action<object>(ContextMenuCommands.OnCriarArquivoCaminhaoClicked));
                return ContextMenuCommands.criarArquivo;
            }
        }

        public static BaseCommand CriarArquivoCliente
        {
            get
            {
                ContextMenuCommands.criarArquivoCliente ??= new BaseCommand(new Action<object>(ContextMenuCommands.OnCriarArquivoClienteClicked));
                return ContextMenuCommands.criarArquivoCliente;
            }
        }

        private async static void OnPrintClicked(object obj)
        {

            try
            {
                TcpClient? client = new();
                await client.ConnectAsync(IPAdress, Port);
                streamWriter1 = new StreamWriter(client.GetStream());

                SfDataGrid dataGrid = ((GridContextMenuInfo)obj).DataGrid;
                await EtiquetaNormal((EtiquetaVolumeItemModel)dataGrid.CurrentItem, (List<EtiquetaVolumeItemModel>)dataGrid.ItemsSource);

                await streamWriter1.FlushAsync();
                await streamWriter1.DisposeAsync();
                streamWriter1.Close();

                MessageBox.Show("Etiquetas impressas", "Impressão de etiquetas", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }

        private async static void OnPrintAllClicked(object obj)
        {
            TcpClient? client = new();
            await client.ConnectAsync(IPAdress, Port);
            streamWriter1 = new StreamWriter(client.GetStream());

            if (obj is not GridColumnContextMenuInfo)
                return;
            SfDataGrid dataGrid = ((GridContextMenuInfo)obj).DataGrid;
            var source = dataGrid.View.Records.Select(rec => rec.Data);
            List<EtiquetaVolumeItemModel> itemsSource = (List<EtiquetaVolumeItemModel>)dataGrid.ItemsSource;
            var groupings = dataGrid.View.Records.GroupBy(rec => ((EtiquetaVolumeItemModel)rec.Data).Sequencia);
            if (MessageBox.Show("Deseja imprimir " + source.Count() + " Produtos?", "Impressão de etiquetas", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.No)
                return;
            foreach (var grouping in groupings)
            {
                var item = grouping;
                await EtiquetaNormal((EtiquetaVolumeItemModel)dataGrid.View.Records.FirstOrDefault(rec => ((EtiquetaVolumeItemModel)rec.Data).Sequencia == item.Key).Data, itemsSource);
            }

            await streamWriter1.FlushAsync();
            await streamWriter1.DisposeAsync();
            streamWriter1.Close();
            client.Close();

            //streamWriter1.Write(buffer, 0, buffer.Length);

            MessageBox.Show("Etiquetas impressas", "Impressão de etiquetas", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private static async Task EtiquetaNormal(EtiquetaVolumeItemModel row, List<EtiquetaVolumeItemModel> dt)
        {
            if (row.BaiaCaminhao == null || row.BaiaCaminhao.Trim().Length == 0)
            {
                MessageBox.Show("Etiqueta sem número de caminão não pode ser impressa", "Impressão de etiquetas", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
               

            try
            {
                /*
                streamWriter1.WriteLine($@"^XA~TA000~JSN^LT0^MNW^MTT^PON^PMN^LH0,0^JMA^PR5,5~SD15^JUS^LRN^CI28^XZ");
                streamWriter1.WriteLine($@"^XA");
                streamWriter1.WriteLine($@"^MMT");
                streamWriter1.WriteLine($@"^PW799");
                streamWriter1.WriteLine($@"^LL1199");
                streamWriter1.WriteLine($@"^LS0");
                streamWriter1.WriteLine($@"^FO0,32^GFA,01152,01152,00012,:Z64:");
                streamWriter1.WriteLine($@"eJztkT0KAjEQhTOLIlglldViKzmFhVcQq2U9goUwlSCWnkKsJLfwBm5hnZME3ajzI2wjCDZONXkzfO8lMeaXBQCfrFvnnO0e9brpMCeDQg0Gal/pbuzcnAyGWxmMptIrHcqzXMAfZAeX0ivdxUYuEE7CwTX3hejQTwnIYBiCxEHkQK1OgaCOt2zwxIdAxoDIBsUxhAnp15Qq4u+F88b3wjF1jBdnLfE5qOLnfQ5apbQ5A+fcE6jlz8hA5TfjR35LOvsOEFfUF8oXUn6g10H55kDc+zeD2HSCcCE/4HdsbEpeb+tg/vX1ugPL3lH5:5877");
                streamWriter1.WriteLine($@"^FO16,117^GB86,31,31^FS");
                streamWriter1.WriteLine($@"^FT16,141^A0N,25,24^FR^FH\^FD{DateTime.Now:MM/dd/yy}^FS");
                streamWriter1.WriteLine($@"^FO131,21^GB517,102,102^FS");
                streamWriter1.WriteLine($@"^FT131,102^A0N,81,60^FB517,1,0,C^FR^FH\^FD{row.Sigla}^FS");
                streamWriter1.WriteLine($@"^FT646,283^BQN,2,6");
                streamWriter1.WriteLine($@"^FH\^FDLA,{row.Sigla}|{row.Volume}^FS");
                streamWriter1.WriteLine($@"^BY3,3,56^FT234,195^BCN,,Y,N");
                streamWriter1.WriteLine($@"^FD>;{row.Barcode}^FS");
                streamWriter1.WriteLine($@"^FT28,182^A0N,25,20^FB90,1,0,C^FH\^FDCAMINHÃO^FS");
                streamWriter1.WriteLine($@"^FT53,254^A0N,80,90^FB35,1,0,C^FH\^FD{row.BaiaCaminhao}^FS");
                streamWriter1.WriteLine($@"^FT133,280^AAN,50,20^FB511,1,0,C^FH\^FDLOCAL DO SHOPPING^FS");
                List<string> stringList1 = QuebraString(row.LocalShoppings, 43);
                streamWriter1.WriteLine($@"^FT9,358^A0N,75,33^FB778,1,0,C^FH\^FD{(stringList1.Count >= 1 ? stringList1[0].Trim() : "")}^FS");
                streamWriter1.WriteLine($@"^FT9,429^A0N,75,33^FB778,1,0,C^FH\^FD{(stringList1.Count == 2 ? stringList1[1].Trim() : "")}^FS");
                streamWriter1.WriteLine($@"^FT108,1050^AAN,40,10^FB181,1,0,C^FH\^FDCONTROLE EXPED.^FS");
                streamWriter1.WriteLine($@"^FO41,1057^GB312,98,98^FS");
                streamWriter1.WriteLine($@"^FT41,1135^A0N,85,108^FB312,1,0,C^FR^FH\^FD{row.Volume}^FS");
                streamWriter1.WriteLine($@"^FT630,910^AAN,40,20^FB100,1,0,C^FH\^FDITEM^FS");
                streamWriter1.WriteLine($@"^FT500,1060^AAN,40,20^FB200,1,0,C^FH\^FDVOLUME^FS");
                streamWriter1.WriteLine($@"^FT379,1140^A0N,85,110^FB417,1,0,C^FH\^FD{row.VolExp} / {row.VolTotExp}^FS");
                streamWriter1.WriteLine($@"^FO609,922^GB162,85,85^FS");
                streamWriter1.WriteLine($@"^FT609,990^A0N,75,67^FB162,1,0,C^FR^FH\^FD{row.ItemMemorial}^FS");
                streamWriter1.WriteLine($@"^FT73,930^AAN,50,20^FB100,1,0,C^FH\^FDDET^FS");
                streamWriter1.WriteLine($@"^FT132,510^AAN,60,25^FB511,1,0,C^FH\^FDDESCRIÇÃO PRODUTO^FS");
                List<string> stringList2 = QuebraString(row.Descricao, 43);
                streamWriter1.WriteLine($@"^FT10,586^A0N,75,33^FB778,1,0,C^FH\^FD{(stringList2.Count >= 1 ? stringList2[0].Trim() : "")}^FS");
                streamWriter1.WriteLine($@"^FT10,657^A0N,75,33^FB778,1,0,C^FH\^FD{(stringList2.Count >= 2 ? stringList2[1].Trim() : "")}^FS");
                streamWriter1.WriteLine($@"^FT10,728^A0N,75,33^FB778,1,0,C^FH\^FD{(stringList2.Count >= 3 ? stringList2[2].Trim() : "")}^FS");
                streamWriter1.WriteLine($@"^FT10,799^A0N,75,33^FB778,1,0,C^FH\^FD{(stringList2.Count >= 4 ? stringList2[3].Trim() : "")}^FS");
                streamWriter1.WriteLine($@"^FT10,870^A0N,75,33^FB778,1,0,C^FH\^FD{(stringList2.Count >= 5 ? stringList2[4].Trim() : "")}^FS");
                streamWriter1.WriteLine($@"^FT10,990^A0N,70,67^FB200,1,0,C^FH\^FD{row.CodDetalhesCompl}^FS");
                streamWriter1.WriteLine($@"^FT250,930^AAN,50,20^FB250,1,0,C^FH\^FDQUANTIDADE^FS");
                streamWriter1.WriteLine($@"^FT280,990^A0N,70,67^FB200,1,0,C^FH\^FD{row.QtdExpedida}^FS");
                streamWriter1.WriteLine($@"^LRY^FO9,157^GB123,0,113^FS^LRN");
                streamWriter1.WriteLine($@"^PQ1,0,1,Y^XZ");
                */

                streamWriter1.WriteLine($@"^XA~TA000~JSN^LT0^MNW^MTT^PON^PMN^LH0,0^JMA^PR5,5~SD15^JUS^LRN^CI28^XZ");
                streamWriter1.WriteLine($@"^XA");
                streamWriter1.WriteLine($@"^MMT");
                streamWriter1.WriteLine($@"^PW799");
                streamWriter1.WriteLine($@"^LL1199");
                streamWriter1.WriteLine($@"^LS0");
                streamWriter1.WriteLine($@"^FO0,32^GFA,01152,01152,00012,:Z64:");
                streamWriter1.WriteLine($@"eJztkT0KAjEQhTOLIlglldViKzmFhVcQq2U9goUwlSCWnkKsJLfwBm5hnZME3ajzI2wjCDZONXkzfO8lMeaXBQCfrFvnnO0e9brpMCeDQg0Gal/pbuzcnAyGWxmMptIrHcqzXMAfZAeX0ivdxUYuEE7CwTX3hejQTwnIYBiCxEHkQK1OgaCOt2zwxIdAxoDIBsUxhAnp15Qq4u+F88b3wjF1jBdnLfE5qOLnfQ5apbQ5A+fcE6jlz8hA5TfjR35LOvsOEFfUF8oXUn6g10H55kDc+zeD2HSCcCE/4HdsbEpeb+tg/vX1ugPL3lH5:5877");
                streamWriter1.WriteLine($@"^FO16,117^GB86,31,31^FS");
                streamWriter1.WriteLine($@"^FT16,141^A0N,25,24^FR^FH\^FD{DateTime.Now:dd/MM/yy}^FS");
                streamWriter1.WriteLine($@"^FO131,21^GB517,102,102^FS");
                streamWriter1.WriteLine($@"^FT131,102^A0N,81,60^FB517,1,0,C^FR^FH\^FD{row.Sigla}^FS");
                streamWriter1.WriteLine($@"^FT295,382^BQN,2,10");
                streamWriter1.WriteLine($@"^FH\^FDLA,{row.Sigla}|{row.Volume}^FS");
                streamWriter1.WriteLine($@"^FT28,182^A0N,18,16^FB90,1,0,C^FH\^FDCAMINHÃO^FS");
                streamWriter1.WriteLine($@"^FT53,254^A0N,73,72^FB35,1,0,C^FH\^FD{row.BaiaCaminhao}^FS");
                streamWriter1.WriteLine($@"^FT132,390^AAN,36,25^FB511,1,0,C^FH\^FDLOCAL DO SHOPPING^FS");
                List<string> stringList1 = QuebraString(row.LocalShoppings, 43);
                streamWriter1.WriteLine($@"^FT9,451^A0N,56,33^FB778,1,0,C^FH\^FD{(stringList1.Count >= 1 ? stringList1[0].Trim() : "")}^FS");
                streamWriter1.WriteLine($@"^FT9,522^A0N,56,33^FB778,1,0,C^FH\^FD{(stringList1.Count == 2 ? stringList1[1].Trim() : "")}^FS");
                streamWriter1.WriteLine($@"^FT108,1051^AAN,18,10^FB181,1,0,C^FH\^FDCONTROLE EXPED.^FS");
                streamWriter1.WriteLine($@"^FO41,1057^GB312,98,98^FS");
                streamWriter1.WriteLine($@"^FT41,1135^A0N,78,108^FB312,1,0,C^FR^FH\^FD{row.Volume}^FS");
                streamWriter1.WriteLine($@"^FT667,944^AAN,18,10^FB49,1,0,C^FH\^FDITEM^FS");
                streamWriter1.WriteLine($@"^FT553,1051^AAN,18,10^FB73,1,0,C^FH\^FDVOLUME^FS");
                streamWriter1.WriteLine($@"^FT379,1140^A0N,85,110^FB417,1,0,C^FH\^FD{row.VolExp} / {row.VolTotExp}^FS");
                streamWriter1.WriteLine($@"^FO609,951^GB162,85,85^FS");
                streamWriter1.WriteLine($@"^FT609,1019^A0N,68,67^FB162,1,0,C^FR^FH\^FD{row.ItemMemorial}^FS");
                streamWriter1.WriteLine($@"^FT73,944^AAN,18,10^FB37,1,0,C^FH\^FDDET^FS");
                streamWriter1.WriteLine($@"^FT132,566^AAN,36,25^FB511,1,0,C^FH\^FDDESCRIÇÃO PRODUTO^FS");
                List<string> stringList2 = QuebraString(row.Descricao, 43);
                streamWriter1.WriteLine($@"^FT10,634^A0N,56,33^FB778,1,0,C^FH\^FD{(stringList2.Count >= 1 ? stringList2[0].Trim() : "")}^FS");
                streamWriter1.WriteLine($@"^FT10,705^A0N,56,33^FB778,1,0,C^FH\^FD{(stringList2.Count >= 2 ? stringList2[1].Trim() : "")}^FS");
                streamWriter1.WriteLine($@"^FT10,776^A0N,56,33^FB778,1,0,C^FH\^FD{(stringList2.Count >= 3 ? stringList2[2].Trim() : "")}^FS");
                streamWriter1.WriteLine($@"^FT10,847^A0N,56,33^FB778,1,0,C^FH\^FD{(stringList2.Count >= 4 ? stringList2[3].Trim() : "")}^FS");
                streamWriter1.WriteLine($@"^FT10,918^A0N,56,33^FB778,1,0,C^FH\^FD{(stringList2.Count >= 5 ? stringList2[4].Trim() : "")}^FS");
                streamWriter1.WriteLine($@"^FT10,1019^A0N,68,67^FB160,1,0,C^FH\^FD{row.CodDetalhesCompl}^FS");
                streamWriter1.WriteLine($@"^FT302,944^AAN,18,10^FB121,1,0,C^FH\^FDQUANTIDADE^FS");
                streamWriter1.WriteLine($@"^FT297,1019^A0N,68,67^FB128,1,0,C^FH\^FD{row.QtdExpedida}^FS");
                streamWriter1.WriteLine($@"^LRY^FO9,157^GB123,0,113^FS^LRN");
                streamWriter1.WriteLine($@" ^ PQ1,0,1,Y^XZ");

                if ((bool)row.Anexo)
                    EtiquetaAnexo((long)row.CodDetalhesCompl, (long)row.Sequencia, row.Sigla, dt);

                LiberarImpressaoModel liberarImpressao = new LiberarImpressaoModel()
                {
                    Sigla = row.Sigla,
                    Planilha = row.Planilha,
                    NomeCaixa = row.CodVol,
                    Impresso = true
                };
                CaixaModel caixaModel = await Task.Run(async () => await new ExpedicaoViewModel().LiberarImpresaoAsync(liberarImpressao));
                //client = null;

            
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private static void EtiquetaAnexo(
          long coddetalhescompl,
          long sequencia,
          string sigla,
          List<EtiquetaVolumeItemModel> dados)
        {
            List<EtiquetaVolumeItemModel> list = dados.Where(n => n.CodDetalhesCompl != coddetalhescompl && n.Sequencia == sequencia && n.Sigla == sigla).ToList();
            int num1 = list.Count % 2;
            int count = list.Count;
            int num2 = 1;

            foreach (EtiquetaVolumeItemModel etiquetaVolumeItemModel in list)
            {
                List<string> stringList1 = new List<string>();
                if (num2 == 1)
                {
                    streamWriter1.WriteLine($@"^XA~TA000~JSN^LT0^MNW^MTT^PON^PMN^LH0,0^JMA^PR7~SD15^JUS^LRN^CI28^XZ");
                    streamWriter1.WriteLine($@"^XA");
                    streamWriter1.WriteLine($@"^MMT");
                    streamWriter1.WriteLine($@"^PW799");
                    streamWriter1.WriteLine($@"^LL1199");
                    streamWriter1.WriteLine($@"^LS0");
                    streamWriter1.WriteLine($@"^FT198,74^AAN,36,20^FH\^FDLOCAL DO SHOPPING^FS");
                    List<string> stringList2 = QuebraString(etiquetaVolumeItemModel.LocalShoppings, 43);
                    streamWriter1.WriteLine($@"^FT10,134^A0N,42,33^FB778,1,0,C^FH\^FD{(stringList2.Count >= 1 ? stringList2[0].Trim() : "")}^FS");
                    streamWriter1.WriteLine($@"^FT10,186^A0N,42,33^FB778,1,0,C^FH\^FD{(stringList2.Count == 2 ? stringList2[1].Trim() : "")}^FS");
                    streamWriter1.WriteLine($@"^FT198,237^AAN,36,20^FB409,1,0,C^FH\^FDDESCRIÇÃO PRODUTO^FS");
                    List<string> stringList3 = QuebraString(etiquetaVolumeItemModel.Descricao, 43);
                    streamWriter1.WriteLine($@"^FT10,295^A0N,42,33^FB778,1,0,C^FH\^FD{(stringList3.Count >= 1 ? stringList3[0].Trim() : "")}^FS");
                    streamWriter1.WriteLine($@"^FT10,347^A0N,42,33^FB778,1,0,C^FH\^FD{(stringList3.Count >= 2 ? stringList3[1].Trim() : "")}^FS");
                    streamWriter1.WriteLine($@"^FT10,399^A0N,42,33^FB778,1,0,C^FH\^FD{(stringList3.Count >= 3 ? stringList3[2].Trim() : "")}^FS");
                    streamWriter1.WriteLine($@"^FT10,451^A0N,42,33^FB778,1,0,C^FH\^FD{(stringList3.Count >= 4 ? stringList3[3].Trim() : "")}^FS");
                    streamWriter1.WriteLine($@"^FT10,503^A0N,42,33^FB778,1,0,C^FH\^FD{(stringList3.Count >= 5 ? stringList3[4].Trim() : "")}^FS");
                    streamWriter1.WriteLine($@"^FT228,555^AAN,18,10^FH\^FDQUANTIDADE: ^FS");
                    streamWriter1.WriteLine($@"^FT390,570^A0N,42,38^FB76,1,0,C^FH\^FD{etiquetaVolumeItemModel.QtdExpedida}^FS");
                    streamWriter1.WriteLine($@"^FT25,555^AAN,18,10^FB86,1,0,C^FH\^FDDET: ^FS");
                    streamWriter1.WriteLine($@"^FT99,570^A0N,42,38^FB95,1,0,C^FH\^FD{etiquetaVolumeItemModel.CodDetalhesCompl}^FS");
                }
                if (num2 == 2)
                {
                    streamWriter1.WriteLine($@"^FO2,603^GB794,0,2^FS");
                    streamWriter1.WriteLine($@"^FT198,644^AAN,36,20^FH\^FDLOCAL DO SHOPPING^FS");
                    List<string> stringList4 = QuebraString(etiquetaVolumeItemModel.LocalShoppings, 43);
                    streamWriter1.WriteLine($@"^FT10,711^A0N,42,33^FB778,1,0,C^FH\^FD{(stringList4.Count >= 1 ? stringList4[0].Trim() : "")}^FS");
                    streamWriter1.WriteLine($@"^FT10,763^A0N,42,33^FB778,1,0,C^FH\^FD{(stringList4.Count == 2 ? stringList4[1].Trim() : "")}^FS");
                    streamWriter1.WriteLine($@"^FT198,812^AAN,36,20^FB409,1,0,C^FH\^FDDESCRIÇÃO PRODUTO^FS");
                    List<string> stringList5 = QuebraString(etiquetaVolumeItemModel.Descricao, 43);
                    streamWriter1.WriteLine($@"^FT10,875^A0N,42,33^FB778,1,0,C^FH\^FD{(stringList5.Count >= 1 ? stringList5[0].Trim() : "")}^FS");
                    streamWriter1.WriteLine($@"^FT10,927^A0N,42,33^FB778,1,0,C^FH\^FD{(stringList5.Count >= 2 ? stringList5[1].Trim() : "")}^FS");
                    streamWriter1.WriteLine($@"^FT10,979^A0N,42,33^FB778,1,0,C^FH\^FD{(stringList5.Count >= 3 ? stringList5[2].Trim() : "")}^FS");
                    streamWriter1.WriteLine($@"^FT10,1031^A0N,42,33^FB778,1,0,C^FH\^FD{(stringList5.Count >= 4 ? stringList5[3].Trim() : "")}^FS");
                    streamWriter1.WriteLine($@"^FT10,1083^A0N,42,33^FB778,1,0,C^FH\^FD{(stringList5.Count >= 5 ? stringList5[4].Trim() : "")}^FS");
                    streamWriter1.WriteLine($@"^FT14,1145^AAN,18,10^FB86,1,0,C^FH\^FDDET : ^FS");
                    streamWriter1.WriteLine($@"^FT88,1160^A0N,42,38^FB95,1,0,C^FH\^FD{etiquetaVolumeItemModel.CodDetalhesCompl}^FS");
                    streamWriter1.WriteLine($@"^FT216,1145^AAN,18,10^FH\^FDQUANTIDADE: ^FS");
                    streamWriter1.WriteLine($@"^FT378,1160^A0N,42,38^FB76,1,0,C^FH\^FD{etiquetaVolumeItemModel.QtdExpedida}^FS");
                }
                if (num2 == 2 || count == 1)
                {
                    streamWriter1.WriteLine("^PQ1,0,1,Y^XZ");
                    num2 = 1;
                }
                else
                    num2 = 2;
                --count;
            }
        }

        private static List<string> QuebraString(string valor, int tamanho)
        {
            List<string> stringList = new List<string>();
            int length = tamanho;
            for (int startIndex = 0; startIndex < valor.Length; startIndex += length)
            {
                if (startIndex + length > valor.Length)
                    length = valor.Length - startIndex;
                string str = valor.Substring(startIndex, length);
                stringList.Add(str);
            }
            return stringList;
        }

        private async static void OnCriarArquivoCaminhaoClicked(object obj)
        {
            var record = ((GridRecordContextMenuInfo)obj).Record as CubagemSiglaCaminaoModel;
            using AppDatabase db = new();
            try
            {
                var itens = await db.ItensCaminhoes.Where(w => w.sigla == record.sigla && w.baia_caminhao == record.baia_caminhao).ToListAsync();
                var itensSemparametros = itens.Where(w => w.custo is null || w.custo == 0 || w.peso is null || w.peso == 0).ToList();

                if(itensSemparametros.Count > 0)
                {
                    StreamWriter swP = new(@"C:\TEMP\PRODUTOS.TXT");
                    foreach (var produto in itensSemparametros)
                    {
                        await swP.WriteLineAsync(produto.codcompladicional.ToString());
                    }
                    swP.Close();
                    MessageBox.Show(@"EXISTEM PRODUTOS SEM CUSTO OU PREÇO, EM 'C:\TEMP\PRODUTOS.TXT' FOI SALVO A RELAÇÃO COM OS CÓDIGOS ");
                    return;
                }

                var sequencia = new OrcamentoSequenceModel { Cliente = record.sigla };
                db.OrcamentoSequences.Add(sequencia);
                await db.SaveChangesAsync();
                long num2 = sequencia.NumeroOrcamento.Value;

                StreamWriter sw1 = new(@"C:\TEMP\ORCAMEN1.FSI");
                await sw1.WriteLineAsync(
                    /*01*/"F210" +
                    /*02*/Convert.ToString(num2).ToString().PadLeft(6, '0') +
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
                sw1.Close();

                StreamWriter sw2 = new(@"C:\TEMP\ORCAMEN2.FSI");
                int item = 0;
                for (int i = 0; i < itens.Count; ++i)
                {
                    ++item;
                    await sw2.WriteLineAsync(
                        /*01*/"F220" +
                        /*02*/Convert.ToString(num2).ToString().PadLeft(6, '0') +
                        /*03*/Convert.ToString(item).PadRight(14) +
                        /*04*/Convert.ToString(itens[i].codcompladicional).PadRight(30) +
                        /*05*/Convert.ToString(itens[i].descricaofiscal).PadRight(60) +
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
                        /*16*/string.Format("{0:000000000000.00}", itens[i].qtd).Replace(",", null).Replace(".", null) +
                        /*17*/Convert.ToString(itens[i].unidade).PadRight(3) +
                        /*18*/string.Format("{0:000000000000.00}", itens[i].custo).Replace(",", null).Replace(".", null) +
                        /*19*/string.Format("{0:000000000000.00}", itens[i].qtd * itens[i].custo).Replace(",", null).Replace(".", null) +
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
                sw2.Close();

                IList listAsync;
                listAsync = itens
                    .Where(x => x.exportado_folhamatic == null)
                    .OrderBy(x => x.descricaofiscal)
                    .Select(x => new
                    {
                        IDENTIFICACAO = x.codcompladicional,
                        DESCRICAO = x.descricaofiscal,
                        NCM = x.ncm,
                        CODBARRA = "",
                        UNIDADEDECOMPRA = x.unidade,
                        UNIDADEVENDA = x.unidade,
                        SITUACAOTRIBUTARIAA = "0",
                        SITUACAOTRIBUTARIAB = "41",
                        CSOSN = "",
                        SITTRIBPIS = "PIS 70 - Operação de Aquisição sem Direito a Crédito",
                        SITTRIBCOFINS = "COFINS 70 - Operação de Aquisição sem Direito a Crédito",
                        SITTRIBIPI = "IPI 99 - Outras saídas",
                        IPI = "0",
                        ICMS = "",
                        REDUCAOICMS = "",
                        ALIQCOFINS = "0",
                        ALIQPIS = "",
                        CATEGORIA = "",
                        CEST = "",
                        CFOP = "",
                        CODIGODEBENEFICIOFISCAL = "",
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
                    .ToList();

                using (var streamWriter = new StreamWriter(@"C:\Temp\Produtos.csv"))
                using (var csvWriter = new CsvWriter(streamWriter, new CultureInfo("pt-BR", true)))
                {                                            
                    csvWriter.WriteRecords(listAsync);
                    streamWriter.Flush();
                }

                MessageBox.Show(@"AQUIVO CRIADO EM 'C:\TEMP'. VERIFICA SE TEM PRODUTOS NÃO ENVIADO PARA A IOB PELO ARQUIVO 'Produtos.csv'");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private async static void OnCriarArquivoClienteClicked(object obj)
        {
            var record = ((GridRecordContextMenuInfo)obj).Record as CubagemSiglaCaminaoModel; //((GridRecordContextMenuInfo)obj).Record = {Expedicao.Model.CubagemSiglaCaminaoModel}
            using AppDatabase db = new();
            try
            {
                var itens = db.ItensCaminhoes
                    .AsEnumerable()
                    .Where(w => w.sigla == record.sigla)
                    .GroupBy(x => new { x.sigla, x.codcompladicional, x.descricaofiscal, x.unidade, x.ncm, x.custo, x.peso, x.exportado_folhamatic })
                    .Select(g => new ItemCaminhaoModel
                    {
                        sigla = g.Key.sigla,
                        codcompladicional = g.Key.codcompladicional,
                        descricaofiscal = g.Key.descricaofiscal,
                        unidade = g.Key.unidade,
                        ncm = g.Key.ncm,
                        qtd = g.Sum(x => x.qtd ?? 0),
                        custo = g.Key.custo,
                        peso = g.Key.peso,
                        exportado_folhamatic = g.Key.exportado_folhamatic
                    })
                    .OrderBy(x => x.descricaofiscal)
                    .ToList();

                var itensSemparametros = itens.Where(w => w.custo is null || w.custo == 0 || w.peso is null || w.peso == 0).ToList();

                if (itensSemparametros.Count > 0)
                {
                    StreamWriter swP = new(@"C:\TEMP\PRODUTOS.TXT");
                    await swP.WriteAsync("");
                    foreach (var produto in itensSemparametros)
                    {
                        await swP.WriteLineAsync(produto.codcompladicional.ToString());
                    }
                    swP.Close();
                    MessageBox.Show(@"EXISTEM PRODUTOS SEM CUSTO OU PREÇO, EM 'C:\TEMP\PRODUTOS.TXT' FOI SALVO A RELAÇÃO COM OS CÓDIGOS ");
                    return;
                }

                var sequencia = new OrcamentoSequenceModel { Cliente = record.sigla };
                db.OrcamentoSequences.Add(sequencia);
                await db.SaveChangesAsync();
                long num2 = sequencia.NumeroOrcamento.Value;

                StreamWriter sw1 = new(@"C:\TEMP\ORCAMEN1.FSI");
                await sw1.WriteLineAsync(
                    /*01*/"F210" +
                    /*02*/Convert.ToString(num2).ToString().PadLeft(6, '0') +
                    /*03*/DateTime.Now.ToString("ddMMyyyy") +
                    /*04*/Convert.ToString("").PadRight(6,'0') +
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
                    /*17*/Convert.ToString("").PadRight(3,'0') +
                    /*18*/DateTime.Now.ToString("ddMMyyyy") +
                    /*19*/Convert.ToString("").PadRight(4,'0') +
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
                sw1.Close();

                StreamWriter sw2 = new(@"C:\TEMP\ORCAMEN2.FSI");
                int item = 0;
                for (int i = 0; i < itens.Count; ++i)
                {
                    ++item;
                    await sw2.WriteLineAsync(
                        /*01*/"F220" +
                        /*02*/Convert.ToString(num2).ToString().PadLeft(6, '0') +
                        /*03*/Convert.ToString(item).PadRight(14) +
                        /*04*/Convert.ToString(itens[i].codcompladicional).PadRight(30) +
                        /*05*/Convert.ToString(itens[i].descricaofiscal).PadRight(60) +
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
                        /*16*/string.Format("{0:000000000000.00}", itens[i].qtd).Replace(",", null).Replace(".", null) +
                        /*17*/Convert.ToString(itens[i].unidade).PadRight(3) +
                        /*18*/string.Format("{0:000000000000.00}", itens[i].custo).Replace(",", null).Replace(".", null) +
                        /*19*/string.Format("{0:000000000000.00}", itens[i].qtd * itens[i].custo).Replace(",", null).Replace(".", null) +
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
                sw2.Close();

                IList listAsync;
                listAsync = itens
                    .Where(x => x.exportado_folhamatic == null)
                    .OrderBy(x => x.descricaofiscal)
                    .Select(x => new
                    {
                        IDENTIFICACAO = x.codcompladicional,
                        DESCRICAO = x.descricaofiscal,
                        NCM = x.ncm,
                        CODBARRA = "",
                        UNIDADEDECOMPRA = x.unidade,
                        UNIDADEVENDA = x.unidade,
                        SITUACAOTRIBUTARIAA = "0",
                        SITUACAOTRIBUTARIAB = "41",
                        CSOSN = "",
                        SITTRIBPIS = "PIS 70 - Operação de Aquisição sem Direito a Crédito",
                        SITTRIBCOFINS = "COFINS 70 - Operação de Aquisição sem Direito a Crédito",
                        SITTRIBIPI = "IPI 99 - Outras saídas",
                        IPI = "0",
                        ICMS = "",
                        REDUCAOICMS = "",
                        ALIQCOFINS = "0",
                        ALIQPIS = "",
                        CATEGORIA = "",
                        CEST = "",
                        CFOP = "",
                        CODIGODEBENEFICIOFISCAL = "",
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
                    .ToList();

                using (var streamWriter = new StreamWriter(@"C:\Temp\Produtos.csv"))
                using (var csvWriter = new CsvWriter(streamWriter, new CultureInfo("pt-BR", true)))
                {
                    csvWriter.WriteRecords(listAsync);
                    streamWriter.Flush();
                }

                MessageBox.Show(@"AQUIVO CRIADO EM 'C:\TEMP'. VERIFICA SE TEM PRODUTOS NÃO ENVIADO PARA A IOB PELO ARQUIVO 'Produtos.csv'");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
    }
}
