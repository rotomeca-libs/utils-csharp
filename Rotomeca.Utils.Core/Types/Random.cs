using Rotomeca.Core.Collections;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Rotomeca.Utils.Types
{
#if NET7_0_OR_GREATER
    /// <summary>
    /// Générateur de nombres aléatoires singleton.
    /// Encapsule <see cref="System.Random"/> en exposant des méthodes statiques utilitaires
    /// avec support optionnel de graine (<c>seed</c>) et, sur .NET 7+, des génériques numériques via <see cref="INumber{T}"/>.
    /// </summary>
#else
    /// <summary>
    /// Générateur de nombres aléatoires singleton.
    /// Encapsule <see cref="System.Random"/> en exposant des méthodes statiques utilitaires
    /// avec support optionnel de graine (<c>seed</c>).
    /// </summary>
#endif
    public class Random
    {
        private static Random? _instance;

        /// <summary>
        /// Instance singleton, créée à la première utilisation.
        /// </summary>
        private static Random Instance => (_instance ??= new Random());

        private System.Random _base;
        private int? _lastSeed = null;

        /// <summary>
        /// Constructeur privé. Initialise le générateur sous-jacent sans graine (séquence non déterministe).
        /// </summary>
        private Random()
        {
            _base = new System.Random();
        }

        /// <summary>
        /// Retourne un entier aléatoire non négatif.
        /// </summary>
        /// <returns>Un entier aléatoire dans l'intervalle [0, <see cref="int.MaxValue"/>).</returns>
        public int Next()
        {
            return _base.Next();
        }

        /// <summary>
        /// Retourne un entier aléatoire dans l'intervalle spécifié.
        /// </summary>
        /// <param name="min">Borne inférieure inclusive.</param>
        /// <param name="max">Borne supérieure exclusive.</param>
        /// <returns>Un entier aléatoire dans l'intervalle [<paramref name="min"/>, <paramref name="max"/>).</returns>
        public int Next(int min, int max)
        {
            return _base.Next(min, max);
        }

        /// <summary>
        /// Retourne un nombre à virgule flottante aléatoire.
        /// </summary>
        /// <returns>Un <see cref="double"/> aléatoire dans l'intervalle [0.0, 1.0).</returns>
        public double NextDouble()
        {
            return _base.NextDouble();
        }

        /// <summary>
        /// Met à jour la graine du générateur sous-jacent.
        /// Si la graine fournie est identique à la dernière appliquée, l'opération est ignorée.
        /// </summary>
        /// <param name="seed">La nouvelle graine à appliquer.</param>
        /// <returns>L'instance courante, pour permettre le chaînage.</returns>
        public Random UpdateSeed(int seed)
        {
            if (_lastSeed is not null && _lastSeed == seed) return this;

            _base = new System.Random(seed);
            return this;
        }

#if NET7_0_OR_GREATER
/// <summary>
/// Retourne un entier aléatoire dans l'intervalle spécifié.
/// Alias typé de <see cref="Range{T}(T, T, int?)"/> pour les entiers.
/// </summary>
/// <param name="min">Borne inférieure (inclusive après normalisation).</param>
/// <param name="max">Borne supérieure (exclusive après normalisation).</param>
/// <param name="seed">Graine optionnelle. Si fournie, réinitialise le générateur avant la génération.</param>
/// <returns>Un entier aléatoire dans l'intervalle normalisé [min, max).</returns>
#else
        /// <summary>
        /// Retourne un entier aléatoire dans l'intervalle spécifié.
        /// Alias typé de <see cref="Range(int, int, int?)"/>.
        /// </summary>
        /// <param name="min">Borne inférieure (inclusive après normalisation).</param>
        /// <param name="max">Borne supérieure (exclusive après normalisation).</param>
        /// <param name="seed">Graine optionnelle. Si fournie, réinitialise le générateur avant la génération.</param>
        /// <returns>Un entier aléatoire dans l'intervalle normalisé [min, max).</returns>
#endif
        public static int IntRange(int min, int max, int? seed = null) => Range(min, max, seed);

#if NET7_0_OR_GREATER
        /// <summary>
        /// Retourne une valeur aléatoire de type <typeparamref name="T"/> dans l'intervalle spécifié.
        /// Les bornes sont automatiquement normalisées si <paramref name="min"/> &gt; <paramref name="max"/>.
        /// </summary>
        /// <remarks>
        /// Le calcul est effectué en <see cref="double"/> puis converti vers <typeparamref name="T"/> via
        /// <c>T.CreateChecked</c>, ce qui garantit la précision
        /// pour les types entiers comme virgule flottante.
        /// </remarks>
        /// <typeparam name="T">
        /// Le type numérique de la valeur retournée. Doit implémenter <see cref="INumber{T}"/>.
        /// </typeparam>
        /// <param name="min">Borne inférieure (inclusive après normalisation).</param>
        /// <param name="max">Borne supérieure (exclusive après normalisation).</param>
        /// <param name="seed">Graine optionnelle. Si fournie, réinitialise le générateur avant la génération.</param>
        /// <returns>Une valeur de type <typeparamref name="T"/> dans l'intervalle normalisé [min, max).</returns>
        public static T Range<T>(T min, T max, int? seed = null) where T : INumber<T>
        {
            T trueMin = min > max ? max : min;
            T trueMax = min > max ? min : max;

            double dMin = double.CreateChecked(trueMin);
            double dMax = double.CreateChecked(trueMax);

            if (seed is not null) Instance.UpdateSeed(seed.Value);

            double result = Instance.NextDouble() * (dMax - dMin) + dMin;

            return T.CreateChecked(result);
        }
#else
        /// <summary>
        /// Retourne un entier aléatoire dans l'intervalle spécifié.
        /// </summary>
        /// <param name="min">Borne inférieure (inclusive après normalisation).</param>
        /// <param name="max">Borne supérieure (exclusive après normalisation).</param>
        /// <param name="seed">Graine optionnelle. Si fournie, réinitialise le générateur avant la génération.</param>
        /// <returns>Un <see cref="int"/> aléatoire dans l'intervalle normalisé [min, max).</returns>
        public static int    Range(int min, int max, int? seed = null) => (int)Range((double)min, max, seed);

        /// <inheritdoc cref="Range(int, int, int?)"/>
        /// <returns>Un <see cref="uint"/> aléatoire dans l'intervalle normalisé [min, max).</returns>
        public static uint   Range(uint min, uint max, int? seed = null) => (uint)Range((double)min, max, seed);

        /// <inheritdoc cref="Range(int, int, int?)"/>
        /// <returns>Un <see cref="long"/> aléatoire dans l'intervalle normalisé [min, max).</returns>
        public static long   Range(long min, long max, int? seed = null) => (long)Range((double)min, max, seed);

        /// <inheritdoc cref="Range(int, int, int?)"/>
        /// <returns>Un <see cref="ulong"/> aléatoire dans l'intervalle normalisé [min, max).</returns>
        public static ulong  Range(ulong min, ulong max, int? seed = null) => (ulong)Range((double)min, max, seed);

        /// <inheritdoc cref="Range(int, int, int?)"/>
        /// <returns>Un <see cref="float"/> aléatoire dans l'intervalle normalisé [min, max).</returns>
        public static float  Range(float min, float max, int? seed = null) => (float)Range((double)min, max, seed);

        /// <summary>
        /// Retourne un nombre à virgule flottante double précision aléatoire dans l'intervalle spécifié.
        /// Toutes les surcharges numériques de <c>Range</c> délèguent à cette méthode.
        /// </summary>
        /// <param name="min">Borne inférieure (inclusive après normalisation).</param>
        /// <param name="max">Borne supérieure (exclusive après normalisation).</param>
        /// <param name="seed">Graine optionnelle. Si fournie, réinitialise le générateur avant la génération.</param>
        /// <returns>Un <see cref="double"/> aléatoire dans l'intervalle normalisé [min, max).</returns>
        public static double Range(double min, double max, int? seed = null)
        {
            double trueMin = min > max ? max : min;
            double trueMax = min > max ? min : max;

            if (seed is not null) Instance.UpdateSeed(seed.Value);

            double result = Instance.NextDouble() * (trueMax - trueMin) + trueMin;

            return result;
        }
#endif

        /// <summary>
        /// Génère une chaîne aléatoire composée de lettres minuscules de l'alphabet latin (a–z).
        /// </summary>
        /// <remarks>
        /// Si une graine est fournie, elle est transmise à chaque appel de <c>Range</c> dans la boucle.
        /// La protection anti-double-seed d'<see cref="UpdateSeed"/> garantit que la réinitialisation
        /// n'a lieu qu'une seule fois, produisant une séquence déterministe reproductible.
        /// </remarks>
        /// <param name="size">Longueur de la chaîne à générer.</param>
        /// <param name="seed">Graine optionnelle pour une séquence reproductible.</param>
        /// <returns>Une chaîne aléatoire de longueur <paramref name="size"/>.</returns>
        public static string RandomString(uint size, int? seed = null)
        {
            const string ALPHA = "abcdefghijklmnopqrstuvwxyz";
            const int ALPHA_MIN = 0;
            const int ALPHA_MAX = 26;

            // TODO: remplacer par new RArray<char>(size) quand le constructeur avec capacité sera implémenté
            RArray<char> strArray = new List<char>((int)size).ToRArray();

            for (uint i = 0; i < size; ++i)
                strArray.Push(ALPHA[Range(ALPHA_MIN, ALPHA_MAX, seed)]);

            return strArray.Join(string.Empty);
        }

        /// <summary>
        /// Génère une chaîne aléatoire composée de caractères tirés du Plan Multilingue de Base (BMP) Unicode,
        /// couvrant l'intégralité des points de code U+0000 à U+FFFF, à l'exclusion des surrogates.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Les surrogates (U+D800–U+DFFF) sont exclus car ils ne sont pas des caractères Unicode valides seuls.
        /// L'espace valide couvre 63 488 caractères : U+0000–U+D7FF et U+E000–U+FFFF.
        /// </para>
        /// <para>
        /// Chaque caractère généré occupe exactement un <see cref="char"/> en mémoire.
        /// La longueur de la chaîne retournée est donc toujours égale à <paramref name="size"/>.
        /// </para>
        /// </remarks>
        /// <param name="size">Nombre de caractères à générer.</param>
        /// <param name="seed">Graine optionnelle pour une séquence reproductible.</param>
        /// <returns>
        /// Une chaîne de longueur <paramref name="size"/> composée de caractères BMP valides.
        /// </returns>
        public static string RandomStringBMP(uint size, int? seed = null)
        {
            // U+0000–U+D7FF (55 296) + U+E000–U+FFFF (8 192) = 63 488 caractères valides
            const int VALID_BMP_COUNT = 63_488;

            // TODO: remplacer par new RArray<char>(size) quand le constructeur avec capacité sera implémenté
            RArray<char> strArray = new List<char>((int)size).ToRArray();

            for (uint i = 0; i < size; ++i)
            {
                int index = Range(0, VALID_BMP_COUNT, seed);
                // Décalage de 0x800 pour sauter le bloc surrogate [U+D800, U+DFFF]
                char c = (char)(index < 0xD800 ? index : index + 0x800);
                strArray.Push(c);
            }

            return strArray.Join(string.Empty);
        }

        /// <summary>
        /// Génère une chaîne aléatoire composée de caractères tirés de l'intégralité
        /// de l'espace Unicode (U+0000 à U+10FFFF), à l'exclusion des surrogates.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Les surrogates (U+D800–U+DFFF) sont exclus. L'espace valide couvre 1 112 064 scalaires :
        /// U+0000–U+D7FF et U+E000–U+10FFFF.
        /// </para>
        /// <para>
        /// Les points de code au-delà de U+FFFF (plans supplémentaires : emojis, caractères historiques, etc.)
        /// sont encodés en UTF-16 sous forme de paires surrogate, ce qui signifie que la longueur
        /// de la chaîne en <see cref="char"/> peut être supérieure à <paramref name="size"/>.
        /// </para>
        /// <para>
        /// <see cref="char.ConvertFromUtf32"/> est utilisé pour garantir la compatibilité avec
        /// NETSTANDARD2.0 sans dépendance à <c>System.Text.Rune</c>.
        /// </para>
        /// </remarks>
        /// <param name="size">Nombre de points de code (scalaires Unicode) à générer.</param>
        /// <param name="seed">Graine optionnelle pour une séquence reproductible.</param>
        /// <returns>
        /// Une chaîne composée de <paramref name="size"/> scalaires Unicode valides.
        /// Sa longueur en <see cref="char"/> peut dépasser <paramref name="size"/>
        /// si des caractères hors-BMP sont générés.
        /// </returns>
        public static string RandomStringUnicode(uint size, int? seed = null)
        {
            // U+0000–U+D7FF (55 296) + U+E000–U+10FFFF (1 056 768) = 1 112 064 scalaires valides
            const int VALID_UNICODE_COUNT = 1_112_064;

            // Capacité approximative : les caractères hors-BMP produisent 2 chars
            var sb = new StringBuilder((int)size);

            for (uint i = 0; i < size; ++i)
            {
                int index = Range(0, VALID_UNICODE_COUNT, seed);
                // Décalage de 0x800 pour sauter le bloc surrogate [U+D800, U+DFFF]
                int codePoint = index < 0xD800 ? index : index + 0x800;
                sb.Append(char.ConvertFromUtf32(codePoint));
            }

            return sb.ToString();
        }
    }
}