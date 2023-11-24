using Syncfusion.UI.Xaml.Grid;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace Expedicao
{
    /// <summary>
    /// class which helps to Search the Text in the DataGrid.
    /// </summary>
    [TemplatePart(Name = "PART_FindNext", Type = typeof(Button))]
    [TemplatePart(Name = "PART_FindPrevious", Type = typeof(Button))]
    [TemplatePart(Name = "PART_Close", Type = typeof(Button))]
    [TemplatePart(Name = "PART_AdornerLayer", Type = typeof(AdornerDecorator))]
    public class SearchControl : Control, IDisposable
    {
        #region Fields
        internal Button? FindNextButton;
        internal Button? FindPreviousButton;
        internal Button? CloseButton;
        internal TextBox? SearchTextBox;
        internal AdornerDecorator? AdornerLayer;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the DataGrid for the corresponding search operation.
        /// </summary>
        public SfDataGrid DataGrid
        {
            get { return (SfDataGrid)GetValue(DataGridProperty); }
            set { SetValue(DataGridProperty, value); }
        }
        public static readonly DependencyProperty DataGridProperty = DependencyProperty.Register("DataGrid", typeof(SfDataGrid), typeof(SearchControl), new PropertyMetadata(null));
        #endregion

        #region Ctor
        static SearchControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SearchControl), new FrameworkPropertyMetadata(typeof(SearchControl)));
        }
        public SearchControl()
        {
        }
        public SearchControl(SfDataGrid datagrid)
        {
            DataGrid = datagrid;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Method to open Search Control.
        /// </summary>
        /// <param name="visible"></param>
        internal void UpdateSearchControlVisiblity(bool visible)
        {
            if (visible)
            {
                Visibility = Visibility.Visible;
                //SearchTextBox.Focus();
            }
            else
            {
                Visibility = Visibility.Hidden;
                //SearchTextBox.Clear();
                DataGrid.SearchHelper.ClearSearch();
                DataGrid.Focus();
            }
        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            FindNextButton = GetTemplateChild("PART_FindNext") as Button;
            FindPreviousButton = GetTemplateChild("PART_FindPrevious") as Button;
            CloseButton = GetTemplateChild("PART_Close") as Button;
            SearchTextBox = GetTemplateChild("PART_TextBox") as TextBox;
            AdornerLayer = GetTemplateChild("PART_AdornerLayer") as AdornerDecorator;
            SearchTextBox.Focus();
            WireEvents();
        }
        #endregion

        #region Events

        /// <summary>
        /// Method to wire the required events.
        /// </summary>
        private void WireEvents()
        {
            FindNextButton.Click += OnFindNextButtonClick;
            FindPreviousButton.Click += OnFindPreviousButtonClick;
            CloseButton.Click += OnCloseButtonClick;
            SearchTextBox.TextChanged += OnTextChanged;
            AdornerLayer.KeyDown += OnAdornerLayerKeyDown;
        }

        /// <summary>
        /// Event handler to handle AdornerLayer key down.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnAdornerLayerKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if ((e.Key == Key.F) && (e.KeyboardDevice.Modifiers & ModifierKeys.Control) != ModifierKeys.None)
                this.UpdateSearchControlVisiblity(true);
            else if (e.Key == Key.Escape)
                this.UpdateSearchControlVisiblity(false);
        }

        /// <summary>
        /// Event handler to handle when text value is changed in SearchTextBox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            DataGrid.SearchHelper = new SearchHelperExt(DataGrid);
            DataGrid.SearchHelper.Search(SearchTextBox.Text);
        }

        /// <summary>
        ///  Event handler to handle when clicking on FindNext button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFindNextButtonClick(object sender, RoutedEventArgs e)
        {
            DataGrid.SearchHelper.FindNext(SearchTextBox.Text);
            SetSelectedItem();
        }

        /// <summary>
        /// Based on searched text, the row will be selected.
        /// </summary>
        private void SetSelectedItem()
        {
            var rowIndex = DataGrid.SearchHelper.CurrentRowColumnIndex.RowIndex;
            var recordIndex = DataGrid.ResolveToRecordIndex(rowIndex);
            DataGrid.SelectedIndex = recordIndex;
        }

        /// <summary>
        /// Event handler to handle when clicking on FindPrevious button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFindPreviousButtonClick(object sender, RoutedEventArgs e)
        {
            DataGrid.SearchHelper.FindPrevious(SearchTextBox.Text);
            SetSelectedItem();
        }

        /// <summary>
        /// Event handler to handle when clicking on Close button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCloseButtonClick(object sender, RoutedEventArgs e)
        {
            this.SearchTextBox.Clear();
            DataGrid.SearchHelper.ClearSearch();
            this.Visibility = Visibility.Collapsed;
            this.DataGrid.Focus();
        }
        #endregion

        /// <summary>
        /// Method to UnWire the wired events.
        /// </summary>
        private void UnWireEvents()
        {
            FindNextButton.Click -= OnFindNextButtonClick;
            FindPreviousButton.Click -= OnFindPreviousButtonClick;
            CloseButton.Click -= OnCloseButtonClick;
            SearchTextBox.TextChanged -= OnTextChanged;
        }

        public void Dispose()
        {
            this.UnWireEvents();
            this.DataGrid = null;
        }
    }

    public class SearchHelperExt : SearchHelper
    {
        public SearchHelperExt(SfDataGrid datagrid) : base(datagrid) {}

        protected override bool SearchCell(DataColumnBase column, object record, bool ApplySearchHighlightBrush)
        {
            var colIndex = DataGrid.SelectionController.CurrentCellManager.CurrentCell.ColumnIndex;
            var mapName = DataGrid.Columns[colIndex].MappingName;
            if (column.GridColumn.MappingName == mapName)
                return base.SearchCell(column, record, ApplySearchHighlightBrush);
            return false;
        }
    }
}
