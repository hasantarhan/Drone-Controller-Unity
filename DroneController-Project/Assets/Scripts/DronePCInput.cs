using System;
using UnityEngine;

public class DronePCInput : DroneInputBase
{
    private GameInput gameInput;
    private void Awake()
    {
        gameInput = new GameInput();
        gameInput.Enable();
    }
    public override Vector2 Look()
    {
        return gameInput.Player.Look.ReadValue<Vector2>();
    }
    public override Vector2 Move()
    {
        return gameInput.Player.Move.ReadValue<Vector2>();
    }
    public override float Lock()
    {
        return gameInput.Player.Lock.ReadValue<float>();
    }
    public override float DashInput()
    {
        return gameInput.Player.Dash.ReadValue<float>();
    }
    public override float Up()
    {
        return gameInput.Player.Up.ReadValue<float>();
    }
    public override float Down()
    {
        return gameInput.Player.Down.ReadValue<float>();
    }
}