using UnityEngine;

public abstract class DroneInputBase : MonoBehaviour
{
    public abstract Vector2 Look();
    public abstract Vector2 Move();
    public abstract float Lock();
    public abstract float DashInput();
    public abstract float Up();
    public abstract float Down();
}