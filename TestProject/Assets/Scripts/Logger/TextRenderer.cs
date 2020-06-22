using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace celia.game
{
    public class TextRenderer : MonoBehaviour
    {
        public Text log;
        private RectTransform logRect;
        private RectTransform holder;
        private RectTransform content;
        // Use this for initialization
        void Start()
        {
            logRect = log.GetComponent<RectTransform>();
            holder = this.GetComponent<RectTransform>();
            content = this.transform.parent.GetComponent<RectTransform>();
        }

        // Update is called once per frame
        void Update()
        {
            holder.sizeDelta = new Vector2(content.rect.width, logRect.sizeDelta.y);
        }
    }
}
