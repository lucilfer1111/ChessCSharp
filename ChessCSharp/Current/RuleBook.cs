using System;
using System.Collections.Generic;
using System.Drawing;

namespace Chess
{
    class RuleBook
    {
        // -8 up, 8 down, -1 left, 1 right, -7 up diag right, 7 down diag left, -9 up diag left, 9 down 
        public static int[] Directions = { -8, 8, -1, 1, -7, 7, -9, 9 };
        public static int[] KnightDirections = { -17, -15, 17, 15, -10, 6, 10, -6 };
        public static int[][] DistanceFromEdges = new int[64][];

        public static int FriendlyKingSquare = 0;
        public static bool KingInCheck = false;
        public static bool DoubleCheck = false;
        public static bool GeneratingAttacks = false;

        public static int SideToGenerateFor;

        public static List<int> TempSquaresToMoveToThatStopCheck = new List<int>();
        public static List<int> SquaresToMoveToThatStopCheck = new List<int>();

        public static List<int> PinnedLocations = new List<int>();
        public static List<int> EnemyAttacks = new List<int>();
        public static List<Move> LegalMoves = new List<Move>();

        public static void ClearLists()
        {
            TempSquaresToMoveToThatStopCheck.Clear();
            SquaresToMoveToThatStopCheck.Clear();
            PinnedLocations.Clear();
            EnemyAttacks.Clear();
            LegalMoves.Clear();
            GameWindow.currentLegalMoves.Clear();

            PieceLocator.PawnLocations.Clear();
            PieceLocator.KnightLocations.Clear();
            PieceLocator.BishopLocations.Clear();
            PieceLocator.RookLocations.Clear();
            PieceLocator.QueenLocations.Clear();
            PieceLocator.WKingLocation.Clear();
            PieceLocator.BKingLocation.Clear();

        }

        
        public static void FindDistanceFromEdges() 
        {

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    
                    int numFromTop = row;
                    int numFromBottom = 7 - row; 
                    int numFromLeft = col;
                    int numFromRight = 7 - col; 
                    int squareNo = (row * 8) + col;

                    DistanceFromEdges[squareNo] = new int[8] {
                        numFromTop,
                        numFromBottom,
                        numFromLeft,
                        numFromRight,
                        Math.Min(numFromTop, numFromRight),
                        Math.Min(numFromBottom, numFromLeft), 
                        Math.Min(numFromTop, numFromLeft), 
                        Math.Min(numFromBottom, numFromRight) 
                    };
                }
            }
        }

        public static int CheckPromotion(Move move)
        {
            if (Piece.Colour(move.Piece) == 8 && (move.MoveTo / 8) == 0) 
            { 
                return Piece.White | Piece.Queen; // Turn pawn to queen
            }
            if (Piece.Colour(move.Piece) == 16 && (move.MoveTo / 8) == 7)
            {
                return Piece.Black | Piece.Queen;
            }
            else
            {
                return move.Piece; 
            }
        }

      
        public static bool CheckIfMoveIsAllowed(Move move)
        {
            if (PinnedLocations.Contains(move.MoveFrom)) 
            {
                return false;
            }
            if (KingInCheck == false) 
            {
                return true;
            }
            else if (SquaresToMoveToThatStopCheck.Contains(move.MoveTo)) 
            {
                return true;
            }
            else 
            {
                return false;
            }
        }
        public static void GenerateEnemyAttacks()
        {
            GeneratingAttacks = true;
            SideToGenerateFor = SideToGenerateFor == 8 ? 16 : 8;
            GetPins();
            GetKingMoves();
            GetPawnMoves();
            GetKnightMoves();
            GetBishopMoves();
            GetRookMoves();
            GetQueenMoves();

            GeneratingAttacks = false;
            SideToGenerateFor = SideToGenerateFor == 8 ? 16 : 8;
        }

       
        public static List<Move> GenerateLegalMoves()
        {
            
            int sideToGenerateFor = GameControl.CheckSideToMove();
            SideToGenerateFor = sideToGenerateFor;
            FriendlyKingSquare = PieceLocator.GetLocationsOf(sideToGenerateFor | Piece.King)[0];
            EnemyAttacks.Clear();
            GenerateEnemyAttacks();
            LegalMoves.Clear();
            PinnedLocations.Clear();
            KingInCheck = false;
            if (EnemyAttacks.Contains(FriendlyKingSquare))
            {
                KingInCheck = true;
                GameControl.Board[FriendlyKingSquare].BackColor = Color.Red;
            }

           

            GetPins();
            GetKingMoves();
            GetPawnMoves();
            GetKnightMoves();
            GetBishopMoves();
            GetRookMoves();
            GetQueenMoves();
            return LegalMoves;
        }

        
        public static void GetPins()
        {
            int KingSquare = PieceLocator.GetLocationsOf(SideToGenerateFor | Piece.King)[0]; 

            for (int directionIndex = 0; directionIndex < 8; directionIndex++) 
            {
                int locationToCheckPinFor = -1;
                int pieceToCheckPinFor = 0;

                for (int i = 0; i < DistanceFromEdges[KingSquare][directionIndex]; i++)
                {
                    int targetSquare = KingSquare + Directions[directionIndex] * (i + 1); 
                    int pieceOnTargetSquare = GameControl.Board[targetSquare].PieceOnSquare; 
                    
                    if (pieceOnTargetSquare == 0) 
                    {

                    }

                    if (pieceOnTargetSquare != 0 && Piece.IsColour(pieceOnTargetSquare, SideToGenerateFor)) 
                    {
                        if(locationToCheckPinFor == -1)
                        {
                            locationToCheckPinFor = targetSquare; 
                            pieceToCheckPinFor = GameControl.Board[locationToCheckPinFor].PieceOnSquare; 
                        }
                        else
                        {
                            break; 
                        }
                    }

                    if (pieceOnTargetSquare != 0 && !Piece.IsColour(pieceOnTargetSquare, SideToGenerateFor)) 
                    {
                        if(locationToCheckPinFor == -1) 
                        {
                            break; 
                        }
                        else 
                        {

                            if((Piece.Type(pieceOnTargetSquare) == Piece.Rook || Piece.Type(pieceOnTargetSquare) == Piece.Queen) && directionIndex < 4) // if piece is rook, queen or pawn and checking an orthogonal axis, add pin
                            {
                                PinnedLocations.Add(locationToCheckPinFor);
                            
                                if(Piece.Type(pieceToCheckPinFor) == Piece.Rook || Piece.Type(pieceToCheckPinFor) == Piece.Queen)
                                {
                                    
                                    for (int square = 0; square < i+1; square++)
                                    {
                                       
                                        if(EnemyAttacks.Contains(KingSquare) == false) 
                                        {
                                            LegalMoves.Add(new Move { Piece = pieceToCheckPinFor, MoveFrom = locationToCheckPinFor, MoveTo = KingSquare + (Directions[directionIndex] * (square + 1)) }); 
                                        }
                                        EnemyAttacks.Add(KingSquare + (Directions[directionIndex] * (square + 1)));
                                    }
                                }
                                if (Piece.Type(pieceToCheckPinFor) == Piece.Pawn && directionIndex < 2)
                                {
                                    if (Piece.Colour(pieceToCheckPinFor) == Piece.White)
                                    {
                                       
                                        if (GameControl.Board[locationToCheckPinFor - 8].PieceOnSquare == 0 && EnemyAttacks.Contains(KingSquare) == false) // If location is empty and king not in check
                                        {
                                            LegalMoves.Add(new Move { Piece = pieceToCheckPinFor, MoveFrom = locationToCheckPinFor, MoveTo = locationToCheckPinFor - 8 }); // Add move
                                        }
                                        if ((locationToCheckPinFor / 8 == 6) && (GameControl.Board[locationToCheckPinFor - 16].PieceOnSquare == 0) && EnemyAttacks.Contains(KingSquare) == false) // If location is empty and king not in check
                                        {
                                            LegalMoves.Add(new Move { Piece = pieceToCheckPinFor, MoveFrom = locationToCheckPinFor, MoveTo = locationToCheckPinFor - 16 }); // Add move
                                        }

                                    }
                                   
                                    else if (Piece.Colour(pieceToCheckPinFor) == Piece.Black)
                                    {
                                        
                                        if (GameControl.Board[locationToCheckPinFor + 8].PieceOnSquare == 0 && EnemyAttacks.Contains(KingSquare) == false)
                                        {
                                            LegalMoves.Add(new Move { Piece = pieceToCheckPinFor, MoveFrom = locationToCheckPinFor, MoveTo = locationToCheckPinFor + 8 });
                                        }
                                        if ((locationToCheckPinFor / 8 == 1) && (GameControl.Board[locationToCheckPinFor + 16].PieceOnSquare == 0) && EnemyAttacks.Contains(KingSquare) == false)
                                        {
                                            LegalMoves.Add(new Move { Piece = pieceToCheckPinFor, MoveFrom = locationToCheckPinFor, MoveTo = locationToCheckPinFor + 16 });
                                        }
                                    }
                                }
                            }

                            else if((Piece.Type(pieceOnTargetSquare) == Piece.Bishop || Piece.Type(pieceOnTargetSquare) == Piece.Queen) && directionIndex > 3) 
                            {
                                PinnedLocations.Add(locationToCheckPinFor);
                             
                                if (Piece.Type(pieceToCheckPinFor) == Piece.Bishop || Piece.Type(pieceToCheckPinFor) == Piece.Queen)
                                {
                                  
                                    for (int square = 0; square < i+1; square++)
                                    {
                                  
                                        LegalMoves.Add(new Move { Piece = pieceToCheckPinFor, MoveFrom = locationToCheckPinFor, MoveTo = KingSquare + (Directions[directionIndex] * (square + 1)) });
                                        EnemyAttacks.Add(KingSquare + (Directions[directionIndex] * (square + 1)));
                                    }
                                }
                                if (Piece.Type(pieceToCheckPinFor) == Piece.Pawn)
                                {
                                    if (Piece.Colour(pieceToCheckPinFor) == Piece.White)
                                    {
                                        if (locationToCheckPinFor - 9 == targetSquare && EnemyAttacks.Contains(KingSquare) == false)
                                        {
                                            LegalMoves.Add(new Move { Piece = pieceToCheckPinFor, MoveFrom = locationToCheckPinFor, MoveTo = locationToCheckPinFor - 9 });
                                        }
                                        if (locationToCheckPinFor - 7 == targetSquare && EnemyAttacks.Contains(KingSquare) == false)
                                        {
                                            LegalMoves.Add(new Move { Piece = pieceToCheckPinFor, MoveFrom = locationToCheckPinFor, MoveTo = locationToCheckPinFor - 7 });
                                        }
                                    }
                                   
                                    else if (Piece.Colour(pieceToCheckPinFor) == Piece.Black)
                                    {
                                        if (locationToCheckPinFor + 9 == targetSquare && EnemyAttacks.Contains(KingSquare) == false)
                                        {
                                            LegalMoves.Add(new Move { Piece = pieceToCheckPinFor, MoveFrom = locationToCheckPinFor, MoveTo = locationToCheckPinFor + 9 });
                                        }
                                        if (locationToCheckPinFor + 7 == targetSquare && EnemyAttacks.Contains(KingSquare) == false)
                                        {
                                            LegalMoves.Add(new Move { Piece = pieceToCheckPinFor, MoveFrom = locationToCheckPinFor, MoveTo = locationToCheckPinFor + 7 });
                                        }
                                    }
                                }
                            }
                            break;
                        }
                    }
                }
            }
        } 

        public static void GetKingMoves()
        {
            int KingSquare = PieceLocator.GetLocationsOf(SideToGenerateFor | Piece.King)[0]; 
            foreach (int direction in Directions)
            {
                int targetSquare = KingSquare + direction; 
                int row = KingSquare % 8;
                int col = KingSquare / 8;

                int targetRow = targetSquare % 8;
                int targetCol = targetSquare / 8;

                if (Math.Abs(targetRow - row) > 1 || Math.Abs(targetCol - col) > 1) 
                {
                    continue;
                }
 

                if (targetSquare < 0 || targetSquare > 63) { continue; }

                int pieceOnTargetSquare = GameControl.Board[targetSquare].PieceOnSquare; 
                Move attemptingMove = new Move { Piece = SideToGenerateFor | Piece.King, MoveFrom = KingSquare, MoveTo = targetSquare };

                if (GeneratingAttacks == true)
                {
                    EnemyAttacks.Add(targetSquare); 
                }

                if (GeneratingAttacks == false)
                {
                    

                    if (pieceOnTargetSquare == 0) 
                    {
                        if (EnemyAttacks.Contains(targetSquare) == false)
                        {
                            LegalMoves.Add(attemptingMove);
                        }
                    }

                    else if (Piece.Colour(pieceOnTargetSquare) != SideToGenerateFor) 
                    {
                        if (EnemyAttacks.Contains(targetSquare) == false)
                        {
                            LegalMoves.Add(attemptingMove);
                        }
                    }
                }
            }
            if (KingInCheck == false && GeneratingAttacks == false) 
            {
                if (SideToGenerateFor == 8)
                {
                    if (KingSquare == 60) 
                    {
                        
                        if (WhiteQueenSide == true && GameControl.Board[57].PieceOnSquare == 0 && GameControl.Board[58].PieceOnSquare == 0 && GameControl.Board[59].PieceOnSquare == 0 && EnemyAttacks.Contains(57) == false && EnemyAttacks.Contains(58) == false && EnemyAttacks.Contains(59) == false && GameControl.Board[56].PieceOnSquare == (Piece.White | Piece.Rook))
                        {
                            LegalMoves.Add(new Move { Piece = SideToGenerateFor | Piece.King, MoveFrom = KingSquare, MoveTo = 58 });
                        }
                        if (WhiteKingSide == true && GameControl.Board[61].PieceOnSquare == 0 && GameControl.Board[62].PieceOnSquare == 0 && EnemyAttacks.Contains(61) == false && EnemyAttacks.Contains(62) == false && GameControl.Board[63].PieceOnSquare == (Piece.White | Piece.Rook))
                        {
                            LegalMoves.Add(new Move { Piece = SideToGenerateFor | Piece.King, MoveFrom = KingSquare, MoveTo = 62 });
                        }
                    }
                }
                if (SideToGenerateFor == 16)
                { 
                    if (KingSquare == 4) 
                    {
                        
                        if (BlackQueenSide == true && GameControl.Board[1].PieceOnSquare == 0 && GameControl.Board[2].PieceOnSquare == 0 && GameControl.Board[3].PieceOnSquare == 0 && EnemyAttacks.Contains(1) == false && EnemyAttacks.Contains(2) == false && EnemyAttacks.Contains(3) == false && GameControl.Board[0].PieceOnSquare == (Piece.Black | Piece.Rook))
                        {
                            LegalMoves.Add(new Move { Piece = SideToGenerateFor | Piece.King, MoveFrom = KingSquare, MoveTo = 2 });
                        }
                        if (BlackKingSide == true && GameControl.Board[5].PieceOnSquare == 0 && GameControl.Board[6].PieceOnSquare == 0 && EnemyAttacks.Contains(5) == false && EnemyAttacks.Contains(6) == false && GameControl.Board[7].PieceOnSquare == (Piece.Black | Piece.Rook))
                        {
                            LegalMoves.Add(new Move { Piece = SideToGenerateFor | Piece.King, MoveFrom = KingSquare, MoveTo = 6 });
                        }
                    }
                }
            }
        }

        public static void GetKnightMoves()
        {
            List<int> KnightSqaures = PieceLocator.GetLocationsOf(SideToGenerateFor | Piece.Knight);

            foreach (int location in KnightSqaures)
            {
              
                int row = location % 8;
                int col = location / 8;

                foreach (int direction in KnightDirections)
                {
                    int targetSquare = location + direction;
                    int targetRow = targetSquare % 8;
                    int targetCol = targetSquare / 8;

                   
                    if (Math.Abs(row - targetRow) > 2)
                    {
                        continue;
                    }
                    if (Math.Abs(col - targetCol) > 2)
                    {
                        continue;
                    }

                   
                    if (targetSquare > 63 || targetSquare < 0) 
                    {  
                      
                        continue;
                    }
                    int pieceOnTargetSquare = GameControl.Board[targetSquare].PieceOnSquare; 

                    if (pieceOnTargetSquare == 0)
                    {
                     
                        if (EnemyAttacks.Contains(targetSquare) == false) { EnemyAttacks.Add(targetSquare); } 
                        Move AttemptingMove = new Move { Piece = SideToGenerateFor | Piece.Knight, MoveFrom = location, MoveTo = targetSquare };
                        if (GeneratingAttacks == false && CheckIfMoveIsAllowed(AttemptingMove)) { LegalMoves.Add(AttemptingMove); } 
                    }
                    else if (Piece.IsColour(pieceOnTargetSquare, SideToGenerateFor) == false) 
                    {

                        if(Piece.Type(pieceOnTargetSquare) == Piece.King)
                        { 
                            SquaresToMoveToThatStopCheck.Add(location);
                            if (KingInCheck == true)
                            {
                                DoubleCheck = true;  
                            }
                            else 
                            { 
                                KingInCheck = true;
                            } 
                        }
                        if (EnemyAttacks.Contains(targetSquare) == false) { EnemyAttacks.Add(targetSquare); } 
                        Move AttemptingMove = new Move { Piece = SideToGenerateFor | Piece.Knight, MoveFrom = location, MoveTo = targetSquare };
                        if (GeneratingAttacks == false && CheckIfMoveIsAllowed(AttemptingMove)) { LegalMoves.Add(AttemptingMove); } 
                    }
                    else if (Piece.IsColour(pieceOnTargetSquare, SideToGenerateFor)) 
                    {
                        if (EnemyAttacks.Contains(targetSquare) == false) { EnemyAttacks.Add(targetSquare); } 
                    }
                }
            }
        } 

        public static void GetPawnMoves()
        {
            List<int> PawnSqaures = PieceLocator.GetLocationsOf(SideToGenerateFor | Piece.Pawn);

            foreach (int location in PawnSqaures)
            {
                

                int[] WPawnTakeDirections = { -9, -7 }; 
                int[] BPawnTakeDirections = { 9, 7 }; 
                int piece = GameControl.sideToMove | Piece.Pawn; 

                if (SideToGenerateFor == 8)
                {

                    int targetSquare = location - 8;
                    if (GameControl.Board[targetSquare].PieceOnSquare == 0)
                    {
                        Move AttemptingMove = new Move { Piece = piece, MoveFrom = location, MoveTo = targetSquare };
                        if (GeneratingAttacks == false && CheckIfMoveIsAllowed(AttemptingMove)) { LegalMoves.Add(AttemptingMove); }
                        targetSquare = location - 16; 
                        if (location / 8 == 6 && GameControl.Board[targetSquare].PieceOnSquare == 0)
                        {
                            Move AttemptingMove2 = new Move { Piece = piece, MoveFrom = location, MoveTo = targetSquare };
                            if (GeneratingAttacks == false && CheckIfMoveIsAllowed(AttemptingMove2)) { LegalMoves.Add(AttemptingMove2); }
                        }
                    }

                    foreach (int direction in WPawnTakeDirections)
                    {

                        if ((location % 8 == 0 && direction == -9) || (location % 8 == 7 && direction == -7)) 
                        {
                            continue;
                        }

                        int pieceOnTargetSquare = GameControl.Board[location + direction].PieceOnSquare;

                        targetSquare = location + direction; 
                        if (EnemyAttacks.Contains(targetSquare) == false) { EnemyAttacks.Add(targetSquare); }
                        if (Piece.IsColour(pieceOnTargetSquare, SideToGenerateFor) == false && pieceOnTargetSquare != 0) 
                        {
                            if (Piece.Type(pieceOnTargetSquare) == Piece.King) { SquaresToMoveToThatStopCheck.Add(location); if (KingInCheck == true) { DoubleCheck = true; } else { KingInCheck = true; } }
                            Move AttemptingMove = new Move { Piece = piece, MoveFrom = location, MoveTo = targetSquare };
                            if (GeneratingAttacks == false && CheckIfMoveIsAllowed(AttemptingMove)) { LegalMoves.Add(AttemptingMove); }
                        }
                    }
                }

                if (SideToGenerateFor == 16)
                {

                    int targetSquare = location + 8;
                    if (GameControl.Board[targetSquare].PieceOnSquare == 0)
                    {
                        Move AttemptingMove = new Move { Piece = piece, MoveFrom = location, MoveTo = targetSquare };
                        if (GeneratingAttacks == false && CheckIfMoveIsAllowed(AttemptingMove)) { LegalMoves.Add(AttemptingMove); }

                        targetSquare = location + 16; 
                        if (location / 8 == 1 && GameControl.Board[targetSquare].PieceOnSquare == 0)
                        {
                            Move AttemptingMove2 = new Move { Piece = piece, MoveFrom = location, MoveTo = targetSquare };
                            if (GeneratingAttacks == false && CheckIfMoveIsAllowed(AttemptingMove2)) { LegalMoves.Add(AttemptingMove2); }
                        }
                    }

                    foreach (int direction in BPawnTakeDirections)
                    {

                        if ((location % 8 == 0 && direction == 7) || (location % 8 == 7 && direction == 9))
                        {
                            continue;
                        }

                        int pieceOnTargetSquare = GameControl.Board[location + direction].PieceOnSquare;

                        targetSquare = location + direction;
                        if (EnemyAttacks.Contains(targetSquare) == false) { EnemyAttacks.Add(targetSquare); }
                        if (Piece.IsColour(pieceOnTargetSquare, SideToGenerateFor) == false && pieceOnTargetSquare != 0)
                        {
                            if (Piece.Type(pieceOnTargetSquare) == Piece.King) { SquaresToMoveToThatStopCheck.Add(location); if (KingInCheck == true) { DoubleCheck = true; } else { KingInCheck = true; } }
                            Move AttemptingMove = new Move { Piece = piece, MoveFrom = location, MoveTo = targetSquare };
                            if (GeneratingAttacks == false && CheckIfMoveIsAllowed(AttemptingMove)) { LegalMoves.Add(AttemptingMove); }
                        }
                    }
                }
            }
        } 

        public static void GetBishopMoves()
        {
            List<int> BishopSquares = PieceLocator.GetLocationsOf(SideToGenerateFor | Piece.Bishop);
            GetSlidingPieceMoves(BishopSquares, SideToGenerateFor | Piece.Bishop);
        } 
        public static void GetRookMoves()
        {
            List<int> RookSquares = PieceLocator.GetLocationsOf(SideToGenerateFor | Piece.Rook);
            GetSlidingPieceMoves(RookSquares, SideToGenerateFor | Piece.Rook);
        } 

        public static void GetQueenMoves()
        {
            List<int> QueenSquares = PieceLocator.GetLocationsOf(SideToGenerateFor | Piece.Queen);
            GetSlidingPieceMoves(QueenSquares, SideToGenerateFor | Piece.Queen);
        }

        public static void GetSlidingPieceMoves(List<int> locationList, int piece)
        {
            foreach(int location in locationList)
            {
                

                int start = (Piece.IsType(piece, Piece.Bishop)) ? 4 : 0; 
                int end = (Piece.IsType(piece, Piece.Rook)) ? 4 : 8;

                for (int directionIndex = start; directionIndex < end; directionIndex++)
                {
                    
                    TempSquaresToMoveToThatStopCheck.Clear();
                    for (int i = 0; i < DistanceFromEdges[location][directionIndex]; i++)
                    {
                        int targetSquare = location + Directions[directionIndex] * (i + 1);
                        int pieceOnTargetSquare = GameControl.Board[targetSquare].PieceOnSquare;
                     

                        if (pieceOnTargetSquare != 0 && Piece.IsColour(pieceOnTargetSquare, SideToGenerateFor)) 
                        {
                            if (EnemyAttacks.Contains(targetSquare) == false) { EnemyAttacks.Add(targetSquare); }
                            break;
                        }

                        if (EnemyAttacks.Contains(targetSquare) == false) { EnemyAttacks.Add(targetSquare); }
                        TempSquaresToMoveToThatStopCheck.Add(targetSquare);

                        Move AttemptingMove = new Move { Piece = piece, MoveFrom = location, MoveTo = targetSquare };
                        if (GeneratingAttacks == false && CheckIfMoveIsAllowed(AttemptingMove)) {LegalMoves.Add(AttemptingMove);}

                        if (pieceOnTargetSquare != 0 && !Piece.IsColour(pieceOnTargetSquare, SideToGenerateFor)) 
                        {
                            if(Piece.Type(pieceOnTargetSquare) == Piece.King)
                            {
                                
                                if (KingInCheck == true) { DoubleCheck = true; } else { KingInCheck = true; }
                                SquaresToMoveToThatStopCheck.AddRange(TempSquaresToMoveToThatStopCheck);
                                SquaresToMoveToThatStopCheck.Add(location);
                                EnemyAttacks.Add(location + Directions[directionIndex] * (i + 2)); 
                            }
                            break;
                        }
                    }
                }
            }
        }
        public static bool BlackQueenSide = true;
        public static bool BlackKingSide = true; 
        public static bool WhiteQueenSide = true; 
        public static bool WhiteKingSide = true;

        public static bool BlackKingHasMoved = false;
        public static bool WhiteKingHasMoved = false;


        public static void CheckIfCastlingRightsHaveChanged(Move move)
        {
            if (move.MoveFrom == 0 || move.MoveTo == 0) 
            {
                BlackQueenSide = false;
            }
            if (move.MoveFrom == 7 || move.MoveTo == 7)
            {
                BlackKingSide = false;
            }
            if (move.MoveFrom == 56 || move.MoveTo == 56)
            {
                WhiteQueenSide = false;
            }
            if (move.MoveFrom == 63 || move.MoveTo == 63)
            {
                WhiteKingSide = false;
            }

            if (move.Piece == (Piece.White | Piece.King)) 
            {
                WhiteKingHasMoved = true;
                WhiteQueenSide = false;
                WhiteKingSide = false;
            }
            if (move.Piece == (Piece.Black | Piece.King))
            {
                BlackKingHasMoved = true;
                BlackQueenSide = false;
                BlackKingSide = false;
            }
        }

    }
}