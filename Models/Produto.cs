using System;

namespace SISTEMA_DE_VENDAS_GENERICO.Models
{
    public class Produto
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string CodigoBarra { get; set; }
        public int IdCategoria { get; set; }
        public decimal PrecoCompra { get; set; }
        public decimal PrecoVenda { get; set; }
        public int EstoqueAtual { get; set; }
        public string Unidade { get; set; }
        public bool Ativo { get; set; }
        public byte[] Imagem { get; set; }
        
        // Propriedades de navegação
        public string NomeCategoria { get; set; }
    }
} 