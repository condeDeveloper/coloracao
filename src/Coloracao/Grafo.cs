namespace Conde.Coloracao;

/// <summary>
/// O SORTEIO, escrito aqui em vez de vir da biblioteca.
///
/// A documentacao do sorteio de biblioteca nao promete a mesma sequencia entre
/// versoes nem entre sistemas, e a integracao continua roda em tres. "A ordem
/// por grau gastou 7 cores e a natural gastou 9" com grafos diferentes em cada
/// sistema nao e uma medida: sao tres medidas que ninguem pode comparar.
/// </summary>
public sealed class Sorteio
{
    private ulong estado;

    public Sorteio(ulong semente) => estado = semente == 0 ? 1 : semente;

    public uint Proximo()
    {
        estado = estado * 6364136223846793005UL + 1442695040888963407UL;
        return (uint)(estado >> 32);
    }

    public int Ate(int limite) => (int)(Proximo() % (uint)limite);

    public double Fracao() => Proximo() / 4294967296.0;

    /// <summary>Embaralha no lugar, pelo metodo de Fisher e Yates.</summary>
    public void Embaralhar<T>(IList<T> lista)
    {
        for (var i = lista.Count - 1; i > 0; i--)
        {
            var j = Ate(i + 1);

            (lista[i], lista[j]) = (lista[j], lista[i]);
        }
    }
}

/// <summary>
/// Um GRAFO simples, sem direcao e sem laco.
///
/// Guardado nas duas formas de uma vez: a lista de vizinhos, que e com o que os
/// algoritmos andam, e a matriz de bits, que responde "estes dois sao vizinhos?"
/// numa olhada so. As duas existem porque as duas sao usadas o tempo todo e
/// manter so uma faria o juiz ficar lento onde ele precisa ser simples.
/// </summary>
public sealed class Grafo
{
    private readonly List<int>[] vizinhos;
    private readonly bool[,] ligado;

    public Grafo(int vertices)
    {
        Vertices = vertices;
        vizinhos = new List<int>[vertices];
        ligado = new bool[vertices, vertices];

        for (var v = 0; v < vertices; v++) vizinhos[v] = [];
    }

    public int Vertices { get; }

    public int Arestas { get; private set; }

    public void Ligar(int a, int b)
    {
        if (a == b) throw new ArgumentException("laco nao e grafo simples", nameof(a));
        if (ligado[a, b]) return;

        ligado[a, b] = true;
        ligado[b, a] = true;
        vizinhos[a].Add(b);
        vizinhos[b].Add(a);
        Arestas++;
    }

    public bool Vizinhos(int a, int b) => ligado[a, b];

    public IReadOnlyList<int> De(int vertice) => vizinhos[vertice];

    public int Grau(int vertice) => vizinhos[vertice].Count;

    /// <summary>O maior grau do grafo, que e o delta dos enunciados.</summary>
    public int MaiorGrau()
    {
        var maior = 0;

        for (var v = 0; v < Vertices; v++) maior = Math.Max(maior, Grau(v));

        return maior;
    }

    public IEnumerable<(int A, int B)> TodasAsArestas()
    {
        for (var a = 0; a < Vertices; a++)
            foreach (var b in vizinhos[a])
                if (a < b) yield return (a, b);
    }
}

/// <summary>
/// Os GRAFOS que separam os algoritmos, e por que eles sao assim.
///
/// Grafo sorteado nao separa quase nada: qualquer ordem gulosa gasta mais ou
/// menos o mesmo tanto de cores. O que separa sao os grafos ARMADOS, e o mais
/// instrutivo deles e bipartido: ele precisa de DUAS cores e consegue fazer a
/// coloracao gulosa gastar metade dos vertices.
/// </summary>
public static class Grafos
{
    /// <summary>
    /// A COROA, que e o grafo que quebra a coloracao gulosa.
    ///
    /// Dois lados de n vertices, e cada um da esquerda ligado a todos da direita
    /// MENOS o do mesmo numero. Ele e bipartido, entao duas cores bastam e nao ha
    /// discussao sobre isso.
    ///
    /// Mas na ordem esquerda1, direita1, esquerda2, direita2 e assim por diante, a
    /// gulosa pinta esquerda1 e direita1 da mesma cor, porque eles nao sao
    /// vizinhos. Dai esquerda2 e direita2 nao podem usar essa cor, porque cada um
    /// deles e vizinho de um dos dois. Entao eles ganham a cor dois. E assim por
    /// diante: n cores para um grafo de duas.
    /// </summary>
    public static (Grafo Grafo, int[] Ordem) Coroa(int lado)
    {
        var grafo = new Grafo(2 * lado);

        for (var i = 0; i < lado; i++)
            for (var j = 0; j < lado; j++)
                if (i != j) grafo.Ligar(i, lado + j);

        // a ordem que faz a gulosa gastar o maximo: os pares intercalados
        var ordem = new int[2 * lado];

        for (var i = 0; i < lado; i++)
        {
            ordem[2 * i] = i;
            ordem[2 * i + 1] = lado + i;
        }

        return (grafo, ordem);
    }

    /// <summary>Um ciclo de n vertices: duas cores se n e par, tres se e impar.</summary>
    public static Grafo Ciclo(int vertices)
    {
        var grafo = new Grafo(vertices);

        for (var v = 0; v < vertices; v++) grafo.Ligar(v, (v + 1) % vertices);

        return grafo;
    }

    /// <summary>O grafo completo: n cores, e nao ha o que discutir.</summary>
    public static Grafo Completo(int vertices)
    {
        var grafo = new Grafo(vertices);

        for (var a = 0; a < vertices; a++)
            for (var b = a + 1; b < vertices; b++) grafo.Ligar(a, b);

        return grafo;
    }

    /// <summary>A roda: um ciclo mais um vertice ligado a todos.</summary>
    public static Grafo Roda(int aro)
    {
        var grafo = new Grafo(aro + 1);

        for (var v = 0; v < aro; v++)
        {
            grafo.Ligar(v, (v + 1) % aro);
            grafo.Ligar(v, aro);
        }

        return grafo;
    }

    /// <summary>
    /// O grafo de PETERSEN: dez vertices, todos de grau tres, numero cromatico
    /// tres.
    ///
    /// Ele existe aqui por causa do teorema de Brooks, que diz que todo grafo
    /// conexo que nao e completo nem ciclo impar cabe no proprio maior grau, e nao
    /// no maior grau mais um. Petersen tem maior grau tres e cabe em tres: o limite
    /// solto de quatro nunca e preciso.
    /// </summary>
    public static Grafo Petersen()
    {
        var grafo = new Grafo(10);

        for (var i = 0; i < 5; i++)
        {
            grafo.Ligar(i, (i + 1) % 5);
            grafo.Ligar(i, 5 + i);
            grafo.Ligar(5 + i, 5 + (i + 2) % 5);
        }

        return grafo;
    }

    /// <summary>
    /// O grafo de GROTZSCH: onze vertices, NENHUM triangulo, e quatro cores.
    ///
    /// Ele e o contraexemplo da unica intuicao que todo mundo tem sobre coloracao:
    /// a de que o que obriga a usar muitas cores e ter muita gente ligada entre si.
    /// O maior grupo de vertices todos vizinhos entre si aqui tem DOIS, ou seja uma
    /// aresta, e mesmo assim tres cores nao bastam.
    ///
    /// E a construcao de Mycielski: comece com o ciclo de cinco, duplique cada
    /// vertice ligando a copia aos vizinhos do original, e acrescente um vertice
    /// ligado a todas as copias. O numero de cores sobe um e o triangulo nao
    /// aparece.
    /// </summary>
    public static Grafo Grotzsch()
    {
        // 0..4: o ciclo de cinco; 5..9: as copias; 10: o vertice de cima
        var grafo = new Grafo(11);

        for (var i = 0; i < 5; i++)
        {
            grafo.Ligar(i, (i + 1) % 5);
            grafo.Ligar(i, (i + 4) % 5);
        }

        for (var i = 0; i < 5; i++)
        {
            grafo.Ligar(5 + i, (i + 1) % 5);
            grafo.Ligar(5 + i, (i + 4) % 5);
            grafo.Ligar(5 + i, 10);
        }

        return grafo;
    }

    /// <summary>
    /// A construcao de MYCIELSKI aplicada a um grafo qualquer.
    ///
    /// Ela sobe o numero cromatico em exatamente um e nao cria triangulo nenhum se
    /// o grafo de partida nao tinha. E a maquina de fabricar contraexemplos para a
    /// ideia de que clique grande e o que obriga a muitas cores.
    /// </summary>
    public static Grafo Mycielski(Grafo grafo)
    {
        var n = grafo.Vertices;
        var novo = new Grafo(2 * n + 1);

        foreach (var (a, b) in grafo.TodasAsArestas())
        {
            novo.Ligar(a, b);
            novo.Ligar(n + a, b);
            novo.Ligar(n + b, a);
        }

        for (var i = 0; i < n; i++) novo.Ligar(n + i, 2 * n);

        return novo;
    }

    /// <summary>Um grafo bipartido sorteado: duas cores, sempre.</summary>
    public static Grafo Bipartido(int esquerda, int direita, double chance = 0.4, ulong semente = 7)
    {
        var sorteio = new Sorteio(semente);
        var grafo = new Grafo(esquerda + direita);

        for (var i = 0; i < esquerda; i++)
            for (var j = 0; j < direita; j++)
                if (sorteio.Fracao() < chance) grafo.Ligar(i, esquerda + j);

        return grafo;
    }

    /// <summary>Um grafo sorteado, cada aresta com a mesma chance.</summary>
    public static Grafo Sorteado(int vertices, double chance = 0.3, ulong semente = 11)
    {
        var sorteio = new Sorteio(semente);
        var grafo = new Grafo(vertices);

        for (var a = 0; a < vertices; a++)
            for (var b = a + 1; b < vertices; b++)
                if (sorteio.Fracao() < chance) grafo.Ligar(a, b);

        return grafo;
    }

    /// <summary>Uma grade de linhas por colunas: bipartida, entao duas cores.</summary>
    public static Grafo Grade(int linhas, int colunas)
    {
        var grafo = new Grafo(linhas * colunas);

        int No(int l, int c) => l * colunas + c;

        for (var l = 0; l < linhas; l++)
            for (var c = 0; c < colunas; c++)
            {
                if (c + 1 < colunas) grafo.Ligar(No(l, c), No(l, c + 1));
                if (l + 1 < linhas) grafo.Ligar(No(l, c), No(l + 1, c));
            }

        return grafo;
    }

    /// <summary>
    /// Os grafos PEQUENOS, onde o numero cromatico cabe em forca bruta.
    ///
    /// O juiz deste repositorio testa todas as atribuicoes de cores, entao ele so
    /// existe ate umas duas dezenas de vertices. E e exatamente por isso que estes
    /// grafos precisam ser muitos e variados: eles sao o unico lugar onde a
    /// resposta e conferida contra a definicao e nao contra outro algoritmo.
    /// </summary>
    public static IEnumerable<(string Nome, Grafo Grafo)> Pequenos()
    {
        yield return ("ciclo de 5", Ciclo(5));
        yield return ("ciclo de 6", Ciclo(6));
        yield return ("ciclo de 9", Ciclo(9));
        yield return ("completo de 5", Completo(5));
        yield return ("roda de 5", Roda(5));
        yield return ("roda de 6", Roda(6));
        yield return ("coroa 4x4", Coroa(4).Grafo);
        yield return ("coroa 5x5", Coroa(5).Grafo);
        yield return ("Petersen", Petersen());
        yield return ("Grotzsch", Grotzsch());
        yield return ("grade 3x4", Grade(3, 4));
        yield return ("bipartido 6x6", Bipartido(6, 6, 0.5, 13));

        for (ulong semente = 1; semente <= 12; semente++)
            yield return ($"sorteado {semente}", Sorteado(11, 0.35, semente));
    }

    /// <summary>Os grafos grandes, onde so as heuristicas chegam.</summary>
    public static IEnumerable<(string Nome, Grafo Grafo)> Grandes()
    {
        yield return ("coroa 100x100", Coroa(100).Grafo);
        yield return ("grade 30x30", Grade(30, 30));
        yield return ("bipartido 150x150", Bipartido(150, 150, 0.2, 17));
        yield return ("sorteado 200 (10%)", Sorteado(200, 0.1, 19));
        yield return ("sorteado 200 (50%)", Sorteado(200, 0.5, 23));
        yield return ("completo de 60", Completo(60));
        yield return ("ciclo de 999", Ciclo(999));
    }
}
