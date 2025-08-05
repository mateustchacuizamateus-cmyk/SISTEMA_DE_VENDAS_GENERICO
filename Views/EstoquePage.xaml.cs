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
    public partial class EstoquePage : Page
    {
        private List<ProdutoEstoque> produtosEstoque;
        private ProdutoEstoque produtoSelecionado;

        public EstoquePage()
        {
            InitializeComponent();
            CarregarDados();
            ConfigurarInterface();
        }

        private void ConfigurarInterface()
        {
            cmbTipoMovimentacao.SelectedIndex = 0; // Entrada por padrão
            cmbMotivo.SelectedIndex = 0; // Compra por padrão
        }

        private void CarregarDados()
        {
            CarregarEstoque();
            CarregarHistorico();
        }

        private void CarregarEstoque()
        {
            try
            {
                string query = @"
                    SELECT p.Id, p.Nome, p.CodigoBarra, p.EstoqueAtual, p.Unidade, 
                           p.PrecoVenda, p.Ativo, ISNULL(c.Nome, 'Sem categoria') as NomeCategoria
                    FROM Produtos p
                    LEFT JOIN Categorias c ON p.IdCategoria = c.Id
                    ORDER BY p.Nome";

                DataTable dt = DatabaseConnection.ExecuteQuery(query);
                
                produtosEstoque = new List<ProdutoEstoque>();
                foreach (DataRow row in dt.Rows)
                {
                    var produto = new ProdutoEstoque
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        Nome = row["Nome"].ToString(),
                        CodigoBarra = row["CodigoBarra"].ToString(),
                        EstoqueAtual = Convert.ToInt32(row["EstoqueAtual"]),
                        Unidade = row["Unidade"].ToString(),
                        PrecoVenda = Convert.ToDecimal(row["PrecoVenda"]),
                        Ativo = Convert.ToBoolean(row["Ativo"]),
                        NomeCategoria = row["NomeCategoria"].ToString(),
                        EstoqueBaixo = Convert.ToInt32(row["EstoqueAtual"]) < 10
                    };
                    produtosEstoque.Add(produto);
                }

                dgEstoque.ItemsSource = produtosEstoque;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar estoque: {ex.Message}", "Erro", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CarregarHistorico()
        {
            try
            {
                string query = @"
                    SELECT TOP 50 me.DataMovimentacao, me.Tipo, me.Quantidade, me.Motivo, p.Nome as NomeProduto
                    FROM MovimentacaoEstoque me
                    INNER JOIN Produtos p ON me.IdProduto = p.Id
                    ORDER BY me.DataMovimentacao DESC";

                DataTable dt = DatabaseConnection.ExecuteQuery(query);
                dgHistorico.ItemsSource = dt.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar histórico: {ex.Message}", "Erro", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void dgEstoque_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            produtoSelecionado = dgEstoque.SelectedItem as ProdutoEstoque;
            
            if (produtoSelecionado != null)
            {
                txtProdutoSelecionado.Text = produtoSelecionado.Nome;
                txtEstoqueAtual.Text = produtoSelecionado.EstoqueAtual.ToString();
                btnConfirmarMovimentacao.IsEnabled = true;
            }
            else
            {
                txtProdutoSelecionado.Text = "Nenhum produto selecionado";
                txtEstoqueAtual.Text = "0";
                btnConfirmarMovimentacao.IsEnabled = false;
            }
        }

        private void btnEntrada_Click(object sender, RoutedEventArgs e)
        {
            cmbTipoMovimentacao.SelectedIndex = 0; // Entrada
            cmbMotivo.SelectedIndex = 0; // Compra
            txtQuantidade.Focus();
        }

        private void btnSaida_Click(object sender, RoutedEventArgs e)
        {
            cmbTipoMovimentacao.SelectedIndex = 1; // Saída
            cmbMotivo.SelectedIndex = 3; // Perda
            txtQuantidade.Focus();
        }

        private void btnAtualizar_Click(object sender, RoutedEventArgs e)
        {
            CarregarDados();
        }

        private void cmbTipoMovimentacao_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbTipoMovimentacao.SelectedIndex == 0) // Entrada
            {
                btnConfirmarMovimentacao.Background = System.Windows.Media.Brushes.Green;
            }
            else // Saída
            {
                btnConfirmarMovimentacao.Background = System.Windows.Media.Brushes.Red;
            }
        }

        private void btnConfirmarMovimentacao_Click(object sender, RoutedEventArgs e)
        {
            if (produtoSelecionado == null)
            {
                MessageBox.Show("Selecione um produto!", "Aviso", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!ValidarMovimentacao())
                return;

            try
            {
                string tipo = (cmbTipoMovimentacao.SelectedItem as ComboBoxItem)?.Content.ToString();
                int quantidade = int.Parse(txtQuantidade.Text);
                string motivo = (cmbMotivo.SelectedItem as ComboBoxItem)?.Content.ToString();
                string observacoes = txtObservacoes.Text.Trim();

                // Verificar se há estoque suficiente para saída
                if (tipo == "Saida" && quantidade > produtoSelecionado.EstoqueAtual)
                {
                    MessageBox.Show($"Estoque insuficiente! Estoque atual: {produtoSelecionado.EstoqueAtual}", 
                        "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show(
                    $"Confirmar {tipo.ToLower()} de {quantidade} {produtoSelecionado.Unidade} do produto '{produtoSelecionado.Nome}'?", 
                    "Confirmação", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Registrar movimentação
                    string queryMovimentacao = @"
                        INSERT INTO MovimentacaoEstoque (IdProduto, Tipo, Quantidade, Motivo, DataMovimentacao)
                        VALUES (@IdProduto, @Tipo, @Quantidade, @Motivo, @DataMovimentacao)";

                    SqlParameter[] parametersMovimentacao = {
                        new SqlParameter("@IdProduto", produtoSelecionado.Id),
                        new SqlParameter("@Tipo", tipo),
                        new SqlParameter("@Quantidade", quantidade),
                        new SqlParameter("@Motivo", string.IsNullOrEmpty(observacoes) ? motivo : $"{motivo} - {observacoes}"),
                        new SqlParameter("@DataMovimentacao", DateTime.Now)
                    };

                    DatabaseConnection.ExecuteNonQuery(queryMovimentacao, parametersMovimentacao);

                    // Atualizar estoque do produto
                    string queryEstoque;
                    if (tipo == "Entrada")
                    {
                        queryEstoque = "UPDATE Produtos SET EstoqueAtual = EstoqueAtual + @Quantidade WHERE Id = @IdProduto";
                    }
                    else
                    {
                        queryEstoque = "UPDATE Produtos SET EstoqueAtual = EstoqueAtual - @Quantidade WHERE Id = @IdProduto";
                    }

                    SqlParameter[] parametersEstoque = {
                        new SqlParameter("@Quantidade", quantidade),
                        new SqlParameter("@IdProduto", produtoSelecionado.Id)
                    };

                    DatabaseConnection.ExecuteNonQuery(queryEstoque, parametersEstoque);

                    MessageBox.Show("Movimentação registrada com sucesso!", "Sucesso", 
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    // Limpar formulário e recarregar dados
                    LimparFormulario();
                    CarregarDados();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao registrar movimentação: {ex.Message}", "Erro", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            LimparFormulario();
        }

        private bool ValidarMovimentacao()
        {
            if (cmbTipoMovimentacao.SelectedIndex == -1)
            {
                MessageBox.Show("Selecione o tipo de movimentação!", "Validação", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            int quantidade;
            if (!int.TryParse(txtQuantidade.Text, out quantidade) || quantidade <= 0)
            {
                MessageBox.Show("Digite uma quantidade válida!", "Validação", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtQuantidade.Focus();
                return false;
            }

            if (cmbMotivo.SelectedIndex == -1)
            {
                MessageBox.Show("Selecione o motivo da movimentação!", "Validação", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void LimparFormulario()
        {
            txtQuantidade.Text = "1";
            txtObservacoes.Text = "";
            cmbTipoMovimentacao.SelectedIndex = 0;
            cmbMotivo.SelectedIndex = 0;
        }
    }

    // Classe auxiliar para o estoque
    public class ProdutoEstoque : Produto
    {
        public bool EstoqueBaixo { get; set; }
    }
}