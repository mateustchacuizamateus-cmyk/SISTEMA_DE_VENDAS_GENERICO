using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SISTEMA_DE_VENDAS_GENERICO.Data;

namespace SISTEMA_DE_VENDAS_GENERICO.Views
{
    public partial class RelatoriosPage : Page
    {
        private DataTable dadosRelatorio;
        private DateTime dataInicial;
        private DateTime dataFinal;

        public RelatoriosPage()
        {
            InitializeComponent();
            ConfigurarPeriodo();
        }

        private void ConfigurarPeriodo()
        {
            cmbPeriodo.SelectedIndex = 0; // Hoje por padrão
            CalcularPeriodo();
        }

        private void cmbTipoRelatorio_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbTipoRelatorio.SelectedItem != null)
            {
                string tipo = (cmbTipoRelatorio.SelectedItem as ComboBoxItem)?.Content.ToString();
                txtTituloRelatorio.Text = $"Relatório: {tipo}";
                txtStatus.Text = $"Selecione o período e clique em 'Gerar Relatório'";
            }
        }

        private void CalcularPeriodo()
        {
            if (cmbPeriodo.SelectedItem == null) return;

            string periodo = (cmbPeriodo.SelectedItem as ComboBoxItem)?.Content.ToString();
            
            switch (periodo)
            {
                case "Hoje":
                    dataInicial = DateTime.Today;
                    dataFinal = DateTime.Today.AddDays(1).AddSeconds(-1);
                    break;
                case "Últimos 7 dias":
                    dataInicial = DateTime.Today.AddDays(-7);
                    dataFinal = DateTime.Today.AddDays(1).AddSeconds(-1);
                    break;
                case "Últimos 30 dias":
                    dataInicial = DateTime.Today.AddDays(-30);
                    dataFinal = DateTime.Today.AddDays(1).AddSeconds(-1);
                    break;
                case "Este mês":
                    dataInicial = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                    dataFinal = dataInicial.AddMonths(1).AddSeconds(-1);
                    break;
                case "Mês anterior":
                    dataInicial = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-1);
                    dataFinal = dataInicial.AddMonths(1).AddSeconds(-1);
                    break;
                case "Personalizado":
                    MostrarCamposPersonalizados();
                    return;
            }

            OcultarCamposPersonalizados();
        }

        private void MostrarCamposPersonalizados()
        {
            txtDataInicialLabel.Visibility = Visibility.Visible;
            dpDataInicial.Visibility = Visibility.Visible;
            txtDataFinalLabel.Visibility = Visibility.Visible;
            dpDataFinal.Visibility = Visibility.Visible;

            if (dpDataInicial.SelectedDate == null)
                dpDataInicial.SelectedDate = DateTime.Today.AddDays(-30);
            if (dpDataFinal.SelectedDate == null)
                dpDataFinal.SelectedDate = DateTime.Today;
        }

        private void OcultarCamposPersonalizados()
        {
            txtDataInicialLabel.Visibility = Visibility.Collapsed;
            dpDataInicial.Visibility = Visibility.Collapsed;
            txtDataFinalLabel.Visibility = Visibility.Collapsed;
            dpDataFinal.Visibility = Visibility.Collapsed;
        }

        private void btnGerarRelatorio_Click(object sender, RoutedEventArgs e)
        {
            if (cmbTipoRelatorio.SelectedItem == null)
            {
                MessageBox.Show("Selecione um tipo de relatório!", "Aviso", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cmbPeriodo.SelectedItem == null)
            {
                MessageBox.Show("Selecione um período!", "Aviso", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Calcular período se for personalizado
            if ((cmbPeriodo.SelectedItem as ComboBoxItem)?.Content.ToString() == "Personalizado")
            {
                if (dpDataInicial.SelectedDate == null || dpDataFinal.SelectedDate == null)
                {
                    MessageBox.Show("Selecione as datas inicial e final!", "Aviso", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                dataInicial = dpDataInicial.SelectedDate.Value;
                dataFinal = dpDataFinal.SelectedDate.Value.AddDays(1).AddSeconds(-1);
            }
            else
            {
                CalcularPeriodo();
            }

            try
            {
                string tipoRelatorio = (cmbTipoRelatorio.SelectedItem as ComboBoxItem)?.Content.ToString();
                GerarRelatorio(tipoRelatorio);
                txtStatus.Text = $"Relatório gerado com sucesso! Período: {dataInicial:dd/MM/yyyy} a {dataFinal:dd/MM/yyyy}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao gerar relatório: {ex.Message}", "Erro", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GerarRelatorio(string tipoRelatorio)
        {
            switch (tipoRelatorio)
            {
                case "Vendas por Período":
                    GerarRelatorioVendas();
                    break;
                case "Produtos Mais Vendidos":
                    GerarRelatorioProdutosVendidos();
                    break;
                case "Clientes Mais Ativos":
                    GerarRelatorioClientesAtivos();
                    break;
                case "Movimentação de Estoque":
                    GerarRelatorioMovimentacaoEstoque();
                    break;
                case "Resumo Financeiro":
                    GerarRelatorioResumoFinanceiro();
                    break;
            }
        }

        private void GerarRelatorioVendas()
        {
            string query = @"
                SELECT 
                    v.Id as 'Número da Venda',
                    v.DataVenda as 'Data',
                    c.Nome as 'Cliente',
                    u.Nome as 'Vendedor',
                    v.Total as 'Total (AOA)',
                    v.TipoPagamento as 'Forma de Pagamento',
                    v.Desconto as 'Desconto (AOA)',
                    v.Observacoes as 'Observações'
                FROM Vendas v
                LEFT JOIN Clientes c ON v.IdCliente = c.Id
                LEFT JOIN Usuarios u ON v.IdUsuario = u.Id
                WHERE v.DataVenda BETWEEN @DataInicial AND @DataFinal
                ORDER BY v.DataVenda DESC";

            SqlParameter[] parameters = {
                new SqlParameter("@DataInicial", dataInicial),
                new SqlParameter("@DataFinal", dataFinal)
            };

            dadosRelatorio = DatabaseConnection.ExecuteQuery(query, parameters);
            dgResultados.ItemsSource = dadosRelatorio.DefaultView;

            decimal totalVendas = dadosRelatorio.AsEnumerable()
                .Sum(row => Convert.ToDecimal(row["Total (AOA)"]));

            txtTotalRelatorio.Text = $"Total: {totalVendas:N2} AOA";
        }

        private void GerarRelatorioProdutosVendidos()
        {
            string query = @"
                SELECT 
                    p.Nome as 'Produto',
                    c.Nome as 'Categoria',
                    SUM(iv.Quantidade) as 'Quantidade Vendida',
                    SUM(iv.TotalItem) as 'Total Vendido (AOA)',
                    AVG(iv.PrecoUnitario) as 'Preço Médio (AOA)',
                    COUNT(DISTINCT v.Id) as 'Número de Vendas'
                FROM ItensVenda iv
                JOIN Produtos p ON iv.IdProduto = p.Id
                LEFT JOIN Categorias c ON p.IdCategoria = c.Id
                JOIN Vendas v ON iv.IdVenda = v.Id
                WHERE v.DataVenda BETWEEN @DataInicial AND @DataFinal
                GROUP BY p.Nome, c.Nome
                ORDER BY SUM(iv.Quantidade) DESC";

            SqlParameter[] parameters = {
                new SqlParameter("@DataInicial", dataInicial),
                new SqlParameter("@DataFinal", dataFinal)
            };

            dadosRelatorio = DatabaseConnection.ExecuteQuery(query, parameters);
            dgResultados.ItemsSource = dadosRelatorio.DefaultView;

            decimal totalVendido = dadosRelatorio.AsEnumerable()
                .Sum(row => Convert.ToDecimal(row["Total Vendido (AOA)"]));

            txtTotalRelatorio.Text = $"Total Vendido: {totalVendido:N2} AOA";
        }

        private void GerarRelatorioClientesAtivos()
        {
            string query = @"
                SELECT 
                    c.Nome as 'Cliente',
                    c.Telefone as 'Telefone',
                    c.Email as 'Email',
                    COUNT(v.Id) as 'Número de Compras',
                    SUM(v.Total) as 'Total Gasto (AOA)',
                    AVG(v.Total) as 'Ticket Médio (AOA)',
                    MAX(v.DataVenda) as 'Última Compra'
                FROM Clientes c
                LEFT JOIN Vendas v ON c.Id = v.IdCliente 
                    AND v.DataVenda BETWEEN @DataInicial AND @DataFinal
                GROUP BY c.Nome, c.Telefone, c.Email
                HAVING COUNT(v.Id) > 0
                ORDER BY SUM(v.Total) DESC";

            SqlParameter[] parameters = {
                new SqlParameter("@DataInicial", dataInicial),
                new SqlParameter("@DataFinal", dataFinal)
            };

            dadosRelatorio = DatabaseConnection.ExecuteQuery(query, parameters);
            dgResultados.ItemsSource = dadosRelatorio.DefaultView;

            decimal totalGasto = dadosRelatorio.AsEnumerable()
                .Sum(row => Convert.ToDecimal(row["Total Gasto (AOA)"]));

            txtTotalRelatorio.Text = $"Total Gasto pelos Clientes: {totalGasto:N2} AOA";
        }

        private void GerarRelatorioMovimentacaoEstoque()
        {
            string query = @"
                SELECT 
                    p.Nome as 'Produto',
                    c.Nome as 'Categoria',
                    me.Tipo as 'Tipo',
                    me.Quantidade as 'Quantidade',
                    me.Motivo as 'Motivo',
                    me.DataMovimentacao as 'Data',
                    u.Nome as 'Responsável'
                FROM MovimentacaoEstoque me
                JOIN Produtos p ON me.IdProduto = p.Id
                LEFT JOIN Categorias c ON p.IdCategoria = c.Id
                LEFT JOIN Usuarios u ON me.IdProduto = p.Id -- Assumindo que o usuário está relacionado
                WHERE me.DataMovimentacao BETWEEN @DataInicial AND @DataFinal
                ORDER BY me.DataMovimentacao DESC";

            SqlParameter[] parameters = {
                new SqlParameter("@DataInicial", dataInicial),
                new SqlParameter("@DataFinal", dataFinal)
            };

            dadosRelatorio = DatabaseConnection.ExecuteQuery(query, parameters);
            dgResultados.ItemsSource = dadosRelatorio.DefaultView;

            int totalEntradas = dadosRelatorio.AsEnumerable()
                .Where(row => row["Tipo"].ToString() == "Entrada")
                .Sum(row => Convert.ToInt32(row["Quantidade"]));

            int totalSaidas = dadosRelatorio.AsEnumerable()
                .Where(row => row["Tipo"].ToString() == "Saida")
                .Sum(row => Convert.ToInt32(row["Quantidade"]));

            txtTotalRelatorio.Text = $"Entradas: {totalEntradas} | Saídas: {totalSaidas}";
        }

        private void GerarRelatorioResumoFinanceiro()
        {
            string query = @"
                SELECT 
                    'Vendas' as 'Tipo',
                    COUNT(v.Id) as 'Quantidade',
                    SUM(v.Total) as 'Total (AOA)',
                    AVG(v.Total) as 'Média (AOA)'
                FROM Vendas v
                WHERE v.DataVenda BETWEEN @DataInicial AND @DataFinal
                UNION ALL
                SELECT 
                    'Descontos Aplicados' as 'Tipo',
                    COUNT(v.Id) as 'Quantidade',
                    SUM(v.Desconto) as 'Total (AOA)',
                    AVG(v.Desconto) as 'Média (AOA)'
                FROM Vendas v
                WHERE v.DataVenda BETWEEN @DataInicial AND @DataFinal
                    AND v.Desconto > 0";

            SqlParameter[] parameters = {
                new SqlParameter("@DataInicial", dataInicial),
                new SqlParameter("@DataFinal", dataFinal)
            };

            dadosRelatorio = DatabaseConnection.ExecuteQuery(query, parameters);
            dgResultados.ItemsSource = dadosRelatorio.DefaultView;

            decimal totalVendas = dadosRelatorio.AsEnumerable()
                .Where(row => row["Tipo"].ToString() == "Vendas")
                .Sum(row => Convert.ToDecimal(row["Total (AOA)"]));

            decimal totalDescontos = dadosRelatorio.AsEnumerable()
                .Where(row => row["Tipo"].ToString() == "Descontos Aplicados")
                .Sum(row => Convert.ToDecimal(row["Total (AOA)"]));

            txtTotalRelatorio.Text = $"Vendas: {totalVendas:N2} AOA | Descontos: {totalDescontos:N2} AOA";
        }

        private void btnExportarPDF_Click(object sender, RoutedEventArgs e)
        {
            if (dadosRelatorio == null || dadosRelatorio.Rows.Count == 0)
            {
                MessageBox.Show("Gere um relatório antes de exportar!", "Aviso", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Implementação básica de exportação PDF
                string nomeArquivo = $"Relatorio_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                ExportarParaArquivo(nomeArquivo);
                
                MessageBox.Show($"Relatório exportado como: {nomeArquivo}\n\n" +
                              "Para exportação em PDF completo, instale uma biblioteca como iTextSharp.", 
                              "Exportação", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao exportar: {ex.Message}", "Erro", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnExportarExcel_Click(object sender, RoutedEventArgs e)
        {
            if (dadosRelatorio == null || dadosRelatorio.Rows.Count == 0)
            {
                MessageBox.Show("Gere um relatório antes de exportar!", "Aviso", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string nomeArquivo = $"Relatorio_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                ExportarParaArquivo(nomeArquivo);
                
                MessageBox.Show($"Relatório exportado como: {nomeArquivo}\n\n" +
                              "Para exportação em Excel completo, instale uma biblioteca como EPPlus.", 
                              "Exportação", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao exportar: {ex.Message}", "Erro", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportarParaArquivo(string nomeArquivo)
        {
            if (dadosRelatorio == null) return;

            var linhas = new List<string>();
            
            // Cabeçalho
            var cabecalho = string.Join(";", dadosRelatorio.Columns.Cast<DataColumn>().Select(c => c.ColumnName));
            linhas.Add(cabecalho);
            
            // Dados
            foreach (DataRow row in dadosRelatorio.Rows)
            {
                var linha = string.Join(";", row.ItemArray.Select(item => item?.ToString() ?? ""));
                linhas.Add(linha);
            }
            
            System.IO.File.WriteAllLines(nomeArquivo, linhas);
        }
    }
} 