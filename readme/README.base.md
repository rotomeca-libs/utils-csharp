## Installation

```bash
dotnet add package Rotomeca.Utils
```

## Compatibilité

| Environnement | Support                                          |
| ------------- | ------------------------------------------------ |
| .NET Standard | 2.0, 2.1                                         |
| .NET          | 8.0, 9.0, 10.0                                   |
| Source Link   | ✅ (navigation vers le code depuis le débogueur) |
| Symbols       | ✅ (`.snupkg`)                                   |
| Nullable      | ✅ (activé)                                      |

---

## Modules

### Rotomeca.Utils.Async

Classe statique de référence : `Asynchronous`.

Utilitaires asynchrones inspirés des API JavaScript, portés pour un usage idiomatique en C#.

```c#
using Rotomeca.Utils.Async;

// -- Parallel --
// Exécute toutes les tâches simultanément et attend leurs résultats
var results = await Asynchronous.Parallel(
    () => FetchUserAsync(1),
    () => FetchUserAsync(2),
    () => FetchUserAsync(3)
);

// -- SequentialGenerator --
// Flux asynchrone : chaque résultat est émis dès qu'il est disponible
await foreach (var result in Asynchronous.SequentialGenerator(fns))
    Console.WriteLine(result);

// -- Retry --
// Réessaie jusqu'à 3 fois avec 500ms entre chaque tentative
var data = await Asynchronous.Retry(
    () => FetchDataAsync(),
    attempts: 3,
    delay: TimeSpan.FromMilliseconds(500)
);

// -- Sleep --
await Asynchronous.Sleep(500);                     // attend 500ms
await Asynchronous.Sleep(1000, cancellationToken); // annulable

// -- SetTimeout / ClearTimeout --
// Équivalent de setTimeout/clearTimeout JavaScript
int id = Asynchronous.SetTimeout(() => Console.WriteLine("Déclenché !"), 2000);
Asynchronous.ClearTimeout(id); // annulation si nécessaire

// -- WithTimeout --
// Lève TimeoutException si la tâche dépasse le délai
var result = await Asynchronous.WithTimeout(LongOperationAsync(), TimeSpan.FromSeconds(5));
```

---

### Rotomeca.Utils.Functional

Classe statique de référence : `Function`.

Utilitaires de programmation fonctionnelle.

```c#
using Rotomeca.Utils.Functional;

// -- Debounce --
// N'exécute l'action qu'après 300ms sans nouvel appel
Action onSearch = Function.Debounce(() => FetchResults(), 300);

// -- Throttle --
// Limite l'exécution à une fois toutes les 100ms
Action onScroll = Function.Throttle(() => UpdateScrollbar(), 100);

// -- Memoize --
// Met en cache le résultat par arguments
Func<int, int> memoFib = Function.Memoize<int, int>(n => n <= 1 ? n : fib(n - 1) + fib(n - 2));
memoFib(10); // calculé
memoFib(10); // retourné depuis le cache

// Surcharges à 2 et 3 arguments disponibles
Func<int, int, int> memoAdd = Function.Memoize<int, int, int>((a, b) => a + b);

// -- Once --
// N'exécute la fonction qu'une seule fois, retourne le résultat mis en cache
var initApp = Function.Once(() => {
    Console.WriteLine("Initialisation...");
    return CreateApp();
});
initApp(); // exécute la fonction → "Initialisation..."
initApp(); // retourne le résultat mis en cache, fn n'est plus appelée

// -- Pipe --
// Composition fonctionnelle, jusqu'à N étapes
var result = Function.Pipe(
    " hello world ",
    s => s.Trim(),
    s => s.ToUpper(),
    s => $"[{s}]"
);
// result => "[HELLO WORLD]"
```

#### Pipeline (API orientée objet)

```c#
using Rotomeca.Utils.Functional;

// Approche OO du Pipe via Pipeline / PipeObject<T>
var result = Pipeline
    .Start(" hello world ")
    .Pipe(s => s.Trim())
    .Pipe(s => s.ToUpper())
    .Unpipe();

// Approche implicite
string result = Pipeline
    .Start(" hello world ")
    .Pipe(s => s.Trim())
    .Pipe(s => s.ToUpper());
```

---

### Rotomeca.Utils.Collection

Classe statique de référence : `Arrays`.

> Note : Les extensions qui rentrent en conflit avec `Linq` étendent `RArray<T>` et non `IEnumerable<T>`.

```c#
using Rotomeca.Core.Collections;

// -- Chunk --
var source = new RArray<int>(1, 2, 3, 4, 5);
RArray<RArray<int>> chunks = source.Chunk(2);
// chunks => [[1, 2], [3, 4], [5]]

// -- Unique --
int[] source = [1, 2, 2, 3, 1, 4];
RArray<int> unique = source.Unique();
// unique => [1, 2, 3, 4]

// -- UniqueBy --
var people = new[] {
    new Person("Alice", 30),
    new Person("Bob",   25),
    new Person("Alice", 42),
};
RArray<Person> unique = people.UniqueBy(p => p.Name);
// unique => [{ "Alice", 30 }, { "Bob", 25 }]

// -- GroupTo --
var words = new[] { "apple", "avocado", "banana", "blueberry" };
var byLetter = words.GroupTo(w => w[0]);
// byLetter['a'] => ["apple", "avocado"]
// byLetter['b'] => ["banana", "blueberry"]

// -- First / Last --
RArray<int> filled = new RArray<int>(10, 20, 30);
MayBe<int> first = filled.First(); // HasValue = true, Value = 10
MayBe<int> last  = filled.Last();  // HasValue = true, Value = 30
int implicit     = first;           // Cast implicite

RArray<int> empty = new RArray<int>();
MayBe<int> none = empty.First(); // HasValue = false

// -- Sum --
#if NET7_0_OR_GREATER
    int[] nums  = [1, 2, 3, 4];
    int   total = nums.Sum(); // 10
#else
    // Basé sur IAggregable ou des surcharges pour les types primitifs
    int[] nums  = [1, 2, 3, 4];
    int   total = nums.Sum(); // 10
#endif

// -- SortBy --
var people = new[] { new Person("Charlie", 35), new Person("Alice", 28) };
RArray<Person> sorted = people.SortBy(p => p.Name);
// sorted => [{ "Alice", 28 }, { "Charlie", 35 }]

// -- Flatten / FlattenDeep --
int[][] nested = [[1, 2], [3, 4], [5]];
RArray<int> flat = nested.Flatten();
// flat => [1, 2, 3, 4, 5]

object[] deep = [1, new object[] { 2, new object[] { 3, 4 } }, 5];
RArray<int> flatDeep = deep.FlattenDeep<int>();
// flatDeep => [1, 2, 3, 4, 5]

// -- Compact --
// Retire les null d'une séquence
RArray<string> compact = new[] { "a", null, "b", null, "c" }.Compact();
// compact => ["a", "b", "c"]

// -- Partition --
var (evens, odds) = new[] { 1, 2, 3, 4, 5 }.Partition(n => n % 2 == 0);
// evens => [2, 4]  |  odds => [1, 3, 5]

// -- Intersection / Difference / Union --
RArray<int> inter = new[] { 1, 2, 3 }.Intersection(new[] { 2, 3, 4 }); // [2, 3]
RArray<int> diff  = new[] { 1, 2, 3 }.Difference(new[] { 2, 3, 4 });   // [1]

// -- Zip --
RArray<(int, string)> zipped = new RArray<int>(1, 2, 3).Zip(new[] { "a", "b", "c" });
// zipped => [(1, "a"), (2, "b"), (3, "c")]

// -- Take / Drop --
RArray<int> taken   = new RArray<int>(1, 2, 3, 4, 5).Take(3); // [1, 2, 3]
RArray<int> dropped = new[] { 1, 2, 3, 4, 5 }.Drop(2);        // [3, 4, 5]

// -- Shuffle --
RArray<int> shuffled = new[] { 1, 2, 3, 4, 5 }.Shuffle();
```

---

### Rotomeca.Utils.Numerics

Classe statique de référence : `Numbers`.

```c#
using Rotomeca.Utils.Numerics;

// -- Clamp --
10.Clamp(0, 5)   // → 5
(-3).Clamp(0, 5) // → 0
3.Clamp(0, 5)    // → 3

// -- RoundTo --
(3.14159).RoundTo(2) // → 3.14
(3.14159f).RoundTo(2) // → 3.14f

// -- IsInRange --
5.IsInRange(0, 10)  // → true
15.IsInRange(0, 10) // → false

// -- Average (.NET 7+, basé sur INumber<T>) --
RArray<int> nums = new RArray<int>(1, 2, 3, 4);
double avg = nums.Average(); // → 2.5
```

---

### Rotomeca.Utils.Dictionaries

Classe statique de référence : `Dicts`.

```c#
using Rotomeca.Utils.Dictionaries;

var dict = new Dictionary<string, int> {
    ["a"] = 1, ["b"] = 2, ["c"] = 3
};

// -- Pick --
// Retourne un nouveau dictionnaire avec uniquement les clés spécifiées
var picked = dict.Pick("a", "c");
// picked => { "a": 1, "c": 3 }

// -- Omit --
// Retourne un nouveau dictionnaire en excluant les clés spécifiées
var omitted = dict.Omit("b");
// omitted => { "a": 1, "c": 3 }

// -- MapValues --
var doubled = dict.MapValues((v, k) => v * 2);
// doubled => { "a": 2, "b": 4, "c": 6 }

// -- MapKeys --
var upper = dict.MapKeys((k, v) => k.ToUpper());
// upper => { "A": 1, "B": 2, "C": 3 }

// -- FilterKeys / FilterValues --
var filtered = dict.FilterValues(v => v > 1);
// filtered => { "b": 2, "c": 3 }

// -- Invert --
var inverted = dict.Invert();
// inverted => { 1: "a", 2: "b", 3: "c" }

// -- IsEmpty --
new Dictionary<string, int>().IsEmpty(); // → true

// -- DeepMerge --
var base_ = new Dictionary<string, object> { ["x"] = 1 };
var patch  = new Dictionary<string, object> { ["y"] = 2 };
var merged = base_.DeepMerge(patch);
// merged => { "x": 1, "y": 2 }

// -- FlattenObject / UnflattenObject --
// Aplatit un objet imbriqué en clés à notation pointée
var nested = new Dictionary<string, object> {
    ["user"] = new Dictionary<string, object> { ["name"] = "Alice", ["age"] = 30 }
};
var flat = nested.FlattenObject();
// flat => { "user.name": "Alice", "user.age": 30 }

var unflat = flat.UnflattenObject();
// unflat => { "user": { "name": "Alice", "age": 30 } }

// -- DeepClone --
var clone = myDict.DeepClone();
```

---

### Rotomeca.Utils.Types

#### `ClampedValue<T>`

Struct `readonly` — valeur immuable contrainte dans un intervalle `[Min, Max]`. Aucune allocation heap, sémantique par valeur.

```c#
using Rotomeca.Utils.Types;

var speed = new ClampedValue<int>(0, 100, 150);
Console.WriteLine(speed.Value); // 100 — clampée au max

var slower = speed.WithValue(80);
Console.WriteLine(slower.Value); // 80

int raw = speed; // conversion implicite → 100
```

#### `JsObject`

Objet dynamique à la JavaScript : collection de propriétés nommées à valeurs de types quelconques, avec syntaxe pointée via `dynamic`.

```c#
using Rotomeca.Utils.Types;

dynamic obj = new JsObject { ["name"] = "Alice", ["age"] = 30 };
obj.age = 31;

Console.WriteLine(obj.name); // "Alice"
Console.WriteLine(obj.age);  // 31

// Snapshot figé (.NET 8+) — optimisé pour les lectures intensives
((JsObject)obj).Freeze();
```

#### `Random`

Générateur de nombres aléatoires singleton, avec support de graine (`seed`) et génériques numériques sur .NET 7+.

```c#
using Rotomeca.Utils.Types;

var n   = Random.IntRange(0, 100);          // entier aléatoire [0, 100[
var n   = Random.IntRange(0, 100, 256);          // entier aléatoire [0, 100[ avec une seed
var d   = Random.Range<double>(min, max);    // double dans [min, max[
var d   = Random.Range<uint>(0, 300, 589);    // uint dans [0, 300[ avec une seed
```

---

### Rotomeca.Utils.Validators

Extensions de validation sur `string`.

```c#
using Rotomeca.Utils.Validators;

"user@example.com".IsEmail();    // → true
"https://rotomeca.dev".IsUrl();  // → true
"not-a-url".IsUrl();             // → false
"550e8400-e29b-41d4-a716-446655440000".IsUUID(); // → true
"3.14".IsNumeric();              // → true (séparateur invariant : ".")
"Héllo".IsAlpha();               // → true (support des accents)
"#ff6600".IsHexColor();          // → true
"fff".IsHexColor();              // → true (sans #, format court)
```

---

### Rotomeca.Utils.Classes (`.NET 7+`)

#### `IIStartObject<TSelf>` / `AStartObject<TSelf>`

Pattern CRTP pour les objets à cycle de vie structuré : initialisation puis exécution, avec méthode de fabrique statique.

```c#
using Rotomeca.Utils.Classes.Abstract;

class MyService : AStartObject<MyService>
{
    private int _port;

    protected override void _p_Init(params object[] args)
        => _port = (int)args[0];

    protected override void _p_Main()
        => Console.WriteLine($"Listening on port {_port}");
}

MyService.Start(3000);
// Output : Listening on port 3000
```

---

### Rotomeca.Utils (Regex)

Expressions régulières précompilées. Sur .NET 7+, générées statiquement par le compilateur (source generator) — aucun overhead au démarrage. Sur `netstandard2.x`, compilées au premier accès via `RegexOptions.Compiled`.

```c#
using Rotomeca.Utils;

bool isEmail   = Regexes.EmailRegex.IsMatch("user@example.com");
bool isUuid    = Regexes.UuidRegex.IsMatch("550e8400-e29b-41d4-a716-446655440000");
bool isAlpha   = Regexes.AlphaRegex.IsMatch("Héllo");
bool isHexColor = Regexes.HexaRegex.IsMatch("#ff6600");
```


---

## Structure du dépôt

```
Rotomeca.Utils/
├── Rotomeca.Utils.Core/
└── Rotomeca.Utils.Tests/
```

---

## Ecosystème Rotomeca

>  Voir la liste complète sur [rotomeca-libs](https://github.com/rotomeca-libs) et les packages C# sur [github](https://github.com/orgs/rotomeca-libs/repositories?q=csharp) ou sur [nuget](https://www.nuget.org/packages?q=Rotomeca&includeComputedFrameworks=true&prerel=true&sortby=relevance)

---

## Contribuer

```bash
git clone https://github.com/rotomeca-libs/utils-csharp.git
cd utils-csharp
dotnet restore
dotnet test
```

Les contributions sont les bienvenues via Pull Request sur la branche `dev`.

---

## Note sur l'utilisation de l'IA

L'intégralité du code de ce projet a d'abord été écrite à la main en essayant d'avoir le C# le plus propre possible. L'IA a ensuite été utilisée pour :

- **Proposer des axes d'amélioration et de refactorisation** si besoin, après relecture de ses modifications par mes soins, si j'ai fais du code un peu crade ou si je pars trop loin en ne respectant pas les principes KISS, SOLID, DRY et LoD.
- **La documentation et les README** -> j'ai toujours été une bille en documentation, je trouve celle de l'IA lisible et explicite ; elle a toujours été relue et validée par mes soins
- **Les tests unitaires** -> tester, c'est facile, mais présenter des tests unitaires, c'est complexe (de mon point de vue) ; l'IA a dans un premier temps généré les tests, je les ai parcourus pour les comprendre et les corriger au besoin
- **La CI/CD** -> vu que ce n'est pas mon domaine, mais ça permet d'apprendre beaucoup 👍

Sa principale contribution a donc été de m'accompagner sur les points qui me sont lacunaires.

---

## Licence

[ISC](LICENSE) © Rotomeca