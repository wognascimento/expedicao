using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Telerik.Windows.Controls;

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
                itens.ItemsSource = await Task.Run(async () => await new ExpedicaoViewModel().GetLiberarImpressaosAsync());
                loadingDetalhes.IsBusy = false;
            }
            catch (Exception ex)
            {
                loadingDetalhes.IsBusy = false;
                MessageBox.Show(ex.Message);
            }
        }

        private async void itens_CellEditEnded(object sender, GridViewCellEditEndedEventArgs e)
        {
            try
            {
                if (e.Cell?.Column is not GridViewCheckBoxColumn || e.Cell.DataContext is not LiberarImpressaoModel liberarImpressao)
                    return;

                await Task.Run(async () => await new ExpedicaoViewModel().LiberarImpresaoAsync(liberarImpressao));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
