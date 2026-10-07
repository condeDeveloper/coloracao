using Conde.Coloracao;
using Xunit;

namespace Conde.Coloracao.Testes;

public class GrafoTestes
{
    [Fact]
    public void OSorteioEhDeterminista()
    {
        var a = new Sorteio(7);
        var b = new Sorteio(7);

        for (var i = 0; i < 1000; i++) Assert.Equal(a.Proximo(), b.Proximo());
    }

    /// <summary>
    /// A fracao precisa cobrir os DOIS LADOS do meio: um deslocamento de 33 em vez
    /// de 32 deixaria o gerador com 31 bits, e toda decisao de "esta aresta
    /// existe?" com chance acima de meio sairia errada, com os numeros continuando
    /// plausiveis.
    /// </summary>
    [Fact]
    public void AFracaoCobreOsDoisLadosDoMeio()
    {
        var sorteio = new Sorteio(7);
        var acima = 0;
        var maior = 0.0;

        for (var i = 0; i < 10_000; i++)
        {
            var f = sorteio.Fracao();

            Assert.InRange(f, 0.0, 1.0);
            if (f > 0.5) acima++;
            if (f > maior) maior = f;
        }

        Assert.InRange(acima, 4_700, 5_300);
        Assert.True(maior > 0.999, $"o maior sorteio foi {maior}");
    }

    [Fact]
    public void OEmbaralhoDevolveUmaPermutacao()
    {
        var lista = Enumerable.Range(0, 200).ToList();

        new Sorteio(11).Embaralhar(lista);

        Assert.Equal(200, lista.Distinct().Count());
        Assert.Equal(Enumerable.Range(0, 200), lista.OrderBy(x => x));
        Assert.NotEqual(Enumerable.Range(0, 200), lista);
    }

    [Fact]
    public void LigarDuasVezesNaoCriaDuasArestas()
    {
        var grafo = new Grafo(3);

        grafo.Ligar(0, 1);
        grafo.Ligar(1, 0);
        grafo.Ligar(0, 1);

        Assert.Equal(1, grafo.Arestas);
        Assert.Single(grafo.De(0));
        Assert.True(grafo.Vizinhos(0, 1));
        Assert.True(grafo.Vizinhos(1, 0));
        Assert.False(grafo.Vizinhos(0, 2));
    }

    [Fact]
    public void LacoNaoEhGrafoSimples() =>
        Assert.Throws<ArgumentException>(() => new Grafo(3).Ligar(1, 1));

    /// <summary>
    /// A COROA e bipartida por construcao, e e esse o ponto dela: ela precisa de
    /// duas cores e nao ha discussao sobre isso.
    /// </summary>
    [Fact]
    public void ACoroaEhBipartidaEOsLadosNaoSeLigam()
    {
        for (var lado = 2; lado <= 8; lado++)
        {
            var (grafo, _) = Grafos.Coroa(lado);

            Assert.Equal(2 * lado, grafo.Vertices);
            Assert.Equal(lado * (lado - 1), grafo.Arestas);
            Assert.True(Teoremas.EhBipartido(grafo));

            // cada um e vizinho de todos do outro lado menos o do mesmo numero
            for (var i = 0; i < lado; i++)
            {
                Assert.Equal(lado - 1, grafo.Grau(i));
                Assert.False(grafo.Vizinhos(i, lado + i));
            }
        }
    }

    [Fact]
    public void OPetersenTemDezVerticesTodosDeGrauTres()
    {
        var grafo = Grafos.Petersen();

        Assert.Equal(10, grafo.Vertices);
        Assert.Equal(15, grafo.Arestas);
        Assert.All(Enumerable.Range(0, 10), v => Assert.Equal(3, grafo.Grau(v)));
        Assert.Equal(0, Medidas.Triangulos(grafo));
    }

    /// <summary>
    /// O GROTZSCH tem onze vertices, vinte arestas e NENHUM triangulo.
    ///
    /// Os tres numeros juntos sao a identidade dele. Se a construcao estivesse
    /// errada e criasse um triangulo, a afirmacao principal do repositorio, que e a
    /// de que ele precisa de quatro cores sem ter clique de tres, perderia o
    /// sentido sem dar nenhum sinal.
    /// </summary>
    [Fact]
    public void OGrotzschNaoTemTriangulo()
    {
        var grafo = Grafos.Grotzsch();

        Assert.Equal(11, grafo.Vertices);
        Assert.Equal(20, grafo.Arestas);
        Assert.Equal(0, Medidas.Triangulos(grafo));
        Assert.Equal(2, ForcaBruta.MaiorClique(grafo));
    }

    /// <summary>
    /// E a construcao de Mycielski aplicada ao ciclo de cinco DA o Grotzsch: os
    /// dois caminhos chegam ao mesmo grafo.
    /// </summary>
    [Fact]
    public void MycielskiDoCicloDeCincoEhOGrotzsch()
    {
        var feito = Grafos.Mycielski(Grafos.Ciclo(5));
        var escrito = Grafos.Grotzsch();

        Assert.Equal(escrito.Vertices, feito.Vertices);
        Assert.Equal(escrito.Arestas, feito.Arestas);

        for (var a = 0; a < 11; a++)
            for (var b = 0; b < 11; b++)
                Assert.Equal(escrito.Vizinhos(a, b), feito.Vizinhos(a, b));
    }

    [Fact]
    public void OCicloTemTodoMundoDeGrauDois()
    {
        for (var n = 3; n <= 12; n++)
        {
            var grafo = Grafos.Ciclo(n);

            Assert.Equal(n, grafo.Arestas);
            Assert.Equal(2, grafo.MaiorGrau());
            Assert.True(Teoremas.EhConexo(grafo));
        }
    }

    [Fact]
    public void OCompletoLigaTodoMundo()
    {
        var grafo = Grafos.Completo(7);

        Assert.Equal(21, grafo.Arestas);
        Assert.True(Teoremas.EhCompleto(grafo));
        Assert.All(Enumerable.Range(0, 7), v => Assert.Equal(6, grafo.Grau(v)));
    }

    [Fact]
    public void AGradeEhBipartida()
    {
        foreach (var (linhas, colunas) in new[] { (2, 3), (4, 4), (5, 7) })
            Assert.True(Teoremas.EhBipartido(Grafos.Grade(linhas, colunas)));
    }
}
