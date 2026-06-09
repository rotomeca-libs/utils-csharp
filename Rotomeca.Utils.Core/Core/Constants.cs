namespace Rotomeca.Utils
{
    /// <summary>
    /// Constantes de chaînes de caractères.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Représente une chaîne de caractères vide (<c>""</c>).
        /// </summary>
        /// <remarks>
        /// Analogue à <see cref="string.Empty"/>, fourni pour assurer
        /// la cohérence avec les packages Rotomeca en TypeScript et PHP.
        /// </remarks>
        /// <remarks>
        /// Miroir TypeScript :
        /// <see href="https://github.com/Rotomeca/node-utils/blob/main/lib/constants.ts#L9"/>
        /// </remarks>
        public const string EMPTY_STRING = "";

        /// <summary>
        /// Représente un espace simple (<c>" "</c>).
        /// </summary>
        /// <remarks>
        /// Sur .NET 10 et supérieur, préférez <c>string.Space</c>
        /// via <see cref="StringExtensions"/>.
        /// </remarks>
        public const string SPACE = " ";
    }
}