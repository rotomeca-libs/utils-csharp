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
        /// <para>
        /// Analogue à <see cref="string.Empty"/>, fourni pour assurer
        /// la cohérence avec les packages Rotomeca en TypeScript et PHP.
        /// </para>
        /// <para>
        /// Miroir TypeScript :
        /// <see href="https://github.com/Rotomeca/node-utils/blob/main/lib/constants.ts#L9"/>
        /// </para>
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

        /// <summary>
        /// Représente un saut de ligne (<c>"\n"</c>).
        /// </summary>
        /// <remarks>
        /// <para>
        /// Contrairement à <see cref="Environment.NewLine"/>, la valeur est
        /// toujours <c>"\n"</c>, quelle que soit la plateforme.
        /// </para>
        /// <para>
        /// Sur .NET 10 et supérieur, préférez <c>string.EndOfLine</c>
        /// via <see cref="StringExtensions"/>, ou <c>char.EndOfLine</c>
        /// via <see cref="CharExtensions"/>.
        /// </para>
        /// </remarks>
        public const string END_OF_LINE = "\n";
    }
}