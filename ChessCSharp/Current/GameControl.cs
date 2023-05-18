using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Chess
{
    public class GameControl
    {
        public static BoardSquare[] Board;

        public static bool gameEnded = false;
        public static int sideToMove = 8;
        public static int computerSide = 16;
        public static int Moves = 0; 
        public static bool KingInCheck = false;
        public static bool SinglePlayer = false; 
        public static int NoGames = 0;

        public static Timer WhiteTimer = new Timer() { Interval = 1000 }; 
        public static Timer BlackTimer = new Timer() { Interval = 1000 };

        public GameControl() 
        {
            Board = new BoardSquare[64];
        }

       
        public static void SetOriginalVariables(bool single)
        {
            if (single == true)
            {
                SinglePlayer = true;
            }
            else
            {
                SinglePlayer = false;
            }

            BlackTimer.Stop();
            WhiteTimer.Stop();

            sideToMove = FenStringUtility.GetSideToMoveFirst();
            Moves = 0;
            KingInCheck = false;
            gameEnded = false;

            RuleBook.BlackKingSide = true;
            RuleBook.WhiteKingSide = true;
            RuleBook.BlackQueenSide = true;
            RuleBook.WhiteQueenSide = true;

            RuleBook.ClearLists();

            NoGames += 1;
        }

        public static bool EndGame(int winningSide)
        {
            if (gameEnded == false && winningSide != -1)
            {
                GameWindow.currentLegalMoves.Clear();
                if (winningSide != 0) 
                {
                    Console.WriteLine($"{Piece.ColourNameFromPieceBin(winningSide | Piece.King)} Has Won The Game");
                    GameWindow.WinnerDisplay.Text = $"{Piece.ColourNameFromColourBin(winningSide)} Wins!";
                }
                else
                {
                    Console.WriteLine("Stalemate");
                    GameWindow.WinnerDisplay.Text = "Stalemate!";
                }
                // stop timers
                BlackTimer.Stop();
                WhiteTimer.Stop();

                GameWindow.WinnerDisplay.Visible = true;
                System.IO.Stream stream = Properties.Resources.GameOver;
                System.Media.SoundPlayer player = new System.Media.SoundPlayer(stream);
                player.Play();

                return true; 
            }
            else if (gameEnded == false && winningSide == 0) { return false; } 
            return true; 

        }


        public static int CheckSideToMove()
        {
            return sideToMove;
        } 

        public static int OpposingSide()
        {
            return sideToMove == 8 ? 16 : 8;
        }
       
        public static void ChangeSideToMove()
        {
            sideToMove = sideToMove == 8 ? 16 : 8;
        }

     
        public static int GetEndGameWeight()
        {
            int weighting = 0;

            
            if(Moves > 30)
            {
                weighting += 3;
            }
            else if(Moves > 20)
            {
                weighting += 2;
            }
            else if (Moves > 10)
            {
                weighting += 1;
            }

          
            if(PieceLocator.BishopLocations.Count == 0)
            {
                weighting += 1;
            }
            if (PieceLocator.KnightLocations.Count == 0)
            {
                weighting += 1;
            }
            if (PieceLocator.RookLocations.Count == 0)
            {
                weighting += 2;
            }

            weighting += (16 - PieceLocator.PawnLocations.Count) / 4;

            return weighting;
        }


        public static void Move(Move move)
        {
            if (move.MoveTo == -1)
            {
                if (RuleBook.KingInCheck == true)
                {
                    EndGame(8);
                    return;
                }
                EndGame(-1);
                return;
            }

            MoveStack.Push(move); 

            if (Moves < 9) 
            {
                OpeningBook.AddToMoveString(move);
            }

       
            System.Media.SoundPlayer player = new System.Media.SoundPlayer();

            
            if (Board[move.MoveTo].PieceOnSquare != 0)
            {
                System.IO.Stream stream = Properties.Resources.Capture;
                player = new System.Media.SoundPlayer(stream);
            }
     
            else
            {
                System.IO.Stream stream = Properties.Resources.Move;
                player = new System.Media.SoundPlayer(stream);
            }


            bool Castled = false;

            if (Piece.Type(move.Piece) == Piece.King && Math.Abs((move.MoveFrom % 8) - (move.MoveTo % 8)) > 1) 
            {
                Castled = true;
                Castle(move); 
            }
            else {
                if (Board[move.MoveTo].PieceOnSquare != 0)
                {
                    RemovePiece(Board[move.MoveTo].PieceOnSquare, move.MoveTo); 
                }
                if (Piece.Type(move.Piece) == Piece.Pawn)
                {
                    move.Piece = RuleBook.CheckPromotion(move); 
                }
                AddPiece(move.Piece, move.MoveTo); 
            }

            Moves += 1; 
            StartStopTimers();
            ChangeSideToMove();

            RuleBook.CheckIfCastlingRightsHaveChanged(move);
            RuleBook.SquaresToMoveToThatStopCheck.Clear();
            GameWindow.SetLegalMoves(); 
            Console.WriteLine($"Side to move: {Piece.ColourNameFromColourBin(sideToMove)}, Total moves: {Moves}, {Piece.PieceToFullName(sideToMove | Piece.King)} is in check: {RuleBook.KingInCheck}, Current Eval:{PieceLocator.GetPosEval()} \n");

            if (RuleBook.KingInCheck == true)
            {
                System.IO.Stream stream = Properties.Resources.Check;
                player = new System.Media.SoundPlayer(stream);
            }

          
            if (Castled == false)
            {
                player.Play(); 
            }

            GameWindow.resetColours(); 
            Board[move.MoveFrom].BackColor = ColorTranslator.FromHtml("#dbdb42"); 
            Board[move.MoveTo].BackColor = ColorTranslator.FromHtml("#e8dd10"); 

       
            Board[move.MoveFrom].Refresh();
            Board[move.MoveTo].Refresh();

            
            if (SinglePlayer == true && sideToMove == computerSide)
            {
                Move ComputerMove = Computer.GenerateMove(); 
                try { RemovePiece(ComputerMove.Piece, ComputerMove.MoveFrom); } 
                catch { Move(ComputerMove); } 
                Move(ComputerMove);
            }
        }

        public static Stack<int> CapturedPieceTrackerStack = new Stack<int>(); 
        public static void makeTestMove(Move move)
        {
            
            ChangeSideToMove(); 

            int piece = move.Piece;

            
            if (Piece.Type(move.Piece) == Piece.King && Math.Abs((move.MoveFrom % 8) - (move.MoveTo % 8)) > 1)
            {
                Board[move.MoveTo].PieceOnSquare = move.Piece; 
                PieceLocator.AddToList(move.Piece, move.MoveTo);

                Board[move.MoveFrom].PieceOnSquare = 0;
                PieceLocator.RemoveFromList(move.Piece, move.MoveFrom);

                
                if (move.MoveTo == 2) // BQS
                {
                    makeTestCastle(move.MoveTo + 1, 0, 16);
                }
                else if (move.MoveTo == 6) // BKS
                {
                    makeTestCastle(move.MoveTo - 1, 7, 16);
                }
                else if (move.MoveTo == 58) // WQS
                {
                    makeTestCastle(move.MoveTo + 1, 56, 8);
                }
                else if (move.MoveTo == 62) // WKS
                {
                    makeTestCastle(move.MoveTo - 1, 63, 8);
                }
            }

            else
            {
                Board[move.MoveFrom].PieceOnSquare = 0; 
                PieceLocator.RemoveFromList(move.Piece, move.MoveFrom);

                CapturedPieceTrackerStack.Push(Board[move.MoveTo].PieceOnSquare); 
                PieceLocator.RemoveFromList(Board[move.MoveTo].PieceOnSquare, move.MoveTo);

                if (Piece.Type(piece) == Piece.Pawn) 
                {
                    piece = RuleBook.CheckPromotion(move);
                }

                Board[move.MoveTo].PieceOnSquare = piece; 
                PieceLocator.AddToList(piece, move.MoveTo);
            }

            
        }
        public static void unmakeTestMove(Move move)
        {
            
            ChangeSideToMove(); 

            int piece = move.Piece;


            if (Piece.Type(piece) == Piece.King && Math.Abs((move.MoveFrom % 8) - (move.MoveTo % 8)) > 1)
            {
                Board[move.MoveTo].PieceOnSquare = 0; 
                PieceLocator.RemoveFromList(move.Piece, move.MoveTo);

                Board[move.MoveFrom].PieceOnSquare = move.Piece; 
                PieceLocator.AddToList(move.Piece, move.MoveFrom);

                if (move.MoveTo == 2) // BQS
                {
                    unmakeTestCastle(move.MoveTo + 1, 0, 16);
                }
                else if (move.MoveTo == 6) // BKS
                {
                    unmakeTestCastle(move.MoveTo - 1, 7, 16);
                }
                else if (move.MoveTo == 58) // WQS
                {
                    unmakeTestCastle(move.MoveTo + 1, 56, 8);
                }
                else if (move.MoveTo == 62) // WKS
                {
                    unmakeTestCastle(move.MoveTo - 1, 63, 8);
                }


            }

           
            else
            {
                Board[move.MoveFrom].PieceOnSquare = piece; 
                PieceLocator.AddToList(piece, move.MoveFrom);

                if(Piece.Type(piece) == Piece.Pawn)
                {
                    if(piece != RuleBook.CheckPromotion(move))
                    {
                        piece = Piece.Colour(piece) | Piece.Queen; 
                    }
                }

                PieceLocator.RemoveFromList(piece, move.MoveTo); 
                int capturedPiece = CapturedPieceTrackerStack.Pop(); 

                Board[move.MoveTo].PieceOnSquare = capturedPiece; 
                PieceLocator.AddToList(capturedPiece, move.MoveTo);

              
            }

           

        }
        public static void makeTestCastle(int kingOffset, int RookCorner, int colour)
        {
           

            int rook = colour | Piece.Rook; 

            Board[RookCorner].PieceOnSquare = 0; 
            PieceLocator.RemoveFromList(rook, RookCorner);

            Board[kingOffset].PieceOnSquare = rook; 
            PieceLocator.AddToList(rook, kingOffset);

         
        }
        public static void unmakeTestCastle(int kingOffset, int RookCorner, int colour)
        {
            

            int rook = colour | Piece.Rook; 

            Board[RookCorner].PieceOnSquare = rook; 
            PieceLocator.AddToList(rook, RookCorner);

            Board[kingOffset].PieceOnSquare = 0; 
            PieceLocator.RemoveFromList(rook, kingOffset);

            
        }

        
        public static void Castle(Move move)
        {
            Console.WriteLine($"castling {move.MoveTo}");
            System.IO.Stream stream = Properties.Resources.Castle;
            System.Media.SoundPlayer player = new System.Media.SoundPlayer(stream);
            player.Play();

            AddPiece(move.Piece, move.MoveTo); 
            if(move.MoveTo == 2){
                RuleBook.BlackQueenSide = false; 
                RemovePiece(Piece.Black | Piece.Rook, 0);
                AddPiece(Piece.Black | Piece.Rook, move.MoveTo + 1);
            }
            else if (move.MoveTo == 6){
                RuleBook.BlackKingSide = false;
                RemovePiece(Piece.Black | Piece.Rook, 7);
                AddPiece(Piece.Black | Piece.Rook, move.MoveTo - 1);
            }
            else if (move.MoveTo == 58){
                RuleBook.WhiteQueenSide = false; 
                RemovePiece(Piece.White | Piece.Rook, 56); 
                AddPiece(Piece.White | Piece.Rook, move.MoveTo + 1);
            }
            else if (move.MoveTo == 62)
            {
                RuleBook.WhiteKingSide = false; 
                RemovePiece(Piece.White | Piece.Rook, 63);
                AddPiece(Piece.White | Piece.Rook, move.MoveTo - 1);
            }

            
            Board[0].Refresh();
            Board[7].Refresh();
            Board[56].Refresh();
            Board[63].Refresh();
            Board[move.MoveTo - 1].Refresh();
            Board[move.MoveTo + 1].Refresh();
        }

        
        public static void StartStopTimers()
        {
            if (sideToMove == 8) 
            {
                BlackTimer.Stop();
                WhiteTimer.Start();
            }
            else if (sideToMove == 16)
            {
                WhiteTimer.Stop();
                BlackTimer.Start();
            }
        }

       
        public static void AddPiece(int piece, int location)
        {
            Board[location].PieceOnSquare = piece;
            Board[location].Image = Piece.PieceToImage(piece);
            PieceLocator.AddToList(piece, location);

            Console.WriteLine($"{Piece.PieceToFullName(piece)} added at square {location}");
            Console.WriteLine(FenStringUtility.GetFenStringFromCurrentBoard(Board) + "\n");
        }

       
        public static void RemovePiece(int piece, int location)
        {
            Board[location].PieceOnSquare = 0;
            Board[location].Image = null;
            PieceLocator.RemoveFromList(piece, location);

            Console.Write($"{Piece.PieceToFullName(piece)} removed at square {location}\n");
        }

    }
}
