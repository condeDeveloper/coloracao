namespace Conde.Coloracao;

/// <summary>
/// A COLORACAO GULOSA, que e tudo o que quase todo mundo usa.
///
/// Percorra os vertices numa ordem, e de a cada um a MENOR cor que nenhum vizinho
/// ja pintado esteja usando. Pronto. Nao existe volta atras, nao existe escolha,
/// nao existe nada: a unica coisa que esse algoritmo tem e a ORDEM.
///
/// E a ordem e tudo. Para todo grafo existe uma ordem que faz a gulosa acertar o
/// numero cromatico em cheio, e isso e um teorema: pinte o grafo da melhor
/// maneira, liste primeiro os da cor um, depois os da cor dois, e a gulosa nessa
/// ordem reproduz a resposta otima. Existe tambem, no mesmo grafo, ordem que faz
/// ela gastar metade dos vertices.
///
/// Ou seja: a gulosa nao e uma heuristica ruim nem uma heuristica boa. Ela e um
/// modo de transformar uma ordem numa coloracao, e o problema inteiro mudou de
/// lugar, nao sumiu.
/// </summary>
public static class Gulosa
{
    /// <param name="Cores">a cor de cada vertice, comecando em zero</param>
    /// <param name="Quantas">quantas cores diferentes foram usadas</param>
    /// <param name="Olhadas">quantas vezes um vizinho foi consultado</param>
    public record Resultado(int[] Cores, int Quantas, long Olhadas);

    /// <summary>
    /// Pinta na ordem dada. A ordem precisa ser uma permutacao de todos os
    /// vertices, porque pintar so uma parte nao e uma coloracao do grafo.
    /// </summary>
    public static Resultado Pintar(Grafo grafo, IReadOnlyList<int> ordem)
    {
        if (ordem.Count != grafo.Vertices) throw new ArgumentException("a ordem nao cobre o grafo", nameof(ordem));

        var cores = new int[grafo.Vertices];
        var olhadas = 0L;
        var proibida = new int[grafo.Vertices + 1];

        Array.Fill(cores, -1);
        Array.Fill(proibida, -1);

        foreach (var v in ordem)
        {
            foreach (var u in grafo.De(v))
            {
                olhadas++;

                if (cores[u] >= 0) proibida[cores[u]] = v;
            }

            var cor = 0;

            while (proibida[cor] == v) cor++;

            cores[v] = cor;
        }

        return new Resultado(cores, cores.Length == 0 ? 0 : cores.Max() + 1, olhadas);
    }

    /// <summary>A ordem natural: zero, um, dois. E a que ninguem escolhe e todo mundo usa.</summary>
    public static int[] OrdemNatural(Grafo grafo) => [.. Enumerable.Range(0, grafo.Vertices)];

    /// <summary>
    /// A ordem de WELSH e POWELL: do maior grau para o menor.
    ///
    /// A ideia e tratar primeiro os vertices dificeis, enquanto ainda ha cor
    /// sobrando. Ela tem um limite provado, e a tabela do medidor diz se ela ganha
    /// de verdade da ordem natural ou se so parece.
    /// </summary>
    public static int[] OrdemPorGrau(Grafo grafo) =>
        [.. Enumerable.Range(0, grafo.Vertices).OrderByDescending(grafo.Grau).ThenBy(v => v)];

    /// <summary>A ordem por grau crescente, que existe aqui so para ser o contraste.</summary>
    public static int[] OrdemPorGrauCrescente(Grafo grafo) =>
        [.. Enumerable.Range(0, grafo.Vertices).OrderBy(grafo.Grau).ThenBy(v => v)];

    public static int[] OrdemSorteada(Grafo grafo, ulong semente)
    {
        var ordem = OrdemNatural(grafo);

        new Sorteio(semente).Embaralhar(ordem);

        return ordem;
    }

    /// <summary>
    /// A MELHOR ordem sorteada entre varias tentativas.
    ///
    /// E o que se faz na pratica quando nao se quer pensar, e a tabela diz o quanto
    /// isso vale: em grafo sorteado, bastante; na coroa, nada, porque quase toda
    /// ordem da o resultado ruim.
    /// </summary>
    public static (int Quantas, ulong Semente) MelhorDeVarias(Grafo grafo, int tentativas)
    {
        var melhor = int.MaxValue;
        var qual = 0UL;

        for (var k = 1UL; k <= (ulong)tentativas; k++)
        {
            var quantas = Pintar(grafo, OrdemSorteada(grafo, k)).Quantas;

            if (quantas >= melhor) continue;

            melhor = quantas;
            qual = k;
        }

        return (melhor, qual);
    }

    /// <summary>
    /// O DSATUR, de Brelaz, 1979: a ordem decidida na hora, e nao antes.
    ///
    /// A cada passo, pinte o vertice com mais CORES DIFERENTES na vizinhanca, e
    /// desempate pelo maior grau. A diferenca para Welsh e Powell e que a escolha
    /// usa o que ja foi pintado, e e so isso.
    ///
    /// Essa frase sozinha e o bastante para o DSATUR acertar em cheio em familias
    /// inteiras de grafos: ele e exato em bipartido, em ciclo, em grafo de
    /// intervalo. Em grafo qualquer ele nao e exato, e nenhuma heuristica de tempo
    /// razoavel seria, porque o problema e dos dificeis.
    /// </summary>
    public static Resultado Dsatur(Grafo grafo)
    {
        var cores = new int[grafo.Vertices];
        var olhadas = 0L;
        var vizinhas = new HashSet<int>[grafo.Vertices];

        Array.Fill(cores, -1);

        for (var v = 0; v < grafo.Vertices; v++) vizinhas[v] = [];

        for (var passo = 0; passo < grafo.Vertices; passo++)
        {
            var escolhido = -1;

            for (var v = 0; v < grafo.Vertices; v++)
            {
                // a varredura CONTA. E o custo escondido do DSATUR: a cada passo
                // ele olha todo mundo que ainda nao foi pintado para escolher o
                // proximo, e isso nao aparece em lugar nenhum do enunciado dele
                olhadas++;

                if (cores[v] >= 0) continue;

                if (escolhido < 0
                    || vizinhas[v].Count > vizinhas[escolhido].Count
                    || (vizinhas[v].Count == vizinhas[escolhido].Count && grafo.Grau(v) > grafo.Grau(escolhido)))
                    escolhido = v;
            }

            var cor = 0;

            while (vizinhas[escolhido].Contains(cor)) cor++;

            cores[escolhido] = cor;

            foreach (var u in grafo.De(escolhido))
            {
                olhadas++;
                vizinhas[u].Add(cor);
            }
        }

        return new Resultado(cores, cores.Max() + 1, olhadas);
    }

    /// <summary>Uma coloracao e valida quando nenhuma aresta tem as duas pontas da mesma cor.</summary>
    public static bool EhValida(Grafo grafo, int[] cores)
    {
        if (cores.Length != grafo.Vertices) return false;
        if (cores.Any(c => c < 0)) return false;

        return grafo.TodasAsArestas().All(a => cores[a.A] != cores[a.B]);
    }

    /// <summary>As cores diferentes que uma coloracao usa.</summary>
    public static int Quantas(int[] cores) => cores.Distinct().Count();
}
