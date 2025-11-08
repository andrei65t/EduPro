import uvicorn
from fastapi import FastAPI, HTTPException, File, UploadFile
from pydantic import BaseModel
import os
import io
import json
import base64

# --- Importuri Specifice ---
import boto3
from dotenv import load_dotenv
# --- SFÂRȘIT Importuri ---


# --- Încărcăm variabilele de mediu din .env ---
load_dotenv()


# --- Inițializare FastAPI ---
app = FastAPI(
    title="Server OCR cu Claude",
    description="Un server simplu care primește o imagine și returnează textul extras."
)

# --- Configurare Client Bedrock (Claude) ---
BEDROCK_TOKEN = os.environ.get('AWS_BEARER_TOKEN_BEDROCK')
BEDROCK_REGION = "us-west-2" # Regiunea ta pentru hackathon
VISION_MODEL_ID = "anthropic.claude-3-5-sonnet-20240620-v1:0" # Modelul de viziune

bedrock_client = None

if not BEDROCK_TOKEN:
    print("❌ AVERTISMENT: Variabila de mediu 'AWS_BEARER_TOKEN_BEDROCK' nu este setată.")
    print("   Serverul OCR va fi INACTIV.")
else:
    try:
        print("Se inițializează clientul Bedrock (Claude)...")
        bedrock_client = boto3.client(
            service_name="bedrock-runtime",
            region_name=BEDROCK_REGION,
            aws_session_token=BEDROCK_TOKEN # Autentificare specială
        )
        print("✅ Clientul Bedrock (Claude) a fost inițializat cu succes.")
    except Exception as e:
        print(f"❌ EROARE la inițializarea clientului Bedrock: {e}")
        bedrock_client = None
# --- SFÂRȘIT Configurare Bedrock ---


def call_claude_vision_ocr(image_bytes: bytes, media_type: str) -> dict:
    """
    Trimite bytes de imagine (ORICARE TIP) către Claude Vision.
    Returnează un dicționar cu textul extras și rezumatul.
    """
    if bedrock_client is None:
        raise HTTPException(status_code=503, detail="Clientul Bedrock nu este inițializat.")

    img_b64 = base64.b64encode(image_bytes).decode("utf-8")

    prompt_text = (
        "Te rog să citești textul din această imagine (poate fi scris de mână sau tipărit). "
        "Răspunde în format JSON cu 2 câmpuri:\n"
        "1. 'text_extras': textul complet extras din imagine, exact așa cum apare\n"
        "2. 'summary': un rezumat concis de 2-3 propoziții care explică ideea principală\n\n"
        "Exemplu format răspuns:\n"
        '{"text_extras": "textul complet aici", "summary": "rezumatul aici"}\n\n'
        "Returnează DOAR JSON-ul, fără alt text."
    )

    body = {
        "anthropic_version": "bedrock-2023-05-31",
        "max_tokens": 4096,
        "messages": [
            {
                "role": "user",
                "content": [
                    {"type": "text", "text": prompt_text},
                    {
                        "type": "image",
                        "source": {
                            "type": "base64",
                            "media_type": media_type, 
                            "data": img_b64,
                        },
                    },
                ],
            }
        ],
    }

    print(f"[Bedrock] Se trimite cererea către Claude Vision (Tip: {media_type})...")
    response = bedrock_client.invoke_model(
        modelId=VISION_MODEL_ID,
        body=json.dumps(body),
        accept="application/json",
        contentType="application/json",
    )

    payload = json.loads(response["body"].read())
    text_blocks = [c.get("text", "") for c in payload.get("content", []) if c.get("type") == "text"]
    print("[Bedrock] Răspuns primit.")
    
    # Extragem răspunsul complet
    full_response = "\n".join(text_blocks).strip()
    
    # Încercăm să parsăm JSON-ul din răspuns
    try:
        # Claude poate returna JSON înconjurat de text, extragem doar JSON-ul
        json_start = full_response.find('{')
        json_end = full_response.rfind('}') + 1
        if json_start != -1 and json_end > json_start:
            json_str = full_response[json_start:json_end]
            result = json.loads(json_str)
            return {
                "text_extras": result.get("text_extras", full_response),
                "summary": result.get("summary", "")
            }
    except:
        pass
    
    # Dacă nu reușim să parsăm JSON, returnăm textul ca atare
    return {
        "text_extras": full_response,
        "summary": ""
    }

# --- SFÂRȘIT Funcții Procesare ---


# --- ==================== ENDPOINTS FastAPI ==================== ---

@app.get("/")
def read_root():
    return {"mesaj": "Serverul OCR rulează. Folosește endpoint-ul /ocr pentru a trimite o imagine."}


@app.post("/ocr")
async def extract_text_from_image(file: UploadFile = File(...)):
    """
    Primește o imagine și extrage textul folosind Claude,
    FĂRĂ nicio pre-procesare locală.
    """
    if bedrock_client is None:
        raise HTTPException(
            status_code=503, 
            detail="Serviciul Bedrock (Claude) nu este disponibil. Verifică token-ul AWS_BEARER_TOKEN_BEDROCK."
        )

    try:
        # 1. Citim bytes din fișierul urcat
        image_bytes = await file.read()
        
        # 2. Obținem tipul media original (ex: "image/jpeg")
        original_content_type = file.content_type
        
        if original_content_type not in ["image/jpeg", "image/png", "image/gif", "image/webp"]:
            raise HTTPException(
                status_code=400, 
                detail=f"Tip de fișier neacceptat: {original_content_type}. Trimite JPG, PNG, GIF, sau WebP."
            )

        # 3. Trimitem direct la Bedrock OCR
        print(f"Am primit fișierul: {file.filename} ({original_content_type})")
        result = call_claude_vision_ocr(image_bytes, original_content_type)
        
        return {
            "file_name": file.filename,
            "content_type": file.content_type,
            "text_extras": result.get("text_extras", "(Nu a fost recunoscut niciun text)"),
            "summary": result.get("summary", "")
        }
        
    except Exception as e:
        import traceback
        print(f"Server OCR: EROARE la /ocr: {e}")
        print(traceback.format_exc())
        raise HTTPException(status_code=500, detail=f"Eroare la procesarea OCR: {e}")

# --- Pornirea Serverului ---
if __name__ == "__main__":
    print("Se pornește Serverul OCR (FĂRĂ PRE-PROCESARE) pe http://0.0.0.0:8001")
    # Bind to 0.0.0.0 so other containers in the same pod/network can reach it
    uvicorn.run(app, host="0.0.0.0", port=8001)