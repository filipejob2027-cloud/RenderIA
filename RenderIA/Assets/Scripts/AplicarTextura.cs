using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class AplicarTextura : MonoBehaviour
{
    public string servidorURL = "http://127.0.0.1:5000/gerar_textura_forge";

    [Header("Configuracao da Textura")]
    public string descricao = "stone wall";
    public int resolucao = 512;
    public float atraso = 0f;

    [Header("Estado")]
    public bool gerarAoIniciar = false;
    public bool estaCarregando = false;

    [HideInInspector]
    public Texture2D ultimaTexturaGerada;

    void Start()
    {
        if (gerarAoIniciar)
            StartCoroutine(AguardarEGerar());
    }

    IEnumerator AguardarEGerar()
    {
        yield return new WaitForSeconds(atraso);
        StartCoroutine(GerarEAplicarTextura());
    }

    public void GerarTextura()
    {
        if (!estaCarregando)
            StartCoroutine(GerarEAplicarTextura());
        else
            Debug.LogWarning("Ja esta a gerar textura, aguarda...");
    }

    IEnumerator GerarEAplicarTextura()
    {
        estaCarregando = true;
        Debug.Log("A pedir textura: " + descricao);

        string jsonBody = JsonUtility.ToJson(new TexturaRequest
        {
            descricao = this.descricao,
            resolucao = this.resolucao
        });

        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        UnityWebRequest request = new UnityWebRequest(servidorURL, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.timeout = 600;

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            byte[] imageData = request.downloadHandler.data;
            Texture2D textura = new Texture2D(2, 2);

            if (textura.LoadImage(imageData))
            {
                textura.wrapMode = TextureWrapMode.Repeat;
                textura.filterMode = FilterMode.Bilinear;
                ultimaTexturaGerada = textura;

                // --- CONEXÃO AUTOMÁTICA ENTRE OS SCRIPTS ---
                // Procura o script de transformar neste mesmo objeto e passa-lhe a textura gerada
                TransformarCena scriptTransformar = GetComponent<TransformarCena>();
                if (scriptTransformar != null)
                {
                    scriptTransformar.imagemOriginal = textura;
                    Debug.Log("Sucesso: Textura base enviada para o script TransformarCena de: " + gameObject.name);
                }
                // --------------------------------------------

                Renderer[] todosOsRenderers = GetComponentsInChildren<Renderer>();

                if (todosOsRenderers.Length > 0)
                {
                    foreach (Renderer rend in todosOsRenderers)
                    {
                        rend.material.mainTexture = textura;
                        Debug.Log("Textura aplicada em: " + rend.gameObject.name);
                    }
                    Debug.Log("Textura aplicada com sucesso em todas as partes de: " + gameObject.name);
                }
                else
                {
                    Debug.LogError("Nenhum Renderer encontrado neste objeto ou nos seus filhos!");
                }
            }
            else
            {
                Debug.LogError("Falha ao carregar imagem PNG recebida.");
            }
        }
        else
        {
            Debug.LogError("Erro HTTP: " + request.error + " | Codigo: " + request.responseCode);
        }

        estaCarregando = false;
    }

    [System.Serializable]
    private class TexturaRequest
    {
        public string descricao;
        public int resolucao;
    }
}