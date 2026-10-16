#if NET8_0_OR_GREATER
using System.Numerics;

namespace Rotomeca.Utils.Types;

/// <summary>
/// Contrat commun aux entiers non signés « sûrs » de la librairie (<see cref="RByte"/>,
/// <see cref="RUshort"/>, <see cref="RUint"/>, <see cref="RUlong"/>, <see cref="RUint128"/>,
/// <see cref="RNuint"/>) : ils lèvent une <see cref="OverflowException"/> au lieu de boucler.
/// </summary>
/// <typeparam name="TSelf">Type qui implémente l'interface.</typeparam>
/// <remarks>
/// Sert notamment de contrainte aux méthodes de <see cref="RUnsignedExtensions"/>, afin de ne
/// pas les exposer sur les types numériques natifs.
/// </remarks>
public interface IRUnsignedInteger<TSelf>
    : IBinaryInteger<TSelf>,
        IUnsignedNumber<TSelf>,
        IMinMaxValue<TSelf>
    where TSelf : IRUnsignedInteger<TSelf>;
#endif
