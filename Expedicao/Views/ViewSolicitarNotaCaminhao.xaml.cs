﻿using Expedicao.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Expedicao.Views
{
    /// <summary>
    /// Interação lógica para ViewSolicitarNotaCaminhao.xam
    /// </summary>
    public partial class ViewSolicitarNotaCaminhao : UserControl
    {
        public ViewSolicitarNotaCaminhao()
        {
            InitializeComponent();
            this.DataContext = new ViewSolicitarNotaCaminhaoViewModel();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });

                ViewSolicitarNotaCaminhaoViewModel vm = (ViewSolicitarNotaCaminhaoViewModel)DataContext;
                vm.SiglasCaminhoes = await vm.GetSiglasCaminhoes();
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show(ex.Message);
            }
        }

    }

    public class ViewSolicitarNotaCaminhaoViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        private ObservableCollection<CubagemSiglaCaminaoModel>? _siglasCaminhoes;
        public ObservableCollection<CubagemSiglaCaminaoModel> SiglasCaminhoes
        {
            get { return _siglasCaminhoes; }
            set { _siglasCaminhoes = value; RaisePropertyChanged("SiglasCaminhoes"); }
        }

        private CubagemSiglaCaminaoModel? _siglaCaminhao;
        public CubagemSiglaCaminaoModel SiglaCaminhao
        {
            get { return _siglaCaminhao; }
            set { _siglaCaminhao = value; RaisePropertyChanged("SiglaCaminhao"); }
        }

        public async Task<ObservableCollection<CubagemSiglaCaminaoModel>> GetSiglasCaminhoes()
        {
            try
            {
                using AppDatabase db = new();
                //var data = await db.CubagemSiglaCaminhoes.OrderBy(c => c.data_de_expedicao).ThenBy(c => c.sigla).ThenBy(c => c.baia_caminhao) .ToListAsync();
                //return new ObservableCollection<CubagemSiglaCaminaoModel>(data);

                var query = await db.CubagemSiglaCaminhoes
                    //.AsEnumerable() // Transforma em IEnumerable para acessar possíveis funcionalidades não suportadas por SQL (caso necessário)
                    .GroupBy(x => new { x.data_de_expedicao, x.sigla, x.baia_caminhao })
                    .Select(g => new CubagemSiglaCaminaoModel
                    {
                        data_de_expedicao = g.Key.data_de_expedicao,
                        sigla = g.Key.sigla,
                        baia_caminhao = g.Key.baia_caminhao,
                        preco = g.Sum(x => x.preco ?? 0),
                        volumes = g.Sum(x => x.volumes ?? 0),
                        pl = g.Sum(x => x.pl ?? 0),
                        pb = g.Sum(x => x.pb ?? 0),
                        m3 = g.Sum(x => x.m3 ?? 0)
                    })
                    .OrderBy(x => x.data_de_expedicao)
                    .ThenBy(x => x.sigla)
                    .ThenBy(x => x.baia_caminhao)
                    .ToListAsync();
                return new ObservableCollection<CubagemSiglaCaminaoModel>(query);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
