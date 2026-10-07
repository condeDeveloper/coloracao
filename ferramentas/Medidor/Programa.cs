using Conde.Coloracao;

var pedido = args.Length > 0 ? args[0] : "tudo";
var tudo = pedido == "tudo";

if (tudo || pedido == "definicao") Definicao();
if (tudo || pedido == "coroa") Coroa();
if (tudo || pedido == "ordens") Ordens();
if (tudo || pedido == "limites") Limites();
if (tudo || pedido == "mycielski") Mycielski();
if (tudo || pedido == "heuristica") Heuristicas();

static void Definicao()
{
    Console.WriteLine();
    Console.WriteLine("  OS GRAFOS PEQUENOS CONTRA A DEFINICAO E CONTRA OS TEOREMAS");
    Console.WriteLine();
    Console.WriteLine("  grafo             vertices   arestas   maior grau   clique   cromatico   Brooks   bipartido   ciclo impar   entre os limites   bipartido = 2 cores   bipartido = sem ciclo impar");

    foreach (var l in Medidas.ContraADefinicao())
        Console.WriteLine($"  {l.Grafo,-17} {l.Vertices,8} {l.Arestas,9} {l.MaiorGrau,12} {l.Clique,8} {l.Cromatico,11} {l.Brooks,8} {Marca(l.Bipartido),11} {l.CicloImpar,13} {Marca(l.EntreOsLimites),18} {Marca(l.BipartidoBate),21} {Marca(l.CicloBate),29}");

    Console.WriteLine();
    Console.WriteLine("  as tres ultimas colunas vem de procedimentos sem nada em comum: a forca");
    Console.WriteLine("  bruta sobre todas as atribuicoes, uma busca em largura e a procura de um");
    Console.WriteLine("  ciclo impar");
    Console.WriteLine();
}

static void Coroa()
{
    Console.WriteLine();
    Console.WriteLine("  A COROA: O GRAFO DE DUAS CORES EM QUE A GULOSA GASTA METADE DOS VERTICES");
    Console.WriteLine();
    Console.WriteLine("  lado   vertices   cromatico   ordem natural   a ordem ruim   por grau   por grau crescente   DSATUR   melhor de 50 sorteadas");

    foreach (var l in Medidas.NaCoroa())
        Console.WriteLine($"  {l.Lado,4} {l.Vertices,10} {l.Cromatico,11} {l.Natural,15} {l.OrdemRuim,14} {l.PorGrau,10} {l.PorGrauCrescente,20} {l.Dsatur,8} {l.MelhorDe50,23}");

    Console.WriteLine();
    Console.WriteLine("  e o mesmo grafo e a mesma gulosa em todas as colunas: o que muda e a ORDEM");
    Console.WriteLine();
}

static void Ordens()
{
    Console.WriteLine();
    Console.WriteLine("  TODAS AS ORDENS POSSIVEIS, NOS GRAFOS EM QUE ISSO CABE");
    Console.WriteLine();
    Console.WriteLine("  grafo             vertices   cromatico   melhor ordem   pior ordem   ordem natural");

    foreach (var l in Medidas.TodasAsOrdens())
        Console.WriteLine($"  {l.Grafo,-17} {l.Vertices,8} {l.Cromatico,11} {l.Menor,14} {l.Maior,12} {l.Natural,15}");

    Console.WriteLine();
    Console.WriteLine("  a coluna da melhor ordem e igual a do cromatico em toda linha, e isso e um");
    Console.WriteLine("  teorema: para todo grafo existe uma ordem que faz a gulosa acertar em cheio");
    Console.WriteLine();
}

static void Limites()
{
    Console.WriteLine();
    Console.WriteLine("  ONDE OS LIMITES APERTAM E ONDE ELES SAO FOLGA PURA");
    Console.WriteLine();
    Console.WriteLine("  grafo             maior grau   clique (por baixo)   cromatico   Brooks (por cima)   Brooks apertado");

    foreach (var l in Medidas.Limites())
        Console.WriteLine($"  {l.Grafo,-17} {l.MaiorGrau,12} {l.Clique,20} {l.Cromatico,11} {l.Brooks,19} {Marca(l.BrooksApertado),17}");

    Console.WriteLine();
}

static void Mycielski()
{
    Console.WriteLine();
    Console.WriteLine("  A ESCADA DE MYCIELSKI: MAIS CORES SEM NENHUM TRIANGULO");
    Console.WriteLine();
    Console.WriteLine("  passo   vertices   arestas   clique   cromatico   triangulos");

    foreach (var l in Medidas.EscadaDeMycielski())
        Console.WriteLine($"  {l.Passo,5} {l.Vertices,10} {l.Arestas,9} {l.Clique,8} {l.Cromatico,11} {l.Triangulos,12}");

    Console.WriteLine();
    Console.WriteLine("  a clique fica em dois e o cromatico sobe: precisar de muitas cores nao tem");
    Console.WriteLine("  nada a ver com ter muita gente ligada entre si");
    Console.WriteLine();
}

static void Heuristicas()
{
    Console.WriteLine();
    Console.WriteLine("  AS HEURISTICAS NOS GRAFOS GRANDES, ONDE A FORCA BRUTA NAO CHEGA");
    Console.WriteLine();
    Console.WriteLine("  grafo                vertices   arestas   maior grau   Brooks   natural   por grau   DSATUR   melhor de 50   olhadas (natural)   olhadas (DSATUR)");

    foreach (var l in Medidas.Heuristicas())
        Console.WriteLine($"  {l.Grafo,-20} {l.Vertices,8:N0} {l.Arestas,9:N0} {l.MaiorGrau,12:N0} {l.Brooks,8:N0} {l.Natural,9:N0} {l.PorGrau,10:N0} {l.Dsatur,8:N0} {l.MelhorDe50,14:N0} {l.OlhadasNatural,19:N0} {l.OlhadasDsatur,18:N0}");

    Console.WriteLine();
    Console.WriteLine("  sem coluna de cromatico aqui, de proposito: numero que nao foi conferido");
    Console.WriteLine("  contra a definicao nao entra em tabela nenhuma deste repositorio");
    Console.WriteLine();
}

static string Marca(bool valor) => valor ? "sim" : "NAO";
