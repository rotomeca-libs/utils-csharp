using System;
using System.Collections.Generic;
using System.Text;
#if NET7_0_OR_GREATER
namespace Rotomeca.Utils.Interfaces
{
    /// <summary>
    /// Définit un contrat pour les types capables de s'initialiser via une méthode de fabrique statique.
    /// </summary>
    /// <remarks>
    /// Nécessite .NET 7+ (C# 11) pour le support des membres statiques abstraits en interface.
    /// Le paramètre <typeparamref name="TSelf"/> suit le pattern CRTP (Curiously Recurring Template Pattern) :
    /// le type implémentant doit se passer lui-même en argument, garantissant que <c>Start</c>
    /// retourne bien le type concret et non un type de base.
    /// </remarks>
    /// <typeparam name="TSelf">Le type implémentant l'interface.</typeparam>
    public interface IIStartObject<TSelf> where TSelf : IIStartObject<TSelf>
    {
        /// <summary>
        /// Crée et retourne une nouvelle instance de <typeparamref name="TSelf"/>.
        /// </summary>
        /// <param name="args">Arguments d'initialisation. Leur nombre et leur type dépendent de l'implémentation.</param>
        /// <returns>Une nouvelle instance de <typeparamref name="TSelf"/>.</returns>
        public static abstract TSelf Start(params object[] args);
    }
}
#endif