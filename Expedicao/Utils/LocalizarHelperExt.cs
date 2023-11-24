using Syncfusion.UI.Xaml.Grid;

namespace Expedicao
{
    public class LocalizarHelperExt : SearchHelper
    {
        public LocalizarHelperExt(SfDataGrid datagrid) : base(datagrid) { }
        protected override bool SearchCell(DataColumnBase column, object record, bool ApplySearchHighlightBrush)
        {
            if (column == null)
                return false;
            if (column.GridColumn.MappingName == "CodDetalhesCompl")
            {
                return base.SearchCell(column, record, ApplySearchHighlightBrush);
            }
            else
                return false;
        }
        protected override bool FilterRecords(object dataRow)
        {
            if (string.IsNullOrEmpty(SearchText))
                return true;

            if (this.Provider == null)
                Provider = this.DataGrid.View.GetPropertyAccessProvider();


            //Passing columnname instead of column to avoid looping datagrid.Columns for improved performance.
            if (MatchSearchTextOpimized("CodDetalhesCompl", dataRow))
                return true;

            return false;
        }
        protected virtual bool MatchSearchTextOpimized(string mappingName, object record)
        {
            if (this.DataGrid.View == null || string.IsNullOrEmpty(SearchText))
                return false;

            var cellvalue = Provider.GetFormattedValue(record, mappingName);
            if (this.SearchType == SearchType.Contains)
            {
                if (!AllowCaseSensitiveSearch)
                    return cellvalue != null && cellvalue.ToString().ToLower().Contains(SearchText.ToString().ToLower());
                else
                    return cellvalue != null && cellvalue.ToString().Contains(SearchText.ToString());
            }
            else if (this.SearchType == SearchType.StartsWith)
            {
                if (!AllowCaseSensitiveSearch)
                    return cellvalue != null && cellvalue.ToString().ToLower().StartsWith(SearchText.ToString().ToLower());
                else
                    return cellvalue != null && cellvalue.ToString().StartsWith(SearchText.ToString());
            }
            else
            {
                if (!AllowCaseSensitiveSearch)
                    return cellvalue != null && cellvalue.ToString().ToLower().EndsWith(SearchText.ToString().ToLower());
                else
                    return cellvalue != null && cellvalue.ToString().EndsWith(SearchText.ToString());
            }
        }

    }
}
