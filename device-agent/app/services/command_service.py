"""Serviço de orquestração de comandos em dispositivos"""
import logging
import asyncio
import time
from typing import Dict, Optional
from app.models.schemas import CommandExecutionResult
from app.services.telnet_client import TelnetDeviceClient

logger = logging.getLogger(__name__)


class DeviceCommandService:
    """Serviço para orquestrar a execução de comandos em dispositivos"""

    def __init__(self):
        """Inicializa o serviço"""
        self.telnet_client = TelnetDeviceClient(timeout=10.0)
        # Mock de dispositivos e seus comandos
        self.devices = self._load_mock_devices()

    async def execute_command(
        self,
        device_id: str,
        operation: str,
        parameters: Dict[str, str],
        device_url: Optional[str] = None
    ) -> CommandExecutionResult:
        """
        Executa um comando em um dispositivo

        Args:
            device_id: Identificador do dispositivo
            operation: Nome da operação
            parameters: Dicionário de parâmetros
            device_url: URL do dispositivo (usa mock se não fornecida)

        Returns:
            Resultado da execução
        """
        start_time = time.time()

        try:
            # Se não tiver URL, tenta obter do dispositivo mockado
            if not device_url:
                if device_id not in self.devices:
                    return CommandExecutionResult(
                        success=False,
                        error=f"Dispositivo {device_id} não encontrado",
                        execution_time_ms=int((time.time() - start_time) * 1000)
                    )

                device_info = self.devices[device_id]
                device_url = device_info.get("url", "telnet://localhost:23")

            # Obtém as informações da operação
            command_info = self._get_command_for_operation(device_id, operation)

            if not command_info:
                return CommandExecutionResult(
                    success=False,
                    error=f"Operação {operation} não encontrada para o dispositivo {device_id}",
                    execution_time_ms=int((time.time() - start_time) * 1000)
                )

            # Monta o comando com parâmetros
            command_string = command_info["command"]
            param_list = self._build_parameter_list(command_info, parameters)

            logger.info(
                f"Executando comando no dispositivo {device_id}: "
                f"operação={operation}, comando={command_string}, parâmetros={param_list}"
            )

            # Executa via Telnet/TCP
            success, response = await self.telnet_client.execute_command(
                device_url,
                command_string,
                param_list
            )

            execution_time_ms = int((time.time() - start_time) * 1000)

            if success:
                logger.info(
                    f"Comando executado com sucesso em {execution_time_ms}ms. "
                    f"Resposta: {response}"
                )
                return CommandExecutionResult(
                    success=True,
                    data=response,
                    execution_time_ms=execution_time_ms
                )
            else:
                logger.error(f"Erro ao executar comando: {response}")
                return CommandExecutionResult(
                    success=False,
                    error=response,
                    execution_time_ms=execution_time_ms
                )

        except Exception as e:
            execution_time_ms = int((time.time() - start_time) * 1000)
            error_msg = f"Erro inesperado ao executar comando: {str(e)}"
            logger.error(error_msg, exc_info=True)
            return CommandExecutionResult(
                success=False,
                error=error_msg,
                execution_time_ms=execution_time_ms
            )

    def _get_command_for_operation(self, device_id: str, operation: str) -> Optional[Dict]:
        """
        Obtém informações do comando para uma operação

        Args:
            device_id: Identificador do dispositivo
            operation: Nome da operação

        Returns:
            Dicionário com informações do comando ou None
        """
        if device_id not in self.devices:
            return None

        device = self.devices[device_id]
        for cmd in device.get("commands", []):
            if cmd["operation"] == operation:
                return cmd["command"]

        return None

    @staticmethod
    def _build_parameter_list(command_info: Dict, parameters: Dict[str, str]) -> list[str]:
        """
        Constrói a lista de parâmetros na ordem esperada pelo comando

        Args:
            command_info: Informações do comando
            parameters: Dicionário de parâmetros fornecidos

        Returns:
            Lista de valores de parâmetros
        """
        param_list = []
        param_defs = command_info.get("parameters", [])

        for param_def in param_defs:
            param_name = param_def.get("name")
            if param_name in parameters:
                param_list.append(parameters[param_name])

        return param_list

    @staticmethod
    def _load_mock_devices() -> Dict:
        """
        Carrega os dispositivos mockados

        Returns:
            Dicionário de dispositivos
        """
        return {
            "sensor-soil-001": {
                "url": "telnet://192.168.1.100:23",
                "description": "Sensor de umidade e temperatura do solo",
                "commands": [
                    {
                        "operation": "READ_HUMIDITY",
                        "command": {
                            "command": "READ",
                            "parameters": [
                                {"name": "sensor_type", "description": "Tipo de sensor"}
                            ]
                        }
                    },
                    {
                        "operation": "SET_THRESHOLD",
                        "command": {
                            "command": "CONFIGURE",
                            "parameters": [
                                {"name": "threshold", "description": "Limiar de alerta"},
                                {"name": "unit", "description": "Unidade"}
                            ]
                        }
                    }
                ]
            },
            "sensor-weather-001": {
                "url": "telnet://192.168.1.101:23",
                "description": "Estação meteorológica",
                "commands": [
                    {
                        "operation": "READ_TEMPERATURE",
                        "command": {
                            "command": "READ_TEMP",
                            "parameters": []
                        }
                    },
                    {
                        "operation": "READ_HUMIDITY",
                        "command": {
                            "command": "READ_HUM",
                            "parameters": []
                        }
                    },
                    {
                        "operation": "READ_RAINFALL",
                        "command": {
                            "command": "READ_RAIN",
                            "parameters": [
                                {"name": "period", "description": "Período"}
                            ]
                        }
                    }
                ]
            },
            "irrigation-system-001": {
                "url": "telnet://192.168.1.102:23",
                "description": "Sistema de irrigação",
                "commands": [
                    {
                        "operation": "START_IRRIGATION",
                        "command": {
                            "command": "START",
                            "parameters": [
                                {"name": "zone", "description": "Zona"},
                                {"name": "duration", "description": "Duração em minutos"}
                            ]
                        }
                    },
                    {
                        "operation": "STOP_IRRIGATION",
                        "command": {
                            "command": "STOP",
                            "parameters": [
                                {"name": "zone", "description": "Zona"}
                            ]
                        }
                    },
                    {
                        "operation": "GET_ZONE_STATUS",
                        "command": {
                            "command": "STATUS",
                            "parameters": [
                                {"name": "zone", "description": "Zona"}
                            ]
                        }
                    }
                ]
            }
        }
