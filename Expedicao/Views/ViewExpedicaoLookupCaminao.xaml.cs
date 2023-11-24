using Expedicao.DataBaseLocal;
using Syncfusion.UI.Xaml.Grid;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Expedicao.Views
{
    /// <summary>
    /// Interação lógica para ViewExpedicaoLookupCaminao.xam
    /// </summary>
    public partial class ViewExpedicaoLookupCaminao : UserControl
    {
        public ViewExpedicaoLookupCaminao()
        {
            InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                itens.ItemsSource = await Task.Run(async () => await new ExpedicaoViewModel().GetCaminhoesAsync(await Task.Run(async () => await new ViewModelLocal().GetRomaneios())));
                this.loadingDetalhes.Visibility = Visibility.Hidden;
                this.loadingBtn.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            

        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                loadingDetalhes.Visibility = Visibility.Visible;
                loadingBtn.Visibility = Visibility.Hidden;
                await Task.Run(async () => await Task.Run(async () => await new ViewModelLocal().GetRemoveAllItemFaltante()));
                foreach (CaminaoModel item in this.itens.View.SourceCollection)
                {
                    if ((bool)item.selecao)
                    {
                        foreach (CarregamentoItenFaltanteModel itenFaltanteModel in (await Task.Run(async () => await new ExpedicaoViewModel().GetItensFaltanteAsync(item.sigla, item.caminao))))
                        {
                            await Task.Run(async () => await Task.Run(async () => await new ViewModelLocal().GetAddItenFaltante(itenFaltanteModel)));
                        }
                    }
                }
                this.loadingDetalhes.Visibility = Visibility.Hidden;
                this.loadingBtn.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }
    }
}
