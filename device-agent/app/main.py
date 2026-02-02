# ===========================================================================================
# DEVICE AGENT PYTHON - main.py
# ===========================================================================================
# Este é o agente intermediário que se comunica com dispositivos IoT via protocolo Telnet.
# Recebe requisições HTTP da API .NET e as converte em comandos Telnet para os dispositivos.
#
# Arquitetura:
# Frontend Angular → .NET API → Device Agent Python (este arquivo) → Dispositivo IoT (Telnet)
#
# Funções principais:
# - Receber requisições HTTP POST com comandos para executar
# - Abrir conexão Telnet (TCP) com o dispositivo
# - Enviar comando formatado: "comando param1 param2\r"
# - Aguardar resposta do dispositivo terminada em "\r"
# - Retornar resposta via HTTP JSON
# ===========================================================================================

import asyncio
import logging
import os
import random
from typing import Dict, Any, Optional
from fastapi import FastAPI, HTTPException
from pydantic import BaseModel

# ===========================================================================================
# CONFIGURAÇÃO DE LOGGING
# ===========================================================================================
# Define o formato e nível de logs para debug e monitoramento

logging.basicConfig(
    level=logging.INFO,  # Nível: DEBUG, INFO, WARNING, ERROR, CRITICAL
    format="%(asctime)s - %(name)s - %(levelname)s - %(message)s"
)
logger = logging.getLogger(__name__)

# ===========================================================================================
# MODO DE SIMULAÇÃO
# ===========================================================================================
# Variável de ambiente para ativar/desativar modo MOCK (sem dispositivos reais)
# No Docker Compose, definido como MOCK_DEVICES=true

MOCK_MODE = os.getenv("MOCK_DEVICES", "true").lower() == "true"
if MOCK_MODE:
    logger.info("🔧 Modo MOCK ativado - dispositivos serão simulados")

# ===========================================================================================
# CONFIGURAÇÃO FASTAPI
# ===========================================================================================
# FastAPI é um framework web moderno e rápido para criar APIs em Python

app = FastAPI(
    title="CIoTD Device Agent",
    description="Agente Python para comunicação Telnet com dispositivos IoT",
    version="1.0.0"
)


# ===========================================================================================
# MODELOS DE DADOS (Pydantic)
# ===========================================================================================
# Definem a estrutura dos dados de requisição e resposta
# Pydantic valida automaticamente os tipos e campos obrigatórios

class ExecuteCommandRequest(BaseModel):
    """Modelo de requisição para executar comando em dispositivo"""
    device_id: str                      # ID do dispositivo (para log)
    device_host: str                    # IP ou hostname do dispositivo
    device_port: int                    # Porta Telnet (geralmente 23)
    command: str                        # Comando a executar (ex: "READ_TEMP")
    parameters: Dict[str, Any] = {}     # Parâmetros opcionais do comando


class ExecuteCommandResponse(BaseModel):
    """Modelo de resposta após executar comando"""
    success: bool                       # true se executou com sucesso
    response: Optional[str] = None      # Resposta do dispositivo (se sucesso)
    error: Optional[str] = None         # Mensagem de erro (se falha)


# ===========================================================================================
# FUNÇÃO: mock_telnet_response
# ===========================================================================================
# Simula respostas de dispositivos IoT quando em modo MOCK (sem dispositivos reais)
# Útil para desenvolvimento e testes sem hardware físico
# ===========================================================================================

def mock_telnet_response(command: str, params: Dict[str, Any]) -> str:
    """
    Retorna resposta simulada para comandos
    
    Args:
        command: Comando a executar (ex: "READ_TEMP")
        params: Parâmetros do comando (ex: {"zone": "1"})
        
    Returns:
        Resposta simulada do dispositivo
    """
    logger.info(f"[MOCK] Executando comando simulado: {command} com parâmetros {params}")
    
    # Dicionário de respostas mockadas para diferentes comandos
    # Cada comando retorna dados aleatórios realisticamente
    mock_responses = {
        "READ_TEMP": f"OK TEMP={random.randint(15, 35)}.{random.randint(0, 9)}C",
        "READ_HUM": f"OK HUMIDITY={random.randint(30, 80)}%",
        "READ_RAIN": f"OK RAINFALL={random.randint(0, 100)}mm",
        "READ": f"OK VALUE={random.randint(20, 80)}",
        "STATUS": f"OK ZONE={params.get('param1', '1')} STATUS=ACTIVE",
        "START": f"OK ZONE={params.get('param1', '1')} STARTED DURATION={params.get('param2', '30')}min",
        "STOP": f"OK ZONE={params.get('param1', '1')} STOPPED",
        "CONFIGURE": f"OK THRESHOLD={params.get('param1', '50')} CONFIGURED"
    }
    
    # Retorna resposta baseada no comando ou resposta genérica se comando desconhecido
    response = mock_responses.get(command, f"OK {command} EXECUTED")
    logger.info(f"[MOCK] Resposta simulada: {response}")
    
    return response


# ===========================================================================================
# FUNÇÃO ASYNC: send_telnet_command
# ===========================================================================================
# Conecta via TCP (Telnet), envia comando formatado e aguarda resposta do dispositivo
# ===========================================================================================

async def send_telnet_command(
    host: str, 
    port: int, 
    command: str, 
    params: Dict[str, Any],
    timeout: float = 5.0
) -> str:
    """
    Conecta via TCP (Telnet), envia comando formatado e retorna resposta.
    
    Formato do comando: comando param1 param2 ... \r
    Formato da resposta: string terminada em \r
    
    Args:
        host: IP ou hostname do dispositivo
        port: Porta Telnet (geralmente 23)
        command: Comando a executar (ex: "READ_TEMP")
        params: Dicionário de parâmetros
        timeout: Tempo máximo de espera em segundos
        
    Returns:
        Resposta do dispositivo sem o \r final
        
    Raises:
        HTTPException: Em caso de timeout ou erro de conexão
    """
    # Se estiver em modo mock, retorna dados simulados (não conecta ao dispositivo real)
    if MOCK_MODE:
        return mock_telnet_response(command, params)
    
    # PASSO 1: Construir string de comando no formato: "comando param1 param2\r"
    param_values = [str(v) for v in params.values()] if params else []
    command_str = command
    if param_values:
        command_str = f"{command} {' '.join(param_values)}"
    command_str += "\r"  # Adiciona terminador de linha (carriage return)
    
    logger.info(f"Conectando a {host}:{port} para enviar: {repr(command_str)}")
    
    try:
        # PASSO 2: Abrir conexão TCP assíncrona (Telnet é TCP na porta 23)
        # asyncio.open_connection retorna um reader (para ler) e writer (para escrever)
        reader, writer = await asyncio.wait_for(
            asyncio.open_connection(host, port),
            timeout=timeout  # Timeout para a conexão
        )
        
        # PASSO 3: Enviar comando para o dispositivo
        writer.write(command_str.encode('utf-8'))  # Converte string para bytes UTF-8
        await writer.drain()  # Aguarda os dados serem realmente enviados
        logger.info(f"Comando enviado: {repr(command_str)}")
        
        # PASSO 4: Aguardar resposta terminada em \r (carriage return)
        # readuntil lê bytes até encontrar o delimitador especificado
        response_bytes = await asyncio.wait_for(
            reader.readuntil(b'\r'),  # Lê até encontrar \r
            timeout=timeout  # Timeout para a resposta
        )
        
        # PASSO 5: Converter resposta de bytes para string e remover o \r final
        response = response_bytes.decode('utf-8').rstrip('\r')
        logger.info(f"Resposta recebida: {repr(response)}")
        
        # PASSO 6: Fechar conexão TCP graciosamente
        writer.close()
        await writer.wait_closed()
        
        # PASSO 7: Retornar a resposta do dispositivo
        return response
        
    except asyncio.TimeoutError:
        # Timeout ao conectar ou aguardar resposta
        logger.error(f"Timeout ao conectar/aguardar resposta de {host}:{port}")
        raise HTTPException(status_code=504, detail="Timeout na comunicação com dispositivo")
    except Exception as e:
        # Qualquer outro erro (conexão recusada, dispositivo offline, etc)
        logger.error(f"Erro na comunicação Telnet: {e}")
        raise HTTPException(status_code=500, detail=f"Erro Telnet: {str(e)}")


# ===========================================================================================
# ENDPOINT: POST /api/execute
# ===========================================================================================
# Endpoint principal que recebe requisições para executar comandos em dispositivos IoT
# ===========================================================================================

@app.post("/api/execute", response_model=ExecuteCommandResponse)
async def execute_command(request: ExecuteCommandRequest):
    """
    Executa comando em dispositivo IoT via Telnet (TCP).
    
    Recebe:
        - device_id: identificador do dispositivo
        - device_host: IP/hostname do dispositivo
        - device_port: porta Telnet
        - command: comando a executar
        - parameters: parâmetros do comando
        
    Retorna:
        - success: true/false
        - response: resposta do dispositivo (se sucesso)
        - error: mensagem de erro (se falha)
    """
    logger.info(
        f"Executando comando no dispositivo {request.device_id}: "
        f"{request.command} com parâmetros {request.parameters}"
    )
    
    try:
        # Envia comando via Telnet (ou mock se MOCK_MODE=true)
        response = await send_telnet_command(
            host=request.device_host,
            port=request.device_port,
            command=request.command,
            params=request.parameters
        )
        
        # Retorna resposta de sucesso
        return ExecuteCommandResponse(
            success=True,
            response=response
        )
    except HTTPException:
        # Re-lança HTTPException (já tem código de status correto)
        raise
    except Exception as e:
        # Qualquer outro erro - retorna como resposta de falha
        logger.error(f"Erro ao executar comando: {e}")
        return ExecuteCommandResponse(
            success=False,
            error=str(e)
        )


# ===========================================================================================
# ENDPOINTS AUXILIARES
# ===========================================================================================

@app.get("/api/health")
async def health_check():
    """Endpoint de health check para monitoramento e verificação de status"""
    return {"status": "healthy", "service": "device-agent"}


@app.get("/")
async def root():
    """Endpoint raiz que retorna informações sobre o serviço"""
    return {
        "service": "CIoTD Device Agent",
        "version": "1.0.0",
        "docs": "/docs"  # URL da documentação automática do FastAPI
    }


# ===========================================================================================
# INICIALIZAÇÃO DO SERVIDOR
# ===========================================================================================
# Executa o servidor apenas se este arquivo for executado diretamente (não importado)

if __name__ == "__main__":
    import uvicorn
    logger.info("Iniciando Device Agent...")
    
    # Uvicorn é o servidor ASGI que executa a aplicação FastAPI
    # host="0.0.0.0" permite conexões de qualquer endereço (importante para Docker)
    # port=8000 define a porta (no Docker Compose é mapeada para 8001 externamente)
    uvicorn.run(app, host="0.0.0.0", port=8000)