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

            /// <summary>
            /// Représente un saut de ligne (<c>"\n"</c>).
            /// </summary>
            /// <remarks>
            /// <para>
            /// Accessible directement sur le type <see cref="string"/> à partir de .NET 10.
            /// Pour les versions antérieures, utilisez <see cref="Constants.END_OF_LINE"/>.
            /// </para>
            /// <para>
            /// Contrairement à <see cref="Environment.NewLine"/>, la valeur est
            /// toujours <c>"\n"</c>, quelle que soit la plateforme.
            /// </para>
            /// </remarks>
            /// <example>
            /// <code>
            /// var result = "hello" + string.EndOfLine + "world"; // "hello\nworld"
            /// </code>
            /// </example>
            public static string EndOfLine => Constants.END_OF_LINE;
        }
#endif
    }
}