using Conde.Coloracao;
using Xunit;

namespace Conde.Coloracao.Testes;

/// <summary>
/// Os teoremas sao os OUTROS juizes, e e disso que este arquivo trata.
///
/// A forca bruta confere contra a definicao, e isso ja seria o bastante se a
/// definicao nao pudesse ser lida errada. Aqui cada afirmacao e conferida tambem
/// por um caminho que nao fala de coloracao nenhuma: uma busca em largura, uma
/// procura de ciclo impar, uma contagem de vizinhos.
/// </summary>
public class TeoremasTestes
{
    /// <summary>
    /// BIPARTIDO e o mesmo que caber em duas cores, em todo grafo pequeno daqui.
    ///
    /// Os dois lados da frase saem de procedimentos sem nada em comum: um e uma
    /// busca em largura que nunca volta atras, o outro e uma busca exaustiva sobre
    /// todas as atribuicoes de cor.
    /// </summary>
    [Fact]
    public void BipartidoEhOMesmoQueCaberEmDuasCores()
    {
        foreach (var (nome, grafo) in Grafos.Pequenos())
            Assert.Equal(Teoremas.EhBipartido(grafo), ForcaBruta.NumeroCromatico(grafo) <= 2);
    }

    /// <summary>
    /// E BIPARTIDO e o mesmo que NAO TER CICLO IMPAR, que e um fato sobre caminhos
    /// e nao sobre cores.
    /// </summary>
    [Fact]
    public void BipartidoEhOMesmoQueNaoTerCicloImpar()
    {
        foreach (var (nome, grafo) in Grafos.Pequenos().Concat(Grafos.Grandes()))
            Assert.Equal(Teoremas.EhBipartido(grafo), Teoremas.MenorCicloImpar(grafo) == 0);
    }

    [Fact]
    public void OCicloImparAchadoEhMesmoImparENaoPassaDoGrafo()
    {
        foreach (var (nome, grafo) in Grafos.Pequenos())
        {
            var tamanho = Teoremas.MenorCicloImpar(grafo);

            if (tamanho == 0) continue;

            Assert.True(tamanho % 2 == 1, $"{nome}: {tamanho}");
            Assert.InRange(tamanho, 3, grafo.Vertices);
        }
    }

    [Fact]
    public void OsLadosDeUmBipartidoNaoTemArestaDentro()
    {
        foreach (var (nome, grafo) in Grafos.Pequenos())
        {
            var lados = Teoremas.Lados(grafo);

            if (lados is null) continue;

            Assert.All(grafo.TodasAsArestas(), a => Assert.NotEqual(lados[a.A], lados[a.B]));
        }
    }

    /// <summary>
    /// A CLIQUE e um limite por baixo e o BROOKS um por cima, em todo grafo
    /// pequeno daqui.
    /// </summary>
    [Fact]
    public void OCromaticoFicaEntreACliqueEOBrooks()
    {
        foreach (var (nome, grafo) in Grafos.Pequenos())
        {
            var c = Teoremas.Conferir(grafo);

            Assert.True(c.EntreOsLimites, $"{nome}: clique {c.Clique}, cromatico {c.Cromatico}, Brooks {c.Brooks}");
        }
    }

    /// <summary>
    /// O TEOREMA DE BROOKS, conferido caso a caso: o maior grau MAIS UM so e
    /// preciso no grafo completo e no ciclo impar.
    ///
    /// Em todo o resto o proprio maior grau basta, e a tabela mostra isso em grafos
    /// que nao tem nada a ver uns com os outros.
    /// </summary>
    [Fact]
    public void OMaisUmDeBrooksSoEhPrecisoNoCompletoENoCicloImpar()
    {
        foreach (var (nome, grafo) in Grafos.Pequenos())
        {
            if (!Teoremas.EhConexo(grafo)) continue;

            var precisa = ForcaBruta.NumeroCromatico(grafo) == grafo.MaiorGrau() + 1;
            var eUmDosDois = Teoremas.EhCompleto(grafo) || Teoremas.EhCicloImpar(grafo);

            Assert.Equal(eUmDosDois, precisa);
        }
    }

    /// <summary>
    /// O ACHADO: a CLIQUE nao basta, e o contraexemplo tem onze vertices.
    ///
    /// A intuicao de todo mundo e que o que obriga a usar muitas cores e ter muita
    /// gente ligada entre si. No Grotzsch o maior grupo de vertices todos vizinhos
    /// entre si tem DOIS, ou seja uma aresta, e mesmo assim tres cores nao bastam.
    ///
    /// E da para subir isso quanto se quiser: a construcao de Mycielski acrescenta
    /// uma cor por vez sem criar triangulo nenhum.
    /// </summary>
    [Fact]
    public void ACliqueNaoBastaEOMycielskiMostraPorQue()
    {
        var grafo = Grafos.Ciclo(5);

        Assert.Equal(2, ForcaBruta.MaiorClique(grafo));
        Assert.Equal(3, ForcaBruta.NumeroCromatico(grafo));
        Assert.Equal(0, Medidas.Triangulos(grafo));

        var subiu = Grafos.Mycielski(grafo);

        Assert.Equal(2, ForcaBruta.MaiorClique(subiu));
        Assert.Equal(4, ForcaBruta.NumeroCromatico(subiu));
        Assert.Equal(0, Medidas.Triangulos(subiu));
    }

    /// <summary>
    /// E em quase todo grafo pequeno sorteado ela BASTA, que e justamente por que a
    /// intuicao e tao dificil de sacudir.
    /// </summary>
    [Fact]
    public void EmGrafoSorteadoPequenoACliqueQuaseSempreBasta()
    {
        var bastou = 0;
        var total = 0;

        for (ulong semente = 1; semente <= 30; semente++)
        {
            var grafo = Grafos.Sorteado(10, 0.4, semente);

            total++;
            if (ForcaBruta.MaiorClique(grafo) == ForcaBruta.NumeroCromatico(grafo)) bastou++;
        }

        Assert.True(bastou > total / 2, $"bastou em {bastou} de {total}");
        Assert.True(bastou < total, "se bastasse em todos, o Grotzsch seria impossivel");
    }

    [Fact]
    public void AConexaoEOCompletoRespondemOObvio()
    {
        Assert.True(Teoremas.EhConexo(Grafos.Ciclo(7)));
        Assert.True(Teoremas.EhConexo(Grafos.Completo(4)));
        Assert.False(Teoremas.EhConexo(new Grafo(3)));
        Assert.True(Teoremas.EhConexo(new Grafo(0)));

        Assert.True(Teoremas.EhCompleto(Grafos.Completo(6)));
        Assert.False(Teoremas.EhCompleto(Grafos.Ciclo(6)));

        Assert.True(Teoremas.EhCicloImpar(Grafos.Ciclo(7)));
        Assert.False(Teoremas.EhCicloImpar(Grafos.Ciclo(8)));
        Assert.False(Teoremas.EhCicloImpar(Grafos.Completo(5)));
    }

    [Fact]
    public void ADesconexaoNaoQuebraNenhumaDasConferencias()
    {
        // dois triangulos separados: nao e conexo, precisa de tres cores
        var grafo = new Grafo(6);

        foreach (var (a, b) in new[] { (0, 1), (1, 2), (2, 0), (3, 4), (4, 5), (5, 3) }) grafo.Ligar(a, b);

        Assert.False(Teoremas.EhConexo(grafo));
        Assert.False(Teoremas.EhBipartido(grafo));
        Assert.Equal(3, ForcaBruta.NumeroCromatico(grafo));
        Assert.Equal(3, ForcaBruta.MaiorClique(grafo));
        Assert.True(Teoremas.Conferir(grafo).EntreOsLimites);
    }
}
