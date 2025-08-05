using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Controls;
using SISTEMA_DE_VENDAS_GENERICO.Data;

namespace SISTEMA_DE_VENDAS_GENERICO.Views
{
    public partial class DashboardPage : Page
    {
        public DashboardPage()
        {
            InitializeComponent();
            CarregarDados();
        }

        private void CarregarDados()
        {
            try
            {
                CarregarEstatisticas();
                CarregarProdutosMaisVendidos();
                CarregarUltimasVendas();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Erro ao carregar dados: " + ex.Message, "Erro", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void CarregarEstatisticas()
        {
            DateTime hoje = DateTime.Today;
            
            // Vendas do dia
            string queryVendas = @"
                SELECT COUNT(*) as TotalVendas, 
                       ISNULL(SUM(Total), 0) as TotalReceita
                FROM Vendas 
                WHERE CAST(DataVenda AS DATE) = @Data";

            SqlParameter[] paramVendas = { new SqlParameter("@Data", hoje) };
            DataTable dtVendas = DatabaseConnection.ExecuteQuery(queryVendas, paramVendas);

            if (dtVendas.Rows.Count > 0)
            {
                DataRow row = dtVendas.Rows[0];
                txtVendasHoje.Text = row["TotalVendas"].ToString();
                decimal totalReceita = Convert.ToDecimal(row["TotalReceita"]);
                txtTotalHoje.Text = totalReceita.ToString("C");
            }

            // Produtos em estoque baixo (menos de 10 unidades)
            string queryEstoque = "SELECT COUNT(*) FROM Produtos WHERE EstoqueAtual < 10 AND Ativo = 1";
            object estoqueBaixo = DatabaseConnection.ExecuteScalar(queryEstoque);
            txtEstoqueBaixo.Text = estoqueBaixo.ToString();

            // Total de clientes
            string queryClientes = "SELECT COUNT(*) FROM Clientes";
            object totalClientes = DatabaseConnection.ExecuteScalar(queryClientes);
            txtTotalClientes.Text = totalClientes.ToString();
        }

        private void CarregarProdutosMaisVendidos()
        {
            DateTime hoje = DateTime.Today;
            
            string query = @"
                SELECT TOP 10 
                    p.Nome as NomeProduto,
                    SUM(iv.Quantidade) as QuantidadeVendida,
                    SUM(iv.TotalItem) as TotalVendido
                FROM ItensVenda iv
                INNER JOIN Produtos p ON iv.IdProduto = p.Id
                INNER JOIN Vendas v ON iv.IdVenda = v.Id
                WHERE CAST(v.DataVenda AS DATE) = @Data
                GROUP BY p.Nome
                ORDER BY QuantidadeVendida DESC";

            SqlParameter[] parameters = { new SqlParameter("@Data", hoje) };
            DataTable dt = DatabaseConnection.ExecuteQuery(query, parameters);
            dgProdutosVendidos.ItemsSource = dt.DefaultView;
        }

        private void CarregarUltimasVendas()
        {
            string query = @"
                SELECT TOP 10 
                    v.DataVenda,
                    ISNULL(c.Nome, 'Cliente não identificado') as NomeCliente,
                    COUNT(iv.Id) as QuantidadeItens,
                    v.Total,
                    v.TipoPagamento
                FROM Vendas v
                LEFT JOIN Clientes c ON v.IdCliente = c.Id
                INNER JOIN ItensVenda iv ON v.Id = iv.IdVenda
                GROUP BY v.Id, v.DataVenda, c.Nome, v.Total, v.TipoPagamento
                ORDER BY v.DataVenda DESC";

            DataTable dt = DatabaseConnection.ExecuteQuery(query);
            dgUltimasVendas.ItemsSource = dt.DefaultView;
        }
    }
} 