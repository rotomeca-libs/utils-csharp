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
#endif
    }
}