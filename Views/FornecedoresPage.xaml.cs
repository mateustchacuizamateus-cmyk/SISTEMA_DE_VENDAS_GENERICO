using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using SISTEMA_DE_VENDAS_GENERICO.Data;
using SISTEMA_DE_VENDAS_GENERICO.Models;

namespace SISTEMA_DE_VENDAS_GENERICO.Views
{
    public partial class FornecedoresPage : Page
    {
        private List<Fornecedor> fornecedores;
        private Fornecedor fornecedorSelecionado;
        private bool modoEdicao = false;

        public FornecedoresPage()
        {
            InitializeComponent();
            CarregarFornecedores();
            LimparFormulario();
        }

        private void CarregarFornecedores()
        {
            try
            {
                string query = @"
                    SELECT Id, Nome, Telefone, Email, Endereco
                    FROM Fornecedores 
                    ORDER BY Nome";

                DataTable dt = DatabaseConnection.ExecuteQuery(query);
                
                fornecedores = new List<Fornecedor>();
                foreach (DataRow row in dt.Rows)
                {
                    var fornecedor = new Fornecedor
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        Nome = row["Nome"].ToString(),
                        Telefone = row["Telefone"].ToString(),
                        Email = row["Email"].ToString(),
                        Endereco = row["Endereco"].ToString()
                    };
                    fornecedores.Add(fornecedor);
                }

                dgFornecedores.ItemsSource = fornecedores;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar fornecedores: {ex.Message}", "Erro", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void dgFornecedores_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            fornecedorSelecionado = dgFornecedores.SelectedItem as Fornecedor;
            btnEditarFornecedor.IsEnabled = fornecedorSelecionado != null;
            btnExcluirFornecedor.IsEnabled = fornecedorSelecionado != null;
        }

        private void btnNovoFornecedor_Click(object sender, RoutedEventArgs e)
        {
            modoEdicao = false;
            fornecedorSelecionado = null;
            LimparFormulario();
            txtStatus.Text = "Novo fornecedor - Preencha os dados e clique em Salvar";
            txtNomeFornecedor.Focus();
        }

        private void btnEditarFornecedor_Click(object sender, RoutedEventArgs e)
        {
            if (fornecedorSelecionado == null)
            {
                MessageBox.Show("Selecione um fornecedor para editar!", "Aviso", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            modoEdicao = true;
            PreencherFormulario(fornecedorSelecionado);
            txtStatus.Text = $"Editando fornecedor: {fornecedorSelecionado.Nome}";
            txtNomeFornecedor.Focus();
        }

        private void btnExcluirFornecedor_Click(object sender, RoutedEventArgs e)
        {
            if (fornecedorSelecionado == null)
            {
                MessageBox.Show("Selecione um fornecedor para excluir!", "Aviso", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Deseja realmente excluir o fornecedor '{fornecedorSelecionado.Nome}'?\n\n" +
                "Esta ação não pode ser desfeita!", 
                "Confirmação", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    string query = "DELETE FROM Fornecedores WHERE Id = @Id";
                    SqlParameter[] parameters = {
                        new SqlParameter("@Id", fornecedorSelecionado.Id)
                    };

                    DatabaseConnection.ExecuteNonQuery(query, parameters);
                    
                    MessageBox.Show("Fornecedor excluído com sucesso!", "Sucesso", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    CarregarFornecedores();
                    LimparFormulario();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao excluir fornecedor: {ex.Message}", "Erro", 
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
                    AtualizarFornecedor();
                }
                else
                {
                    InserirFornecedor();
                }

                CarregarFornecedores();
                LimparFormulario();
                txtStatus.Text = "Fornecedor salvo com sucesso!";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar fornecedor: {ex.Message}", "Erro", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            LimparFormulario();
        }

        private bool ValidarFormulario()
        {
            if (string.IsNullOrWhiteSpace(txtNomeFornecedor.Text))
            {
                MessageBox.Show("Nome do fornecedor é obrigatório!", "Validação", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtNomeFornecedor.Focus();
                return false;
            }

            // Validar email se fornecido
            if (!string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                if (!IsValidEmail(txtEmail.Text))
                {
                    MessageBox.Show("Email inválido!", "Validação", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtEmail.Focus();
                    return false;
                }
            }

            return true;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private void InserirFornecedor()
        {
            string query = @"
                INSERT INTO Fornecedores (Nome, Telefone, Email, Endereco)
                VALUES (@Nome, @Telefone, @Email, @Endereco)";

            SqlParameter[] parameters = {
                new SqlParameter("@Nome", txtNomeFornecedor.Text.Trim()),
                new SqlParameter("@Telefone", string.IsNullOrWhiteSpace(txtTelefone.Text) ? DBNull.Value : (object)txtTelefone.Text.Trim()),
                new SqlParameter("@Email", string.IsNullOrWhiteSpace(txtEmail.Text) ? DBNull.Value : (object)txtEmail.Text.Trim()),
                new SqlParameter("@Endereco", string.IsNullOrWhiteSpace(txtEndereco.Text) ? DBNull.Value : (object)txtEndereco.Text.Trim())
            };

            DatabaseConnection.ExecuteNonQuery(query, parameters);
        }

        private void AtualizarFornecedor()
        {
            string query = @"
                UPDATE Fornecedores 
                SET Nome = @Nome, Telefone = @Telefone, Email = @Email, Endereco = @Endereco
                WHERE Id = @Id";

            SqlParameter[] parameters = {
                new SqlParameter("@Id", fornecedorSelecionado.Id),
                new SqlParameter("@Nome", txtNomeFornecedor.Text.Trim()),
                new SqlParameter("@Telefone", string.IsNullOrWhiteSpace(txtTelefone.Text) ? DBNull.Value : (object)txtTelefone.Text.Trim()),
                new SqlParameter("@Email", string.IsNullOrWhiteSpace(txtEmail.Text) ? DBNull.Value : (object)txtEmail.Text.Trim()),
                new SqlParameter("@Endereco", string.IsNullOrWhiteSpace(txtEndereco.Text) ? DBNull.Value : (object)txtEndereco.Text.Trim())
            };

            DatabaseConnection.ExecuteNonQuery(query, parameters);
        }

        private void PreencherFormulario(Fornecedor fornecedor)
        {
            txtNomeFornecedor.Text = fornecedor.Nome;
            txtTelefone.Text = fornecedor.Telefone;
            txtEmail.Text = fornecedor.Email;
            txtEndereco.Text = fornecedor.Endereco;
        }

        private void LimparFormulario()
        {
            txtNomeFornecedor.Text = "";
            txtTelefone.Text = "";
            txtEmail.Text = "";
            txtEndereco.Text = "";
            txtStatus.Text = "";
            modoEdicao = false;
            fornecedorSelecionado = null;
        }
    }

    // Classe para Fornecedor
    public class Fornecedor
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Telefone { get; set; }
        public string Email { get; set; }
        public string Endereco { get; set; }
    }
}