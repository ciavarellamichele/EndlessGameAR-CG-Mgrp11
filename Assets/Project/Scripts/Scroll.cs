using UnityEngine;

namespace Project.Scripts
{
    public class Scroll : MonoBehaviour
    {
        private GameObject _player;

        private void Start()
        {
            _player = PlayerController.Player;
        }

        private void FixedUpdate()
        {
            if (PlayerController.Dead) return;

            const float speed = -0.1f * GenerateWorld.scale;
            transform.position += _player.transform.forward * speed;

            var currentPlatform = PlayerController.CurrentPlatform;
            if (currentPlatform == null) return;

            const float stairSlope = 0.06f * GenerateWorld.scale;
            if (currentPlatform.CompareTag("stairsUp"))
            {
                // Stairs are at a 60 degree angle.
                // For every one step forward, move the "world" 6 steps down.
                transform.Translate(0, -stairSlope, 0);
                PlayerController._cubeOutherspace.transform.Translate(0, 0.006f * GenerateWorld.scale * Time.deltaTime, 0, relativeTo:Space.Self);
            }
            else if (currentPlatform.CompareTag("stairsDown"))
            {
                // Same logic as above, just in reverse.
                transform.Translate(0, stairSlope, 0);
                PlayerController._cubeOutherspace.transform.Translate(0, -0.006f * GenerateWorld.scale * Time.deltaTime, 0, relativeTo:Space.Self);
            }
        }
    }
}
