# 🚀 Guia Completo - Deploy do Projeto CIoTD no Azure

## 📋 Índice
1. [Pré-requisitos](#pré-requisitos)
2. [Opção 1: Azure Container Apps (Recomendado)](#opção-1-azure-container-apps)
3. [Opção 2: Azure VM com Docker](#opção-2-azure-vm-com-docker)
4. [Configuração de Domínio e HTTPS](#configuração-de-domínio-e-https)
5. [Custos Estimados](#custos-estimados)

---

## 📌 Pré-requisitos

### 1. Conta Azure
- Crie uma conta em: https://azure.microsoft.com/free
- Free tier inclui $200 de créditos por 30 dias

### 2. Azure CLI (Interface de Linha de Comando)
```powershell
# Instalar Azure CLI no Windows
winget install Microsoft.AzureCLI

# Ou baixar de: https://aka.ms/installazurecliwindows
```

### 3. Docker Desktop
- Já deve estar instalado (você está usando no projeto)

### 4. Git
- Para versionar e fazer push do código

---

## ⭐ Opção 1: Azure Container Apps (RECOMENDADO)

### Vantagens:
- ✅ Serverless (não gerencia VMs)
- ✅ Auto-scaling
- ✅ HTTPS automático
- ✅ Free tier disponível
- ✅ Fácil deploy

### Passo 1: Login no Azure

```powershell
# Login na conta Azure
az login

# Verificar assinatura ativa
az account show
```

### Passo 2: Criar Resource Group

```powershell
# Criar grupo de recursos
az group create --name rg-ciotd --location eastus

# Verificar
az group list --output table
```

### Passo 3: Criar Azure Container Registry (ACR)

```powershell
# Criar registry para armazenar suas imagens Docker
az acr create --resource-group rg-ciotd --name ciotdregistry --sku Basic

# Fazer login no registry
az acr login --name ciotdregistry
```

### Passo 4: Build e Push das Imagens Docker

```powershell
# Navegar até a pasta do projeto
cd c:\Projeto_Piloto\desafio_Centro_von_Braun

# Build e Push do Backend
docker build -t ciotdregistry.azurecr.io/ciotd-backend:latest ./backend-dotnet
docker push ciotdregistry.azurecr.io/ciotd-backend:latest

# Build e Push do Device Agent
docker build -t ciotdregistry.azurecr.io/ciotd-device-agent:latest ./device-agent
docker push ciotdregistry.azurecr.io/ciotd-device-agent:latest

# Build e Push do Frontend
docker build -t ciotdregistry.azurecr.io/ciotd-frontend:latest -f ./frontend-angular/Dockerfile.prod ./frontend-angular/ciotd-frontend
docker push ciotdregistry.azurecr.io/ciotd-frontend:latest
```

**⚠️ IMPORTANTE:** Você precisará criar um `Dockerfile.prod` para o frontend. Veja seção "Dockerfiles de Produção" abaixo.

### Passo 5: Criar Container Apps Environment

```powershell
# Instalar extensão (se necessário)
az extension add --name containerapp --upgrade

# Criar ambiente
az containerapp env create `
  --name ciotd-env `
  --resource-group rg-ciotd `
  --location eastus
```

### Passo 6: Deploy dos Container Apps

#### 6.1 Device Agent
```powershell
az containerapp create `
  --name ciotd-device-agent `
  --resource-group rg-ciotd `
  --environment ciotd-env `
  --image ciotdregistry.azurecr.io/ciotd-device-agent:latest `
  --target-port 8000 `
  --ingress external `
  --registry-server ciotdregistry.azurecr.io `
  --env-vars MOCK_DEVICES=true `
  --cpu 0.5 --memory 1Gi
```

#### 6.2 Backend
```powershell
# Obter URL do device-agent
$DEVICE_AGENT_URL = az containerapp show `
  --name ciotd-device-agent `
  --resource-group rg-ciotd `
  --query properties.configuration.ingress.fqdn `
  --output tsv

az containerapp create `
  --name ciotd-backend `
  --resource-group rg-ciotd `
  --environment ciotd-env `
  --image ciotdregistry.azurecr.io/ciotd-backend:latest `
  --target-port 5000 `
  --ingress external `
  --registry-server ciotdregistry.azurecr.io `
  --env-vars "DEVICE_AGENT_URL=https://$DEVICE_AGENT_URL" `
  --cpu 0.5 --memory 1Gi
```

#### 6.3 Frontend
```powershell
# Obter URL do backend
$BACKEND_URL = az containerapp show `
  --name ciotd-backend `
  --resource-group rg-ciotd `
  --query properties.configuration.ingress.fqdn `
  --output tsv

az containerapp create `
  --name ciotd-frontend `
  --resource-group rg-ciotd `
  --environment ciotd-env `
  --image ciotdregistry.azurecr.io/ciotd-frontend:latest `
  --target-port 80 `
  --ingress external `
  --registry-server ciotdregistry.azurecr.io `
  --env-vars "API_URL=https://$BACKEND_URL" `
  --cpu 0.5 --memory 1Gi
```

### Passo 7: Obter URLs de Acesso

```powershell
# URL do Frontend
az containerapp show --name ciotd-frontend --resource-group rg-ciotd --query properties.configuration.ingress.fqdn

# URL do Backend
az containerapp show --name ciotd-backend --resource-group rg-ciotd --query properties.configuration.ingress.fqdn

# URL do Device Agent
az containerapp show --name ciotd-device-agent --resource-group rg-ciotd --query properties.configuration.ingress.fqdn
```

---

## 🖥️ Opção 2: Azure VM com Docker (Mais Simples)

### Vantagens:
- ✅ Roda exatamente como no seu computador
- ✅ Usa o mesmo docker-compose.yml
- ✅ Mais controle total

### Passo 1: Criar VM Ubuntu

```powershell
# Criar VM
az vm create `
  --resource-group rg-ciotd `
  --name vm-ciotd `
  --image Ubuntu2204 `
  --size Standard_B2s `
  --admin-username azureuser `
  --generate-ssh-keys

# Abrir portas
az vm open-port --port 4200 --resource-group rg-ciotd --name vm-ciotd --priority 1001
az vm open-port --port 5001 --resource-group rg-ciotd --name vm-ciotd --priority 1002
az vm open-port --port 8001 --resource-group rg-ciotd --name vm-ciotd --priority 1003
```

### Passo 2: Conectar na VM

```powershell
# Obter IP público
az vm show --resource-group rg-ciotd --name vm-ciotd --show-details --query publicIps --output tsv

# Conectar via SSH (substitua o IP)
ssh azureuser@SEU_IP_PUBLICO
```

### Passo 3: Instalar Docker na VM

```bash
# Atualizar sistema
sudo apt update && sudo apt upgrade -y

# Instalar Docker
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh

# Adicionar usuário ao grupo docker
sudo usermod -aG docker $USER

# Instalar Docker Compose
sudo apt install docker-compose -y

# Reiniciar sessão
exit
```

### Passo 4: Fazer Deploy do Projeto

```bash
# Conectar novamente via SSH
ssh azureuser@SEU_IP_PUBLICO

# Instalar Git
sudo apt install git -y

# Clonar seu repositório (você precisa ter o código no GitHub)
git clone https://github.com/SEU_USUARIO/SEU_REPO.git
cd SEU_REPO

# Subir os containers
docker-compose up -d

# Verificar status
docker-compose ps

# Ver logs
docker-compose logs -f
```

### Passo 5: Acessar o Sistema

```
Frontend: http://SEU_IP_PUBLICO:4200
Backend:  http://SEU_IP_PUBLICO:5001
Agent:    http://SEU_IP_PUBLICO:8001
```

---

## 📦 Dockerfiles de Produção

### Frontend Dockerfile (`frontend-angular/Dockerfile.prod`)

```dockerfile
# Stage 1: Build
FROM node:20 AS build
WORKDIR /app
COPY ciotd-frontend/package*.json ./
RUN npm ci
COPY ciotd-frontend/ ./
RUN npx @angular/cli@17 build --configuration production

# Stage 2: Production
FROM nginx:alpine
COPY --from=build /app/dist/ciotd-frontend/browser /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

### Nginx Config (`frontend-angular/nginx.conf`)

```nginx
server {
    listen 80;
    server_name _;
    root /usr/share/nginx/html;
    index index.html;

    location / {
        try_files $uri $uri/ /index.html;
    }

    location /api {
        proxy_pass ${API_URL};
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
    }
}
```

---

## 🔒 Configuração de Domínio e HTTPS

### Com Azure Container Apps (Automático)

```powershell
# Container Apps já vem com HTTPS automático!
# URLs são do tipo: https://seu-app.region.azurecontainerapps.io

# Para usar domínio próprio:
az containerapp hostname add `
  --name ciotd-frontend `
  --resource-group rg-ciotd `
  --hostname seudominio.com
```

### Com VM (Manual)

```bash
# Instalar Nginx e Certbot
sudo apt install nginx certbot python3-certbot-nginx -y

# Configurar Nginx como proxy reverso
sudo nano /etc/nginx/sites-available/ciotd

# Obter certificado SSL gratuito
sudo certbot --nginx -d seudominio.com
```

---

## 💰 Custos Estimados Mensais

### Opção 1: Container Apps
- **Free Tier:** Até 180,000 vCPU-segundos/mês GRÁTIS
- **Estimativa:** $0 - $15/mês (dependendo do uso)

### Opção 2: VM Standard_B2s
- **VM:** ~$30/mês
- **Disco:** ~$5/mês
- **IP Público:** ~$3/mês
- **Total:** ~$38/mês

### Recomendação:
💡 Comece com **Container Apps** no free tier!

---

## 🔄 Atualizar a Aplicação

### Container Apps
```powershell
# Rebuild e push nova versão
docker build -t ciotdregistry.azurecr.io/ciotd-frontend:v2 ./frontend-angular
docker push ciotdregistry.azurecr.io/ciotd-frontend:v2

# Atualizar container app
az containerapp update `
  --name ciotd-frontend `
  --resource-group rg-ciotd `
  --image ciotdregistry.azurecr.io/ciotd-frontend:v2
```

### VM
```bash
# Conectar via SSH
cd SEU_REPO

# Baixar atualizações
git pull

# Recriar containers
docker-compose down
docker-compose up -d --build
```

---

## 🛑 Deletar Recursos (Evitar Custos)

```powershell
# Deletar tudo de uma vez
az group delete --name rg-ciotd --yes --no-wait
```

---

## 🆘 Troubleshooting

### Ver logs dos Container Apps
```powershell
az containerapp logs show --name ciotd-backend --resource-group rg-ciotd --follow
```

### Ver logs da VM
```bash
docker-compose logs -f backend
```

### Container não inicia
- Verifique as variáveis de ambiente
- Confira se as portas estão corretas
- Veja os logs para erros

---

## 📚 Recursos Úteis

- [Azure Container Apps Docs](https://learn.microsoft.com/azure/container-apps/)
- [Azure CLI Reference](https://learn.microsoft.com/cli/azure/)
- [Azure Free Account](https://azure.microsoft.com/free)
- [Azure Pricing Calculator](https://azure.microsoft.com/pricing/calculator/)

---

## ✅ Checklist Final

- [ ] Conta Azure criada
- [ ] Azure CLI instalado
- [ ] Recursos criados (Resource Group)
- [ ] Imagens Docker buildadas e no registry
- [ ] Container Apps deployados
- [ ] URLs funcionando
- [ ] HTTPS configurado
- [ ] Domínio apontado (opcional)
- [ ] Backup configurado (opcional)

---

**Criado em:** Janeiro 2026  
**Projeto:** CIoTD - Centro von Braun  
**Plataforma:** Microsoft Azure

🚀 Boa sorte com o deploy!
