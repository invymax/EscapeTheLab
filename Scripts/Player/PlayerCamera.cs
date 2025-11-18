using UnityEngine;

[DisallowMultipleComponent]
public class PlayerCamera : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Трансформ, который вращается только по Y (обычно parent/ориентация игрока)")]
    public Transform orientation;

    [Header("Sensitivity")]
    [Tooltip("Чувствительность по X (горизонталь).")]
    public float sensitivityX = 200f;
    [Tooltip("Чувствительность по Y (вертикаль).")]
    public float sensitivityY = 200f;
    [Tooltip("Умножитель чувствительности при быстром движении мыши (ускорение).")]
    public float acceleration = 1f;
    [Tooltip("Включить ускорение (ускоряет при больших смещениях мыши)")]
    public bool useAcceleration = false;

    [Header("Smoothing")]
    [Tooltip("Включить сглаживание входа (плавность).")]
    public bool useSmoothing = true;
    [Tooltip("Время сглаживания в секундах. Меньше = тверже.")]
    [Range(0.001f, 0.5f)]
    public float smoothing = 0.05f;

    [Header("Limits & Options")]
    [Tooltip("Инвертировать вертикальную ось (Y).")]
    public bool invertY = false;
    [Tooltip("Минимальный угол по вертикали (в градусах, отрицательный вниз).")]
    public float minPitch = -80f;
    [Tooltip("Максимальный угол по вертикали (в градусах).")]
    public float maxPitch = 80f;

    [Header("Input axes (editable)")]
    public string axisX = "Mouse X";
    public string axisY = "Mouse Y";

    [Header("Runtime control")]
    [Tooltip("Если false — камера игнорирует ввод мыши (полезно при открытом UI).")]
    public bool inputEnabled = true;

    // внутреннее состояние
    private float pitch = 0f; // vertical rotation (x rotation)
    private float yaw = 0f;   // horizontal rotation (y rotation)

    private Vector2 currentVelocity = Vector2.zero;
    private Vector2 currentSmoothed = Vector2.zero;

    // cursor state
    private bool cursorLocked = true;

    private void Start()
    {
        // инициализация углов по текущему состоянию
        Vector3 e = transform.eulerAngles;
        pitch = e.x;
        yaw = orientation != null ? orientation.eulerAngles.y : e.y;

        SetCursorLock(true);
    }

    private void Update()
    {
        // Обрабатываем переключение курсора (Esc и возможный клик)
        // HandleCursorToggle проверяет Escape всегда, а авто‑блокирование по клику — только если inputEnabled == true.
        HandleCursorToggle();

        // Если ввод отключен — не читаем движения мыши (но оставляем возможность отключать/включать курсор через Esc)
        if (!inputEnabled) return;

        // Получаем вход (мышь / контроллеры, если настроены)
        float rawX = Input.GetAxisRaw(axisX);
        float rawY = Input.GetAxisRaw(axisY);

        // Нормализуем для одинакового поведения при разном framerate
        Vector2 rawInput = new Vector2(rawX, rawY);

        // Преобразуем чувствительность в дельту (с учётом Time.deltaTime)
        Vector2 targetDelta = new Vector2(rawInput.x * sensitivityX, rawInput.y * sensitivityY) * Time.deltaTime;

        // Применяем опциональное ускорение: сильное движение => немного увеличенная чувствительность
        if (useAcceleration)
        {
            float speedFactor = Mathf.Clamp(rawInput.magnitude * 10f, 1f, 3f); // настройка «агрессии»
            targetDelta *= Mathf.Lerp(1f, acceleration, (speedFactor - 1f) / 2f);
        }

        // Сглаживание входа
        if (useSmoothing)
        {
            // Плавно приближаемся к целевой дельте
            currentSmoothed = Vector2.SmoothDamp(currentSmoothed, targetDelta, ref currentVelocity, smoothing);
        }
        else
        {
            currentSmoothed = targetDelta;
            currentVelocity = Vector2.zero;
        }

        // Инверсия Y, аккуратно применяем к накоплению угла (pitch)
        float deltaYaw = currentSmoothed.x;
        float deltaPitch = currentSmoothed.y * (invertY ? 1f : -1f);

        // Накопление углов
        yaw += deltaYaw;
        pitch += deltaPitch;

        // Ограничение вертикального угла
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // Применяем поворот:
        // - камера (этот transform) получает pitch (X) и yaw (Y)
        // - orientation (игрок) получает только yaw (чтобы тело поворачивалось вместе с камерой)
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);

        if (orientation != null)
        {
            Vector3 oEuler = orientation.eulerAngles;
            orientation.rotation = Quaternion.Euler(oEuler.x, yaw, oEuler.z);
        }
    }

    private void HandleCursorToggle()
    {
        // Toggle lock with Escape — всегда реагируем, чтобы игрок мог закрыть UI
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SetCursorLock(!cursorLocked);
        }

        // If cursor unlocked and player clicks left mouse, lock again — но только если ввод камеры включён.
        // Это предотвращает автоматическое перепривязывание курсора при клике по UI, когда мы отключили ввод.
        if (!cursorLocked && inputEnabled && Input.GetMouseButtonDown(0))
        {
            SetCursorLock(true);
        }
    }

    /// <summary>
    /// Lock/unlock cursor and update visibility.
    /// Use this from UI (InventoryUI) to open/close cursor control.
    /// </summary>
    public void SetCursorLock(bool locked)
    {
        cursorLocked = locked;
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    /// <summary>
    /// Convenience: enable/disable camera input and (optionally) force cursor state.
    /// </summary>
    public void EnableInput(bool enabled, bool keepCursorLocked = true)
    {
        inputEnabled = enabled;
        if (!keepCursorLocked)
            SetCursorLock(false);
    }

    /// <summary>
    /// Возвращает текущее состояние блокировки курсора.
    /// </summary>
    public bool IsCursorLocked() => cursorLocked;

    // Optional helper to set sensitivity at runtime
    public void SetSensitivity(float x, float y)
    {
        sensitivityX = x;
        sensitivityY = y;
    }
}