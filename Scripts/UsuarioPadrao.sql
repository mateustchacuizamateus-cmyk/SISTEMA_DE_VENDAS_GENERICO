-- Script para inserir usuário padrão para teste
USE SistemaVendas;
GO

-- Inserir usuário administrador padrão
INSERT INTO Usuarios (Nome, Usuario, Senha, NivelAcesso, Ativo, DataCadastro)
VALUES ('Administrador', 'admin', 'admin123', 'Administrador', 1, GETDATE());

-- Inserir usuário vendedor padrão
INSERT INTO Usuarios (Nome, Usuario, Senha, NivelAcesso, Ativo, DataCadastro)
VALUES ('Vendedor', 'vendedor', 'venda123', 'Vendedor', 1, GETDATE());

-- Inserir usuário gerente padrão
INSERT INTO Usuarios (Nome, Usuario, Senha, NivelAcesso, Ativo, DataCadastro)
VALUES ('Gerente', 'gerente', 'gerente123', 'Gerente', 1, GETDATE());

PRINT 'Usuários padrão criados com sucesso!';
PRINT 'Administrador: admin / admin123';
PRINT 'Vendedor: vendedor / venda123';
PRINT 'Gerente: gerente / gerente123';
GO 