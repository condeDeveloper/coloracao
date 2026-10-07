using Conde.Coloracao;
using Xunit;

namespace Conde.Coloracao.Testes;

/// <summary>
/// Os testes das TABELAS que o medidor imprime.
///
/// Eles existem porque numero em README que ninguem confere envelhece errado em
/// silencio: alguem troca uma constante, a tabela muda e o texto continua dizendo
/// a frase antiga. Aqui o teste afirma sobre o mesmo objeto que vira linha
/// impressa.
/// </summary>
public class MedidasTestes
{
    /// <summary>
    /// A linha principal: em todo grafo pequeno, os tres caminhos independentes
    /// concordam.
    /// </summary>
    [Fact]
    public void OsTresCaminhosIndependentesConcordamEmTodaLinha()
    {
        var linhas = Medidas.ContraADefinicao();

        Assert.Equal(24, linhas.Count);
        Assert.All(linhas, l => Assert.True(l.EntreOsLimites, l.Grafo));
        Assert.All(linhas, l => Assert.True(l.BipartidoBate, l.Grafo));
        Assert.All(linhas, l => Assert.True(l.CicloBate, l.Grafo));
    }

    /// <summary>
    /// E a clique NAO basta em todas elas: o Grotzsch e a linha que estraga a
    /// coluna.
    /// </summary>
    [Fact]
    public void ACliqueBastaEmQuaseTodasENaoNoGrotzsch()
    {
        var linhas = Medidas.ContraADefinicao();
        var grotzsch = linhas.Single(l => l.Grafo == "Grotzsch");

        Assert.False(grotzsch.CliqueBastou);
        Assert.Equal(2, grotzsch.Clique);
        Assert.Equal(4, grotzsch.Cromatico);
        Assert.True(linhas.Count(l => l.CliqueBastou) > linhas.Count / 2);
    }

    /// <summary>
    /// A frase do texto: na coroa, a ordem ruim gasta UMA COR POR PAR num grafo de
    /// duas cores, e a ordem natural gasta duas.
    /// </summary>
    [Fact]
    public void NaCoroaAOrdemRuimGastaUmaCorPorPar()
    {
        var linhas = Medidas.NaCoroa();

        Assert.Equal(6, linhas.Count);
        Assert.All(linhas, l => Assert.Equal(2, l.Cromatico));
        Assert.All(linhas, l => Assert.Equal(2, l.Natural));
        Assert.All(linhas, l => Assert.Equal(2, l.Dsatur));
        Assert.All(linhas, l => Assert.Equal(2, l.PorGrau));

        foreach (var l in linhas.Where(l => l.Lado >= 3))
        {
            Assert.Equal(l.Lado, l.OrdemRuim);
            Assert.Equal(l.Vertices / 2, l.OrdemRuim);
        }
    }

    /// <summary>
    /// A frase do texto: a melhor ordem acerta o cromatico em TODA linha, e a pior
    /// nao.
    /// </summary>
    [Fact]
    public void AMelhorOrdemAcertaEmTodaLinha()
    {
        var linhas = Medidas.TodasAsOrdens();

        Assert.Equal(12, linhas.Count);
        Assert.All(linhas, l => Assert.Equal(l.Cromatico, l.Menor));
        Assert.All(linhas, l => Assert.True(l.Maior >= l.Cromatico, l.Grafo));
        Assert.Contains(linhas, l => l.Maior > l.Cromatico);

        var coroa = linhas.Single(l => l.Grafo == "coroa 4x4");

        Assert.Equal(2, coroa.Menor);
        Assert.Equal(4, coroa.Maior);
    }

    /// <summary>
    /// A frase do texto: no bipartido grande, a ordem por grau gasta cinco vezes
    /// mais cores que a ordem natural.
    /// </summary>
    [Fact]
    public void NoBipartidoGrandeAOrdemPorGrauEhAPior()
    {
        var linha = Medidas.Heuristicas().Single(l => l.Grafo == "bipartido 150x150");

        Assert.Equal(2, linha.Natural);
        Assert.Equal(2, linha.Dsatur);
        Assert.True(linha.PorGrau >= 5 * linha.Natural, $"por grau gastou {linha.PorGrau}");
    }

    /// <summary>
    /// E o DSATUR paga a varredura: no ciclo de novecentos e noventa e nove, quase
    /// quinhentas mil olhadas contra duas mil.
    /// </summary>
    [Fact]
    public void ODsaturPagaAVarreduraNaTabela()
    {
        var linha = Medidas.Heuristicas().Single(l => l.Grafo == "ciclo de 999");

        Assert.Equal(3, linha.Dsatur);
        Assert.Equal(1_998, linha.OlhadasNatural);
        Assert.True(linha.OlhadasDsatur > 100 * linha.OlhadasNatural);
    }

    /// <summary>
    /// No completo de sessenta todas as heuristicas gastam sessenta, e e assim que
    /// tem de ser: nao ha nada a ganhar quando a resposta e o grafo inteiro.
    /// </summary>
    [Fact]
    public void NoCompletoNinguemGanhaDeNinguem()
    {
        var linha = Medidas.Heuristicas().Single(l => l.Grafo == "completo de 60");

        Assert.Equal(60, linha.Natural);
        Assert.Equal(60, linha.PorGrau);
        Assert.Equal(60, linha.Dsatur);
        Assert.Equal(60, linha.MelhorDe50);
        Assert.Equal(60, linha.Brooks);
    }

    /// <summary>
    /// A frase do texto: o Brooks aperta so em cinco linhas das vinte e quatro.
    /// </summary>
    [Fact]
    public void OBrooksEhFolgaNaMaioriaDasLinhas()
    {
        var linhas = Medidas.Limites();

        Assert.Equal(24, linhas.Count);
        Assert.All(linhas, l => Assert.True(l.Clique <= l.Cromatico && l.Cromatico <= l.Brooks, l.Grafo));
        Assert.True(linhas.Count(l => l.BrooksApertado) < linhas.Count / 2);

        var grotzsch = linhas.Single(l => l.Grafo == "Grotzsch");

        Assert.Equal(2, grotzsch.Clique);
        Assert.Equal(4, grotzsch.Cromatico);
        Assert.Equal(5, grotzsch.Brooks);
    }

    /// <summary>
    /// A frase do texto: a escada de Mycielski sobe uma cor por passo e nao cria
    /// triangulo nenhum.
    /// </summary>
    [Fact]
    public void AEscadaDeMycielskiSobeUmaCorPorPasso()
    {
        var linhas = Medidas.EscadaDeMycielski();

        Assert.Equal(2, linhas.Count);
        Assert.All(linhas, l => Assert.Equal(0, l.Triangulos));
        Assert.All(linhas, l => Assert.Equal(2, l.Clique));

        for (var i = 1; i < linhas.Count; i++)
            Assert.Equal(linhas[i - 1].Cromatico + 1, linhas[i].Cromatico);

        Assert.Equal(3, linhas[0].Cromatico);
        Assert.Equal(4, linhas[^1].Cromatico);
    }
}
