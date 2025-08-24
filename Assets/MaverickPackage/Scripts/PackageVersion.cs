using TMPro;
using UnityEngine;

namespace Ameria.Maverick
{
    public class PackageVersion : MonoBehaviour
    {
        [SerializeField] private PackageData _packageData;
        [SerializeField] private TMP_Text _versionText;

        private void Start()
        {
            _versionText.text = _packageData.Version;
        }

    }
}