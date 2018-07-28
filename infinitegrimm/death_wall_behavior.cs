using System.Collections;
using GlobalEnums;
using UnityEngine;

namespace infinitegrimm
{
    public class death_wall_behavior : MonoBehaviour
    {
        private const float FADE_LENGTH = 8f;
        private SpriteRenderer cachedSpriteRenderer;
        
        private void Start()
        {
            cachedSpriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            StartCoroutine(fadeInWall());
            log("Started deathwall thingy.");
        }

        private IEnumerator fadeInWall()
        {
            Rigidbody2D physMeme = gameObject.AddComponent<Rigidbody2D>();
            physMeme.isKinematic = true;
            physMeme.gravityScale = 0;
            BoxCollider2D hitHero = gameObject.AddComponent<BoxCollider2D>();
            hitHero.size = cachedSpriteRenderer.bounds.size;
            hitHero.isTrigger = true;
            hitHero.offset = new Vector2(0, cachedSpriteRenderer.size.y / 2);
            
            Color cachedClr = cachedSpriteRenderer.color;
            for (float time = 0; time < FADE_LENGTH; time += Time.deltaTime)
            {
                cachedClr.a = time / FADE_LENGTH;
                cachedSpriteRenderer.color = cachedClr;
                yield return null;
            }
            cachedSpriteRenderer.color = Color.white;

            
            DamageHero h = gameObject.AddComponent<DamageHero>();

            h.damageDealt = 2;
            h.hazardType = (int) HazardType.ACID;
            h.shadowDashHazard = true;
            h.resetOnEnable = false;
            h.enabled = true;
            log("Deathwalls now doing damage. GLHF.");
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            HeroController.instance.SetHazardRespawn(new Vector3(80f, 6.6f), true);   
        }

        private static void log(string str)
        {
            Modding.Logger.Log("[Infinite Grimm] " + str);
        }
    }
}