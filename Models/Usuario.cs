using System;

namespace SISTEMA_DE_VENDAS_GENERICO.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string NomeUsuario { get; set; }
        public string Senha { get; set; }
        public string NivelAcesso { get; set; }
        public bool Ativo { get; set; }
        public DateTime DataCadastro { get; set; }

        public Usuario()
        {
            DataCadastro = DateTime.Now;
            Ativo = true;
        }
    }
} 