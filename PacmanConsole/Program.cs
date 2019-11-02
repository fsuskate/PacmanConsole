using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace PacmanConsole
{
    class PacmanConsole
    {
        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern SafeFileHandle CreateFile(
        string fileName,
        [MarshalAs(UnmanagedType.U4)] uint fileAccess,
        [MarshalAs(UnmanagedType.U4)] uint fileShare,
        IntPtr securityAttributes,
        [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
        [MarshalAs(UnmanagedType.U4)] int flags,
        IntPtr template);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteConsoleOutput(
        SafeFileHandle hConsoleOutput,
        CharInfo[] lpBuffer,
        Coord dwBufferSize,
        Coord dwBufferCoord,
        ref SmallRect lpWriteRegion);

        [StructLayout(LayoutKind.Sequential)]
        public struct Coord
        {
            public short X;
            public short Y;

            public Coord(short X, short Y)
            {
                this.X = X;
                this.Y = Y;
            }
        };

        [StructLayout(LayoutKind.Explicit)]
        public struct CharUnion
        {
            [FieldOffset(0)] public char UnicodeChar;
            [FieldOffset(0)] public byte AsciiChar;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct CharInfo
        {
            [FieldOffset(0)] public CharUnion Char;
            [FieldOffset(2)] public short Attributes;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SmallRect
        {
            public short Left;
            public short Top;
            public short Right;
            public short Bottom;
        }

        public int[,] grid = new int[,]
        {
            { 1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
            { 1,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,1},
            { 1,0,1,1,0,1,1,1,0,1,0,1,1,1,0,1,1,0,1},
            { 1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            { 1,0,1,1,0,1,0,1,1,1,1,1,0,1,0,1,1,0,1},
            { 1,0,0,0,0,1,0,0,0,1,0,0,0,1,0,0,0,0,1},
            { 1,1,1,1,0,1,1,1,0,1,0,1,1,1,0,1,1,1,1},
            { 2,2,2,1,0,1,0,0,0,0,0,0,0,1,0,1,2,2,2},
            { 1,1,1,1,0,1,0,1,1,1,1,1,0,1,0,1,1,1,1},
            { 0,0,0,0,0,0,0,1,2,2,2,1,0,0,0,0,0,0,0},
            { 1,1,1,1,0,1,0,1,1,1,1,1,0,1,0,1,1,1,1},
            { 2,2,2,1,0,1,0,0,0,0,0,0,0,1,0,1,2,2,2},
            { 1,1,1,1,0,1,0,1,1,1,1,1,0,1,0,1,1,1,1},
            { 1,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,1},
            { 1,0,1,1,0,1,1,1,0,1,0,1,1,1,0,1,1,0,1},
            { 1,0,0,1,0,0,0,0,0,0,0,0,0,0,0,1,0,0,1},
            { 1,1,0,1,0,1,0,1,1,1,1,1,0,1,0,1,0,1,1},
            { 1,0,0,0,0,1,0,0,0,1,0,0,0,1,0,0,0,0,1},
            { 1,0,1,1,1,1,1,1,0,1,0,1,1,1,1,1,1,0,1},
            { 1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            { 1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
        };

        int pmRow = 1;
        int pmCol = 1;
        int score = 0;

        public short GetElementColor(MazeElementType element)
        {
            switch (element)
            {
                case MazeElementType.Wall:
                    return (short)(1 | (1 << 4));
                case MazeElementType.Pellet:
                    return (short)(15 | (0 << 4));
                case MazeElementType.Empty:
                    return (short)(0 | (0 << 4));
                case MazeElementType.Pacman:
                    return (short)(6 | (0 << 4));
                default:
                    return (short)(0 | (0 << 4));
            }
        }

        public enum MazeElementType
        {
            Pellet = 0,
            Wall = 1,
            Empty = 2,
            Pacman = (int)'C'
        }

        public enum Colors
        {
            Blue = 1,
        }

        public int MAX_WIDTH = 19;
        public int MAX_HEIGHT = 20;

        public char GetElementDisplay(MazeElementType element)
        {
            switch (element)
            {
                case MazeElementType.Wall:
                    return '#';
                case MazeElementType.Pellet:
                    return '*';
                case MazeElementType.Empty:
                    return ' ';
                case MazeElementType.Pacman:
                    return 'C';
                default:
                    return (char)element;
            }
        }

        public bool IsPacman(int row, int col)
        {
            if (pmCol == col && pmRow == row)
            {
                return true;
            }

            return false;
        }

        public Tuple<byte, short> GetGridElement(int row, int col)
        {
            char screenElement = (char)0;
            short color = 0;
            if (IsPacman(row, col))
            {
                screenElement = GetElementDisplay(MazeElementType.Pacman);
                color = GetElementColor(MazeElementType.Pacman);
            }
            else
            {
                screenElement = GetElementDisplay((MazeElementType)grid[row, col]);
                color = GetElementColor((MazeElementType)grid[row, col]);
            }

            return new Tuple<byte, short>((byte)screenElement, (short)color);
        }

        public void DrawGrid(SafeFileHandle h)
        {
            int numRows = grid.GetLength(0); // GetLength gets the length of the specified dimension
            int numCols = grid.GetLength(1);
            int numHeadersRows = 2;

            if (!h.IsInvalid)
            {
                CharInfo[] buf = new CharInfo[numCols * (numRows + numHeadersRows)];
                SmallRect rect = new SmallRect() { Left = 0, Top = 0, Right = (short)numCols, Bottom = (short)(numRows + numHeadersRows) };

                for (int row = 0; row < numRows; ++row)
                {
                    for (int col = 0; col < numCols; col++)
                    {
                        var gridElement = GetGridElement(row, col);
                        int index = ((row + numHeadersRows) * MAX_WIDTH) + col;
                        buf[index].Attributes = gridElement.Item2;
                        buf[index].Char.AsciiChar = gridElement.Item1;
                    }
                }

                _ = WriteConsoleOutput(h, buf,
                        new Coord() { X = (short)numCols, Y = (short)(numRows + numHeadersRows) },
                        new Coord() { X = 0, Y = 0 },
                        ref rect);
            }
        }

        public bool DetectWallCollision(int row, int col)
        {
            if (grid[row, col] == (int)MazeElementType.Wall)
            {
                return true;
            }

            return false;
        }

        public void EatPellet()
        {
            if (grid[pmRow, pmCol] == (int)MazeElementType.Pellet)
            {
                grid[pmRow, pmCol] = (int)MazeElementType.Empty;
                score += 10;
            }
        }

        public bool MovePacman(ConsoleKey key)
        {
            bool moved = false;
            int tempCol = pmCol;
            int tempRow = pmRow;
            switch (key)
            {
                case ConsoleKey.LeftArrow:
                    if (tempCol == 0) { tempCol = MAX_WIDTH; }
                    if (!DetectWallCollision(pmRow, --tempCol))
                    {
                        pmCol = tempCol;
                        moved = true;
                    }
                    break;
                case ConsoleKey.RightArrow:
                    if (tempCol >= MAX_WIDTH - 1) { tempCol = -1; }
                    if (!DetectWallCollision(pmRow, ++tempCol))
                    {
                        pmCol = tempCol;
                        moved = true;
                    }
                    break;
                case ConsoleKey.DownArrow:
                    if (!DetectWallCollision(++tempRow, pmCol))
                    {
                        pmRow = tempRow;
                        moved = true;
                    }
                    break;
                case ConsoleKey.UpArrow:
                    if (!DetectWallCollision(--tempRow, pmCol))
                    {
                        pmRow = tempRow;
                        moved = true;
                    }
                    break;
            }

            if (moved)
            {
                EatPellet();
            }

            return moved;
        }

        public void DrawHeader(SafeFileHandle h)
        {
            int numCols = grid.GetLength(1);
            int numHeadersRows = 2;
            string header = " 1UP HIGH SCORE 2UP";
            string scoreStr = score.ToString();

            if (!h.IsInvalid)
            {
                CharInfo[] buf = new CharInfo[numCols * numHeadersRows];
                SmallRect rect = new SmallRect() { Left = 0, Top = 0, Right = (short)numCols, Bottom = (short)(numHeadersRows) };

                for (int col = 0; col < header.Length; col++)
                {
                    var gridElement = header[col];
                    buf[col].Attributes = (7 | (0 << 4));
                    buf[col].Char.AsciiChar = (byte)gridElement;
                }

                for (int col = 0; col < scoreStr.Length; col++)
                {
                    var gridElement = scoreStr[col];
                    buf[MAX_WIDTH + col].Attributes = (7 | (0 << 4));
                    buf[MAX_WIDTH + col].Char.AsciiChar = (byte)gridElement;
                }

                _ = WriteConsoleOutput(h, buf,
                        new Coord() { X = (short)numCols, Y = (short)(numHeadersRows) },
                        new Coord() { X = 0, Y = 0 },
                        ref rect);
            }

        }

        public void GameLoop()
        {
            SafeFileHandle h = CreateFile("CONOUT$", 0x40000000, 2, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);

            bool moved = true;
            while (true)
            {
                if (moved)
                {
                    DrawGrid(h);
                    DrawHeader(h);
                }
                var key = Console.ReadKey().Key;
                moved = MovePacman(key);
            }
        }

        static void Main(string[] args)
        {
            PacmanConsole pm = new PacmanConsole();
            pm.GameLoop();
        }
    }
}
