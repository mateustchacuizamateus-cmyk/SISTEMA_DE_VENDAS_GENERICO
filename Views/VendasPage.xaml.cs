using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SISTEMA_DE_VENDAS_GENERICO.Data;
using SISTEMA_DE_VENDAS_GENERICO.Models;

namespace SISTEMA_DE_VENDAS_GENERICO.Views
{
    public partial class VendasPage : Page
    {
        private List<Produto> produtosEncontrados;
        private List<ItemVenda> carrinho;
        private List<Cliente> clientes;
        private List<string> formasPagamento;
        private decimal subtotal = 0;
        private decimal desconto = 0;
        private decimal total = 0;

        public VendasPage()
        {
            InitializeComponent();
            InicializarVenda();
        }

        private void InicializarVenda()
        {
            produtosEncontrados = new List<Produto>();
            carrinho = new List<ItemVenda>();
            clientes = new List<Cliente>();
            formasPagamento = new List<string>();

            subtotal = 0;
            desconto = 0;
            total = 0;

            CarregarClientes();
            CarregarFormasPagamento();

            txtBuscaProduto.Text = "";
            txtDesconto.Text = "0,00";
            txtObservacoes.Text = "";

            dgProdutos.ItemsSource = null;
            dgCarrinho.ItemsSource = null;

            txtSubtotal.Text = "0,00 AOA";
            txtTotal.Text = "0,00 AOA";

            btnFinalizarVenda.IsEnabled = false;
        }

        private void CarregarClientes()
        {
            try
            {
                string query = @"SELECT Id, Nome, Telefone, Email, Endereco FROM Clientes ORDER BY Nome";
                DataTable dt = DatabaseConnection.ExecuteQuery(query);
                clientes = new List<Cliente>();
                
                // Adicionar opção "Cliente não identificado"
                clientes.Add(new Cliente { Id = 0, Nome = "Cliente não identificado" });
                
                foreach (DataRow row in dt.Rows)
                {
                    clientes.Add(new Cliente
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        Nome = row["Nome"].ToString(),
                        Telefone = row["Telefone"].ToString(),
                        Email = row["Email"].ToString(),
                        Endereco = row["Endereco"].ToString()
                    });
                }
                cmbClientes.ItemsSource = clientes;
                cmbClientes.DisplayMemberPath = "Nome";
                cmbClientes.SelectedValuePath = "Id";
                cmbClientes.SelectedIndex = 0; // Selecionar "Cliente não identificado" por padrão
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar clientes: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CarregarFormasPagamento()
        {
            try
            {
                string query = "SELECT Descricao FROM FormasPagamento ORDER BY Descricao";
                DataTable dt = DatabaseConnection.ExecuteQuery(query);
                formasPagamento = new List<string>();
                foreach (DataRow row in dt.Rows)
                {
                    formasPagamento.Add(row["Descricao"].ToString());
                }
                cmbFormaPagamento.ItemsSource = formasPagamento;
                if (formasPagamento.Count > 0)
                    cmbFormaPagamento.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar formas de pagamento: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BuscarProdutos(string termo)
        {
            try
            {
                string query = @"
                    SELECT p.Id, p.CodigoBarra, p.Nome, p.PrecoVenda, p.EstoqueAtual, p.Unidade, c.Nome as NomeCategoria
                    FROM Produtos p
                    INNER JOIN Categorias c ON p.IdCategoria = c.Id
                    WHERE p.Ativo = 1 AND p.EstoqueAtual > 0 
                    AND (p.Nome LIKE @Termo OR p.CodigoBarra LIKE @Termo OR c.Nome LIKE @Termo)
                    ORDER BY p.Nome";

                SqlParameter[] parameters = { new SqlParameter("@Termo", $"%{termo}%") };
                DataTable dt = DatabaseConnection.ExecuteQuery(query, parameters);

                produtosEncontrados = new List<Produto>();
                foreach (DataRow row in dt.Rows)
                {
                    produtosEncontrados.Add(new Produto
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        CodigoBarra = row["CodigoBarra"].ToString(),
                        Nome = row["Nome"].ToString(),
                        PrecoVenda = Convert.ToDecimal(row["PrecoVenda"]),
                        EstoqueAtual = Convert.ToInt32(row["EstoqueAtual"]),
                        Unidade = row["Unidade"].ToString(),
                        NomeCategoria = row["NomeCategoria"].ToString()
                    });
                }

                dgProdutos.ItemsSource = produtosEncontrados;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao buscar produtos: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AdicionarProdutoAoCarrinho(Produto produto)
        {
            var itemExistente = carrinho.FirstOrDefault(i => i.IdProduto == produto.Id);
            if (itemExistente != null)
            {
                if (itemExistente.Quantidade < produto.EstoqueAtual)
                {
                    itemExistente.Quantidade++;
                    itemExistente.TotalItem = itemExistente.Quantidade * itemExistente.PrecoUnitario;
                }
                else
                {
                    MessageBox.Show("Estoque insuficiente para este produto!", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            else
            {
                carrinho.Add(new ItemVenda
                {
                    IdProduto = produto.Id,
                    NomeProduto = produto.Nome,
                    Quantidade = 1,
                    PrecoUnitario = produto.PrecoVenda,
                    TotalItem = produto.PrecoVenda
                });
            }

            AtualizarCarrinho();
            CalcularTotais();
        }

        private void AtualizarCarrinho()
        {
            dgCarrinho.ItemsSource = null;
            dgCarrinho.ItemsSource = carrinho;
            btnFinalizarVenda.IsEnabled = carrinho.Count > 0;
        }

        private void CalcularTotais()
        {
            subtotal = carrinho.Sum(item => item.TotalItem);
            decimal valorDesconto;
            if (decimal.TryParse(txtDesconto.Text, out valorDesconto))
            {
                desconto = valorDesconto;
            }
            else
            {
                desconto = 0;
            }
            total = subtotal - desconto;
            if (total < 0) total = 0;
            txtSubtotal.Text = $"{subtotal:N2} AOA";
            txtTotal.Text = $"{total:N2} AOA";
        }

        // Event Handlers
        private void txtBuscaProduto_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtBuscaProduto.Text))
            {
                BuscarProdutos(txtBuscaProduto.Text);
            }
        }

        private void txtBuscaProduto_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BuscarProdutos(txtBuscaProduto.Text);
            }
        }

        private void btnBuscar_Click(object sender, RoutedEventArgs e)
        {
            BuscarProdutos(txtBuscaProduto.Text);
        }

        private void dgProdutos_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var produto = dgProdutos.SelectedItem as Produto;
            if (produto != null)
            {
                AdicionarProdutoAoCarrinho(produto);
            }
        }

        private void btnRemoverItem_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                var item = button.DataContext as ItemVenda;
                if (item != null)
                {
                    carrinho.Remove(item);
                    AtualizarCarrinho();
                    CalcularTotais();
                }
            }
        }

        private void txtDesconto_TextChanged(object sender, TextChangedEventArgs e)
        {
            CalcularTotais();
        }

        private void btnFinalizarVenda_Click(object sender, RoutedEventArgs e)
        {
            if (carrinho.Count == 0)
            {
                MessageBox.Show("Adicione produtos ao carrinho antes de finalizar a venda!", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (cmbFormaPagamento.SelectedItem == null)
            {
                MessageBox.Show("Selecione uma forma de pagamento!", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Inserir venda
                int? idCliente = null;
                var clienteSelecionado = (Cliente)cmbClientes.SelectedItem;
                if (clienteSelecionado != null && clienteSelecionado.Id > 0)
                {
                    idCliente = clienteSelecionado.Id;
                }

                string queryVenda = @"
                    INSERT INTO Vendas (IdCliente, IdUsuario, DataVenda, Total, TipoPagamento, Desconto, Observacoes)
                    VALUES (@IdCliente, @IdUsuario, @DataVenda, @Total, @TipoPagamento, @Desconto, @Observacoes);
                    SELECT SCOPE_IDENTITY();";
                
                SqlParameter[] parametersVenda = {
                    new SqlParameter("@IdCliente", idCliente.HasValue ? (object)idCliente.Value : DBNull.Value),
                    new SqlParameter("@IdUsuario", LoginWindow.UsuarioLogado.Id),
                    new SqlParameter("@DataVenda", DateTime.Now),
                    new SqlParameter("@Total", total),
                    new SqlParameter("@TipoPagamento", cmbFormaPagamento.SelectedItem.ToString()),
                    new SqlParameter("@Desconto", desconto),
                    new SqlParameter("@Observacoes", string.IsNullOrWhiteSpace(txtObservacoes.Text) ? DBNull.Value : (object)txtObservacoes.Text.Trim())
                };
                
                int idVenda = Convert.ToInt32(DatabaseConnection.ExecuteScalar(queryVenda, parametersVenda));

                // Inserir itens da venda
                foreach (var item in carrinho)
                {
                    string queryItem = @"
                        INSERT INTO ItensVenda (IdVenda, IdProduto, Quantidade, PrecoUnitario, TotalItem)
                        VALUES (@IdVenda, @IdProduto, @Quantidade, @PrecoUnitario, @TotalItem)";
                    
                    SqlParameter[] parametersItem = {
                        new SqlParameter("@IdVenda", idVenda),
                        new SqlParameter("@IdProduto", item.IdProduto),
                        new SqlParameter("@Quantidade", item.Quantidade),
                        new SqlParameter("@PrecoUnitario", item.PrecoUnitario),
                        new SqlParameter("@TotalItem", item.TotalItem)
                    };
                    DatabaseConnection.ExecuteNonQuery(queryItem, parametersItem);

                    // Atualizar estoque
                    string queryEstoque = @"UPDATE Produtos SET EstoqueAtual = EstoqueAtual - @Quantidade WHERE Id = @IdProduto";
                    SqlParameter[] parametersEstoque = {
                        new SqlParameter("@Quantidade", item.Quantidade),
                        new SqlParameter("@IdProduto", item.IdProduto)
                    };
                    DatabaseConnection.ExecuteNonQuery(queryEstoque, parametersEstoque);

                    // Registrar movimentação de estoque
                    string queryMovimentacao = @"
                        INSERT INTO MovimentacaoEstoque (IdProduto, Tipo, Quantidade, Motivo, DataMovimentacao)
                        VALUES (@IdProduto, 'Saida', @Quantidade, 'Venda', @DataMovimentacao)";
                    
                    SqlParameter[] parametersMovimentacao = {
                        new SqlParameter("@IdProduto", item.IdProduto),
                        new SqlParameter("@Quantidade", item.Quantidade),
                        new SqlParameter("@DataMovimentacao", DateTime.Now)
                    };
                    DatabaseConnection.ExecuteNonQuery(queryMovimentacao, parametersMovimentacao);
                }

                MessageBox.Show($"Venda finalizada com sucesso!\n\nTotal: {total:N2} AOA", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                
                var result = MessageBox.Show("Deseja imprimir o recibo?", "Imprimir Recibo", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    ImprimirRecibo(idVenda);
                }
                
                InicializarVenda();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao finalizar venda: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancelarVenda_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Deseja realmente cancelar esta venda?", "Confirmar Cancelamento", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                InicializarVenda();
            }
        }

        private void ImprimirRecibo(int idVenda)
        {
            try
            {
                string query = @"
                    SELECT v.Id, v.DataVenda, v.Total, v.FormaPagamento, c.Nome as NomeCliente, c.Telefone
                           ISNULL(c.Nome, 'Cliente não identificado') as NomeCliente, 
                           ISNULL(c.Telefone, '') as Telefone,
                           u.Nome as NomeVendedor
                    LEFT JOIN Clientes c ON v.IdCliente = c.Id
                    INNER JOIN Usuarios u ON v.IdUsuario = u.Id
                    WHERE v.Id = @IdVenda";

                SqlParameter[] parameters = { new SqlParameter("@IdVenda", idVenda) };
                DataTable dtVenda = DatabaseConnection.ExecuteQuery(query, parameters);

                if (dtVenda.Rows.Count > 0)
                {
                    DataRow venda = dtVenda.Rows[0];
                    string queryItens = @"
                        SELECT p.Nome, iv.Quantidade, iv.PrecoUnitario, iv.TotalItem
                        FROM ItensVenda iv
                        INNER JOIN Produtos p ON iv.IdProduto = p.Id
                        WHERE iv.IdVenda = @IdVenda";

                    DataTable dtItens = DatabaseConnection.ExecuteQuery(queryItens, parameters);

                    string recibo = $"================================\n";
                    recibo += $"      SISTEMA DE VENDAS\n";
                    recibo += $"================================\n";
                    recibo += $"Data: {Convert.ToDateTime(venda["DataVenda"]):dd/MM/yyyy HH:mm}\n";
                    recibo += $"Venda #: {venda["Id"]}\n";
                    recibo += $"Vendedor: {venda["NomeVendedor"]}\n";
                    recibo += $"Cliente: {venda["NomeCliente"]}\n";
                    if (!string.IsNullOrEmpty(venda["Telefone"].ToString()))
                        recibo += $"Telefone: {venda["Telefone"]}\n";
                    recibo += $"Forma Pagamento: {venda["TipoPagamento"]}\n";
                    recibo += $"================================\n";
                    recibo += $"ITENS:\n";

                    foreach (DataRow item in dtItens.Rows)
                    {
                        recibo += $"{item["Nome"]}\n";
                        recibo += $"  {item["Quantidade"]} x {Convert.ToDecimal(item["PrecoUnitario"]):N2} AOA = {Convert.ToDecimal(item["TotalItem"]):N2} AOA\n";
                    }

                    recibo += $"================================\n";
                    decimal subtotalRecibo = Convert.ToDecimal(venda["Total"]) + Convert.ToDecimal(venda["Desconto"]);
                    recibo += $"SUBTOTAL: {subtotalRecibo:N2} AOA\n";
                    if (Convert.ToDecimal(venda["Desconto"]) > 0)
                        recibo += $"DESCONTO: -{Convert.ToDecimal(venda["Desconto"]):N2} AOA\n";
                    recibo += $"TOTAL: {Convert.ToDecimal(venda["Total"]):N2} AOA\n";
                    recibo += $"================================\n";
                    if (!string.IsNullOrEmpty(venda["Observacoes"].ToString()))
                    {
                        recibo += $"Obs: {venda["Observacoes"]}\n";
                        recibo += $"================================\n";
                    }
                    recibo += $"Obrigado pela preferência!\n";

                    string nomeArquivo = $"Recibo_Venda_{idVenda}_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                    System.IO.File.WriteAllText(nomeArquivo, recibo);
                    
                    var result = MessageBox.Show($"Recibo salvo como: {nomeArquivo}\n\nDeseja abrir o arquivo?", "Recibo", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    if (result == MessageBoxResult.Yes)
                    {
                        System.Diagnostics.Process.Start(nomeArquivo);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao gerar recibo: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
} 