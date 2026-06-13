using System.Text.RegularExpressions;

namespace Rotomeca.Utils
{
#if NET7_0_OR_GREATER
    /// <summary>
    /// Fournit des expressions régulières précompilées à la compilation via <c>[GeneratedRegex]</c>.
    /// </summary>
    /// <remarks>
    /// Sur .NET 7+, les regex sont générées statiquement par le compilateur (source generator) :
    /// aucun overhead au démarrage, les erreurs de pattern sont détectées à la compilation.
    /// Sur NETSTANDARD2.0, ce sont des champs <c>static readonly</c> compilés au premier accès.
    /// </remarks>
    public static partial class Regexes
    {

        [GeneratedRegex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$")]
        private static partial Regex _EmailRegex();
        /// <summary>
        /// Valide le format minimal d'une adresse email.
        /// Vérifie la présence d'un <c>@</c>, d'un domaine et d'une extension.
        /// </summary>
        /// <remarks>
        /// Validation intentionnellement permissive — ne couvre pas l'intégralité
        /// de la RFC 5322. Pour une validation stricte, préférer une vérification
        /// côté serveur ou un envoi de confirmation.
        /// </remarks>
        public static Regex EmailRegex => _EmailRegex();


        [GeneratedRegex(@"^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$", RegexOptions.IgnoreCase)]
        private static partial Regex _UuidRegex();

        /// <summary>
        /// Valide le format standard d'un UUID (toutes versions).
        /// Format attendu : <c>xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx</c> (insensible à la casse).
        /// </summary>
        public static Regex UuidRegex => _UuidRegex();

        [GeneratedRegex(@"^[a-zA-ZÀ-ÿ]+$")]
        private static partial Regex _AlphaRegex();
        /// <summary>
        /// Valide qu'une chaîne ne contient que des lettres, avec support des caractères accentués.
        /// Couvre les plages ASCII <c>a-z</c>, <c>A-Z</c> et les caractères latins étendus <c>À-ÿ</c>.
        /// </summary>
        public static Regex AlphaRegex => _AlphaRegex();

        [GeneratedRegex(@"^#?([0-9a-f]{3}|[0-9a-f]{6})$", RegexOptions.IgnoreCase)]
        private static partial Regex _HexaRegex();
        /// <summary>
        /// Valide le format d'une couleur hexadécimale.
        /// Accepte les formats court <c>#fff</c> et long <c>#ffffff</c>,
        /// avec ou sans <c>#</c> (insensible à la casse).
        /// </summary>
        public static Regex HexaRegex => _HexaRegex();
    }
#else
    /// <summary>
    /// Fournit des expressions régulières précompilées au premier accès via <c>RegexOptions.Compiled</c>.
    /// </summary>
    /// <remarks>
    /// Sur .NET 7+, les regex sont générées statiquement par le compilateur (source generator) :
    /// aucun overhead au démarrage, les erreurs de pattern sont détectées à la compilation.
    /// Sur NETSTANDARD2.0, ce sont des champs <c>static readonly</c> compilés au premier accès.
    /// </remarks>
    public static class Regexes
    {
        /// <summary>
        /// Valide le format minimal d'une adresse email.
        /// Vérifie la présence d'un <c>@</c>, d'un domaine et d'une extension.
        /// </summary>
        /// <remarks>
        /// Validation intentionnellement permissive — ne couvre pas l'intégralité
        /// de la RFC 5322. Pour une validation stricte, préférer une vérification
        /// côté serveur ou un envoi de confirmation.
        /// </remarks>
        public static readonly Regex EmailRegex = new (
            @"^[^\s@]+@[^\s@]+\.[^\s@]+$",
            RegexOptions.Compiled);

        /// <summary>
        /// Valide le format standard d'un UUID (toutes versions).
        /// Format attendu : <c>xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx</c> (insensible à la casse).
        /// </summary>
        public static readonly Regex UuidRegex = new (
            @"^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Valide qu'une chaîne ne contient que des lettres, avec support des caractères accentués.
        /// Couvre les plages ASCII <c>a-z</c>, <c>A-Z</c> et les caractères latins étendus <c>À-ÿ</c>.
        /// </summary>
        public static readonly Regex AlphaRegex = new (
            @"^[a-zA-ZÀ-ÿ]+$",
            RegexOptions.Compiled);

        /// <summary>
        /// Valide le format d'une couleur hexadécimale.
        /// Accepte les formats court <c>#fff</c> et long <c>#ffffff</c>,
        /// avec ou sans <c>#</c> (insensible à la casse).
        /// </summary>
        public static readonly Regex HexaRegex = new (
            @"^#?([0-9a-f]{3}|[0-9a-f]{6})$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
    }
#endif
}