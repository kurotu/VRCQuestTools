namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Way to assign NetworkIDs.
    /// </summary>
    public enum NetworkIDAssignmentMethod
    {
        /// <summary>
        /// Use hash of hierarchy path.
        /// </summary>
        HierarchyHash,

        /// <summary>
        /// Use VRChat SDK's method.
        /// </summary>
        VRChatSDK,
    }
}
