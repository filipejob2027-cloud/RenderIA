# 🎨 RenderIA — Generative AI Texture & Scene Transformer for Unity

O **RenderIA** é uma solução completa (*Client-Server*) desenvolvida para otimizar o fluxo de trabalho de artistas e desenvolvedores no Unity. A ferramenta permite gerar e transformar texturas (*seamless/tileable*) em tempo de execução (*runtime*) ou no editor, aplicando-as diretamente nos materiais dos objetos 3D através de *prompts* de texto e modelos de Inteligência Artificial.

---

## 🚀 Funcionalidades Principais

* **Geração de Texturas Seamless (`txt2img`):**
  * Pré-processamento automático do *prompt* para garantir texturas contínuas sem emendas visíveis (`seamless, tileable, 4k`).
  * Suporte para backend local **Stable Diffusion (WebUI Forge SDAPI)** e API remota de alta velocidade (**Pollinations.ai**).
  * Redimensionamento e otimização de imagem (*LANCZOS*) com resposta direta em PNG.

* **Transformação de Cenas & Estilos (`img2img`):**
  * Reestilização visual de objetos e ambientes em tempo real a partir de uma textura ou imagem base.
  * Controlo de variação (*denoising strength*) para preservar a estrutura geométrica enquanto aplica novos estilos (ex: estilo cartográfico antigo, linhas douradas, etc.).

* **Integração Nativa no Unity (C# Client):**
  * **Interface de Utilizador (UI):** Seleção dinâmica de objetos via *dropdown* e importação de imagens locais via `SimpleFileBrowser`.
  * **Processamento em Lote (Batching):** Varredura automática da hierarquia para aplicar texturas a múltiplos *renderers* e objetos filhos sequencialmente.
  * Mapeamento de textura otimizado com `TextureWrapMode.Repeat` e filtro `Bilinear`.

---

## 🛠️ Arquitetura & Tecnologias

### **Backend (API REST Server)**
* **Linguagem:** Python 3
* **Framework Web:** Flask (rotas `/gerar_textura`, `/gerar_textura_forge`, `/transformar_cena`)
* **Processamento de Imagem:** PIL (Pillow), Base64, io
* **Modelos de IA:** Stable Diffusion (WebUI Forge / SDAPI) & Pollinations.ai API

### **Frontend (Game Engine)**
* **Motor 3D:** Unity 2022+ / C#
* **Comunicação Web:** `UnityWebRequest` assíncrono (Corrotinas)
* **Interface:** TextMeshPro (TMP), Dropdowns, SimpleFileBrowser

---

## ⚙️ Como Funciona o Fluxo de Dados

1. **Entrada no Unity:** O utilizador insere um *prompt* (ex: `"fantasy world map"`) e seleciona o objeto 3D.
2. **Pedido HTTP Assíncrono:** O script C# envia um pedido `POST` JSON para o servidor local Python em segundo plano (*coroutine*), evitando bloquear a execução do jogo.
3. **Pipeline de IA & Flask:** O Flask enriquece o *prompt* com parâmetros técnicos e comunica com a API do Stable Diffusion (Forge `/sdapi/v1/txt2img` ou `/img2img`).
4. **Pós-Processamento:** O servidor Python formata, redimensiona e converte a imagem para PNG/Base64.
5. **Aplicação Dinâmica:** O Unity recebe os bytes, cria uma `Texture2D` e atribui-a automaticamente ao `material.mainTexture` de todos os *renderers* do modelo 3D.

---

## 📁 Estrutura do Repositório

```text
├── RenderIA/              # Código C# e scripts do Unity (AplicarTextura.cs, RenderIAUI.cs, etc.)
├── exemplos/              # Imagens e vídeos de demonstração das texturas e transformações
├── docs/                  # Documentação do projeto (Plano de Trabalho Final)
├── servidor.py            # Servidor Flask (Middleware REST para comunicação com a IA)
├── calcular_ssim.py       # Script de avaliação de qualidade de imagem
├── teste_api.py           # Script de testes de latência e integração da API
