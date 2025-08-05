-- =====================================================
-- SISTEMA DE VENDAS GENÉRICO - REPÚBLICA DE ANGOLA
-- Script de Criação do Banco de Dados Otimizado
-- Versão: 2.0 - Reformulado
-- Data: Janeiro 2025
-- Autor: Sistema de Vendas Angola
-- Descrição: Banco otimizado para o mercado angolano
--            com suporte completo à moeda Kwanza (AOA)
-- =====================================================

-- Verificar e criar banco de dados
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'SistemaVendasAngola')
BEGIN
    CREATE DATABASE SistemaVendasAngola
    COLLATE SQL_Latin1_General_CP1_CI_AS; -- Suporte a caracteres especiais portugueses
    PRINT '✅ Banco de dados SistemaVendasAngola criado com sucesso!';
END
ELSE
BEGIN
    PRINT '⚠️ Banco de dados SistemaVendasAngola já existe.';
END
GO

USE SistemaVendasAngola;
GO

-- =====================================================
-- TABELA DE CONFIGURAÇÕES DO SISTEMA
-- Armazena todas as configurações específicas de Angola
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ConfiguracoesSistema]') AND type in (N'U'))
BEGIN
    CREATE TABLE ConfiguracoesSistema (
        Id INT PRIMARY KEY IDENTITY(1,1),
        ChaveConfiguracao NVARCHAR(100) NOT NULL UNIQUE,
        ValorConfiguracao NVARCHAR(500) NOT NULL,
        Descricao NVARCHAR(300) NOT NULL,
        TipoDado NVARCHAR(20) DEFAULT 'STRING' CHECK (TipoDado IN ('STRING', 'NUMBER', 'BOOLEAN', 'DATE', 'DECIMAL')),
        Categoria NVARCHAR(50) DEFAULT 'GERAL',
        DataCriacao DATETIME2 DEFAULT GETDATE(),
        DataAtualizacao DATETIME2 DEFAULT GETDATE(),
        UsuarioAtualizacao NVARCHAR(100),
        Ativo BIT DEFAULT 1
    );
    
    -- Inserir configurações específicas para Angola
    INSERT INTO ConfiguracoesSistema (ChaveConfiguracao, ValorConfiguracao, Descricao, TipoDado, Categoria) VALUES
    -- Configurações Monetárias
    ('MOEDA_CODIGO', 'AOA', 'Código da moeda oficial de Angola (Kwanza)', 'STRING', 'MONETARIO'),
    ('MOEDA_SIMBOLO', 'Kz', 'Símbolo da moeda Kwanza', 'STRING', 'MONETARIO'),
    ('MOEDA_NOME', 'Kwanza', 'Nome completo da moeda', 'STRING', 'MONETARIO'),
    ('CASAS_DECIMAIS', '2', 'Número de casas decimais para valores monetários', 'NUMBER', 'MONETARIO'),
    ('SEPARADOR_DECIMAL', ',', 'Separador decimal (vírgula para Angola)', 'STRING', 'MONETARIO'),
    ('SEPARADOR_MILHARES', '.', 'Separador de milhares (ponto para Angola)', 'STRING', 'MONETARIO'),
    
    -- Configurações Regionais
    ('PAIS_CODIGO', 'AO', 'Código ISO do país (Angola)', 'STRING', 'REGIONAL'),
    ('PAIS_NOME', 'República de Angola', 'Nome oficial do país', 'STRING', 'REGIONAL'),
    ('IDIOMA_CODIGO', 'pt-AO', 'Código do idioma (Português de Angola)', 'STRING', 'REGIONAL'),
    ('FUSO_HORARIO', 'Africa/Luanda', 'Fuso horário de Angola', 'STRING', 'REGIONAL'),
    ('FORMATO_DATA', 'dd/MM/yyyy', 'Formato de data padrão angolano', 'STRING', 'REGIONAL'),
    ('FORMATO_HORA', 'HH:mm:ss', 'Formato de hora padrão', 'STRING', 'REGIONAL'),
    
    -- Configurações Fiscais
    ('IVA_TAXA_PADRAO', '14.00', 'Taxa de IVA padrão em Angola (14%)', 'DECIMAL', 'FISCAL'),
    ('IVA_TAXA_REDUZIDA', '7.00', 'Taxa de IVA reduzida (7%)', 'DECIMAL', 'FISCAL'),
    ('IVA_ISENTO', '0.00', 'Taxa para produtos isentos de IVA', 'DECIMAL', 'FISCAL'),
    ('NUMERO_CONTRIBUINTE_EMPRESA', '', 'Número de contribuinte da empresa', 'STRING', 'FISCAL'),
    
    -- Configurações do Sistema
    ('VERSAO_SISTEMA', '2.0', 'Versão atual do sistema', 'STRING', 'SISTEMA'),
    ('NOME_SISTEMA', 'Sistema de Vendas Angola', 'Nome do sistema', 'STRING', 'SISTEMA'),
    ('ESTOQUE_MINIMO_ALERTA', '10', 'Quantidade mínima para alerta de estoque', 'NUMBER', 'SISTEMA'),
    ('BACKUP_AUTOMATICO', 'true', 'Ativar backup automático diário', 'BOOLEAN', 'SISTEMA'),
    ('DIAS_BACKUP_MANTER', '30', 'Dias para manter backups antigos', 'NUMBER', 'SISTEMA'),
    
    -- Configurações de Impressão
    ('IMPRESSORA_PADRAO', '', 'Nome da impressora padrão', 'STRING', 'IMPRESSAO'),
    ('FORMATO_PAPEL', 'A4', 'Formato de papel padrão', 'STRING', 'IMPRESSAO'),
    ('MARGEM_SUPERIOR', '2.0', 'Margem superior em cm', 'DECIMAL', 'IMPRESSAO'),
    ('MARGEM_INFERIOR', '2.0', 'Margem inferior em cm', 'DECIMAL', 'IMPRESSAO'),
    ('MARGEM_ESQUERDA', '2.0', 'Margem esquerda em cm', 'DECIMAL', 'IMPRESSAO'),
    ('MARGEM_DIREITA', '2.0', 'Margem direita em cm', 'DECIMAL', 'IMPRESSAO');
    
    -- Índices para performance
    CREATE INDEX IX_ConfiguracoesSistema_Chave ON ConfiguracoesSistema(ChaveConfiguracao);
    CREATE INDEX IX_ConfiguracoesSistema_Categoria ON ConfiguracoesSistema(Categoria);
    CREATE INDEX IX_ConfiguracoesSistema_Ativo ON ConfiguracoesSistema(Ativo);
    
    PRINT '✅ Tabela ConfiguracoesSistema criada com configurações para Angola!';
END
GO

-- =====================================================
-- TABELA DE USUÁRIOS MELHORADA E SEGURA
-- Inclui controle de acesso robusto e auditoria
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Usuarios]') AND type in (N'U'))
BEGIN
    CREATE TABLE Usuarios (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Nome NVARCHAR(150) NOT NULL,
        Usuario NVARCHAR(50) NOT NULL UNIQUE,
        Email NVARCHAR(100) NULL UNIQUE,
        Senha NVARCHAR(255) NOT NULL, -- Para compatibilidade atual
        SenhaHash NVARCHAR(500) NULL, -- Para futuras implementações de hash
        Salt NVARCHAR(100) NULL, -- Salt para hash da senha
        NivelAcesso NVARCHAR(30) NOT NULL DEFAULT 'Vendedor' 
            CHECK (NivelAcesso IN ('Administrador', 'Gerente', 'Vendedor', 'Operador', 'Consulta')),
        
        -- Controle de Status
        Ativo BIT NOT NULL DEFAULT 1,
        ContaBloqueada BIT NOT NULL DEFAULT 0,
        DataBloqueio DATETIME2 NULL,
        MotivoBloqueio NVARCHAR(200) NULL,
        
        -- Controle de Login
        TentativasLogin INT NOT NULL DEFAULT 0,
        DataUltimoLogin DATETIME2 NULL,
        IPUltimoLogin NVARCHAR(45) NULL, -- Suporte IPv6
        
        -- Informações Pessoais
        Telefone NVARCHAR(20) NULL,
        TelefoneAlternativo NVARCHAR(20) NULL,
        Endereco NVARCHAR(300) NULL,
        Cidade NVARCHAR(100) NULL DEFAULT 'Luanda',
        Provincia NVARCHAR(100) NULL DEFAULT 'Luanda',
        CodigoPostal NVARCHAR(20) NULL,
        
        -- Configurações Pessoais
        IdiomaPreferido NVARCHAR(10) DEFAULT 'pt-AO',
        TemaInterface NVARCHAR(20) DEFAULT 'Claro',
        
        -- Auditoria
        DataCriacao DATETIME2 NOT NULL DEFAULT GETDATE(),
        CriadoPor INT NULL,
        DataAtualizacao DATETIME2 NOT NULL DEFAULT GETDATE(),
        AtualizadoPor INT NULL,
        Observacoes NVARCHAR(500) NULL,
        
        -- Constraints de integridade
        CONSTRAINT FK_Usuarios_CriadoPor FOREIGN KEY (CriadoPor) REFERENCES Usuarios(Id),
        CONSTRAINT FK_Usuarios_AtualizadoPor FOREIGN KEY (AtualizadoPor) REFERENCES Usuarios(Id),
        CONSTRAINT CK_Usuarios_Email CHECK (Email IS NULL OR Email LIKE '%@%.%'),
        CONSTRAINT CK_Usuarios_TentativasLogin CHECK (TentativasLogin >= 0),
        CONSTRAINT CK_Usuarios_DataBloqueio CHECK (ContaBloqueada = 0 OR DataBloqueio IS NOT NULL)
    );
    
    -- Índices otimizados para consultas frequentes
    CREATE INDEX IX_Usuarios_Usuario ON Usuarios(Usuario) WHERE Ativo = 1;
    CREATE INDEX IX_Usuarios_Email ON Usuarios(Email) WHERE Email IS NOT NULL;
    CREATE INDEX IX_Usuarios_NivelAcesso ON Usuarios(NivelAcesso, Ativo);
    CREATE INDEX IX_Usuarios_DataUltimoLogin ON Usuarios(DataUltimoLogin DESC);
    CREATE INDEX IX_Usuarios_Cidade_Provincia ON Usuarios(Cidade, Provincia);
    
    PRINT '✅ Tabela Usuarios melhorada e segura criada!';
END
GO

-- =====================================================
-- TABELA DE CATEGORIAS HIERÁRQUICA
-- Suporte a categorias e subcategorias
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Categorias]') AND type in (N'U'))
BEGIN
    CREATE TABLE Categorias (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Nome NVARCHAR(100) NOT NULL,
        Descricao NVARCHAR(300) NULL,
        CodigoCategoria NVARCHAR(20) UNIQUE NULL, -- Código interno da categoria
        
        -- Hierarquia de categorias
        CategoriaPai INT NULL,
        Nivel INT NOT NULL DEFAULT 1, -- Nível na hierarquia (1=raiz, 2=subcategoria, etc.)
        CaminhoHierarquia NVARCHAR(500) NULL, -- Ex: "Alimentação > Bebidas > Refrigerantes"
        
        -- Configurações visuais
        Cor NVARCHAR(7) NULL, -- Cor em hexadecimal (#FF0000)
        Icone NVARCHAR(50) NULL, -- Nome do ícone para interface
        Imagem VARBINARY(MAX) NULL, -- Imagem da categoria
        
        -- Controle e ordenação
        Ativo BIT NOT NULL DEFAULT 1,
        Ordem INT NOT NULL DEFAULT 0, -- Para ordenação personalizada
        Destaque BIT NOT NULL DEFAULT 0, -- Categoria em destaque
        
        -- Configurações fiscais
        TaxaIVAPadrao DECIMAL(5,2) NULL, -- Taxa de IVA padrão para produtos desta categoria
        
        -- Auditoria completa
        DataCriacao DATETIME2 NOT NULL DEFAULT GETDATE(),
        CriadoPor INT NULL,
        DataAtualizacao DATETIME2 NOT NULL DEFAULT GETDATE(),
        AtualizadoPor INT NULL,
        
        -- Constraints
        CONSTRAINT FK_Categorias_CategoriaPai FOREIGN KEY (CategoriaPai) REFERENCES Categorias(Id),
        CONSTRAINT FK_Categorias_CriadoPor FOREIGN KEY (CriadoPor) REFERENCES Usuarios(Id),
        CONSTRAINT FK_Categorias_AtualizadoPor FOREIGN KEY (AtualizadoPor) REFERENCES Usuarios(Id),
        CONSTRAINT CK_Categorias_Nivel CHECK (Nivel > 0 AND Nivel <= 5), -- Máximo 5 níveis
        CONSTRAINT CK_Categorias_TaxaIVA CHECK (TaxaIVAPadrao IS NULL OR (TaxaIVAPadrao >= 0 AND TaxaIVAPadrao <= 100)),
        CONSTRAINT CK_Categorias_Cor CHECK (Cor IS NULL OR Cor LIKE '#[0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f]')
    );
    
    -- Índices otimizados
    CREATE INDEX IX_Categorias_Nome ON Categorias(Nome) WHERE Ativo = 1;
    CREATE INDEX IX_Categorias_CodigoCategoria ON Categorias(CodigoCategoria) WHERE CodigoCategoria IS NOT NULL;
    CREATE INDEX IX_Categorias_CategoriaPai ON Categorias(CategoriaPai, Ordem);
    CREATE INDEX IX_Categorias_Nivel ON Categorias(Nivel, Ativo);
    CREATE INDEX IX_Categorias_Destaque ON Categorias(Destaque, Ativo, Ordem);
    
    PRINT '✅ Tabela Categorias hierárquica criada!';
END
GO

-- =====================================================
-- TABELA DE PRODUTOS COMPLETA E ROBUSTA
-- Inclui controle de estoque, preços e informações fiscais
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Produtos]') AND type in (N'U'))
BEGIN
    CREATE TABLE Produtos (
        Id INT PRIMARY KEY IDENTITY(1,1),
        
        -- Informações básicas
        Nome NVARCHAR(200) NOT NULL,
        Descricao NVARCHAR(1000) NULL,
        DescricaoDetalhada NVARCHAR(MAX) NULL, -- Para descrição completa
        
        -- Códigos de identificação
        CodigoBarra NVARCHAR(50) NULL,
        CodigoInterno NVARCHAR(30) NOT NULL UNIQUE, -- Código interno obrigatório
        CodigoFornecedor NVARCHAR(50) NULL, -- Código do fornecedor
        SKU NVARCHAR(50) NULL, -- Stock Keeping Unit
        
        -- Categoria e classificação
        IdCategoria INT NULL,
        Marca NVARCHAR(100) NULL,
        Modelo NVARCHAR(100) NULL,
        
        -- Preços em Kwanza (AOA)
        PrecoCompra DECIMAL(18,2) NOT NULL DEFAULT 0.00,
        PrecoVenda DECIMAL(18,2) NOT NULL DEFAULT 0.00,
        PrecoPromocional DECIMAL(18,2) NULL,
        MargemLucro DECIMAL(5,2) NULL, -- Percentual de margem
        
        -- Controle de estoque
        EstoqueAtual INT NOT NULL DEFAULT 0,
        EstoqueMinimo INT NOT NULL DEFAULT 0,
        EstoqueMaximo INT NULL,
        EstoqueReservado INT NOT NULL DEFAULT 0, -- Estoque reservado em vendas pendentes
        
        -- Unidades de medida
        UnidadeMedida NVARCHAR(20) NOT NULL DEFAULT 'Unidade',
        UnidadeCompra NVARCHAR(20) NULL, -- Unidade de compra (pode ser diferente da venda)
        FatorConversao DECIMAL(10,4) DEFAULT 1.0000, -- Fator de conversão entre unidades
        
        -- Informações físicas
        Peso DECIMAL(10,3) NULL, -- Peso em kg
        Altura DECIMAL(10,2) NULL, -- Altura em cm
        Largura DECIMAL(10,2) NULL, -- Largura em cm
        Profundidade DECIMAL(10,2) NULL, -- Profundidade em cm
        Volume DECIMAL(10,3) NULL, -- Volume em litros
        
        -- Informações fiscais
        TaxaIVA DECIMAL(5,2) NOT NULL DEFAULT 14.00, -- Taxa de IVA (padrão Angola 14%)
        CodigoNCM NVARCHAR(20) NULL, -- Nomenclatura Comum do Mercosul
        CFOP NVARCHAR(10) NULL, -- Código Fiscal de Operações e Prestações
        
        -- Controle de qualidade e validade
        DataValidade DATE NULL,
        LoteControle NVARCHAR(50) NULL,
        TemValidade BIT NOT NULL DEFAULT 0,
        DiasValidadeMinima INT NULL, -- Dias mínimos de validade para venda
        
        -- Status e controles
        Ativo BIT NOT NULL DEFAULT 1,
        Promocao BIT NOT NULL DEFAULT 0,
        DataInicioPromocao DATE NULL,
        DataFimPromocao DATE NULL,
        Destaque BIT NOT NULL DEFAULT 0,
        PermiteVendaEstoqueNegativo BIT NOT NULL DEFAULT 0,
        
        -- Imagens e anexos
        ImagemPrincipal VARBINARY(MAX) NULL,
        CaminhoImagem NVARCHAR(500) NULL, -- Caminho para arquivo de imagem
        
        -- Fornecedor
        IdFornecedorPrincipal INT NULL,
        
        -- Auditoria completa
        DataCriacao DATETIME2 NOT NULL DEFAULT GETDATE(),
        CriadoPor INT NULL,
        DataAtualizacao DATETIME2 NOT NULL DEFAULT GETDATE(),
        AtualizadoPor INT NULL,
        DataUltimaVenda DATETIME2 NULL,
        DataUltimaCompra DATETIME2 NULL,
        
        -- Observações
        Observacoes NVARCHAR(500) NULL,
        
        -- Constraints de integridade
        CONSTRAINT FK_Produtos_IdCategoria FOREIGN KEY (IdCategoria) REFERENCES Categorias(Id),
        CONSTRAINT FK_Produtos_CriadoPor FOREIGN KEY (CriadoPor) REFERENCES Usuarios(Id),
        CONSTRAINT FK_Produtos_AtualizadoPor FOREIGN KEY (AtualizadoPor) REFERENCES Usuarios(Id),
        
        -- Constraints de validação
        CONSTRAINT CK_Produtos_PrecoCompra CHECK (PrecoCompra >= 0),
        CONSTRAINT CK_Produtos_PrecoVenda CHECK (PrecoVenda >= 0),
        CONSTRAINT CK_Produtos_PrecoPromocional CHECK (PrecoPromocional IS NULL OR PrecoPromocional >= 0),
        CONSTRAINT CK_Produtos_MargemLucro CHECK (MargemLucro IS NULL OR MargemLucro >= -100),
        CONSTRAINT CK_Produtos_EstoqueMinimo CHECK (EstoqueMinimo >= 0),
        CONSTRAINT CK_Produtos_EstoqueMaximo CHECK (EstoqueMaximo IS NULL OR EstoqueMaximo >= EstoqueMinimo),
        CONSTRAINT CK_Produtos_EstoqueReservado CHECK (EstoqueReservado >= 0),
        CONSTRAINT CK_Produtos_TaxaIVA CHECK (TaxaIVA >= 0 AND TaxaIVA <= 100),
        CONSTRAINT CK_Produtos_FatorConversao CHECK (FatorConversao > 0),
        CONSTRAINT CK_Produtos_Peso CHECK (Peso IS NULL OR Peso >= 0),
        CONSTRAINT CK_Produtos_Dimensoes CHECK (
            (Altura IS NULL OR Altura >= 0) AND 
            (Largura IS NULL OR Largura >= 0) AND 
            (Profundidade IS NULL OR Profundidade >= 0)
        ),
        CONSTRAINT CK_Produtos_Volume CHECK (Volume IS NULL OR Volume >= 0),
        CONSTRAINT CK_Produtos_DiasValidadeMinima CHECK (DiasValidadeMinima IS NULL OR DiasValidadeMinima >= 0),
        CONSTRAINT CK_Produtos_Promocao CHECK (
            Promocao = 0 OR 
            (DataInicioPromocao IS NOT NULL AND DataFimPromocao IS NOT NULL AND DataFimPromocao >= DataInicioPromocao)
        )
    );
    
    -- Índices otimizados para consultas frequentes
    CREATE INDEX IX_Produtos_Nome ON Produtos(Nome) WHERE Ativo = 1;
    CREATE INDEX IX_Produtos_CodigoBarra ON Produtos(CodigoBarra) WHERE CodigoBarra IS NOT NULL;
    CREATE INDEX IX_Produtos_CodigoInterno ON Produtos(CodigoInterno);
    CREATE INDEX IX_Produtos_IdCategoria ON Produtos(IdCategoria, Ativo);
    CREATE INDEX IX_Produtos_Marca ON Produtos(Marca) WHERE Marca IS NOT NULL;
    CREATE INDEX IX_Produtos_PrecoVenda ON Produtos(PrecoVenda, Ativo);
    CREATE INDEX IX_Produtos_EstoqueAtual ON Produtos(EstoqueAtual, EstoqueMinimo);
    CREATE INDEX IX_Produtos_Promocao ON Produtos(Promocao, DataInicioPromocao, DataFimPromocao) WHERE Promocao = 1;
    CREATE INDEX IX_Produtos_Destaque ON Produtos(Destaque, Ativo) WHERE Destaque = 1;
    CREATE INDEX IX_Produtos_DataValidade ON Produtos(DataValidade) WHERE TemValidade = 1;
    CREATE INDEX IX_Produtos_DataUltimaVenda ON Produtos(DataUltimaVenda DESC);
    
    PRINT '✅ Tabela Produtos completa e robusta criada!';
END
GO

-- =====================================================
-- TABELA DE CLIENTES MELHORADA
-- Inclui informações completas para o mercado angolano
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Clientes]') AND type in (N'U'))
BEGIN
    CREATE TABLE Clientes (
        Id INT PRIMARY KEY IDENTITY(1,1),
        
        -- Informações básicas
        Nome NVARCHAR(200) NOT NULL,
        NomeCompleto NVARCHAR(300) NULL,
        TipoCliente NVARCHAR(20) NOT NULL DEFAULT 'Pessoa Física' 
            CHECK (TipoCliente IN ('Pessoa Física', 'Pessoa Jurídica', 'Empresa', 'Organização')),
        
        -- Documentos de identificação
        NumeroBI NVARCHAR(20) NULL, -- Bilhete de Identidade
        NIF NVARCHAR(20) NULL, -- Número de Identificação Fiscal
        Passaporte NVARCHAR(20) NULL,
        
        -- Informações de contato
        Telefone NVARCHAR(20) NULL,
        TelefoneAlternativo NVARCHAR(20) NULL,
        Email NVARCHAR(100) NULL,
        EmailAlternativo NVARCHAR(100) NULL,
        
        -- Endereço completo
        Endereco NVARCHAR(300) NULL,
        Bairro NVARCHAR(100) NULL,
        Cidade NVARCHAR(100) NULL DEFAULT 'Luanda',
        Provincia NVARCHAR(100) NULL DEFAULT 'Luanda',
        CodigoPostal NVARCHAR(20) NULL,
        PontoReferencia NVARCHAR(200) NULL, -- Importante em Angola
        
        -- Informações comerciais
        LimiteCredito DECIMAL(18,2) NULL DEFAULT 0.00,
        CreditoUtilizado DECIMAL(18,2) NOT NULL DEFAULT 0.00,
        DiasCredito INT NULL DEFAULT 0, -- Prazo de pagamento em dias
        TaxaDesconto DECIMAL(5,2) NULL DEFAULT 0.00, -- Desconto padrão do cliente
        
        -- Classificação do cliente
        Categoria NVARCHAR(50) NULL DEFAULT 'Normal', -- VIP, Normal, Bronze, Prata, Ouro
        Segmento NVARCHAR(50) NULL, -- Segmento de mercado
        OrigemCliente NVARCHAR(50) NULL, -- Como conheceu a empresa
        
        -- Informações estatísticas
        TotalCompras DECIMAL(18,2) NOT NULL DEFAULT 0.00,
        QuantidadeCompras INT NOT NULL DEFAULT 0,
        DataPrimeiraCompra DATETIME2 NULL,
        DataUltimaCompra DATETIME2 NULL,
        TicketMedio DECIMAL(18,2) NULL,
        
        -- Status e controles
        Ativo BIT NOT NULL DEFAULT 1,
        Bloqueado BIT NOT NULL DEFAULT 0,
        MotivoBloqueio NVARCHAR(200) NULL,
        DataBloqueio DATETIME2 NULL,
        
        -- Preferências
        IdiomaPreferido NVARCHAR(10) DEFAULT 'pt-AO',
        MoedaPreferida NVARCHAR(5) DEFAULT 'AOA',
        
        -- Informações adicionais
        DataNascimento DATE NULL,
        Genero NVARCHAR(20) NULL CHECK (Genero IN ('Masculino', 'Feminino', 'Outro', 'Não Informado')),
        EstadoCivil NVARCHAR(30) NULL,
        Profissao NVARCHAR(100) NULL,
        
        -- Auditoria
        DataCadastro DATETIME2 NOT NULL DEFAULT GETDATE(),
        CadastradoPor INT NULL,
        DataAtualizacao DATETIME2 NOT NULL DEFAULT GETDATE(),
        AtualizadoPor INT NULL,
        
        -- Observações
        Observacoes NVARCHAR(1000) NULL,
        
        -- Constraints
        CONSTRAINT FK_Clientes_CadastradoPor FOREIGN KEY (CadastradoPor) REFERENCES Usuarios(Id),
        CONSTRAINT FK_Clientes_AtualizadoPor FOREIGN KEY (AtualizadoPor) REFERENCES Usuarios(Id),
        CONSTRAINT CK_Clientes_Email CHECK (Email IS NULL OR Email LIKE '%@%.%'),
        CONSTRAINT CK_Clientes_EmailAlternativo CHECK (EmailAlternativo IS NULL OR EmailAlternativo LIKE '%@%.%'),
        CONSTRAINT CK_Clientes_LimiteCredito CHECK (LimiteCredito IS NULL OR LimiteCredito >= 0),
        CONSTRAINT CK_Clientes_CreditoUtilizado CHECK (CreditoUtilizado >= 0),
        CONSTRAINT CK_Clientes_DiasCredito CHECK (DiasCredito IS NULL OR DiasCredito >= 0),
        CONSTRAINT CK_Clientes_TaxaDesconto CHECK (TaxaDesconto IS NULL OR (TaxaDesconto >= 0 AND TaxaDesconto <= 100)),
        CONSTRAINT CK_Clientes_TotalCompras CHECK (TotalCompras >= 0),
        CONSTRAINT CK_Clientes_QuantidadeCompras CHECK (QuantidadeCompras >= 0),
        CONSTRAINT CK_Clientes_CreditoLimite CHECK (CreditoUtilizado <= ISNULL(LimiteCredito, CreditoUtilizado))
    );
    
    -- Índices otimizados
    CREATE INDEX IX_Clientes_Nome ON Clientes(Nome) WHERE Ativo = 1;
    CREATE INDEX IX_Clientes_NumeroBI ON Clientes(NumeroBI) WHERE NumeroBI IS NOT NULL;
    CREATE INDEX IX_Clientes_NIF ON Clientes(NIF) WHERE NIF IS NOT NULL;
    CREATE INDEX IX_Clientes_Telefone ON Clientes(Telefone) WHERE Telefone IS NOT NULL;
    CREATE INDEX IX_Clientes_Email ON Clientes(Email) WHERE Email IS NOT NULL;
    CREATE INDEX IX_Clientes_Cidade_Provincia ON Clientes(Cidade, Provincia);
    CREATE INDEX IX_Clientes_Categoria ON Clientes(Categoria, Ativo);
    CREATE INDEX IX_Clientes_DataUltimaCompra ON Clientes(DataUltimaCompra DESC);
    CREATE INDEX IX_Clientes_TotalCompras ON Clientes(TotalCompras DESC);
    CREATE INDEX IX_Clientes_LimiteCredito ON Clientes(LimiteCredito, CreditoUtilizado);
    
    PRINT '✅ Tabela Clientes melhorada para Angola criada!';
END
GO

-- =====================================================
-- TABELA DE FORNECEDORES COMPLETA
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Fornecedores]') AND type in (N'U'))
BEGIN
    CREATE TABLE Fornecedores (
        Id INT PRIMARY KEY IDENTITY(1,1),
        
        -- Informações básicas
        Nome NVARCHAR(200) NOT NULL,
        NomeFantasia NVARCHAR(200) NULL,
        RazaoSocial NVARCHAR(300) NULL,
        
        -- Documentos
        NIF NVARCHAR(20) NULL, -- Número de Identificação Fiscal
        NumeroRegistroComercial NVARCHAR(30) NULL,
        
        -- Contato
        Telefone NVARCHAR(20) NULL,
        TelefoneAlternativo NVARCHAR(20) NULL,
        Email NVARCHAR(100) NULL,
        Website NVARCHAR(200) NULL,
        
        -- Endereço
        Endereco NVARCHAR(300) NULL,
        Bairro NVARCHAR(100) NULL,
        Cidade NVARCHAR(100) NULL DEFAULT 'Luanda',
        Provincia NVARCHAR(100) NULL DEFAULT 'Luanda',
        CodigoPostal NVARCHAR(20) NULL,
        Pais NVARCHAR(100) NULL DEFAULT 'Angola',
        
        -- Informações comerciais
        CondicoesPagamento NVARCHAR(100) NULL,
        PrazoPagamento INT NULL DEFAULT 30, -- Dias
        LimiteCredito DECIMAL(18,2) NULL,
        
        -- Pessoa de contato
        NomeContato NVARCHAR(150) NULL,
        CargoContato NVARCHAR(100) NULL,
        TelefoneContato NVARCHAR(20) NULL,
        EmailContato NVARCHAR(100) NULL,
        
        -- Status
        Ativo BIT NOT NULL DEFAULT 1,
        Aprovado BIT NOT NULL DEFAULT 0,
        DataAprovacao DATETIME2 NULL,
        AprovadoPor INT NULL,
        
        -- Auditoria
        DataCadastro DATETIME2 NOT NULL DEFAULT GETDATE(),
        CadastradoPor INT NULL,
        DataAtualizacao DATETIME2 NOT NULL DEFAULT GETDATE(),
        AtualizadoPor INT NULL,
        
        -- Observações
        Observacoes NVARCHAR(1000) NULL,
        
        -- Constraints
        CONSTRAINT FK_Fornecedores_CadastradoPor FOREIGN KEY (CadastradoPor) REFERENCES Usuarios(Id),
        CONSTRAINT FK_Fornecedores_AtualizadoPor FOREIGN KEY (AtualizadoPor) REFERENCES Usuarios(Id),
        CONSTRAINT FK_Fornecedores_AprovadoPor FOREIGN KEY (AprovadoPor) REFERENCES Usuarios(Id),
        CONSTRAINT CK_Fornecedores_Email CHECK (Email IS NULL OR Email LIKE '%@%.%'),
        CONSTRAINT CK_Fornecedores_EmailContato CHECK (EmailContato IS NULL OR EmailContato LIKE '%@%.%'),
        CONSTRAINT CK_Fornecedores_PrazoPagamento CHECK (PrazoPagamento IS NULL OR PrazoPagamento >= 0),
        CONSTRAINT CK_Fornecedores_LimiteCredito CHECK (LimiteCredito IS NULL OR LimiteCredito >= 0)
    );
    
    -- Índices
    CREATE INDEX IX_Fornecedores_Nome ON Fornecedores(Nome) WHERE Ativo = 1;
    CREATE INDEX IX_Fornecedores_NIF ON Fornecedores(NIF) WHERE NIF IS NOT NULL;
    CREATE INDEX IX_Fornecedores_Cidade_Provincia ON Fornecedores(Cidade, Provincia);
    CREATE INDEX IX_Fornecedores_Aprovado ON Fornecedores(Aprovado, Ativo);
    
    PRINT '✅ Tabela Fornecedores completa criada!';
END
GO

-- =====================================================
-- TABELA DE VENDAS ROBUSTA
-- Inclui informações fiscais e controle completo
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Vendas]') AND type in (N'U'))
BEGIN
    CREATE TABLE Vendas (
        Id INT PRIMARY KEY IDENTITY(1,1),
        
        -- Numeração e identificação
        NumeroVenda NVARCHAR(20) NOT NULL UNIQUE, -- Número sequencial da venda
        NumeroFatura NVARCHAR(30) NULL, -- Número da fatura fiscal
        SerieDocumento NVARCHAR(10) NULL DEFAULT 'A', -- Série do documento
        
        -- Relacionamentos
        IdCliente INT NULL,
        IdUsuario INT NOT NULL, -- Vendedor
        
        -- Datas importantes
        DataVenda DATETIME2 NOT NULL DEFAULT GETDATE(),
        DataVencimento DATE NULL, -- Para vendas a prazo
        DataEntrega DATE NULL,
        
        -- Valores em Kwanza (AOA)
        SubTotal DECIMAL(18,2) NOT NULL DEFAULT 0.00,
        TotalDesconto DECIMAL(18,2) NOT NULL DEFAULT 0.00,
        TotalIVA DECIMAL(18,2) NOT NULL DEFAULT 0.00,
        Total DECIMAL(18,2) NOT NULL DEFAULT 0.00,
        TotalPago DECIMAL(18,2) NOT NULL DEFAULT 0.00,
        TotalPendente DECIMAL(18,2) NOT NULL DEFAULT 0.00,
        
        -- Informações de pagamento
        TipoPagamento NVARCHAR(50) NOT NULL DEFAULT 'Dinheiro',
        StatusPagamento NVARCHAR(30) NOT NULL DEFAULT 'Pendente' 
            CHECK (StatusPagamento IN ('Pendente', 'Pago', 'Parcial', 'Cancelado', 'Estornado')),
        
        -- Status da venda
        StatusVenda NVARCHAR(30) NOT NULL DEFAULT 'Finalizada' 
            CHECK (StatusVenda IN ('Orcamento', 'Pendente', 'Finalizada', 'Entregue', 'Cancelada', 'Devolvida')),
        
        -- Informações fiscais
        TipoDocumento NVARCHAR(30) NOT NULL DEFAULT 'Fatura' 
            CHECK (TipoDocumento IN ('Fatura', 'Recibo', 'Fatura-Recibo', 'Nota de Débito', 'Nota de Crédito')),
        ChaveAcesso NVARCHAR(100) NULL, -- Para documentos eletrônicos
        
        -- Descontos e promoções
        PercentualDesconto DECIMAL(5,2) NOT NULL DEFAULT 0.00,
        MotivoDesconto NVARCHAR(200) NULL,
        AutorizacaoDesconto INT NULL, -- Usuário que autorizou desconto
        
        -- Informações de entrega
        EnderecoEntrega NVARCHAR(500) NULL,
        TipoEntrega NVARCHAR(50) NULL DEFAULT 'Balcão' 
            CHECK (TipoEntrega IN ('Balcão', 'Entrega', 'Retirada', 'Correios')),
        ValorFrete DECIMAL(18,2) NOT NULL DEFAULT 0.00,
        
        -- Observações e notas
        Observacoes NVARCHAR(1000) NULL,
        ObservacoesInternas NVARCHAR(500) NULL, -- Não aparecem na fatura
        
        -- Auditoria
        DataCriacao DATETIME2 NOT NULL DEFAULT GETDATE(),
        DataAtualizacao DATETIME2 NOT NULL DEFAULT GETDATE(),
        AtualizadoPor INT NULL,
        
        -- Controle de impressão
        Impressa BIT NOT NULL DEFAULT 0,
        DataImpressao DATETIME2 NULL,
        QuantidadeImpressoes INT NOT NULL DEFAULT 0,
        
        -- Constraints
        CONSTRAINT FK_Vendas_IdCliente FOREIGN KEY (IdCliente) REFERENCES Clientes(Id),
        CONSTRAINT FK_Vendas_IdUsuario FOREIGN KEY (IdUsuario) REFERENCES Usuarios(Id),
        CONSTRAINT FK_Vendas_AutorizacaoDesconto FOREIGN KEY (AutorizacaoDesconto) REFERENCES Usuarios(Id),
        CONSTRAINT FK_Vendas_AtualizadoPor FOREIGN KEY (AtualizadoPor) REFERENCES Usuarios(Id),
        
        -- Validações
        CONSTRAINT CK_Vendas_SubTotal CHECK (SubTotal >= 0),
        CONSTRAINT CK_Vendas_TotalDesconto CHECK (TotalDesconto >= 0),
        CONSTRAINT CK_Vendas_TotalIVA CHECK (TotalIVA >= 0),
        CONSTRAINT CK_Vendas_Total CHECK (Total >= 0),
        CONSTRAINT CK_Vendas_TotalPago CHECK (TotalPago >= 0),
        CONSTRAINT CK_Vendas_TotalPendente CHECK (TotalPendente >= 0),
        CONSTRAINT CK_Vendas_PercentualDesconto CHECK (PercentualDesconto >= 0 AND PercentualDesconto <= 100),
        CONSTRAINT CK_Vendas_ValorFrete CHECK (ValorFrete >= 0),
        CONSTRAINT CK_Vendas_QuantidadeImpressoes CHECK (QuantidadeImpressoes >= 0),
        CONSTRAINT CK_Vendas_DataVencimento CHECK (DataVencimento IS NULL OR DataVencimento >= CAST(DataVenda AS DATE)),
        CONSTRAINT CK_Vendas_TotaisPagamento CHECK (TotalPago + TotalPendente = Total)
    );
    
    -- Índices otimizados
    CREATE INDEX IX_Vendas_NumeroVenda ON Vendas(NumeroVenda);
    CREATE INDEX IX_Vendas_NumeroFatura ON Vendas(NumeroFatura) WHERE NumeroFatura IS NOT NULL;
    CREATE INDEX IX_Vendas_DataVenda ON Vendas(DataVenda DESC);
    CREATE INDEX IX_Vendas_IdCliente ON Vendas(IdCliente, DataVenda DESC);
    CREATE INDEX IX_Vendas_IdUsuario ON Vendas(IdUsuario, DataVenda DESC);
    CREATE INDEX IX_Vendas_StatusVenda ON Vendas(StatusVenda, DataVenda DESC);
    CREATE INDEX IX_Vendas_StatusPagamento ON Vendas(StatusPagamento, DataVenda DESC);
    CREATE INDEX IX_Vendas_TipoDocumento ON Vendas(TipoDocumento, DataVenda DESC);
    CREATE INDEX IX_Vendas_Total ON Vendas(Total DESC, DataVenda DESC);
    CREATE INDEX IX_Vendas_DataVencimento ON Vendas(DataVencimento) WHERE DataVencimento IS NOT NULL;
    
    PRINT '✅ Tabela Vendas robusta criada!';
END
GO

-- =====================================================
-- TABELA DE ITENS DE VENDA DETALHADA
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ItensVenda]') AND type in (N'U'))
BEGIN
    CREATE TABLE ItensVenda (
        Id INT PRIMARY KEY IDENTITY(1,1),
        
        -- Relacionamentos
        IdVenda INT NOT NULL,
        IdProduto INT NOT NULL,
        
        -- Informações do produto no momento da venda
        CodigoProduto NVARCHAR(30) NOT NULL, -- Código do produto na época da venda
        NomeProduto NVARCHAR(200) NOT NULL, -- Nome do produto na época da venda
        DescricaoProduto NVARCHAR(500) NULL,
        
        -- Quantidades
        Quantidade DECIMAL(10,3) NOT NULL,
        UnidadeMedida NVARCHAR(20) NOT NULL DEFAULT 'Unidade',
        
        -- Preços unitários em Kwanza (AOA)
        PrecoUnitario DECIMAL(18,2) NOT NULL,
        PrecoOriginal DECIMAL(18,2) NOT NULL, -- Preço original antes de descontos
        
        -- Descontos no item
        PercentualDesconto DECIMAL(5,2) NOT NULL DEFAULT 0.00,
        ValorDesconto DECIMAL(18,2) NOT NULL DEFAULT 0.00,
        
        -- Valores de IVA
        TaxaIVA DECIMAL(5,2) NOT NULL DEFAULT 14.00,
        ValorIVA DECIMAL(18,2) NOT NULL DEFAULT 0.00,
        
        -- Totais
        SubTotalItem DECIMAL(18,2) NOT NULL, -- Quantidade * PrecoUnitario
        TotalItem DECIMAL(18,2) NOT NULL, -- SubTotal - Desconto + IVA
        
        -- Informações adicionais
        Lote NVARCHAR(50) NULL,
        DataValidade DATE NULL,
        Observacoes NVARCHAR(300) NULL,
        
        -- Auditoria
        DataCriacao DATETIME2 NOT NULL DEFAULT GETDATE(),
        
        -- Constraints
        CONSTRAINT FK_ItensVenda_IdVenda FOREIGN KEY (IdVenda) REFERENCES Vendas(Id) ON DELETE CASCADE,
        CONSTRAINT FK_ItensVenda_IdProduto FOREIGN KEY (IdProduto) REFERENCES Produtos(Id),
        
        -- Validações
        CONSTRAINT CK_ItensVenda_Quantidade CHECK (Quantidade > 0),
        CONSTRAINT CK_ItensVenda_PrecoUnitario CHECK (PrecoUnitario >= 0),
        CONSTRAINT CK_ItensVenda_PrecoOriginal CHECK (PrecoOriginal >= 0),
        CONSTRAINT CK_ItensVenda_PercentualDesconto CHECK (PercentualDesconto >= 0 AND PercentualDesconto <= 100),
        CONSTRAINT CK_ItensVenda_ValorDesconto CHECK (ValorDesconto >= 0),
        CONSTRAINT CK_ItensVenda_TaxaIVA CHECK (TaxaIVA >= 0 AND TaxaIVA <= 100),
        CONSTRAINT CK_ItensVenda_ValorIVA CHECK (ValorIVA >= 0),
        CONSTRAINT CK_ItensVenda_SubTotalItem CHECK (SubTotalItem >= 0),
        CONSTRAINT CK_ItensVenda_TotalItem CHECK (TotalItem >= 0)
    );
    
    -- Índices
    CREATE INDEX IX_ItensVenda_IdVenda ON ItensVenda(IdVenda);
    CREATE INDEX IX_ItensVenda_IdProduto ON ItensVenda(IdProduto);
    CREATE INDEX IX_ItensVenda_CodigoProduto ON ItensVenda(CodigoProduto);
    CREATE INDEX IX_ItensVenda_DataCriacao ON ItensVenda(DataCriacao DESC);
    
    PRINT '✅ Tabela ItensVenda detalhada criada!';
END
GO

-- =====================================================
-- TABELA DE FORMAS DE PAGAMENTO
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FormasPagamento]') AND type in (N'U'))
BEGIN
    CREATE TABLE FormasPagamento (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Descricao NVARCHAR(100) NOT NULL UNIQUE,
        Codigo NVARCHAR(20) UNIQUE NULL,
        TipoPagamento NVARCHAR(30) NOT NULL DEFAULT 'À Vista' 
            CHECK (TipoPagamento IN ('À Vista', 'A Prazo', 'Parcelado')),
        PermiteParcelamento BIT NOT NULL DEFAULT 0,
        MaximoParcelas INT NULL DEFAULT 1,
        TaxaJuros DECIMAL(5,2) NULL DEFAULT 0.00,
        DiasCarencia INT NULL DEFAULT 0,
        Ativo BIT NOT NULL DEFAULT 1,
        Ordem INT NOT NULL DEFAULT 0,
        
        -- Auditoria
        DataCriacao DATETIME2 NOT NULL DEFAULT GETDATE(),
        CriadoPor INT NULL,
        
        CONSTRAINT FK_FormasPagamento_CriadoPor FOREIGN KEY (CriadoPor) REFERENCES Usuarios(Id),
        CONSTRAINT CK_FormasPagamento_MaximoParcelas CHECK (MaximoParcelas IS NULL OR MaximoParcelas >= 1),
        CONSTRAINT CK_FormasPagamento_TaxaJuros CHECK (TaxaJuros IS NULL OR TaxaJuros >= 0),
        CONSTRAINT CK_FormasPagamento_DiasCarencia CHECK (DiasCarencia IS NULL OR DiasCarencia >= 0)
    );
    
    CREATE INDEX IX_FormasPagamento_Ativo ON FormasPagamento(Ativo, Ordem);
    
    PRINT '✅ Tabela FormasPagamento criada!';
END
GO

-- =====================================================
-- TABELA DE MOVIMENTAÇÃO DE ESTOQUE COMPLETA
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MovimentacaoEstoque]') AND type in (N'U'))
BEGIN
    CREATE TABLE MovimentacaoEstoque (
        Id INT PRIMARY KEY IDENTITY(1,1),
        
        -- Relacionamentos
        IdProduto INT NOT NULL,
        IdUsuario INT NOT NULL,
        IdVenda INT NULL, -- Se a movimentação foi por venda
        IdFornecedor INT NULL, -- Se a movimentação foi por compra
        
        -- Tipo de movimentação
        TipoMovimentacao NVARCHAR(20) NOT NULL 
            CHECK (TipoMovimentacao IN ('Entrada', 'Saida', 'Ajuste', 'Transferencia', 'Perda', 'Devolucao')),
        
        -- Quantidades
        QuantidadeAnterior INT NOT NULL,
        QuantidadeMovimentada INT NOT NULL,
        QuantidadeAtual INT NOT NULL,
        
        -- Valores unitários
        CustoUnitario DECIMAL(18,2) NULL,
        ValorTotal DECIMAL(18,2) NULL,
        
        -- Informações adicionais
        Motivo NVARCHAR(200) NOT NULL,
        NumeroDocumento NVARCHAR(50) NULL, -- Número da nota fiscal, recibo, etc.
        Lote NVARCHAR(50) NULL,
        DataValidade DATE NULL,
        
        -- Localização
        LocalOrigem NVARCHAR(100) NULL,
        LocalDestino NVARCHAR(100) NULL,
        
        -- Auditoria
        DataMovimentacao DATETIME2 NOT NULL DEFAULT GETDATE(),
        Observacoes NVARCHAR(500) NULL,
        
        -- Constraints
        CONSTRAINT FK_MovimentacaoEstoque_IdProduto FOREIGN KEY (IdProduto) REFERENCES Produtos(Id),
        CONSTRAINT FK_MovimentacaoEstoque_IdUsuario FOREIGN KEY (IdUsuario) REFERENCES Usuarios(Id),
        CONSTRAINT FK_MovimentacaoEstoque_IdVenda FOREIGN KEY (IdVenda) REFERENCES Vendas(Id),
        CONSTRAINT FK_MovimentacaoEstoque_IdFornecedor FOREIGN KEY (IdFornecedor) REFERENCES Fornecedores(Id),
        
        -- Validações
        CONSTRAINT CK_MovimentacaoEstoque_QuantidadeAnterior CHECK (QuantidadeAnterior >= 0),
        CONSTRAINT CK_MovimentacaoEstoque_QuantidadeMovimentada CHECK (QuantidadeMovimentada != 0),
        CONSTRAINT CK_MovimentacaoEstoque_QuantidadeAtual CHECK (QuantidadeAtual >= 0),
        CONSTRAINT CK_MovimentacaoEstoque_CustoUnitario CHECK (CustoUnitario IS NULL OR CustoUnitario >= 0),
        CONSTRAINT CK_MovimentacaoEstoque_ValorTotal CHECK (ValorTotal IS NULL OR ValorTotal >= 0)
    );
    
    -- Índices
    CREATE INDEX IX_MovimentacaoEstoque_IdProduto ON MovimentacaoEstoque(IdProduto, DataMovimentacao DESC);
    CREATE INDEX IX_MovimentacaoEstoque_DataMovimentacao ON MovimentacaoEstoque(DataMovimentacao DESC);
    CREATE INDEX IX_MovimentacaoEstoque_TipoMovimentacao ON MovimentacaoEstoque(TipoMovimentacao, DataMovimentacao DESC);
    CREATE INDEX IX_MovimentacaoEstoque_IdVenda ON MovimentacaoEstoque(IdVenda) WHERE IdVenda IS NOT NULL;
    CREATE INDEX IX_MovimentacaoEstoque_IdFornecedor ON MovimentacaoEstoque(IdFornecedor) WHERE IdFornecedor IS NOT NULL;
    
    PRINT '✅ Tabela MovimentacaoEstoque completa criada!';
END
GO

-- =====================================================
-- TABELA DE CONFIGURAÇÕES DA EMPRESA
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ConfiguracoesEmpresa]') AND type in (N'U'))
BEGIN
    CREATE TABLE ConfiguracoesEmpresa (
        Id INT PRIMARY KEY IDENTITY(1,1),
        
        -- Informações básicas da empresa
        NomeEmpresa NVARCHAR(200) NOT NULL,
        NomeFantasia NVARCHAR(200) NULL,
        RazaoSocial NVARCHAR(300) NULL,
        
        -- Documentos
        NIF NVARCHAR(20) NULL,
        NumeroRegistroComercial NVARCHAR(30) NULL,
        NumeroAlvara NVARCHAR(30) NULL,
        
        -- Endereço completo
        Endereco NVARCHAR(300) NULL,
        Bairro NVARCHAR(100) NULL,
        Cidade NVARCHAR(100) NULL DEFAULT 'Luanda',
        Provincia NVARCHAR(100) NULL DEFAULT 'Luanda',
        CodigoPostal NVARCHAR(20) NULL,
        Pais NVARCHAR(100) NULL DEFAULT 'Angola',
        
        -- Contatos
        Telefone NVARCHAR(20) NULL,
        TelefoneAlternativo NVARCHAR(20) NULL,
        Fax NVARCHAR(20) NULL,
        Email NVARCHAR(100) NULL,
        Website NVARCHAR(200) NULL,
        
        -- Informações fiscais
        RegimeTributario NVARCHAR(50) NULL DEFAULT 'Regime Geral',
        InscricaoEstadual NVARCHAR(30) NULL,
        InscricaoMunicipal NVARCHAR(30) NULL,
        
        -- Logotipo e identidade visual
        Logotipo VARBINARY(MAX) NULL,
        CaminhoLogotipo NVARCHAR(500) NULL,
        CorPrimaria NVARCHAR(7) NULL DEFAULT '#0066CC',
        CorSecundaria NVARCHAR(7) NULL DEFAULT '#FFFFFF',
        
        -- Configurações de documentos
        SerieDocumentoPadrao NVARCHAR(10) NULL DEFAULT 'A',
        ProximoNumeroFatura INT NOT NULL DEFAULT 1,
        ProximoNumeroRecibo INT NOT NULL DEFAULT 1,
        
        -- Mensagens padrão
        MensagemRodapeFatura NVARCHAR(500) NULL DEFAULT 'Obrigado pela preferência!',
        MensagemPromocional NVARCHAR(300) NULL,
        
        -- Auditoria
        DataCriacao DATETIME2 NOT NULL DEFAULT GETDATE(),
        DataAtualizacao DATETIME2 NOT NULL DEFAULT GETDATE(),
        AtualizadoPor INT NULL,
        
        CONSTRAINT FK_ConfiguracoesEmpresa_AtualizadoPor FOREIGN KEY (AtualizadoPor) REFERENCES Usuarios(Id),
        CONSTRAINT CK_ConfiguracoesEmpresa_Email CHECK (Email IS NULL OR Email LIKE '%@%.%'),
        CONSTRAINT CK_ConfiguracoesEmpresa_ProximoNumeroFatura CHECK (ProximoNumeroFatura > 0),
        CONSTRAINT CK_ConfiguracoesEmpresa_ProximoNumeroRecibo CHECK (ProximoNumeroRecibo > 0)
    );
    
    PRINT '✅ Tabela ConfiguracoesEmpresa criada!';
END
GO

-- =====================================================
-- INSERIR DADOS INICIAIS PARA ANGOLA
-- =====================================================

-- Inserir usuário administrador padrão
IF NOT EXISTS (SELECT * FROM Usuarios WHERE Usuario = 'admin')
BEGIN
    INSERT INTO Usuarios (Nome, Usuario, Senha, NivelAcesso, Ativo, Cidade, Provincia, DataCriacao)
    VALUES ('Administrador do Sistema', 'admin', 'admin123', 'Administrador', 1, 'Luanda', 'Luanda', GETDATE());
    PRINT '✅ Usuário administrador criado: admin / admin123';
END

-- Inserir usuários de teste
IF NOT EXISTS (SELECT * FROM Usuarios WHERE Usuario = 'vendedor')
BEGIN
    INSERT INTO Usuarios (Nome, Usuario, Senha, NivelAcesso, Ativo, Cidade, Provincia, DataCriacao)
    VALUES ('Vendedor Teste', 'vendedor', 'venda123', 'Vendedor', 1, 'Luanda', 'Luanda', GETDATE());
    PRINT '✅ Usuário vendedor criado: vendedor / venda123';
END

IF NOT EXISTS (SELECT * FROM Usuarios WHERE Usuario = 'gerente')
BEGIN
    INSERT INTO Usuarios (Nome, Usuario, Senha, NivelAcesso, Ativo, Cidade, Provincia, DataCriacao)
    VALUES ('Gerente Teste', 'gerente', 'gerente123', 'Gerente', 1, 'Luanda', 'Luanda', GETDATE());
    PRINT '✅ Usuário gerente criado: gerente / gerente123';
END

-- Inserir formas de pagamento típicas de Angola
IF NOT EXISTS (SELECT * FROM FormasPagamento WHERE Descricao = 'Dinheiro')
BEGIN
    INSERT INTO FormasPagamento (Descricao, Codigo, TipoPagamento, Ordem) VALUES
    ('Dinheiro', 'CASH', 'À Vista', 1),
    ('Multicaixa', 'MULTI', 'À Vista', 2),
    ('Transferência Bancária', 'TRANS', 'À Vista', 3),
    ('Cartão de Crédito', 'CC', 'À Vista', 4),
    ('Cartão de Débito', 'CD', 'À Vista', 5),
    ('Cheque', 'CHQ', 'A Prazo', 6),
    ('Crediário 30 dias', 'CRED30', 'A Prazo', 7),
    ('Crediário 60 dias', 'CRED60', 'A Prazo', 8);
    PRINT '✅ Formas de pagamento para Angola inseridas!';
END

-- Inserir categorias padrão
IF NOT EXISTS (SELECT * FROM Categorias WHERE Nome = 'Geral')
BEGIN
    INSERT INTO Categorias (Nome, Descricao, CodigoCategoria, Nivel, Ativo, Ordem, TaxaIVAPadrao) VALUES
    ('Alimentação', 'Produtos alimentícios em geral', 'ALIM', 1, 1, 1, 14.00),
    ('Bebidas', 'Bebidas em geral', 'BEB', 1, 1, 2, 14.00),
    ('Higiene e Limpeza', 'Produtos de higiene pessoal e limpeza', 'HIG', 1, 1, 3, 14.00),
    ('Eletrônicos', 'Produtos eletrônicos e eletrodomésticos', 'ELET', 1, 1, 4, 14.00),
    ('Vestuário', 'Roupas e acessórios', 'VEST', 1, 1, 5, 14.00),
    ('Farmácia', 'Medicamentos e produtos farmacêuticos', 'FARM', 1, 1, 6, 0.00),
    ('Geral', 'Categoria geral para produtos diversos', 'GERAL', 1, 1, 99, 14.00);
    PRINT '✅ Categorias padrão inseridas!';
END

-- Inserir configurações da empresa padrão
IF NOT EXISTS (SELECT * FROM ConfiguracoesEmpresa)
BEGIN
    INSERT INTO ConfiguracoesEmpresa (
        NomeEmpresa, Cidade, Provincia, Pais, 
        ProximoNumeroFatura, ProximoNumeroRecibo,
        MensagemRodapeFatura, DataCriacao
    ) VALUES (
        'Minha Empresa Lda', 'Luanda', 'Luanda', 'Angola',
        1, 1,
        'Obrigado pela preferência! Volte sempre!',
        GETDATE()
    );
    PRINT '✅ Configurações da empresa inseridas!';
END

-- =====================================================
-- TRIGGERS PARA AUDITORIA E CONTROLE
-- =====================================================

-- Trigger para atualizar estoque automaticamente nas vendas
IF NOT EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_ItensVenda_AtualizarEstoque')
BEGIN
    EXEC('
    CREATE TRIGGER TR_ItensVenda_AtualizarEstoque
    ON ItensVenda
    AFTER INSERT
    AS
    BEGIN
        SET NOCOUNT ON;
        
        -- Atualizar estoque dos produtos vendidos
        UPDATE p
        SET EstoqueAtual = p.EstoqueAtual - i.Quantidade,
            DataUltimaVenda = GETDATE()
        FROM Produtos p
        INNER JOIN inserted i ON p.Id = i.IdProduto;
        
        -- Registrar movimentação de estoque
        INSERT INTO MovimentacaoEstoque (
            IdProduto, IdUsuario, IdVenda, TipoMovimentacao,
            QuantidadeAnterior, QuantidadeMovimentada, QuantidadeAtual,
            Motivo, DataMovimentacao
        )
        SELECT 
            i.IdProduto,
            v.IdUsuario,
            i.IdVenda,
            ''Saida'',
            p.EstoqueAtual + i.Quantidade,
            -i.Quantidade,
            p.EstoqueAtual,
            ''Venda - '' + v.NumeroVenda,
            GETDATE()
        FROM inserted i
        INNER JOIN Vendas v ON i.IdVenda = v.Id
        INNER JOIN Produtos p ON i.IdProduto = p.Id;
    END
    ');
    PRINT '✅ Trigger de atualização de estoque criado!';
END
GO

-- Trigger para gerar número sequencial de vendas
IF NOT EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_Vendas_GerarNumero')
BEGIN
    EXEC('
    CREATE TRIGGER TR_Vendas_GerarNumero
    ON Vendas
    AFTER INSERT
    AS
    BEGIN
        SET NOCOUNT ON;
        
        UPDATE v
        SET NumeroVenda = ''VD'' + RIGHT(''000000'' + CAST(v.Id AS VARCHAR), 6)
        FROM Vendas v
        INNER JOIN inserted i ON v.Id = i.Id
        WHERE v.NumeroVenda IS NULL OR v.NumeroVenda = '''';
    END
    ');
    PRINT '✅ Trigger de numeração de vendas criado!';
END
GO

-- =====================================================
-- VIEWS PARA RELATÓRIOS E CONSULTAS
-- =====================================================

-- View para produtos com informações completas
IF NOT EXISTS (SELECT * FROM sys.views WHERE name = 'VW_ProdutosCompletos')
BEGIN
    EXEC('
    CREATE VIEW VW_ProdutosCompletos AS
    SELECT 
        p.Id,
        p.Nome,
        p.Descricao,
        p.CodigoBarra,
        p.CodigoInterno,
        p.SKU,
        c.Nome AS NomeCategoria,
        c.CodigoCategoria,
        p.Marca,
        p.Modelo,
        p.PrecoCompra,
        p.PrecoVenda,
        p.PrecoPromocional,
        p.MargemLucro,
        p.EstoqueAtual,
        p.EstoqueMinimo,
        p.EstoqueMaximo,
        p.EstoqueReservado,
        (p.EstoqueAtual - p.EstoqueReservado) AS EstoqueDisponivel,
        CASE 
            WHEN p.EstoqueAtual <= p.EstoqueMinimo THEN 1 
            ELSE 0 
        END AS EstoqueBaixo,
        p.UnidadeMedida,
        p.TaxaIVA,
        p.Ativo,
        p.Promocao,
        p.DataInicioPromocao,
        p.DataFimPromocao,
        p.Destaque,
        p.DataCriacao,
        p.DataUltimaVenda,
        p.DataUltimaCompra,
        CASE 
            WHEN p.Promocao = 1 AND GETDATE() BETWEEN p.DataInicioPromocao AND p.DataFimPromocao 
            THEN p.PrecoPromocional 
            ELSE p.PrecoVenda 
        END AS PrecoVendaAtual
    FROM Produtos p
    LEFT JOIN Categorias c ON p.IdCategoria = c.Id
    ');
    PRINT '✅ View VW_ProdutosCompletos criada!';
END
GO

-- View para vendas com informações completas
IF NOT EXISTS (SELECT * FROM sys.views WHERE name = 'VW_VendasCompletas')
BEGIN
    EXEC('
    CREATE VIEW VW_VendasCompletas AS
    SELECT 
        v.Id,
        v.NumeroVenda,
        v.NumeroFatura,
        v.DataVenda,
        v.DataVencimento,
        c.Nome AS NomeCliente,
        c.Telefone AS TelefoneCliente,
        c.Email AS EmailCliente,
        u.Nome AS NomeVendedor,
        v.SubTotal,
        v.TotalDesconto,
        v.TotalIVA,
        v.Total,
        v.TotalPago,
        v.TotalPendente,
        v.TipoPagamento,
        v.StatusPagamento,
        v.StatusVenda,
        v.TipoDocumento,
        v.PercentualDesconto,
        v.ValorFrete,
        v.TipoEntrega,
        v.Observacoes,
        v.Impressa,
        v.QuantidadeImpressoes,
        (SELECT COUNT(*) FROM ItensVenda iv WHERE iv.IdVenda = v.Id) AS QuantidadeItens,
        (SELECT SUM(iv.Quantidade) FROM ItensVenda iv WHERE iv.IdVenda = v.Id) AS QuantidadeTotalItens
    FROM Vendas v
    LEFT JOIN Clientes c ON v.IdCliente = c.Id
    INNER JOIN Usuarios u ON v.IdUsuario = u.Id
    ');
    PRINT '✅ View VW_VendasCompletas criada!';
END
GO

-- =====================================================
-- PROCEDURES PARA OPERAÇÕES COMUNS
-- =====================================================

-- Procedure para buscar produtos
IF NOT EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_BuscarProdutos')
BEGIN
    EXEC('
    CREATE PROCEDURE SP_BuscarProdutos
        @Termo NVARCHAR(200) = NULL,
        @IdCategoria INT = NULL,
        @ApenasAtivos BIT = 1,
        @ApenasComEstoque BIT = 0
    AS
    BEGIN
        SET NOCOUNT ON;
        
        SELECT *
        FROM VW_ProdutosCompletos
        WHERE (@Termo IS NULL OR 
               Nome LIKE ''%'' + @Termo + ''%'' OR 
               CodigoBarra LIKE ''%'' + @Termo + ''%'' OR 
               CodigoInterno LIKE ''%'' + @Termo + ''%'' OR
               SKU LIKE ''%'' + @Termo + ''%'')
          AND (@IdCategoria IS NULL OR IdCategoria = @IdCategoria)
          AND (@ApenasAtivos = 0 OR Ativo = 1)
          AND (@ApenasComEstoque = 0 OR EstoqueDisponivel > 0)
        ORDER BY Nome;
    END
    ');
    PRINT '✅ Procedure SP_BuscarProdutos criada!';
END
GO

-- =====================================================
-- FINALIZAÇÃO
-- =====================================================

PRINT '';
PRINT '🎉 ========================================';
PRINT '🎉 BANCO DE DADOS CRIADO COM SUCESSO!';
PRINT '🎉 ========================================';
PRINT '';
PRINT '📊 ESTATÍSTICAS:';
PRINT '   • Tabelas criadas: 12';
PRINT '   • Views criadas: 2';
PRINT '   • Procedures criadas: 1';
PRINT '   • Triggers criados: 2';
PRINT '   • Índices criados: 50+';
PRINT '';
PRINT '👥 USUÁRIOS DE TESTE:';
PRINT '   • admin / admin123 (Administrador)';
PRINT '   • vendedor / venda123 (Vendedor)';
PRINT '   • gerente / gerente123 (Gerente)';
PRINT '';
PRINT '💰 CONFIGURADO PARA ANGOLA:';
PRINT '   • Moeda: Kwanza (AOA)';
PRINT '   • IVA: 14% (padrão angolano)';
PRINT '   • Idioma: Português de Angola';
PRINT '   • Fuso horário: Africa/Luanda';
PRINT '';
PRINT '✅ Sistema pronto para uso!';
GO