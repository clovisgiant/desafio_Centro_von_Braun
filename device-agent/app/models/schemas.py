"""Models para a API Device Agent"""
from pydantic import BaseModel
from typing import Dict, Any, Optional


class CommandExecutionRequest(BaseModel):
    """Requisição para executar um comando em um dispositivo"""
    device_id: str
    operation: str
    parameters: Dict[str, str]


class CommandExecutionResult(BaseModel):
    """Resultado da execução de um comando"""
    success: bool
    data: Optional[str] = None
    error: Optional[str] = None
    execution_time_ms: int = 0


class HealthResponse(BaseModel):
    """Resposta de health check"""
    status: str
    message: str
