using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TPSRoguelite.UI
{
    public class TitleModel : MonoBehaviour
    {
        [SerializeField] private Button StartButton;

        public event UnityAction OnRetryAction;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Aweke()
        {
            if (StartButton != null)
            {
                StartButton.onClick.AddListener(() => OnRetryAction?.Invoke());
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
