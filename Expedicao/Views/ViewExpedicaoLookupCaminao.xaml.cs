using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        private readonly List<string> _siglas;

        public List<CarregamentoItenFaltanteModel> ItensFaltantes { get; } = [];

        public ViewExpedicaoLookupCaminao()
            : this([])
        {
        }

        public ViewExpedicaoLookupCaminao(List<string> siglas)
        {
            _siglas = siglas;
            InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                itens.ItemsSource = await new ExpedicaoViewModel().GetCaminhoesAsync(_siglas);
                loadingDetalhes.IsBusy = false;
                loadingDetalhes.Visibility = Visibility.Hidden;
                loadingBtn.Visibility = Visibility.Visible;
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
                loadingDetalhes.IsBusy = true;
                loadingBtn.Visibility = Visibility.Hidden;

                ItensFaltantes.Clear();
                foreach (var item in ((IEnumerable)itens.ItemsSource).Cast<CaminaoModel>())
                {
                    if ((bool)item.selecao)
                    {
                        var faltantes = await new ExpedicaoViewModel().GetItensFaltanteAsync(item.sigla, item.caminao);
                        ItensFaltantes.AddRange(faltantes);
                    }
                }

                loadingDetalhes.IsBusy = false;
                loadingDetalhes.Visibility = Visibility.Hidden;
                loadingBtn.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                loadingDetalhes.IsBusy = false;
                MessageBox.Show(ex.Message);
            }
        }
    }
}
