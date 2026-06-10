using Rotomeca.Utils.Types;
using Rotomeca.Utils.Types.Interfaces;

namespace Rotomeca.Utils.Async.Helpers
{
    /// <summary>
    /// Représente un nombre de tentatives garanti dans l'intervalle [1, <see cref="uint.MaxValue"/>].
    /// </summary>
    /// <remarks>
    /// <para>
    /// Toute valeur inférieure à 1 est automatiquement clampée à 1 —
    /// il est impossible de créer une instance avec 0 tentatives.
    /// </para>
    /// <para>
    /// Les conversions implicites depuis et vers <see cref="uint"/> permettent
    /// une utilisation transparente aux points d'appel.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// AttemptNumber attempts = 3;
    /// AttemptNumber zero     = 0; // clampée à 1 silencieusement
    ///
    /// await Asynchronous.Retry(fn, attempts, delay: 500);
    /// </code>
    /// </example>
    /// <seealso cref="Rotomeca.Utils.Types.ClampedValue{T}"/>
    public readonly struct AttemptNumber(uint value = 1) : IClampedValue<uint>
    {
        private readonly ClampedValue<uint> _internal = new(1, uint.MaxValue, value);

        /// <inheritdoc/>
        public uint Min => _internal.Min;

        /// <inheritdoc/>
        public uint Max => _internal.Max;

        /// <inheritdoc/>
        public uint Value => _internal.Value;

        /// <summary>
        /// Retourne un <see cref="IClampedValue{T}"/> avec <paramref name="value"/>
        /// clampée dans [1, <see cref="uint.MaxValue"/>].
        /// </summary>
        /// <param name="value">Nouveau nombre de tentatives.</param>
        public IClampedValue<uint> WithValue(uint value) => new AttemptNumber(value);

        /// <inheritdoc/>
        public override string ToString() => Value.ToString();

        /// <summary>
        /// Convertit implicitement en <see cref="uint"/>.
        /// </summary>
        public static implicit operator uint(AttemptNumber nb) => nb.Value;

        /// <summary>
        /// Convertit implicitement un <see cref="uint"/> en <see cref="AttemptNumber"/>.
        /// </summary>
        public static implicit operator AttemptNumber(uint nb) => new(nb);
    }
}