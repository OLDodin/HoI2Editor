using System.Linq;

namespace HoI2Editor.Models
{
    /// <summary>
    ///     Military data
    /// </summary>
    public static class Branches
    {
        #region Internal constant

        /// <summary>
        ///     Military name
        /// </summary>
        private static readonly TextId[] Names =
        {
            TextId.Empty,
            TextId.BranchArmy,
            TextId.BranchNavy,
            TextId.BranchAirForce
        };

        #endregion

        #region Public method

        /// <summary>
        ///     Get the name of the soldier
        /// </summary>
        /// <param name="branch">Military department</param>
        /// <returns>Military name</returns>
        public static string GetName(Branch branch)
        {
            return Config.GetText(Names[(int) branch]);
        }

        /// <summary>
        ///     Get a set of military names
        /// </summary>
        /// <returns>Set of military names</returns>
        public static string[] GetNames()
        {
            return Names.Where(id => id != TextId.Empty).Select(Config.GetText).ToArray();
        }

        #endregion
    }

    /// <summary>
    ///     Army
    /// </summary>
    public enum Branch
    {
        None,
        Army, // Army
        Navy, //Navy
        Airforce //Air Force
    }
}
