using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SISTEMA_DE_VENDAS_GENERICO.Models
{
    /// <summary>
    /// Modelo de dados para Usuários do sistema
    /// Implementa INotifyPropertyChanged para binding automático com WPF
    /// Inclui validações e propriedades específicas para o mercado angolano
    /// </summary>
    public class Usuario : INotifyPropertyChanged
    {
        #region Campos Privados
        
        private int _id;
        private string _nome;
        private string _nomeUsuario;
        private string _email;
        private string _senha;
        private string _nivelAcesso;
        private bool _ativo;
        private DateTime _dataCadastro;
        private DateTime? _dataUltimoLogin;
        private int _tentativasLogin;
        private bool _contaBloqueada;
        private DateTime? _dataBloqueio;
        private string _telefone;
        private string _telefoneAlternativo;
        private string _endereco;
        private string _cidade;
        private string _provincia;
        private string _codigoPostal;
        private string _idiomaPreferido;
        private string _temaInterface;
        private string _observacoes;
        
        #endregion

        #region Propriedades Públicas

        /// <summary>
        /// Identificador único do usuário
        /// </summary>
        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        /// <summary>
        /// Nome completo do usuário
        /// </summary>
        public string Nome
        {
            get => _nome;
            set => SetProperty(ref _nome, value?.Trim());
        }

        /// <summary>
        /// Nome de usuário para login (único no sistema)
        /// </summary>
        public string NomeUsuario
        {
            get => _nomeUsuario;
            set => SetProperty(ref _nomeUsuario, value?.Trim()?.ToLower());
        }

        /// <summary>
        /// Endereço de email do usuário
        /// </summary>
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value?.Trim()?.ToLower());
        }

        /// <summary>
        /// Senha do usuário (será criptografada em versões futuras)
        /// </summary>
        public string Senha
        {
            get => _senha;
            set => SetProperty(ref _senha, value);
        }

        /// <summary>
        /// Nível de acesso do usuário no sistema
        /// Valores possíveis: Administrador, Gerente, Vendedor, Operador, Consulta
        /// </summary>
        public string NivelAcesso
        {
            get => _nivelAcesso;
            set => SetProperty(ref _nivelAcesso, value);
        }

        /// <summary>
        /// Indica se o usuário está ativo no sistema
        /// </summary>
        public bool Ativo
        {
            get => _ativo;
            set => SetProperty(ref _ativo, value);
        }

        /// <summary>
        /// Data de cadastro do usuário
        /// </summary>
        public DateTime DataCadastro
        {
            get => _dataCadastro;
            set => SetProperty(ref _dataCadastro, value);
        }

        /// <summary>
        /// Data e hora do último login realizado
        /// </summary>
        public DateTime? DataUltimoLogin
        {
            get => _dataUltimoLogin;
            set => SetProperty(ref _dataUltimoLogin, value);
        }

        /// <summary>
        /// Número de tentativas de login incorretas
        /// </summary>
        public int TentativasLogin
        {
            get => _tentativasLogin;
            set => SetProperty(ref _tentativasLogin, value);
        }

        /// <summary>
        /// Indica se a conta está bloqueada por excesso de tentativas
        /// </summary>
        public bool ContaBloqueada
        {
            get => _contaBloqueada;
            set => SetProperty(ref _contaBloqueada, value);
        }

        /// <summary>
        /// Data em que a conta foi bloqueada
        /// </summary>
        public DateTime? DataBloqueio
        {
            get => _dataBloqueio;
            set => SetProperty(ref _dataBloqueio, value);
        }

        /// <summary>
        /// Telefone principal do usuário (formato angolano)
        /// </summary>
        public string Telefone
        {
            get => _telefone;
            set => SetProperty(ref _telefone, FormatarTelefoneAngolano(value));
        }

        /// <summary>
        /// Telefone alternativo do usuário
        /// </summary>
        public string TelefoneAlternativo
        {
            get => _telefoneAlternativo;
            set => SetProperty(ref _telefoneAlternativo, FormatarTelefoneAngolano(value));
        }

        /// <summary>
        /// Endereço completo do usuário
        /// </summary>
        public string Endereco
        {
            get => _endereco;
            set => SetProperty(ref _endereco, value?.Trim());
        }

        /// <summary>
        /// Cidade onde o usuário reside (padrão: Luanda)
        /// </summary>
        public string Cidade
        {
            get => _cidade;
            set => SetProperty(ref _cidade, value?.Trim());
        }

        /// <summary>
        /// Província onde o usuário reside (padrão: Luanda)
        /// </summary>
        public string Provincia
        {
            get => _provincia;
            set => SetProperty(ref _provincia, value?.Trim());
        }

        /// <summary>
        /// Código postal do endereço
        /// </summary>
        public string CodigoPostal
        {
            get => _codigoPostal;
            set => SetProperty(ref _codigoPostal, value?.Trim());
        }

        /// <summary>
        /// Idioma preferido do usuário (padrão: pt-AO)
        /// </summary>
        public string IdiomaPreferido
        {
            get => _idiomaPreferido;
            set => SetProperty(ref _idiomaPreferido, value);
        }

        /// <summary>
        /// Tema da interface preferido pelo usuário
        /// </summary>
        public string TemaInterface
        {
            get => _temaInterface;
            set => SetProperty(ref _temaInterface, value);
        }

        /// <summary>
        /// Observações adicionais sobre o usuário
        /// </summary>
        public string Observacoes
        {
            get => _observacoes;
            set => SetProperty(ref _observacoes, value?.Trim());
        }

        #endregion

        #region Propriedades Calculadas

        /// <summary>
        /// Nome de exibição formatado para a interface
        /// </summary>
        public string NomeExibicao => !string.IsNullOrEmpty(Nome) ? Nome : NomeUsuario;

        /// <summary>
        /// Iniciais do nome do usuário
        /// </summary>
        public string Iniciais
        {
            get
            {
                if (string.IsNullOrEmpty(Nome))
                    return "??";

                var partes = Nome.Split(' ');
                if (partes.Length == 1)
                    return partes[0].Substring(0, Math.Min(2, partes[0].Length)).ToUpper();

                return (partes[0].Substring(0, 1) + partes[partes.Length - 1].Substring(0, 1)).ToUpper();
            }
        }

        /// <summary>
        /// Status formatado do usuário
        /// </summary>
        public string StatusFormatado
        {
            get
            {
                if (ContaBloqueada)
                    return "Bloqueado";
                
                return Ativo ? "Ativo" : "Inativo";
            }
        }

        /// <summary>
        /// Cor do status para exibição na interface
        /// </summary>
        public string CorStatus
        {
            get
            {
                if (ContaBloqueada)
                    return "#E74C3C"; // Vermelho
                
                return Ativo ? "#27AE60" : "#95A5A6"; // Verde ou Cinza
            }
        }

        /// <summary>
        /// Indica se o usuário tem permissões administrativas
        /// </summary>
        public bool IsAdministrador => NivelAcesso == "Administrador";

        /// <summary>
        /// Indica se o usuário tem permissões de gerente
        /// </summary>
        public bool IsGerente => NivelAcesso == "Gerente" || IsAdministrador;

        /// <summary>
        /// Indica se o usuário pode realizar vendas
        /// </summary>
        public bool PodeVender => NivelAcesso == "Vendedor" || IsGerente;

        /// <summary>
        /// Endereço completo formatado
        /// </summary>
        public string EnderecoCompleto
        {
            get
            {
                var endereco = new System.Text.StringBuilder();
                
                if (!string.IsNullOrEmpty(Endereco))
                    endereco.Append(Endereco);
                
                if (!string.IsNullOrEmpty(Cidade))
                {
                    if (endereco.Length > 0) endereco.Append(", ");
                    endereco.Append(Cidade);
                }
                
                if (!string.IsNullOrEmpty(Provincia))
                {
                    if (endereco.Length > 0) endereco.Append(", ");
                    endereco.Append(Provincia);
                }
                
                return endereco.ToString();
            }
        }

        #endregion

        #region Construtores

        /// <summary>
        /// Construtor padrão
        /// Inicializa com valores padrão para Angola
        /// </summary>
        public Usuario()
        {
            _dataCadastro = DateTime.Now;
            _ativo = true;
            _tentativasLogin = 0;
            _contaBloqueada = false;
            _cidade = "Luanda";
            _provincia = "Luanda";
            _idiomaPreferido = "pt-AO";
            _temaInterface = "Claro";
            _nivelAcesso = "Vendedor";
        }

        /// <summary>
        /// Construtor com parâmetros básicos
        /// </summary>
        /// <param name="nome">Nome completo</param>
        /// <param name="nomeUsuario">Nome de usuário</param>
        /// <param name="senha">Senha</param>
        /// <param name="nivelAcesso">Nível de acesso</param>
        public Usuario(string nome, string nomeUsuario, string senha, string nivelAcesso) : this()
        {
            Nome = nome;
            NomeUsuario = nomeUsuario;
            Senha = senha;
            NivelAcesso = nivelAcesso;
        }

        #endregion

        #region Métodos de Validação

        /// <summary>
        /// Valida se os dados do usuário estão corretos
        /// </summary>
        /// <returns>Lista de erros encontrados</returns>
        public System.Collections.Generic.List<string> Validar()
        {
            var erros = new System.Collections.Generic.List<string>();

            // Validar nome
            if (string.IsNullOrWhiteSpace(Nome))
                erros.Add("Nome é obrigatório");
            else if (Nome.Length < 2)
                erros.Add("Nome deve ter pelo menos 2 caracteres");
            else if (Nome.Length > 150)
                erros.Add("Nome não pode ter mais de 150 caracteres");

            // Validar nome de usuário
            if (string.IsNullOrWhiteSpace(NomeUsuario))
                erros.Add("Nome de usuário é obrigatório");
            else if (NomeUsuario.Length < 3)
                erros.Add("Nome de usuário deve ter pelo menos 3 caracteres");
            else if (NomeUsuario.Length > 50)
                erros.Add("Nome de usuário não pode ter mais de 50 caracteres");
            else if (!System.Text.RegularExpressions.Regex.IsMatch(NomeUsuario, @"^[a-zA-Z0-9._-]+$"))
                erros.Add("Nome de usuário pode conter apenas letras, números, pontos, hífens e sublinhados");

            // Validar senha
            if (string.IsNullOrWhiteSpace(Senha))
                erros.Add("Senha é obrigatória");
            else if (Senha.Length < 6)
                erros.Add("Senha deve ter pelo menos 6 caracteres");
            else if (Senha.Length > 255)
                erros.Add("Senha não pode ter mais de 255 caracteres");

            // Validar email se fornecido
            if (!string.IsNullOrWhiteSpace(Email))
            {
                if (!IsEmailValido(Email))
                    erros.Add("Email inválido");
            }

            // Validar nível de acesso
            var niveisValidos = new[] { "Administrador", "Gerente", "Vendedor", "Operador", "Consulta" };
            if (!niveisValidos.Contains(NivelAcesso))
                erros.Add("Nível de acesso inválido");

            // Validar telefone se fornecido
            if (!string.IsNullOrWhiteSpace(Telefone))
            {
                if (!IsTelefoneAngolanoValido(Telefone))
                    erros.Add("Telefone inválido para Angola");
            }

            return erros;
        }

        /// <summary>
        /// Verifica se o email é válido
        /// </summary>
        /// <param name="email">Email a ser validado</param>
        /// <returns>True se válido</returns>
        private bool IsEmailValido(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Verifica se o telefone é válido para Angola
        /// </summary>
        /// <param name="telefone">Telefone a ser validado</param>
        /// <returns>True se válido</returns>
        private bool IsTelefoneAngolanoValido(string telefone)
        {
            if (string.IsNullOrWhiteSpace(telefone))
                return false;

            // Remover formatação
            var numeroLimpo = telefone.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace("+", "");

            // Verificar padrões angolanos
            // Formato: +244 9XX XXX XXX ou 9XX XXX XXX
            if (numeroLimpo.StartsWith("244"))
                numeroLimpo = numeroLimpo.Substring(3);

            // Deve ter 9 dígitos e começar com 9
            return numeroLimpo.Length == 9 && numeroLimpo.StartsWith("9") && numeroLimpo.All(char.IsDigit);
        }

        /// <summary>
        /// Formata telefone para o padrão angolano
        /// </summary>
        /// <param name="telefone">Telefone a ser formatado</param>
        /// <returns>Telefone formatado</returns>
        private string FormatarTelefoneAngolano(string telefone)
        {
            if (string.IsNullOrWhiteSpace(telefone))
                return telefone;

            // Remover formatação existente
            var numeroLimpo = telefone.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace("+", "");

            // Remover código do país se presente
            if (numeroLimpo.StartsWith("244"))
                numeroLimpo = numeroLimpo.Substring(3);

            // Formatar se for um número angolano válido
            if (numeroLimpo.Length == 9 && numeroLimpo.StartsWith("9") && numeroLimpo.All(char.IsDigit))
            {
                return $"+244 {numeroLimpo.Substring(0, 3)} {numeroLimpo.Substring(3, 3)} {numeroLimpo.Substring(6, 3)}";
            }

            return telefone; // Retornar original se não conseguir formatar
        }

        #endregion

        #region Métodos Utilitários

        /// <summary>
        /// Cria uma cópia do usuário
        /// </summary>
        /// <returns>Nova instância com os mesmos dados</returns>
        public Usuario Clone()
        {
            return new Usuario
            {
                Id = this.Id,
                Nome = this.Nome,
                NomeUsuario = this.NomeUsuario,
                Email = this.Email,
                Senha = this.Senha,
                NivelAcesso = this.NivelAcesso,
                Ativo = this.Ativo,
                DataCadastro = this.DataCadastro,
                DataUltimoLogin = this.DataUltimoLogin,
                TentativasLogin = this.TentativasLogin,
                ContaBloqueada = this.ContaBloqueada,
                DataBloqueio = this.DataBloqueio,
                Telefone = this.Telefone,
                TelefoneAlternativo = this.TelefoneAlternativo,
                Endereco = this.Endereco,
                Cidade = this.Cidade,
                Provincia = this.Provincia,
                CodigoPostal = this.CodigoPostal,
                IdiomaPreferido = this.IdiomaPreferido,
                TemaInterface = this.TemaInterface,
                Observacoes = this.Observacoes
            };
        }

        /// <summary>
        /// Converte o usuário para string de exibição
        /// </summary>
        /// <returns>Representação em string</returns>
        public override string ToString()
        {
            return $"{NomeExibicao} ({NomeUsuario}) - {NivelAcesso}";
        }

        /// <summary>
        /// Verifica se dois usuários são iguais
        /// </summary>
        /// <param name="obj">Objeto a ser comparado</param>
        /// <returns>True se iguais</returns>
        public override bool Equals(object obj)
        {
            if (obj is Usuario outroUsuario)
            {
                return Id == outroUsuario.Id && NomeUsuario == outroUsuario.NomeUsuario;
            }
            return false;
        }

        /// <summary>
        /// Gera hash code para o usuário
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode() ^ (NomeUsuario?.GetHashCode() ?? 0);
        }

        #endregion

        #region INotifyPropertyChanged Implementation

        /// <summary>
        /// Evento disparado quando uma propriedade é alterada
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notifica que uma propriedade foi alterada
        /// </summary>
        /// <param name="propertyName">Nome da propriedade</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Define o valor de uma propriedade e notifica se houve alteração
        /// </summary>
        /// <typeparam name="T">Tipo da propriedade</typeparam>
        /// <param name="field">Campo de apoio</param>
        /// <param name="value">Novo valor</param>
        /// <param name="propertyName">Nome da propriedade</param>
        /// <returns>True se o valor foi alterado</returns>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (System.Collections.Generic.EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        #endregion
    }
}