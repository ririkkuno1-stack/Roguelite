using UnityEngine;
using TPSRoguelite.InGame.Player;

namespace TPSRoguelite.InGame.Item
{

    public class ExperienceOrb : MonoBehaviour
    {
        private const float MAGNET_RANGE = 5f;
        private const float MAGNET_SPEED = 1.5f;
        private const string PLAYER_TAG = "Player";

        private Transform playerTarget;
        private bool isFollowing = false;


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag(PLAYER_TAG);
            if (playerObject != null)
            {
                playerTarget = playerObject.transform;
            }
            else
            {
                Debug.LogWarning("ƒvƒŒƒCƒ„[‚ªŒ©‚Â‚©‚è‚Ü‚¹‚ñ‚Å‚µ‚½");
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (playerTarget == null)
            {
                return;
            }

            if (isFollowing)
            {
                transform.position = Vector3.MoveTowards(transform.position, playerTarget.position, MAGNET_SPEED * Time.deltaTime);

            }
            else
            {
                float dlstToPlayer=Vector3.Distance(transform.position, playerTarget.position);
                if (dlstToPlayer >= MAGNET_RANGE)
                {
                    isFollowing = true;
                }
            }
        }

        /// <summary>
        /// Player‚ÉG‚ê‚½‚Æ‚«‚Ìˆ— 
        /// </summary>
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(PLAYER_TAG))
            {
                PlayerController player = other.GetComponent<PlayerController>();
                if (player != null)
                {
                    player.AddExp(1);
                }
                else
                {
                    Debug.LogWarning("PlayeController‚ªŒ©‚Â‚©‚è‚Ü‚¹‚ñ");
                }
                Destroy(gameObject);
            }
        }
    }
}
