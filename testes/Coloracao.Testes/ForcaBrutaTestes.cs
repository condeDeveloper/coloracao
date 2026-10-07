using Conde.Coloracao;
using Xunit;

namespace Conde.Coloracao.Testes;

/// <summary>
/// O juiz tambem precisa de juiz.
///
/// Todo veredicto deste repositorio e "a heuristica gastou tantas cores e o minimo
/// era tanto". Se a forca bruta tivesse um erro, as heuristicas seriam julgadas
/// contra uma referencia errada, e o erro mais provavel dela seria devolver um
/// numero PEQUENO demais, que e o erro que nao da nenhum sinal: a coloracao com k
/// cores nem sequer e devolvida junto.
///
/// Por isso os testes daqui conferem duas coisas separadas: que os numeros batem
/// com grafos de resposta conhecida de fora, e que a coloracao que a busca devolve
/// EXISTE e e valida.
/// </summary>
public class ForcaBrutaTestes
{
    [Theory]
    [InlineData(3, 3)]
    [InlineData(4, 2)]
    [InlineData(5, 3)]
    [InlineData(6, 2)]
    [InlineData(7, 3)]
    [InlineData(10, 2)]
    [InlineData(11, 3)]
    public void OCicloPrecisaDeDuasCoresSeEhParEDeTresSeEhImpar(int tamanho, int esperado) =>
        Assert.Equal(esperado, ForcaBruta.NumeroCromatico(Grafos.Ciclo(tamanho)));

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(5)]
    [InlineData(8)]
    public void OCompletoPrecisaDeUmaCorPorVertice(int tamanho) =>
        Assert.Equal(tamanho, ForcaBruta.NumeroCromatico(Grafos.Completo(tamanho)));

    [Fact]
    public void OPetersenPrecisaDeTresCores() =>
        Assert.Equal(3, ForcaBruta.NumeroCromatico(Grafos.Petersen()));

    /// <summary>
    /// O GROTZSCH precisa de QUATRO cores sem ter nenhum triangulo.
    ///
    /// E a frase mais importante do repositorio, e ela junta dois numeros que vem
    /// de procedimentos diferentes: a forca bruta sobre todas as atribuicoes diz
    /// quatro, e a forca bruta sobre todos os subconjuntos diz que o maior grupo de
    /// vertices todos vizinhos entre si tem DOIS.
    /// </summary>
    [Fact]
    public void OGrotzschPrecisaDeQuatroCoresSemTerTriangulo()
    {
        var grafo = Grafos.Grotzsch();

        Assert.Equal(4, ForcaBruta.NumeroCromatico(grafo));
        Assert.Equal(2, ForcaBruta.MaiorClique(grafo));
        Assert.Equal(0, Medidas.Triangulos(grafo));
        Assert.Null(ForcaBruta.ExisteComAteKCores(grafo, 3));
    }

    /// <summary>
    /// A coloracao devolvida pela busca EXISTE e e valida, e a com uma cor a menos
    /// nao existe.
    ///
    /// Sao as duas metades da afirmacao "o numero cromatico e k", e devolver so uma
    /// delas e o jeito classico de publicar um limite com cara de resposta.
    /// </summary>
    [Fact]
    public void ABuscaDevolveUmaColoracaoValidaERecusaUmaCorAMenos()
    {
        foreach (var (nome, grafo) in Grafos.Pequenos())
        {
            var k = ForcaBruta.NumeroCromatico(grafo);
            var cores = ForcaBruta.ExisteComAteKCores(grafo, k);

            Assert.NotNull(cores);
            Assert.True(Gulosa.EhValida(grafo, cores), nome);
            Assert.True(Gulosa.Quantas(cores) <= k, nome);

            if (k > 1) Assert.Null(ForcaBruta.ExisteComAteKCores(grafo, k - 1));
        }
    }

    /// <summary>
    /// Com MAIS cores do que o necessario a busca continua achando, e a poda das
    /// cores simetricas nao perde nenhuma resposta.
    /// </summary>
    [Fact]
    public void ComMaisCoresDoQuePrecisaElaContinuaAchando()
    {
        var grafo = Grafos.Petersen();

        for (var k = 3; k <= 10; k++)
        {
            var cores = ForcaBruta.ExisteComAteKCores(grafo, k);

            Assert.NotNull(cores);
            Assert.True(Gulosa.EhValida(grafo, cores));
        }
    }

    [Fact]
    public void OGrafoSemArestaPrecisaDeUmaCorSo()
    {
        Assert.Equal(1, ForcaBruta.NumeroCromatico(new Grafo(10)));
        Assert.Equal(0, ForcaBruta.NumeroCromatico(new Grafo(0)));
    }

    [Fact]
    public void AMaiorCliqueEhOQueSeEsperaNosCasosObvios()
    {
        Assert.Equal(7, ForcaBruta.MaiorClique(Grafos.Completo(7)));
        Assert.Equal(2, ForcaBruta.MaiorClique(Grafos.Ciclo(7)));
        Assert.Equal(3, ForcaBruta.MaiorClique(Grafos.Ciclo(3)));
        Assert.Equal(3, ForcaBruta.MaiorClique(Grafos.Roda(6)));
        Assert.Equal(1, ForcaBruta.MaiorClique(new Grafo(5)));
    }

    /// <summary>
    /// O TEOREMA DA ORDEM: para todo grafo existe uma ordem que faz a gulosa
    /// acertar o numero cromatico em cheio.
    ///
    /// A prova cabe numa linha: pinte da melhor maneira, liste primeiro todos os da
    /// cor um, depois os da cor dois, e a gulosa nessa ordem nunca precisa de uma
    /// cor nova fora de hora. Aqui isso e conferido com TODAS as ordens, uma a uma.
    /// </summary>
    [Fact]
    public void ParaTodoGrafoExisteUmaOrdemQueFazAGulosaAcertar()
    {
        foreach (var (nome, grafo) in Medidas.Pequenosinhos())
        {
            var (menor, maior) = ForcaBruta.MelhorEPiorOrdem(grafo);

            Assert.Equal(ForcaBruta.NumeroCromatico(grafo), menor);
            Assert.True(maior >= menor, nome);
        }
    }

    /// <summary>
    /// E a PIOR ordem da coroa gasta uma cor por par, o que a forca bruta sobre
    /// todas as ordens confirma sem depender da ordem que eu montei na mao.
    /// </summary>
    [Fact]
    public void APiorOrdemDaCoroaGastaUmaCorPorPar()
    {
        foreach (var lado in new[] { 3, 4 })
        {
            var (grafo, _) = Grafos.Coroa(lado);
            var (menor, maior) = ForcaBruta.MelhorEPiorOrdem(grafo);

            Assert.Equal(2, menor);
            Assert.Equal(lado, maior);
        }
    }

    [Fact]
    public void AsPermutacoesSaoTodasEDiferentes()
    {
        for (var n = 1; n <= 7; n++)
        {
            var todas = ForcaBruta.Permutacoes(n).Select(p => string.Join(",", p)).ToList();
            var fatorial = Enumerable.Range(1, n).Aggregate(1, (a, b) => a * b);

            Assert.Equal(fatorial, todas.Count);
            Assert.Equal(fatorial, todas.Distinct().Count());
        }
    }

    [Fact]
    public void AForcaBrutaRecusaGrafoGrandeDemaisEmVezDeDemorarPraSempre()
    {
        Assert.Throws<ArgumentException>(() => ForcaBruta.MaiorClique(new Grafo(30)));
        Assert.Throws<ArgumentException>(() => ForcaBruta.MelhorEPiorOrdem(new Grafo(12)));
    }
}
