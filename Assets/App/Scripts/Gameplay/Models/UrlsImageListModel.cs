using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Gameplay.Models
{
    [System.Serializable]
    public class UrlsImageListModel
    {
        [System.Serializable]
        public class UrlAndImageData
        {
            public string Url = null;
            public string Name = null;

        }

        [SerializeField] public List<UrlAndImageData> Urls = null;
    }
}