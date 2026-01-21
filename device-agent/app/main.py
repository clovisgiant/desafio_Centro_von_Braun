import asyncio
import logging
import os
import random
from typing import Dict, Any, Optional
from fastapi import FastAPI, HTTPException
from pydantic import BaseModel

# Configuração de logging
logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s - %(name)s - %(levelname)s - %(message)s"
)
logger = logging.getLogger(__name__)

# Modo de simulação
MOCK_MODE = os.getenv("MOCK_DEVICES", "true").lower() == "true"
if MOCK_MODE:
    logger.info("🔧 Modo MOCK ativado - dispositivos serão simulados")

app = FastAPI(
    title="CIoTD Device Agent",
    description="Agente Python para comunicação Telnet com dispositivos IoT",
    version="1.0.0"
)


class ExecuteCommandRequest(BaseModel):
    device_id: str
    device_host: str
    device_port: int
    command: str
    parameters: Dict[str, Any] = {}


class ExecuteCommandResponse(BaseModel):
    success: bool
    response: Optional[str] = None
    error: Optional[str] = None


def mock_telnet_response(command: str, params: Dict[str, Any]) -> str:
    """
    Retorna resposta simulada para comandos
    
    Args:
        command: Comando a executar
        params: Parâmetros do comando
        
    Returns:
        Resposta simulada
    """
    logger.info(f"[MOCK] Executando comando simulado: {command} com parâmetros {params}")
    
    # Respostas simuladas baseadas no comando
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
    
    # Retorna resposta baseada no comando ou resposta genérica
    response = mock_responses.get(command, f"OK {command} EXECUTED")
    logger.info(f"[MOCK] Resposta simulada: {response}")
    
    return response


async def send_telnet_command(
    host: str, 
    port: int, 
    command: str, 
    params: Dict[str, Any],
    timeout: float = 5.0
) -> str:
    """
    Conecta via TCP (Telnet), envia comando formatado e retorna resposta.
    
    Formato: comando param1 param2 ... \r
    Resposta: string terminada em \r
    """
    # Se estiver em modo mock, retorna dados simulados
    if MOCK_MODE:
        return mock_telnet_response(command, params)
    
    # Construir string de comando: comando + params separados por espaço + \r
    param_values = [str(v) for v in params.values()] if params else []
    command_str = command
    if param_values:
        command_str = f"{command} {' '.join(param_values)}"
    command_str += "\r"
    
    logger.info(f"Conectando a {host}:{port} para enviar: {repr(command_str)}")
    
    try:
        # Abrir conexão TCP assíncrona
        reader, writer = await asyncio.wait_for(
            asyncio.open_connection(host, port),
            timeout=timeout
        )
        
        # Enviar comando
        writer.write(command_str.encode('utf-8'))
        await writer.drain()
        logger.info(f"Comando enviado: {repr(command_str)}")
        
        # Aguardar resposta terminada em \r
        response_bytes = await asyncio.wait_for(
            reader.readuntil(b'\r'),
            timeout=timeout
        )
        
        response = response_bytes.decode('utf-8').rstrip('\r')
        logger.info(f"Resposta recebida: {repr(response)}")
        
        # Fechar conexão
        writer.close()
        await writer.wait_closed()
        
        return response
        
    except asyncio.TimeoutError:
        logger.error(f"Timeout ao conectar/aguardar resposta de {host}:{port}")
        raise HTTPException(status_code=504, detail="Timeout na comunicação com dispositivo")
    except Exception as e:
        logger.error(f"Erro na comunicação Telnet: {e}")
        raise HTTPException(status_code=500, detail=f"Erro Telnet: {str(e)}")


@app.post("/api/execute", response_model=ExecuteCommandResponse)
async def execute_command(request: ExecuteCommandRequest):
    """
    Executa comando em dispositivo IoT via Telnet (TCP).
    """
    logger.info(
        f"Executando comando no dispositivo {request.device_id}: "
        f"{request.command} com parâmetros {request.parameters}"
    )
    
    try:
        response = await send_telnet_command(
            host=request.device_host,
            port=request.device_port,
            command=request.command,
            params=request.parameters
        )
        
        return ExecuteCommandResponse(
            success=True,
            response=response
        )
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Erro ao executar comando: {e}")
        return ExecuteCommandResponse(
            success=False,
            error=str(e)
        )


@app.get("/api/health")
async def health_check():
    """Health check endpoint"""
    return {"status": "healthy", "service": "device-agent"}


@app.get("/")
async def root():
    return {
        "service": "CIoTD Device Agent",
        "version": "1.0.0",
        "docs": "/docs"
    }


if __name__ == "__main__":
    import uvicorn
    logger.info("Iniciando Device Agent...")
    uvicorn.run(app, host="0.0.0.0", port=8000)