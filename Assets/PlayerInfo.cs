using System;
using UnityEngine;

namespace Assets
{
    [Serializable]
    public class PlayerInfo
    {
        public KeyCode MoveRight;
        public KeyCode MoveLeft;
        public KeyCode MoveUp;
        public KeyCode MoveDown;
        public Vector2Int SpawningPosition;
        public Color SnakeColor;
    }
}
