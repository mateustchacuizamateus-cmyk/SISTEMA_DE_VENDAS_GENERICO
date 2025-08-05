using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Windows;

namespace SISTEMA_DE_VENDAS_GENERICO.Data
{
    public class DatabaseConnection
    {
        private static string connectionString = null;
        private static bool connectionTested = false;

        private static string GetConnectionString()
        {
            if (connectionString == null)
            {
                try
                {
                    // Primeiro tenta LocalDB
                    connectionString = ConfigurationManager.ConnectionStrings["SistemaVendasConnection"].ConnectionString;
                }
                catch
                {
                    try
                    {
                        // Se falhar, tenta SQL Server
                        connectionString = ConfigurationManager.ConnectionStrings["SistemaVendasConnectionSQLServer"].ConnectionString;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Erro ao carregar string de conexão: {ex.Message}", "Erro de Configuração", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=SistemaVendas;Integrated Security=True;Connect Timeout=30";
                    }
                }
            }
            return connectionString;
        }

        public static SqlConnection GetConnection()
        {
            try
            {
                return new SqlConnection(GetConnectionString());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao criar conexão: {ex.Message}", "Erro de Conexão", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        public static bool TestConnection()
        {
            if (connectionTested)
                return true;

            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    connectionTested = true;
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao testar conexão com o banco de dados:\n\n{ex.Message}\n\nVerifique se:\n• O SQL Server está rodando\n• O banco 'SistemaVendas' existe\n• A string de conexão está correta", 
                    "Erro de Conexão", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public static DataTable ExecuteQuery(string query, SqlParameter[] parameters = null)
        {
            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }

                        DataTable dataTable = new DataTable();
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        adapter.Fill(dataTable);
                        return dataTable;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao executar consulta:\n{ex.Message}", "Erro de Banco de Dados", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        public static int ExecuteNonQuery(string query, SqlParameter[] parameters = null)
        {
            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }

                        return command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao executar comando:\n{ex.Message}", "Erro de Banco de Dados", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        public static object ExecuteScalar(string query, SqlParameter[] parameters = null)
        {
            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }

                        return command.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao executar consulta escalar:\n{ex.Message}", "Erro de Banco de Dados", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        public static bool CreateDatabaseIfNotExists()
        {
            try
            {
                // String de conexão para o servidor master
                string masterConnectionString = GetConnectionString().Replace("Initial Catalog=SistemaVendas", "Initial Catalog=master");
                
                using (var connection = new SqlConnection(masterConnectionString))
                {
                    connection.Open();
                    
                    // Verificar se o banco existe
                    string checkDbQuery = "SELECT COUNT(*) FROM sys.databases WHERE name = 'SistemaVendas'";
                    using (var command = new SqlCommand(checkDbQuery, connection))
                    {
                        int count = (int)command.ExecuteScalar();
                        if (count == 0)
                        {
                            // Criar o banco
                            string createDbQuery = "CREATE DATABASE SistemaVendas";
                            using (var createCommand = new SqlCommand(createDbQuery, connection))
                            {
                                createCommand.ExecuteNonQuery();
                                MessageBox.Show("Banco de dados 'SistemaVendas' criado com sucesso!", "Sucesso", 
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao criar banco de dados:\n{ex.Message}", "Erro", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }
} 