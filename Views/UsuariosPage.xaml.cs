using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SISTEMA_DE_VENDAS_GENERICO.Data;
using SISTEMA_DE_VENDAS_GENERICO.Models;

namespace SISTEMA_DE_VENDAS_GENERICO.Views
{
    public partial class UsuariosPage : Page
    {
        private List<Usuario> usuarios;
        private Usuario usuarioSelecionado;
        private bool modoEdicao = false;

        public UsuariosPage()
        {
            InitializeComponent();
            CarregarUsuarios();
            LimparFormulario();
        }

        private void CarregarUsuarios()
        {
            try
            {
                string query = @"
                    SELECT Id, Nome, Usuario, Senha, NivelAcesso, Ativo, DataCadastro
                    FROM Usuarios 
                    ORDER BY Nome";

                DataTable dt = DatabaseConnection.ExecuteQuery(query);
                
                usuarios = new List<Usuario>();
                foreach (DataRow row in dt.Rows)
                {
                    var usuario = new Usuario
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        Nome = row["Nome"].ToString(),
                        NomeUsuario = row["Usuario"].ToString(),
                        Senha = row["Senha"].ToString(),
                        NivelAcesso = row["NivelAcesso"].ToString(),
                        Ativo = Convert.ToBoolean(row["Ativo"]),
                        DataCadastro = Convert.ToDateTime(row["DataCadastro"])
                    };
                    usuarios.Add(usuario);
                }

                dgUsuarios.ItemsSource = usuarios;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar usuários: {ex.Message}", "Erro", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void dgUsuarios_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            usuarioSelecionado = dgUsuarios.SelectedItem as Usuario;
            btnEditarUsuario.IsEnabled = usuarioSelecionado != null;
            btnDesativarUsuario.IsEnabled = usuarioSelecionado != null;
            
            // Não permitir desativar o próprio usuário
            if (usuarioSelecionado != null && LoginWindow.UsuarioLogado != null && 
                usuarioSelecionado.Id == LoginWindow.UsuarioLogado.Id)
            {
                btnDesativarUsuario.IsEnabled = false;
            }
        }

        private void btnNovoUsuario_Click(object sender, RoutedEventArgs e)
        {
            modoEdicao = false;
            usuarioSelecionado = null;
            LimparFormulario();
            txtStatus.Text = "Novo usuário - Preencha os dados e clique em Salvar";
            txtNomeCompleto.Focus();
        }

        private void btnEditarUsuario_Click(object sender, RoutedEventArgs e)
        {
            if (usuarioSelecionado == null)
            {
                MessageBox.Show("Selecione um usuário para editar!", "Aviso", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            modoEdicao = true;
            PreencherFormulario(usuarioSelecionado);
            txtStatus.Text = $"Editando usuário: {usuarioSelecionado.Nome}";
            txtNomeCompleto.Focus();
        }

        private void btnDesativarUsuario_Click(object sender, RoutedEventArgs e)
        {
            if (usuarioSelecionado == null)
            {
                MessageBox.Show("Selecione um usuário para desativar!", "Aviso", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (usuarioSelecionado.Id == LoginWindow.UsuarioLogado.Id)
            {
                MessageBox.Show("Você não pode desativar seu próprio usuário!", "Aviso", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string acao = usuarioSelecionado.Ativo ? "desativar" : "ativar";
            var result = MessageBox.Show(
                $"Deseja realmente {acao} o usuário '{usuarioSelecionado.Nome}'?", 
                "Confirmação", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    string query = "UPDATE Usuarios SET Ativo = @Ativo WHERE Id = @Id";
                    SqlParameter[] parameters = {
                        new SqlParameter("@Ativo", !usuarioSelecionado.Ativo),
                        new SqlParameter("@Id", usuarioSelecionado.Id)
                    };

                    DatabaseConnection.ExecuteNonQuery(query, parameters);
                    
                    MessageBox.Show($"Usuário {acao}do com sucesso!", "Sucesso", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    CarregarUsuarios();
                    LimparFormulario();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao {acao} usuário: {ex.Message}", "Erro", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnSalvar_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarFormulario())
                return;

            try
            {
                if (modoEdicao)
                {
                    AtualizarUsuario();
                }
                else
                {
                    InserirUsuario();
                }

                CarregarUsuarios();
                LimparFormulario();
                txtStatus.Text = "Usuário salvo com sucesso!";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar usuário: {ex.Message}", "Erro", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            LimparFormulario();
        }

        private bool ValidarFormulario()
        {
            if (string.IsNullOrWhiteSpace(txtNomeCompleto.Text))
            {
                MessageBox.Show("Nome completo é obrigatório!", "Validação", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtNomeCompleto.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtNomeUsuario.Text))
            {
                MessageBox.Show("Nome de usuário é obrigatório!", "Validação", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtNomeUsuario.Focus();
                return false;
            }

            // Verificar se o nome de usuário já existe (apenas para novos usuários ou se mudou)
            if (!modoEdicao || (modoEdicao && usuarioSelecionado.NomeUsuario != txtNomeUsuario.Text))
            {
                if (VerificarUsuarioExiste(txtNomeUsuario.Text))
                {
                    MessageBox.Show("Este nome de usuário já existe!", "Validação", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtNomeUsuario.Focus();
                    return false;
                }
            }

            if (!modoEdicao && string.IsNullOrWhiteSpace(txtSenha.Password))
            {
                MessageBox.Show("Senha é obrigatória!", "Validação", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtSenha.Focus();
                return false;
            }

            if (!modoEdicao && txtSenha.Password != txtConfirmarSenha.Password)
            {
                MessageBox.Show("As senhas não coincidem!", "Validação", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtConfirmarSenha.Focus();
                return false;
            }

            if (cmbNivelAcesso.SelectedIndex == -1)
            {
                MessageBox.Show("Selecione o nível de acesso!", "Validação", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbNivelAcesso.Focus();
                return false;
            }

            return true;
        }

        private bool VerificarUsuarioExiste(string nomeUsuario)
        {
            try
            {
                string query = "SELECT COUNT(*) FROM Usuarios WHERE Usuario = @Usuario";
                SqlParameter[] parameters = {
                    new SqlParameter("@Usuario", nomeUsuario)
                };

                int count = Convert.ToInt32(DatabaseConnection.ExecuteScalar(query, parameters));
                return count > 0;
            }
            catch
            {
                return false;
            }
        }

        private void InserirUsuario()
        {
            string nivelAcesso = (cmbNivelAcesso.SelectedItem as ComboBoxItem)?.Content.ToString();

            string query = @"
                INSERT INTO Usuarios (Nome, Usuario, Senha, NivelAcesso, Ativo, DataCadastro)
                VALUES (@Nome, @Usuario, @Senha, @NivelAcesso, @Ativo, @DataCadastro)";

            SqlParameter[] parameters = {
                new SqlParameter("@Nome", txtNomeCompleto.Text.Trim()),
                new SqlParameter("@Usuario", txtNomeUsuario.Text.Trim()),
                new SqlParameter("@Senha", txtSenha.Password),
                new SqlParameter("@NivelAcesso", nivelAcesso),
                new SqlParameter("@Ativo", chkUsuarioAtivo.IsChecked ?? true),
                new SqlParameter("@DataCadastro", DateTime.Now)
            };

            DatabaseConnection.ExecuteNonQuery(query, parameters);
        }

        private void AtualizarUsuario()
        {
            string nivelAcesso = (cmbNivelAcesso.SelectedItem as ComboBoxItem)?.Content.ToString();

            string query;
            SqlParameter[] parameters;

            if (!string.IsNullOrWhiteSpace(txtSenha.Password))
            {
                // Atualizar com nova senha
                query = @"
                    UPDATE Usuarios 
                    SET Nome = @Nome, Usuario = @Usuario, Senha = @Senha, 
                        NivelAcesso = @NivelAcesso, Ativo = @Ativo
                    WHERE Id = @Id";

                parameters = new SqlParameter[] {
                    new SqlParameter("@Id", usuarioSelecionado.Id),
                    new SqlParameter("@Nome", txtNomeCompleto.Text.Trim()),
                    new SqlParameter("@Usuario", txtNomeUsuario.Text.Trim()),
                    new SqlParameter("@Senha", txtSenha.Password),
                    new SqlParameter("@NivelAcesso", nivelAcesso),
                    new SqlParameter("@Ativo", chkUsuarioAtivo.IsChecked ?? true)
                };
            }
            else
            {
                // Atualizar sem alterar a senha
                query = @"
                    UPDATE Usuarios 
                    SET Nome = @Nome, Usuario = @Usuario, NivelAcesso = @NivelAcesso, Ativo = @Ativo
                    WHERE Id = @Id";

                parameters = new SqlParameter[] {
                    new SqlParameter("@Id", usuarioSelecionado.Id),
                    new SqlParameter("@Nome", txtNomeCompleto.Text.Trim()),
                    new SqlParameter("@Usuario", txtNomeUsuario.Text.Trim()),
                    new SqlParameter("@NivelAcesso", nivelAcesso),
                    new SqlParameter("@Ativo", chkUsuarioAtivo.IsChecked ?? true)
                };
            }

            DatabaseConnection.ExecuteNonQuery(query, parameters);
        }

        private void PreencherFormulario(Usuario usuario)
        {
            txtNomeCompleto.Text = usuario.Nome;
            txtNomeUsuario.Text = usuario.NomeUsuario;
            txtSenha.Password = "";
            txtConfirmarSenha.Password = "";
            chkUsuarioAtivo.IsChecked = usuario.Ativo;

            // Selecionar nível de acesso
            foreach (ComboBoxItem item in cmbNivelAcesso.Items)
            {
                if (item.Content.ToString() == usuario.NivelAcesso)
                {
                    cmbNivelAcesso.SelectedItem = item;
                    break;
                }
            }
        }

        private void LimparFormulario()
        {
            txtNomeCompleto.Text = "";
            txtNomeUsuario.Text = "";
            txtSenha.Password = "";
            txtConfirmarSenha.Password = "";
            chkUsuarioAtivo.IsChecked = true;
            cmbNivelAcesso.SelectedIndex = -1;
            txtStatus.Text = "";
            modoEdicao = false;
            usuarioSelecionado = null;
        }
    }
}