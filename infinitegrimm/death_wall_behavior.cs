using System.Collections;
using GlobalEnums;
using UnityEngine;

namespace infinitegrimm
{
    public class death_wall_behavior : MonoBehaviour
    {
        private const float FADE_LENGTH = 8f;
        private SpriteRenderer cachedSpriteRenderer;

        private const float WALL_IFRAMES = 3.0f;
        private bool isInvulnerable = false;
        private bool doingDamage = false;
        
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
            doingDamage = true;
            log("Deathwalls now doing damage. GLHF.");
        }

        private IEnumerator invulnerabilityFrames()
        {
            log("Making player invulnerable for a few seconds");
            isInvulnerable = true;
            yield return null;
            for (float time = 0; time < WALL_IFRAMES; time += Time.deltaTime)
            {
                if (!HeroController.instance.cState.shadowDashing)
                    PlayerData.instance.isInvincible = true;
                yield return null;
            }

            isInvulnerable = false;
            if (!HeroController.instance.cState.shadowDashing)
            {
                PlayerData.instance.isInvincible = false;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.name != "HeroBox" || !doingDamage || isInvulnerable) return;
            HeroController.instance.SetHazardRespawn(new Vector3(80f, 6.6f), true);
            StartCoroutine(invulnerabilityFrames());
        }

        private static void log(string str)
        {
            Modding.Logger.Log("[Infinite Grimm] " + str);
        }
    }
}