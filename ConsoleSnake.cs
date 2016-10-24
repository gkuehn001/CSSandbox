using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CSSandbox
{
    public class ConsoleSnake
    {
        struct Point
        {
            public Point(int _x, int _y) { x = _x; y = _y; }
            public Point Add(Point p) { return new Point(x + p.x, y + p.y); }
            public int x;
            public int y;
        }

        class Rectangle
        {
            public Rectangle(int _x, int _y, int _width, int _height)
            {
                X = _x;
                Y = _y;
                Width = _width;
                Height = _height;
            }

            public int X { get; set; }
            public int Y { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }

            public int Left { get { return X; } }
            public int Right { get { return X + Width; } }
            public int Top { get { return Y; } }
            public int Bottom { get { return Y + Height; } }

            public int CenterX { get { return (Left + Right) / 2; } }
            public int CenterY { get { return (Top + Bottom) / 2; } }
            public Point Center { get { return new Point(CenterX, CenterY); } }
        }

        class Board
        {
            public Board(Rectangle rect)
            {
                Rect = rect;
                freePositions = new HashSet<Point>();
                rnd = new Random();
            }
            public Rectangle Rect { get; }
            public Point Food { get; private set; }

            private HashSet<Point> freePositions;
            private Random rnd;

            public void Reset()
            {
                DrawBox(Rect);
                freePositions.Clear();
                for (int x = Rect.Left + 1; x < Rect.Right; x++)
                {
                    for (int y = Rect.Top + 1; y < Rect.Bottom; y++)
                    {
                        freePositions.Add(new Point(x, y));
                    }
                }
                GenerateFood(Rect.Center);
            }

            public bool GenerateFood()
            {
                if (freePositions.Count > 0)
                {
                    int index = rnd.Next(freePositions.Count);
                    GenerateFood(freePositions.ElementAt(index));
                    return true;
                }
                return false;
            }

            public void GenerateFood(Point p)
            {
                Food = p; new Point(Rect.CenterX, Rect.CenterY);
                WriteAt(Food, "X");
            }

            public bool IsFree(Point p)
            {
                return freePositions.Contains(p);
            }

            public void FreePosition(Point p)
            {
                freePositions.Add(p);
            }

            public void OccupyPosition(Point p)
            {
                freePositions.Remove(p);
            }

            public void ClampPosition(ref Point p)
            {
                if (p.x == Rect.Left)
                {
                    p.x = Rect.Right - 1;
                }
                else if (p.x == Rect.Right)
                {
                    p.x = Rect.Left + 1;
                }

                if (p.y == Rect.Top)
                {
                    p.y = Rect.Bottom - 1;
                }
                else if (p.y == Rect.Bottom)
                {
                    p.y = Rect.Top + 1;
                }
            }
        }

        class Player
        {
            public Player(int x, int y, int width, int height, int startLength)
            {
                board = new Board(new Rectangle(x, y, width, height));
                snake = new Queue<Point>();
                StartLength = startLength;
            }
            public Board board;
            public Queue<Point> snake;
            public int Score { get; private set; }
            public int Speed { get; private set; }
            private Point Direction { get; set; }
            private int StartLength { get; set; }
            private int StartUp { get; set; }

            private static Point DIRECTION_UP = new Point(0, -1);
            private static Point DIRECTION_DOWN = new Point(0, 1);
            private static Point DIRECTION_LEFT = new Point(-1, 0);
            private static Point DIRECTION_RIGHT = new Point(1, 0);

            public void Reset()
            {
                board.Reset();
                snake.Clear();

                Score = -1;
                Speed = 500;

                Direction = new Point(1, 0);

                StartUp = StartLength;

                MoveSnakeTo(board.Rect.Center);

                WriteAt(snake.Last(), "#");
            }

            public void HandleKeyStroke(ConsoleKey key)
            {
                switch (key)
                {
                    case ConsoleKey.DownArrow:
                        if (!Direction.Equals(DIRECTION_UP))
                        {
                            Direction = DIRECTION_DOWN;
                        }
                        break;
                    case ConsoleKey.UpArrow:
                        if (!Direction.Equals(DIRECTION_DOWN))
                        {
                            Direction = DIRECTION_UP;
                        }
                        break;
                    case ConsoleKey.LeftArrow:
                        if (!Direction.Equals(DIRECTION_RIGHT))
                        {
                            Direction = DIRECTION_LEFT;
                        }
                        break;
                    case ConsoleKey.RightArrow:
                        if (!Direction.Equals(DIRECTION_LEFT))
                        {
                            Direction = DIRECTION_RIGHT;
                        }
                        break;
                }
            }

            public bool MoveSnake()
            {
                return MoveSnakeTo(snake.Last().Add(Direction));
            }

            private bool MoveSnakeTo(Point newPoint)
            {
                board.ClampPosition(ref newPoint);

                if (board.IsFree(newPoint))
                {
                    snake.Enqueue(newPoint);
                    WriteAt(snake.Last().x, snake.Last().y, "#");
                    board.OccupyPosition(newPoint);

                    if (!(newPoint.Equals(board.Food)))
                    {
                        if (StartUp-- <= 0)
                        {
                            Point p = snake.Dequeue();
                            WriteAt(p, " ");
                            board.FreePosition(p);
                        }
                    }
                    else
                    {
                        Score++;
                        Speed = Math.Max(100, Speed - 10);
                        board.GenerateFood();
                    }
                    return true;
                }

                Alert(newPoint);

                return false;
            }

            private void Alert(Point p)
            {
                int alertTime = 300;
                for (int i = 0; i <= 5; i++)
                {
                    WriteAt(p, " ");
                    Thread.Sleep(alertTime);
                    WriteAt(p, "#");
                    Thread.Sleep(alertTime);

                }
            }
        }

        static void WriteAt(Point p, string s)
        {
            WriteAt(p.x, p.y, s);
        }

        static void WriteAt(int x, int y, string s)
        {
            Console.SetCursorPosition(x, y);
            Console.Write(s);
        }

        static void DrawBox(Rectangle rect, bool filled = false)
        {
            WriteAt(rect.Left, rect.Top, "+");
            WriteAt(rect.Right, rect.Top, "+");
            WriteAt(rect.Left, rect.Bottom, "+");
            WriteAt(rect.Right, rect.Bottom, "+");
            for (int xx = rect.Left + 1; xx < rect.Right; xx++)
            {
                WriteAt(xx, rect.Top, "-");
                WriteAt(xx, rect.Bottom, "-");
                if (filled)
                {
                    for (int yy = rect.Top + 1; yy < rect.Bottom; yy++)
                    {
                        WriteAt(xx, yy, " ");
                    }
                }
            }
            for (int yy = rect.Top + 1; yy < rect.Bottom; yy++)
            {
                WriteAt(rect.Left, yy, "|");
                WriteAt(rect.Right, yy, "|");
            }
        }

        static void ShowGameOver()
        {
            var rect = new Rectangle(6, 3, 20, 6);
            DrawBox(rect, true);
            WriteAt(rect.Left + 1, rect.Top + 2, "     Game Over!");
            WriteAt(rect.Left + 1, rect.Top + 4, " Play again? [y|n]");
        }

        public static void Run()
        {
            Console.CursorVisible = false;

            var player1 = new Player(2, 1, 20, 10, 5);

            bool playing = true;
            do
            {
                Console.Clear();
                player1.Reset();

                ConsoleKeyInfo cKey = new ConsoleKeyInfo();
                bool running = true;
                do
                {
                    if (Console.KeyAvailable)
                    {
                        cKey = Console.ReadKey(false);
                        player1.HandleKeyStroke(cKey.Key);
                    }

                    running = player1.MoveSnake();

                    if (running)
                    {
                        WriteAt(24, 2, $"Player1:         ");
                        WriteAt(34, 2, player1.Score.ToString());
                        Thread.Sleep(player1.Speed);
                    }

                } while (cKey.Key != ConsoleKey.Escape && running);

                while (Console.KeyAvailable)
                {
                    Console.ReadKey(false);
                }

                ShowGameOver();

                bool waiting = true;

                while (!Console.KeyAvailable && waiting)
                {
                    switch (Console.ReadKey(false).Key)
                    {
                        case ConsoleKey.Y:
                            waiting = false;
                            break;
                        default:
                            waiting = false;
                            playing = false;
                            break;
                    }
                    Thread.Sleep(100);
                }
            } while (playing);

            Console.CursorVisible = true;
        }
    }
}
