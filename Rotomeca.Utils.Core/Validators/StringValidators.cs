using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Rotomeca.Utils.Validators
{
    /// <summary>
    /// Fournit des méthodes d'extension de validation pour les chaînes de caractères.
    /// </summary>
    public static class StringValidators
    {
        /// <summary>
        /// Vérifie que la chaîne respecte le format minimal d'une adresse email.
        /// </summary>
        /// <remarks>
        /// Validation intentionnellement permissive — ne couvre pas l'intégralité de la RFC 5322.
        /// Pour une validation stricte, préférer une vérification côté serveur ou un envoi de confirmation.
        /// </remarks>
        /// <param name="str">La chaîne à valider.</param>
        /// <returns><c>true</c> si la chaîne ressemble à une adresse email valide ; <c>false</c> sinon.</returns>
        public static bool IsEmail(this string str) => Regexes.EmailRegex.IsMatch(str);

        /// <summary>
        /// Vérifie que la chaîne est une URL absolue valide avec un schéma HTTP ou HTTPS.
        /// </summary>
        /// <param name="str">La chaîne à valider.</param>
        /// <returns>
        /// <c>true</c> si la chaîne est une URL absolue dont le schéma est <c>http</c> ou <c>https</c> ;
        /// <c>false</c> sinon.
        /// </returns>
        public static bool IsUrl(this string str) => Uri.TryCreate(str, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

        /// <summary>
        /// Vérifie que la chaîne respecte le format standard d'un UUID (toutes versions).
        /// Format attendu : <c>xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx</c> (insensible à la casse).
        /// </summary>
        /// <param name="str">La chaîne à valider.</param>
        /// <returns><c>true</c> si la chaîne est un UUID valide ; <c>false</c> sinon.</returns>
        public static bool IsUUID(this string str) => Regexes.UuidRegex.IsMatch(str);

        /// <summary>
        /// Vérifie que la chaîne représente un nombre décimal valide.
        /// </summary>
        /// <remarks>
        /// Utilise <see cref="CultureInfo.InvariantCulture"/> : le séparateur décimal attendu est toujours <c>.</c>,
        /// indépendamment des paramètres régionaux du système.
        /// </remarks>
        /// <param name="str">La chaîne à valider.</param>
        /// <returns><c>true</c> si la chaîne est un nombre décimal valide ; <c>false</c> sinon.</returns>
        public static bool IsNumeric(this string str) => decimal.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal _);

        /// <summary>
        /// Vérifie que la chaîne ne contient que des lettres,
        /// y compris les caractères latins accentués (plage <c>À-ÿ</c>).
        /// </summary>
        /// <param name="str">La chaîne à valider.</param>
        /// <returns><c>true</c> si la chaîne est entièrement alphabétique ; <c>false</c> sinon.</returns>
        public static bool IsAlpha(this string str) => Regexes.AlphaRegex.IsMatch(str);

        /// <summary>
        /// Vérifie que la chaîne représente une couleur hexadécimale valide.
        /// Accepte les formats court <c>#fff</c> et long <c>#ffffff</c>,
        /// avec ou sans <c>#</c> (insensible à la casse).
        /// </summary>
        /// <param name="str">La chaîne à valider.</param>
        /// <returns><c>true</c> si la chaîne est une couleur hexadécimale valide ; <c>false</c> sinon.</returns>
        public static bool IsHexColor(this string str) => Regexes.HexaRegex.IsMatch(str);
    }
}