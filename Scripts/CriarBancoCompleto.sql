-- Script completo para criar o banco de dados SistemaVendas
-- Execute este script no SQL Server Management Studio ou no Visual Studio

-- Criar banco de dados se não existir
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'SistemaVendas')
BEGIN
    CREATE DATABASE SistemaVendas;
    PRINT 'Banco de dados SistemaVendas criado com sucesso!';
END
ELSE
BEGIN
    PRINT 'Banco de dados SistemaVendas já existe.';
END
GO

USE SistemaVendas;
GO

-- Tabela de Usuários
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Usuarios]') AND type in (N'U'))
BEGIN
    CREATE TABLE Usuarios (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Nome NVARCHAR(100) NOT NULL,
        Usuario NVARCHAR(50) UNIQUE NOT NULL,
        Senha NVARCHAR(25) NOT NULL,
        NivelAcesso NVARCHAR(20) NOT NULL CHECK (NivelAcesso IN ('Administrador', 'Vendedor', 'Gerente')),
        Ativo BIT DEFAULT 1,
        DataCadastro DATETIME DEFAULT GETDATE()
    );
    PRINT 'Tabela Usuarios criada com sucesso!';
END
ELSE
BEGIN
    PRINT 'Tabela Usuarios já existe.';
END
GO

-- Tabela de Categorias de Produtos
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Categorias]') AND type in (N'U'))
BEGIN
    CREATE TABLE Categorias (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Nome NVARCHAR(100) NOT NULL,
        Descricao NVARCHAR(255)
    );
    PRINT 'Tabela Categorias criada com sucesso!';
END
ELSE
BEGIN
    PRINT 'Tabela Categorias já existe.';
END
GO

-- Tabela de Produtos
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Produtos]') AND type in (N'U'))
BEGIN
    CREATE TABLE Produtos (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Nome NVARCHAR(150) NOT NULL,
        CodigoBarra NVARCHAR(50),
        IdCategoria INT FOREIGN KEY REFERENCES Categorias(Id),
        PrecoCompra DECIMAL(18,2) NOT NULL,
        PrecoVenda DECIMAL(18,2) NOT NULL,
        EstoqueAtual INT DEFAULT 0,
        Unidade NVARCHAR(20),
        Ativo BIT DEFAULT 1,
        Imagem VARBINARY(MAX)
    );
    PRINT 'Tabela Produtos criada com sucesso!';
END
ELSE
BEGIN
    PRINT 'Tabela Produtos já existe.';
END
GO

-- Tabela de Clientes
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Clientes]') AND type in (N'U'))
BEGIN
    CREATE TABLE Clientes (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Nome NVARCHAR(150) NOT NULL,
        Telefone NVARCHAR(20),
        Email NVARCHAR(100),
        Endereco NVARCHAR(255),
        DataCadastro DATETIME DEFAULT GETDATE()
    );
    PRINT 'Tabela Clientes criada com sucesso!';
END
ELSE
BEGIN
    PRINT 'Tabela Clientes já existe.';
END
GO

-- Tabela de Fornecedores
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Fornecedores]') AND type in (N'U'))
BEGIN
    CREATE TABLE Fornecedores (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Nome NVARCHAR(150) NOT NULL,
        Telefone NVARCHAR(20),
        Email NVARCHAR(100),
        Endereco NVARCHAR(255)
    );
    PRINT 'Tabela Fornecedores criada com sucesso!';
END
ELSE
BEGIN
    PRINT 'Tabela Fornecedores já existe.';
END
GO

-- Tabela de Vendas
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Vendas]') AND type in (N'U'))
BEGIN
    CREATE TABLE Vendas (
        Id INT PRIMARY KEY IDENTITY(1,1),
        IdCliente INT NULL FOREIGN KEY REFERENCES Clientes(Id),
        IdUsuario INT NOT NULL FOREIGN KEY REFERENCES Usuarios(Id),
        DataVenda DATETIME DEFAULT GETDATE(),
        Total DECIMAL(18,2) NOT NULL,
        TipoPagamento NVARCHAR(50),
        Desconto DECIMAL(18,2) DEFAULT 0,
        Observacoes NVARCHAR(255)
    );
    PRINT 'Tabela Vendas criada com sucesso!';
END
ELSE
BEGIN
    PRINT 'Tabela Vendas já existe.';
END
GO

-- Tabela de Itens da Venda
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ItensVenda]') AND type in (N'U'))
BEGIN
    CREATE TABLE ItensVenda (
        Id INT PRIMARY KEY IDENTITY(1,1),
        IdVenda INT NOT NULL FOREIGN KEY REFERENCES Vendas(Id),
        IdProduto INT NOT NULL FOREIGN KEY REFERENCES Produtos(Id),
        Quantidade INT NOT NULL,
        PrecoUnitario DECIMAL(18,2) NOT NULL,
        TotalItem DECIMAL(18,2) NOT NULL
    );
    PRINT 'Tabela ItensVenda criada com sucesso!';
END
ELSE
BEGIN
    PRINT 'Tabela ItensVenda já existe.';
END
GO

-- Tabela de Formas de Pagamento
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FormasPagamento]') AND type in (N'U'))
BEGIN
    CREATE TABLE FormasPagamento (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Descricao NVARCHAR(100) NOT NULL
    );
    PRINT 'Tabela FormasPagamento criada com sucesso!';
END
ELSE
BEGIN
    PRINT 'Tabela FormasPagamento já existe.';
END
GO

-- Tabela de Movimentação de Estoque
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MovimentacaoEstoque]') AND type in (N'U'))
BEGIN
    CREATE TABLE MovimentacaoEstoque (
        Id INT PRIMARY KEY IDENTITY(1,1),
        IdProduto INT NOT NULL FOREIGN KEY REFERENCES Produtos(Id),
        Tipo NVARCHAR(20) NOT NULL CHECK (Tipo IN ('Entrada', 'Saida')),
        Quantidade INT NOT NULL,
        Motivo NVARCHAR(100),
        DataMovimentacao DATETIME DEFAULT GETDATE()
    );
    PRINT 'Tabela MovimentacaoEstoque criada com sucesso!';
END
ELSE
BEGIN
    PRINT 'Tabela MovimentacaoEstoque já existe.';
END
GO

-- Tabela de Configurações Gerais do Sistema
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Configuracoes]') AND type in (N'U'))
BEGIN
    CREATE TABLE Configuracoes (
        Id INT PRIMARY KEY IDENTITY(1,1),
        NomeEmpresa NVARCHAR(150),
        Endereco NVARCHAR(200),
        Telefone NVARCHAR(50),
        Email NVARCHAR(100),
        Logotipo VARBINARY(MAX)
    );
    PRINT 'Tabela Configuracoes criada com sucesso!';
END
ELSE
BEGIN
    PRINT 'Tabela Configuracoes já existe.';
END
GO

-- Inserir dados iniciais

-- Inserir usuários padrão
IF NOT EXISTS (SELECT * FROM Usuarios WHERE Usuario = 'admin')
BEGIN
    INSERT INTO Usuarios (Nome, Usuario, Senha, NivelAcesso, Ativo, DataCadastro)
    VALUES ('Administrador', 'admin', 'admin123', 'Administrador', 1, GETDATE());
    PRINT 'Usuário administrador criado.';
END

IF NOT EXISTS (SELECT * FROM Usuarios WHERE Usuario = 'vendedor')
BEGIN
    INSERT INTO Usuarios (Nome, Usuario, Senha, NivelAcesso, Ativo, DataCadastro)
    VALUES ('Vendedor', 'vendedor', 'venda123', 'Vendedor', 1, GETDATE());
    PRINT 'Usuário vendedor criado.';
END

IF NOT EXISTS (SELECT * FROM Usuarios WHERE Usuario = 'gerente')
BEGIN
    INSERT INTO Usuarios (Nome, Usuario, Senha, NivelAcesso, Ativo, DataCadastro)
    VALUES ('Gerente', 'gerente', 'gerente123', 'Gerente', 1, GETDATE());
    PRINT 'Usuário gerente criado.';
END

-- Inserir formas de pagamento padrão
IF NOT EXISTS (SELECT * FROM FormasPagamento WHERE Descricao = 'Dinheiro')
BEGIN
    INSERT INTO FormasPagamento (Descricao) VALUES ('Dinheiro');
END

IF NOT EXISTS (SELECT * FROM FormasPagamento WHERE Descricao = 'Cartão de Crédito')
BEGIN
    INSERT INTO FormasPagamento (Descricao) VALUES ('Cartão de Crédito');
END

IF NOT EXISTS (SELECT * FROM FormasPagamento WHERE Descricao = 'Cartão de Débito')
BEGIN
    INSERT INTO FormasPagamento (Descricao) VALUES ('Cartão de Débito');
END

IF NOT EXISTS (SELECT * FROM FormasPagamento WHERE Descricao = 'PIX')
BEGIN
    INSERT INTO FormasPagamento (Descricao) VALUES ('PIX');
END

-- Inserir categorias padrão
IF NOT EXISTS (SELECT * FROM Categorias WHERE Nome = 'Geral')
BEGIN
    INSERT INTO Categorias (Nome, Descricao) VALUES ('Geral', 'Categoria geral para produtos');
END

PRINT 'Script de criação do banco de dados concluído com sucesso!';
PRINT '';
PRINT 'Usuários de teste criados:';
PRINT '• Administrador: admin / admin123';
PRINT '• Vendedor: vendedor / venda123';
PRINT '• Gerente: gerente / gerente123';
PRINT '';
PRINT 'O sistema está pronto para uso!';
GO 