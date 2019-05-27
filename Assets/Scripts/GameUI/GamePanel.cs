using System.Collections;
using Config;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameUI
{
    public class GamePanel : Panel
    {
        [SerializeField, LabelText("随机泡泡")]
        private RandomBubble _randomBubble;

        [SerializeField, LabelText("待发射的泡泡")]
        private WaitingBubble _waitingBubble;

        [Button]
        private void SpawnRandomBubble()
        {
            _randomBubble.Respawn();
        }

        [Button]
        public void SpawnWaitBubble()
        {
            if (_randomBubble.BubbType == BubbType.Empty)
                _randomBubble.Respawn();

            StartCoroutine(WaitAndSpawnRandomBubb());

            IEnumerator WaitAndSpawnRandomBubb()
            {
                yield return _randomBubble.PlayMoveAnim();
                yield return _waitingBubble.Respawn(_randomBubble.BubbType);

                SpawnRandomBubble();
            }
        }
    }
}