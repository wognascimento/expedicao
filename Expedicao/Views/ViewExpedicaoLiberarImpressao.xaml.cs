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
                itens.ItemsSource = await new ExpedicaoViewModel().GetLiberarImpressaosAsync();
                loadingDetalhes.IsBusy = false;
            }
            catch (Exception ex)
            {
                loadingDetalhes.IsBusy = false;
                MessageBox.Show(ex.Message);
            }
        }

        private async void ImpressoCheckBox_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is not CheckBox { DataContext: LiberarImpressaoModel liberarImpressao })
                    return;

                await new ExpedicaoViewModel().LiberarImpresaoAsync(liberarImpressao);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
