# 📋 Instruções de Instalação - Sistema de Vendas Genérico

## 🎯 Visão Geral

Este documento contém as instruções passo a passo para instalar e configurar o Sistema de Vendas Genérico em seu ambiente.

## 📋 Pré-requisitos

### Software Necessário
- **Windows 10/11** ou **Windows Server 2016+**
- **Visual Studio 2019** ou superior (Community Edition é suficiente)
- **SQL Server LocalDB** (incluído com Visual Studio)
- **.NET Framework 4.8** (geralmente já instalado no Windows 10/11)

### Verificação de Instalação
1. Abra o **Prompt de Comando** como administrador
2. Execute: `sqlcmd -L` (deve listar instâncias do SQL Server)
3. Execute: `dotnet --version` (deve mostrar versão do .NET)

## 🚀 Passos de Instalação

### 1. Preparação do Ambiente

#### 1.1 Instalar Visual Studio
1. Baixe o Visual Studio Community em: https://visualstudio.microsoft.com/pt-br/vs/community/
2. Durante a instalação, selecione:
   - **Desenvolvimento para desktop com .NET**
   - **SQL Server Express LocalDB**
3. Complete a instalação

#### 1.2 Verificar .NET Framework 4.8
1. Abra o **Painel de Controle**
2. Vá em **Programas e Recursos**
3. Verifique se ".NET Framework 4.8" está listado
4. Se não estiver, baixe em: https://dotnet.microsoft.com/download/dotnet-framework/net48

### 2. Configuração do Banco de Dados

#### 2.1 Abrir SQL Server Management Studio
1. Abra o **SQL Server Management Studio (SSMS)**
2. Conecte-se à instância **LocalDB**:
   - Nome do servidor: `(LocalDB)\MSSQLLocalDB`
   - Autenticação: **Windows Authentication**

#### 2.2 Criar Banco de Dados
1. Clique com botão direito em **Databases**
2. Selecione **New Database**
3. Nome: `SistemaVendas`
4. Clique em **OK**

#### 2.3 Executar Scripts SQL
1. Abra o arquivo `Scripts/CriarTabelas.sql`
2. Execute o script completo (F5)
3. Abra o arquivo `Scripts/InserirDadosTeste.sql`
4. Execute o script completo (F5)

### 3. Configuração do Projeto

#### 3.1 Abrir Projeto no Visual Studio
1. Abra o **Visual Studio**
2. Selecione **Open a project or solution**
3. Navegue até a pasta do projeto
4. Selecione `SISTEMA_DE_VENDAS_GENERICO.sln`

#### 3.2 Verificar Configurações
1. Abra o arquivo `App.config`
2. Verifique se a string de conexão está correta:
   ```xml
   <connectionStrings>
       <add name="SistemaVendasConnection" 
            connectionString="Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\Database1.mdf;Integrated Security=True;Connect Timeout=30"
            providerName="System.Data.SqlClient" />
   </connectionStrings>
   ```

#### 3.3 Compilar Projeto
1. No Visual Studio, pressione **Ctrl+Shift+B** para compilar
2. Verifique se não há erros de compilação
3. Se houver erros, verifique se todas as referências estão corretas

### 4. Execução do Sistema

#### 4.1 Primeira Execução
1. Pressione **F5** no Visual Studio para executar
2. O sistema deve abrir a tela de login
3. Use os dados de acesso padrão:
   - **Usuário**: `admin`
   - **Senha**: `admin123`

#### 4.2 Verificação de Funcionalidades
1. **Login**: Teste o acesso com diferentes usuários
2. **Dashboard**: Verifique se as estatísticas carregam
3. **Vendas**: Teste a funcionalidade de caixa
4. **Navegação**: Teste todos os menus

## 🔧 Solução de Problemas

### Problema: Erro de Conexão com Banco
**Sintomas**: Erro ao tentar conectar com SQL Server
**Solução**:
1. Verifique se o SQL Server LocalDB está rodando:
   ```cmd
   sqllocaldb info
   ```
2. Se não estiver, inicie:
   ```cmd
   sqllocaldb start "MSSQLLocalDB"
   ```

### Problema: Erro de Compilação
**Sintomas**: Erros ao compilar o projeto
**Solução**:
1. Verifique se todas as referências estão corretas
2. Restaure os pacotes NuGet (se houver)
3. Limpe e recompile a solução

### Problema: Sistema não Inicia
**Sintomas**: Aplicação não abre ou fecha imediatamente
**Solução**:
1. Verifique se o .NET Framework 4.8 está instalado
2. Execute como administrador
3. Verifique os logs de erro no Event Viewer

### Problema: Dados não Carregam
**Sintomas**: Dashboard vazio ou erros de dados
**Solução**:
1. Verifique se os scripts SQL foram executados
2. Confirme se há dados nas tabelas
3. Verifique a string de conexão

## 📊 Dados de Acesso Padrão

Após executar os scripts SQL, você terá acesso aos seguintes usuários:

| Usuário | Senha | Nível de Acesso | Permissões |
|---------|-------|-----------------|------------|
| admin | admin123 | Administrador | Acesso total |
| joao | joao123 | Gerente | Vendas, produtos, relatórios |
| maria | maria123 | Vendedor | Apenas vendas |
| pedro | pedro123 | Vendedor | Apenas vendas |

## 🔒 Configurações de Segurança

### Recomendações
1. **Altere as senhas padrão** após o primeiro acesso
2. **Configure backup automático** do banco de dados
3. **Limite o acesso** ao servidor de banco de dados
4. **Monitore os logs** de acesso

### Backup do Banco
1. No SSMS, clique com botão direito no banco `SistemaVendas`
2. Selecione **Tasks > Back Up**
3. Configure o caminho e frequência do backup

## 📞 Suporte

### Informações de Contato
- **Email**: suporte@sistema.ao
- **Telefone**: +244 999 999 999
- **Horário**: Segunda a Sexta, 8h às 17h

### Logs e Diagnóstico
- **Logs do Sistema**: Verifique a pasta `logs/` (se existir)
- **Logs do Banco**: Use o SQL Server Profiler
- **Logs do Windows**: Event Viewer > Applications

## ✅ Checklist de Instalação

- [ ] Visual Studio instalado
- [ ] SQL Server LocalDB funcionando
- [ ] .NET Framework 4.8 instalado
- [ ] Banco de dados criado
- [ ] Scripts SQL executados
- [ ] Projeto compilado sem erros
- [ ] Sistema executando
- [ ] Login funcionando
- [ ] Dashboard carregando dados
- [ ] Módulo de vendas testado

---

**🎉 Parabéns! Se você chegou até aqui, o sistema está instalado e funcionando corretamente.**

**📝 Lembre-se**: Este é um sistema de demonstração. Para uso em produção, considere implementar medidas de segurança adicionais. 