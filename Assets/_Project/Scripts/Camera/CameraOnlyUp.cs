using UnityEngine;

namespace PisoEsLava
{
    // Camara que solo sube (GDD 5.1.6, regla 4) y mueve el fondo un poco menos que ella (paralaje)
    [RequireComponent(typeof(Camera))]
    public class CameraOnlyUp : MonoBehaviour
    {
        public Transform target;
        [Tooltip("Cuanto por encima del jugador se coloca el centro de la camara")]
        public float verticalOffset = 3f;
        public float smoothTime = 0.25f;
        public float minY;
        public float maxY = 1000f;

        [Header("Fondo con paralaje")]
        public Transform background;
        [Range(0f, 1f)]
        [Tooltip("1 = el fondo se mueve igual que la camara (parece fijo); 0 = no se mueve")]
        public float backgroundFactor = 0.84f;

        public float BottomY => baseY - cam.orthographicSize;

        Camera cam;
        float baseY;
        // Altura mas alta alcanzada: la camara nunca baja de ella
        float highestY;
        float velocity;
        float startY;
        Vector3 backgroundStart;

        void Awake()
        {
            cam = GetComponent<Camera>();
            baseY = startY = highestY = transform.position.y;
            if (background != null)
            {
                backgroundStart = background.position;
            }
        }

        void LateUpdate()
        {
            if (target != null)
            {
                float desired = Mathf.Clamp(target.position.y + verticalOffset, minY, maxY);
                // Nunca baja: solo se queda con la altura maxima alcanzada
                highestY = Mathf.Max(highestY, desired);
            }

            baseY = Mathf.SmoothDamp(baseY, highestY, ref velocity, smoothTime);
            transform.position = new Vector3(transform.position.x, baseY, transform.position.z);
            if (background != null)
            {
                background.position = backgroundStart + Vector3.up * ((baseY - startY) * backgroundFactor);
            }
        }
    }
}
