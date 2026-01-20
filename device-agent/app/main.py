import asyncio
import logging
from typing import Dict, Any, Optional
from fastapi import FastAPI, HTTPException
from pydantic import BaseModel

# Configuração de logging
logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s - %(name)s - %(levelname)s - %(message)s"
)
logger = logging.getLogger(__name__)

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