#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Random = System.Random;

// ReSharper disable CollectionNeverUpdated.Local, IT'S A SERIALIZE FIELD! 
#pragma warning disable CS8618 // AWAKE INSTEAD OF CONSTRUCTOR
#pragma warning disable CS0649 // SERIALIZE FIELD

namespace Assets
{
    public class Board : MonoBehaviour
    {
        // TODO: To make the board infinite, we have 2 options I can think of:
        //  1-  Make the game multiplayer across a network, this way we can have a sort of "fog-of-war" where the cells
        //      are only there for rendering. I.e. we keep the player close to the middle and keep an offset between
        //      the rendering cells coordinates and the "world" coordinates. If something is in the world, but outside the
        //      bounds of the rendering cells, we simply do not render it.
        //
        //  2-  Make the camera zoom out and focus on different things. I have a game that does something similar, but I didn't
        //      want to bother with camera code for this simple project.
        //
        [SerializeField, Tooltip("Size of the game board")]
        private int BoardSize;

        [SerializeField, Tooltip("Distance between cells")]
        private float BoardSpacing;

        [SerializeField, Tooltip("Default color of the board cells.")]
        private Color EmptyColor = Color.white;

        [SerializeField, Tooltip("Color of a cell when an Apple is on it.")]
        private Color AppleColor = Color.red;

        [SerializeField, Tooltip("The amount of seconds to wait in between each game tick.")]
        private float SecondsToWaitBetweenTicks = 0.25f;

        // Adding N amount of players is already possible
        [SerializeField, Tooltip("Players to spawn into the game.")]
        private List<PlayerInfo> Players;

        public bool HasGameEnded { get; private set; }

        // Returns the color of an empty cell.
        public Color EmptyCellColor => this.EmptyColor;

        private readonly Dictionary<KeyCode, Action> KeyBindings = new();
        private readonly List<Snake> Snakes = new();
        private Vector2Int? Apple;
        private Cell[,] Cells;
        private Timer TickTimer;

        public void EatApple() => this.Apple = null;

        public bool IsOnBoard(Vector2Int Position) => this.IsOnBoard(Position.x, Position.y);
        public bool IsOnBoard(int X, int Y)
        {
            return X >= 0 && X < this.BoardSize
                && Y >= 0 && Y < this.BoardSize;
        }

        public bool IsAppleAt(Vector2Int Position) => this.IsAppleAt(Position.x, Position.y);
        public bool IsAppleAt(int X, int Y)
        {
            if (!this.IsOnBoard(X, Y))
            {
                Debug.LogError("Called IsAppleAt using coordinates that were not on the Board.");
                return false;
            }

            return this.Cells[X, Y].Type == CellType.APPLE;
        }

        public bool IsEmptyAt(Vector2Int Position) => this.IsEmptyAt(Position.x, Position.y);
        public bool IsEmptyAt(int X, int Y)
        {
            if (!this.IsOnBoard(X, Y))
            {
                Debug.LogError("Called IsEmptyAt using coordinates that were not on the Board.");
                return false;
            }

            return this.Cells[X, Y].Type == CellType.EMPTY;
        }

        public Snake? SpawnSnakeAt(int X, int Y, Color SnakeColor)
        {
            if (!this.IsOnBoard(X, Y))
            {
                Debug.LogError("Attempting to spawn a Snake with a Coordinate that's outside the grid.");
                return null;
            }

            if (this.Cells[X, Y].Type != CellType.EMPTY)
            {
                Debug.LogError("Attempting to spawn a Snake on a cell that's not EMPTY.");
                return null;
            }

            Snake SpawnedSnake = Snake.SpawnAt(this, X, Y, SnakeColor);
            this.Snakes.Add(SpawnedSnake);
            return SpawnedSnake;
        }

        public Cell? GetCell(Vector2Int Position) => this.GetCell(Position.x, Position.y);
        public Cell? GetCell(int X, int Y)
        {
            if (!this.IsOnBoard(X, Y))
                return null;

            Cell Cell = this.Cells[X, Y];
            return Cell;
        }

        private void Awake()
        {
            Assert.IsTrue(this.BoardSize > 1, "Board size must be bigger than 1.");

            this.TickTimer = Timer.FromSeconds(this.SecondsToWaitBetweenTicks);
            this.InitializeCells();
            this.InitializePlayers();
            this.SpawnApple();
        }

        private void Update()
        {
            if (this.HasGameEnded)
                return;

            this.ReadInputs();
        }

        private void FixedUpdate()
        {
            if (this.HasGameEnded || !this.TickTimer.Elapsed())
                return;

            this.TickTimer.Start();
            this.Tick();
        }

        private void Tick()
        {
            var HasGameJustEnded = true; // true by default, for if there are no snakes
            var TotalSnakeCells = 0;

            foreach (Snake Snake in this.Snakes)
            {
                if (!Snake.IsDead)
                {
                    Snake.Move(this);
                    TotalSnakeCells += Snake.Size;
                }

                HasGameJustEnded &= Snake.IsDead;
            }

            if (HasGameJustEnded)
            {
                Debug.Log("Game has ended. All players are dead.");
                this.HasGameEnded = true;
                return;
            }

            // The size of the board is always BoardSize^2, so if the combined sizes of all snakes that are alive
            // is equal or bigger to that, then the players have won.
            if (TotalSnakeCells >= this.BoardSize * this.BoardSize)
            {
                Debug.Log("Players have won !");
                this.HasGameEnded = true;
                return;
            }

            if (this.Apple == null)
            {
                this.SpawnApple();
            }
        }

        private void ReadInputs()
        {
            // Because of how I read inputs, the last key inside the dictionary is the one which will take precedence over all others
            // in the case a player has multiple keys held down at the same time. I decide to ignore this issue for this simple game.

            // Todo: To change control schemes easily, we could use the new Unity Input System API. For simplicity, as I didn't want to 
            // bother with non programmatic problems, I went with an event-driven pattern using the old unity input system. Being event-driven,
            // it's easy to change.

            foreach ((KeyCode Key, Action Binding) in this.KeyBindings)
            {
                if (Input.GetKeyDown(Key))
                    Binding.Invoke();
            }
        }

        private void InitializeCells()
        {
            this.Cells = new Cell[this.BoardSize, this.BoardSize];

            for (var X = 0; X < this.BoardSize; ++X)
            {
                for (var Y = 0; Y < this.BoardSize; ++Y)
                {
                    var CellObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    CellObject.transform.SetParent(this.transform);
                    CellObject.name = $"CELL [X={X}, Y={Y}]";

                    var Cell = new Cell(CellObject);
                    Cell.SetPosition(X, Y, this.BoardSpacing);
                    Cell.SetScale(this.BoardSpacing);

                    // By default, cells are all empty.
                    Cell.SetColor(this.EmptyColor);

                    this.Cells[X, Y] = Cell;
                }
            }
        }

        private void InitializePlayers()
        {
            foreach (PlayerInfo Player in this.Players)
            {
                // Spawn a Snake at the provided position.
                Snake? Snake = this.SpawnSnakeAt(Player.SpawningPosition.x, Player.SpawningPosition.y, Player.SnakeColor);
                if (Snake == null)
                {
                    Debug.LogWarning("Could not spawn player. Skipping bindings registration.");
                    return;
                }

                // Register keybindings.
                this.RegisterKeyBinding(Player.MoveRight, () => Snake.GoRight());
                this.RegisterKeyBinding(Player.MoveLeft, () => Snake.GoLeft());
                this.RegisterKeyBinding(Player.MoveUp, () => Snake.GoUp());
                this.RegisterKeyBinding(Player.MoveDown, () => Snake.GoDown());
            }
        }

        private void RegisterKeyBinding(KeyCode Key, Action Binding)
        {
            // Prevent multiple players from using the same key.
            if (this.KeyBindings.ContainsKey(Key))
            {
                Debug.LogError($"Key for {Key} was already registered.");
                return;
            }

            this.KeyBindings.Add(Key, Binding);
        }

        private void SpawnApple()
        {
            Vector2Int ApplePosition = this.CalculateNextApplePosition();
            this.Apple = ApplePosition;
            this.GetCell(ApplePosition)!.SetTypeAndColor(CellType.APPLE, this.AppleColor);
        }

        private Vector2Int CalculateNextApplePosition()
        {
            var Random = new Random(); // Could add a seed to reproduce snake games across machines !

            // iterates until a valid position is found. therefore, it's important to verify game ending conditions before
            // calculating apples, else there might be no valid positions left and the game would freeze.

            while (true) 
            {
                int X = Random.Next(0, this.BoardSize);
                int Y = Random.Next(0, this.BoardSize);

                if (this.IsEmptyAt(X, Y))
                    return new Vector2Int(X, Y);
            }
        }
    }
}
