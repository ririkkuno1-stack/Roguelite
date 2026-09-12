using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

namespace TPSRoguelite.UI
{

    public class ResultView : MonoBehaviour 
    {
        [SerializeField] private TextMeshProUGUI resultText;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button returnToTitleButton;

        public event UnityAction OnRetryAction;
        public event UnityAction OnReturnToTitleActiom;

        private void Awake()
        {
            if (retryButton != null)
            {
                retryButton.onClick.AddListener(() => OnRetryAction?.Invoke());
               
            }
            if (returnToTitleButton != null)
            {
                returnToTitleButton.onClick.AddListener(() => OnReturnToTitleActiom?.Invoke());
            }

            
        }
        public void SetResultText(string text)
        {
            if (resultText != null)
            { 
                resultText.text = text;
            }
        }
    }
}