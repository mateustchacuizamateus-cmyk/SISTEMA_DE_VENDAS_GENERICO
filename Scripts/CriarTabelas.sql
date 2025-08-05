-- Script para criar as tabelas do Sistema de Vendas Genérico
-- Execute este script antes do InserirDadosTeste.sql

-- Criar tabela de Categorias
CREATE TABLE Categorias (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Nome NVARCHAR(100) NOT NULL,
    Descricao NVARCHAR(255),
    Ativo BIT DEFAULT 1
);

-- Criar tabela de Produtos
CREATE TABLE Produtos (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Nome NVARCHAR(150) NOT NULL,
    CodigoBarra NVARCHAR(50),
    IdCategoria INT FOREIGN KEY REFERENCES Categorias(Id),
    PrecoCompra DECIMAL(18,2) NOT NULL,
    PrecoVenda DECIMAL(18,2) NOT NULL,
    EstoqueAtual INT DEFAULT 0,
    Unidade NVARCHAR(20) DEFAULT 'Unidade',
    Ativo BIT DEFAULT 1,
    Imagem VARBINARY(MAX)
);

-- Criar tabela de Usuários
CREATE TABLE Usuarios (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Nome NVARCHAR(100) NOT NULL,
    Usuario NVARCHAR(50) NOT NULL UNIQUE,
    Senha NVARCHAR(25) NOT NULL,
    NivelAcesso NVARCHAR(20) NOT NULL,
    Ativo BIT DEFAULT 1,
    DataCadastro DATETIME DEFAULT GETDATE()
);

-- Criar tabela de Clientes
CREATE TABLE Clientes (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Nome NVARCHAR(150) NOT NULL,
    Telefone NVARCHAR(20),
    Email NVARCHAR(100),
    Endereco NVARCHAR(255),
    DataCadastro DATETIME DEFAULT GETDATE()
);

-- Criar tabela de Fornecedores
CREATE TABLE Fornecedores (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Nome NVARCHAR(150) NOT NULL,
    Telefone NVARCHAR(20),
    Email NVARCHAR(100),
    Endereco NVARCHAR(255)
);

-- Criar tabela de Formas de Pagamento
CREATE TABLE FormasPagamento (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Descricao NVARCHAR(100) NOT NULL
);

-- Criar tabela de Configurações
CREATE TABLE Configuracoes (
    Id INT PRIMARY KEY IDENTITY(1,1),
    NomeEmpresa NVARCHAR(150),
    Endereco NVARCHAR(200),
    Telefone NVARCHAR(50),
    Email NVARCHAR(100),
    Logotipo VARBINARY(MAX)
);

-- Criar tabela de Vendas
CREATE TABLE Vendas (
    Id INT PRIMARY KEY IDENTITY(1,1),
    IdCliente INT FOREIGN KEY REFERENCES Clientes(Id),
    IdUsuario INT FOREIGN KEY REFERENCES Usuarios(Id) NOT NULL,
    DataVenda DATETIME DEFAULT GETDATE(),
    Total DECIMAL(18,2) NOT NULL,
    TipoPagamento NVARCHAR(50),
    Desconto DECIMAL(18,2) DEFAULT 0,
    Observacoes NVARCHAR(255)
);

-- Criar tabela de Itens de Venda
CREATE TABLE ItensVenda (
    Id INT PRIMARY KEY IDENTITY(1,1),
    IdVenda INT FOREIGN KEY REFERENCES Vendas(Id) NOT NULL,
    IdProduto INT FOREIGN KEY REFERENCES Produtos(Id) NOT NULL,
    Quantidade INT NOT NULL,
    PrecoUnitario DECIMAL(18,2) NOT NULL,
    TotalItem DECIMAL(18,2) NOT NULL
);

-- Criar tabela de Movimentação de Estoque
CREATE TABLE MovimentacaoEstoque (
    Id INT PRIMARY KEY IDENTITY(1,1),
    IdProduto INT FOREIGN KEY REFERENCES Produtos(Id) NOT NULL,
    Tipo NVARCHAR(20) NOT NULL, -- 'Entrada' ou 'Saída'
    Quantidade INT NOT NULL,
    Motivo NVARCHAR(100),
    DataMovimentacao DATETIME DEFAULT GETDATE(),
    IdUsuario INT FOREIGN KEY REFERENCES Usuarios(Id)
);

-- Criar índices para melhor performance
CREATE INDEX IX_Produtos_CodigoBarra ON Produtos(CodigoBarra);
CREATE INDEX IX_Produtos_Categoria ON Produtos(IdCategoria);
CREATE INDEX IX_Vendas_Data ON Vendas(DataVenda);
CREATE INDEX IX_Vendas_Cliente ON Vendas(IdCliente);
CREATE INDEX IX_ItensVenda_Venda ON ItensVenda(IdVenda);
CREATE INDEX IX_ItensVenda_Produto ON ItensVenda(IdProduto);
CREATE INDEX IX_MovimentacaoEstoque_Produto ON MovimentacaoEstoque(IdProduto);
CREATE INDEX IX_MovimentacaoEstoque_Data ON MovimentacaoEstoque(DataMovimentacao);

PRINT 'Tabelas criadas com sucesso!'
PRINT 'Execute o script InserirDadosTeste.sql para inserir dados de exemplo.' 