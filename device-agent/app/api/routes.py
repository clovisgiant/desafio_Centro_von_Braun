"""Endpoints da API Device Agent"""
import logging
from fastapi import APIRouter, Depends, HTTPException, status
from app.models.schemas import CommandExecutionRequest, CommandExecutionResult, HealthResponse
from app.services.command_service import DeviceCommandService

logger = logging.getLogger(__name__)
router = APIRouter(prefix="/api", tags=["Device Commands"])


def get_command_service() -> DeviceCommandService:
    """Dependency injection do serviço de comandos"""
    return DeviceCommandService()


@router.post(
    "/execute",
    response_model=CommandExecutionResult,
    status_code=status.HTTP_200_OK,
    summary="Executa um comando em um dispositivo",
    description="Envia um comando para ser executado em um dispositivo IoT via Telnet/TCP"
)
async def execute_command(
    request: CommandExecutionRequest,
    service: DeviceCommandService = Depends(get_command_service)
) -> CommandExecutionResult:
    """
    Executa um comando em um dispositivo

    - **device_id**: Identificador do dispositivo
    - **operation**: Nome da operação a executar
    - **parameters**: Dicionário de parâmetros (chave -> valor)
    """
    logger.info(
        f"Recebida requisição de execução: device_id={request.device_id}, "
        f"operation={request.operation}, parameters={request.parameters}"
    )

    result = await service.execute_command(
        request.device_id,
        request.operation,
        request.parameters
    )

    return result


@router.get(
    "/health",
    response_model=HealthResponse,
    status_code=status.HTTP_200_OK,
    summary="Health check",
    description="Verifica se o serviço está saudável"
)
async def health_check() -> HealthResponse:
    """
    Verifica a saúde do serviço Device Agent
    """
    return HealthResponse(
        status="healthy",
        message="Device Agent funcionando corretamente"
    )
