using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SISTEMA_DE_VENDAS_GENERICO.Data;

namespace SISTEMA_DE_VENDAS_GENERICO.Views
{
    public partial class ConfiguracoesPage : Page
    {
        public ConfiguracoesPage()
        {
            InitializeComponent();
            CarregarConfiguracoes();
            CarregarFormasPagamento();
        }

        private void CarregarConfiguracoes()
        {
            try
            {
                string query = "SELECT TOP 1 NomeEmpresa, Endereco, Telefone, Email FROM Configuracoes";
                DataTable dt = DatabaseConnection.ExecuteQuery(query);
                
                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    txtNomeEmpresa.Text = row["NomeEmpresa"]?.ToString() ?? "";
                    txtEnderecoEmpresa.Text = row["Endereco"]?.ToString() ?? "";
                    txtTelefoneEmpresa.Text = row["Telefone"]?.ToString() ?? "";
                    txtEmailEmpresa.Text = row["Email"]?.ToString() ?? "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar configurações: {ex.Message}", "Erro", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CarregarFormasPagamento()
        {
            try
            {
                string query = "SELECT Descricao FROM FormasPagamento ORDER BY Descricao";
                DataTable dt = DatabaseConnection.ExecuteQuery(query);
                
                lstFormasPagamento.Items.Clear();
                foreach (DataRow row in dt.Rows)
                {
                    lstFormasPagamento.Items.Add(row["Descricao"].ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar formas de pagamento: {ex.Message}", "Erro", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnSalvarEmpresa_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Verificar se já existe configuração
                string queryVerificar = "SELECT COUNT(*) FROM Configuracoes";
                int count = Convert.ToInt32(DatabaseConnection.ExecuteScalar(queryVerificar));
                
                string query;
                SqlParameter[] parameters = {
                    new SqlParameter("@NomeEmpresa", txtNomeEmpresa.Text.Trim()),
                    new SqlParameter("@Endereco", string.IsNullOrWhiteSpace(txtEnderecoEmpresa.Text) ? DBNull.Value : (object)txtEnderecoEmpresa.Text.Trim()),
                    new SqlParameter("@Telefone", string.IsNullOrWhiteSpace(txtTelefoneEmpresa.Text) ? DBNull.Value : (object)txtTelefoneEmpresa.Text.Trim()),
                    new SqlParameter("@Email", string.IsNullOrWhiteSpace(txtEmailEmpresa.Text) ? DBNull.Value : (object)txtEmailEmpresa.Text.Trim())
                };

                if (count == 0)
                {
                    // Inserir nova configuração
                    query = @"
                        INSERT INTO Configuracoes (NomeEmpresa, Endereco, Telefone, Email)
                        VALUES (@NomeEmpresa, @Endereco, @Telefone, @Email)";
                }
                else
                {
                    // Atualizar configuração existente
                    query = @"
                        UPDATE Configuracoes 
                        SET NomeEmpresa = @NomeEmpresa, Endereco = @Endereco, 
                            Telefone = @Telefone, Email = @Email
                        WHERE Id = (SELECT TOP 1 Id FROM Configuracoes)";
                }

                DatabaseConnection.ExecuteNonQuery(query, parameters);
                
                txtStatusEmpresa.Text = "Configurações salvas com sucesso!";
                MessageBox.Show("Configurações da empresa salvas com sucesso!", "Sucesso", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar configurações: {ex.Message}", "Erro", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnAdicionarFormaPagamento_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNovaFormaPagamento.Text))
            {
                MessageBox.Show("Digite uma forma de pagamento!", "Aviso", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string novaForma = txtNovaFormaPagamento.Text.Trim();
                
                // Verificar se já existe
                string queryVerificar = "SELECT COUNT(*) FROM FormasPagamento WHERE Descricao = @Descricao";
                SqlParameter[] paramVerificar = {
                    new SqlParameter("@Descricao", novaForma)
                };
                
                int count = Convert.ToInt32(DatabaseConnection.ExecuteScalar(queryVerificar, paramVerificar));
                
                if (count > 0)
                {
                    MessageBox.Show("Esta forma de pagamento já existe!", "Aviso", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Inserir nova forma de pagamento
                string query = "INSERT INTO FormasPagamento (Descricao) VALUES (@Descricao)";
                SqlParameter[] parameters = {
                    new SqlParameter("@Descricao", novaForma)
                };

                DatabaseConnection.ExecuteNonQuery(query, parameters);
                
                txtNovaFormaPagamento.Text = "";
                CarregarFormasPagamento();
                
                MessageBox.Show("Forma de pagamento adicionada com sucesso!", "Sucesso", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao adicionar forma de pagamento: {ex.Message}", "Erro", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnBackup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Arquivo de Backup (*.bak)|*.bak|Todos os arquivos (*.*)|*.*",
                    FileName = $"SistemaVendas_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.bak",
                    Title = "Salvar Backup do Banco de Dados"
                };

                if (dialog.ShowDialog() == true)
                {
                    string backupPath = dialog.FileName;
                    
                    // Comando de backup do SQL Server
                    string query = $@"
                        BACKUP DATABASE SistemaVendas 
                        TO DISK = '{backupPath}' 
                        WITH FORMAT, 
                        MEDIANAME = 'SistemaVendasBackup',
                        NAME = 'SistemaVendas-Full Database Backup'";

                    DatabaseConnection.ExecuteNonQuery(query);
                    
                    txtUltimoBackup.Text = $"Último Backup: {DateTime.Now:dd/MM/yyyy HH:mm}";
                    
                    MessageBox.Show($"Backup realizado com sucesso!\n\nArquivo: {backupPath}", "Sucesso", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao fazer backup: {ex.Message}\n\n" +
                              "Certifique-se de que o SQL Server tem permissões para escrever no diretório selecionado.", 
                              "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnRestaurar_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "ATENÇÃO: Restaurar um backup irá substituir todos os dados atuais!\n\n" +
                "Certifique-se de fazer um backup antes de continuar.\n\n" +
                "Deseja continuar?", 
                "Confirmação", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var dialog = new Microsoft.Win32.OpenFileDialog
                    {
                        Filter = "Arquivo de Backup (*.bak)|*.bak|Todos os arquivos (*.*)|*.*",
                        Title = "Selecionar Arquivo de Backup"
                    };

                    if (dialog.ShowDialog() == true)
                    {
                        string backupPath = dialog.FileName;
                        
                        // Verificar se o arquivo existe
                        if (!File.Exists(backupPath))
                        {
                            MessageBox.Show("Arquivo de backup não encontrado!", "Erro", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        // Comando de restauração do SQL Server
                        string query = $@"
                            USE master;
                            ALTER DATABASE SistemaVendas SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                            RESTORE DATABASE SistemaVendas 
                            FROM DISK = '{backupPath}' 
                            WITH REPLACE;
                            ALTER DATABASE SistemaVendas SET MULTI_USER;";

                        DatabaseConnection.ExecuteNonQuery(query);
                        
                        MessageBox.Show("Backup restaurado com sucesso!\n\n" +
                                      "O sistema será reiniciado para aplicar as mudanças.", 
                                      "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                        
                        // Reiniciar aplicação
                        Application.Current.Shutdown();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao restaurar backup: {ex.Message}\n\n" +
                                  "Certifique-se de que o arquivo é válido e o SQL Server tem permissões adequadas.", 
                                  "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
} 