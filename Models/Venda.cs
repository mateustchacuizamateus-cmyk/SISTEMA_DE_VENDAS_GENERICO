using System;
using System.Collections.Generic;

namespace SISTEMA_DE_VENDAS_GENERICO.Models
{
    public class Venda
    {
        public int Id { get; set; }
        public int? IdCliente { get; set; }
        public int IdUsuario { get; set; }
        public DateTime DataVenda { get; set; }
        public decimal Total { get; set; }
        public string TipoPagamento { get; set; }
        public decimal Desconto { get; set; }
        public string Observacoes { get; set; }

        // Propriedades de navegação
        public string NomeCliente { get; set; }
        public string NomeUsuario { get; set; }
        public List<ItemVenda> Itens { get; set; }

        public Venda()
        {
            DataVenda = DateTime.Now;
            Itens = new List<ItemVenda>();
            Desconto = 0;
        }
    }
} 