using System;
using System.Collections;
using System.Collections.Generic;
using Scripts.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.Gameplay.Views
{
    public class GameFieldView : MonoBehaviour
    {
        [Serializable]
        public class ImageAndMore
        {
            public RawImage image = null;
            public Button button = null;
            [HideInInspector, NonSerialized] public int id = 0;
            [HideInInspector, NonSerialized] public int paired = -1;
            [HideInInspector, NonSerialized] public int currentlyChecking = -1;
        }

        private enum GameState
        {
            Prepare,
            Play,
            End
        }
        public event Action<bool> OnEndGame;
        [SerializeField] private List<ImageAndMore> allImages = new List<ImageAndMore>();
        [SerializeField] private float prepareTime = 7f;
        [SerializeField] private float gameTime = 60f;
        [SerializeField] private TMPro.TMP_Text text = null;

        private ImageAndMore first = null;
        private GameState gameState = GameState.Prepare;
        private int currentlyGuessed = 0;

        [SerializeField] private AnimationClip clipToPLay = null;

        private WaitForSeconds waiter = new WaitForSeconds(1f);

        public void Loading()
        {
            text.text = "Loading";
            foreach (var item in allImages)
            {
                item.button.interactable = false;
            }
        }

        private void AnimationOn(RawImage imageToUse, bool forward)
        {
            StartCoroutine(playAnimation(imageToUse, forward));
        }

        private IEnumerator playAnimation(RawImage imageToUse, bool forward)
        {
            float multiplyer = forward ? 1 : -1;
            float lengthOfCLip = clipToPLay.length;

            float initial = forward ? 0 : clipToPLay.length;
            float timeForAnim = initial;

            if (forward)
            {
                while (timeForAnim < lengthOfCLip)
                {
                    timeForAnim += Time.deltaTime * multiplyer;
                    clipToPLay.SampleAnimation(imageToUse.gameObject, timeForAnim);
                    yield return null;
                }
            }
            else
            {
                while (timeForAnim > 0)
                {
                    timeForAnim += Time.deltaTime * multiplyer;
                    clipToPLay.SampleAnimation(imageToUse.gameObject, timeForAnim);
                    yield return null;
                }
            }

            clipToPLay.SampleAnimation(imageToUse.gameObject, forward ? clipToPLay.length : 0);

        }

        public void Initialize(Dictionary<string, Texture2D> allTextures)
        {
            Generate(allTextures);
            var time = GameTimers.GetNowTimestampSeconds();
            // Callback hell - we can use promises or async instead
            GameTimers.AddNewTimer(gameState.ToString(), time, time + (long)prepareTime,
            (currentTimer) =>
            {
                gameState = GameState.Play;
                var time2 = GameTimers.GetNowTimestampSeconds();
                Play();

                GameTimers.AddNewTimer(gameState.ToString(), time, time + (long)gameTime,
                            (currentTimer2) =>
                            {
                                gameState = GameState.End;
                                Clear();
                                // Show cards
                            },
                            (timeLeft2) =>
                            {
                                text.text = timeLeft2.ToString();
                            }
                );
            },
            (timeLeft) =>
            {
                text.text = timeLeft.ToString();
            }
            );

            // remove interactable from buttons, activate on Play state
            Clear();
        }

        private void Generate(Dictionary<string, Texture2D> allTextures)
        {
            int length = allImages.Count;
            var shuffled = RandomExtension.ShuffledArray(length);
            List<string> namesOfTextures = new List<string>(allTextures.Keys);

            Texture2D texture = null;
            for (int i = 0, j = 0; i < length; i += 2, j++)
            {
                texture = allTextures[namesOfTextures[j]];

                allImages[shuffled[i]].id = shuffled[i];
                allImages[shuffled[i + 1]].id = shuffled[i + 1];

                allImages[shuffled[i]].image.texture = texture;
                allImages[shuffled[i + 1]].image.texture = texture;

                allImages[shuffled[i]].paired = allImages[shuffled[i + 1]].id;
                allImages[shuffled[i + 1]].paired = allImages[shuffled[i]].id;
            }
            texture = null;

            foreach (var item in allImages)
            {
                AnimationOn(item.image, true);
            }

            foreach (var item in allImages)
            {

                item.button.onClick.AddListener(() =>
                {
                    var myself = item; // clojure from item
                    AnimationOn(myself.image, true);
                    if (first == null)
                    {
                        first = myself;
                    }
                    else
                    {
                        if (myself.paired == first.id)
                        {
                            RightGuess(new ValueTuple<int, int>(myself.id, first.id));

                        }
                        else
                        {
                            WrongGuess(new ValueTuple<int, int>(myself.id, first.id));
                        }
                        first = null;
                    }
                });
            }
        }

        private void Play()
        {
            foreach (var item in allImages)
            {
                item.button.interactable = true;
            }

            foreach (var item in allImages)
            {
                AnimationOn(item.image, false);
            }
        }

        private void Clear()
        {
            foreach (var item in allImages)
            {
                item.button.interactable = false;
            }

            foreach (var item in allImages)
            {
                AnimationOn(item.image, true);
            }
        }

        private void RightGuess(ValueTuple<int, int> paired)
        {
            allImages[paired.Item1].button.interactable = false;
            allImages[paired.Item2].button.interactable = false;

            allImages[paired.Item1].image.gameObject.SetActive(false);
            allImages[paired.Item2].image.gameObject.SetActive(false);
            currentlyGuessed += 2;
            if (currentlyGuessed >= allImages.Count)
            {
                var toStop = GameTimers.GetTimer(gameState.ToString());
                toStop.Stop();
                OnEndGame?.Invoke(true);
            }
        }

        private void WrongGuess(ValueTuple<int, int> paired)
        {
            allImages[paired.Item1].currentlyChecking = -1;
            allImages[paired.Item2].currentlyChecking = -1;
            StartCoroutine(waitForSec(paired));
        }

        private IEnumerator waitForSec(ValueTuple<int, int> paired)
        {
            yield return waiter;
            AnimationOn(allImages[paired.Item1].image, false);
            AnimationOn(allImages[paired.Item2].image, false);
        }
    }
}