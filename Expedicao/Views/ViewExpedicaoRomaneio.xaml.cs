using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Expedicao.Views
{
    /// <summary>
    /// Interação lógica para ViewExpedicaoRomaneio.xam
    /// </summary>
    public partial class ViewExpedicaoRomaneio : UserControl
    {

        public ViewExpedicaoRomaneio()
        {
            this.DataContext = new RomaneioViewModel();
            InitializeComponent();
        }

        public ViewExpedicaoRomaneio(RomaneioModel romaneio)
        {
            this.DataContext = new RomaneioViewModel();
            this.InitializeComponent();

            RomaneioViewModel vm = (RomaneioViewModel)DataContext;
            vm.Romaneio = romaneio;
            //this.DataContext = this;
        }

        private async void UserControl_Initialized(object sender, EventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });

                RomaneioViewModel vm = (RomaneioViewModel)DataContext;
                vm.Aprovados = await Task.Run(vm.GetAprovados);
                vm.Tranportadoras = await Task.Run(vm.GetTransportadoras);
    
                if (vm.Romaneio == null)
                {
                    Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                    return;
                }
                    

                IList<AprovadoModel> itemsSource1 = (IList<AprovadoModel>)shopping_destino.ItemsSource;
                int num1 = itemsSource1.IndexOf(itemsSource1.Where(a => a.SiglaServ.Equals(vm.Romaneio.ShoppingDestino)).FirstOrDefault());

                IList<TranportadoraModel> itemsSource2 = (IList<TranportadoraModel>)codtransportadora.ItemsSource;
                int num2 = itemsSource2.IndexOf(itemsSource2.Where(t => t.CodTransportadora.Equals(vm.Romaneio.CodTransportadora)).FirstOrDefault());

                operacao.ItemsSource = vm.OperacoesList;
                condicao_caminhao.ItemsSource = vm.CondicaoCaminhaoList;
                operacao.SelectedIndex = vm.OperacoesList.FindIndex(o => o.Equals(vm.Romaneio.Operacao));
                cod_romaneiro.Value = vm.Romaneio.CodRomaneiro;
                data_carregamento.DateTime = new DateTime?((DateTime)vm.Romaneio.DataCarregamento);
                hora_chegada.Value = vm.Romaneio.HoraChegada.ToString();
                shopping_destino.SelectedIndex = num1;
                numero_caminhao.Value = new long?((long)vm.Romaneio.NumeroCaminhao);
                local_carregamento.Text = vm.Romaneio.LocalCarregamento;
                codtransportadora.SelectedIndex = num2;
                nome_motorista.Text = vm.Romaneio.NomeMotorista;
                numero_cnh.Text = vm.Romaneio.NumeroCnh;
                telefone_motorista.Text = vm.Romaneio.TelefoneMotorista;
                condicao_caminhao.SelectedIndex = vm.CondicaoCaminhaoList.FindIndex(c => c.Equals(vm.Romaneio.CondicaoCaminhao));
                placa_caminhao.Text = vm.Romaneio.PlacaCaminhao;
                placa_cidade.Text = vm.Romaneio.PlacaCidade;
                placa_estado.Text = vm.Romaneio.PlacaEstado;
                placa_carroceria.Text = vm.Romaneio.PlacaCarroceria;
                placa_carroceria_cidade.Text = vm.Romaneio.PlacaCarroceriaCidade;
                placa_carroceria_estado.Text = vm.Romaneio.PlacaCarroceriaEstado;
                bau_altura.Value = new double?((double)vm.Romaneio.BauAltura);
                bau_largura.Value = new double?((double)vm.Romaneio.BauLargura);
                bau_profundidade.Value = new double?((double)vm.Romaneio.BauProfundidade);
                m3_carregado.Value = new double?((double)vm.Romaneio?.M3Carregado);
                bau_soba.Value = new double?((double)vm.Romaneio?.BauSoba);
                m3_portaria.Value = new double?((double)vm.Romaneio?.M3Portaria);
                nome_conferente.Text = vm.Romaneio.NomeConferente;
                num_lacres.Text = vm.Romaneio.NumLacres;
                numero_container.Text = vm.Romaneio.NumeroContainer;
                dateSaida.DateTime = new DateTime?((DateTime)vm.Romaneio.DataHoraLiberacao);

                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show(ex.Message);
            }
        }

        private void nome_motorista_SelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }

        private void placa_caminhao_SelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }

        private void placa_carroceria_SelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }

        private void btnNovo_Click(object sender, RoutedEventArgs e)
        {
            Limpar();
        }

        private async void btnGravar_Click(object sender, RoutedEventArgs e)
        {
            if (!this.Validar())
                return;
            try
            {
                RomaneioViewModel vm = (RomaneioViewModel)DataContext;
                //if (this.Romaneio == null)
                //{
                foreach (AprovadoModel selectedItem in shopping_destino.SelectedItems)
                {
                        RomaneioModel Romaneio = new RomaneioModel();
                        Romaneio.CodRomaneiro = vm.Romaneio?.CodRomaneiro;
                        Romaneio.Operacao = operacao.SelectionBoxItem.ToString();
                        Romaneio.DataCarregamento = data_carregamento.DateTime.Value;
                        Romaneio.HoraChegada = TimeSpan.Parse(hora_chegada.Value.ToString());
                        Romaneio.ShoppingDestino = selectedItem.SiglaServ;
                        Romaneio.NumeroCaminhao = numero_caminhao.Value.Value;
                        Romaneio.LocalCarregamento = local_carregamento.Text;
                        Romaneio.CodTransportadora = (codtransportadora.SelectedItem as TranportadoraModel).CodTransportadora;
                        Romaneio.NomeMotorista = nome_motorista.Text;
                        Romaneio.NumeroCnh = numero_cnh.Text;
                        Romaneio.TelefoneMotorista = telefone_motorista.Text;
                        Romaneio.CondicaoCaminhao = condicao_caminhao.SelectionBoxItem.ToString();
                        Romaneio.PlacaCaminhao = placa_caminhao.Text;
                        Romaneio.PlacaCidade = placa_cidade.Text;
                        Romaneio.PlacaEstado = placa_estado.Text;
                        Romaneio.PlacaCarroceria = placa_carroceria.Text;
                        Romaneio.PlacaCarroceriaCidade = placa_carroceria_cidade.Text;
                        Romaneio.PlacaCarroceriaEstado = placa_carroceria_estado.Text;
                        Romaneio.BauAltura = bau_altura?.Value;
                        Romaneio.BauLargura = bau_largura?.Value;
                        Romaneio.BauProfundidade = bau_profundidade?.Value;
                        Romaneio.M3Carregado = m3_carregado?.Value;
                        Romaneio.BauSoba = bau_soba?.Value;
                        Romaneio.M3Portaria = m3_portaria?.Value;
                        Romaneio.NomeConferente = nome_conferente.Text;
                        Romaneio.NumLacres = num_lacres.Text;
                        Romaneio.NumeroContainer = numero_container.Text;
                        Romaneio.DataHoraLiberacao = dateSaida.DateTime.Value;
                        RomaneioModel romaneioModel = await vm.SaveAsync(Romaneio);
                }
                //}
                MessageBox.Show("Romaneio salvo com sucesso...", "Romaneio", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                Limpar();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Limpar()
        {
            this.operacao.SelectedValue = null;
            this.operacao.IsDropDownOpen = true;
            this.cod_romaneiro.Text = "0";
            this.data_carregamento.DateTime = new DateTime?();
            this.hora_chegada.Value = null;
            this.shopping_destino.SelectedValue = null;
            this.numero_caminhao.Text = null;
            ((TextBox)this.local_carregamento).Text = "JACAREÍ";
            this.codtransportadora.SelectedValue = null;
            ((TextBox)this.nome_motorista).Text = null;
            ((TextBox)this.numero_cnh).Text = null;
            ((TextBox)this.telefone_motorista).Text = null;
            this.condicao_caminhao.SelectedValue = null;
            ((TextBox)this.placa_caminhao).Text = null;
            ((TextBox)this.placa_cidade).Text = null;
            ((TextBox)this.placa_estado).Text = null;
            ((TextBox)this.placa_carroceria).Text = null;
            ((TextBox)this.placa_carroceria_cidade).Text = null;
            ((TextBox)this.placa_carroceria_estado).Text = null;
            this.bau_altura.Value = new double?();
            this.bau_largura.Value = new double?();
            this.bau_profundidade.Value = new double?();
            this.m3_carregado.Value = new double?();
            this.bau_soba.Value = new double?();
            this.m3_portaria.Value = new double?();
            ((TextBox)this.nome_conferente).Text = null;
            ((TextBox)this.num_lacres).Text = null;
            ((TextBox)this.numero_container).Text = null;
            this.operacao.Focusable = true;
            this.operacao.Focus();
        }

        private bool Validar()
        {
            if (operacao.SelectedValue == null)
            {
                MessageBox.Show("Operação é obrigatório", "Informação requerida", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                operacao.Focus();
                operacao.IsDropDownOpen = true;
                return false;
            }
            if (!data_carregamento.DateTime.HasValue)
            {
                MessageBox.Show("Data de Carregamento é obrigatório", "Informação requerida", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                data_carregamento.Focus();
                data_carregamento.IsDropDownOpen = true;
                return false;
            }
            if (hora_chegada.Text == "")
            {
                MessageBox.Show("Hora do Carregamento é obrigatório", "Informação requerida", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                hora_chegada.Focus();
                return false;
            }
            if (shopping_destino.SelectedValue == null)
            {
                MessageBox.Show("Shopping é obrigatório", "Informação requerida", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                shopping_destino.Focus();
                shopping_destino.IsDropDownOpen = true;
                return false;
            }
            if (numero_caminhao.Text == "")
            {
                MessageBox.Show("Nº de Caminhão é obrigatório", "Informação requerida", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                numero_caminhao.Focus();
                return false;
            }
            if (local_carregamento.Text == "")
            {
                MessageBox.Show("Local de Carregamento é obrigatório", "Informação requerida", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                local_carregamento.Focus();
                return false;
            }
            if (codtransportadora.SelectedValue == null)
            {
                MessageBox.Show("Transportadora é obrigatório", "Informação requerida", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                codtransportadora.Focus();
                codtransportadora.IsDropDownOpen = true;
                return false;
            }
            if (nome_motorista.Text == "")
            {
                MessageBox.Show("Motorista é obrigatório", "Informação requerida", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                nome_motorista.Focus();
                return false;
            }
            if (numero_cnh.Text == "")
            {
                MessageBox.Show("Nº da CNH é obrigatório", "Informação requerida", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                numero_cnh.Focus();
                return false;
            }
            if (telefone_motorista.Text == "")
            {
                MessageBox.Show("Telefone é obrigatório", "Informação requerida", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                telefone_motorista.Focus();
                return false;
            }
            if (condicao_caminhao.SelectedValue == null)
            {
                MessageBox.Show("Condição do Caminhão é obrigatório", "Informação requerida", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                condicao_caminhao.Focus();
                condicao_caminhao.IsDropDownOpen = true;
                return false;
            }
            if (placa_caminhao.Text == "")
            {
                MessageBox.Show("Placa Caminhão/Cavalo é obrigatório", "Informação requerida", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                placa_caminhao.Focus();
                return false;
            }
            if (placa_cidade.Text == "")
            {
                MessageBox.Show("Cidade é obrigatório", "Informação requerida", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                placa_cidade.Focus();
                return false;
            }
            if (placa_estado.Text == "")
            {
                MessageBox.Show("Estado é obrigatório", "Informação requerida", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                placa_estado.Focus();
                return false;
            }
            if (bau_altura.Text == "")
            {
                MessageBox.Show("Altura é obrigatório", "Informação requerida", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                bau_altura.Focus();
                return false;
            }
            if (bau_largura.Text == "")
            {
                MessageBox.Show("Largura é obrigatório", "Informação requerida", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                bau_largura.Focus();
                return false;
            }
            if (bau_profundidade.Text == "")
            {
                MessageBox.Show("Profundidade é obrigatório", "Informação requerida", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                bau_profundidade.Focus();
                return false;
            }
            if (nome_conferente.Text == "")
            {
                MessageBox.Show("Conferente é obrigatório", "Informação requerida", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                nome_conferente.Focus();
                return false;
            }
            return true;
        }
    }

    public class RomaneioViewModel : INotifyPropertyChanged
    {
        /*
        private List<string> operacoes = new List<string>()
        {
          "CARREGAMENTO SHOPPING",
          "CARREGAMENTO TRANSFERÊNCIA",
          "DESCARREGAMENTO SHOPPING",
          "DESCARREGAMENTO TRANSFERÊNCIA",
          "KIT SOLUÇÃO"
        };
        */
        /*
        private List<string> condicaoCaminhao = new List<string>()
        {
          "BOA",
          "REGULAR",
          "PÉSSIMA"
        };
        */

        private RomaneioModel? romaneio;
        public RomaneioModel Romaneio
        {
            get { return romaneio; }
            set { romaneio = value; RaisePropertyChanged("Romaneio"); }
        }

        private ObservableCollection<RomaneioModel>? romaneios;
        public ObservableCollection<RomaneioModel> Romaneios
        {
            get { return romaneios; }
            set { romaneios = value; RaisePropertyChanged("Romaneios"); }
        }

        private List<string>? operacoesList = new List<string> { "CARREGAMENTO SHOPPING", "CARREGAMENTO TRANSFERÊNCIA", "DESCARREGAMENTO SHOPPING", "DESCARREGAMENTO TRANSFERÊNCIA", "KIT SOLUÇÃO" };
        public List<string> OperacoesList
        {
            get { return operacoesList; }
            set { operacoesList = value; RaisePropertyChanged("OperacoesList"); }
        }

        private List<string>? condicaoCaminhaoList = new List<string> { "BOA", "REGULAR", "PÉSSIMA"  };
        public List<string> CondicaoCaminhaoList
        {
            get { return condicaoCaminhaoList; }
            set { condicaoCaminhaoList = value; RaisePropertyChanged("CondicaoCaminhaoList"); }
        }

        private ObservableCollection<AprovadoModel>? aprovados;
        public ObservableCollection<AprovadoModel> Aprovados
        {
            get { return aprovados; }
            set { aprovados = value; RaisePropertyChanged("Aprovados"); }
        }

        private ObservableCollection<TranportadoraModel>? tranportadoras;
        public ObservableCollection<TranportadoraModel> Tranportadoras
        {
            get { return tranportadoras; }
            set { tranportadoras = value; RaisePropertyChanged("Tranportadoras"); }
        }

        /*public List<string> OperacoesList
        {
            get => this.operacoes;
            set => this.operacoes = value;
        }*/
        /*
        public List<string> CondicaoCaminhaoList
        {
            get => this.condicaoCaminhao;
            set => this.condicaoCaminhao = value;
        }
        */

        public async Task<ObservableCollection<AprovadoModel>> GetAprovados()
        {
            try
            {
                using AppDatabase db = new();
                var data = await db.Aprovados.OrderBy(c => c.SiglaServ).ToListAsync();
                return new ObservableCollection<AprovadoModel>(data);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ObservableCollection<TranportadoraModel>> GetTransportadoras()
        {
            try
            {
                using AppDatabase db = new();
                var data = await db.Tranportadoras.OrderBy(c => c.NomeTransportadora).ToListAsync();
                return new ObservableCollection<TranportadoraModel>(data);
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<RomaneioModel> SaveAsync(RomaneioModel romaneio)
        {
            try
            {
                using AppDatabase db = new AppDatabase();
                await db.Romaneios.SingleMergeAsync(romaneio);
                int num = await db.SaveChangesAsync();
                long? codRomaneiro = romaneio.CodRomaneiro;
            }
            catch (Exception)
            {
                throw;
            }
            return romaneio;
        }

        public async Task<ObservableCollection<RomaneioModel>> GetRomaneiosAsync()
        {

            try
            {
                using AppDatabase db = new();
                var data = await db.Romaneios.OrderBy(n => n.ShoppingDestino).ThenBy(n => n.NumeroCaminhao).ToListAsync();
                return new ObservableCollection<RomaneioModel>(data);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
