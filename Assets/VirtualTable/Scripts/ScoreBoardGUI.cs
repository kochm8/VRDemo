using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

namespace CpvrLab.VirtualTable
{

    /// <summary>
    /// Generic score board that can hold any kind of table data and sync over the network
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class ScoreBoardGUI : MonoBehaviour
    {
        public Text title;
        public RectTransform scoreHeader;
        public RectTransform scoreContent;

        public GameObject headerCellPrefab;
        public GameObject contentCellPrefab;
        public GameObject contentRowPrefab;

        int colCount = 0;
        int rowCount = 0;

        private List<string> _headerCache = new List<string>();
        private List<List<string>> _dataCache = new List<List<string>>();

        List<RectTransform> rows = new List<RectTransform>(); 

        void Start()
        {
            GameManager.instance.scoreBoardData.OnDataChanged += UpdateDisplay;
            GameManager.instance.scoreBoardData.OnShow += Show;
            GameManager.instance.scoreBoardData.OnHide += Hide;

            Hide();
        }

        void OnDestroy()
        {
            GameManager.instance.scoreBoardData.OnDataChanged -= UpdateDisplay;
        }

        public void Show()
        {
            GetComponent<CanvasGroup>().alpha = 1.0f;
        }

        public void Hide()
        {
            GetComponent<CanvasGroup>().alpha = 0.0f;
        }

        // todo:    only update what has changed!
        //          currently we throw away everything and just redo the whole thing
        //          this can be optimized
        public void UpdateDisplay(ScoreBoard sb)
        {
            Debug.Log("UpdateDisplay");
            
            if (_headerCache.Count == sb.cols)
            {

            }

            if (sb.show)
                Show();
            else
                Hide();

            title.text = sb.title;

            float sizeRatio = 1.0f / (float)sb.cols;
            
            foreach (Transform child in scoreHeader)
                Destroy(child.gameObject);

            for(int i = 0; i < sb.cols; i++)
            {
                var go = Instantiate(headerCellPrefab);
                var rectTransform = go.GetComponent<RectTransform>();
                var text = go.GetComponent<Text>();
                rectTransform.SetParent(scoreHeader, false);
                rectTransform.anchorMin = new Vector2(i * sizeRatio, rectTransform.anchorMin.y);
                rectTransform.anchorMax = new Vector2((i + 1) * sizeRatio, rectTransform.anchorMax.y);
                text.text = sb.headers[i];
            }


            foreach (Transform child in scoreContent)
                Destroy(child.gameObject);

            float rowSpacing = 5.0f;
            for (int i = 0; i < sb.rows; i++)
            {
                var rowGo = Instantiate(contentRowPrefab);
                var rowRectTrfrm = rowGo.GetComponent<RectTransform>();
                rowRectTrfrm.SetParent(scoreContent, false);
                var offsetMax = rowRectTrfrm.offsetMax;
                var offsetMin = rowRectTrfrm.offsetMin;
                offsetMax.y = -i * (rowSpacing + rowRectTrfrm.rect.height);
                offsetMin.y = offsetMax.y - rowRectTrfrm.rect.height;
                //offsetMin.y =
                //rowRectTrfrm.rect.height = 
                rowRectTrfrm.offsetMax = offsetMax;
                rowRectTrfrm.offsetMin = offsetMin;
                //rowRectTrfrm.localPosition = Vector3.zero;
                //rowRectTrfrm.

                for (int j = 0; j < sb.cols; j++)
                {
                    var go = Instantiate(contentCellPrefab);
                    var rectTransform = go.GetComponent<RectTransform>();
                    var text = go.GetComponent<Text>();
                    rectTransform.SetParent(rowRectTrfrm, false);
                    rectTransform.anchorMin = new Vector2(j * sizeRatio, rectTransform.anchorMin.y);
                    rectTransform.anchorMax = new Vector2((j + 1) * sizeRatio, rectTransform.anchorMax.y);
                    text.text = sb.rowData[i][j];
                }
            }

            // update our cached version
            _headerCache = new List<string>(sb.headers);
            _dataCache = new List<List<string>>();
            for(int i = 0; i < sb.rows; i++)            
                _dataCache.Add(sb.rowData[i]);
            
        }
    }

}