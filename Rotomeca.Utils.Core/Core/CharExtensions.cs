namespace Rotomeca.Utils
{
    /// <summary>
    /// Fournit des membres d'extension pour le type <see cref="char"/>.
    /// </summary>
    public static class CharExtensions
    {
#if NET10_0_OR_GREATER
        extension(char)
        {
            /// <summary>
            /// Représente le caractère espace (<c>' '</c>).
            /// </summary>
            /// <remarks>
            /// Accessible directement sur le type <see cref="char"/> à partir de .NET 10.
            /// Pour les versions antérieures, utilisez <c>Constants.SPACE[0]</c>.
            /// </remarks>
            /// <example>
            /// <code>
            /// var parts = "hello world".Split(char.Space); // ["hello", "world"]
            /// </code>
            /// </example>
            public static char Space => Constants.SPACE[0];

            /// <summary>
            /// Représente le caractère de saut de ligne (<c>'\n'</c>).
            /// </summary>
            /// <remarks>
            /// <para>
            /// Accessible directement sur le type <see cref="char"/> à partir de .NET 10.
            /// Pour les versions antérieures, utilisez <c>Constants.END_OF_LINE[0]</c>.
            /// </para>
            /// <para>
            /// Contrairement à <see cref="Environment.NewLine"/>, la valeur est
            /// toujours <c>'\n'</c>, quelle que soit la plateforme.
            /// </para>
            /// </remarks>
            /// <example>
            /// <code>
            /// var lines = "hello\nworld".Split(char.EndOfLine); // ["hello", "world"]
            /// </code>
            /// </example>
            public static char EndOfLine => Constants.END_OF_LINE[0];
        }
#endif
    }
}
