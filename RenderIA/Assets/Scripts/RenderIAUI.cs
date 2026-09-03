using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.IO;
using SimpleFileBrowser;
public class RenderIAUI : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField campoDescricao;
    public Button botaoGerar;
    public Button botaoTransformar;
    public Button botaoEscolherImagem;
    public Button botaoGerarAmbiente;
    public Button botaoTransformarAmbiente;
    public TMP_Dropdown dropdownObjeto;
    [Header("Objetos 3D")]
    public GameObject[] objetos;
    private Texture2D imagemCarregada;
    void Start()
    {
        dropdownObjeto.ClearOptions();
        System.Collections.Generic.List<string> nomes = new System.Collections.Generic.List<string>();
        foreach (GameObject obj in objetos)
            nomes.Add(obj.name);
        dropdownObjeto.AddOptions(nomes);
        botaoGerar.onClick.AddListener(GerarTextura);
        botaoTransformar.onClick.AddListener(TransformarCena);
        botaoEscolherImagem.onClick.AddListener(EscolherImagem);
        botaoGerarAmbiente.onClick.AddListener(GerarAmbiente);
        botaoTransformarAmbiente.onClick.AddListener(TransformarAmbiente);
    }
    void EscolherImagem()
    {
        FileBrowser.SetFilters(true, new FileBrowser.Filter("Imagens", ".png", ".jpg", ".jpeg"));
        FileBrowser.SetDefaultFilter(".png");
        StartCoroutine(MostrarFileBrowser());
    }
    IEnumerator MostrarFileBrowser()
    {
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Files, false, null, null, "Escolher Imagem", "Carregar");
        if (FileBrowser.Success)
        {
            string caminho = FileBrowser.Result[0];
            byte[] bytes = File.ReadAllBytes(caminho);
            imagemCarregada = new Texture2D(2, 2);
            imagemCarregada.LoadImage(bytes);
            Debug.Log("Imagem carregada: " + caminho);
        }
    }
    void GerarTextura()
    {
        string descricao = campoDescricao.text;
        if (string.IsNullOrEmpty(descricao))
        {
            Debug.LogWarning("Escreve uma descricao primeiro!");
            return;
        }
        GameObject obj = GetObjetoSelecionado();
        if (obj != null)
        {
            AplicarTextura script = obj.GetComponent<AplicarTextura>();
            if (script != null)
            {
                script.descricao = descricao;
                script.GerarTextura();
                Debug.Log("A gerar textura para: " + obj.name);
            }
        }
    }
    void GerarAmbiente()
    {
        string descricao = campoDescricao.text;
        if (string.IsNullOrEmpty(descricao))
        {
            Debug.LogWarning("Escreve uma descricao primeiro!");
            return;
        }
        StartCoroutine(GerarTodosObjetos(descricao));
    }
    IEnumerator GerarTodosObjetos(string descricao)
    {
        Debug.Log("A gerar ambiente completo: " + descricao);
        foreach (GameObject obj in objetos)
        {
            if (obj == null) continue;
            AplicarTextura script = obj.GetComponent<AplicarTextura>();
            if (script != null && !script.estaCarregando)
            {
                script.descricao = descricao;
                script.GerarTextura();
                Debug.Log("A gerar textura para: " + obj.name);
                yield return new WaitUntil(() => !script.estaCarregando);
                yield return new WaitForSeconds(1f);
            }
        }
        Debug.Log("Ambiente gerado com sucesso!");
    }
    void TransformarAmbiente()
    {
        string estilo = campoDescricao.text;
        if (string.IsNullOrEmpty(estilo))
        {
            Debug.LogWarning("Escreve um estilo primeiro!");
            return;
        }
        StartCoroutine(TransformarTodosObjetos(estilo));
    }
    IEnumerator TransformarTodosObjetos(string estilo)
    {
        Debug.Log("A transformar ambiente completo: " + estilo);
        foreach (GameObject obj in objetos)
        {
            if (obj == null) continue;
            TransformarCena script = obj.GetComponent<TransformarCena>();
            if (script != null && !script.estaProcessando)
            {
                script.estilo = estilo;
                script.transformarAoIniciar = false;

                // Tenta obter imagem de entrada
                if (imagemCarregada != null)
                {
                    script.imagemOriginal = imagemCarregada;
                }
                else
                {
                    AplicarTextura aplicar = obj.GetComponent<AplicarTextura>();
                    if (aplicar != null && aplicar.ultimaTexturaGerada != null)
                    {
                        script.imagemOriginal = aplicar.ultimaTexturaGerada;
                    }
                    else
                    {
                        Renderer rend = obj.GetComponent<Renderer>();
                        if (rend != null)
                        {
                            Texture texOriginal = null;
                            if (rend.material.HasProperty("_BaseMap"))
                                texOriginal = rend.material.GetTexture("_BaseMap");
                            else if (rend.material.mainTexture != null)
                                texOriginal = rend.material.mainTexture;
                            Texture2D texAtual = texOriginal as Texture2D;
                            if (texAtual != null)
                                script.imagemOriginal = texAtual;
                        }
                    }
                }

                if (script.imagemOriginal == null)
                {
                    Debug.LogWarning("Sem imagem para: " + obj.name + " — a saltar.");
                    continue;
                }

                StartCoroutine(script.IniciarTransformacao());
                Debug.Log("A transformar: " + obj.name);
                yield return new WaitUntil(() => !script.estaProcessando);
                yield return new WaitForSeconds(1f);
            }
        }
        Debug.Log("Ambiente transformado com sucesso!");
    }
    void TransformarCena()
    {
        string estilo = campoDescricao.text;
        if (string.IsNullOrEmpty(estilo))
        {
            Debug.LogWarning("Escreve um estilo primeiro!");
            return;
        }
        GameObject obj = GetObjetoSelecionado();
        if (obj != null)
        {
            TransformarCena script = obj.GetComponent<TransformarCena>();
            if (script != null)
            {
                script.estilo = estilo;
                script.transformarAoIniciar = false;
                if (imagemCarregada != null)
                {
                    script.imagemOriginal = imagemCarregada;
                }
                else
                {
                    AplicarTextura aplicar = obj.GetComponent<AplicarTextura>();
                    if (aplicar != null && aplicar.ultimaTexturaGerada != null)
                    {
                        script.imagemOriginal = aplicar.ultimaTexturaGerada;
                    }
                    else
                    {
                        Renderer rend = obj.GetComponent<Renderer>();
                        if (rend != null)
                        {
                            Texture texOriginal = null;
                            if (rend.material.HasProperty("_BaseMap"))
                                texOriginal = rend.material.GetTexture("_BaseMap");
                            else if (rend.material.mainTexture != null)
                                texOriginal = rend.material.mainTexture;
                            Texture2D texAtual = texOriginal as Texture2D;
                            if (texAtual != null)
                                script.imagemOriginal = texAtual;
                        }
                    }
                }
                if (script.imagemOriginal == null)
                {
                    Debug.LogError("Nenhuma imagem disponivel para transformar! Carrega uma imagem ou gera uma textura primeiro.");
                    return;
                }
                StartCoroutine(script.IniciarTransformacao());
                Debug.Log("A transformar cena: " + obj.name);
            }
        }
    }
    GameObject GetObjetoSelecionado()
    {
        int i = dropdownObjeto.value;
        if (i >= 0 && i < objetos.Length)
            return objetos[i];
        return null;
    }
}