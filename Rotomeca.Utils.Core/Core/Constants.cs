namespace Rotomeca.Utils
{
    /// <summary>
    /// Fournit des membres d'extension pour le type <see cref="string"/>.
    /// </summary>
    public static class StringExtensions
    {
#if NET10_0_OR_GREATER
        extension(string)
        {
            /// <summary>
            /// Représente un espace simple.
            /// </summary>
            /// <remarks>
            /// Accessible directement sur le type <see cref="string"/> à partir de .NET 10.
            /// Pour les versions antérieures, utilisez <see cref="Constants.SPACE"/>.
            /// </remarks>
            /// <example>
            /// <code>
            /// var result = string.Space + "hello"; // " hello"
            /// </code>
            /// </example>
            public static string Space => Constants.SPACE;
        }
#else

#endif
    }

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