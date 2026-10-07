namespace Conde.Coloracao;

/// <summary>
/// O JUIZ, e ele nao e outra heuristica.
///
/// Comparar duas heuristicas de coloracao diz quando elas discordam e nao diz
/// qual esta certa, e duas que concordam e erram juntas sao piores que uma que
/// erra sozinha. Aqui o juiz e a DEFINICAO: o numero cromatico e o menor k para o
/// qual existe uma atribuicao de k cores sem nenhuma aresta monocromatica, e o
/// jeito de saber que ele e k e tentar k-1 e nao conseguir.
///
/// Isso custa caro, e precisa custar. Colorir grafo e um dos problemas dificeis
/// de verdade, e um juiz rapido para ele seria um juiz errado. O preco e o juiz
/// so existir ate umas duas dezenas de vertices, e e la que todas as afirmacoes
/// exatas deste repositorio sao feitas.
/// </summary>
public static class ForcaBruta
{
    /// <summary>
    /// O NUMERO CROMATICO: o menor k que da certo.
    ///
    /// Subir k de um em um parece desperdicio e nao e: a busca com k-1 cores, que
    /// falha, e justamente a parte que PROVA que k e minimo. Devolver "achei uma
    /// coloracao com k cores" sem essa busca seria devolver um limite superior com
    /// cara de resposta.
    /// </summary>
    public static int NumeroCromatico(Grafo grafo)
    {
        if (grafo.Vertices == 0) return 0;
        if (grafo.Arestas == 0) return 1;

        for (var k = 2; k <= grafo.Vertices; k++)
            if (ExisteComAteKCores(grafo, k) is not null) return k;

        return grafo.Vertices;
    }

    /// <summary>
    /// Uma coloracao com no maximo k cores, ou null quando nao existe nenhuma.
    ///
    /// A busca e por tentativa e erro com uma poda que nao muda a resposta e muda
    /// tudo no custo: o vertice de indice i nunca precisa experimentar uma cor
    /// maior que a maior ja usada mais um. Trocar o nome de duas cores nao faz uma
    /// coloracao nova, e sem essa poda a busca refaz o mesmo trabalho k! vezes.
    ///
    /// A pilha e explicita, e nao por capricho: a profundidade e o numero de
    /// vertices, e um repositorio vizinho ja levou estouro de pilha no macOS da
    /// integracao continua com uma recursao tao funda quanto esta.
    /// </summary>
    public static int[]? ExisteComAteKCores(Grafo grafo, int k)
    {
        var n = grafo.Vertices;
        var cores = new int[n];
        var tentada = new int[n];

        Array.Fill(cores, -1);

        var v = 0;
        var maiorUsada = -1;

        while (v >= 0)
        {
            if (v == n) return cores;

            var cor = tentada[v]++;
            var teto = Math.Min(k - 1, maiorUsada + 1);

            if (cor > teto)
            {
                // sem cor nenhuma para este vertice: volta um passo
                tentada[v] = 0;
                cores[v] = -1;
                v--;

                if (v >= 0)
                {
                    cores[v] = -1;
                    maiorUsada = Maior(cores, v);
                }

                continue;
            }

            if (Conflita(grafo, cores, v, cor)) continue;

            cores[v] = cor;
            maiorUsada = Math.Max(maiorUsada, cor);
            v++;
        }

        return null;
    }

    private static bool Conflita(Grafo grafo, int[] cores, int v, int cor)
    {
        foreach (var u in grafo.De(v))
            if (u < v && cores[u] == cor) return true;

        return false;
    }

    private static int Maior(int[] cores, int ate)
    {
        var maior = -1;

        for (var i = 0; i < ate; i++) maior = Math.Max(maior, cores[i]);

        return maior;
    }

    /// <summary>
    /// A MAIOR CLIQUE: o maior grupo de vertices todos vizinhos entre si.
    ///
    /// Ela e um limite por baixo do numero cromatico, e de graca: se k vertices sao
    /// todos vizinhos entre si, eles precisam de k cores diferentes. A pergunta
    /// interessante nao e se o limite vale, e se ele BASTA, e a resposta esta na
    /// tabela do medidor.
    /// </summary>
    public static int MaiorClique(Grafo grafo)
    {
        if (grafo.Vertices > 20) throw new ArgumentException("forca bruta so ate 20 vertices", nameof(grafo));

        var melhor = 0;

        for (var mascara = 1; mascara < 1 << grafo.Vertices; mascara++)
        {
            var tamanho = System.Numerics.BitOperations.PopCount((uint)mascara);

            if (tamanho <= melhor) continue;

            var todos = true;

            for (var a = 0; a < grafo.Vertices && todos; a++)
            {
                if ((mascara & (1 << a)) == 0) continue;

                for (var b = a + 1; b < grafo.Vertices; b++)
                {
                    if ((mascara & (1 << b)) == 0) continue;
                    if (grafo.Vizinhos(a, b)) continue;

                    todos = false;
                    break;
                }
            }

            if (todos) melhor = tamanho;
        }

        return melhor;
    }

    /// <summary>
    /// O MENOR e o MAIOR numero de cores que a gulosa consegue, sobre TODAS as
    /// ordens possiveis.
    ///
    /// E a medida que diz o que a gulosa realmente e. O menor e sempre o numero
    /// cromatico, e isso e um teorema. O maior e o que ela faz com voce quando a
    /// ordem nao foi escolhida, e na coroa esses dois numeros sao dois e metade dos
    /// vertices.
    ///
    /// Sao n fatorial ordens, entao isso so existe ate uns oito vertices.
    /// </summary>
    public static (int Menor, int Maior) MelhorEPiorOrdem(Grafo grafo)
    {
        if (grafo.Vertices > 9) throw new ArgumentException("todas as ordens so ate 9 vertices", nameof(grafo));

        var menor = int.MaxValue;
        var maior = 0;

        foreach (var ordem in Permutacoes(grafo.Vertices))
        {
            var quantas = Gulosa.Pintar(grafo, ordem).Quantas;

            menor = Math.Min(menor, quantas);
            maior = Math.Max(maior, quantas);
        }

        return (menor, maior);
    }

    /// <summary>Todas as ordens de n elementos, pelo algoritmo de Heap, sem recursao.</summary>
    public static IEnumerable<int[]> Permutacoes(int n)
    {
        var atual = Enumerable.Range(0, n).ToArray();
        var contador = new int[n];

        yield return (int[])atual.Clone();

        var i = 0;

        while (i < n)
        {
            if (contador[i] >= i) { contador[i] = 0; i++; continue; }

            var j = i % 2 == 0 ? 0 : contador[i];

            (atual[i], atual[j]) = (atual[j], atual[i]);

            yield return (int[])atual.Clone();

            contador[i]++;
            i = 0;
        }
    }
}
