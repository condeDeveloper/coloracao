namespace Conde.Coloracao;

/// <summary>
/// As TABELAS que o medidor imprime e que os testes conferem.
///
/// Elas ficam aqui, e nao dentro do programa do medidor, por um motivo so: numero
/// em README que ninguem confere envelhece errado em silencio. Alguem troca uma
/// constante, a tabela muda, e o texto continua afirmando a frase antiga. Aqui o
/// teste afirma sobre o MESMO objeto que vira linha impressa.
/// </summary>
public static class Medidas
{
    public record LinhaDefinicao(
        string Grafo, int Vertices, int Arestas, int MaiorGrau,
        int Clique, int Cromatico, int Brooks, bool Bipartido, int CicloImpar,
        bool EntreOsLimites, bool BipartidoBate, bool CicloBate, bool CliqueBastou);

    /// <summary>
    /// Os grafos pequenos contra a DEFINICAO e contra os teoremas.
    /// </summary>
    public static List<LinhaDefinicao> ContraADefinicao()
    {
        var linhas = new List<LinhaDefinicao>();

        foreach (var (nome, grafo) in Grafos.Pequenos())
        {
            var c = Teoremas.Conferir(grafo);

            linhas.Add(new LinhaDefinicao(
                nome, grafo.Vertices, grafo.Arestas, grafo.MaiorGrau(),
                c.Clique, c.Cromatico, c.Brooks, c.Bipartido, c.CicloImpar,
                c.EntreOsLimites, c.BipartidoEhDuasCores, c.BipartidoEhSemCicloImpar, c.ACliqueBastou));
        }

        return linhas;
    }

    public record LinhaCoroa(
        int Lado, int Vertices, int Cromatico,
        int Natural, int OrdemRuim, int PorGrau, int PorGrauCrescente, int Dsatur, int MelhorDe50);

    /// <summary>
    /// A COROA com o lado crescendo: o grafo de DUAS cores em que a gulosa gasta
    /// metade dos vertices.
    ///
    /// E a tabela mais importante do repositorio, porque ela nao mede uma
    /// heuristica contra outra: ela mede a MESMA heuristica contra ela mesma, com a
    /// ordem trocada.
    /// </summary>
    public static List<LinhaCoroa> NaCoroa(params int[] lados)
    {
        if (lados.Length == 0) lados = [2, 3, 5, 10, 25, 50];

        var linhas = new List<LinhaCoroa>();

        foreach (var lado in lados)
        {
            var (grafo, ruim) = Grafos.Coroa(lado);

            linhas.Add(new LinhaCoroa(
                lado, grafo.Vertices,
                // a coroa e bipartida por construcao, entao duas cores, e a forca
                // bruta so confirma isso nos tamanhos em que ela ainda cabe
                grafo.Arestas == 0 ? 1 : 2,
                Gulosa.Pintar(grafo, Gulosa.OrdemNatural(grafo)).Quantas,
                Gulosa.Pintar(grafo, ruim).Quantas,
                Gulosa.Pintar(grafo, Gulosa.OrdemPorGrau(grafo)).Quantas,
                Gulosa.Pintar(grafo, Gulosa.OrdemPorGrauCrescente(grafo)).Quantas,
                Gulosa.Dsatur(grafo).Quantas,
                Gulosa.MelhorDeVarias(grafo, 50).Quantas));
        }

        return linhas;
    }

    public record LinhaOrdens(string Grafo, int Vertices, int Cromatico, int Menor, int Maior, int Natural);

    /// <summary>
    /// TODAS as ordens possiveis, nos grafos pequenos o bastante para isso.
    ///
    /// A coluna "menor" e o teorema: existe sempre uma ordem que faz a gulosa
    /// acertar em cheio. A coluna "maior" e o aviso.
    /// </summary>
    public static List<LinhaOrdens> TodasAsOrdens()
    {
        var linhas = new List<LinhaOrdens>();

        foreach (var (nome, grafo) in Pequenosinhos())
        {
            var (menor, maior) = ForcaBruta.MelhorEPiorOrdem(grafo);

            linhas.Add(new LinhaOrdens(
                nome, grafo.Vertices, ForcaBruta.NumeroCromatico(grafo), menor, maior,
                Gulosa.Pintar(grafo, Gulosa.OrdemNatural(grafo)).Quantas));
        }

        return linhas;
    }

    /// <summary>Os grafos com ate nove vertices, onde todas as ordens cabem.</summary>
    public static IEnumerable<(string Nome, Grafo Grafo)> Pequenosinhos()
    {
        yield return ("coroa 3x3", Grafos.Coroa(3).Grafo);
        yield return ("coroa 4x4", Grafos.Coroa(4).Grafo);
        yield return ("ciclo de 6", Grafos.Ciclo(6));
        yield return ("ciclo de 7", Grafos.Ciclo(7));
        yield return ("roda de 6", Grafos.Roda(6));
        yield return ("completo de 5", Grafos.Completo(5));
        yield return ("grade 2x4", Grafos.Grade(2, 4));
        yield return ("bipartido 4x4", Grafos.Bipartido(4, 4, 0.5, 29));

        for (ulong semente = 1; semente <= 4; semente++)
            yield return ($"sorteado {semente}", Grafos.Sorteado(8, 0.4, semente));
    }

    public record LinhaHeuristica(
        string Grafo, int Vertices, int Arestas, int MaiorGrau, int Brooks,
        int Natural, int PorGrau, int Dsatur, int MelhorDe50, long OlhadasNatural, long OlhadasDsatur);

    /// <summary>
    /// As heuristicas nos grafos grandes, onde a forca bruta nao chega.
    ///
    /// Sem numero cromatico nesta tabela, de proposito: colocar aqui um numero que
    /// nao foi conferido contra a definicao seria exatamente o tipo de numero que
    /// este repositorio existe para nao publicar. O que da para afirmar e o limite
    /// de Brooks, que e um teorema, e a comparacao entre as colunas.
    /// </summary>
    public static List<LinhaHeuristica> Heuristicas()
    {
        var linhas = new List<LinhaHeuristica>();

        foreach (var (nome, grafo) in Grafos.Grandes())
        {
            var natural = Gulosa.Pintar(grafo, Gulosa.OrdemNatural(grafo));
            var dsatur = Gulosa.Dsatur(grafo);

            linhas.Add(new LinhaHeuristica(
                nome, grafo.Vertices, grafo.Arestas, grafo.MaiorGrau(), Teoremas.LimiteDeBrooks(grafo),
                natural.Quantas,
                Gulosa.Pintar(grafo, Gulosa.OrdemPorGrau(grafo)).Quantas,
                dsatur.Quantas,
                Gulosa.MelhorDeVarias(grafo, 50).Quantas,
                natural.Olhadas, dsatur.Olhadas));
        }

        return linhas;
    }

    public record LinhaLimite(string Grafo, int MaiorGrau, int Clique, int Cromatico, int Brooks, bool BrooksApertado);

    /// <summary>
    /// Onde os dois limites APERTAM e onde eles sao folga pura.
    ///
    /// A clique e um limite por baixo, o Brooks um por cima, e a distancia entre os
    /// dois e o espaco onde o problema de verdade vive. No grafo de Grotzsch essa
    /// distancia e de dois para cinco, com a resposta em quatro.
    /// </summary>
    public static List<LinhaLimite> Limites()
    {
        var linhas = new List<LinhaLimite>();

        foreach (var (nome, grafo) in Grafos.Pequenos())
        {
            var c = Teoremas.Conferir(grafo);

            linhas.Add(new LinhaLimite(nome, grafo.MaiorGrau(), c.Clique, c.Cromatico, c.Brooks, c.Brooks == c.Cromatico));
        }

        return linhas;
    }

    public record LinhaMycielski(int Passo, int Vertices, int Arestas, int Clique, int Cromatico, int Triangulos);

    /// <summary>
    /// A ESCADA DE MYCIELSKI: o numero cromatico sobe um a cada passo e o maior
    /// grupo de vertices todos vizinhos entre si CONTINUA DOIS.
    ///
    /// E a demonstracao, em tabela, de que precisar de muitas cores nao tem nada a
    /// ver com ter muita gente ligada entre si. Comeca no ciclo de cinco, que
    /// precisa de tres cores e nao tem triangulo nenhum, e cada passo acrescenta
    /// uma cor sem acrescentar um triangulo.
    /// </summary>
    public static List<LinhaMycielski> EscadaDeMycielski(int passos = 1)
    {
        var linhas = new List<LinhaMycielski>();
        var grafo = Grafos.Ciclo(5);

        for (var passo = 0; passo <= passos; passo++)
        {
            linhas.Add(new LinhaMycielski(
                passo, grafo.Vertices, grafo.Arestas,
                ForcaBruta.MaiorClique(grafo), ForcaBruta.NumeroCromatico(grafo), Triangulos(grafo)));

            if (passo < passos) grafo = Grafos.Mycielski(grafo);
        }

        return linhas;
    }

    /// <summary>Quantos triangulos o grafo tem, pela definicao.</summary>
    public static int Triangulos(Grafo grafo)
    {
        var quantos = 0;

        for (var a = 0; a < grafo.Vertices; a++)
            for (var b = a + 1; b < grafo.Vertices; b++)
            {
                if (!grafo.Vizinhos(a, b)) continue;

                for (var c = b + 1; c < grafo.Vertices; c++)
                    if (grafo.Vizinhos(a, c) && grafo.Vizinhos(b, c)) quantos++;
            }

        return quantos;
    }
}
