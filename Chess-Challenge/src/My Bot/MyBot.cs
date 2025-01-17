﻿using ChessChallenge.API;
using System;


// [x] iterative deepening
// [x] eval sort moves
// [ ] mate in one
// [ ] transposition table
// [ ] killer moves
// [ ] quiescence search
// [ ] history heuristic
// [ ] null move pruning

public class TranspositionEntry
{
    public ulong Key { get; set; }
    public int Depth { get; set; }
    public int Score { get; set; }
    public int Alpha { get; set; }
    public int Beta { get; set; }
}

public class MyBot : IChessBot
{
    // Piece values: null, pawn, knight, bishop, rook, qumeen, king
    int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };
    int infinity = 1000000000;
    int NodesSearched = 0;
    Move bestMove = new Move();
    static int transpositionTableSize = 268435456;
    TranspositionEntry[] transpositionTable = new TranspositionEntry[transpositionTableSize]; // 2^16

    int evaluate(Board board, int ply)
    {
        if (board.IsInCheckmate()) return (board.IsWhiteToMove ? ply-infinity : infinity-ply);

        // evaluate a positive score for the player who made the move
        int score = (
            (
                BitboardHelper.GetNumberOfSetBits(board.GetPieceBitboard(PieceType.Pawn, true))
                - BitboardHelper.GetNumberOfSetBits(board.GetPieceBitboard(PieceType.Pawn, false))
            ) * pieceValues[1] +
            (
                BitboardHelper.GetNumberOfSetBits(board.GetPieceBitboard(PieceType.Knight, true))
                - BitboardHelper.GetNumberOfSetBits(board.GetPieceBitboard(PieceType.Knight, false))
            ) * pieceValues[2] +
            (
                BitboardHelper.GetNumberOfSetBits(board.GetPieceBitboard(PieceType.Bishop, true))
                - BitboardHelper.GetNumberOfSetBits(board.GetPieceBitboard(PieceType.Bishop, false))
            ) * pieceValues[3] +
            (
                BitboardHelper.GetNumberOfSetBits(board.GetPieceBitboard(PieceType.Rook, true))
                - BitboardHelper.GetNumberOfSetBits(board.GetPieceBitboard(PieceType.Rook, false))
            ) * pieceValues[4] +
            (
                BitboardHelper.GetNumberOfSetBits(board.GetPieceBitboard(PieceType.Queen, true))
                - BitboardHelper.GetNumberOfSetBits(board.GetPieceBitboard(PieceType.Queen, false))
            ) * pieceValues[5]
        );

        // // number of squares attacked
        // score += (GetAttacks(board, true, board.AllPiecesBitboard) - GetAttacks(board, false, board.AllPiecesBitboard))*25;
        // score += (GetUniqueAttacks(board, true, board.AllPiecesBitboard) - GetUniqueAttacks(board, false, board.AllPiecesBitboard))*25;

        return score * (board.IsWhiteToMove ? 1 : -1);
    }

    public  int RankMove(Board board, Move move)
    {

        // int checkScore = (BitboardHelper.GetPieceAttacks(move.MovePieceType, move.TargetSquare, board.AllPiecesBitboard, board.IsWhiteToMove)
        //     & board.GetPieceBitboard(PieceType.King, !board.IsWhiteToMove)) != 0 ? 10000 : 0;

        // PieceType movedPiece = move.MovePieceType;
        int capPieceValue = pieceValues[(int)move.CapturePieceType];
        int sacrificedPieceValue = board.SquareIsAttackedByOpponent(move.TargetSquare)  ? pieceValues[(int)move.MovePieceType] : 0;

        int enPassantScore = move.IsEnPassant ? 500 : 0;
        int castlingScore = move.IsCastles ? 200 : 0;
        int promotionScore = pieceValues[(int)move.PromotionPieceType];
        // int queenCaptureScore = move.CapturePieceType == PieceType.Queen ? capPieceValue/2 : 0;

        // int pawnMoveScore = movedPiece == PieceType.Pawn ? 10 : 0;

        // // score for target square is close to midle
        // int moveToTheMiddleScore = move.TargetSquare.File is >=2 and <=5 ? 10 : 0;
        return capPieceValue + promotionScore + enPassantScore - sacrificedPieceValue;
    }

    public void StoreTransposition(Board board, int value, int depth, int alpha, int beta ){

        int key = (int)board.ZobristKey & (transpositionTableSize-1);

        if (transpositionTable[key] != null) {
            Console.WriteLine($"Collision: old value {transpositionTable[key].Score}, new value {value}");
        }

        transpositionTable[key] = new TranspositionEntry{
            Key = board.ZobristKey,
            Depth = depth,
            Score = value,
            Alpha = alpha,
            Beta = beta
        };
    }

    public int NegaMax(Board board, int ply, int depth, int alpha, int beta, bool maxPlayer)
    {
        // Evaluated a node
        NodesSearched++;
        int score;
        bool storeTransposition = true;

        if (depth <=0 )
        {
            score = evaluate(board, ply);
            StoreTransposition(board, score, depth, alpha, beta);
            return score;
        }
        Move[] moves = board.GetLegalMoves();

        int alphaOrig = alpha;
        int bestValue = -infinity;
        Array.Sort(moves, (x, y) => RankMove(board, y).CompareTo(RankMove(board, x)));
        // Array.Sort(moves, (x, y) => CompareMove(board, y, x));
        foreach (Move move in moves)
        {
            board.MakeMove(move);

            // transposition table
            // This is wrong, we need to use the transpositionTable for sorting moves!!
            TranspositionEntry entry = transpositionTable[(int)board.ZobristKey & (transpositionTableSize-1)];
            if (entry != null && entry.Depth <= depth) {
                Console.WriteLine($"Transposition hit: {entry.Score}");
                storeTransposition = false;
                score = entry.Score;
            } else {
                score = -NegaMax(board, ply+1, depth - 1, -beta, -alpha, !maxPlayer);
            }

            // score = -NegaMax(board, ply+1, depth - 1, -beta, -alpha, !maxPlayer);
            board.UndoMove(move);

            // single call
            if (score > bestValue)
            {
                bestValue = score;
                if (ply == 1) {
                    bestMove = move;
                }
            }

            if (storeTransposition) StoreTransposition(board, bestValue, depth, alphaOrig, beta);

            // bestValue = Math.Max(bestValue, score);
            alpha = Math.Max(alpha, bestValue);
            if (beta <= alpha)
                break;
        }

        return bestValue;
    }


    public Move Think(Board board, Timer timer)
    {
        NodesSearched = 0;
        // Array.Clear(transpositionTable);
        int initialAlpha = -infinity;
        int initialBeta = infinity;

        // single call
        for (int depth = 1; depth <= 8; depth++)
        {
            NegaMax(board, 1, depth - 1, initialAlpha, initialBeta, true);
            // Console.WriteLine($"Depth: {depth} Best move: {bestMove} - Visitied nodes: {NodesSearched}");

            if ((timer.MillisecondsElapsedThisTurn > (timer.GameStartTimeMilliseconds/200) || timer.MillisecondsElapsedThisTurn > (timer.MillisecondsRemaining / 10)))
            {
                // Console.WriteLine("Time is up! - Visitied nodes: " + NodesSearched);
                Console.WriteLine("Visitied nodes: " + NodesSearched);
                Console.WriteLine("Best move: " + bestMove);
                int transpositionCount = Array.FindAll(transpositionTable, c => c != null).Length;
                Console.WriteLine($"{transpositionCount} transpositions stored, ({transpositionCount * 100 / transpositionTableSize}%)");
                return bestMove;
            }
        }

        // // perf test: position 5
        // rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8
        // Depth: 1 Best move: Move: 'd7c8q' - Visitied nodes: 44
        // Depth: 2 Best move: Move: 'e1f2' - Visitied nodes: 1574
        // Depth: 3 Best move: Move: 'd7c8q' - Visitied nodes: 6543
        // Depth: 4 Best move: Move: 'e1f2' - Visitied nodes: 556201
        // rb5/4r3/3p1npb/3kp1P1/1P3P1P/5nR1/2Q1BK2/bN4NR w - - 3 61 #1
        // r2qkb1r/pp2nppp/3p4/2pNN1B1/2BnP3/3P4/PPP2PPP/R2bK2R w KQkq - 1 0 #2
        Console.WriteLine("Visitied nodes: " + NodesSearched);
        return bestMove;
    }
}