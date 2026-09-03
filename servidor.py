# -*- coding: utf-8 -*-
from flask import Flask, request, jsonify, send_file
import requests
from PIL import Image
import io
import time
import base64

app = Flask(__name__)

def pre_processar_descricao(descricao):
    descricao = descricao.strip().lower()
    sufixo = ", seamless texture, high quality, 4k, tileable"
    if "seamless" not in descricao:
        descricao = descricao + sufixo
    return descricao

def pos_processar_imagem(imagem_bytes, resolucao=1024):
    imagem = Image.open(io.BytesIO(imagem_bytes))
    imagem = imagem.resize((resolucao, resolucao), Image.LANCZOS)
    imagem = imagem.convert("RGB")
    buffer = io.BytesIO()
    imagem.save(buffer, format="PNG")
    buffer.seek(0)
    return buffer

@app.route('/gerar_textura', methods=['POST'])
def gerar_textura():
    try:
        t_total_inicio = time.time()

        dados = request.json
        descricao = dados.get('descricao', 'texture')
        resolucao = dados.get('resolucao', 1024)
        seed = dados.get('seed', 42)

        descricao_processada = pre_processar_descricao(descricao)
        print(f"\n--- Pedido: {descricao} ---")

        url = f"https://image.pollinations.ai/prompt/{requests.utils.quote(descricao_processada)}?width={resolucao}&height={resolucao}&seed={seed}"

        t_geracao_inicio = time.time()
        response = requests.get(url, timeout=120)
        t_geracao_fim = time.time()

        if response.status_code == 200:
            t_processo_inicio = time.time()
            imagem_buffer = pos_processar_imagem(response.content, resolucao)
            caminho = "textura_gerada.png"
            with open(caminho, "wb") as f:
                f.write(imagem_buffer.getvalue())
            imagem_buffer.seek(0)
            t_processo_fim = time.time()

            t_total_fim = time.time()

            t_geracao   = round(t_geracao_fim - t_geracao_inicio, 3)
            t_processo  = round(t_processo_fim - t_processo_inicio, 3)
            t_total     = round(t_total_fim - t_total_inicio, 3)

            print(f"[LATENCIA] Geracao IA:      {t_geracao}s")
            print(f"[LATENCIA] Pos-processamento: {t_processo}s")
            print(f"[LATENCIA] TOTAL:            {t_total}s")

            return send_file(imagem_buffer, mimetype='image/png')
        else:
            print(f"Erro na API Pollinations: {response.status_code} - {response.text}")
            return jsonify({"erro": response.text}), response.status_code

    except Exception as e:
        print(f"EXCECAO: {str(e)}")
        import traceback
        traceback.print_exc()
        return jsonify({"erro": str(e)}), 500


@app.route('/gerar_textura_forge', methods=['POST'])
def gerar_textura_forge():
    try:
        t_total_inicio = time.time()

        dados = request.json
        descricao = dados.get('descricao', 'texture')
        resolucao = dados.get('resolucao', 512)

        descricao_processada = pre_processar_descricao(descricao)
        print(f"\n--- Pedido Forge txt2img: {descricao} ---")

        payload = {
            "prompt": descricao_processada,
            "negative_prompt": "ugly, blurry, low quality, deformed, watermark",
            "steps": 20,
            "cfg_scale": 7,
            "width": resolucao,
            "height": resolucao,
            "sampler_name": "Euler a"
        }

        # ALTERADO: timeout=None para o Python esperar o Forge terminar
        response = requests.post(
            "http://127.0.0.1:7860/sdapi/v1/txt2img",
            json=payload,
            timeout=None
        )

        if response.status_code == 200:
            resultado = response.json()
            imagem_bytes = base64.b64decode(resultado['images'][0])

            t_processo_inicio = time.time()
            imagem_buffer = pos_processar_imagem(imagem_bytes, resolucao)
            caminho = "textura_gerada.png"
            with open(caminho, "wb") as f:
                f.write(imagem_buffer.getvalue())
            imagem_buffer.seek(0)
            t_processo_fim = time.time()

            t_total_fim = time.time()
            t_processo = round(t_processo_fim - t_processo_inicio, 3)
            t_total = round(t_total_fim - t_total_inicio, 3)

            print(f"[LATENCIA] Pos-processamento: {t_processo}s")
            print(f"[LATENCIA] TOTAL: {t_total}s")

            return send_file(imagem_buffer, mimetype='image/png')
        else:
            print(f"Erro Forge txt2img: {response.text}")
            return jsonify({"erro": response.text}), 500

    except Exception as e:
        print(f"EXCECAO: {str(e)}")
        import traceback
        traceback.print_exc()
        return jsonify({"erro": str(e)}), 500


@app.route('/transformar_cena', methods=['POST'])
def transformar_cena():
    try:
        t_inicio = time.time()

        dados = request.json
        imagem_base64 = dados.get('imagem')
        style = dados.get('estilo', 'ancient egyptian room')
        denoising = dados.get('denoising', 0.6)

        print(f"\n--- Transformar cena: {style} ---")

        payload = {
            "init_images": [imagem_base64],
            "prompt": style + ", highly detailed, high quality, 4k",
            "negative_prompt": "ugly, blurry, low quality, deformed, watermark",
            "denoising_strength": denoising,
            "steps": 20,
            "cfg_scale": 7,
            "width": 512,
            "height": 512,
            "sampler_name": "Euler a"
        }

        # ALTERADO: timeout=None tambem aqui para prevenir timeouts
        response = requests.post(
            "http://127.0.0.1:7860/sdapi/v1/img2img",
            json=payload,
            timeout=None
        )

        if response.status_code == 200:
            resultado = response.json()
            imagem_resultado = resultado['images'][0]

            t_total = round(time.time() - t_inicio, 3)
            print(f"[LATENCIA] Transformacao total: {t_total}s")

            return jsonify({"imagem": imagem_resultado, "tempo": t_total})
        else:
            print(f"Erro Forge: {response.text}")
            return jsonify({"erro": response.text}), 500

    except Exception as e:
        print(f"EXCECAO: {str(e)}")
        import traceback
        traceback.print_exc()
        return jsonify({"erro": str(e)}), 500

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000)