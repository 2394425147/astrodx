using System.Collections;
using System.Collections.Generic;
using Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace Title
{
    public class TitleManager : MonoBehaviour
    {
        public Text anyKeyPressed;
        
        public List<string> textList;

        private int _textIndex;
        private void Start()
        {
            textList = new List<string>
            {
                Localization.ParseAuto($"TITLE_ANYKEY"),
                Localization.ParseAuto($"TITLE_THANKS"),
                $"{Localization.ParseAuto($"TITLE_BUILDVERSION")} {Application.version}"
            };

            StartCoroutine(CycleThroughText());
        }

        private IEnumerator CycleThroughText()
        {
            while (true)
            {
                _textIndex = (_textIndex + 1) % textList.Count;
                anyKeyPressed.text = textList[_textIndex];
                yield return new WaitForSeconds(3);
            }
            
            // ReSharper disable once IteratorNeverReturns
        }

        public void ChangeScene()
        {
            SceneSwapper.LoadScene("ModeSelection");
        }
    }
}
