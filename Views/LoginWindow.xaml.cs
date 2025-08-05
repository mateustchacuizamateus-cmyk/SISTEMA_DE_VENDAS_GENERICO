using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using SISTEMA_DE_VENDAS_GENERICO.Data;
using SISTEMA_DE_VENDAS_GENERICO.Models;

namespace SISTEMA_DE_VENDAS_GENERICO.Views
{
    /// <summary>
    /// Janela de login do Sistema de Vendas Angola
    /// Inclui validações robustas, controle de tentativas e interface moderna
    /// </summary>
    public partial class LoginWindow : Window
    {
        #region Propriedades Estáticas

        /// <summary>
        /// Usuário atualmente logado no sistema
        /// </summary>
        public static Usuario UsuarioLogado { get; private set; }

        #endregion

        #region Propriedades Privadas

        /// <summary>
        /// Controla se a senha está visível ou oculta
        /// </summary>
        private bool senhaVisivel = false;

        /// <summary>
        /// Número máximo de tentativas de login permitidas
        /// </summary>
        private const int MAX_TENTATIVAS_LOGIN = 5;

        /// <summary>
        /// Contador de tentativas de login da sessão atual
        /// </summary>
        private int tentativasAtual = 0;

        /// <summary>
        /// Flag para controlar se está processando login
        /// </summary>
        private bool processandoLogin = false;

        #endregion

        #region Construtor

        /// <summary>
        /// Inicializa a janela de login
        /// </summary>
        public LoginWindow()
        {
            InitializeComponent();
            InicializarJanela();
        }

        #endregion

        #region Métodos de Inicialização

        /// <summary>
        /// Configura a janela e seus componentes
        /// </summary>
        private void InicializarJanela()
        {
            // Configurar eventos
            this.Loaded += LoginWindow_Loaded;
            this.KeyDown += LoginWindow_KeyDown;
            
            // Configurar eventos dos campos
            ConfigurarEventosCampos();
            
            // Inicializar interface
            LimparCampos();
            
            // Verificar conexão de forma assíncrona
            VerificarConexaoAsync();
        }

        /// <summary>
        /// Configura os eventos dos campos de entrada
        /// </summary>
        private void ConfigurarEventosCampos()
        {
            // Eventos de foco
            txtUsuario.GotFocus += TxtUsuario_GotFocus;
            txtSenha.GotFocus += TxtSenha_GotFocus;
            txtSenhaVisivel.GotFocus += TxtSenhaVisivel_GotFocus;
            
            // Eventos de teclado
            txtUsuario.KeyDown += TxtUsuario_KeyDown;
            txtSenha.KeyDown += TxtSenha_KeyDown;
            txtSenhaVisivel.KeyDown += TxtSenhaVisivel_KeyDown;
            
            // Eventos de mudança de texto
            txtUsuario.TextChanged += TxtUsuario_TextChanged;
            txtSenhaVisivel.TextChanged += TxtSenhaVisivel_TextChanged;
            txtSenha.PasswordChanged += TxtSenha_PasswordChanged;
        }

        /// <summary>
        /// Evento disparado quando a janela é carregada
        /// </summary>
        private void LoginWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Garantir que os campos estejam habilitados
            HabilitarCampos(true);
            
            // Definir foco inicial
            txtUsuario.Focus();
            
            // Carregar dados salvos se existirem
            CarregarDadosSalvos();
        }

        #endregion

        #region Métodos de Interface

        /// <summary>
        /// Limpa todos os campos do formulário
        /// </summary>
        private void LimparCampos()
        {
            txtUsuario.Text = "";
            txtSenha.Password = "";
            txtSenhaVisivel.Text = "";
            chkLembrar.IsChecked = false;
            
            // Resetar visibilidade da senha
            senhaVisivel = false;
            AtualizarVisibilidadeSenha();
        }

        /// <summary>
        /// Habilita ou desabilita os campos de entrada
        /// </summary>
        /// <param name="habilitado">True para habilitar, False para desabilitar</param>
        private void HabilitarCampos(bool habilitado)
        {
            txtUsuario.IsEnabled = habilitado;
            txtSenha.IsEnabled = habilitado;
            txtSenhaVisivel.IsEnabled = habilitado;
            btnMostrarSenha.IsEnabled = habilitado;
            btnLogin.IsEnabled = habilitado;
            chkLembrar.IsEnabled = habilitado;
        }

        /// <summary>
        /// Atualiza a visibilidade do campo de senha
        /// </summary>
        private void AtualizarVisibilidadeSenha()
        {
            if (senhaVisivel)
            {
                // Mostrar senha como texto
                txtSenhaVisivel.Text = txtSenha.Password;
                txtSenha.Visibility = Visibility.Collapsed;
                txtSenhaVisivel.Visibility = Visibility.Visible;
                borderSenhaVisivel.Visibility = Visibility.Visible;
                btnMostrarSenha.Content = "🙈";
                btnMostrarSenha.ToolTip = "Ocultar palavra-passe";
            }
            else
            {
                // Ocultar senha
                txtSenha.Password = txtSenhaVisivel.Text;
                txtSenha.Visibility = Visibility.Visible;
                txtSenhaVisivel.Visibility = Visibility.Collapsed;
                borderSenhaVisivel.Visibility = Visibility.Collapsed;
                btnMostrarSenha.Content = "👁️";
                btnMostrarSenha.ToolTip = "Mostrar palavra-passe";
            }
        }

        /// <summary>
        /// Atualiza o status da conexão na interface
        /// </summary>
        /// <param name="conectado">Status da conexão</param>
        /// <param name="mensagem">Mensagem de status</param>
        private void AtualizarStatusConexao(bool conectado, string mensagem)
        {
            Dispatcher.Invoke(() =>
            {
                indicadorConexao.Fill = conectado ? 
                    new SolidColorBrush(Color.FromRgb(39, 174, 96)) :  // Verde
                    new SolidColorBrush(Color.FromRgb(231, 76, 60));   // Vermelho
                
                txtStatusConexao.Text = mensagem;
            });
        }

        #endregion

        #region Eventos de Teclado

        /// <summary>
        /// Manipula teclas globais da janela
        /// </summary>
        private void LoginWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                btnCancelar_Click(sender, e);
            }
        }

        /// <summary>
        /// Evento de tecla no campo usuário
        /// </summary>
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

        /// <summary>
        /// Evento de tecla no campo senha oculta
        /// </summary>
        private void TxtSenha_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnLogin_Click(sender, e);
            }
        }

        /// <summary>
        /// Evento de tecla no campo senha visível
        /// </summary>
        private void TxtSenhaVisivel_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnLogin_Click(sender, e);
            }
        }

        #endregion

        #region Eventos de Foco

        /// <summary>
        /// Seleciona todo o texto quando o campo usuário recebe foco
        /// </summary>
        private void TxtUsuario_GotFocus(object sender, RoutedEventArgs e)
        {
            txtUsuario.SelectAll();
        }

        /// <summary>
        /// Seleciona todo o texto quando o campo senha oculta recebe foco
        /// </summary>
        private void TxtSenha_GotFocus(object sender, RoutedEventArgs e)
        {
            txtSenha.SelectAll();
        }

        /// <summary>
        /// Seleciona todo o texto quando o campo senha visível recebe foco
        /// </summary>
        private void TxtSenhaVisivel_GotFocus(object sender, RoutedEventArgs e)
        {
            txtSenhaVisivel.SelectAll();
        }

        #endregion

        #region Eventos de Mudança de Texto

        /// <summary>
        /// Evento disparado quando o texto do usuário muda
        /// </summary>
        private void TxtUsuario_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Converter para minúsculas automaticamente
            var textBox = sender as TextBox;
            if (textBox != null)
            {
                int selectionStart = textBox.SelectionStart;
                textBox.Text = textBox.Text.ToLower();
                textBox.SelectionStart = selectionStart;
            }
        }

        /// <summary>
        /// Sincroniza senha visível com senha oculta
        /// </summary>
        private void TxtSenhaVisivel_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (senhaVisivel)
            {
                txtSenha.Password = txtSenhaVisivel.Text;
            }
        }

        /// <summary>
        /// Sincroniza senha oculta com senha visível
        /// </summary>
        private void TxtSenha_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!senhaVisivel)
            {
                txtSenhaVisivel.Text = txtSenha.Password;
            }
        }

        #endregion

        #region Eventos de Botões

        /// <summary>
        /// Alterna a visibilidade da senha
        /// </summary>
        private void btnMostrarSenha_Click(object sender, RoutedEventArgs e)
        {
            senhaVisivel = !senhaVisivel;
            AtualizarVisibilidadeSenha();
            
            // Manter foco no campo de senha ativo
            if (senhaVisivel)
                txtSenhaVisivel.Focus();
            else
                txtSenha.Focus();
        }

        /// <summary>
        /// Processa o login do usuário
        /// </summary>
        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (processandoLogin)
                return;

            processandoLogin = true;
            
            try
            {
                // Desabilitar interface durante o processamento
                HabilitarCampos(false);
                btnLogin.Content = "🔄 VERIFICANDO...";
                
                // Obter dados do formulário
                string usuario = txtUsuario.Text.Trim();
                string senha = senhaVisivel ? txtSenhaVisivel.Text.Trim() : txtSenha.Password.Trim();

                // Validar campos obrigatórios
                if (!ValidarCamposObrigatorios(usuario, senha))
                {
                    return;
                }

                // Verificar limite de tentativas
                if (tentativasAtual >= MAX_TENTATIVAS_LOGIN)
                {
                    MostrarErro("Muitas tentativas de login. Reinicie a aplicação para tentar novamente.");
                    return;
                }

                // Testar conexão com banco
                bool conexaoOk = await DatabaseConnection.TestConnectionAsync();
                if (!conexaoOk)
                {
                    var resultado = MessageBox.Show(
                        "Não foi possível conectar ao banco de dados.\n\n" +
                        "Deseja tentar criar o banco automaticamente?",
                        "Erro de Conexão - Sistema de Vendas Angola",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question
                    );

                    if (resultado == MessageBoxResult.Yes)
                    {
                        bool bancoCriado = DatabaseConnection.CreateDatabaseIfNotExists();
                        if (!bancoCriado)
                        {
                            MostrarErro("Não foi possível criar o banco de dados. Verifique as permissões.");
                            return;
                        }
                    }
                    else
                    {
                        return;
                    }
                }

                // Tentar autenticar
                Usuario usuarioAutenticado = await AutenticarUsuarioAsync(usuario, senha);
                
                if (usuarioAutenticado != null)
                {
                    // Login bem-sucedido
                    await ProcessarLoginSucesso(usuarioAutenticado);
                }
                else
                {
                    // Login falhou
                    ProcessarLoginFalha(usuario);
                }
            }
            catch (Exception ex)
            {
                MostrarErro($"Erro inesperado durante o login: {ex.Message}");
            }
            finally
            {
                // Reabilitar interface
                processandoLogin = false;
                HabilitarCampos(true);
                btnLogin.Content = "🚀 ENTRAR NO SISTEMA";
            }
        }

        /// <summary>
        /// Cancela o login e fecha a aplicação
        /// </summary>
        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            var resultado = MessageBox.Show(
                "Deseja realmente sair do Sistema de Vendas Angola?",
                "Confirmação de Saída",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (resultado == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

        /// <summary>
        /// Fecha a janela
        /// </summary>
        private void btnFechar_Click(object sender, RoutedEventArgs e)
        {
            btnCancelar_Click(sender, e);
        }

        #endregion

        #region Métodos de Validação

        /// <summary>
        /// Valida se os campos obrigatórios estão preenchidos
        /// </summary>
        /// <param name="usuario">Nome de usuário</param>
        /// <param name="senha">Senha</param>
        /// <returns>True se válidos</returns>
        private bool ValidarCamposObrigatorios(string usuario, string senha)
        {
            if (string.IsNullOrEmpty(usuario))
            {
                MostrarAviso("Por favor, digite o nome de usuário.");
                txtUsuario.Focus();
                return false;
            }

            if (string.IsNullOrEmpty(senha))
            {
                MostrarAviso("Por favor, digite a palavra-passe.");
                if (senhaVisivel)
                    txtSenhaVisivel.Focus();
                else
                    txtSenha.Focus();
                return false;
            }

            // Validações adicionais
            if (usuario.Length < 3)
            {
                MostrarAviso("Nome de usuário deve ter pelo menos 3 caracteres.");
                txtUsuario.Focus();
                return false;
            }

            if (senha.Length < 6)
            {
                MostrarAviso("Palavra-passe deve ter pelo menos 6 caracteres.");
                if (senhaVisivel)
                    txtSenhaVisivel.Focus();
                else
                    txtSenha.Focus();
                return false;
            }

            return true;
        }

        #endregion

        #region Métodos de Autenticação

        /// <summary>
        /// Autentica o usuário no banco de dados de forma assíncrona
        /// </summary>
        /// <param name="usuario">Nome de usuário</param>
        /// <param name="senha">Senha</param>
        /// <returns>Usuário autenticado ou null</returns>
        private async Task<Usuario> AutenticarUsuarioAsync(string usuario, string senha)
        {
            return await Task.Run(() =>
            {
                try
                {
                    string query = @"
                        SELECT Id, Nome, Usuario, Email, Senha, NivelAcesso, Ativo, 
                               ContaBloqueada, TentativasLogin, DataUltimoLogin,
                               Telefone, Endereco, Cidade, Provincia, IdiomaPreferido
                        FROM Usuarios 
                        WHERE Usuario = @Usuario AND Senha = @Senha";

                    SqlParameter[] parameters = {
                        new SqlParameter("@Usuario", usuario),
                        new SqlParameter("@Senha", senha)
                    };

                    DataTable dt = DatabaseConnection.ExecuteQuery(query, parameters);

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        
                        // Verificar se usuário está ativo
                        bool ativo = Convert.ToBoolean(row["Ativo"]);
                        if (!ativo)
                        {
                            return null; // Usuário inativo
                        }

                        // Verificar se conta está bloqueada
                        bool contaBloqueada = Convert.ToBoolean(row["ContaBloqueada"]);
                        if (contaBloqueada)
                        {
                            return null; // Conta bloqueada
                        }

                        // Criar objeto usuário
                        var usuarioAutenticado = new Usuario
                        {
                            Id = Convert.ToInt32(row["Id"]),
                            Nome = row["Nome"].ToString(),
                            NomeUsuario = row["Usuario"].ToString(),
                            Email = row["Email"]?.ToString(),
                            Senha = row["Senha"].ToString(),
                            NivelAcesso = row["NivelAcesso"].ToString(),
                            Ativo = ativo,
                            ContaBloqueada = contaBloqueada,
                            TentativasLogin = Convert.ToInt32(row["TentativasLogin"]),
                            DataUltimoLogin = row["DataUltimoLogin"] as DateTime?,
                            Telefone = row["Telefone"]?.ToString(),
                            Endereco = row["Endereco"]?.ToString(),
                            Cidade = row["Cidade"]?.ToString() ?? "Luanda",
                            Provincia = row["Provincia"]?.ToString() ?? "Luanda",
                            IdiomaPreferido = row["IdiomaPreferido"]?.ToString() ?? "pt-AO"
                        };

                        return usuarioAutenticado;
                    }

                    return null; // Usuário não encontrado ou senha incorreta
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erro na autenticação: {ex.Message}");
                    return null;
                }
            });
        }

        /// <summary>
        /// Processa login bem-sucedido
        /// </summary>
        /// <param name="usuario">Usuário autenticado</param>
        private async Task ProcessarLoginSucesso(Usuario usuario)
        {
            try
            {
                // Atualizar último login no banco
                await AtualizarUltimoLoginAsync(usuario.Id);

                // Definir usuário logado
                UsuarioLogado = usuario;

                // Salvar dados se solicitado
                if (chkLembrar.IsChecked == true)
                {
                    SalvarDadosLogin(usuario.NomeUsuario);
                }

                // Mostrar mensagem de boas-vindas
                MessageBox.Show(
                    $"Bem-vindo ao Sistema de Vendas Angola!\n\n" +
                    $"Usuário: {usuario.NomeExibicao}\n" +
                    $"Nível: {usuario.NivelAcesso}\n" +
                    $"Cidade: {usuario.Cidade}, {usuario.Provincia}",
                    "Login Realizado com Sucesso",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                // Abrir janela principal
                var mainWindow = new MainWindow();
                mainWindow.Show();
                
                // Fechar janela de login
                this.Close();
            }
            catch (Exception ex)
            {
                MostrarErro($"Erro ao processar login: {ex.Message}");
            }
        }

        /// <summary>
        /// Processa falha no login
        /// </summary>
        /// <param name="usuario">Nome de usuário que tentou fazer login</param>
        private void ProcessarLoginFalha(string usuario)
        {
            tentativasAtual++;
            
            // Incrementar tentativas no banco se usuário existir
            IncrementarTentativasLogin(usuario);

            int tentativasRestantes = MAX_TENTATIVAS_LOGIN - tentativasAtual;
            
            if (tentativasRestantes > 0)
            {
                MostrarErro(
                    $"Nome de usuário ou palavra-passe incorretos.\n\n" +
                    $"Tentativas restantes: {tentativasRestantes}\n\n" +
                    "Contas de demonstração:\n" +
                    "• admin / admin123 (Administrador)\n" +
                    "• gerente / gerente123 (Gerente)\n" +
                    "• vendedor / venda123 (Vendedor)"
                );
            }
            else
            {
                MostrarErro(
                    "Limite de tentativas excedido!\n\n" +
                    "Por segurança, reinicie a aplicação para tentar novamente."
                );
                
                // Desabilitar botão de login
                btnLogin.IsEnabled = false;
            }

            // Limpar senha
            txtSenha.Password = "";
            txtSenhaVisivel.Text = "";
            
            // Focar no campo apropriado
            if (string.IsNullOrEmpty(txtUsuario.Text))
                txtUsuario.Focus();
            else if (senhaVisivel)
                txtSenhaVisivel.Focus();
            else
                txtSenha.Focus();
        }

        /// <summary>
        /// Atualiza a data do último login no banco
        /// </summary>
        /// <param name="usuarioId">ID do usuário</param>
        private async Task AtualizarUltimoLoginAsync(int usuarioId)
        {
            await Task.Run(() =>
            {
                try
                {
                    string query = @"
                        UPDATE Usuarios 
                        SET DataUltimoLogin = @DataLogin, 
                            TentativasLogin = 0
                        WHERE Id = @Id";

                    SqlParameter[] parameters = {
                        new SqlParameter("@DataLogin", DateTime.Now),
                        new SqlParameter("@Id", usuarioId)
                    };

                    DatabaseConnection.ExecuteNonQuery(query, parameters);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erro ao atualizar último login: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// Incrementa o contador de tentativas de login no banco
        /// </summary>
        /// <param name="usuario">Nome de usuário</param>
        private void IncrementarTentativasLogin(string usuario)
        {
            try
            {
                string query = @"
                    UPDATE Usuarios 
                    SET TentativasLogin = TentativasLogin + 1,
                        ContaBloqueada = CASE 
                            WHEN TentativasLogin + 1 >= 10 THEN 1 
                            ELSE ContaBloqueada 
                        END,
                        DataBloqueio = CASE 
                            WHEN TentativasLogin + 1 >= 10 THEN GETDATE() 
                            ELSE DataBloqueio 
                        END
                    WHERE Usuario = @Usuario";

                SqlParameter[] parameters = {
                    new SqlParameter("@Usuario", usuario)
                };

                DatabaseConnection.ExecuteNonQuery(query, parameters);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao incrementar tentativas: {ex.Message}");
            }
        }

        #endregion

        #region Métodos de Conexão

        /// <summary>
        /// Verifica a conexão com o banco de forma assíncrona
        /// </summary>
        private async void VerificarConexaoAsync()
        {
            AtualizarStatusConexao(false, "Verificando conexão...");
            
            try
            {
                bool conectado = await DatabaseConnection.TestConnectionAsync();
                
                if (conectado)
                {
                    AtualizarStatusConexao(true, "Conectado");
                    
                    // Validar estrutura do banco
                    bool estruturaOk = DatabaseConnection.ValidateDatabaseStructure();
                    if (!estruturaOk)
                    {
                        AtualizarStatusConexao(false, "Estrutura incompleta");
                    }
                }
                else
                {
                    AtualizarStatusConexao(false, "Sem conexão");
                }
            }
            catch (Exception ex)
            {
                AtualizarStatusConexao(false, "Erro de conexão");
                System.Diagnostics.Debug.WriteLine($"Erro ao verificar conexão: {ex.Message}");
            }
        }

        #endregion

        #region Métodos de Persistência

        /// <summary>
        /// Salva os dados de login para lembrar na próxima vez
        /// </summary>
        /// <param name="usuario">Nome de usuário</param>
        private void SalvarDadosLogin(string usuario)
        {
            try
            {
                Properties.Settings.Default.UltimoUsuario = usuario;
                Properties.Settings.Default.LembrarUsuario = true;
                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao salvar dados: {ex.Message}");
            }
        }

        /// <summary>
        /// Carrega os dados salvos anteriormente
        /// </summary>
        private void CarregarDadosSalvos()
        {
            try
            {
                if (Properties.Settings.Default.LembrarUsuario)
                {
                    txtUsuario.Text = Properties.Settings.Default.UltimoUsuario ?? "";
                    chkLembrar.IsChecked = true;
                    
                    // Se há usuário salvo, focar na senha
                    if (!string.IsNullOrEmpty(txtUsuario.Text))
                    {
                        if (senhaVisivel)
                            txtSenhaVisivel.Focus();
                        else
                            txtSenha.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar dados salvos: {ex.Message}");
            }
        }

        #endregion

        #region Métodos de Mensagens

        /// <summary>
        /// Exibe uma mensagem de erro
        /// </summary>
        /// <param name="mensagem">Mensagem de erro</param>
        private void MostrarErro(string mensagem)
        {
            MessageBox.Show(
                mensagem,
                "Erro - Sistema de Vendas Angola",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }

        /// <summary>
        /// Exibe uma mensagem de aviso
        /// </summary>
        /// <param name="mensagem">Mensagem de aviso</param>
        private void MostrarAviso(string mensagem)
        {
            MessageBox.Show(
                mensagem,
                "Aviso - Sistema de Vendas Angola",
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );
        }

        /// <summary>
        /// Exibe uma mensagem informativa
        /// </summary>
        /// <param name="mensagem">Mensagem informativa</param>
        private void MostrarInfo(string mensagem)
        {
            MessageBox.Show(
                mensagem,
                "Informação - Sistema de Vendas Angola",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }

        #endregion
    }
}