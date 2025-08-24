using UnityEngine;

namespace Ameria.Maverick
{
    [CreateAssetMenu(fileName = "PackageData", menuName = "Maverick/PackageData", order = 1)]
    public class PackageData : ScriptableObject
    {
        public string Id;
        public string Title;
        public string Version;
        public string IconPath;
        public string[] Scenes;
    }
}