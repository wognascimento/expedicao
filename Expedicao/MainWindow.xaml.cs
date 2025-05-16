using CsvHelper;
using Expedicao.Views;
using Microsoft.EntityFrameworkCore;
using Squirrel;
using Syncfusion.SfSkinManager;
using Syncfusion.Windows.Tools.Controls;
using Syncfusion.XlsIO;
using Syncfusion.XlsIO.Implementation.PivotTables;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Telerik.Windows.Controls;
using SizeMode = Syncfusion.SfSkinManager.SizeMode;

namespace Expedicao
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private DataBase dB = DataBase.Instance;
        private UpdateManager manager;
        DataBase BaseSettings = DataBase.Instance;

        #region Fields
        private string currentVisualStyle;
		private string currentSizeMode;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the current visual style.
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public string CurrentVisualStyle
        {
            get
            {
                return currentVisualStyle;
            }
            set
            {
                currentVisualStyle = value;
                OnVisualStyleChanged();
            }
        }
		
		/// <summary>
        /// Gets or sets the current Size mode.
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public string CurrentSizeMode
        {
            get
            {
                return currentSizeMode;
            }
            set
            {
                currentSizeMode = value;
                OnSizeModeChanged();
            }
        }
        #endregion
        public MainWindow()
        {
            InitializeComponent();
			this.Loaded += OnLoaded;
            StyleManager.ApplicationTheme = new Windows11Theme();

            var appSettings = ConfigurationManager.GetSection("appSettings") as NameValueCollection;
            if (appSettings[0].Length > 0)
                dB.Username = appSettings[0];

            txtUsername.Text = dB.Username;
            txtDataBase.Text = dB.Database;
        }
		/// <summary>
        /// Called when [loaded].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            CurrentVisualStyle = "Metro"; //"FluentLight";
	        CurrentSizeMode = "Default";
            /*
            try
            {
                manager = await UpdateManager.GitHubUpdateManager(@"https://github.com/wognascimento/expedicao");
                var updateInfo = await manager.CheckForUpdate();
                if (updateInfo.ReleasesToApply.Count > 0)
                {
                    RadWindow.Confirm(new DialogParameters()
                    {
                        Header = "Atualização",
                        Content = "Existe uma atualização para o sistema, deseja atualiza?",
                        Closed = async (object sender, WindowClosedEventArgs e) =>
                        {
                            var result = e.DialogResult;
                            if (result == true)
                            {
                                await manager.UpdateApp();
                                RadWindow.Alert("Sistema atualizado!\nFecha e abre o Sistema, para aplicar a atualização.");
                            }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                RadWindow.Alert(ex.Message);
            }
            */
        }
		/// <summary>
        /// On Visual Style Changed.
        /// </summary>
        /// <remarks></remarks>
        private void OnVisualStyleChanged()
        {
            VisualStyles visualStyle = VisualStyles.Default;
            Enum.TryParse(CurrentVisualStyle, out visualStyle);            
            if (visualStyle != VisualStyles.Default)
            {
                SfSkinManager.ApplyStylesOnApplication = true;
                SfSkinManager.SetVisualStyle(this, visualStyle);
                SfSkinManager.ApplyStylesOnApplication = false;
            }
        }
		
		/// <summary>
        /// On Size Mode Changed event.
        /// </summary>
        /// <remarks></remarks>
        private void OnSizeModeChanged()
        {
            SizeMode sizeMode = SizeMode.Default;
            Enum.TryParse(CurrentSizeMode, out sizeMode);
            if (sizeMode != SizeMode.Default)
            {
                SfSkinManager.ApplyStylesOnApplication = true;
                SfSkinManager.SetSizeMode(this, sizeMode);
                SfSkinManager.ApplyStylesOnApplication = false;
            }
        }

        public void adicionarFilho(object filho, string title, string name)
        {
            var doc = ExistDocumentInDocumentContainer(name);
            if (doc == null)
            {
                doc = (FrameworkElement?)filho;
                DocumentContainer.SetHeader(doc, title);
                doc.Name = name.ToLower();
                _mdi.Items.Add(doc);
            }
            else
            {
                //_mdi.RestoreDocument(doc as UIElement);
                _mdi.ActiveDocument = doc;
            }
        }

        private FrameworkElement ExistDocumentInDocumentContainer(string name_)
        {
            foreach (FrameworkElement element in _mdi.Items)
            {
                if (name_.ToLower() == element.Name)
                {
                    return element;
                }
            }
            return null;
        }


        private void expedProduto_Click(object sender, RoutedEventArgs e)
        {
            /*
            ViewExpedicaoProduto expedicaoProduto = new();
            DocumentContainer.SetHeader((DependencyObject)expedicaoProduto, (object)"EXPEDIÇÃO PRODUTO SHOPPING");
            DocumentContainer.SetSizetoContentInMDI((DependencyObject)expedicaoProduto, true);
            DocumentContainer.SetMDIBounds((DependencyObject)expedicaoProduto, new Rect((this._mdi.ActualWidth - 1000.0) / 2.0, (this._mdi.ActualHeight - 700.0) / 2.0, 1000.0, 700.0));
            DocumentContainer.SetMDIWindowState((DependencyObject)expedicaoProduto, MDIWindowState.Maximized);
            this._mdi.CanMDIMaximize = true;
            this._mdi.Items.Add((object)expedicaoProduto);
            */
            adicionarFilho(new ViewExpedicaoProduto(), "EXPEDIÇÃO PRODUTO SHOPPING", "EXPEDICAO_PRODUTO_SHOPPING");
        }

        private void expedImprimirEtiqueta_Click(object sender, RoutedEventArgs e)
        {
            /*
            ViewExpedicaoImpressaoEtiqueta impressaoEtiqueta = new();
            DocumentContainer.SetHeader((DependencyObject)impressaoEtiqueta, (object)"EXPEDIÇÃO IMPRESSÃO DE ETIQUETA");
            DocumentContainer.SetSizetoContentInMDI((DependencyObject)impressaoEtiqueta, true);
            DocumentContainer.SetMDIBounds((DependencyObject)impressaoEtiqueta, new Rect((this._mdi.ActualWidth - 900.0) / 2.0, (this._mdi.ActualHeight - 600.0) / 2.0, 900.0, 600.0));
            DocumentContainer.SetMDIWindowState((DependencyObject)impressaoEtiqueta, MDIWindowState.Maximized);
            this._mdi.CanMDIMaximize = true;
            this._mdi.Items.Add((object)impressaoEtiqueta);
            */
            adicionarFilho(new ViewExpedicaoImpressaoEtiqueta(), "EXPEDIÇÃO IMPRESSÃO DE ETIQUETA", "EXPEDICAO_IMPRESSAO_ETIQUETA");
        }

        private void liberarImpressao_Click(object sender, RoutedEventArgs e)
        {
            /*
            ViewExpedicaoLiberarImpressao liberarImpressao = new ViewExpedicaoLiberarImpressao();
            DocumentContainer.SetHeader((DependencyObject)liberarImpressao, (object)"EXPEDIÇÃO LIBERAR IMPRESSÃO DE ETIQUETA");
            DocumentContainer.SetSizetoContentInMDI((DependencyObject)liberarImpressao, true);
            DocumentContainer.SetMDIBounds((DependencyObject)liberarImpressao, new Rect((this._mdi.ActualWidth - 900.0) / 2.0, (this._mdi.ActualHeight - 600.0) / 2.0, 900.0, 600.0));
            DocumentContainer.SetMDIWindowState((DependencyObject)liberarImpressao, MDIWindowState.Maximized);
            this._mdi.CanMDIMaximize = true;
            this._mdi.Items.Add((object)liberarImpressao);
            */
            adicionarFilho(new ViewExpedicaoLiberarImpressao(), "EXPEDIÇÃO LIBERAR IMPRESSÃO DE ETIQUETA", "EXPEDICAO_LIBERAR_IMPRESSAO_ETIQUETA");
        }

        private void expedNovoRomaneio_Click(object sender, RoutedEventArgs e)
        {
            /*
            ViewExpedicaoRomaneio expedicaoRomaneio = new ViewExpedicaoRomaneio();
            DocumentContainer.SetHeader((DependencyObject)expedicaoRomaneio, (object)"EXPEDIÇÃO ROMANEIO");
            DocumentContainer.SetSizetoContentInMDI((DependencyObject)expedicaoRomaneio, true);
            DocumentContainer.SetMDIWindowState((DependencyObject)expedicaoRomaneio, MDIWindowState.Normal);
            DocumentContainer.SetMDIBounds((DependencyObject)expedicaoRomaneio, new Rect((this._mdi.ActualWidth - 900.0) / 2.0, (this._mdi.ActualHeight - 530.0) / 2.0, 900.0, 530.0));
            DocumentContainer.SetMDIWindowState((DependencyObject)expedicaoRomaneio, MDIWindowState.Normal);
            this._mdi.CanMDIMaximize = false;
            this._mdi.Items.Add((object)expedicaoRomaneio);
            */
            adicionarFilho(new ViewExpedicaoRomaneio(), "EXPEDIÇÃO ROMANEIO", "EXPEDICAO_ROMANEIO");
        }

        private void expedTodosRomaneios_Click(object sender, RoutedEventArgs e)
        {
            /*
            ViewExpedicaoRomaneios expedicaoRomaneios = new ViewExpedicaoRomaneios("PRINCIPAL");
            DocumentContainer.SetHeader((DependencyObject)expedicaoRomaneios, (object)"EXPEDIÇÃO ROMANEIOS");
            DocumentContainer.SetSizetoContentInMDI((DependencyObject)expedicaoRomaneios, true);
            DocumentContainer.SetMDIBounds((DependencyObject)expedicaoRomaneios, new Rect((this._mdi.ActualWidth - 800.0) / 2.0, (this._mdi.ActualHeight - 600.0) / 2.0, 800.0, 600.0));
            DocumentContainer.SetMDIWindowState((DependencyObject)expedicaoRomaneios, MDIWindowState.Normal);
            this._mdi.CanMDIMaximize = false;
            this._mdi.Items.Add((object)expedicaoRomaneios);
            */
            adicionarFilho(new ViewExpedicaoRomaneios("PRINCIPAL"), "EXPEDIÇÃO ROMANEIOS", "EXPEDICAO_ROMANEIOS");
        }

        private void expedColetarDados_Click(object sender, RoutedEventArgs e)
        {
            /*
            ViewExpedicaoColetaDados expedicaoColetaDados = new ViewExpedicaoColetaDados();
            DocumentContainer.SetHeader((DependencyObject)expedicaoColetaDados, (object)"EXPEDIÇÃO COLETA DE DADOS");
            DocumentContainer.SetSizetoContentInMDI((DependencyObject)expedicaoColetaDados, true);
            DocumentContainer.SetMDIBounds((DependencyObject)expedicaoColetaDados, new Rect((this._mdi.ActualWidth - 800.0) / 2.0, (this._mdi.ActualHeight - 600.0) / 2.0, 800.0, 600.0));
            DocumentContainer.SetMDIWindowState((DependencyObject)expedicaoColetaDados, MDIWindowState.Maximized);
            this._mdi.CanMDIMaximize = true;
            this._mdi.Items.Add((object)expedicaoColetaDados);
            */
            adicionarFilho(new ViewExpedicaoColetaDados(), "EXPEDIÇÃO COLETA DE DADOS", "EXPEDICAO_COLETA_DADOS");
        }

        private void ItensFaltantes_Click(object sender, RoutedEventArgs e)
        {
            
            ViewExpedicaoExcel viewExpedicaoExcel = new ViewExpedicaoExcel("ITENS_FALTANTES");
            DocumentContainer.SetHeader((DependencyObject)viewExpedicaoExcel, (object)"EXPEDIÇÃO ITENS FALTANTES");
            DocumentContainer.SetSizetoContentInMDI((DependencyObject)viewExpedicaoExcel, true);
            DocumentContainer.SetMDIWindowState((DependencyObject)viewExpedicaoExcel, MDIWindowState.Normal);
            DocumentContainer.SetMDIBounds((DependencyObject)viewExpedicaoExcel, new Rect((this._mdi.ActualWidth - 600.0) / 2.0, (this._mdi.ActualHeight - 80.0) / 2.0, 600.0, 80.0));
            this._mdi.CanMDIMaximize = false;
            this._mdi.Items.Add((object)viewExpedicaoExcel);
            

            //adicionarFilho(new ViewExpedicaoExcel("ITENS_FALTANTES"), "EXPEDIÇÃO ITENS FALTANTES", "EXPEDICAO_ITENS_FALTANTES");
        }

        private void ItensCarregados_Click(object sender, RoutedEventArgs e)
        {
            /*
            ViewExpedicaoExcel viewExpedicaoExcel = new ViewExpedicaoExcel("ITENS_CARREGADOS");
            DocumentContainer.SetHeader((DependencyObject)viewExpedicaoExcel, (object)"EXPEDIÇÃO ITENS CARREGADOS");
            DocumentContainer.SetSizetoContentInMDI((DependencyObject)viewExpedicaoExcel, true);
            DocumentContainer.SetMDIWindowState((DependencyObject)viewExpedicaoExcel, MDIWindowState.Normal);
            DocumentContainer.SetMDIBounds((DependencyObject)viewExpedicaoExcel, new Rect((this._mdi.ActualWidth - 600.0) / 2.0, (this._mdi.ActualHeight - 80.0) / 2.0, 600.0, 80.0));
            this._mdi.CanMDIMaximize = false;
            this._mdi.Items.Add((object)viewExpedicaoExcel);
            */
            adicionarFilho(new ViewExpedicaoExcel("ITENS_CARREGADOS"), "EXPEDIÇÃO ITENS CARREGADOS", "EXPEDICAO_ITENS_CARREGADOS");
        }

        private async void OnExpedClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                ExcelEngine excelEngine = new();
                IApplication excel = excelEngine.Excel;
                excel.DefaultVersion = ExcelVersion.Xlsx;
                IWorkbook workbook = excel.Workbooks.Create(1);
                IWorksheet worksheet = workbook.Worksheets[0];

                using AppDatabase db = new();
                
                IList<QryExpedModel> dados = await db.QryExpeds.ToListAsync();
                ExcelImportDataOptions importDataOptions = new()
                {
                    FirstRow = 1,
                    FirstColumn = 1,
                    IncludeHeader = true,
                    PreserveTypes = true
                };
                worksheet.ImportData(dados, importDataOptions);
                worksheet.UsedRange.AutofitColumns();
                workbook.SaveAs(@$"{BaseSettings.CaminhoSistema}\Impressos\exped.xlsx");
                workbook.Close();
                excelEngine.Dispose();

                Process.Start(new ProcessStartInfo(@$"{BaseSettings.CaminhoSistema}\Impressos\exped.xlsx")
                {
                    UseShellExecute = true
                });
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }

        }

        private async void OnCaixasEnderecadasClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                ExcelEngine excelEngine = new();
                IApplication excel = excelEngine.Excel;
                excel.DefaultVersion = ExcelVersion.Xlsx;
                IWorkbook workbook = excel.Workbooks.Create(1);
                IWorksheet worksheet = workbook.Worksheets[0];

                using AppDatabase db = new();

                IList<CaixasEnderecadasModel> dados = await db.CaixasEnderecadas.ToListAsync();
                ExcelImportDataOptions importDataOptions = new()
                {
                    FirstRow = 1,
                    FirstColumn = 1,
                    IncludeHeader = true,
                    PreserveTypes = true
                };
                worksheet.ImportData(dados, importDataOptions);
                worksheet.UsedRange.AutofitColumns();
                workbook.SaveAs(@$"{BaseSettings.CaminhoSistema}\Impressos\caixas_enderecadas.xlsx");
                workbook.Close();
                excelEngine.Dispose();

                Process.Start(new ProcessStartInfo(@$"{BaseSettings.CaminhoSistema}\Impressos\caixas_enderecadas.xlsx")
                {
                    UseShellExecute = true
                });
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
        }

        private async void OnSaldoGeralShoppingClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                ExcelEngine excelEngine = new();
                IApplication excel = excelEngine.Excel;
                excel.DefaultVersion = ExcelVersion.Xlsx;
                IWorkbook workbook = excel.Workbooks.Create(1);
                IWorksheet worksheet = workbook.Worksheets[0];

                using AppDatabase db = new();
                IList<SaldoGeralShoppingModel> dados = await db.SaldoGeralShoppings.OrderBy(s => s.sigla).ToListAsync();
                ExcelImportDataOptions importDataOptions = new()
                {
                    FirstRow = 1,
                    FirstColumn = 1,
                    IncludeHeader = true,
                    PreserveTypes = true
                };

        

                
                IConditionalFormats ConditionalFormats1 = worksheet.Range[$"A2:K{dados.Count+1}"].ConditionalFormats;
                IConditionalFormat condition1 = ConditionalFormats1.AddCondition();
                condition1.FormatType = ExcelCFType.Formula;
                condition1.FirstFormula = "=$I2 >= 1";
                condition1.BackColorRGB = Color.FromArgb(83, 255, 161);
                
                
                IConditionalFormats ConditionalFormats2 = worksheet.Range[$"A2:K{dados.Count + 1}"].ConditionalFormats;
                IConditionalFormat condition2 = ConditionalFormats2.AddCondition();
                condition2.FormatType = ExcelCFType.Formula;
                condition2.FirstFormula = "=$I2 > 0,5";
                condition2.BackColorRGB = Color.FromArgb(29, 158, 255);
                

                
                IConditionalFormats ConditionalFormats3 = worksheet.Range[$"A2:K{dados.Count + 1}"].ConditionalFormats;
                IConditionalFormat condition3 = ConditionalFormats3.AddCondition();
                condition3.Operator = ExcelComparisonOperator.Equal;
                condition3.FormatType = ExcelCFType.Formula;
                condition3.FirstFormula = "=$I2 < 0,5";
                condition3.BackColorRGB = Color.FromArgb(255, 255, 255);

                //worksheet.Range["C:C"].NumberFormat = "_-* #.##0,00_-;-* #.##0,00_-;_-* "-"??_-;_-@_-";

                /*
                 * 
                 * Columns("C:C").Select
                   Selection.Style = "Comma"
                   Columns("D:D").Select
                   Selection.Style = "Comma"
                   Columns("E:E").Select
                   Selection.Style = "Comma"
                   Columns("F:F").Select
                   Selection.Style = "Comma"
                   Columns("G:G").Select
                   Selection.Style = "Comma"
                   Columns("I:I").Select
                   Selection.Style = "Percent"
                   Columns("J:J").Select
                   Selection.Style = "Comma" 
                 * */



                worksheet.ImportData(dados, importDataOptions);
                worksheet.UsedRange.AutofitColumns();
                workbook.SaveAs(@$"{BaseSettings.CaminhoSistema}\Impressos\saldo_geral_shopping.xlsx");
                workbook.Close();
                excelEngine.Dispose();

                Process.Start(new ProcessStartInfo(@$"{BaseSettings.CaminhoSistema}\Impressos\saldo_geral_shopping.xlsx")
                {
                    UseShellExecute = true
                });
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
        }

        private async void OnProdutosExpedidoDataClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                ExcelEngine excelEngine = new();
                IApplication excel = excelEngine.Excel;
                excel.DefaultVersion = ExcelVersion.Xlsx;
                IWorkbook workbook = excel.Workbooks.Create(1);
                IWorksheet worksheet = workbook.Worksheets[0];

                using AppDatabase db = new();

                IList<ProdutosBaiadosGeralTotalDataModel> dados = await db.produtosBaiadosData.ToListAsync();
                ExcelImportDataOptions importDataOptions = new()
                {
                    FirstRow = 1,
                    FirstColumn = 1,
                    IncludeHeader = true,
                    PreserveTypes = true
                };
                worksheet.ImportData(dados, importDataOptions);
                worksheet.UsedRange.AutofitColumns();
                workbook.SaveAs(@$"{BaseSettings.CaminhoSistema}\Impressos\produtos_expedidos_data.xlsx");
                workbook.Close();
                excelEngine.Dispose();

                Process.Start(new ProcessStartInfo(@$"{BaseSettings.CaminhoSistema}\Impressos\produtos_expedidos_data.xlsx")
                {
                    UseShellExecute = true
                });
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }

        }

        private async void OnCubagemDiaClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                ExcelEngine excelEngine = new();
                IApplication excel = excelEngine.Excel;
                excel.DefaultVersion = ExcelVersion.Xlsx;
                IWorkbook workbook = excel.Workbooks.Create(1);
                IWorksheet worksheet = workbook.Worksheets[0];

                using AppDatabase db = new();

                IList<CubagemDiaModel> dados = await db.CubagemDias.ToListAsync();
                ExcelImportDataOptions importDataOptions = new()
                {
                    FirstRow = 1,
                    FirstColumn = 1,
                    IncludeHeader = true,
                    PreserveTypes = true
                };
                worksheet.ImportData(dados, importDataOptions);
                worksheet.UsedRange.AutofitColumns();
                workbook.SaveAs(@$"{BaseSettings.CaminhoSistema}\Impressos\cubagem_dia.xlsx");
                workbook.Close();
                excelEngine.Dispose();

                Process.Start(new ProcessStartInfo(@$"{BaseSettings.CaminhoSistema}\Impressos\cubagem_dia.xlsx")
                {
                    UseShellExecute = true
                });
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
        }

        private async void OnCubagemSemanaAnosClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                ExcelEngine excelEngine = new();
                IApplication excel = excelEngine.Excel;
                excel.DefaultVersion = ExcelVersion.Xlsx;
                IWorkbook workbook = excel.Workbooks.Create(1);
                IWorksheet worksheet = workbook.Worksheets[0];

                using AppDatabase db = new();

                IList<CubagemSemanaAnoAnteriorAtualModel> dados = await db.CubagemSemanaAnos.ToListAsync();
                ExcelImportDataOptions importDataOptions = new()
                {
                    FirstRow = 1,
                    FirstColumn = 1,
                    IncludeHeader = true,
                    PreserveTypes = true
                };
                worksheet.ImportData(dados, importDataOptions);
                worksheet.UsedRange.AutofitColumns();
                workbook.SaveAs(@$"{BaseSettings.CaminhoSistema}\Impressos\cubagem_ano_atual_ano_anterior.xlsx");
                workbook.Close();
                excelEngine.Dispose();

                Process.Start(new ProcessStartInfo(@$"{BaseSettings.CaminhoSistema}\Impressos\cubagem_ano_atual_ano_anterior.xlsx")
                {
                    UseShellExecute = true
                });
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
        }

        private async void OnCubagemPrevistaClienteClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                ExcelEngine excelEngine = new();
                IApplication excel = excelEngine.Excel;
                excel.DefaultVersion = ExcelVersion.Xlsx;
                IWorkbook workbook = excel.Workbooks.Create(1);
                IWorksheet worksheet = workbook.Worksheets[0];

                using AppDatabase db = new();

                IList<CubagemPrevistaClienteModel> dados = await db.CubagemPrevistaClientes.ToListAsync();
                ExcelImportDataOptions importDataOptions = new()
                {
                    FirstRow = 1,
                    FirstColumn = 1,
                    IncludeHeader = true,
                    PreserveTypes = true
                };
                worksheet.ImportData(dados, importDataOptions);
                worksheet.UsedRange.AutofitColumns();
                workbook.SaveAs(@$"{BaseSettings.CaminhoSistema}\Impressos\cubagem_prevista_cliente.xlsx");
                workbook.Close();
                excelEngine.Dispose();

                Process.Start(new ProcessStartInfo(@$"{BaseSettings.CaminhoSistema}\Impressos\cubagem_prevista_cliente.xlsx")
                {
                    UseShellExecute = true
                });
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
        }

        private async void OnCubagemFracionadaCaminao(object sender, RoutedEventArgs e)
        {
            try
            {

                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });

                using AppDatabase db = new();
                IList<CubagemEnderecada> dados = await db.CubagemEnderecadas.ToListAsync();

                ExcelEngine excelEngine = new();
                IApplication excel = excelEngine.Excel;
                excel.DefaultVersion = ExcelVersion.Xlsx;
                IWorkbook workbook = excel.Workbooks.Create(2);

                IWorksheet worksheet = workbook.Worksheets[0];
                IWorksheet pivotSheet = workbook.Worksheets[1];

               
                ExcelImportDataOptions importDataOptions = new()
                {
                    FirstRow = 1,
                    FirstColumn = 1,
                    IncludeHeader = true,
                    PreserveTypes = true
                };
                worksheet.ImportData(dados, importDataOptions);
                
                IPivotCache cache = workbook.PivotCaches.Add(worksheet[$"A1:L1048576"]);
                IPivotTable pivotTable = pivotSheet.PivotTables.Add("PivotTable1", pivotSheet["A1"], cache);

                pivotTable.Fields[1].Axis = PivotAxisTypes.Page;
                pivotTable.Fields[9].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[9].Subtotals = PivotSubtotalTypes.None;
                pivotTable.Fields[4].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[4].Subtotals = PivotSubtotalTypes.None;
                pivotTable.Fields[5].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[5].Subtotals = PivotSubtotalTypes.None;
                pivotTable.Fields[3].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[3].Subtotals = PivotSubtotalTypes.None;
                pivotTable.Fields[10].Axis = PivotAxisTypes.Column;

                IPivotField pageField = pivotTable.Fields[1];
                pageField.Items[1].Visible = false;
                IPivotField field = pivotTable.Fields[7];
                pivotTable.DataFields.Add(field, "Soma das Cubagens", PivotSubtotalTypes.Sum).NumberFormat = @"#,###0.00";

                IPivotTableOptions options = pivotTable.Options;
                options.ShowFieldList = false;

                options.RowLayout = PivotTableRowLayout.Tabular;

                //Set classic layout
                ((PivotTableOptions)options).ShowGridDropZone = true;

                options.RowHeaderCaption = "Payment Dates";
                options.ColumnHeaderCaption = "Payments";

                pivotTable.ColumnGrand = false;
                pivotTable.RowGrand = true;

                pivotTable.DisplayFieldCaptions = true;

                workbook.Worksheets[1].Activate();
                workbook.SaveAs(@$"{BaseSettings.CaminhoSistema}\Impressos\cubagem_fracionada_caminhao.xlsx");
                workbook.Close();
                excelEngine.Dispose();

                Process.Start(new ProcessStartInfo(@$"{BaseSettings.CaminhoSistema}\Impressos\cubagem_fracionada_caminhao.xlsx")
                {
                    UseShellExecute = true
                });
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
        }

        private async void OnCubagemEfetivaClick(object sender, RoutedEventArgs e)
        {
            try
            {

                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });

                using AppDatabase db = new();
                IList<CubagemEnderecada> dados = await db.CubagemEnderecadas.ToListAsync();

                ExcelEngine excelEngine = new();
                IApplication excel = excelEngine.Excel;
                excel.DefaultVersion = ExcelVersion.Xlsx;
                IWorkbook workbook = excel.Workbooks.Create(2);

                IWorksheet worksheet = workbook.Worksheets[0];
                IWorksheet pivotSheet = workbook.Worksheets[1];


                ExcelImportDataOptions importDataOptions = new()
                {
                    FirstRow = 1,
                    FirstColumn = 1,
                    IncludeHeader = true,
                    PreserveTypes = true
                };
                worksheet.ImportData(dados, importDataOptions);

                IPivotCache cache = workbook.PivotCaches.Add(worksheet[$"A1:L1048576"]);
                IPivotTable pivotTable = pivotSheet.PivotTables.Add("PivotTable1", pivotSheet["A1"], cache);

                pivotTable.Fields[1].Axis = PivotAxisTypes.Page;
                pivotTable.Fields[9].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[9].Subtotals = PivotSubtotalTypes.None;
                pivotTable.Fields[4].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[4].Subtotals = PivotSubtotalTypes.None;
                pivotTable.Fields[5].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[5].Subtotals = PivotSubtotalTypes.None;
                pivotTable.Fields[3].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[3].Subtotals = PivotSubtotalTypes.None;
                pivotTable.Fields[10].Axis = PivotAxisTypes.Column;

                IPivotField pageField = pivotTable.Fields[1];
                pageField.Items[1].Visible = false;
                IPivotField field = pivotTable.Fields[7];
                pivotTable.DataFields.Add(field, "Soma das Cubagens", PivotSubtotalTypes.Sum).NumberFormat = @"#,###0.00";

                IPivotTableOptions options = pivotTable.Options;
                options.ShowFieldList = false;

                options.RowLayout = PivotTableRowLayout.Tabular;

                //Set classic layout
                ((PivotTableOptions)options).ShowGridDropZone = true;

                options.RowHeaderCaption = "Payment Dates";
                options.ColumnHeaderCaption = "Payments";

                pivotTable.ColumnGrand = false;
                pivotTable.RowGrand = true;

                pivotTable.DisplayFieldCaptions = true;

                workbook.Worksheets[1].Activate();
                workbook.SaveAs(@$"{BaseSettings.CaminhoSistema}\Impressos\cubagem_fracionada_caminhao.xlsx");
                workbook.Close();
                excelEngine.Dispose();

                Process.Start(new ProcessStartInfo(@$"{BaseSettings.CaminhoSistema}\Impressos\cubagem_fracionada_caminhao.xlsx")
                {
                    UseShellExecute = true
                });
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
        }

        private async void OnPendenciaExpedicaoClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                ExcelEngine excelEngine = new();
                IApplication excel = excelEngine.Excel;
                excel.DefaultVersion = ExcelVersion.Xlsx;
                IWorkbook workbook = excel.Workbooks.Create(1);
                IWorksheet worksheet = workbook.Worksheets[0];

                using AppDatabase db = new();

                IList<PendenciaExpedicaoModel> dados = await db.PendenciaExpedicaos.ToListAsync();
                ExcelImportDataOptions importDataOptions = new()
                {
                    FirstRow = 1,
                    FirstColumn = 1,
                    IncludeHeader = true,
                    PreserveTypes = true
                };
                worksheet.ImportData(dados, importDataOptions);
                worksheet.UsedRange.AutofitColumns();
                workbook.SaveAs(@$"{BaseSettings.CaminhoSistema}\Impressos\pendencias_expedicao.xlsx");
                workbook.Close();
                excelEngine.Dispose();

                Process.Start(new ProcessStartInfo(@$"{BaseSettings.CaminhoSistema}\Impressos\pendencias_expedicao.xlsx")
                {
                    UseShellExecute = true
                });
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
        }

        private async void OnExpedicaoVirtualClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                ExcelEngine excelEngine = new();
                IApplication excel = excelEngine.Excel;
                excel.DefaultVersion = ExcelVersion.Xlsx;
                IWorkbook workbook = excel.Workbooks.Create(1);
                IWorksheet worksheet = workbook.Worksheets[0];

                using AppDatabase db = new();

                IList<ControleVirtualModel> dados = await db.ControleVirtuals.ToListAsync();
                ExcelImportDataOptions importDataOptions = new()
                {
                    FirstRow = 1,
                    FirstColumn = 1,
                    IncludeHeader = true,
                    PreserveTypes = true
                };
                worksheet.ImportData(dados, importDataOptions);
                worksheet.UsedRange.AutofitColumns();
                workbook.SaveAs(@$"{BaseSettings.CaminhoSistema}\Impressos\expedicao_virtual.xlsx");
                workbook.Close();
                excelEngine.Dispose();

                Process.Start(new ProcessStartInfo(@$"{BaseSettings.CaminhoSistema}\Impressos\expedicao_virtual.xlsx")
                {
                    UseShellExecute = true
                });
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
        }

        private void OnCubagemZeradaClienteClick(object sender, RoutedEventArgs e)
        {
            /*
            ViewCubagemClienteZerada expedicaoRomaneio = new();
            DocumentContainer.SetHeader(expedicaoRomaneio, "CUBAGEM ZERADA CLIENTE");
            DocumentContainer.SetSizetoContentInMDI(expedicaoRomaneio, true);
            DocumentContainer.SetMDIBounds(expedicaoRomaneio, new Rect((this._mdi.ActualWidth - 1024.0) / 2.0, (this._mdi.ActualHeight - 780.0) / 2.0, 1024.0, 780.0));
            DocumentContainer.SetMDIWindowState(expedicaoRomaneio, MDIWindowState.Maximized);
            this._mdi.CanMDIMaximize = true;
            this._mdi.Items.Add(expedicaoRomaneio);
            */
            adicionarFilho(new ViewCubagemClienteZerada(), "CUBAGEM ZERADA CLIENTE", "CUBAGEM_ZERADA_CLIENTE");
        }

        private void OnPreItensFaltantesClick(object sender, RoutedEventArgs e)
        {
            /*
            ViewExpedicaoExcel viewExpedicaoExcel = new ViewExpedicaoExcel("PRE_ITENS_FALTANTES");
            DocumentContainer.SetHeader((DependencyObject)viewExpedicaoExcel, (object)"EXPEDIÇÃO PRÉ-CONFERENCIA ITENS FALTANTES");
            DocumentContainer.SetSizetoContentInMDI((DependencyObject)viewExpedicaoExcel, true);
            DocumentContainer.SetMDIWindowState((DependencyObject)viewExpedicaoExcel, MDIWindowState.Normal);
            DocumentContainer.SetMDIBounds((DependencyObject)viewExpedicaoExcel, new Rect((this._mdi.ActualWidth - 600.0) / 2.0, (this._mdi.ActualHeight - 80.0) / 2.0, 600.0, 80.0));
            this._mdi.CanMDIMaximize = false;
            this._mdi.Items.Add((object)viewExpedicaoExcel);
            */
            adicionarFilho(new ViewExpedicaoExcel("PRE_ITENS_FALTANTES"), "EXPEDIÇÃO PRÉ-CONFERENCIA ITENS FALTANTES", "EXPEDICAO_PRE_CONFERENCIA_ITENS_FALTANTES");
        }

        private void OnPreItensCarregadosClick(object sender, RoutedEventArgs e)
        {
            /*
            ViewExpedicaoExcel viewExpedicaoExcel = new ViewExpedicaoExcel("PRE_ITENS_CONFERIDOS");
            DocumentContainer.SetHeader((DependencyObject)viewExpedicaoExcel, (object)"EXPEDIÇÃO PRÉ-CONFERENCIA ITENS CONFERIDOS");
            DocumentContainer.SetSizetoContentInMDI((DependencyObject)viewExpedicaoExcel, true);
            DocumentContainer.SetMDIWindowState((DependencyObject)viewExpedicaoExcel, MDIWindowState.Normal);
            DocumentContainer.SetMDIBounds((DependencyObject)viewExpedicaoExcel, new Rect((this._mdi.ActualWidth - 600.0) / 2.0, (this._mdi.ActualHeight - 80.0) / 2.0, 600.0, 80.0));
            this._mdi.CanMDIMaximize = false;
            this._mdi.Items.Add((object)viewExpedicaoExcel);
            */
            adicionarFilho(new ViewExpedicaoExcel("PRE_ITENS_CONFERIDOS"), "EXPEDIÇÃO PRÉ-CONFERENCIA ITENS CONFERIDOS", "EXPEDICAO_PRE_CONFERENCIA_ITENS_CONFERIDOS");
        }

        private void expedNotaCaminhao(object sender, RoutedEventArgs e)
        {
            /*
            ViewSolicitarNotaCaminhao expedicaoRomaneio = new();
            DocumentContainer.SetHeader(expedicaoRomaneio, "SOLICITA NOTA FISCAL POR CAMINHÃO");
            DocumentContainer.SetSizetoContentInMDI(expedicaoRomaneio, true);
            DocumentContainer.SetMDIBounds(expedicaoRomaneio, new Rect((this._mdi.ActualWidth - 1024.0) / 2.0, (this._mdi.ActualHeight - 780.0) / 2.0, 1024.0, 780.0));
            DocumentContainer.SetMDIWindowState(expedicaoRomaneio, MDIWindowState.Maximized);
            this._mdi.CanMDIMaximize = false;
            this._mdi.Items.Add(expedicaoRomaneio);
            */
            adicionarFilho(new ViewSolicitarNotaCaminhao(), "SOLICITA NOTA FISCAL POR CAMINHÃO", "SOLICITA_NOTA_FISCAL_CAMINHAO");
        }

        private void expedNotaCliente_Click(object sender, RoutedEventArgs e)
        {
            adicionarFilho(new ViewSolicitarNotaCliente(), "SOLICITA NOTA FISCAL POR CLIENTE", "SOLICITA_NOTA_FISCAL_CLIENTE");
        }

        private void OnMdiCloseAllTabs(object sender, CloseTabEventArgs e)
        {
            _mdi.Items.Clear();
        }

        private void OnMdiCloseButtonClick(object sender, CloseButtonEventArgs e)
        {
            var tab = (DocumentContainer)sender;
            _mdi.Items.Remove(tab.ActiveDocument);
        }

        private void OnAlterarUsuario(object sender, MouseButtonEventArgs e)
        {
            Login window = new();
            window.ShowDialog();

            try
            {
                var appSettings = ConfigurationManager.GetSection("appSettings") as NameValueCollection;
                dB.Username = appSettings[0];
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
                        txtDataBase.Text = dB.Database;
                        _mdi.Items.Clear();
                    }
                }
            });
        }

        private async void OnProdutoCSVClick(object sender, RoutedEventArgs e)
        {
            //var dados = await Task.Run(async () => await new ExpedicaoViewModel().GetCarregamentoItemCaminhaosNaoExportadoMaticAsync(await Task.Run(async () => await new ViewModelLocal().GetRomaneios())));


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
                        //x.Key.CodComplAdicional,
                        //x.Key.DescricaoFiscal,
                        //x.Key.Unidade

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

                //using (var streamWriter = new StreamWriter(Path.Combine(caminhoArquivo, nomeArquivo)))
                using var streamWriter = new StreamWriter(@"C:\Temp\Produtos.csv");
                using var csvWriter = new CsvWriter(streamWriter, new CultureInfo("pt-BR", true));
                //csvWriter.Context.RegisterClassMap<DadosAnexoMap>();                                               
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
