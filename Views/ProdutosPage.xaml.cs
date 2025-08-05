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
    public partial class ProdutosPage : Page
    {
        private List<Produto> produtos;
        private List<Categoria> categorias;
        private Produto produtoSelecionado;
        private bool modoEdicao = false;

        public ProdutosPage()
        {
            InitializeComponent();
            CarregarDados();
        }

        private void CarregarDados()
        {
            CarregarCategorias();
            CarregarProdutos();
        }

        private void CarregarCategorias()
        {
            try
            {
                string query = "SELECT Id, Nome FROM Categorias ORDER BY Nome";
                DataTable dt = DatabaseConnection.ExecuteQuery(query);
                
                categorias = new List<Categoria>();
                foreach (DataRow row in dt.Rows)
                {
                    categorias.Add(new Categoria
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        Nome = row["Nome"].ToString()
                    });
                }

                cmbCategoria.ItemsSource = categorias;
                cmbCategoria.DisplayMemberPath = "Nome";
                cmbCategoria.SelectedValuePath = "Id";
                
                if (categorias.Count > 0)
                    cmbCategoria.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar categorias: {ex.Message}", "Erro", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CarregarProdutos()
        {
            try
            {
                string query = @"
                    SELECT p.Id, p.Nome, p.CodigoBarra, p.IdCategoria, p.PrecoCompra, p.PrecoVenda, 
                           p.EstoqueAtual, p.Unidade, p.Ativo, ISNULL(c.Nome, 'Sem categoria') as NomeCategoria
                    FROM Produtos p
                    LEFT JOIN Categorias c ON p.IdCategoria = c.Id
                    ORDER BY p.Nome";

                DataTable dt = DatabaseConnection.ExecuteQuery(query);
                
                produtos = new List<Produto>();
                foreach (DataRow row in dt.Rows)
                {
                    produtos.Add(new Produto
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        Nome = row["Nome"].ToString(),
                        CodigoBarra = row["CodigoBarra"].ToString(),
                        IdCategoria = row["IdCategoria"] == DBNull.Value ? 0 : Convert.ToInt32(row["IdCategoria"]),
                        PrecoCompra = Convert.ToDecimal(row["PrecoCompra"]),
                        PrecoVenda = Convert.ToDecimal(row["PrecoVenda"]),
                        EstoqueAtual = Convert.ToInt32(row["EstoqueAtual"]),
                        Unidade = row["Unidade"].ToString(),
                        Ativo = Convert.ToBoolean(row["Ativo"]),
                        NomeCategoria = row["NomeCategoria"].ToString()
                    });
                }

                dgProdutos.ItemsSource = produtos;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar produtos: {ex.Message}", "Erro", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void dgProdutos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            produtoSelecionado = dgProdutos.SelectedItem as Produto;
            btnEditarProduto.IsEnabled = produtoSelecionado != null;
            btnExcluirProduto.IsEnabled = produtoSelecionado != null;
        }

        private void btnNovoProduto_Click(object sender, RoutedEventArgs e)
        {
            modoEdicao = false;
            produtoSelecionado = null;
            LimparFormulario();
            HabilitarFormulario(true);
            txtNomeProduto.Focus();
        }

        private void btnEditarProduto_Click(object sender, RoutedEventArgs e)
        {
            if (produtoSelecionado == null)
            {
                MessageBox.Show("Selecione um produto para editar!", "Aviso", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            modoEdicao = true;
            PreencherFormulario(produtoSelecionado);
            HabilitarFormulario(true);
            txtNomeProduto.Focus();
        }

        private void btnExcluirProduto_Click(object sender, RoutedEventArgs e)
        {
            if (produtoSelecionado == null)
            {
                MessageBox.Show("Selecione um produto para excluir!", "Aviso", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Deseja realmente excluir o produto '{produtoSelecionado.Nome}'?", 
                "Confirmar Exclusão", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Verificar se o produto tem vendas associadas
                    string queryVerificar = "SELECT COUNT(*) FROM ItensVenda WHERE IdProduto = @IdProduto";
                    SqlParameter[] paramVerificar = {
                        new SqlParameter("@IdProduto", produtoSelecionado.Id)
                    };
                    
                    int count = Convert.ToInt32(DatabaseConnection.ExecuteScalar(queryVerificar, paramVerificar));
                    
                    if (count > 0)
                    {
                        MessageBox.Show("Não é possível excluir este produto pois ele possui vendas associadas!\n\n" +
                                      "Use a opção 'Desativar' em vez de excluir.", "Aviso", 
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    string query = "DELETE FROM Produtos WHERE Id = @Id";
                    SqlParameter[] parameters = {
                        new SqlParameter("@Id", produtoSelecionado.Id)
                    };

                    DatabaseConnection.ExecuteNonQuery(query, parameters);
                    
                    MessageBox.Show("Produto excluído com sucesso!", "Sucesso", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    CarregarProdutos();
                    LimparFormulario();
                    HabilitarFormulario(false);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao excluir produto: {ex.Message}", "Erro", 
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
                    AtualizarProduto();
                }
                else
                {
                    InserirProduto();
                }

                MessageBox.Show($"Produto {(modoEdicao ? "atualizado" : "cadastrado")} com sucesso!", "Sucesso", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                
                CarregarProdutos();
                LimparFormulario();
                HabilitarFormulario(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar produto: {ex.Message}", "Erro", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            LimparFormulario();
            HabilitarFormulario(false);
        }

        private bool ValidarFormulario()
        {
            if (string.IsNullOrWhiteSpace(txtNomeProduto.Text))
            {
                MessageBox.Show("Digite o nome do produto!", "Validação", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtNomeProduto.Focus();
                return false;
            }

            if (cmbCategoria.SelectedIndex == -1)
            {
                MessageBox.Show("Selecione uma categoria!", "Validação", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbCategoria.Focus();
                return false;
            }

            decimal precoCompra;
            if (!decimal.TryParse(txtPrecoCompra.Text, out precoCompra) || precoCompra < 0)
            {
                MessageBox.Show("Preço de compra deve ser um valor válido!", "Validação", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPrecoCompra.Focus();
                return false;
            }

            decimal precoVenda;
            if (!decimal.TryParse(txtPrecoVenda.Text, out precoVenda) || precoVenda < 0)
            {
                MessageBox.Show("Preço de venda deve ser um valor válido!", "Validação", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPrecoVenda.Focus();
                return false;
            }

            int estoque;
            if (!int.TryParse(txtEstoqueInicial.Text, out estoque) || estoque < 0)
            {
                MessageBox.Show("Estoque inicial deve ser um valor válido!", "Validação", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtEstoqueInicial.Focus();
                return false;
            }

            if (cmbUnidade.SelectedIndex == -1)
            {
                MessageBox.Show("Selecione uma unidade!", "Validação", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbUnidade.Focus();
                return false;
            }

            return true;
        }

        private void InserirProduto()
        {
            int idCategoria = categorias[cmbCategoria.SelectedIndex].Id;
            string unidade = (cmbUnidade.SelectedItem as ComboBoxItem)?.Content.ToString();

            string query = @"
                INSERT INTO Produtos (Nome, CodigoBarra, IdCategoria, PrecoCompra, PrecoVenda, 
                                    EstoqueAtual, Unidade, Ativo)
                VALUES (@Nome, @CodigoBarra, @IdCategoria, @PrecoCompra, @PrecoVenda, 
                        @EstoqueAtual, @Unidade, @Ativo)";

            SqlParameter[] parameters = {
                new SqlParameter("@Nome", txtNomeProduto.Text.Trim()),
                new SqlParameter("@CodigoBarra", txtCodigoBarra.Text.Trim()),
                new SqlParameter("@IdCategoria", idCategoria),
                new SqlParameter("@PrecoCompra", decimal.Parse(txtPrecoCompra.Text)),
                new SqlParameter("@PrecoVenda", decimal.Parse(txtPrecoVenda.Text)),
                new SqlParameter("@EstoqueAtual", int.Parse(txtEstoqueInicial.Text)),
                new SqlParameter("@Unidade", unidade),
                new SqlParameter("@Ativo", chkAtivo.IsChecked ?? true)
            };

            DatabaseConnection.ExecuteNonQuery(query, parameters);

            // Se há estoque inicial, registrar movimentação
            if (int.Parse(txtEstoqueInicial.Text) > 0)
            {
                RegistrarMovimentacaoEstoque(int.Parse(txtEstoqueInicial.Text), "Entrada", "Estoque inicial");
            }
        }

        private void AtualizarProduto()
        {
            int idCategoria = categorias[cmbCategoria.SelectedIndex].Id;
            string unidade = (cmbUnidade.SelectedItem as ComboBoxItem)?.Content.ToString();

            string query = @"
                UPDATE Produtos 
                SET Nome = @Nome, CodigoBarra = @CodigoBarra, IdCategoria = @IdCategoria,
                    PrecoCompra = @PrecoCompra, PrecoVenda = @PrecoVenda, 
                    Unidade = @Unidade, Ativo = @Ativo
                WHERE Id = @Id";

            SqlParameter[] parameters = {
                new SqlParameter("@Id", produtoSelecionado.Id),
                new SqlParameter("@Nome", txtNomeProduto.Text.Trim()),
                new SqlParameter("@CodigoBarra", txtCodigoBarra.Text.Trim()),
                new SqlParameter("@IdCategoria", idCategoria),
                new SqlParameter("@PrecoCompra", decimal.Parse(txtPrecoCompra.Text)),
                new SqlParameter("@PrecoVenda", decimal.Parse(txtPrecoVenda.Text)),
                new SqlParameter("@Unidade", unidade),
                new SqlParameter("@Ativo", chkAtivo.IsChecked ?? true)
            };

            DatabaseConnection.ExecuteNonQuery(query, parameters);
        }

        private void RegistrarMovimentacaoEstoque(int quantidade, string tipo, string motivo)
        {
            try
            {
                string query = @"
                    INSERT INTO MovimentacaoEstoque (IdProduto, Tipo, Quantidade, Motivo, DataMovimentacao)
                    VALUES (@IdProduto, @Tipo, @Quantidade, @Motivo, @DataMovimentacao)";

                SqlParameter[] parameters = {
                    new SqlParameter("@IdProduto", produtoSelecionado?.Id ?? 0),
                    new SqlParameter("@Tipo", tipo),
                    new SqlParameter("@Quantidade", quantidade),
                    new SqlParameter("@Motivo", motivo),
                    new SqlParameter("@DataMovimentacao", DateTime.Now)
                };

                DatabaseConnection.ExecuteNonQuery(query, parameters);
            }
            catch (Exception ex)
            {
                // Log do erro mas não interromper o fluxo principal
                System.Diagnostics.Debug.WriteLine($"Erro ao registrar movimentação: {ex.Message}");
            }
        }

        private void PreencherFormulario(Produto produto)
        {
            txtNomeProduto.Text = produto.Nome;
            txtCodigoBarra.Text = produto.CodigoBarra;
            txtPrecoCompra.Text = produto.PrecoCompra.ToString("N2");
            txtPrecoVenda.Text = produto.PrecoVenda.ToString("N2");
            txtEstoqueInicial.Text = produto.EstoqueAtual.ToString();
            chkAtivo.IsChecked = produto.Ativo;

            // Selecionar categoria
            var categoria = categorias.FirstOrDefault(c => c.Id == produto.IdCategoria);
            if (categoria != null)
            {
                cmbCategoria.SelectedItem = categoria;
            }

            // Selecionar unidade
            foreach (ComboBoxItem item in cmbUnidade.Items)
            {
                if (item.Content.ToString() == produto.Unidade)
                {
                    cmbUnidade.SelectedItem = item;
                    break;
                }
            }
        }

        private void LimparFormulario()
        {
            txtNomeProduto.Text = "";
            txtCodigoBarra.Text = "";
            txtPrecoCompra.Text = "0,00";
            txtPrecoVenda.Text = "0,00";
            txtEstoqueInicial.Text = "0";
            chkAtivo.IsChecked = true;
            cmbCategoria.SelectedIndex = -1;
            cmbUnidade.SelectedIndex = 0; // Selecionar "Unidade" por padrão
            txtStatus.Text = "";
        }

        private void HabilitarFormulario(bool habilitado)
        {
            txtNomeProduto.IsEnabled = habilitado;
            txtCodigoBarra.IsEnabled = habilitado;
            cmbCategoria.IsEnabled = habilitado;
            txtPrecoCompra.IsEnabled = habilitado;
            txtPrecoVenda.IsEnabled = habilitado;
            txtEstoqueInicial.IsEnabled = habilitado;
            cmbUnidade.IsEnabled = habilitado;
            chkAtivo.IsEnabled = habilitado;
            btnSalvar.IsEnabled = habilitado;
            btnCancelar.IsEnabled = habilitado;
        }
    }
} 