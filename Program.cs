// MiniTetris
// written by Andrea Giani
//

using System;
using System.Timers;
using static System.Console;
using System.Collections.Generic;
using System.Threading;

class TetrisGame
{
    enum CellStates { Empty, Dead, Alive }
    enum Movement { Left, Down, Right, RotLeft, RotRight }

    static float SpeedUp = 0.9f;
    static float Delay = 800;
    const int Height = 20;
    const int Width = 10;

    static CellStates[,] World = new CellStates[Width, Height];
    static Random Rand = new Random();
    static System.Timers.Timer GameTimer = new System.Timers.Timer();
    static int Score = 0;

    static int[,] Pieces = new int[,] { 
        { 0, 1, 1, 1, 2, 1, 3, 1 },  // I
        { 0, 0, 0, 1, 1, 1, 2, 1 },  // J
        { 0, 1, 1, 1, 2, 0, 2, 1 },  // L
        { 1, 0, 2, 0, 1, 1, 2, 1 },  // O
        { 0, 1, 1, 0, 1, 1, 2, 0 },  // S
        { 0, 1, 1, 0, 1, 1, 2, 1 },  // T
        { 0, 0, 1, 0, 1, 1, 2, 1 }   // Z
    };
    
    static int[] PieceColors = { 1, 2, 3, 4, 5, 6, 7 };
    static int CurrentPieceColor = 1;

    static void Main()
    {
        try {
            SetupConsole();
            InitializeGame();
            RunGameLoop();
        }
        catch (Exception ex) {
            ResetConsole();
            WriteLine($"Errore: {ex.Message}");
        }
    }

    static void SetupConsole()
    {
        CursorVisible = false;
        Title = "C# MINITETRIS";
        SetWindowSize(Width * 2 + 20, Height + 3);
        SetBufferSize(Width * 2 + 20, Height + 3);
    }

    static void ResetConsole()
    {
        ResetColor();
        CursorVisible = true;
        Clear();
    }

    static void InitializeGame()
    {
        // Inizializza il mondo vuoto
        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
                World[x, y] = CellStates.Empty;

        SpawnPiece();
        Draw();

        GameTimer.Elapsed += Tick;
        GameTimer.Interval = Delay;
        GameTimer.Start();
    }

    static void RunGameLoop()
    {
        Thread musicThread = new Thread(PlaySong);
        musicThread.IsBackground = true;
        musicThread.Start();

        ConsoleKeyInfo cki;
        while ((cki = ReadKey(true)) != null)
            Input(cki.Key);        
    }
    
    static bool kbhit() {
        return KeyAvailable;
    }

    static void Input(ConsoleKey key)
    {
        switch (key)
        {
            case ConsoleKey.Q:
                MovePiece(Movement.RotLeft);
                break;
            case ConsoleKey.E:
                MovePiece(Movement.RotRight);
                break;
            case ConsoleKey.A:
                MovePiece(Movement.Left);
                break;
            case ConsoleKey.S:
                Tick(null, null);
                break;
            case ConsoleKey.D:
                MovePiece(Movement.Right);
                break;
            case ConsoleKey.Escape:
                ResetConsole();
                Environment.Exit(0);
                break;
        }
    }

    static void SpawnPiece()
    {
        int rand = Rand.Next(7);
        CurrentPieceColor = PieceColors[rand];
        bool exit = false;

        for (int i = 0; i < 8; i += 2)
        {
            int x = Pieces[rand, i] + Width / 2 - 2;
            int y = Pieces[rand, i + 1];
            
            if (x >= 0 && x < Width && y >= 0 && y < Height)
            {
                if (World[x, y] != CellStates.Empty)
                    exit = true;
                else
                    World[x, y] = CellStates.Alive;
            }
        }

        Draw();

        if (exit) {
            ResetConsole();
            WriteLine("Game Over! Punteggio finale: " + Score);
            Thread.Sleep(2000);
            Environment.Exit(0);
        }
    }

    static void Draw()
    {
        SetCursorPosition(0, 0);
        
        // Bordo superiore
        ForegroundColor = ConsoleColor.White;
        Write("╔");
        for (int i = 0; i < Width * 2; i++)
            Write("═");
        WriteLine("╗");
        
        // Contenuto gioco
        for (int y = 0; y < Height; y++)
        {
            Write("║");
            for (int x = 0; x < Width; x++)
            {
                if (World[x, y] == CellStates.Empty) {
                    ForegroundColor = ConsoleColor.DarkGray;
                    Write("░░");
                }
                else {
                    // Sceglie colore in base al tipo di pezzo
                    SetPieceColor(World[x, y] == CellStates.Alive ? CurrentPieceColor : (x + y) % 7 + 1);
                    Write("██");
                }
            }
            ForegroundColor = ConsoleColor.White;
            WriteLine("║");
        }
        
        // Bordo inferiore
        Write("╚");
        for (int i = 0; i < Width * 2; i++)
            Write("═");
        WriteLine("╝");
        
        // Informazioni di gioco
        SetCursorPosition(Width * 2 + 5, 2);
        ForegroundColor = ConsoleColor.White;
        Write("C# MINITETRIS");
        
        SetCursorPosition(Width * 2 + 5, 4);
        Write("Punteggio: " + Score);
        
        SetCursorPosition(Width * 2 + 5, 6);
        Write("Controlli:");
        SetCursorPosition(Width * 2 + 5, 7);
        Write("A - Sinistra");
        SetCursorPosition(Width * 2 + 5, 8);
        Write("D - Destra");
        SetCursorPosition(Width * 2 + 5, 9);
        Write("S - Giù");
        SetCursorPosition(Width * 2 + 5, 10);
        Write("Q/E - Ruota");
        SetCursorPosition(Width * 2 + 5, 11);
        Write("ESC - Esci");
    }
    
    static void SetPieceColor(int colorIndex)
    {
        switch (colorIndex % 7)
        {
            case 0: ForegroundColor = ConsoleColor.Cyan; break;      // I
            case 1: ForegroundColor = ConsoleColor.Blue; break;      // J
            case 2: ForegroundColor = ConsoleColor.DarkYellow; break; // L
            case 3: ForegroundColor = ConsoleColor.Yellow; break;    // O
            case 4: ForegroundColor = ConsoleColor.Green; break;     // S
            case 5: ForegroundColor = ConsoleColor.Magenta; break;   // T
            case 6: ForegroundColor = ConsoleColor.Red; break;       // Z
        }
    }

    static void KillAll()
    {
        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
                if (World[x, y] == CellStates.Alive)
                    World[x, y] = CellStates.Dead;

        RemoveLines();
        SpawnPiece();
    }

    static void RemoveLines()
    {
        int linesRemoved = 0;
        
        for (int y = 0; y < Height; y++)
        {
            int filled = 0;
            for (int x = 0; x < Width; x++)
                if (World[x, y] != CellStates.Empty)
                    filled++;

            if (filled == Width)
            {
                linesRemoved++;
                // Effetto visuale linea rimossa
                VisualLineEffect(y);
                
                // Sposta tutte le linee sopra verso il basso
                for (int i = y - 1; i >= 0; i--)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        if (World[x, i] != CellStates.Empty)
                            World[x, i + 1] = CellStates.Dead;
                        else
                            World[x, i + 1] = CellStates.Empty;
                            
                        World[x, i] = CellStates.Empty;
                    }
                }
            }
        }
        
        // Aggiorna punteggio e velocità
        if (linesRemoved > 0)
        {
            // Punteggio: 100 per una linea, bonus per più linee contemporaneamente
            int points = 100 * linesRemoved * linesRemoved;
            Score += points;
            
            // Aumenta velocità
            Delay *= SpeedUp;
            if (Delay < 100) Delay = 100; // Velocità massima
            
            GameTimer.Interval = Delay;
        }
    }
    
    static void VisualLineEffect(int line)
    {
        // Effetto lampeggiante per linea completata
        for (int flash = 0; flash < 3; flash++)
        {
            for (int x = 0; x < Width; x++)
            {
                SetCursorPosition(x * 2 + 1, line + 1);
                ForegroundColor = ConsoleColor.White;
                Write("**");
            }
            Thread.Sleep(50);
            
            for (int x = 0; x < Width; x++)
            {
                SetCursorPosition(x * 2 + 1, line + 1);
                ForegroundColor = ConsoleColor.Black;
                Write("  ");
            }
            Thread.Sleep(50);
        }
    }

    static void Tick(object sender, ElapsedEventArgs e)
    {
        GameTimer.Stop();
        
        MovePiece(Movement.Down);
        
        GameTimer.Interval = Delay;
        GameTimer.Start();
    }

    static void MovePiece(Movement direction)
    {
        HashSet<Point> toUpdate = new HashSet<Point>();
        bool needToKill = false;

        // Trova tutti i blocchi vivi
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (World[x, y] == CellStates.Alive)
                {
                    if (direction == Movement.Down && 
                        (y == Height - 1 || World[x, y + 1] == CellStates.Dead))
                    {
                        needToKill = true;
                    }
                    toUpdate.Add(new Point(x, y));
                }
            }
        }

        if (needToKill)
        {
            KillAll();
            return;
        }

        int xOff = 0;
        int yOff = 0;

        switch (direction)
        {
            case Movement.Left:
                xOff = -1;
                break;
            case Movement.Down:
                yOff = 1;
                break;
            case Movement.Right:
                xOff = 1;
                break;
            case Movement.RotLeft:
            case Movement.RotRight:
                RotatePiece(direction, toUpdate);
                return;
        }

        // Verifica se il movimento è valido
        bool valid = true;
        foreach (Point point in toUpdate)
        {
            int newX = point.x + xOff;
            int newY = point.y + yOff;
            
            if (newX < 0 || newX >= Width || newY < 0 || newY >= Height || 
                (World[newX, newY] == CellStates.Dead))
            {
                valid = false;
                break;
            }
        }

        if (valid)
        {
            // Rimuovi i blocchi dalle vecchie posizioni
            foreach (Point point in toUpdate)
                World[point.x, point.y] = CellStates.Empty;
                
            // Aggiungi i blocchi nelle nuove posizioni
            foreach (Point point in toUpdate)
                World[point.x + xOff, point.y + yOff] = CellStates.Alive;

            Draw();
        }
    }
    
    static void RotatePiece(Movement rotDir, HashSet<Point> pieces)
    {
        if (pieces.Count != 4) return; // Solo i tetromini hanno 4 blocchi
        
        // Trova il centro di rotazione (media dei punti)
        float centerX = 0, centerY = 0;
        foreach (Point p in pieces)
        {
            centerX += p.x;
            centerY += p.y;
        }
        centerX /= pieces.Count;
        centerY /= pieces.Count;
        
        // Arrotonda al punto più vicino
        Point center = new Point((int)Math.Round(centerX), (int)Math.Round(centerY));
        
        HashSet<Point> newPositions = new HashSet<Point>();
        foreach (Point p in pieces)
        {
            int relX = p.x - center.x;
            int relY = p.y - center.y;
            
            // Rotazione (senso orario o antiorario)
            int newRelX = rotDir == Movement.RotRight ? -relY : relY;
            int newRelY = rotDir == Movement.RotRight ? relX : -relX;
            
            newPositions.Add(new Point(center.x + newRelX, center.y + newRelY));
        }
        
        // Verifica se le nuove posizioni sono valide
        bool valid = true;
        foreach (Point p in newPositions)
        {
            if (p.x < 0 || p.x >= Width || p.y < 0 || p.y >= Height || 
                (World[p.x, p.y] != CellStates.Empty && !pieces.Contains(new Point(p.x, p.y))))
            {
                valid = false;
                break;
            }
        }
        
        if (valid)
        {
            // Rimuovi i blocchi dalle vecchie posizioni
            foreach (Point p in pieces)
                World[p.x, p.y] = CellStates.Empty;
                
            // Aggiungi i blocchi nelle nuove posizioni
            foreach (Point p in newPositions)
                World[p.x, p.y] = CellStates.Alive;
                
            Draw();
        }
    }

    static void PlaySong()
    {
        try
        {
            while (true)
            {
                Beep(1320,150); Beep(990,100); Beep(1056,100); Beep(1188,100); 
                Beep(1320,50); Beep(1188,50); Beep(1056,100); Beep(990,100); 
                Beep(880,300); Beep(880,100); Beep(1056,100); Beep(1320,300); 
                Beep(1188,100); Beep(1056,100); Beep(990,300); Beep(1056,100); 
                Beep(1188,300); Beep(1320,300); Beep(1056,300); Beep(880,300); 
                Beep(880,300);
                Thread.Sleep(500);
            }
        }
        catch (Exception)
        {
            // Termina silenziosamente
        }
    }

    struct Point
    {
        public int x, y;

        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        
        public override bool Equals(object obj)
        {
            if (!(obj is Point)) return false;
            Point other = (Point)obj;
            return x == other.x && y == other.y;
        }
        
        public override int GetHashCode()
        {
            return x * 31 + y;
        }
    }
}
