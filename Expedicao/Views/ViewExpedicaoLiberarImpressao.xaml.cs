using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Expedicao.Views
{
    /// <summary>
    /// Interação lógica para ViewExpedicaoLiberarImpressao.xam
    /// </summary>
    public partial class ViewExpedicaoLiberarImpressao : UserControl
    {
        public ViewExpedicaoLiberarImpressao()
        {
            InitializeComponent();
        }

        private async void UserControl_Initialized(object sender, EventArgs e)
        {
            try
            {
                itens.ItemsSource = (object)await Task.Run(async () => await new ExpedicaoViewModel().GetLiberarImpressaosAsync());
                loadingDetalhes.Visibility = Visibility.Hidden;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void itens_CurrentCellValueChanged(object sender, CurrentCellValueChangedEventArgs e)
        {
            try
            {
                LiberarImpressaoModel? liberarImpressao = e.Record as LiberarImpressaoModel;
                CaixaModel caixaModel = await Task.Run(async () => await new ExpedicaoViewModel().LiberarImpresaoAsync(liberarImpressao));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            this.itens.UpdateDataRow(e.RowColumnIndex.RowIndex);
        }
    }
}
