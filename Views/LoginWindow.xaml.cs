using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using SISTEMA_DE_VENDAS_GENERICO.Data;
using SISTEMA_DE_VENDAS_GENERICO.Models;

namespace SISTEMA_DE_VENDAS_GENERICO.Views
{
    public partial class LoginWindow : Window
    {
        public static Usuario UsuarioLogado { get; private set; }
        private bool senhaVisivel = false;

        public LoginWindow()
        {
            InitializeComponent();
            
            // Configurar a janela
            this.Loaded += LoginWindow_Loaded;
            
            // Configurar eventos de teclado
            this.KeyDown += LoginWindow_KeyDown;
            
            // Configurar eventos dos campos
            txtUsuario.GotFocus += TxtUsuario_GotFocus;
            txtSenha.GotFocus += TxtSenha_GotFocus;
            txtSenhaVisivel.GotFocus += TxtSenhaVisivel_GotFocus;
            
            // Configurar eventos de teclado
            txtUsuario.KeyDown += TxtUsuario_KeyDown;
            txtSenha.KeyDown += TxtSenha_KeyDown;
            txtSenhaVisivel.KeyDown += TxtSenhaVisivel_KeyDown;
            
            // Configurar eventos de clique nos botões
            btnLogin.Click += btnLogin_Click;
            btnCancelar.Click += btnCancelar_Click;
            btnMostrarSenha.Click += btnMostrarSenha_Click;
        }

        private void LoginWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Garantir que os campos estejam habilitados e funcionais
            txtUsuario.IsEnabled = true;
            txtSenha.IsEnabled = true;
            txtSenhaVisivel.IsEnabled = true;
            btnLogin.IsEnabled = true;
            btnCancelar.IsEnabled = true;
            btnMostrarSenha.IsEnabled = true;
            
            // Limpar campos e definir foco
            txtUsuario.Text = "";
            txtSenha.Password = "";
            txtSenhaVisivel.Text = "";
            txtUsuario.Focus();
            
            // Testar e configurar conexão com o banco
            ConfigurarConexaoBanco();
        }

        private void LoginWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                btnCancelar_Click(sender, e);
            }
        }

        private void TxtUsuario_GotFocus(object sender, RoutedEventArgs e)
        {
            txtUsuario.SelectAll();
        }

        private void TxtSenha_GotFocus(object sender, RoutedEventArgs e)
        {
            txtSenha.SelectAll();
        }

        private void TxtSenhaVisivel_GotFocus(object sender, RoutedEventArgs e)
        {
            txtSenhaVisivel.SelectAll();
        }

        private void TxtUsuario_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (senhaVisivel)
                    txtSenhaVisivel.Focus();
                else
                    txtSenha.Focus();
            }
        }

        private void TxtSenha_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnLogin_Click(sender, e);
            }
        }

        private void TxtSenhaVisivel_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnLogin_Click(sender, e);
            }
        }

        private void btnMostrarSenha_Click(object sender, RoutedEventArgs e)
        {
            senhaVisivel = !senhaVisivel;
            
            if (senhaVisivel)
            {
                // Mostrar senha
                txtSenhaVisivel.Text = txtSenha.Password;
                txtSenha.Visibility = Visibility.Collapsed;
                txtSenhaVisivel.Visibility = Visibility.Visible;
                btnMostrarSenha.Content = "🙈";
                btnMostrarSenha.ToolTip = "Ocultar senha";
                txtSenhaVisivel.Focus();
            }
            else
            {
                // Ocultar senha
                txtSenha.Password = txtSenhaVisivel.Text;
                txtSenha.Visibility = Visibility.Visible;
                txtSenhaVisivel.Visibility = Visibility.Collapsed;
                btnMostrarSenha.Content = "👁️";
                btnMostrarSenha.ToolTip = "Mostrar senha";
                txtSenha.Focus();
            }
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            // Desabilitar botão para evitar cliques múltiplos
            btnLogin.IsEnabled = false;
            
            try
            {
                string usuario = txtUsuario.Text.Trim();
                string senha = senhaVisivel ? txtSenhaVisivel.Text.Trim() : txtSenha.Password.Trim();

                if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(senha))
                {
                    MessageBox.Show("Por favor, preencha usuário e senha.", "Aviso", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtUsuario.Focus();
                    return;
                }

                // Testar conexão com o banco primeiro
                if (!DatabaseConnection.TestConnection())
                {
                    var result = MessageBox.Show("Não foi possível conectar ao banco de dados.\n\nDeseja tentar criar o banco de dados automaticamente?", 
                        "Erro de Conexão", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    
                    if (result == MessageBoxResult.Yes)
                    {
                        if (!DatabaseConnection.CreateDatabaseIfNotExists())
                        {
                            return;
                        }
                    }
                    else
                    {
                        return;
                    }
                }

                if (AutenticarUsuario(usuario, senha))
                {
                    // Abrir a janela principal
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Usuário ou senha incorretos.\n\nTente:\n• admin / admin123\n• vendedor / venda123\n• gerente / gerente123", "Erro de Login", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    
                    if (senhaVisivel)
                    {
                        txtSenhaVisivel.Text = "";
                        txtSenhaVisivel.Focus();
                    }
                    else
                    {
                    txtSenha.Password = "";
                    txtSenha.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro inesperado: {ex.Message}", "Erro", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Reabilitar botão
                btnLogin.IsEnabled = true;
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Deseja realmente sair do sistema?", 
                "Confirmação", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

        private void ConfigurarConexaoBanco()
        {
            try
            {
                // Tentar testar a conexão silenciosamente
                if (!DatabaseConnection.TestConnection())
                {
                    // Se falhar, tentar criar o banco automaticamente
                    DatabaseConnection.CreateDatabaseIfNotExists();
                }
            }
            catch
            {
                // Erro silencioso na inicialização
            }
        }

        private bool AutenticarUsuario(string usuario, string senha)
        {
            string query = @"
                SELECT Id, Nome, Usuario, Senha, NivelAcesso, Ativo 
                FROM Usuarios 
                WHERE Usuario = @Usuario AND Senha = @Senha AND Ativo = 1";

            SqlParameter[] parameters = {
                new SqlParameter("@Usuario", usuario),
                new SqlParameter("@Senha", senha)
            };

            DataTable dt = DatabaseConnection.ExecuteQuery(query, parameters);

            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                UsuarioLogado = new Usuario
                {
                    Id = Convert.ToInt32(row["Id"]),
                    Nome = row["Nome"].ToString(),
                    NomeUsuario = row["Usuario"].ToString(),
                    Senha = row["Senha"].ToString(),
                    NivelAcesso = row["NivelAcesso"].ToString(),
                    Ativo = Convert.ToBoolean(row["Ativo"])
                };
                return true;
            }

            return false;
        }
    }
} 