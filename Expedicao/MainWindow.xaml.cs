using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Expedicao.Views;
using Microsoft.EntityFrameworkCore;
using Syncfusion.Data.Extensions;
using Syncfusion.SfSkinManager;
using Syncfusion.UI.Xaml.Spreadsheet;
using Syncfusion.Windows.Tools.Controls;
using Syncfusion.XlsIO;
using Syncfusion.XlsIO.Implementation.Collections;
using Syncfusion.XlsIO.Implementation.PivotTables;
using SizeMode = Syncfusion.SfSkinManager.SizeMode;

namespace Expedicao
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private DataBase dB = DataBase.Instance;

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

            dB.Database = DateTime.Now.Year.ToString();
            dB.Host = "192.168.0.23";
            dB.Username = Environment.UserName;
            dB.Password = "123mudar";
            txtUsername.Text = dB.Username;
            txtDataBase.Text = dB.Database;
        }
		/// <summary>
        /// Called when [loaded].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            CurrentVisualStyle = "Metro"; //"FluentLight";
	        CurrentSizeMode = "Default";
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

        private void expedProduto_Click(object sender, RoutedEventArgs e)
        {
            ViewExpedicaoProduto expedicaoProduto = new();
            DocumentContainer.SetHeader((DependencyObject)expedicaoProduto, (object)"EXPEDIÇÃO PRODUTO SHOPPING");
            DocumentContainer.SetSizetoContentInMDI((DependencyObject)expedicaoProduto, true);
            DocumentContainer.SetMDIBounds((DependencyObject)expedicaoProduto, new Rect((this._dc.ActualWidth - 1000.0) / 2.0, (this._dc.ActualHeight - 700.0) / 2.0, 1000.0, 700.0));
            DocumentContainer.SetMDIWindowState((DependencyObject)expedicaoProduto, MDIWindowState.Maximized);
            this._dc.CanMDIMaximize = true;
            this._dc.Items.Add((object)expedicaoProduto);
        }

        private void expedImprimirEtiqueta_Click(object sender, RoutedEventArgs e)
        {
            ViewExpedicaoImpressaoEtiqueta impressaoEtiqueta = new();
            DocumentContainer.SetHeader((DependencyObject)impressaoEtiqueta, (object)"EXPEDIÇÃO IMPRESSÃO DE ETIQUETA");
            DocumentContainer.SetSizetoContentInMDI((DependencyObject)impressaoEtiqueta, true);
            DocumentContainer.SetMDIBounds((DependencyObject)impressaoEtiqueta, new Rect((this._dc.ActualWidth - 900.0) / 2.0, (this._dc.ActualHeight - 600.0) / 2.0, 900.0, 600.0));
            DocumentContainer.SetMDIWindowState((DependencyObject)impressaoEtiqueta, MDIWindowState.Maximized);
            this._dc.CanMDIMaximize = true;
            this._dc.Items.Add((object)impressaoEtiqueta);
        }

        private void liberarImpressao_Click(object sender, RoutedEventArgs e)
        {
            ViewExpedicaoLiberarImpressao liberarImpressao = new ViewExpedicaoLiberarImpressao();
            DocumentContainer.SetHeader((DependencyObject)liberarImpressao, (object)"EXPEDIÇÃO LIBERAR IMPRESSÃO DE ETIQUETA");
            DocumentContainer.SetSizetoContentInMDI((DependencyObject)liberarImpressao, true);
            DocumentContainer.SetMDIBounds((DependencyObject)liberarImpressao, new Rect((this._dc.ActualWidth - 900.0) / 2.0, (this._dc.ActualHeight - 600.0) / 2.0, 900.0, 600.0));
            DocumentContainer.SetMDIWindowState((DependencyObject)liberarImpressao, MDIWindowState.Maximized);
            this._dc.CanMDIMaximize = true;
            this._dc.Items.Add((object)liberarImpressao);
        }

        private void expedNovoRomaneio_Click(object sender, RoutedEventArgs e)
        {
            ViewExpedicaoRomaneio expedicaoRomaneio = new ViewExpedicaoRomaneio();
            DocumentContainer.SetHeader((DependencyObject)expedicaoRomaneio, (object)"EXPEDIÇÃO ROMANEIO");
            DocumentContainer.SetSizetoContentInMDI((DependencyObject)expedicaoRomaneio, true);
            DocumentContainer.SetMDIWindowState((DependencyObject)expedicaoRomaneio, MDIWindowState.Normal);
            DocumentContainer.SetMDIBounds((DependencyObject)expedicaoRomaneio, new Rect((this._dc.ActualWidth - 900.0) / 2.0, (this._dc.ActualHeight - 530.0) / 2.0, 900.0, 530.0));
            DocumentContainer.SetMDIWindowState((DependencyObject)expedicaoRomaneio, MDIWindowState.Normal);
            this._dc.CanMDIMaximize = false;
            this._dc.Items.Add((object)expedicaoRomaneio);
        }

        private void expedTodosRomaneios_Click(object sender, RoutedEventArgs e)
        {
            ViewExpedicaoRomaneios expedicaoRomaneios = new ViewExpedicaoRomaneios("PRINCIPAL");
            DocumentContainer.SetHeader((DependencyObject)expedicaoRomaneios, (object)"EXPEDIÇÃO ROMANEIOS");
            DocumentContainer.SetSizetoContentInMDI((DependencyObject)expedicaoRomaneios, true);
            DocumentContainer.SetMDIBounds((DependencyObject)expedicaoRomaneios, new Rect((this._dc.ActualWidth - 800.0) / 2.0, (this._dc.ActualHeight - 600.0) / 2.0, 800.0, 600.0));
            DocumentContainer.SetMDIWindowState((DependencyObject)expedicaoRomaneios, MDIWindowState.Normal);
            this._dc.CanMDIMaximize = false;
            this._dc.Items.Add((object)expedicaoRomaneios);
        }

        private void expedColetarDados_Click(object sender, RoutedEventArgs e)
        {
            ViewExpedicaoColetaDados expedicaoColetaDados = new ViewExpedicaoColetaDados();
            DocumentContainer.SetHeader((DependencyObject)expedicaoColetaDados, (object)"EXPEDIÇÃO COLETA DE DADOS");
            DocumentContainer.SetSizetoContentInMDI((DependencyObject)expedicaoColetaDados, true);
            DocumentContainer.SetMDIBounds((DependencyObject)expedicaoColetaDados, new Rect((this._dc.ActualWidth - 800.0) / 2.0, (this._dc.ActualHeight - 600.0) / 2.0, 800.0, 600.0));
            DocumentContainer.SetMDIWindowState((DependencyObject)expedicaoColetaDados, MDIWindowState.Maximized);
            this._dc.CanMDIMaximize = true;
            this._dc.Items.Add((object)expedicaoColetaDados);
        }

        private void ItensFaltantes_Click(object sender, RoutedEventArgs e)
        {
            ViewExpedicaoExcel viewExpedicaoExcel = new ViewExpedicaoExcel("ITENS_FALTANTES");
            DocumentContainer.SetHeader((DependencyObject)viewExpedicaoExcel, (object)"EXPEDIÇÃO ITENS FALTANTES");
            DocumentContainer.SetSizetoContentInMDI((DependencyObject)viewExpedicaoExcel, true);
            DocumentContainer.SetMDIWindowState((DependencyObject)viewExpedicaoExcel, MDIWindowState.Normal);
            DocumentContainer.SetMDIBounds((DependencyObject)viewExpedicaoExcel, new Rect((this._dc.ActualWidth - 600.0) / 2.0, (this._dc.ActualHeight - 80.0) / 2.0, 600.0, 80.0));
            this._dc.CanMDIMaximize = false;
            this._dc.Items.Add((object)viewExpedicaoExcel);
        }

        private void ItensCarregados_Click(object sender, RoutedEventArgs e)
        {
            ViewExpedicaoExcel viewExpedicaoExcel = new ViewExpedicaoExcel("ITENS_CARREGADOS");
            DocumentContainer.SetHeader((DependencyObject)viewExpedicaoExcel, (object)"EXPEDIÇÃO ITENS CARREGADOS");
            DocumentContainer.SetSizetoContentInMDI((DependencyObject)viewExpedicaoExcel, true);
            DocumentContainer.SetMDIWindowState((DependencyObject)viewExpedicaoExcel, MDIWindowState.Normal);
            DocumentContainer.SetMDIBounds((DependencyObject)viewExpedicaoExcel, new Rect((this._dc.ActualWidth - 600.0) / 2.0, (this._dc.ActualHeight - 80.0) / 2.0, 600.0, 80.0));
            this._dc.CanMDIMaximize = false;
            this._dc.Items.Add((object)viewExpedicaoExcel);
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
                workbook.SaveAs(@"c:\relatorios\exped.xlsx");
                workbook.Close();
                excelEngine.Dispose();

                Process.Start(new ProcessStartInfo(@"c:\relatorios\exped.xlsx")
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
                workbook.SaveAs(@"c:\relatorios\caixas_enderecadas.xlsx");
                workbook.Close();
                excelEngine.Dispose();

                Process.Start(new ProcessStartInfo(@"c:\relatorios\caixas_enderecadas.xlsx")
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
                IList<SaldoGeralShoppingModel> dados = await db.SaldoGeralShoppings.ToListAsync();
                ExcelImportDataOptions importDataOptions = new()
                {
                    FirstRow = 1,
                    FirstColumn = 1,
                    IncludeHeader = true,
                    PreserveTypes = true
                };

        

                
                IConditionalFormats ConditionalFormats1 = worksheet.Range[$"A2:J{dados.Count+1}"].ConditionalFormats;
                IConditionalFormat condition1 = ConditionalFormats1.AddCondition();
                condition1.FormatType = ExcelCFType.Formula;
                condition1.FirstFormula = "=$I2 >= 1";
                condition1.BackColorRGB = Color.FromArgb(83, 255, 161);
                
                
                IConditionalFormats ConditionalFormats2 = worksheet.Range[$"A2:J{dados.Count + 1}"].ConditionalFormats;
                IConditionalFormat condition2 = ConditionalFormats2.AddCondition();
                condition2.FormatType = ExcelCFType.Formula;
                condition2.FirstFormula = "=$I2 > 0.5";
                condition2.BackColorRGB = Color.FromArgb(29, 158, 255);
                

                
                IConditionalFormats ConditionalFormats3 = worksheet.Range[$"A2:J{dados.Count + 1}"].ConditionalFormats;
                IConditionalFormat condition3 = ConditionalFormats3.AddCondition();
                condition3.Operator = ExcelComparisonOperator.Equal;
                condition3.FormatType = ExcelCFType.Formula;
                condition3.FirstFormula = "=$I2 < 0.5";
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
                workbook.SaveAs(@"c:\relatorios\saldo_geral_shopping.xlsx");
                workbook.Close();
                excelEngine.Dispose();

                Process.Start(new ProcessStartInfo(@"c:\relatorios\saldo_geral_shopping.xlsx")
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
                workbook.SaveAs(@"c:\relatorios\produtos_expedidos_data.xlsx");
                workbook.Close();
                excelEngine.Dispose();

                Process.Start(new ProcessStartInfo(@"c:\relatorios\produtos_expedidos_data.xlsx")
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
                workbook.SaveAs(@"c:\relatorios\cubagem_dia.xlsx");
                workbook.Close();
                excelEngine.Dispose();

                Process.Start(new ProcessStartInfo(@"c:\relatorios\cubagem_dia.xlsx")
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
                workbook.SaveAs(@"c:\relatorios\cubagem_ano_atual_ano_anterior.xlsx");
                workbook.Close();
                excelEngine.Dispose();

                Process.Start(new ProcessStartInfo(@"c:\relatorios\cubagem_ano_atual_ano_anterior.xlsx")
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
                workbook.SaveAs(@"c:\relatorios\cubagem_prevista_cliente.xlsx");
                workbook.Close();
                excelEngine.Dispose();

                Process.Start(new ProcessStartInfo(@"c:\relatorios\cubagem_prevista_cliente.xlsx")
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
                workbook.SaveAs(@"c:\relatorios\cubagem_fracionada_caminhao.xlsx");
                workbook.Close();
                excelEngine.Dispose();

                Process.Start(new ProcessStartInfo(@"c:\relatorios\cubagem_fracionada_caminhao.xlsx")
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
                workbook.SaveAs(@"c:\relatorios\cubagem_fracionada_caminhao.xlsx");
                workbook.Close();
                excelEngine.Dispose();

                Process.Start(new ProcessStartInfo(@"c:\relatorios\cubagem_fracionada_caminhao.xlsx")
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
                workbook.SaveAs(@"c:\relatorios\pendencias_expedicao.xlsx");
                workbook.Close();
                excelEngine.Dispose();

                Process.Start(new ProcessStartInfo(@"c:\relatorios\pendencias_expedicao.xlsx")
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
                workbook.SaveAs(@"c:\relatorios\expedicao_virtual.xlsx");
                workbook.Close();
                excelEngine.Dispose();

                Process.Start(new ProcessStartInfo(@"c:\relatorios\expedicao_virtual.xlsx")
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
            ViewCubagemClienteZerada expedicaoRomaneio = new();
            DocumentContainer.SetHeader(expedicaoRomaneio, "CUBAGEM ZERADA CLIENTE");
            DocumentContainer.SetSizetoContentInMDI(expedicaoRomaneio, true);
            DocumentContainer.SetMDIBounds(expedicaoRomaneio, new Rect((this._dc.ActualWidth - 1024.0) / 2.0, (this._dc.ActualHeight - 780.0) / 2.0, 1024.0, 780.0));
            DocumentContainer.SetMDIWindowState(expedicaoRomaneio, MDIWindowState.Maximized);
            this._dc.CanMDIMaximize = true;
            this._dc.Items.Add(expedicaoRomaneio);
        }

        private void OnPreItensFaltantesClick(object sender, RoutedEventArgs e)
        {
            ViewExpedicaoExcel viewExpedicaoExcel = new ViewExpedicaoExcel("PRE_ITENS_FALTANTES");
            DocumentContainer.SetHeader((DependencyObject)viewExpedicaoExcel, (object)"EXPEDIÇÃO PRÉ-CONFERENCIA ITENS FALTANTES");
            DocumentContainer.SetSizetoContentInMDI((DependencyObject)viewExpedicaoExcel, true);
            DocumentContainer.SetMDIWindowState((DependencyObject)viewExpedicaoExcel, MDIWindowState.Normal);
            DocumentContainer.SetMDIBounds((DependencyObject)viewExpedicaoExcel, new Rect((this._dc.ActualWidth - 600.0) / 2.0, (this._dc.ActualHeight - 80.0) / 2.0, 600.0, 80.0));
            this._dc.CanMDIMaximize = false;
            this._dc.Items.Add((object)viewExpedicaoExcel);
        }

        private void OnPreItensCarregadosClick(object sender, RoutedEventArgs e)
        {
            ViewExpedicaoExcel viewExpedicaoExcel = new ViewExpedicaoExcel("PRE_ITENS_CONFERIDOS");
            DocumentContainer.SetHeader((DependencyObject)viewExpedicaoExcel, (object)"EXPEDIÇÃO PRÉ-CONFERENCIA ITENS CONFERIDOS");
            DocumentContainer.SetSizetoContentInMDI((DependencyObject)viewExpedicaoExcel, true);
            DocumentContainer.SetMDIWindowState((DependencyObject)viewExpedicaoExcel, MDIWindowState.Normal);
            DocumentContainer.SetMDIBounds((DependencyObject)viewExpedicaoExcel, new Rect((this._dc.ActualWidth - 600.0) / 2.0, (this._dc.ActualHeight - 80.0) / 2.0, 600.0, 80.0));
            this._dc.CanMDIMaximize = false;
            this._dc.Items.Add((object)viewExpedicaoExcel);
        }

        private void expedNotaCaminhao(object sender, RoutedEventArgs e)
        {
            ViewSolicitarNotaCaminhao expedicaoRomaneio = new();
            DocumentContainer.SetHeader(expedicaoRomaneio, "SOLICITA NOTA FISCAL POR CAMINHÃO");
            DocumentContainer.SetSizetoContentInMDI(expedicaoRomaneio, true);
            DocumentContainer.SetMDIBounds(expedicaoRomaneio, new Rect((this._dc.ActualWidth - 1024.0) / 2.0, (this._dc.ActualHeight - 780.0) / 2.0, 1024.0, 780.0));
            DocumentContainer.SetMDIWindowState(expedicaoRomaneio, MDIWindowState.Maximized);
            this._dc.CanMDIMaximize = false;
            this._dc.Items.Add(expedicaoRomaneio);
        }
    }
}
