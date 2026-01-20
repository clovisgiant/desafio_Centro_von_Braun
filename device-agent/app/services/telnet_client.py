"""Serviço de comunicação assíncrona com dispositivos via Telnet/TCP"""
import asyncio
import logging
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
