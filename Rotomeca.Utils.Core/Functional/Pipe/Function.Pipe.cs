namespace Rotomeca.Utils.Functional
{
    public static partial class Function
    {
        /// <summary>
        /// Applique une fonction à une valeur et retourne le résultat.
        /// </summary>
        /// <typeparam name="TSource">Type de la valeur d'entrée.</typeparam>
        /// <typeparam name="TResult1">Type du résultat final.</typeparam>
        /// <param name="source">Valeur initiale.</param>
        /// <param name="func1">Fonction à appliquer.</param>
        /// <returns>Résultat de la fonction.</returns>
        public static TResult1 Pipe<TSource, TResult1>(TSource source, Func<TSource, TResult1> func1)
        {
            return func1(source);
        }

        /// <summary>
        /// Enchaîne 2 fonctions en appliquant chaque fonction au résultat de la précédente.
        /// </summary>
        /// <typeparam name="TSource">Type de la valeur d'entrée.</typeparam>
        /// <typeparam name="TResult1">Type du résultat de la fonction 1.</typeparam>
        /// <typeparam name="TResult2">Type du résultat final.</typeparam>
        /// <param name="source">Valeur initiale.</param>
        /// <param name="func1">Première fonction à appliquer.</param>
        /// <param name="func2">Deuxième fonction à appliquer.</param>
        /// <returns>Résultat de la 2ème fonction.</returns>
        public static TResult2 Pipe<TSource, TResult1, TResult2>(TSource source, Func<TSource, TResult1> func1, Func<TResult1, TResult2> func2)
        {
            return func2(func1(source));
        }

        /// <summary>
        /// Enchaîne 3 fonctions en appliquant chaque fonction au résultat de la précédente.
        /// </summary>
        /// <typeparam name="TSource">Type de la valeur d'entrée.</typeparam>
        /// <typeparam name="TResult1">Type du résultat de la fonction 1.</typeparam>
        /// <typeparam name="TResult2">Type du résultat de la fonction 2.</typeparam>
        /// <typeparam name="TResult3">Type du résultat final.</typeparam>
        /// <param name="source">Valeur initiale.</param>
        /// <param name="func1">Première fonction à appliquer.</param>
        /// <param name="func2">Deuxième fonction à appliquer.</param>
        /// <param name="func3">Troisième fonction à appliquer.</param>
        /// <returns>Résultat de la 3ème fonction.</returns>
        public static TResult3 Pipe<TSource, TResult1, TResult2, TResult3>(TSource source, Func<TSource, TResult1> func1, Func<TResult1, TResult2> func2, Func<TResult2, TResult3> func3)
        {
            return func3(func2(func1(source)));
        }

        /// <summary>
        /// Enchaîne 4 fonctions en appliquant chaque fonction au résultat de la précédente.
        /// </summary>
        /// <typeparam name="TSource">Type de la valeur d'entrée.</typeparam>
        /// <typeparam name="TResult1">Type du résultat de la fonction 1.</typeparam>
        /// <typeparam name="TResult2">Type du résultat de la fonction 2.</typeparam>
        /// <typeparam name="TResult3">Type du résultat de la fonction 3.</typeparam>
        /// <typeparam name="TResult4">Type du résultat final.</typeparam>
        /// <param name="source">Valeur initiale.</param>
        /// <param name="func1">Première fonction à appliquer.</param>
        /// <param name="func2">Deuxième fonction à appliquer.</param>
        /// <param name="func3">Troisième fonction à appliquer.</param>
        /// <param name="func4">Quatrième fonction à appliquer.</param>
        /// <returns>Résultat de la 4ème fonction.</returns>
        public static TResult4 Pipe<TSource, TResult1, TResult2, TResult3, TResult4>(TSource source, Func<TSource, TResult1> func1, Func<TResult1, TResult2> func2, Func<TResult2, TResult3> func3, Func<TResult3, TResult4> func4)
        {
            return func4(func3(func2(func1(source))));
        }

        /// <summary>
        /// Enchaîne 5 fonctions en appliquant chaque fonction au résultat de la précédente.
        /// </summary>
        /// <typeparam name="TSource">Type de la valeur d'entrée.</typeparam>
        /// <typeparam name="TResult1">Type du résultat de la fonction 1.</typeparam>
        /// <typeparam name="TResult2">Type du résultat de la fonction 2.</typeparam>
        /// <typeparam name="TResult3">Type du résultat de la fonction 3.</typeparam>
        /// <typeparam name="TResult4">Type du résultat de la fonction 4.</typeparam>
        /// <typeparam name="TResult5">Type du résultat final.</typeparam>
        /// <param name="source">Valeur initiale.</param>
        /// <param name="func1">Première fonction à appliquer.</param>
        /// <param name="func2">Deuxième fonction à appliquer.</param>
        /// <param name="func3">Troisième fonction à appliquer.</param>
        /// <param name="func4">Quatrième fonction à appliquer.</param>
        /// <param name="func5">Cinquième fonction à appliquer.</param>
        /// <returns>Résultat de la 5ème fonction.</returns>
        public static TResult5 Pipe<TSource, TResult1, TResult2, TResult3, TResult4, TResult5>(TSource source, Func<TSource, TResult1> func1, Func<TResult1, TResult2> func2, Func<TResult2, TResult3> func3, Func<TResult3, TResult4> func4, Func<TResult4, TResult5> func5)
        {
            return func5(func4(func3(func2(func1(source)))));
        }

        /// <summary>
        /// Enchaîne 6 fonctions en appliquant chaque fonction au résultat de la précédente.
        /// </summary>
        /// <typeparam name="TSource">Type de la valeur d'entrée.</typeparam>
        /// <typeparam name="TResult1">Type du résultat de la fonction 1.</typeparam>
        /// <typeparam name="TResult2">Type du résultat de la fonction 2.</typeparam>
        /// <typeparam name="TResult3">Type du résultat de la fonction 3.</typeparam>
        /// <typeparam name="TResult4">Type du résultat de la fonction 4.</typeparam>
        /// <typeparam name="TResult5">Type du résultat de la fonction 5.</typeparam>
        /// <typeparam name="TResult6">Type du résultat final.</typeparam>
        /// <param name="source">Valeur initiale.</param>
        /// <param name="func1">Première fonction à appliquer.</param>
        /// <param name="func2">Deuxième fonction à appliquer.</param>
        /// <param name="func3">Troisième fonction à appliquer.</param>
        /// <param name="func4">Quatrième fonction à appliquer.</param>
        /// <param name="func5">Cinquième fonction à appliquer.</param>
        /// <param name="func6">Sixième fonction à appliquer.</param>
        /// <returns>Résultat de la 6ème fonction.</returns>
        public static TResult6 Pipe<TSource, TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>(TSource source, Func<TSource, TResult1> func1, Func<TResult1, TResult2> func2, Func<TResult2, TResult3> func3, Func<TResult3, TResult4> func4, Func<TResult4, TResult5> func5, Func<TResult5, TResult6> func6)
        {
            return func6(func5(func4(func3(func2(func1(source))))));
        }

        /// <summary>
        /// Enchaîne 7 fonctions en appliquant chaque fonction au résultat de la précédente.
        /// </summary>
        /// <typeparam name="TSource">Type de la valeur d'entrée.</typeparam>
        /// <typeparam name="TResult1">Type du résultat de la fonction 1.</typeparam>
        /// <typeparam name="TResult2">Type du résultat de la fonction 2.</typeparam>
        /// <typeparam name="TResult3">Type du résultat de la fonction 3.</typeparam>
        /// <typeparam name="TResult4">Type du résultat de la fonction 4.</typeparam>
        /// <typeparam name="TResult5">Type du résultat de la fonction 5.</typeparam>
        /// <typeparam name="TResult6">Type du résultat de la fonction 6.</typeparam>
        /// <typeparam name="TResult7">Type du résultat final.</typeparam>
        /// <param name="source">Valeur initiale.</param>
        /// <param name="func1">Première fonction à appliquer.</param>
        /// <param name="func2">Deuxième fonction à appliquer.</param>
        /// <param name="func3">Troisième fonction à appliquer.</param>
        /// <param name="func4">Quatrième fonction à appliquer.</param>
        /// <param name="func5">Cinquième fonction à appliquer.</param>
        /// <param name="func6">Sixième fonction à appliquer.</param>
        /// <param name="func7">Septième fonction à appliquer.</param>
        /// <returns>Résultat de la 7ème fonction.</returns>
        public static TResult7 Pipe<TSource, TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7>(TSource source, Func<TSource, TResult1> func1, Func<TResult1, TResult2> func2, Func<TResult2, TResult3> func3, Func<TResult3, TResult4> func4, Func<TResult4, TResult5> func5, Func<TResult5, TResult6> func6, Func<TResult6, TResult7> func7)
        {
            return func7(func6(func5(func4(func3(func2(func1(source)))))));
        }

        /// <summary>
        /// Enchaîne 8 fonctions en appliquant chaque fonction au résultat de la précédente.
        /// </summary>
        /// <typeparam name="TSource">Type de la valeur d'entrée.</typeparam>
        /// <typeparam name="TResult1">Type du résultat de la fonction 1.</typeparam>
        /// <typeparam name="TResult2">Type du résultat de la fonction 2.</typeparam>
        /// <typeparam name="TResult3">Type du résultat de la fonction 3.</typeparam>
        /// <typeparam name="TResult4">Type du résultat de la fonction 4.</typeparam>
        /// <typeparam name="TResult5">Type du résultat de la fonction 5.</typeparam>
        /// <typeparam name="TResult6">Type du résultat de la fonction 6.</typeparam>
        /// <typeparam name="TResult7">Type du résultat de la fonction 7.</typeparam>
        /// <typeparam name="TResult8">Type du résultat final.</typeparam>
        /// <param name="source">Valeur initiale.</param>
        /// <param name="func1">Première fonction à appliquer.</param>
        /// <param name="func2">Deuxième fonction à appliquer.</param>
        /// <param name="func3">Troisième fonction à appliquer.</param>
        /// <param name="func4">Quatrième fonction à appliquer.</param>
        /// <param name="func5">Cinquième fonction à appliquer.</param>
        /// <param name="func6">Sixième fonction à appliquer.</param>
        /// <param name="func7">Septième fonction à appliquer.</param>
        /// <param name="func8">Huitième fonction à appliquer.</param>
        /// <returns>Résultat de la 8ème fonction.</returns>
        public static TResult8 Pipe<TSource, TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8>(TSource source, Func<TSource, TResult1> func1, Func<TResult1, TResult2> func2, Func<TResult2, TResult3> func3, Func<TResult3, TResult4> func4, Func<TResult4, TResult5> func5, Func<TResult5, TResult6> func6, Func<TResult6, TResult7> func7, Func<TResult7, TResult8> func8)
        {
            return func8(func7(func6(func5(func4(func3(func2(func1(source))))))));
        }

        /// <summary>
        /// Enchaîne 9 fonctions en appliquant chaque fonction au résultat de la précédente.
        /// </summary>
        /// <typeparam name="TSource">Type de la valeur d'entrée.</typeparam>
        /// <typeparam name="TResult1">Type du résultat de la fonction 1.</typeparam>
        /// <typeparam name="TResult2">Type du résultat de la fonction 2.</typeparam>
        /// <typeparam name="TResult3">Type du résultat de la fonction 3.</typeparam>
        /// <typeparam name="TResult4">Type du résultat de la fonction 4.</typeparam>
        /// <typeparam name="TResult5">Type du résultat de la fonction 5.</typeparam>
        /// <typeparam name="TResult6">Type du résultat de la fonction 6.</typeparam>
        /// <typeparam name="TResult7">Type du résultat de la fonction 7.</typeparam>
        /// <typeparam name="TResult8">Type du résultat de la fonction 8.</typeparam>
        /// <typeparam name="TResult9">Type du résultat final.</typeparam>
        /// <param name="source">Valeur initiale.</param>
        /// <param name="func1">Première fonction à appliquer.</param>
        /// <param name="func2">Deuxième fonction à appliquer.</param>
        /// <param name="func3">Troisième fonction à appliquer.</param>
        /// <param name="func4">Quatrième fonction à appliquer.</param>
        /// <param name="func5">Cinquième fonction à appliquer.</param>
        /// <param name="func6">Sixième fonction à appliquer.</param>
        /// <param name="func7">Septième fonction à appliquer.</param>
        /// <param name="func8">Huitième fonction à appliquer.</param>
        /// <param name="func9">Neuvième fonction à appliquer.</param>
        /// <returns>Résultat de la 9ème fonction.</returns>
        public static TResult9 Pipe<TSource, TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9>(TSource source, Func<TSource, TResult1> func1, Func<TResult1, TResult2> func2, Func<TResult2, TResult3> func3, Func<TResult3, TResult4> func4, Func<TResult4, TResult5> func5, Func<TResult5, TResult6> func6, Func<TResult6, TResult7> func7, Func<TResult7, TResult8> func8, Func<TResult8, TResult9> func9)
        {
            return func9(func8(func7(func6(func5(func4(func3(func2(func1(source)))))))));
        }

        /// <summary>
        /// Enchaîne 10 fonctions en appliquant chaque fonction au résultat de la précédente.
        /// </summary>
        /// <typeparam name="TSource">Type de la valeur d'entrée.</typeparam>
        /// <typeparam name="TResult1">Type du résultat de la fonction 1.</typeparam>
        /// <typeparam name="TResult2">Type du résultat de la fonction 2.</typeparam>
        /// <typeparam name="TResult3">Type du résultat de la fonction 3.</typeparam>
        /// <typeparam name="TResult4">Type du résultat de la fonction 4.</typeparam>
        /// <typeparam name="TResult5">Type du résultat de la fonction 5.</typeparam>
        /// <typeparam name="TResult6">Type du résultat de la fonction 6.</typeparam>
        /// <typeparam name="TResult7">Type du résultat de la fonction 7.</typeparam>
        /// <typeparam name="TResult8">Type du résultat de la fonction 8.</typeparam>
        /// <typeparam name="TResult9">Type du résultat de la fonction 9.</typeparam>
        /// <typeparam name="TResult10">Type du résultat final.</typeparam>
        /// <param name="source">Valeur initiale.</param>
        /// <param name="func1">Première fonction à appliquer.</param>
        /// <param name="func2">Deuxième fonction à appliquer.</param>
        /// <param name="func3">Troisième fonction à appliquer.</param>
        /// <param name="func4">Quatrième fonction à appliquer.</param>
        /// <param name="func5">Cinquième fonction à appliquer.</param>
        /// <param name="func6">Sixième fonction à appliquer.</param>
        /// <param name="func7">Septième fonction à appliquer.</param>
        /// <param name="func8">Huitième fonction à appliquer.</param>
        /// <param name="func9">Neuvième fonction à appliquer.</param>
        /// <param name="func10">Dixième fonction à appliquer.</param>
        /// <returns>Résultat de la 10ème fonction.</returns>
        public static TResult10 Pipe<TSource, TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9, TResult10>(TSource source, Func<TSource, TResult1> func1, Func<TResult1, TResult2> func2, Func<TResult2, TResult3> func3, Func<TResult3, TResult4> func4, Func<TResult4, TResult5> func5, Func<TResult5, TResult6> func6, Func<TResult6, TResult7> func7, Func<TResult7, TResult8> func8, Func<TResult8, TResult9> func9, Func<TResult9, TResult10> func10)
        {
            return func10(func9(func8(func7(func6(func5(func4(func3(func2(func1(source))))))))));
        }

    }
}