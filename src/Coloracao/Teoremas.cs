namespace Conde.Coloracao;

/// <summary>
/// OS TEOREMAS, que sao os outros juizes.
///
/// A forca bruta confere a resposta contra a definicao, e isso ja seria o
/// bastante. Mas a definicao e so uma maneira de olhar, e um resultado conferido
/// por uma maneira so de olhar continua tendo um ponto cego: o de o enunciado
/// estar certo e a leitura dele estar errada.
///
/// Entao aqui ficam as conferencias que nao falam de coloracao nenhuma:
///
/// - um grafo e bipartido quando da para dividir em dois lados, e isso sai de uma
///   busca em largura, sem tentar cor nenhuma;
/// - um grafo e bipartido quando nao tem ciclo IMPAR, e isso e um fato sobre
///   caminhos;
/// - toda coloracao precisa de pelo menos tantas cores quanto o maior grupo de
///   vertices todos vizinhos entre si;
/// - e nenhuma precisa de mais que o maior grau mais um, que e o limite de
///   Brooks, apertado para o proprio maior grau fora de dois casos.
///
/// Quando a forca bruta diz dois e a busca em largura diz bipartido e nao existe
/// ciclo impar, o que esta certo nao e a implementacao: e o resultado.
/// </summary>
public static class Teoremas
{
    /// <summary>
    /// BIPARTIDO por busca em largura: pinte a origem de um lado, os vizinhos do
    /// outro, e veja se alguma aresta fecha dentro do mesmo lado.
    ///
    /// Repare que isto e uma coloracao com duas cores, e ainda assim nao e o mesmo
    /// algoritmo: nao ha escolha, nao ha volta atras, nao ha tentativa. A cor de
    /// cada vertice e forcada pela distancia ate a origem.
    /// </summary>
    public static bool EhBipartido(Grafo grafo) => Lados(grafo) is not null;

    /// <summary>Os dois lados, ou null quando o grafo nao e bipartido.</summary>
    public static int[]? Lados(Grafo grafo)
    {
        var lado = new int[grafo.Vertices];

        Array.Fill(lado, -1);

        for (var inicio = 0; inicio < grafo.Vertices; inicio++)
        {
            if (lado[inicio] >= 0) continue;

            lado[inicio] = 0;

            var fila = new Queue<int>();

            fila.Enqueue(inicio);

            while (fila.Count > 0)
            {
                var v = fila.Dequeue();

                foreach (var u in grafo.De(v))
                {
                    if (lado[u] < 0)
                    {
                        lado[u] = 1 - lado[v];
                        fila.Enqueue(u);
                    }
                    else if (lado[u] == lado[v]) return null;
                }
            }
        }

        return lado;
    }

    /// <summary>
    /// O MENOR CICLO IMPAR, ou zero quando nao existe nenhum.
    ///
    /// Sai de uma busca em largura a partir de cada vertice: quando uma aresta liga
    /// dois vertices da MESMA distancia, os dois caminhos ate eles mais a aresta
    /// fecham um ciclo de comprimento impar.
    ///
    /// O numero que isto devolve e um limite por cima do menor ciclo impar de
    /// verdade, e e exato quando a busca comeca em cima dele, que e por que ela
    /// comeca de todos. Serve aqui para uma afirmacao so, que e a que importa:
    /// existe ou nao existe.
    /// </summary>
    public static int MenorCicloImpar(Grafo grafo)
    {
        var melhor = 0;

        for (var inicio = 0; inicio < grafo.Vertices; inicio++)
        {
            var distancia = new int[grafo.Vertices];

            Array.Fill(distancia, -1);
            distancia[inicio] = 0;

            var fila = new Queue<int>();

            fila.Enqueue(inicio);

            while (fila.Count > 0)
            {
                var v = fila.Dequeue();

                foreach (var u in grafo.De(v))
                {
                    if (distancia[u] < 0)
                    {
                        distancia[u] = distancia[v] + 1;
                        fila.Enqueue(u);

                        continue;
                    }

                    if (distancia[u] != distancia[v]) continue;

                    var tamanho = distancia[u] + distancia[v] + 1;

                    if (melhor == 0 || tamanho < melhor) melhor = tamanho;
                }
            }
        }

        return melhor;
    }

    public static bool EhConexo(Grafo grafo)
    {
        if (grafo.Vertices == 0) return true;

        var visto = new bool[grafo.Vertices];
        var pilha = new Stack<int>();
        var quantos = 1;

        visto[0] = true;
        pilha.Push(0);

        while (pilha.Count > 0)
        {
            foreach (var u in grafo.De(pilha.Pop()))
            {
                if (visto[u]) continue;

                visto[u] = true;
                quantos++;
                pilha.Push(u);
            }
        }

        return quantos == grafo.Vertices;
    }

    public static bool EhCompleto(Grafo grafo) =>
        grafo.Arestas == grafo.Vertices * (grafo.Vertices - 1) / 2;

    public static bool EhCicloImpar(Grafo grafo) =>
        grafo.Vertices % 2 == 1 && grafo.Vertices >= 3 && EhConexo(grafo)
        && Enumerable.Range(0, grafo.Vertices).All(v => grafo.Grau(v) == 2);

    /// <summary>
    /// O LIMITE DE BROOKS: o maior grau, salvo em dois casos.
    ///
    /// Todo grafo cabe no maior grau mais um, e isso sai da propria gulosa: na hora
    /// de pintar um vertice, ele tem no maximo tantos vizinhos quanto o maior grau,
    /// entao uma das primeiras cores sempre sobra.
    ///
    /// Brooks provou em 1941 que o "mais um" so e preciso em dois casos: o grafo
    /// completo e o ciclo impar. Em todo o resto, se for conexo, o proprio maior
    /// grau basta. E um teorema sobre o que a gulosa NAO ve.
    /// </summary>
    public static int LimiteDeBrooks(Grafo grafo)
    {
        var delta = grafo.MaiorGrau();

        if (!EhConexo(grafo)) return delta + 1;
        if (EhCompleto(grafo) || EhCicloImpar(grafo)) return delta + 1;

        return Math.Max(delta, 1);
    }

    /// <param name="Clique">o maior grupo de vertices todos vizinhos entre si</param>
    /// <param name="Cromatico">o numero cromatico, por forca bruta</param>
    /// <param name="Brooks">o limite de Brooks</param>
    /// <param name="Bipartido">o que a busca em largura diz</param>
    /// <param name="CicloImpar">o menor ciclo impar achado, ou zero</param>
    public record Conferencia(int Clique, int Cromatico, int Brooks, bool Bipartido, int CicloImpar)
    {
        /// <summary>A clique e um limite por baixo e o Brooks um por cima.</summary>
        public bool EntreOsLimites => Clique <= Cromatico && Cromatico <= Brooks;

        /// <summary>Bipartido e o mesmo que caber em duas cores.</summary>
        public bool BipartidoEhDuasCores => Bipartido == (Cromatico <= 2);

        /// <summary>E bipartido e o mesmo que nao ter ciclo impar.</summary>
        public bool BipartidoEhSemCicloImpar => Bipartido == (CicloImpar == 0);

        /// <summary>A clique basta? Quase sempre, e e o "quase" que interessa.</summary>
        public bool ACliqueBastou => Clique == Cromatico;
    }

    public static Conferencia Conferir(Grafo grafo) => new(
        ForcaBruta.MaiorClique(grafo),
        ForcaBruta.NumeroCromatico(grafo),
        LimiteDeBrooks(grafo),
        EhBipartido(grafo),
        MenorCicloImpar(grafo));
}
