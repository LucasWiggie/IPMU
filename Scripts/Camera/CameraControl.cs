using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public float m_DampTime = 0.2f; // tiempo que tarda la cçamara en moverse                
    public float m_ScreenEdgeBuffer = 4f;  //   número que se añade al tamaño de la pantalla para asegurar que los tanques no se salen de esta       
    public float m_MinSize = 6.5f; // tamaño mínimo de zoom que puede hacerse con la cámara          
    [HideInInspector] public Transform[] m_Targets; // array de tanques 


    private Camera m_Camera; //  variable de acceso a la cámara para poder cambiar cosas               
    private float m_ZoomSpeed;                      
    private Vector3 m_MoveVelocity;                 
    private Vector3 m_DesiredPosition;  // posición a la que la cámara quiere llegar
                                        // también es la posición que toma la cámara de referencia al hacer zoom
                                        // Es el punto medio entre los dos tanques

    private void Awake()
    {
        m_Camera = GetComponentInChildren<Camera>();
    }


    private void FixedUpdate()
    {
        Move();
        Zoom();
    }


    private void Move()
    {
        FindAveragePosition();

        // Mover la cámara desde su posición actual a la posición deseada
        transform.position = Vector3.SmoothDamp(transform.position, m_DesiredPosition, ref m_MoveVelocity, m_DampTime);
    }


    private void FindAveragePosition()
    {
        Vector3 averagePos = new Vector3();
        int numTargets = 0;

        for (int i = 0; i < m_Targets.Length; i++)
        {
            // si el tanque no está activo, pasamos a la siguiente iteración del bucle for
            if (!m_Targets[i].gameObject.activeSelf) 
                continue;

            averagePos += m_Targets[i].position;
            numTargets++;
        }

        // si hay tanques ac´tivos, la posición pasará a ser el punto medio entre el número de tanques que haya
        if (numTargets > 0)
            averagePos /= numTargets;

        // asegurar que la cámara no se mueve en el eje y 
        averagePos.y = transform.position.y;

        m_DesiredPosition = averagePos;
    }


    private void Zoom()
    {
        float requiredSize = FindRequiredSize();
        m_Camera.orthographicSize = Mathf.SmoothDamp(m_Camera.orthographicSize, requiredSize, ref m_ZoomSpeed, m_DampTime);
    }


    private float FindRequiredSize()
    {
        Vector3 desiredLocalPos = transform.InverseTransformPoint(m_DesiredPosition);

        float size = 0f;

        for (int i = 0; i < m_Targets.Length; i++)
        {
            if (!m_Targets[i].gameObject.activeSelf)
                continue;

            Vector3 targetLocalPos = transform.InverseTransformPoint(m_Targets[i].position);

            Vector3 desiredPosToTarget = targetLocalPos - desiredLocalPos;

            size = Mathf.Max (size, Mathf.Abs (desiredPosToTarget.y));

            size = Mathf.Max (size, Mathf.Abs (desiredPosToTarget.x) / m_Camera.aspect);
        }
        
        size += m_ScreenEdgeBuffer;

        size = Mathf.Max(size, m_MinSize);

        return size;
    }


    public void SetStartPositionAndSize()
    {
        FindAveragePosition();

        transform.position = m_DesiredPosition;

        m_Camera.orthographicSize = FindRequiredSize();
    }
}