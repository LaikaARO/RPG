using UnityEngine;

/// <summary>
/// Controla a câmera do jogo, permitindo rotação com o botão direito do mouse e zoom com o scroll.
/// Segue o personagem do jogador mantendo uma distância e ângulo configuráveis.
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("Referência ao transform do jogador que a câmera deve seguir")]
    [SerializeField] private Transform target;

    [Header("Configurações de Posição")]
    [Tooltip("Distância inicial da câmera em relação ao alvo")]
    [SerializeField] private float distance = 5.0f;
    [Tooltip("Altura da câmera em relação ao alvo")]
    [SerializeField] private float height = 2.0f;
    [Tooltip("Deslocamento lateral da câmera")]
    [SerializeField] private float horizontalOffset = 0.0f;

    [Header("Configurações de Rotação")]
    [Tooltip("Ângulo inicial de rotação horizontal (em graus)")]
    [SerializeField] private float rotationX = 45.0f;
    [Tooltip("Ângulo inicial de rotação vertical (em graus)")]
    [SerializeField] private float rotationY = 45.0f;
    [Tooltip("Velocidade de rotação da câmera")]
    [SerializeField] private float rotationSpeed = 3.0f;
    [Tooltip("Suavização da rotação da câmera")]
    [SerializeField] private float rotationDamping = 0.2f;

    [Header("Configurações de Zoom")]
    [Tooltip("Distância mínima de zoom")]
    [SerializeField] private float minZoomDistance = 2.0f;
    [Tooltip("Distância máxima de zoom")]
    [SerializeField] private float maxZoomDistance = 10.0f;
    [Tooltip("Velocidade do zoom")]
    [SerializeField] private float zoomSpeed = 1.0f;
    [Tooltip("Suavização do zoom")]
    [SerializeField] private float zoomDamping = 0.2f;

    [Header("Configurações de Colisão")]
    [Tooltip("Camadas que a câmera deve evitar atravessar")]
    [SerializeField] private LayerMask collisionLayers;
    [Tooltip("Raio de colisão da câmera")]
    [SerializeField] private float collisionRadius = 0.2f;

    // Variáveis privadas para controle interno
    private float currentDistance;
    private float targetDistance;
    private float currentRotationX;
    private float currentRotationY;
    private float targetRotationX;
    private float targetRotationY;
    private Vector3 cameraOffset;
    private Vector3 targetPosition;

    private void Start()
    {
        // Se nenhum alvo for definido, tenta encontrar o jogador pela tag
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
            else
            {
                Debug.LogError("CameraController: Nenhum alvo definido e nenhum objeto com tag 'Player' encontrado.");
            }
        }

        // Inicializar variáveis
        currentDistance = distance;
        targetDistance = distance;
        currentRotationX = rotationX;
        currentRotationY = rotationY;
        targetRotationX = rotationX;
        targetRotationY = rotationY;

        // Desabilitar a rotação automática da câmera
        Cursor.lockState = CursorLockMode.None;
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        // Processar entrada do mouse para rotação
        HandleRotationInput();

        // Processar entrada do scroll para zoom
        HandleZoomInput();

        // Suavizar rotação e zoom
        SmoothCameraMovement();

        // Calcular a posição desejada da câmera
        CalculateCameraPosition();

        // Verificar colisões e ajustar a posição da câmera se necessário
        HandleCameraCollision();

        // Aplicar a posição e rotação final
        ApplyCameraTransform();
    }

    /// <summary>
    /// Processa a entrada do mouse para rotação da câmera
    /// </summary>
    private void HandleRotationInput()
    {
        // Rotacionar a câmera apenas quando o botão direito do mouse estiver pressionado
        if (Input.GetMouseButton(1))
        {
            targetRotationX += Input.GetAxis("Mouse X") * rotationSpeed;
            targetRotationY -= Input.GetAxis("Mouse Y") * rotationSpeed;

            // Limitar a rotação vertical para evitar que a câmera vire de cabeça para baixo
            targetRotationY = Mathf.Clamp(targetRotationY, 10f, 80f);
        }
    }

    /// <summary>
    /// Processa a entrada do scroll para zoom da câmera
    /// </summary>
    private void HandleZoomInput()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0)
        {
            targetDistance -= scrollInput * zoomSpeed;
            targetDistance = Mathf.Clamp(targetDistance, minZoomDistance, maxZoomDistance);
        }
    }

    /// <summary>
    /// Suaviza os movimentos de rotação e zoom da câmera
    /// </summary>
    private void SmoothCameraMovement()
    {
        // Suavizar rotação
        currentRotationX = Mathf.Lerp(currentRotationX, targetRotationX, rotationDamping);
        currentRotationY = Mathf.Lerp(currentRotationY, targetRotationY, rotationDamping);

        // Suavizar zoom
        currentDistance = Mathf.Lerp(currentDistance, targetDistance, zoomDamping);
    }

    /// <summary>
    /// Calcula a posição desejada da câmera com base na rotação e distância
    /// </summary>
    private void CalculateCameraPosition()
    {
        // Calcular a rotação da câmera
        Quaternion rotation = Quaternion.Euler(currentRotationY, currentRotationX, 0);

        // Calcular o offset da câmera com base na rotação e distância
        cameraOffset = new Vector3(horizontalOffset, height, -currentDistance);
        cameraOffset = rotation * cameraOffset;

        // Calcular a posição alvo da câmera
        targetPosition = target.position + cameraOffset;
    }

    /// <summary>
    /// Verifica colisões da câmera com objetos no ambiente
    /// </summary>
    private void HandleCameraCollision()
    {
        if (collisionLayers != 0) // Se a máscara de colisão não for zero
        {
            RaycastHit hit;
            Vector3 directionToCamera = targetPosition - target.position;
            float distanceToTarget = directionToCamera.magnitude;

            // Verificar se há colisão entre o alvo e a posição desejada da câmera
            if (Physics.SphereCast(target.position, collisionRadius, directionToCamera.normalized, 
                out hit, distanceToTarget, collisionLayers))
            {
                // Ajustar a posição da câmera para o ponto de colisão
                float collisionDistance = hit.distance;
                targetPosition = target.position + directionToCamera.normalized * (collisionDistance - collisionRadius);
            }
        }
    }

    /// <summary>
    /// Aplica a posição e rotação final à câmera
    /// </summary>
    private void ApplyCameraTransform()
    {
        // Definir a posição da câmera
        transform.position = targetPosition;

        // Fazer a câmera olhar para o alvo
        transform.LookAt(target.position + Vector3.up * height * 0.5f);
    }

    /// <summary>
    /// Define um novo alvo para a câmera seguir
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    /// <summary>
    /// Redefine a rotação da câmera para os valores iniciais
    /// </summary>
    public void ResetRotation()
    {
        targetRotationX = rotationX;
        targetRotationY = rotationY;
    }

    /// <summary>
    /// Redefine o zoom da câmera para o valor inicial
    /// </summary>
    public void ResetZoom()
    {
        targetDistance = distance;
    }
}