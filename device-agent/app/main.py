"""Aplicação FastAPI principal do Device Agent"""
import logging
from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from app.api.routes import router

# Configurar logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s'
)
logger = logging.getLogger(__name__)

# Criar aplicação FastAPI
app = FastAPI(
    title="CIoTD Device Agent",
    description="Serviço de comunicação assíncrona com dispositivos IoT via Telnet/TCP",
    version="1.0.0",
    docs_url="/docs",
    redoc_url="/redoc",
    openapi_url="/openapi.json"
)

# Configurar CORS
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Incluir rotas
app.include_router(router)


@app.on_event("startup")
async def startup_event():
    """Evento de inicialização"""
    logger.info("Device Agent iniciando...")
    logger.info("API disponível em http://localhost:8000")
    logger.info("Documentação em http://localhost:8000/docs")


@app.on_event("shutdown")
async def shutdown_event():
    """Evento de shutdown"""
    logger.info("Device Agent encerrando...")


if __name__ == "__main__":
    import uvicorn

    uvicorn.run(
        "app.main:app",
        host="0.0.0.0",
        port=8000,
        reload=True,
        log_level="info"
    )
