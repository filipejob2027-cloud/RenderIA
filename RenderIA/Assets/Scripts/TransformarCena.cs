using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public class TransformarCena : MonoBehaviour
{
    public string servidorURL = "http://127.0.0.1:5000/transformar_cena";

    [Header("Configuracao")]
    public string estilo = "ancient egyptian room";
    [Range(0.1f, 1.0f)]
    public float denoising = 0.6f;
    public Texture2D imagemOriginal;
    public bool transformarAoIniciar = true;

    [Header("Estado")]
    public bool estaProcessando = false;

    void Start()
    {
        if (transformarAoIniciar && imagemOriginal != null)
            StartCoroutine(EnviarETransformar());
    }

    IEnumerator EnviarETransformar()
    {
        estaProcessando = true;
        Debug.Log("A transformar cena: " + estilo);

        // Copia os pixels da imagem original de forma segura
        Texture2D texCopia = new Texture2D(imagemOriginal.width, imagemOriginal.height, TextureFormat.RGBA32, false);
        texCopia.SetPixels(imagemOriginal.GetPixels());
        texCopia.Apply();

        // Converte para PNG e depois para Base64
        byte[] imagemBytes = texCopia.EncodeToPNG();
        string imagemBase64 = "data:image/png;base64," + Convert.ToBase64String(imagemBytes);

        // Monta o JSON formatando o float do denoising com ponto (padrão internacional)
        string jsonBody = "{\"imagem\": \"" + imagemBase64 + "\", \"estilo\": \"" + estilo + "\", \"denoising\": " + denoising.ToString("F1", System.Globalization.CultureInfo.InvariantCulture) + "}";
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);

        // Configura a requisição web
        UnityWebRequest request = new UnityWebRequest(servidorURL, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.timeout = 300;

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            ResultadoTransformacao resultado = JsonUtility.FromJson<ResultadoTransformacao>(json);

            byte[] imagemResultado = Convert.FromBase64String(resultado.imagem);
            Texture2D texturaFinal = new Texture2D(2, 2);
            texturaFinal.LoadImage(imagemResultado);

            // --- CORREÇÃO: Procura os Renderers no objeto Pai e em todos os seus filhos ---
            Renderer[] todosOsRenderers = GetComponentsInChildren<Renderer>();

            if (todosOsRenderers.Length > 0)
            {
                foreach (Renderer r in todosOsRenderers)
                {
                    r.material.mainTexture = texturaFinal;
                    Debug.Log("Nova textura transformada aplicada em: " + r.gameObject.name);
                }
                Debug.Log("Cena transformada aplicada com sucesso em " + resultado.tempo + "s");
            }
            else
            {
                Debug.LogError("Nenhum Renderer encontrado neste objeto ou nos seus filhos para aplicar a transformação!");
            }
            // --- FIM DA CORREÇÃO ---
        }
        else
        {
            Debug.LogError("Erro: " + request.error);
        }

        estaProcessando = false;
    }

    [Serializable]
    private class ResultadoTransformacao
    {
        public string imagem;
        public float tempo;
    }

    public IEnumerator IniciarTransformacao()
    {
        yield return StartCoroutine(EnviarETransformar());
    }
}