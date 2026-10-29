using System.Collections;
using UnityEngine;

namespace PisoEsLava
{
    // Plataforma que se desmorona (GDD tabla 4): al pisarla se pone anaranjada (el aviso),
    // despues desaparece, y al rato vuelve para que el nivel nunca quede sin salida.
    [RequireComponent(typeof(BoxCollider2D))]
    public class CrumblingPlatform : MonoBehaviour
    {
        [Tooltip("Segundos que esta anaranjada (el aviso) desde que se pisa hasta que desaparece")]
        public float warningTime = 1f;
        [Tooltip("Segundos que tarda en volver a aparecer")]
        public float respawnTime = 3f;
        [Tooltip("Color de aviso mientras aguanta")]
        public Color warningColor = new Color(1f, 0.6f, 0.4f);

        bool warning;
        bool collapsed;
        BoxCollider2D area;
        SpriteRenderer[] parts;

        void Awake()
        {
            area = GetComponent<BoxCollider2D>();
            parts = GetComponentsInChildren<SpriteRenderer>();
        }

        void OnCollisionStay2D(Collision2D collision)
        {
            // Unity avisa de los choques aunque el script este desactivado
            if (!enabled || warning || collapsed || collision.collider.GetComponentInParent<PlayerController>() == null)
            {
                return;
            }
            // Solo cuenta si el jugador esta de pie encima (no si choca por un lado o por debajo)
            if (collision.rigidbody != null && collision.rigidbody.position.y >= area.bounds.max.y - 0.1f)
            {
                StartCoroutine(Crumble());
            }
        }

        IEnumerator Crumble()
        {
            // Aviso: se pone anaranjada y todavia aguanta
            warning = true;
            SetColor(warningColor);
            yield return new WaitForSeconds(warningTime);

            // Desaparece: el jugador cae
            warning = false;
            collapsed = true;
            area.enabled = false;
            SetVisible(false);
            yield return new WaitForSeconds(respawnTime);

            // Vuelve como al principio
            SetColor(Color.white);
            SetVisible(true);
            area.enabled = true;
            collapsed = false;
        }

        void SetColor(Color color)
        {
            foreach (SpriteRenderer part in parts)
            {
                part.color = color;
            }
        }

        void SetVisible(bool visible)
        {
            foreach (SpriteRenderer part in parts)
            {
                part.enabled = visible;
            }
        }
    }
}
