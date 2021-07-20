using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
namespace Project.Scripts
{
    public class GenerateWorld : MonoBehaviour
    {
        public static GameObject DummyTraveller;
        public static GameObject LastPlatform;
        public const float scale=0.1f;
        public static void RunDummy()
        {
            var p = Pool.Singleton.GetRandomItem();
            if (p == null) return;

            var player = PlayerController.Player;
            if (LastPlatform != null)
            {
                var moveDistance = 10 * scale;
                if (LastPlatform.gameObject.CompareTag("platformTSection"))
                {
                    moveDistance = 20 * scale;
                }

                DummyTraveller.transform.position = LastPlatform.transform.position +
                                                    player.transform.forward * moveDistance;

                if (LastPlatform.CompareTag("stairsUp"))
                {
                    DummyTraveller.transform.Translate(0, 5 * scale, 0);
                }
            }

            LastPlatform = p;

            p.transform.position = DummyTraveller.transform.position;
            p.transform.rotation = DummyTraveller.transform.rotation;

            if (p.CompareTag("stairsDown"))
            {
                DummyTraveller.transform.Translate(0, -5 * scale, 0);
                p.transform.Rotate(0, 180, 0);
                p.transform.position = DummyTraveller.transform.position;
            }

            p.SetActive(true);
            Instantiate(p, p.transform.position, p.transform.rotation);
        }

        private void Awake()
        {
            DummyTraveller = new GameObject("dummy");
        }
    }
}
