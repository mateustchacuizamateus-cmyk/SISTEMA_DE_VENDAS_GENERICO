-- Script para inserir dados de teste no Sistema de Vendas
-- Execute este script após criar as tabelas

-- Inserir Categorias
INSERT INTO Categorias (Nome, Descricao, Ativo) VALUES 
('Bebidas', 'Bebidas em geral', 1),
('Alimentos', 'Produtos alimentícios', 1),
('Higiene', 'Produtos de higiene pessoal', 1),
('Limpeza', 'Produtos de limpeza', 1),
('Eletrónicos', 'Produtos electrónicos', 1);

-- Inserir Produtos
INSERT INTO Produtos (Nome, CodigoBarra, IdCategoria, PrecoCompra, PrecoVenda, EstoqueAtual, Unidade, Ativo) VALUES 
('Coca-Cola 330ml', '7891234567890', 1, 50.00, 75.00, 100, 'Unidade', 1),
('Pepsi 330ml', '7891234567891', 1, 45.00, 70.00, 80, 'Unidade', 1),
('Água Mineral 500ml', '7891234567892', 1, 25.00, 40.00, 150, 'Unidade', 1),
('Pão Francês', '7891234567893', 2, 15.00, 25.00, 50, 'Unidade', 1),
('Arroz 5kg', '7891234567894', 2, 800.00, 1200.00, 30, 'Saco', 1),
('Feijão 1kg', '7891234567895', 2, 300.00, 450.00, 40, 'Pacote', 1),
('Sabonete', '7891234567896', 3, 80.00, 120.00, 60, 'Unidade', 1),
('Shampoo', '7891234567897', 3, 150.00, 250.00, 25, 'Unidade', 1),
('Detergente', '7891234567898', 4, 120.00, 180.00, 35, 'Unidade', 1),
('Pilhas AA', '7891234567899', 5, 50.00, 80.00, 100, 'Pacote', 1);

-- Inserir Usuários
INSERT INTO Usuarios (Nome, Usuario, Senha, NivelAcesso, Ativo, DataCadastro) VALUES 
('Administrador', 'admin', 'admin123', 'Administrador', 1, GETDATE()),
('João Silva', 'joao', 'joao123', 'Gerente', 1, GETDATE()),
('Maria Santos', 'maria', 'maria123', 'Vendedor', 1, GETDATE()),
('Pedro Costa', 'pedro', 'pedro123', 'Vendedor', 1, GETDATE());

-- Inserir Clientes
INSERT INTO Clientes (Nome, Telefone, Email, Endereco, DataCadastro) VALUES 
('Carlos Oliveira', '+244 912 345 678', 'carlos@email.com', 'Rua das Flores, 123 - Luanda', GETDATE()),
('Ana Pereira', '+244 923 456 789', 'ana@email.com', 'Av. 4 de Fevereiro, 456 - Luanda', GETDATE()),
('Manuel Santos', '+244 934 567 890', 'manuel@email.com', 'Rua Rainha Ginga, 789 - Luanda', GETDATE()),
('Isabel Costa', '+244 945 678 901', 'isabel@email.com', 'Av. Ho Chi Minh, 321 - Luanda', GETDATE()),
('Francisco Lima', '+244 956 789 012', 'francisco@email.com', 'Rua Comandante Valódia, 654 - Luanda', GETDATE());

-- Inserir Fornecedores
INSERT INTO Fornecedores (Nome, Telefone, Email, Endereco) VALUES 
('Distribuidora Central', '+244 911 111 111', 'central@distribuidora.ao', 'Zona Industrial, Luanda'),
('Importadora Luanda', '+244 922 222 222', 'info@importadora.ao', 'Porto de Luanda'),
('Comercial Angola', '+244 933 333 333', 'vendas@comercial.ao', 'Centro Comercial, Luanda');

-- Inserir Formas de Pagamento
INSERT INTO FormasPagamento (Descricao) VALUES 
('Dinheiro'),
('Multicaixa'),
('Cartão de Crédito'),
('Cartão de Débito'),
('Transferência');

-- Inserir Configurações
INSERT INTO Configuracoes (NomeEmpresa, Endereco, Telefone, Email) VALUES 
('Sistema de Vendas Genérico', 'Luanda, Angola', '+244 999 999 999', 'contato@sistema.ao');

-- Inserir algumas vendas de exemplo
INSERT INTO Vendas (IdCliente, IdUsuario, DataVenda, Total, TipoPagamento, Desconto) VALUES 
(1, 3, DATEADD(day, -1, GETDATE()), 150.00, 'Dinheiro', 0),
(2, 4, DATEADD(day, -1, GETDATE()), 320.00, 'Multicaixa', 20.00),
(3, 3, GETDATE(), 95.00, 'Dinheiro', 0),
(4, 4, GETDATE(), 180.00, 'Cartão de Débito', 0);

-- Inserir itens das vendas
INSERT INTO ItensVenda (IdVenda, IdProduto, Quantidade, PrecoUnitario, TotalItem) VALUES 
(1, 1, 2, 75.00, 150.00),
(2, 5, 1, 1200.00, 1200.00),
(2, 6, 2, 450.00, 900.00),
(2, 7, 1, 120.00, 120.00),
(3, 3, 2, 40.00, 80.00),
(3, 4, 1, 25.00, 25.00),
(4, 8, 1, 250.00, 250.00),
(4, 9, 1, 180.00, 180.00);

-- Atualizar estoque dos produtos vendidos
UPDATE Produtos SET EstoqueAtual = EstoqueAtual - 2 WHERE Id = 1;
UPDATE Produtos SET EstoqueAtual = EstoqueAtual - 1 WHERE Id = 5;
UPDATE Produtos SET EstoqueAtual = EstoqueAtual - 2 WHERE Id = 6;
UPDATE Produtos SET EstoqueAtual = EstoqueAtual - 1 WHERE Id = 7;
UPDATE Produtos SET EstoqueAtual = EstoqueAtual - 2 WHERE Id = 3;
UPDATE Produtos SET EstoqueAtual = EstoqueAtual - 1 WHERE Id = 4;
UPDATE Produtos SET EstoqueAtual = EstoqueAtual - 1 WHERE Id = 8;
UPDATE Produtos SET EstoqueAtual = EstoqueAtual - 1 WHERE Id = 9;

PRINT 'Dados de teste inseridos com sucesso!'
PRINT 'Usuários criados:'
PRINT '  - admin/admin123 (Administrador)'
PRINT '  - joao/joao123 (Gerente)'
PRINT '  - maria/maria123 (Vendedor)'
PRINT '  - pedro/pedro123 (Vendedor)' 