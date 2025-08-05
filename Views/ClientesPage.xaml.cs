using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SISTEMA_DE_VENDAS_GENERICO.Models;
using SISTEMA_DE_VENDAS_GENERICO.Data;

namespace SISTEMA_DE_VENDAS_GENERICO.Views
{
    public partial class ClientesPage : Page
    {
        private List<Cliente> clientes;
        private Cliente clienteSelecionado;
        private bool modoEdicao = false;

        public ClientesPage()
        {
            InitializeComponent();
            CarregarClientes();
            LimparFormulario();
        }

        private void CarregarClientes()
        {
            try
            {
                string query = @"
                    SELECT Id, Nome, Telefone, Email, Endereco, DataCadastro
                    FROM Clientes 
                    ORDER BY Nome";

                DataTable dt = DatabaseConnection.ExecuteQuery(query);
                
                clientes = new List<Cliente>();
                foreach (DataRow row in dt.Rows)
                {
                    var cliente = new Cliente
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        Nome = row["Nome"].ToString(),
                        Telefone = row["Telefone"].ToString(),
                        Email = row["Email"].ToString(),
                        Endereco = row["Endereco"].ToString(),
                        DataCadastro = Convert.ToDateTime(row["DataCadastro"])
                    };
                    clientes.Add(cliente);
                }

                dgClientes.ItemsSource = clientes;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar clientes: {ex.Message}", "Erro", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void dgClientes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            clienteSelecionado = dgClientes.SelectedItem as Cliente;
            btnEditarCliente.IsEnabled = clienteSelecionado != null;
            btnExcluirCliente.IsEnabled = clienteSelecionado != null;
        }

        private void btnNovoCliente_Click(object sender, RoutedEventArgs e)
        {
            modoEdicao = false;
            clienteSelecionado = null;
            LimparFormulario();
            txtStatus.Text = "Novo cliente - Preencha os dados e clique em Salvar";
        }

        private void btnEditarCliente_Click(object sender, RoutedEventArgs e)
        {
            if (clienteSelecionado == null)
            {
                MessageBox.Show("Selecione um cliente para editar!", "Aviso", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            modoEdicao = true;
            PreencherFormulario(clienteSelecionado);
            txtStatus.Text = $"Editando cliente: {clienteSelecionado.Nome}";
        }

        private void btnExcluirCliente_Click(object sender, RoutedEventArgs e)
        {
            if (clienteSelecionado == null)
            {
                MessageBox.Show("Selecione um cliente para excluir!", "Aviso", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Deseja realmente excluir o cliente '{clienteSelecionado.Nome}'?\n\n" +
                "Esta ação não pode ser desfeita!", 
                "Confirmação", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Verificar se o cliente tem vendas associadas
                    string queryVerificar = "SELECT COUNT(*) FROM Vendas WHERE IdCliente = @Id";
                    SqlParameter[] paramVerificar = {
                        new SqlParameter("@Id", clienteSelecionado.Id)
                    };

                    int vendasAssociadas = Convert.ToInt32(DatabaseConnection.ExecuteScalar(queryVerificar, paramVerificar));

                    if (vendasAssociadas > 0)
                    {
                        MessageBox.Show(
                            $"Não é possível excluir o cliente '{clienteSelecionado.Nome}' porque existem {vendasAssociadas} venda(s) associada(s).\n\n" +
                            "Para excluir o cliente, primeiro remova todas as vendas associadas.", 
                            "Aviso", 
                            MessageBoxButton.OK, 
                            MessageBoxImage.Warning);
                        return;
                    }

                    string query = "DELETE FROM Clientes WHERE Id = @Id";
                    SqlParameter[] parameters = {
                        new SqlParameter("@Id", clienteSelecionado.Id)
                    };

                    DatabaseConnection.ExecuteNonQuery(query, parameters);
                    
                    MessageBox.Show("Cliente excluído com sucesso!", "Sucesso", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    CarregarClientes();
                    LimparFormulario();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao excluir cliente: {ex.Message}", "Erro", 
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
                    AtualizarCliente();
                }
                else
                {
                    InserirCliente();
                }

                CarregarClientes();
                LimparFormulario();
                txtStatus.Text = "Cliente salvo com sucesso!";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar cliente: {ex.Message}", "Erro", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            LimparFormulario();
        }

        private bool ValidarFormulario()
        {
            if (string.IsNullOrWhiteSpace(txtNomeCliente.Text))
            {
                MessageBox.Show("Nome do cliente é obrigatório!", "Validação", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtNomeCliente.Focus();
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

        private void InserirCliente()
        {
            string query = @"
                INSERT INTO Clientes (Nome, Telefone, Email, Endereco, DataCadastro)
                VALUES (@Nome, @Telefone, @Email, @Endereco, @DataCadastro)";

            SqlParameter[] parameters = {
                new SqlParameter("@Nome", txtNomeCliente.Text.Trim()),
                new SqlParameter("@Telefone", txtTelefone.Text.Trim()),
                new SqlParameter("@Email", string.IsNullOrWhiteSpace(txtEmail.Text) ? DBNull.Value : (object)txtEmail.Text.Trim()),
                new SqlParameter("@Endereco", string.IsNullOrWhiteSpace(txtEndereco.Text) ? DBNull.Value : (object)txtEndereco.Text.Trim()),
                new SqlParameter("@DataCadastro", DateTime.Now)
            };

            DatabaseConnection.ExecuteNonQuery(query, parameters);
        }

        private void AtualizarCliente()
        {
            string query = @"
                UPDATE Clientes 
                SET Nome = @Nome, Telefone = @Telefone, Email = @Email, Endereco = @Endereco
                WHERE Id = @Id";

            SqlParameter[] parameters = {
                new SqlParameter("@Id", clienteSelecionado.Id),
                new SqlParameter("@Nome", txtNomeCliente.Text.Trim()),
                new SqlParameter("@Telefone", txtTelefone.Text.Trim()),
                new SqlParameter("@Email", string.IsNullOrWhiteSpace(txtEmail.Text) ? DBNull.Value : (object)txtEmail.Text.Trim()),
                new SqlParameter("@Endereco", string.IsNullOrWhiteSpace(txtEndereco.Text) ? DBNull.Value : (object)txtEndereco.Text.Trim())
            };

            DatabaseConnection.ExecuteNonQuery(query, parameters);
        }

        private void PreencherFormulario(Cliente cliente)
        {
            txtNomeCliente.Text = cliente.Nome;
            txtTelefone.Text = cliente.Telefone;
            txtEmail.Text = cliente.Email;
            txtEndereco.Text = cliente.Endereco;
        }

        private void LimparFormulario()
        {
            txtNomeCliente.Text = "";
            txtTelefone.Text = "";
            txtEmail.Text = "";
            txtEndereco.Text = "";
            txtStatus.Text = "";
            modoEdicao = false;
            clienteSelecionado = null;
        }
    }
} 