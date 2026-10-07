# coloracao

Coloração de grafos do zero em C# e .NET 8: a gulosa com cinco ordens
diferentes, o DSATUR, e um juiz que é a **definição** — o menor número de cores
para o qual existe uma atribuição sem nenhuma aresta de uma cor só, achado por
busca exaustiva.

A pergunta do repositório não é qual heurística é melhor. É esta:

```
$ dotnet medidor.dll coroa

  lado   vertices   cromatico   ordem natural   a ordem ruim   por grau   por grau crescente   DSATUR   melhor de 50 sorteadas
     3          6           2               2              3          2                    2        2                       2
     5         10           2               2              5          2                    2        2                       2
    10         20           2               2             10          2                    2        2                       2
    25         50           2               2             25          2                    2        2                       2
    50        100           2               2             50          2                    2        2                       2
```

É o **mesmo grafo** e a **mesma gulosa** em todas as colunas. O que muda é a
ordem em que os vértices são visitados.

A coroa é bipartida: dois lados, cada um ligado a todos do outro menos o do
mesmo número. Duas cores bastam, e uma busca em largura acha essas duas cores
sem tentar nada. Na ordem dos pares intercalados, a gulosa gasta uma cor por
par: cinquenta cores num grafo de duas.

E não é uma ordem exótica que só existe porque eu a escrevi à mão. A busca sobre
**todas as ordens possíveis** da coroa de oito vértices chega ao mesmo pior
caso, de quatro cores, sem saber nada sobre como ela foi construída.

## A gulosa não é uma heurística. É uma ordem.

```
$ dotnet medidor.dll ordens

  grafo             vertices   cromatico   melhor ordem   pior ordem   ordem natural
  coroa 3x3                6           2              2            3               2
  coroa 4x4                8           2              2            4               2
  ciclo de 6               6           2              2            3               2
  ciclo de 7               7           3              3            3               3
  roda de 6                7           3              3            4               3
  completo de 5            5           5              5            5               5
  grade 2x4                8           2              2            4               2
  bipartido 4x4            8           2              2            3               2
  sorteado 1               8           3              3            4               3
  sorteado 2               8           4              4            5               4
  sorteado 3               8           3              3            3               3
  sorteado 4               8           3              3            4               3
```

A coluna da melhor ordem é igual à do número cromático em **toda** linha, e isso
é um teorema, não um acaso da tabela: pinte da melhor maneira, liste primeiro os
da cor um, depois os da cor dois, e a gulosa nessa ordem reproduz a resposta
ótima.

Ou seja, a gulosa não é uma aproximação ruim nem uma aproximação boa. Ela é uma
máquina que transforma uma ordem numa coloração, e o problema inteiro mudou de
lugar, não sumiu.

## O que a clique não vê

A intuição de todo mundo é que o que obriga a usar muitas cores é ter muita
gente ligada entre si. Se k vértices são todos vizinhos uns dos outros, são
precisas k cores, e esse é o limite grátis por baixo.

```
$ dotnet medidor.dll mycielski

  passo   vertices   arestas   clique   cromatico   triangulos
      0          5         5        2           3            0
      1         11        20        2           4            0
```

O grafo de Grötzsch tem onze vértices, nenhum triângulo, e precisa de quatro
cores. O maior grupo de vértices todos vizinhos entre si tem **dois**, ou seja
uma aresta.

E dá para subir isso quanto se quiser: a construção de Mycielski acrescenta uma
cor por passo sem criar triângulo nenhum.

## Três caminhos que não têm nada em comum

```
$ dotnet medidor.dll definicao

  grafo             vertices   arestas   maior grau   clique   cromatico   Brooks   bipartido   ciclo impar   entre os limites   bipartido = 2 cores   bipartido = sem ciclo impar
  ciclo de 5               5         5            2        2           3        3         NAO             5                sim                   sim                           sim
  ciclo de 6               6         6            2        2           2        2         sim             0                sim                   sim                           sim
  completo de 5            5        10            4        5           5        5         NAO             3                sim                   sim                           sim
  roda de 5                6        10            5        3           4        5         NAO             3                sim                   sim                           sim
  coroa 5x5               10        20            4        2           2        4         sim             0                sim                   sim                           sim
  Petersen                10        15            3        2           3        3         NAO             5                sim                   sim                           sim
  Grotzsch                11        20            5        2           4        5         NAO             5                sim                   sim                           sim
```

As três últimas colunas vêm de procedimentos sem nada em comum: uma busca
exaustiva sobre todas as atribuições de cor, uma busca em largura que nunca
volta atrás, e a procura de um ciclo ímpar. Quando as três concordam, o que está
certo não é a implementação: é o resultado.

A coluna do Brooks é o outro teorema. Toda coloração cabe no maior grau mais um,
e o "mais um" só é preciso em dois casos no mundo inteiro: o grafo completo e o
ciclo ímpar. O Petersen tem maior grau três e cabe em três.

## Nos grafos grandes, onde a força bruta não chega

```
$ dotnet medidor.dll heuristica

  grafo                vertices   arestas   maior grau   Brooks   natural   por grau   DSATUR   melhor de 50   olhadas (natural)   olhadas (DSATUR)
  coroa 100x100             200     9,900           99       99         2          2        2              2              19,800             59,800
  grade 30x30               900     1,740            4        4         2          2        2              4               3,480            813,480
  bipartido 150x150         300     4,585           45       45         2         10        2              3               9,170             99,170
  sorteado 200 (10%)        200     2,081           35       35        12         10        8             10               4,162             44,162
  sorteado 200 (50%)        200     9,874          116      116        35         34       31             34              19,748             59,748
  completo de 60             60     1,770           59       60        60         60       60             60               3,540              7,140
  ciclo de 999              999       999            2        3         3          3        3              3               1,998            999,999
```

Sem coluna de número cromático aqui, de propósito: colocar nesta tabela um
número que não foi conferido contra a definição seria exatamente o tipo de
número que este repositório existe para não publicar.

## Como rodar

```
dotnet test testes/Coloracao.Testes/Coloracao.Testes.csproj
dotnet run --project ferramentas/Medidor/Medidor.csproj -c Release -- tudo
```

Os comandos do medidor são `definicao`, `coroa`, `ordens`, `limites`,
`mycielski`, `heuristica` e `tudo`.

A integração contínua roda nos três sistemas. Nada aqui é medido em segundos: a
unidade é **quantas vezes um vizinho foi consultado**, porque segundo depende de
máquina e de quem mais está rodando, e olhada é o mesmo número no Linux, no
Windows e no macOS.

## O que as medidas me corrigiram

**Ordenar por grau, que é o conselho que todo mundo dá, foi a pior das três.** Eu
tinha escrito o teste afirmando que a ordem por grau decrescente nunca gasta
mais cores que a ordem natural, porque é isso que "trate primeiro os difíceis"
sugere. Num bipartido sorteado de cento e cinquenta por cento e cinquenta, ela
gasta **dez** cores e a ordem natural gasta **duas**.

O motivo, depois de olhar: num bipartido sorteado os graus dos dois lados se
misturam, então ordenar por grau **intercala** os lados, que é exatamente a
ordem ruim da coroa. A ordem natural, por acaso, lista um lado inteiro antes do
outro. E "por acaso" é a palavra certa: a ordem natural não sabe de nada, ela só
não atrapalhou.

**O DSATUR erra, e o contraexemplo tem onze vértices.** Eu tinha escrito outro
teste afirmando que ele acerta em todo grafo pequeno daqui, porque ele acerta em
quase todos e porque é exato em famílias inteiras: bipartido, ciclo, grafo de
intervalo. Um grafo sorteado de onze vértices e dezessete arestas gasta quatro
cores com ele e três com a força bruta.

Era de se esperar, porque colorir grafo é um dos problemas difíceis de verdade e
uma heurística rápida que acertasse sempre seria uma notícia bem maior que este
repositório. Mas a diferença entre saber disso e ver o contraexemplo com onze
vértices é a diferença inteira.

**O DSATUR paga uma varredura por passo, e ninguém conta essa conta.** O
enunciado dele é "escolha o vértice com mais cores diferentes na vizinhança", e
escolher significa olhar todo mundo. No ciclo de novecentos e noventa e nove, a
gulosa com ordem pronta faz 1.998 consultas e o DSATUR faz 999.999. A tabela só
passou a mostrar isso depois que eu contei a varredura junto: antes, as duas
colunas davam o mesmo número e a comparação não queria dizer nada.

**Sortear ordens não é um plano.** A melhor de cinquenta ordens sorteadas gasta
**quatro** cores na grade de trinta por trinta, que precisa de duas e que a
ordem natural acerta. Quando quase toda ordem é ruim, sortear cinquenta delas
não ajuda, e a coroa é o caso em que isso fica óbvio.

## Licença

MIT.
