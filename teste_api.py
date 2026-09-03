import requests

TOKEN = "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"

headers = {"Authorization": f"Bearer {TOKEN}"}

response = requests.post(
    "https://router.huggingface.co/hf-inference/models/stabilityai/stable-diffusion-xl-base-1.0",
    headers=headers,
    json={"inputs": "stone wall texture, seamless, high quality"},
    timeout=120
)

print(f"Status: {response.status_code}")

if response.status_code == 200:
    with open("textura_teste.png", "wb") as f:
        f.write(response.content)
    print("Textura gerada com sucesso! Ficheiro: textura_teste.png")
else:
    print(f"Erro: {response.text}")