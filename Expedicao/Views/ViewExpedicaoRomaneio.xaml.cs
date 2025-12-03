using Dapper;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Syncfusion.UI.Xaml.Diagram;
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
using static System.Runtime.InteropServices.JavaScript.JSType;

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
                int num1 = itemsSource1.IndexOf(itemsSource1.Where(a => a.SiglaServ.Equals(vm.Romaneio.shopping_destino)).FirstOrDefault());

                IList<TranportadoraModel> itemsSource2 = (IList<TranportadoraModel>)codtransportadora.ItemsSource;
                int num2 = itemsSource2.IndexOf(itemsSource2.Where(t => t.CodTransportadora.Equals(vm.Romaneio.codtransportadora)).FirstOrDefault());

                operacao.ItemsSource = vm.OperacoesList;
                condicao_caminhao.ItemsSource = vm.CondicaoCaminhaoList;
                operacao.SelectedIndex = vm.OperacoesList.FindIndex(o => o.Equals(vm.Romaneio.operacao));
                cod_romaneiro.Value = vm.Romaneio.cod_romaneiro;
                data_carregamento.DateTime = new DateTime?((DateTime)vm.Romaneio.data_carregamento);
                hora_chegada.Value = vm.Romaneio.hora_chegada.ToString();
                shopping_destino.SelectedIndex = num1;
                numero_caminhao.Value = new long?((long)vm.Romaneio.numero_caminhao);
                local_carregamento.Text = vm.Romaneio.local_carregamento;
                codtransportadora.SelectedIndex = num2;
                nome_motorista.Text = vm.Romaneio.nome_motorista;
                numero_cnh.Text = vm.Romaneio.numero_cnh;
                telefone_motorista.Text = vm.Romaneio.telefone_motorista;
                condicao_caminhao.SelectedIndex = vm.CondicaoCaminhaoList.FindIndex(c => c.Equals(vm.Romaneio.condicao_caminhao));
                placa_caminhao.Text = vm.Romaneio.placa_caminhao;
                placa_cidade.Text = vm.Romaneio.placa_cidade;
                placa_estado.Text = vm.Romaneio.placa_estado;
                placa_carroceria.Text = vm.Romaneio.placa_carroceria;
                placa_carroceria_cidade.Text = vm.Romaneio.placa_carroceria_cidade;
                placa_carroceria_estado.Text = vm.Romaneio.placa_carroceria_estado;
                bau_altura.Value = new double?((double)vm.Romaneio.bau_altura);
                bau_largura.Value = new double?((double)vm.Romaneio.bau_largura);
                bau_profundidade.Value = new double?((double)vm.Romaneio.bau_profundidade);
                m3_carregado.Value = new double?((double)vm.Romaneio?.m3_carregado);
                bau_soba.Value = new double?((double)vm.Romaneio?.bau_soba);
                m3_portaria.Value = new double?((double)vm.Romaneio?.m3_portaria);
                nome_conferente.Text = vm.Romaneio.nome_conferente;
                num_lacres.Text = vm.Romaneio.num_lacres;
                numero_container.Text = vm.Romaneio.numero_container;
                dateSaida.DateTime = vm.Romaneio?.data_hora_liberacao;//new DateTime?((DateTime)vm.Romaneio?.data_hora_liberacao);

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
                        Romaneio.cod_romaneiro = vm.Romaneio?.cod_romaneiro;
                        Romaneio.operacao = operacao.SelectionBoxItem.ToString();
                        Romaneio.data_carregamento = data_carregamento.DateTime.Value;
                        Romaneio.hora_chegada = TimeSpan.Parse(hora_chegada.Value.ToString());
                        Romaneio.shopping_destino = selectedItem.SiglaServ;
                        Romaneio.numero_caminhao = numero_caminhao.Value.Value;
                        Romaneio.local_carregamento = local_carregamento.Text;
                        Romaneio.codtransportadora = (codtransportadora.SelectedItem as TranportadoraModel).CodTransportadora;
                        Romaneio.nome_motorista = nome_motorista.Text;
                        Romaneio.numero_cnh = numero_cnh.Text;
                        Romaneio.telefone_motorista = telefone_motorista.Text;
                        Romaneio.condicao_caminhao = condicao_caminhao.SelectionBoxItem.ToString();
                        Romaneio.placa_caminhao = placa_caminhao.Text;
                        Romaneio.placa_cidade = placa_cidade.Text;
                        Romaneio.placa_estado = placa_estado.Text;
                        Romaneio.placa_carroceria = placa_carroceria.Text;
                        Romaneio.placa_carroceria_cidade = placa_carroceria_cidade.Text;
                        Romaneio.placa_carroceria_estado = placa_carroceria_estado.Text;
                        Romaneio.bau_altura = bau_altura?.Value;
                        Romaneio.bau_largura = bau_largura?.Value;
                        Romaneio.bau_profundidade = bau_profundidade?.Value;
                        Romaneio.m3_carregado = m3_carregado?.Value;
                        Romaneio.bau_soba = bau_soba?.Value;
                        Romaneio.m3_portaria = m3_portaria?.Value;
                        Romaneio.nome_conferente = nome_conferente.Text;
                        Romaneio.num_lacres = num_lacres.Text;
                        Romaneio.numero_container = numero_container.Text;
                        Romaneio.data_hora_liberacao = dateSaida.DateTime.Value;
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

        private readonly DataBase BaseSettings = DataBase.Instance;
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


        public async Task<RomaneioModel> SaveAsync(RomaneioModel model)
        {
            /*
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
            */

            using var conn = new NpgsqlConnection(BaseSettings.ConnectionString);
            var sqlSelect = @"SELECT * FROM expedicao.t_romaneio WHERE cod_romaneiro = @cod_romaneiro";
            var existente = await conn.QueryFirstOrDefaultAsync<RomaneioModel?>(sqlSelect, new { model.cod_romaneiro });
            if (existente == null)
            {
                // INSERT
                var sqlInsert = @"
                    INSERT INTO expedicao.t_romaneio
                    (   
                        operacao,
                        data_carregamento,
                        hora_chegada,
                        shopping_destino,
                        numero_caminhao,
                        local_carregamento,
                        codtransportadora,
                        nome_motorista,
                        numero_cnh,
                        telefone_motorista,
                        condicao_caminhao,
                        placa_caminhao,
                        placa_cidade,
                        placa_estado,
                        placa_carroceria,
                        placa_carroceria_cidade,
                        placa_carroceria_estado,
                        bau_altura,
                        bau_largura,
                        bau_profundidade,
                        m3_carregado,
                        bau_soba,
                        m3_portaria,
                        nome_conferente,
                        num_lacres,
                        numero_container,
                        data_hora_liberacao
                    )
                    VALUES
                    (
                        @operacao,
                        @data_carregamento,
                        @hora_chegada,
                        @shopping_destino,
                        @numero_caminhao,
                        @local_carregamento,
                        @codtransportadora,
                        @nome_motorista,
                        @numero_cnh,
                        @telefone_motorista,
                        @condicao_caminhao,
                        @placa_caminhao,
                        @placa_cidade,
                        @placa_estado,
                        @placa_carroceria,
                        @placa_carroceria_cidade,
                        @placa_carroceria_estado,
                        @bau_altura,
                        @bau_largura,
                        @bau_profundidade,
                        @m3_carregado,
                        @bau_soba,
                        @m3_portaria,
                        @nome_conferente,
                        @num_lacres,
                        @numero_container,
                        @data_hora_liberacao
                    )
                    RETURNING cod_romaneiro;
                ";
                model.cod_romaneiro = await conn.ExecuteScalarAsync<int>(sqlInsert, model);
            }
            else
            {
                var tipo = typeof(RomaneioModel);
                // 2) Lista de SETs só dos alterados
                var setList = new List<string>();
                var parametros = new DynamicParameters();
                foreach (var prop in tipo.GetProperties())
                {
                    if (prop.Name.Equals("cod_romaneiro", StringComparison.OrdinalIgnoreCase))
                        continue;

                    var valorNovo = prop.GetValue(model);
                    var valorAntigo = prop.GetValue(existente);

                    // Ignora valores nulos do modelo novo
                    // (você pode mudar esse comportamento)
                    if (valorNovo == null)
                        continue;

                    // Só adiciona se mudou
                    if (!Equals(valorNovo, valorAntigo))
                    {
                        setList.Add($"{prop.Name} = @{prop.Name}");
                        parametros.Add(prop.Name, valorNovo);
                    }
                }
                // Se nada mudou, não atualizar
                if (setList.Count == 0)
                    return model;
                // 3) Completar parâmetros com @id
                parametros.Add("cod_romaneiro", model.cod_romaneiro);
                // 4) Montar SQL final
                var sqlUpdate = $@"
                    UPDATE expedicao.t_romaneio
                    SET {string.Join(", ", setList)}
                    WHERE cod_romaneiro = @cod_romaneiro;
                ";
                await conn.ExecuteAsync(sqlUpdate, model);

                string[] campos = [
                    "placa_carroceria",
                    "data_carregamento",
                    "codtransportadora"
                ];
                // verifica se algum dos campos monitorados realmente foi alterado
                bool camposAlterados = setList.Any(s => campos.Any(c => s.Contains(c)));
                if (camposAlterados)
                {
                    var updateServicosList = new List<string>();
                    parametros = new DynamicParameters();
                    // SE placa mudou → incluir no UPDATE
                    if (setList.Any(s => s.Contains("placa_carroceria")))
                    {
                        updateServicosList.Add("placa_caminhao = @placa_carroceria");
                        parametros.Add("placa_carroceria", model.placa_carroceria);
                    }
                    // SE data mudou → incluir no UPDATE
                    if (setList.Any(s => s.Contains("data_carregamento")))
                    {
                        updateServicosList.Add("data_chegada_cipolatti = @data_carregamento");
                        parametros.Add("data_carregamento", model.data_carregamento);
                    }
                    // SE transportadora mudou → incluir no UPDATE
                    if (setList.Any(s => s.Contains("codtransportadora")))
                    {
                        var transp = Tranportadoras.SingleOrDefault(t => t.CodTransportadora == model.codtransportadora);
                        updateServicosList.Add("transportadora = @transportadora");
                        parametros.Add("transportadora", transp.NomeTransportadora);
                    }
                    // se nenhum campo do romaneio mudou → não faz nada
                    if (updateServicosList.Count == 0)
                        return model;

                    // parametros fixos
                    parametros.Add("siglaserv", model.shopping_destino);
                    parametros.Add("caminhao", string.Format("{0:00}", model.numero_caminhao));

                    string sqlUpdateServicos = "";

                    if (model.operacao == "DESCARREGAMENTO SHOPPING")
                        sqlUpdateServicos = $@"
                            UPDATE operacional.t_cargas_desmontagem
                            SET {string.Join(", ", updateServicosList)}
                            WHERE siglaserv = @siglaserv
                              AND caminhao = @caminhao;
                        ";
                    /*else
                        sqlUpdateServicos = $@"
                            UPDATE operacional.t_cargas_carregamento
                            SET {string.Join(", ", updateServicosList)}
                            WHERE siglaserv = @siglaserv
                              AND caminhao = @caminhao
                              AND operacao = 'CARREGAMENTO SHOPPING';
                        ";*/

                    await conn.ExecuteAsync(sqlUpdateServicos, parametros);
                }
            }

            return model;
        }

        public async Task<ObservableCollection<RomaneioModel>> GetRomaneiosAsync()
        {

            try
            {
                using AppDatabase db = new();
                var data = await db.Romaneios.OrderBy(n => n.shopping_destino).ThenBy(n => n.numero_caminhao).ToListAsync();
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
