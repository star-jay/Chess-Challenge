using ChessChallenge.API;
using System;
using System.Linq;
using System.Collections.Generic;

// Bee's

// kill bees
// send out bees down the path
// for bee follow scent (rand based on scent strength)
// for bee follow on nectar
// for bee follow random
// watch out for wasps

// update node upstream

// eval move => board, return nectar




// Decision tree bot
// look for checks
// look for captures
// highest scoring move = defensive fortress

// [V] don't capture if can be recaptured
// don't check opp, if can be captured (not checking for check atm)

// Bad approach

// Eval board = score for white - score for black (* depth)

// for each moving piece, get the squares it was defending
// for each of those squares, get the pieces that are on those squares
// get the scores of the pieces on those squares
// move the piece that is defending the least
// move to undefended squares




public class Flower
{
    Board board;
    public Move lastMove;
    public bool isWhiteToMove = true;
    public bool weAreWhite = true;

    public int scent = 0;
    public int nectar = 0;

    public Flower? parent;
    public List<Flower> trail = new List<Flower>();

    int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };
    int[] pieceDefensiveValues = { 5, 10, 30, 30, 50, 90, 0 };

    public Flower(Board board, Move lastMove, Flower? parent = null)
    {
        // maybe not best practive to pass in board
        this.lastMove = lastMove;
        this.parent = parent;

        // The scent is
        if (this.lastMove.IsNull is false) {
            this.scent = Smell(board, lastMove);
            board.MakeMove(this.lastMove);
            this.nectar = Harvest(board, this.lastMove);
            board.UndoMove(this.lastMove);
            this.weAreWhite = parent.weAreWhite;
        } else {
            this.weAreWhite = board.IsWhiteToMove;
        }
    }

    public void CreateTrail(Board board)
    {
        this.isWhiteToMove = board.IsWhiteToMove;
        foreach (Move move in board.GetLegalMoves()) {
            this.trail.Add(new Flower(board, move, this));
        }
    }

    public int Visit(Board board, int age)
    {
        board.MakeMove(this.lastMove);
        // if (this.lastMove.IsNull is false) {
        //     board.MakeMove(this.lastMove);
        // } else {
        //     board.SkipTurn();
        // }

        if (this.trail.Count == 0){
            CreateTrail(board);
        }

        age -= 1;
        if (age > 0 && this.trail.Count > 0) {
            Random rng = new();
            Flower visitedFlower = this.trail[rng.Next(this.trail.Count)];
            int pollenDownTheTrail = visitedFlower.Visit(board, age);

            Console.WriteLine(visitedFlower.lastMove.ToString() + pollenDownTheTrail + board.IsWhiteToMove);

            if (board.IsWhiteToMove != this.weAreWhite) {
                // black played last move
                this.nectar = Math.Max(pollenDownTheTrail, this.nectar);
            } else {
                // white played last move
                this.nectar = Math.Min(pollenDownTheTrail, this.nectar);
            }
        }
        // if (this.lastMove.IsNull is false) {
        //     board.UndoMove(this.lastMove);
        // }
        board.UndoMove(this.lastMove);

        return this.nectar;
    }






    public int Smell(Board board, Move lastMove, bool debug = false)
    {
        PieceType movedPiece = lastMove.MovePieceType;

        int capPieceValue = pieceValues[(int)lastMove.CapturePieceType];
        int sacrificedPieceValue = board.SquareIsAttackedByOpponent(lastMove.TargetSquare)  ? pieceValues[(int)movedPiece] : 0;

        int enPassantScore = lastMove.IsEnPassant ? 500 : 0;
        int castlingScore = lastMove.IsCastles ? 200 : 0;
        int promotionScore = lastMove.PromotionPieceType == PieceType.Queen ? pieceValues[5]/2 : 0;
        int queenCaptureScore = lastMove.CapturePieceType == PieceType.Queen ? capPieceValue/2 : 0;

        int pawnMoveScore = movedPiece == PieceType.Pawn ? 10 : 0;

        // score for target square is close to midle
        int moveToTheMiddleScore = lastMove.TargetSquare.File is >=2 and <=5 ? 10 : 0;

        // after move
        board.MakeMove(lastMove);
        int checkScore = board.IsInCheck() ? 300 : 0;
        int drawScore = board.IsDraw() ? 100 : 0;
        if (board.IsInCheckmate()) {
            board.UndoMove(lastMove);
            return 1000000;
        }
        board.UndoMove(lastMove);

        if (debug) {
            Console.WriteLine($"capPieceValue: {capPieceValue}");
            Console.WriteLine($"sacrificedPieceValue: {sacrificedPieceValue}");
            Console.WriteLine($"enPassantScore: {enPassantScore}");
            Console.WriteLine($"castlingScore: {castlingScore}");
            Console.WriteLine($"promotionScore: {promotionScore}");
            Console.WriteLine($"queenCaptureScore: {queenCaptureScore}");
            Console.WriteLine($"checkScore: {checkScore}");
            Console.WriteLine($"drawScore: {drawScore}");
            Console.WriteLine($"pawnMoveScore: {pawnMoveScore}");
            Console.WriteLine($"moveToTheMiddleScore: {moveToTheMiddleScore}");

            Console.WriteLine($"lastMove.TargetSquare.File: {lastMove.TargetSquare.File}");
            Console.WriteLine($"lastMove.TargetSquare.Rank: {lastMove.TargetSquare.Rank}");

        }

        return capPieceValue - sacrificedPieceValue + enPassantScore + castlingScore + promotionScore + queenCaptureScore - drawScore + pawnMoveScore + checkScore;

    }

    public int Harvest(Board board, Move move, bool recursive = false)
    {


        PieceType movedPiece = lastMove.MovePieceType;

        int piecesScore = BitboardHelper.GetNumberOfSetBits(board.GetPieceBitboard(PieceType.Pawn, true)) * pieceValues[1] +
            BitboardHelper.GetNumberOfSetBits(board.GetPieceBitboard(PieceType.Knight, true)) * pieceValues[2] +
            BitboardHelper.GetNumberOfSetBits(board.GetPieceBitboard(PieceType.Bishop, true)) * pieceValues[3] +
            BitboardHelper.GetNumberOfSetBits(board.GetPieceBitboard(PieceType.Rook, true)) * pieceValues[4] +
            BitboardHelper.GetNumberOfSetBits(board.GetPieceBitboard(PieceType.Queen, true)) * pieceValues[5] -
            BitboardHelper.GetNumberOfSetBits(board.GetPieceBitboard(PieceType.Pawn, false)) * pieceValues[1] -
            BitboardHelper.GetNumberOfSetBits(board.GetPieceBitboard(PieceType.Knight, false)) * pieceValues[2] -
            BitboardHelper.GetNumberOfSetBits(board.GetPieceBitboard(PieceType.Bishop, false)) * pieceValues[3] -
            BitboardHelper.GetNumberOfSetBits(board.GetPieceBitboard(PieceType.Rook, false)) * pieceValues[4] -
            BitboardHelper.GetNumberOfSetBits(board.GetPieceBitboard(PieceType.Queen, false)) * pieceValues[5];

        int capPieceValue = pieceValues[(int)lastMove.CapturePieceType];
        int sacrificedPieceValue = board.SquareIsAttackedByOpponent(lastMove.TargetSquare)  ? pieceValues[(int)movedPiece] : 0;
        int enPassantScore = lastMove.IsEnPassant ? 500 : 0;
        int castlingScore = lastMove.IsCastles ? 200 : 0;
        int promotionScore = lastMove.PromotionPieceType == PieceType.Queen ? pieceValues[5]/2 : 0;
        int queenCaptureScore = lastMove.CapturePieceType == PieceType.Queen ? capPieceValue/2 : 0;
        int pawnMoveScore = movedPiece == PieceType.Pawn ? 10 : 0;
        // score for target square is close to midle
        int moveToTheMiddleScore = lastMove.TargetSquare.File is >=2 and <=5 ? 10 : 0;


        if (this.isWhiteToMove) {
            // + capPieceValue - sacrificedPieceValue
            return piecesScore + enPassantScore + castlingScore + promotionScore + queenCaptureScore + pawnMoveScore + moveToTheMiddleScore;

        } else {
            // - capPieceValue + sacrificedPieceValue
            return piecesScore  - enPassantScore - castlingScore - promotionScore - queenCaptureScore - pawnMoveScore - moveToTheMiddleScore;
        }

    }

    public Flower FollowTheQueen(Board board, int depth=2)
    {
        // follow the queen
        if (depth > 0) {
            foreach (Flower flower in this.trail) {

                if (flower.lastMove.RawValue == board.GameMoveHistory[^depth].RawValue) {
                    Console.WriteLine("Found "+  board.GameMoveHistory[^depth].ToString() + ": " + flower.lastMove.ToString());
                    // recursion
                    return flower.FollowTheQueen(board, depth - 1);
                }
            }
            Console.WriteLine("No flower found");
            return new Flower(board, new Move(), null);
        }
        Console.WriteLine("LastMove "+ board.GameMoveHistory[^1] + ": " + lastMove.ToString());
        return this;
    }

    public Move MakeANewQueen()
    {
        if (this.trail.Count > 0) {
            Flower prettiestFlower = this.trail[0];
            int highestScore = prettiestFlower.nectar;
            foreach (Flower flower in this.trail)
            {
                Console.WriteLine("Flower: " + flower.lastMove.ToString() + ": " + flower.nectar);
                if (this.isWhiteToMove) {
                    if (flower.nectar > highestScore)
                    {
                        prettiestFlower = flower;
                        highestScore = flower.nectar;
                    }
                } else {
                    if (flower.nectar < highestScore)
                    {
                        prettiestFlower = flower;
                        highestScore = flower.nectar;
                    }
                }

            }
            Console.WriteLine("Highest value: " + prettiestFlower.lastMove.ToString() + ": " + prettiestFlower.nectar);
            // prettiestFlower.Smell(board, prettiestFlower.lastMove, true);
            return prettiestFlower.lastMove;
        }
        Console.WriteLine("No Trail Found");
        return new Move();
    }
}


public class LittleBeesBot : IChessBot
{
    // Piece values: null, pawn, knight, bishop, rook, queen, king

    Flower hive;

    public void MoveTheHive(Board board)
    {
        if (this.hive == null) {
            // create hive
            // Console.WriteLine("Creating hive");
            this.hive = new Flower(board, new Move());
        } else {
            this.hive = this.hive.FollowTheQueen(board, 2);
            this.hive.parent = null;
        }
        // Console.WriteLine("A new Hive!" + this.hive.lastMove.ToString() + " " + (this.hive.isWhiteToMove ? "white" : "black"));
        this.hive.lastMove = new Move();

        // Todo
        // [ ] Kill bees that were from wrong branch

        // TODO: Find flower which was the last move
        // hive = board.GameMoveHistory[^1];
        // if (hive.parent != null)
        // {
        //     hive.parent.trail = hive.parent.trail + hive;
        // }
        // hive.parent = null;
    }


    public Move Think(Board board, Timer timer)
    {
        MoveTheHive(board);

        // LayEggs(board);

        int beesInTheHive = 2000;
        int beeAge = 6;

        for (int i = 0; i < beesInTheHive; i++)
        {
            this.hive.Visit(board, beeAge);
        }

        return this.hive.MakeANewQueen();


    }
}
