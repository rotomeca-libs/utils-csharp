using System;
using System.Collections.Generic;
using System.Text;
#if NET7_0_OR_GREATER
namespace Rotomeca.Utils.Classes.Abstract
{
    /// <summary>
    /// Classe de base abstraite fournissant un cycle de vie structuré : initialisation puis exécution.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Les sous-classes doivent surcharger <see cref="_p_Init"/> et <see cref="_p_Main"/> pour injecter
    /// leur logique métier. Le cycle de vie est déclenché via la méthode statique <see cref="Start"/>,
    /// seul point d'entrée prévu pour instancier un <c>AStartObject</c>.
    /// </para>
    /// <para>
    /// Nécessite .NET 7+ (C# 11) pour le support des membres statiques abstraits en interface.
    /// </para>
    /// </remarks>
    /// <typeparam name="TSelf">
    /// Le type de la sous-classe concrète. Suit le pattern CRTP : la sous-classe doit se passer
    /// elle-même en argument, garantissant que <see cref="Start"/> retourne le type concret.
    /// </typeparam>
    /// <example>
    /// <code>
    /// class MyService : AStartObject&lt;MyService&gt;
    /// {
    ///     private int _port;
    ///
    ///     protected override void _p_Init(params object[] args)
    ///         => _port = (int)args[0];
    ///
    ///     protected override void _p_Main()
    ///         => Console.WriteLine($"Listening on port {_port}");
    /// }
    ///
    /// MyService.Start(3000);
    /// // Output : Listening on port 3000
    /// </code>
    /// </example>
    public abstract class AStartObject<TSelf> : Interfaces.IIStartObject<TSelf>
        where TSelf : AStartObject<TSelf>, Interfaces.IIStartObject<TSelf>
    {
        /// <summary>
        /// Constructeur protégé sans argument.
        /// L'instanciation passe par <see cref="Start"/> via <see cref="Activator.CreateInstance(Type, bool)"/>.
        /// </summary>
        protected AStartObject() { }

        /// <summary>
        /// Déclenche la phase d'initialisation en déléguant à <see cref="_p_Init"/>.
        /// </summary>
        /// <param name="args">Arguments transmis à <see cref="_p_Init"/>.</param>
        private void Init(object[] args) => _p_Init(args);

        /// <summary>
        /// Déclenche la phase d'exécution principale en déléguant à <see cref="_p_Main"/>.
        /// </summary>
        private void Main() => _p_Main();

        /// <summary>
        /// Hook d'initialisation, appelé une seule fois avant <see cref="_p_Main"/>.
        /// Surchargez cette méthode pour effectuer le travail de setup
        /// (chargement de configuration, injection de dépendances, etc.).
        /// </summary>
        /// <param name="args">Arguments transmis depuis <see cref="Start"/>.</param>
        protected abstract void _p_Init(params object[] args);

        /// <summary>
        /// Hook d'exécution principale, appelé une seule fois après <see cref="_p_Init"/>.
        /// Surchargez cette méthode pour implémenter la logique cœur de l'objet
        /// (démarrage d'un serveur, lancement d'un processus, etc.).
        /// </summary>
        protected abstract void _p_Main();

        /// <summary>
        /// Méthode de fabrique statique : instancie la sous-classe concrète, exécute son cycle
        /// de vie complet et retourne l'instance prête à l'emploi.
        /// </summary>
        /// <remarks>
        /// <para>C'est le <b>seul point d'entrée</b> prévu pour créer un <c>AStartObject</c>.</para>
        /// Les étapes sont, dans l'ordre :
        /// <list type="number">
        ///   <item>Instanciation de <typeparamref name="TSelf"/> via <see cref="Activator.CreateInstance(Type, bool)"/> (constructeur protégé).</item>
        ///   <item>Appel de <c>Init(args)</c> → dispatche vers <see cref="_p_Init"/>.</item>
        ///   <item>Appel de <c>Main()</c> → dispatche vers <see cref="_p_Main"/>.</item>
        ///   <item>Retour de l'instance complètement initialisée.</item>
        /// </list>
        /// </remarks>
        /// <param name="args">Arguments transmis à <see cref="_p_Init"/> de la sous-classe.</param>
        /// <returns>Une instance complètement initialisée de <typeparamref name="TSelf"/>.</returns>
        public static TSelf Start(params object[] args)
        {
            var self = (TSelf)Activator.CreateInstance(typeof(TSelf), nonPublic: true)!;
            self.Init(args);
            self.Main();
            return self;
        }
    }
}
#endif