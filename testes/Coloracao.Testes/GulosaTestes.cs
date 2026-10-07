using Conde.Coloracao;
using Xunit;

namespace Conde.Coloracao.Testes;

public class GulosaTestes
{
    /// <summary>
    /// A gulosa SEMPRE devolve uma coloracao valida, em qualquer ordem.
    ///
    /// Isso nao e uma formalidade: e a unica coisa que ela promete. Ela nao promete
    /// poucas cores, nao promete o otimo, nao promete nada alem de que nenhuma
    /// aresta vai ficar com as duas pontas da mesma cor.
    /// </summary>
    [Fact]
    public void AGulosaSempreDevolveUmaColoracaoValida()
    {
        foreach (var (nome, grafo) in Grafos.Pequenos())
            for (var semente = 1UL; semente <= 20; semente++)
            {
                var r = Gulosa.Pintar(grafo, Gulosa.OrdemSorteada(grafo, semente));

                Assert.True(Gulosa.EhValida(grafo, r.Cores), $"{nome}, semente {semente}");
                Assert.Equal(r.Quantas, Gulosa.Quantas(r.Cores));
            }
    }

    [Fact]
    public void ODsaturTambemSempreDevolveUmaColoracaoValida()
    {
        foreach (var (nome, grafo) in Grafos.Pequenos().Concat(Grafos.Grandes()))
        {
            var r = Gulosa.Dsatur(grafo);

            Assert.True(Gulosa.EhValida(grafo, r.Cores), nome);
            Assert.Equal(r.Quantas, Gulosa.Quantas(r.Cores));
        }
    }

    [Fact]
    public void UmaOrdemQueNaoCobreOGrafoEhRecusada() =>
        Assert.Throws<ArgumentException>(() => Gulosa.Pintar(Grafos.Ciclo(5), [0, 1, 2]));

    /// <summary>
    /// O ACHADO PRINCIPAL: na coroa, a gulosa gasta METADE DOS VERTICES num grafo
    /// que precisa de duas cores.
    ///
    /// O grafo e bipartido. Duas cores bastam, e a busca em largura acha essas duas
    /// cores sem nenhuma tentativa. A mesma gulosa, no mesmo grafo, com a ordem dos
    /// pares intercalados, gasta uma cor por par.
    /// </summary>
    [Fact]
    public void NaCoroaAGulosaGastaMetadeDosVertices()
    {
        foreach (var lado in new[] { 3, 5, 10, 25, 50 })
        {
            var (grafo, ruim) = Grafos.Coroa(lado);

            Assert.True(Teoremas.EhBipartido(grafo));
            Assert.Equal(lado, Gulosa.Pintar(grafo, ruim).Quantas);
        }
    }

    /// <summary>
    /// E na MESMA coroa, com a ordem natural, ela gasta duas.
    ///
    /// Nao e que a gulosa seja ruim na coroa: e que ela nao e nada sem a ordem.
    /// </summary>
    [Fact]
    public void NaMesmaCoroaComAOrdemNaturalElaGastaDuas()
    {
        foreach (var lado in new[] { 3, 5, 10, 25, 50 })
        {
            var (grafo, _) = Grafos.Coroa(lado);

            Assert.Equal(2, Gulosa.Pintar(grafo, Gulosa.OrdemNatural(grafo)).Quantas);
        }
    }

    /// <summary>
    /// O ACHADO QUE ME CORRIGIU: ordenar por GRAU, que e o conselho que todo mundo
    /// da, e a PIOR das tres num grafo bipartido sorteado.
    ///
    /// Eu tinha escrito o teste afirmando que a ordem por grau nunca gasta mais que
    /// a ordem natural, porque e isso que a ideia de "trate primeiro os dificeis"
    /// sugere. Num bipartido de cento e cinquenta por cento e cinquenta ela gasta
    /// DEZ cores e a ordem natural gasta DUAS.
    ///
    /// O motivo, depois de olhar: num bipartido sorteado os graus dos dois lados se
    /// misturam, entao ordenar por grau INTERCALA os lados, que e exatamente a
    /// ordem ruim da coroa. A ordem natural, por acaso, lista um lado inteiro antes
    /// do outro.
    ///
    /// E "por acaso" e a palavra certa: a ordem natural nao sabe de nada. Ela so
    /// nao atrapalhou.
    /// </summary>
    [Fact]
    public void AOrdemPorGrauEhAPiorNumBipartidoSorteado()
    {
        var grafo = Grafos.Bipartido(150, 150, 0.2, 17);

        var natural = Gulosa.Pintar(grafo, Gulosa.OrdemNatural(grafo)).Quantas;
        var porGrau = Gulosa.Pintar(grafo, Gulosa.OrdemPorGrau(grafo)).Quantas;
        var dsatur = Gulosa.Dsatur(grafo).Quantas;

        Assert.Equal(2, natural);
        Assert.Equal(2, dsatur);
        Assert.True(porGrau > 5, $"a ordem por grau gastou {porGrau}");
    }

    /// <summary>
    /// O DSATUR acerta em cheio em todo grafo BIPARTIDO, e isso nao e sorte: ele e
    /// exato nessa familia inteira.
    /// </summary>
    [Fact]
    public void ODsaturEhExatoEmBipartido()
    {
        foreach (var (nome, grafo) in new[]
        {
            ("coroa 25", Grafos.Coroa(25).Grafo),
            ("grade 20x20", Grafos.Grade(20, 20)),
            ("bipartido 60x60", Grafos.Bipartido(60, 60, 0.3, 19)),
            ("bipartido 150x150", Grafos.Bipartido(150, 150, 0.2, 17)),
        })
        {
            Assert.True(Teoremas.EhBipartido(grafo), nome);
            Assert.Equal(2, Gulosa.Dsatur(grafo).Quantas);
        }
    }

    /// <summary>
    /// O ACHADO QUE ME CORRIGIU DE NOVO: o DSATUR ERRA, e o contraexemplo tem onze
    /// vertices.
    ///
    /// Eu tinha escrito este teste afirmando que ele acerta em todo grafo pequeno
    /// daqui, porque ele acerta em quase todos e porque e exato em familias
    /// inteiras. Um grafo sorteado de onze vertices e dezessete arestas gasta
    /// QUATRO cores com ele e tres com a forca bruta.
    ///
    /// E era de se esperar: colorir grafo e um dos problemas dificeis de verdade, e
    /// uma heuristica de tempo razoavel que acertasse sempre seria uma noticia bem
    /// maior que este repositorio. Mas a diferenca entre saber disso e ver o
    /// contraexemplo com onze vertices e a diferenca inteira.
    /// </summary>
    [Fact]
    public void ODsaturErraEOContraexemploTemOnzeVertices()
    {
        var errou = new List<string>();

        foreach (var (nome, grafo) in Grafos.Pequenos())
            if (Gulosa.Dsatur(grafo).Quantas != ForcaBruta.NumeroCromatico(grafo)) errou.Add(nome);

        Assert.Single(errou);
        Assert.Equal("sorteado 10", errou[0]);

        var culpado = Grafos.Sorteado(11, 0.35, 10);

        Assert.Equal(11, culpado.Vertices);
        Assert.Equal(3, ForcaBruta.NumeroCromatico(culpado));
        Assert.Equal(4, Gulosa.Dsatur(culpado).Quantas);
    }

    /// <summary>
    /// O CUSTO ESCONDIDO do DSATUR: a varredura para escolher o proximo vertice.
    ///
    /// A gulosa com ordem pronta olha cada aresta duas vezes e acabou. O DSATUR
    /// olha, alem disso, a lista inteira de vertices a cada passo, e isso e o
    /// numero de vertices AO QUADRADO. No ciclo de novecentos e noventa e nove,
    /// sao duas mil olhadas contra um milhao.
    /// </summary>
    [Fact]
    public void ODsaturPagaUmaVarreduraPorPasso()
    {
        var grafo = Grafos.Ciclo(999);

        var natural = Gulosa.Pintar(grafo, Gulosa.OrdemNatural(grafo));
        var dsatur = Gulosa.Dsatur(grafo);

        Assert.Equal(2 * grafo.Arestas, natural.Olhadas);
        Assert.True(dsatur.Olhadas > 100 * natural.Olhadas,
            $"natural {natural.Olhadas}, DSATUR {dsatur.Olhadas}");

        // a varredura e o numero de vertices ao quadrado, mais as arestas: ele
        // percorre a lista INTEIRA a cada passo, inclusive quem ja tem cor
        Assert.Equal(999L * 999 + 2 * grafo.Arestas, dsatur.Olhadas);
    }

    /// <summary>
    /// A melhor de cinquenta ordens sorteadas nao salva a coroa, e salva pouco em
    /// qualquer outro lugar.
    /// </summary>
    [Fact]
    public void SortearOrdensNaoEhUmPlano()
    {
        // na grade, que precisa de duas cores, sortear ordens da quatro
        var grade = Grafos.Grade(30, 30);

        Assert.Equal(2, ForcaBruta.NumeroCromatico(Grafos.Grade(3, 4)));
        Assert.Equal(2, Gulosa.Dsatur(grade).Quantas);
        Assert.True(Gulosa.MelhorDeVarias(grade, 50).Quantas > 2);
    }

    [Fact]
    public void AColoracaoComUmaCorSoEhRecusadaQuandoHaAresta()
    {
        var grafo = Grafos.Ciclo(4);

        Assert.False(Gulosa.EhValida(grafo, [0, 0, 0, 0]));
        Assert.True(Gulosa.EhValida(grafo, [0, 1, 0, 1]));
        Assert.False(Gulosa.EhValida(grafo, [0, 1, 0]));
        Assert.False(Gulosa.EhValida(grafo, [-1, 1, 0, 1]));
    }
}
