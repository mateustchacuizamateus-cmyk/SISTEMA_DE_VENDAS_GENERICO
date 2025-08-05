using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Globalization;
using System.Threading.Tasks;

namespace SISTEMA_DE_VENDAS_GENERICO.Data
{
    /// <summary>
    /// Classe responsável pela conexão e operações com o banco de dados
    /// Otimizada para o mercado angolano com tratamento robusto de erros
    /// </summary>
    public static class DatabaseConnection
    {
        #region Propriedades Privadas
        
        /// <summary>
        /// String de conexão em cache para melhor performance
        /// </summary>
        private static string connectionString = null;
        
        /// <summary>
        /// Flag para controlar se a conexão já foi testada
        /// </summary>
        private static bool connectionTested = false;
        
        /// <summary>
        /// Timeout padrão para operações de banco (em segundos)
        /// </summary>
        private const int DEFAULT_TIMEOUT = 30;
        
        /// <summary>
        /// Número máximo de tentativas de reconexão
        /// </summary>
        private const int MAX_RETRY_ATTEMPTS = 3;
        
        #endregion

        #region Métodos de Configuração

        /// <summary>
        /// Obtém a string de conexão com fallback automático
        /// Prioriza LocalDB para desenvolvimento e SQL Server para produção
        /// </summary>
        /// <returns>String de conexão válida</returns>
        private static string GetConnectionString()
        {
            if (connectionString != null)
                return connectionString;

            try
            {
                // Primeira tentativa: LocalDB (desenvolvimento)
                var localDbConnection = ConfigurationManager.ConnectionStrings["SistemaVendasConnection"];
                if (localDbConnection != null && !string.IsNullOrEmpty(localDbConnection.ConnectionString))
                {
                    connectionString = localDbConnection.ConnectionString;
                    return connectionString;
                }
            }
            catch (Exception ex)
            {
                LogError("Erro ao carregar conexão LocalDB", ex);
            }

            try
            {
                // Segunda tentativa: SQL Server (produção)
                var sqlServerConnection = ConfigurationManager.ConnectionStrings["SistemaVendasConnectionSQLServer"];
                if (sqlServerConnection != null && !string.IsNullOrEmpty(sqlServerConnection.ConnectionString))
                {
                    connectionString = sqlServerConnection.ConnectionString;
                    return connectionString;
                }
            }
            catch (Exception ex)
            {
                LogError("Erro ao carregar conexão SQL Server", ex);
            }

            // Fallback: String de conexão padrão para Angola
            connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=SistemaVendasAngola;Integrated Security=True;Connect Timeout=30;MultipleActiveResultSets=True;Application Name=SistemaVendasAngola";
            
            LogWarning("Usando string de conexão padrão para Angola");
            return connectionString;
        }

        /// <summary>
        /// Cria uma nova conexão SQL com configurações otimizadas
        /// </summary>
        /// <returns>Conexão SQL configurada</returns>
        public static SqlConnection GetConnection()
        {
            try
            {
                var connection = new SqlConnection(GetConnectionString());
                
                // Configurar cultura para Angola (formato de números e datas)
                var angolaCulture = new CultureInfo("pt-AO");
                System.Threading.Thread.CurrentThread.CurrentCulture = angolaCulture;
                System.Threading.Thread.CurrentThread.CurrentUICulture = angolaCulture;
                
                return connection;
            }
            catch (Exception ex)
            {
                LogError("Erro ao criar conexão com banco de dados", ex);
                throw new InvalidOperationException("Não foi possível estabelecer conexão com o banco de dados. Verifique as configurações.", ex);
            }
        }

        #endregion

        #region Métodos de Teste e Validação

        /// <summary>
        /// Testa a conexão com o banco de dados de forma assíncrona
        /// Inclui validação de permissões e estrutura básica
        /// </summary>
        /// <returns>True se a conexão for bem-sucedida</returns>
        public static bool TestConnection()
        {
            if (connectionTested)
                return true;

            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    
                    // Testar se consegue executar uma consulta básica
                    using (var command = new SqlCommand("SELECT GETDATE() AS DataServidor, @@VERSION AS VersaoSQL", connection))
                    {
                        command.CommandTimeout = DEFAULT_TIMEOUT;
                        var result = command.ExecuteScalar();
                        
                        if (result != null)
                        {
                            connectionTested = true;
                            LogInfo("Conexão com banco de dados estabelecida com sucesso");
                            return true;
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                string errorMessage = GetFriendlyErrorMessage(sqlEx);
                LogError("Erro SQL ao testar conexão", sqlEx);
                
                MessageBox.Show(
                    $"Erro de conexão com o banco de dados:\n\n{errorMessage}\n\n" +
                    "Verifique se:\n" +
                    "• O SQL Server está executando\n" +
                    "• O banco 'SistemaVendasAngola' existe\n" +
                    "• As credenciais estão corretas\n" +
                    "• A rede está funcionando",
                    "Erro de Conexão - Sistema de Vendas Angola",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return false;
            }
            catch (Exception ex)
            {
                LogError("Erro geral ao testar conexão", ex);
                
                MessageBox.Show(
                    $"Erro inesperado ao conectar com o banco:\n\n{ex.Message}\n\n" +
                    "Entre em contato com o suporte técnico.",
                    "Erro Crítico - Sistema de Vendas Angola",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return false;
            }

            return false;
        }

        /// <summary>
        /// Testa a conexão de forma assíncrona para não bloquear a interface
        /// </summary>
        /// <returns>Task com resultado do teste</returns>
        public static async Task<bool> TestConnectionAsync()
        {
            return await Task.Run(() => TestConnection());
        }

        #endregion

        #region Métodos de Execução de Consultas

        /// <summary>
        /// Executa uma consulta SELECT e retorna os dados em DataTable
        /// Inclui retry automático e tratamento de erros robusto
        /// </summary>
        /// <param name="query">Consulta SQL a ser executada</param>
        /// <param name="parameters">Parâmetros da consulta (opcional)</param>
        /// <param name="timeoutSeconds">Timeout personalizado (opcional)</param>
        /// <returns>DataTable com os resultados</returns>
        public static DataTable ExecuteQuery(string query, SqlParameter[] parameters = null, int timeoutSeconds = DEFAULT_TIMEOUT)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("A consulta SQL não pode estar vazia", nameof(query));

            DataTable dataTable = new DataTable();
            int attempts = 0;

            while (attempts < MAX_RETRY_ATTEMPTS)
            {
                try
                {
                    using (var connection = GetConnection())
                    {
                        connection.Open();
                        
                        using (var command = new SqlCommand(query, connection))
                        {
                            command.CommandTimeout = timeoutSeconds;
                            
                            // Adicionar parâmetros se fornecidos
                            if (parameters != null && parameters.Length > 0)
                            {
                                command.Parameters.AddRange(parameters);
                            }

                            using (var adapter = new SqlDataAdapter(command))
                            {
                                adapter.Fill(dataTable);
                            }
                        }
                    }

                    // Se chegou até aqui, a operação foi bem-sucedida
                    LogInfo($"Consulta executada com sucesso. Registros retornados: {dataTable.Rows.Count}");
                    return dataTable;
                }
                catch (SqlException sqlEx) when (IsTransientError(sqlEx) && attempts < MAX_RETRY_ATTEMPTS - 1)
                {
                    // Erro transitório - tentar novamente
                    attempts++;
                    LogWarning($"Erro transitório na consulta (tentativa {attempts}). Tentando novamente...");
                    System.Threading.Thread.Sleep(1000 * attempts); // Backoff exponencial
                }
                catch (SqlException sqlEx)
                {
                    LogError("Erro SQL ao executar consulta", sqlEx, query);
                    throw new InvalidOperationException(GetFriendlyErrorMessage(sqlEx), sqlEx);
                }
                catch (Exception ex)
                {
                    LogError("Erro geral ao executar consulta", ex, query);
                    throw new InvalidOperationException("Erro inesperado ao executar consulta no banco de dados", ex);
                }
            }

            throw new InvalidOperationException("Não foi possível executar a consulta após múltiplas tentativas");
        }

        /// <summary>
        /// Executa comandos INSERT, UPDATE, DELETE
        /// Retorna o número de linhas afetadas
        /// </summary>
        /// <param name="query">Comando SQL a ser executado</param>
        /// <param name="parameters">Parâmetros do comando (opcional)</param>
        /// <param name="timeoutSeconds">Timeout personalizado (opcional)</param>
        /// <returns>Número de linhas afetadas</returns>
        public static int ExecuteNonQuery(string query, SqlParameter[] parameters = null, int timeoutSeconds = DEFAULT_TIMEOUT)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("O comando SQL não pode estar vazio", nameof(query));

            int attempts = 0;

            while (attempts < MAX_RETRY_ATTEMPTS)
            {
                try
                {
                    using (var connection = GetConnection())
                    {
                        connection.Open();
                        
                        using (var command = new SqlCommand(query, connection))
                        {
                            command.CommandTimeout = timeoutSeconds;
                            
                            // Adicionar parâmetros se fornecidos
                            if (parameters != null && parameters.Length > 0)
                            {
                                command.Parameters.AddRange(parameters);
                            }

                            int rowsAffected = command.ExecuteNonQuery();
                            LogInfo($"Comando executado com sucesso. Linhas afetadas: {rowsAffected}");
                            return rowsAffected;
                        }
                    }
                }
                catch (SqlException sqlEx) when (IsTransientError(sqlEx) && attempts < MAX_RETRY_ATTEMPTS - 1)
                {
                    // Erro transitório - tentar novamente
                    attempts++;
                    LogWarning($"Erro transitório no comando (tentativa {attempts}). Tentando novamente...");
                    System.Threading.Thread.Sleep(1000 * attempts);
                }
                catch (SqlException sqlEx)
                {
                    LogError("Erro SQL ao executar comando", sqlEx, query);
                    throw new InvalidOperationException(GetFriendlyErrorMessage(sqlEx), sqlEx);
                }
                catch (Exception ex)
                {
                    LogError("Erro geral ao executar comando", ex, query);
                    throw new InvalidOperationException("Erro inesperado ao executar comando no banco de dados", ex);
                }
            }

            throw new InvalidOperationException("Não foi possível executar o comando após múltiplas tentativas");
        }

        /// <summary>
        /// Executa uma consulta que retorna um único valor
        /// </summary>
        /// <param name="query">Consulta SQL</param>
        /// <param name="parameters">Parâmetros da consulta (opcional)</param>
        /// <param name="timeoutSeconds">Timeout personalizado (opcional)</param>
        /// <returns>Valor único retornado pela consulta</returns>
        public static object ExecuteScalar(string query, SqlParameter[] parameters = null, int timeoutSeconds = DEFAULT_TIMEOUT)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("A consulta SQL não pode estar vazia", nameof(query));

            int attempts = 0;

            while (attempts < MAX_RETRY_ATTEMPTS)
            {
                try
                {
                    using (var connection = GetConnection())
                    {
                        connection.Open();
                        
                        using (var command = new SqlCommand(query, connection))
                        {
                            command.CommandTimeout = timeoutSeconds;
                            
                            // Adicionar parâmetros se fornecidos
                            if (parameters != null && parameters.Length > 0)
                            {
                                command.Parameters.AddRange(parameters);
                            }

                            object result = command.ExecuteScalar();
                            LogInfo("Consulta escalar executada com sucesso");
                            return result;
                        }
                    }
                }
                catch (SqlException sqlEx) when (IsTransientError(sqlEx) && attempts < MAX_RETRY_ATTEMPTS - 1)
                {
                    // Erro transitório - tentar novamente
                    attempts++;
                    LogWarning($"Erro transitório na consulta escalar (tentativa {attempts}). Tentando novamente...");
                    System.Threading.Thread.Sleep(1000 * attempts);
                }
                catch (SqlException sqlEx)
                {
                    LogError("Erro SQL ao executar consulta escalar", sqlEx, query);
                    throw new InvalidOperationException(GetFriendlyErrorMessage(sqlEx), sqlEx);
                }
                catch (Exception ex)
                {
                    LogError("Erro geral ao executar consulta escalar", ex, query);
                    throw new InvalidOperationException("Erro inesperado ao executar consulta escalar no banco de dados", ex);
                }
            }

            throw new InvalidOperationException("Não foi possível executar a consulta escalar após múltiplas tentativas");
        }

        #endregion

        #region Métodos de Criação e Manutenção do Banco

        /// <summary>
        /// Cria o banco de dados se ele não existir
        /// Específico para o sistema de vendas angolano
        /// </summary>
        /// <returns>True se o banco foi criado ou já existe</returns>
        public static bool CreateDatabaseIfNotExists()
        {
            try
            {
                // String de conexão para o banco master
                string masterConnectionString = GetConnectionString()
                    .Replace("Initial Catalog=SistemaVendas", "Initial Catalog=master")
                    .Replace("Initial Catalog=SistemaVendasAngola", "Initial Catalog=master");

                using (var connection = new SqlConnection(masterConnectionString))
                {
                    connection.Open();

                    // Verificar se o banco existe
                    string checkDbQuery = @"
                        SELECT COUNT(*) 
                        FROM sys.databases 
                        WHERE name IN ('SistemaVendas', 'SistemaVendasAngola')";

                    using (var command = new SqlCommand(checkDbQuery, connection))
                    {
                        int count = Convert.ToInt32(command.ExecuteScalar());
                        
                        if (count == 0)
                        {
                            // Criar o banco com configurações para Angola
                            string createDbQuery = @"
                                CREATE DATABASE SistemaVendasAngola
                                COLLATE SQL_Latin1_General_CP1_CI_AS";

                            using (var createCommand = new SqlCommand(createDbQuery, connection))
                            {
                                createCommand.CommandTimeout = 60; // Timeout maior para criação
                                createCommand.ExecuteNonQuery();
                            }

                            LogInfo("Banco de dados SistemaVendasAngola criado com sucesso");
                            
                            MessageBox.Show(
                                "Banco de dados 'SistemaVendasAngola' criado com sucesso!\n\n" +
                                "O sistema está configurado para o mercado angolano com:\n" +
                                "• Moeda: Kwanza (AOA)\n" +
                                "• IVA: 14% (padrão de Angola)\n" +
                                "• Idioma: Português de Angola",
                                "Banco Criado - Sistema de Vendas Angola",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information
                            );
                        }
                        else
                        {
                            LogInfo("Banco de dados já existe");
                        }
                    }
                }

                // Atualizar flag de conexão testada
                connectionTested = true;
                return true;
            }
            catch (SqlException sqlEx)
            {
                LogError("Erro SQL ao criar banco de dados", sqlEx);
                
                MessageBox.Show(
                    $"Erro ao criar banco de dados:\n\n{GetFriendlyErrorMessage(sqlEx)}\n\n" +
                    "Verifique se você tem permissões de administrador no SQL Server.",
                    "Erro ao Criar Banco - Sistema de Vendas Angola",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return false;
            }
            catch (Exception ex)
            {
                LogError("Erro geral ao criar banco de dados", ex);
                
                MessageBox.Show(
                    $"Erro inesperado ao criar banco de dados:\n\n{ex.Message}",
                    "Erro Crítico - Sistema de Vendas Angola",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return false;
            }
        }

        /// <summary>
        /// Verifica se as tabelas essenciais existem no banco
        /// </summary>
        /// <returns>True se todas as tabelas essenciais existem</returns>
        public static bool ValidateDatabaseStructure()
        {
            try
            {
                string[] essentialTables = {
                    "ConfiguracoesSistema", "Usuarios", "Categorias", "Produtos",
                    "Clientes", "Fornecedores", "Vendas", "ItensVenda",
                    "FormasPagamento", "MovimentacaoEstoque", "ConfiguracoesEmpresa"
                };

                string query = @"
                    SELECT TABLE_NAME 
                    FROM INFORMATION_SCHEMA.TABLES 
                    WHERE TABLE_TYPE = 'BASE TABLE' 
                    AND TABLE_NAME IN ('" + string.Join("','", essentialTables) + "')";

                var result = ExecuteQuery(query);
                
                if (result.Rows.Count == essentialTables.Length)
                {
                    LogInfo("Estrutura do banco de dados validada com sucesso");
                    return true;
                }
                else
                {
                    LogWarning($"Estrutura do banco incompleta. Encontradas {result.Rows.Count} de {essentialTables.Length} tabelas");
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogError("Erro ao validar estrutura do banco", ex);
                return false;
            }
        }

        #endregion

        #region Métodos Utilitários

        /// <summary>
        /// Verifica se um erro SQL é transitório (temporário)
        /// </summary>
        /// <param name="sqlEx">Exceção SQL</param>
        /// <returns>True se o erro é transitório</returns>
        private static bool IsTransientError(SqlException sqlEx)
        {
            // Códigos de erro que indicam problemas transitórios
            int[] transientErrorNumbers = { 2, 53, 121, 233, 10053, 10054, 10060, 40197, 40501, 40613 };
            
            foreach (SqlError error in sqlEx.Errors)
            {
                if (Array.IndexOf(transientErrorNumbers, error.Number) >= 0)
                    return true;
            }
            
            return false;
        }

        /// <summary>
        /// Converte erros SQL técnicos em mensagens amigáveis
        /// </summary>
        /// <param name="sqlEx">Exceção SQL</param>
        /// <returns>Mensagem amigável</returns>
        private static string GetFriendlyErrorMessage(SqlException sqlEx)
        {
            switch (sqlEx.Number)
            {
                case 2:
                case 53:
                    return "Servidor de banco de dados não encontrado ou inacessível. Verifique se o SQL Server está executando.";
                
                case 18456:
                    return "Falha na autenticação. Verifique o usuário e senha do banco de dados.";
                
                case 4060:
                    return "Banco de dados não encontrado. O sistema tentará criar automaticamente.";
                
                case 2812:
                    return "Procedimento armazenado não encontrado. A estrutura do banco pode estar incompleta.";
                
                case 208:
                    return "Tabela ou view não encontrada. A estrutura do banco pode estar incompleta.";
                
                case 547:
                    return "Violação de chave estrangeira. Verifique se os dados relacionados existem.";
                
                case 2627:
                case 2601:
                    return "Violação de chave única. Já existe um registro com estes dados.";
                
                case 8152:
                    return "Dados muito longos para o campo. Reduza o tamanho do texto.";
                
                case 245:
                    return "Erro de conversão de dados. Verifique se os valores estão no formato correto.";
                
                case 1205:
                    return "Deadlock detectado. Tente a operação novamente.";
                
                default:
                    return $"Erro no banco de dados (Código: {sqlEx.Number}): {sqlEx.Message}";
            }
        }

        /// <summary>
        /// Formata valores monetários para o padrão angolano
        /// </summary>
        /// <param name="value">Valor a ser formatado</param>
        /// <returns>Valor formatado em Kwanza</returns>
        public static string FormatCurrency(decimal value)
        {
            try
            {
                // Formato angolano: 1.234,56 Kz
                var angolaCulture = new CultureInfo("pt-AO");
                return value.ToString("N2", angolaCulture) + " Kz";
            }
            catch
            {
                // Fallback para formato padrão
                return value.ToString("N2") + " Kz";
            }
        }

        /// <summary>
        /// Formata datas para o padrão angolano
        /// </summary>
        /// <param name="date">Data a ser formatada</param>
        /// <returns>Data formatada</returns>
        public static string FormatDate(DateTime date)
        {
            try
            {
                var angolaCulture = new CultureInfo("pt-AO");
                return date.ToString("dd/MM/yyyy", angolaCulture);
            }
            catch
            {
                return date.ToString("dd/MM/yyyy");
            }
        }

        /// <summary>
        /// Formata data e hora para o padrão angolano
        /// </summary>
        /// <param name="dateTime">Data e hora a ser formatada</param>
        /// <returns>Data e hora formatadas</returns>
        public static string FormatDateTime(DateTime dateTime)
        {
            try
            {
                var angolaCulture = new CultureInfo("pt-AO");
                return dateTime.ToString("dd/MM/yyyy HH:mm:ss", angolaCulture);
            }
            catch
            {
                return dateTime.ToString("dd/MM/yyyy HH:mm:ss");
            }
        }

        #endregion

        #region Métodos de Log

        /// <summary>
        /// Registra informações no log
        /// </summary>
        /// <param name="message">Mensagem</param>
        private static void LogInfo(string message)
        {
            System.Diagnostics.Debug.WriteLine($"[INFO] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
        }

        /// <summary>
        /// Registra avisos no log
        /// </summary>
        /// <param name="message">Mensagem</param>
        private static void LogWarning(string message)
        {
            System.Diagnostics.Debug.WriteLine($"[WARNING] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
        }

        /// <summary>
        /// Registra erros no log
        /// </summary>
        /// <param name="message">Mensagem</param>
        /// <param name="ex">Exceção</param>
        /// <param name="query">Query SQL (opcional)</param>
        private static void LogError(string message, Exception ex, string query = null)
        {
            string logMessage = $"[ERROR] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}\n" +
                               $"Exception: {ex.Message}\n" +
                               $"StackTrace: {ex.StackTrace}";
            
            if (!string.IsNullOrEmpty(query))
            {
                logMessage += $"\nQuery: {query}";
            }
            
            System.Diagnostics.Debug.WriteLine(logMessage);
        }

        #endregion
    }
}