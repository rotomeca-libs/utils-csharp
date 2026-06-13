## Installation

```bash
dotnet add package Rotomeca.Core.Optionals
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

### Rotomeca.Utils.Collections

Classe statique de référence : `Arrays`.

>Note : Les extensions qui rentrent en conflit avec `Linq` étendent `RArray<T>` et pas `IEnumerable<T>`

```c#
using Rotomeca.Core.Collections;

// -- Chunck --
var source = new RArray<int>(1, 2, 3, 4, 5);
RArray<RArray<int>> chunks = source.Chunck(2);

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

// -- First --
RArray<int> filled = new RArray<int>(10, 20, 30);
MayBe<int> first = filled.First(); // HasValue = true, Value = 10
int implicitFirst = first; // Cast implicit

RArray<int> empty = new RArray<int>();
MayBe<int> none = empty.First(); // HasValue = false

// -- Last --
RArray<int> filled = new RArray<int>(10, 20, 30);
MayBe<int> last = filled.Last(); // HasValue = true, Value = 30
int implicitLast = last; // Cast implicit

RArray<int> empty = new RArray<int>();
MayBe<int> none = empty.Last(); // HasValue = false

// -- Sum --
#if NET7_0_OR_GREATER
    // Basée sur INumber<T>
    int[] nums = [1, 2, 3, 4];
    int total = nums.Sum(); // 10
#else
    // -- Sum<T> --
    // basé sur IAggregable
    public class Summable(int baseValue) : IAggregable<int> { 
        private int _base = baseValue;
        public int Add(int b) => _base + b;
    }

    RArray<Summable> itemsToSum = [];

    for (int i = 1; i < 5; ++i) {
        itemsToSum.Push(new Summable(i));
    }

    Summable total = itemsToSum.Sum(); //10

    // -- Sum<int/long/etc...> --
    // basé sur des surcharges
    int[] nums = [1, 2, 3, 4];
    int total = nums.Sum(); // 10
#endif
    
```