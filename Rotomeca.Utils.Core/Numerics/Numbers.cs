using System.Numerics;
using Rotomeca.Core.Collections;

namespace Rotomeca.Utils.Numerics
{
    /// <summary>
    /// Fournit des méthodes utilitaires pour les opérations numériques courantes.
    /// </summary>
    public static partial class Numbers
    {
        /// <summary>
        /// Contraint une valeur dans un intervalle <c>[min, max]</c>.
        /// </summary>
        /// <typeparam name="T">Tout type implémentant <see cref="IComparable{T}"/>.</typeparam>
        /// <param name="number">La valeur à contraindre.</param>
        /// <param name="min">Borne inférieure inclusive.</param>
        /// <param name="max">Borne supérieure inclusive.</param>
        /// <returns>
        /// <paramref name="min"/> si la valeur est inférieure,
        /// <paramref name="max"/> si elle est supérieure,
        /// sinon la valeur inchangée.
        /// </returns>
        /// <example>
        /// <code>
        /// 10.Clamp(0, 5)   // → 5
        /// (-3).Clamp(0, 5) // → 0
        /// 3.Clamp(0, 5)    // → 3
        /// </code>
        /// </example>
        public static T Clamp<T>(this T number, T min, T max) where T : IComparable<T>
        {
            if (number.CompareTo(max) > 0) return max;
            else if (number.CompareTo(min) < 0) return min;
            return number;
        }

        /// <summary>
        /// Arrondit un <see cref="double"/> à un nombre de décimales donné.
        /// </summary>
        /// <param name="n">La valeur à arrondir.</param>
        /// <param name="decimals">
        /// Le nombre de décimales souhaité.
        /// Automatiquement contraint à <c>[0, 15]</c> (limites de précision du <see cref="double"/>).
        /// </param>
        /// <returns>La valeur arrondie.</returns>
        /// <example>
        /// <code>
        /// (3.14159).RoundTo(2) // → 3.14
        /// (3.14159).RoundTo(0) // → 3.0
        /// </code>
        /// </example>
        public static double RoundTo(this double n, uint decimals) => Math.Round(n, (int)decimals.Clamp(0u, 15u));

        /// <summary>
        /// Arrondit un <see cref="float"/> à un nombre de décimales donné.
        /// </summary>
        /// <param name="n">La valeur à arrondir.</param>
        /// <param name="decimals">
        /// Le nombre de décimales souhaité.
        /// Automatiquement contraint à <c>[0, 7]</c> (limites de précision du <see cref="float"/>).
        /// </param>
        /// <returns>La valeur arrondie.</returns>
        /// <remarks>
        /// Délègue en interne à <see cref="RoundTo(double, uint)"/> via un cast <c>double</c>
        /// afin d'éviter les erreurs d'arrondi inhérentes à l'arithmétique en simple précision.
        /// </remarks>
        /// <example>
        /// <code>
        /// (3.14159f).RoundTo(2) // → 3.14f
        /// </code>
        /// </example>
        public static float RoundTo(this float n, uint decimals) => (float)((double)n).RoundTo(decimals.Clamp(0u, 7u));

        /// <summary>
        /// Détermine si une valeur est comprise dans un intervalle <c>[min, max]</c> (bornes incluses).
        /// </summary>
        /// <typeparam name="T">Tout type implémentant <see cref="IComparable{T}"/>.</typeparam>
        /// <param name="number">La valeur à tester.</param>
        /// <param name="min">Borne inférieure inclusive.</param>
        /// <param name="max">Borne supérieure inclusive.</param>
        /// <returns>
        /// <see langword="true"/> si <paramref name="min"/> ≤ <paramref name="number"/> ≤ <paramref name="max"/> ;
        /// <see langword="false"/> sinon.
        /// </returns>
        /// <example>
        /// <code>
        /// 3.IsInRange(1, 5) // → true
        /// 6.IsInRange(1, 5) // → false
        /// 1.IsInRange(1, 5) // → true  (borne incluse)
        /// </code>
        /// </example>
        public static bool IsInRange<T>(this T number, T min, T max) where T : IComparable<T>
            => number.CompareTo(min) >= 0 && number.CompareTo(max) <= 0;

#if NET7_0_OR_GREATER
        /// <summary>
        /// Calcule la moyenne arithmétique d'un <see cref="RArray{T}"/>.
        /// </summary>
        /// <typeparam name="T">Type numérique implémentant <see cref="INumber{T}"/>.</typeparam>
        /// <param name="arr">Le tableau de valeurs.</param>
        /// <returns>
        /// La moyenne sous forme de <see cref="double"/>,
        /// ou <c>0.0</c> si le tableau est vide.
        /// </returns>
        /// <remarks>
        /// L'accumulation et la division sont effectuées en <see cref="double"/> pour tous les types,
        /// ce qui évite les débordements et la division entière pour les types entiers (<c>int</c>, <c>long</c>, etc.).
        /// Chaque élément est converti via <c>CreateSaturating</c> : aucune exception n'est levée,
        /// mais une perte de précision peut survenir pour les grands <c>ulong</c> ou pour <c>decimal</c>.
        /// </remarks>
        /// <example>
        /// <code>
        /// new RArray&lt;double&gt; { 1, 2, 3, 4 }.Average() // → 2.5
        /// new RArray&lt;int&gt; { 1, 2 }.Average()          // → 1.5
        /// new RArray&lt;double&gt; { }.Average()            // → 0.0
        /// </code>
        /// </example>
        public static double Average<T>(this RArray<T> arr) where T : INumber<T>
        {
            if (arr.Length == 0) return 0d;

            double sum = 0d;
            foreach (var n in arr)
                sum += double.CreateSaturating(n);

            return sum / arr.Length;
        }
#else
        /// <summary>
        /// Calcule la moyenne arithmétique d'un <see cref="RArray{T}"/> de <see cref="double"/>.
        /// </summary>
        /// <param name="arr">Le tableau de valeurs.</param>
        /// <returns>La moyenne, ou <c>0.0</c> si le tableau est vide.</returns>
        public static double Average(this RArray<double> arr)
        {
            if (arr.Length == 0) return 0d;
            double sum = 0d;
            foreach (var n in arr) sum += n;
            return sum / arr.Length;
        }

        /// <summary>
        /// Calcule la moyenne arithmétique d'un <see cref="RArray{T}"/> de <see cref="float"/>.
        /// </summary>
        /// <param name="arr">Le tableau de valeurs.</param>
        /// <returns>La moyenne, ou <c>0f</c> si le tableau est vide.</returns>
        /// <remarks>
        /// L'accumulation est effectuée en <see cref="double"/> pour limiter les erreurs
        /// de précision propres à l'arithmétique en simple précision.
        /// </remarks>
        public static float Average(this RArray<float> arr)
        {
            if (arr.Length == 0) return 0f;
            double sum = 0d;
            foreach (var n in arr) sum += n;
            return (float)(sum / arr.Length);
        }

        /// <summary>
        /// Calcule la moyenne arithmétique d'un <see cref="RArray{T}"/> de <see cref="int"/>.
        /// </summary>
        /// <param name="arr">Le tableau de valeurs.</param>
        /// <returns>
        /// La moyenne sous forme de <see cref="double"/> pour préserver la partie décimale,
        /// ou <c>0.0</c> si le tableau est vide.
        /// </returns>
        public static double Average(this RArray<int> arr)
        {
            if (arr.Length == 0) return 0d;
            double sum = 0d;
            foreach (var n in arr) sum += n;
            return sum / arr.Length;
        }

        /// <summary>
        /// Calcule la moyenne arithmétique d'un <see cref="RArray{T}"/> de <see cref="long"/>.
        /// </summary>
        /// <param name="arr">Le tableau de valeurs.</param>
        /// <returns>
        /// La moyenne sous forme de <see cref="double"/> pour préserver la partie décimale,
        /// ou <c>0.0</c> si le tableau est vide.
        /// </returns>
        public static double Average(this RArray<long> arr)
        {
            if (arr.Length == 0) return 0d;
            double sum = 0d;
            foreach (var n in arr) sum += n;
            return sum / arr.Length;
        }
#endif
    }
}