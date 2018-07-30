using GlobalEnums;
using Modding;
using UnityEngine;
using UnityEngine.UI;

namespace infinitegrimm
{
    public class time_attack : MonoBehaviour
    {
        public static int secondsToRun;
        private float timeRemaining;
        private bool didDestroy = false;
        private Text textObj;
        private GameObject canvas;

        private void Start()
        {
            timeRemaining = (float) secondsToRun;
            
            if (canvas != null) return;
            
            CanvasUtil.CreateFonts();
            canvas = CanvasUtil.CreateCanvas(RenderMode.ScreenSpaceOverlay, new Vector2(1920, 1080));
            GameObject go =
                CanvasUtil.CreateTextPanel(canvas, "", 27, TextAnchor.MiddleCenter,
                    new CanvasUtil.RectData(
                        new Vector2(0, 0),
                        new Vector2(0, 0),
                        new Vector2(0, 0),
                        new Vector2(1.9f, 1.9f),
                        new Vector2(0.5f, 0.5f)));
            
            
            textObj = go.GetComponent<Text>();
            textObj.color = Color.white;
            textObj.font = CanvasUtil.TrajanBold;
            textObj.text = getTimeInCleanFormat((float)secondsToRun);
            textObj.fontSize = 50;
            textObj.CrossFadeAlpha(1f, 0f, false);
        }

        private void Update()
        {
            timeRemaining -= Time.deltaTime;
            if (timeRemaining > 0.0f)
            {
                textObj.text = getTimeInCleanFormat(timeRemaining);
            }
            else if (!didDestroy)
            {
                didDestroy = true;
                Destroy(textObj);
                Destroy(canvas);
                PlayerData.instance.health = -10;
                HeroController.instance.TakeDamage(HeroController.instance.gameObject, CollisionSide.other, 10, 1);
            }
        }


        public static string getTimeInCleanFormat(float time)
        {
            string seconds = (((int) time) % 60).ToString();
            if (seconds.Length == 1)
            {
                seconds = "0" + seconds;
            }
            string minutes = (((int) time) / 60).ToString();
            return (minutes + ":" + seconds);
        }
    }
}