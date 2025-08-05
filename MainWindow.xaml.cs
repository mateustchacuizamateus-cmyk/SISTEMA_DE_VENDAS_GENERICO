using System.Windows;
using SISTEMA_DE_VENDAS_GENERICO.Views;

namespace SISTEMA_DE_VENDAS_GENERICO
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ConfigurarInterface();
            CarregarDashboard();
        }

        private void ConfigurarInterface()
        {
            if (Views.LoginWindow.UsuarioLogado != null)
            {
                txtUsuarioLogado.Text = $"Usuário: {Views.LoginWindow.UsuarioLogado.Nome}";
                
                // Configurar permissões baseadas no nível de acesso
                ConfigurarPermissoes();
            }
        }

        private void ConfigurarPermissoes()
        {
            string nivelAcesso = Views.LoginWindow.UsuarioLogado.NivelAcesso;
            
            // Vendedor - acesso limitado
            if (nivelAcesso == "Vendedor")
            {
                btnUsuarios.IsEnabled = false;
                btnConfiguracoes.IsEnabled = false;
                btnRelatorios.IsEnabled = false;
            }
            // Gerente - acesso intermediário
            else if (nivelAcesso == "Gerente")
            {
                btnUsuarios.IsEnabled = false;
            }
            // Administrador - acesso total
        }

        private void CarregarDashboard()
        {
            // Por padrão, carrega o dashboard
            frameConteudo.Navigate(new DashboardPage());
        }

        #region Eventos dos Botões do Menu

        private void btnDashboard_Click(object sender, RoutedEventArgs e)
        {
            frameConteudo.Navigate(new DashboardPage());
        }

        private void btnVendas_Click(object sender, RoutedEventArgs e)
        {
            frameConteudo.Navigate(new VendasPage());
        }

        private void btnProdutos_Click(object sender, RoutedEventArgs e)
        {
            frameConteudo.Navigate(new ProdutosPage());
        }

        private void btnEstoque_Click(object sender, RoutedEventArgs e)
        {
            frameConteudo.Navigate(new EstoquePage());
        }

        private void btnClientes_Click(object sender, RoutedEventArgs e)
        {
            frameConteudo.Navigate(new ClientesPage());
        }

        private void btnFornecedores_Click(object sender, RoutedEventArgs e)
        {
            frameConteudo.Navigate(new FornecedoresPage());
        }

        private void btnRelatorios_Click(object sender, RoutedEventArgs e)
        {
            frameConteudo.Navigate(new RelatoriosPage());
        }

        private void btnUsuarios_Click(object sender, RoutedEventArgs e)
        {
            frameConteudo.Navigate(new UsuariosPage());
        }

        private void btnConfiguracoes_Click(object sender, RoutedEventArgs e)
        {
            frameConteudo.Navigate(new ConfiguracoesPage());
        }

        private void btnSair_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Deseja realmente sair do sistema?", 
                                       "Confirmação", 
                                       MessageBoxButton.YesNo, 
                                       MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

        #endregion
    }
}
