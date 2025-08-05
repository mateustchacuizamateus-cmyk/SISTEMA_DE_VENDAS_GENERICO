namespace SISTEMA_DE_VENDAS_GENERICO.Models
{
    public class ItemVenda
    {
        public int Id { get; set; }
        public int IdVenda { get; set; }
        public int IdProduto { get; set; }
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }
        public decimal TotalItem { get; set; }

        // Propriedades de navegação
        public string NomeProduto { get; set; }
        public string CodigoBarra { get; set; }
    }
} 