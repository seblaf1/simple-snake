#nullable enable
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets
{
    public class Snake
    {
        public bool IsDead { get; private set; }

        private readonly List<Vector2Int> Body;
        private readonly Color Color;
        private Vector2Int CurrentDirection;
        private Vector2Int NextDirection;

        private Snake(Color Color)
        {
            this.Body = new List<Vector2Int>();
            this.Color = Color;
            this.CurrentDirection = Vector2Int.right;
            this.NextDirection = Vector2Int.right;
        }

        /// <summary>
        /// Gets the size of this snake.
        /// </summary>
        public int Size => this.Body.Count;

        // TODO: [AI] easy go right command
        public void GoRight()
        {
            // Can't go in opposite direction.
            if (this.CurrentDirection == Vector2Int.left)
                return;

            this.NextDirection = Vector2Int.right;
        }

        // TODO: [AI] easy go left command
        public void GoLeft()
        {
            // Can't go in opposite direction.
            if (this.CurrentDirection == Vector2Int.right)
                return;

            this.NextDirection = Vector2Int.left;
        }

        // TODO: [AI] easy go up command
        public void GoUp()
        {
            // Can't go in opposite direction.
            if (this.CurrentDirection == Vector2Int.down)
                return;

            this.NextDirection = Vector2Int.up;
        }

        // TODO: [AI] easy go down command
        public void GoDown()
        {
            // Can't go in opposite direction.
            if (this.CurrentDirection == Vector2Int.up)
                return;

            this.NextDirection = Vector2Int.down;
        }

        /// <summary>
        /// Spawns a new Snake at coordinates [X, Y] on the provided Board.
        /// </summary>
        public static Snake SpawnAt(Board Board, int X, int Y, Color Color)
        {
            var Snake = new Snake(Color);
            var Head = new Vector2Int(X, Y);

            Snake.Body.Add(Head);

            Cell Cell = Board.GetCell(X, Y)!;
            Cell.SetTypeAndColor(CellType.SNAKE, Snake.Color);

            return Snake;
        }

        /// <summary>
        /// Moves the snake on the provided grid. Called on game tick.
        /// TODO: To handle replays, have this method return the movement that was made and store it.
        /// </summary>
        public void Move(Board Board)
        {
            this.CurrentDirection = this.NextDirection;
            Vector2Int HeadPosition = this.Body[0] + this.CurrentDirection;
            var HasEaten = false;

            if (!Board.IsOnBoard(HeadPosition))
            {
                Debug.Log("Player lost by leaving the grid.");
                this.Kill(Board);
                return;
            }

            if (Board.IsAppleAt(HeadPosition))
            {
                Board.EatApple();
                HasEaten = true;
            }

            else if (!Board.IsEmptyAt(HeadPosition))
            {
                Debug.Log("Player lost by colliding with a snake.");
                this.Kill(Board);
                return;
            }

            this.Body.Insert(0, HeadPosition);
            
            Board.GetCell(HeadPosition)!.SetTypeAndColor(CellType.SNAKE, this.Color);

            if (HasEaten)
                return;

            // If the snake has not eaten, remove the tail (essentially moving the snake).
            Vector2Int TailPosition = this.Body.Last();
            this.Body.RemoveAt(this.Body.Count - 1);
            
            Board.GetCell(TailPosition)!.SetTypeAndColor(CellType.EMPTY, Board.EmptyCellColor);
        }

        private void Kill(Board Board)
        {
            this.IsDead = true;

            // Upon dying, set all cells of the current player's snake to empty.
            foreach (Vector2Int Position in this.Body)
            {
                Board.GetCell(Position)!.SetTypeAndColor(CellType.EMPTY, Board.EmptyCellColor);
            }
        }
    }
}
