"""Serviço de comunicação assíncrona com dispositivos via Telnet/TCP"""
import asyncio
import logging
import os
import random
from typing import Tuple, Optional
from urllib.parse import urlparse

logger = logging.getLogger(__name__)


class TelnetDeviceClient:
    """Cliente assíncrono para comunicação via Telnet/TCP com dispositivos IoT"""

    def __init__(self, timeout: float = 5.0):
        """
        Inicializa o cliente

        Args:
            timeout: Tempo máximo de espera por resposta (em segundos)
        """
        self.timeout = timeout
        self.mock_mode = os.getenv("MOCK_DEVICES", "true").lower() == "true"
        if self.mock_mode:
            logger.info("Modo MOCK ativado - dispositivos serão simulados")

    async def execute_command(
        self,
        device_url: str,
        command: str,
        parameters: list[str]
    ) -> Tuple[bool, Optional[str]]:
        """
        Executa um comando em um dispositivo via Telnet/TCP

        Args:
            device_url: URL do dispositivo (ex: telnet://192.168.1.100:23)
            command: Comando a executar
            parameters: Lista de parâmetros

        Returns:
            Tupla (sucesso, resposta)
        """
        # Se estiver em modo mock, retorna dados simulados
        if self.mock_mode:
            return self._execute_mock_command(command, parameters)
        
        try:
            # Extrai host e porta da URL
            host, port = self._parse_device_url(device_url)

            # Monta a string de comando: cmd param1 param2\r
            command_string = self._format_command(command, parameters)

            logger.info(f"Conectando a {host}:{port}")

            # Abre conexão TCP assíncrona
            reader, writer = await asyncio.wait_for(
                asyncio.open_connection(host, port),
                timeout=self.timeout
            )

            try:
                logger.debug(f"Enviando comando: {repr(command_string)}")

                # Envia o comando com terminador \r
                writer.write(command_string.encode('utf-8'))
                await writer.drain()

                # Aguarda a resposta
                response = await asyncio.wait_for(
                    self._read_until_terminator(reader),
                    timeout=self.timeout
                )

                logger.info(f"Resposta recebida: {repr(response)}")

                return True, response

            finally:
                writer.close()
                await writer.wait_closed()

        except asyncio.TimeoutError:
            error_msg = f"Timeout ao comunicar com dispositivo {device_url}"
            logger.error(error_msg)
            return False, error_msg

        except ConnectionRefusedError:
            error_msg = f"Conexão recusada ao dispositivo {device_url}"
            logger.error(error_msg)
            return False, error_msg

        except OSError as e:
            error_msg = f"Erro de comunicação com dispositivo {device_url}: {str(e)}"
            logger.error(error_msg)
            return False, error_msg

        except Exception as e:
            error_msg = f"Erro inesperado ao comunicar com dispositivo: {str(e)}"
            logger.error(error_msg, exc_info=True)
            return False, error_msg

    async def _read_until_terminator(self, reader: asyncio.StreamReader, terminator: bytes = b'\r') -> str:
        """
        Lê dados do stream até encontrar o terminador

        Args:
            reader: StreamReader assíncrono
            terminator: Bytes terminadores (padrão: \r)

        Returns:
            Dados lidos como string
        """
        data = b''
        while True:
            try:
                chunk = await asyncio.wait_for(
                    reader.read(1),
                    timeout=self.timeout
                )

                if not chunk:
                    break

                data += chunk

                if data.endswith(terminator):
                    # Remove o terminador antes de retornar
                    return data[:-len(terminator)].decode('utf-8', errors='ignore')

            except asyncio.TimeoutError:
                if data:
                    return data.decode('utf-8', errors='ignore')
                raise

        return data.decode('utf-8', errors='ignore')

    @staticmethod
    def _parse_device_url(device_url: str) -> Tuple[str, int]:
        """
        Extrai host e porta de uma URL de dispositivo

        Args:
            device_url: URL do dispositivo (ex: telnet://192.168.1.100:23)

        Returns:
            Tupla (host, porta)
        """
        parsed = urlparse(device_url)

        host = parsed.hostname or "localhost"
        port = parsed.port or 23

        return host, port

    @staticmethod
    def _format_command(command: str, parameters: list[str]) -> str:
        """
        Formata o comando com parâmetros

        Regras:
        - Separador: espaço (\x20)
        - Terminador: \r (Carriage Return)

        Args:
            command: Comando a executar
            parameters: Lista de parâmetros

        Returns:
            String formatada com terminador
        """
        # Monta a string: cmd param1 param2 param3
        parts = [command] + parameters
        command_string = ' '.join(parts)

        # Adiciona terminador \r
        return command_string + '\r'

    def _execute_mock_command(self, command: str, parameters: list[str]) -> Tuple[bool, str]:
        """
        Executa comando em modo simulado (mock)
        
        Args:
            command: Comando a executar
            parameters: Lista de parâmetros
            
        Returns:
            Tupla (sucesso, resposta simulada)
        """
        logger.info(f"[MOCK] Executando comando simulado: {command} {' '.join(parameters)}")
        
        # Respostas simuladas baseadas no comando
        mock_responses = {
            "READ_TEMP": f"OK TEMP={random.randint(15, 35)}.{random.randint(0, 9)}C",
            "READ_HUM": f"OK HUMIDITY={random.randint(30, 80)}%",
            "READ_RAIN": f"OK RAINFALL={random.randint(0, 100)}mm",
            "READ": f"OK VALUE={random.randint(20, 80)}",
            "STATUS": f"OK ZONE={parameters[0] if parameters else '1'} STATUS=ACTIVE",
            "START": f"OK ZONE={parameters[0] if parameters else '1'} STARTED DURATION={parameters[1] if len(parameters) > 1 else '30'}min",
            "STOP": f"OK ZONE={parameters[0] if parameters else '1'} STOPPED",
            "CONFIGURE": f"OK THRESHOLD={parameters[0] if parameters else '50'} CONFIGURED"
        }
        
        # Retorna resposta baseada no comando ou resposta genérica
        response = mock_responses.get(command, f"OK {command} EXECUTED")
        logger.info(f"[MOCK] Resposta simulada: {response}")
        
        return True, response
