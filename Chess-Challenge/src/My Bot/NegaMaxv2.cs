using ChessChallenge.API;
using System;


// [x] iterative deepening
// eval sort moves
// quiescence search


public class NegaMaxv2 : IChessBot
{
    // Piece values: null, pawn, knight, bishop, rook, queen, king
    int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };
    int infinity = 1000000000;

    int GetAttacks(Board board, bool isWhite, ulong occupiedSquares)
    {
        int squares_attacked = 0;
        foreach (Piece pawn in board.GetPieceList(PieceType.Pawn, isWhite))
        {
            squares_attacked += BitboardHelper.GetNumberOfSetBits(BitboardHelper.GetPawnAttacks(pawn.Square, isWhite));
        }
        foreach (Piece knight in board.GetPieceList(PieceType.Knight, isWhite))
        {
            squares_attacked += BitboardHelper.GetNumberOfSetBits(BitboardHelper.GetKnightAttacks(knight.Square));
        }
        foreach (Piece bishop in board.GetPieceList(PieceType.Bishop, isWhite))
        {
            squares_attacked += BitboardHelper.GetNumberOfSetBits(BitboardHelper.GetSliderAttacks(PieceType.Bishop, bishop.Square, occupiedSquares));
        }
        foreach (Piece rook in board.GetPieceList(PieceType.Rook, isWhite))
        {
            squares_attacked += BitboardHelper.GetNumberOfSetBits(BitboardHelper.GetSliderAttacks(PieceType.Rook, rook.Square, occupiedSquares));
        }
        foreach (Piece queen in board.GetPieceList(PieceType.Queen, isWhite))
        {
            squares_attacked += BitboardHelper.GetNumberOfSetBits(BitboardHelper.GetSliderAttacks(PieceType.Queen, queen.Square, occupiedSquares));
        }
        return squares_attacked;
    }

    int GetUniqueAttacks(Board board, bool isWhite, ulong occupiedSquares)
    {
        ulong squares_attacked = 0;

        foreach (Piece pawn in board.GetPieceList(PieceType.Pawn, isWhite))
        {
            squares_attacked |= BitboardHelper.GetPawnAttacks(pawn.Square, isWhite);
        }
        foreach (Piece knight in board.GetPieceList(PieceType.Knight, isWhite))
        {
            squares_attacked |= BitboardHelper.GetKnightAttacks(knight.Square);
        }
        foreach (Piece bishop in board.GetPieceList(PieceType.Bishop, isWhite))
        {
            squares_attacked |= BitboardHelper.GetSliderAttacks(PieceType.Bishop, bishop.Square, occupiedSquares);
        }
        foreach (Piece rook in board.GetPieceList(PieceType.Rook, isWhite))
        {
            squares_attacked |= BitboardHelper.GetSliderAttacks(PieceType.Rook, rook.Square, occupiedSquares);
        }
        foreach (Piece queen in board.GetPieceList(PieceType.Queen, isWhite))
        {
            squares_attacked |= BitboardHelper.GetSliderAttacks(PieceType.Queen, queen.Square, occupiedSquares);
        }

        return BitboardHelper.GetNumberOfSetBits(squares_attacked);
    }

    //     Console.WriteLine(squares_attacked);
    //     return squares_attacked;
    // }

    int evaluate(Board board)
    {



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

        // number of squares attacked
        score += (GetAttacks(board, true, board.AllPiecesBitboard) - GetAttacks(board, false, board.AllPiecesBitboard))*25;
        score += (GetUniqueAttacks(board, true, board.AllPiecesBitboard) - GetUniqueAttacks(board, false, board.AllPiecesBitboard))*25;

        return score * (board.IsWhiteToMove ? 1 : -1);
    }

    public int RankMove(Board board, Move move)
    {

        // PieceType movedPiece = move.MovePieceType;
        int capPieceValue = pieceValues[(int)move.CapturePieceType];
        // int sacrificedPieceValue = board.SquareIsAttackedByOpponent(move.TargetSquare)  ? pieceValues[(int)movedPiece] : 0;

        int enPassantScore = move.IsEnPassant ? 500 : 0;
        // int castlingScore = move.IsCastles ? 200 : 0;
        int promotionScore = pieceValues[(int)move.PromotionPieceType];
        // int queenCaptureScore = move.CapturePieceType == PieceType.Queen ? capPieceValue/2 : 0;

        // int pawnMoveScore = movedPiece == PieceType.Pawn ? 10 : 0;

        // // score for target square is close to midle
        // int moveToTheMiddleScore = move.TargetSquare.File is >=2 and <=5 ? 10 : 0;
        return capPieceValue + promotionScore + enPassantScore;
    }

    private static int CompareMove(Move moveA, Move moveB)
    {
        // board.moveGen.GetOpponentAttackMap(board)
        if (moveA.IsEnPassant && !moveB.IsEnPassant) return 1;
        if (!moveA.IsEnPassant && moveB.IsEnPassant) return -1;

        // If capture
        if ((int)moveA.CapturePieceType > 0)
        {
            // Capture with the smaller piece
            if ((int)moveA.CapturePieceType > (int)moveB.CapturePieceType ) return 1;
            if ((int)moveA.CapturePieceType < (int)moveB.CapturePieceType ) return -1;
            if ((int)moveA.MovePieceType < (int)moveB.MovePieceType ) return 1;
            if ((int)moveA.MovePieceType > (int)moveB.MovePieceType ) return -1;
        }

        return 0;
    }


    public int NegaMax(Board board, int depth, int alpha, int beta)
    {
        Move[] moves = board.GetLegalMoves();
        if (moves.Length == 0) {
             // checkmate
            if (board.IsInCheckmate())
            {
                return board.IsWhiteToMove ? board.GameMoveHistory.Length -infinity : board.GameMoveHistory.Length +infinity;
            }
        }
        if (depth <=0 ) return evaluate(board);

        int bestValue = -infinity;
        // Array.Sort(moves, (x, y) => RankMove(board, y).CompareTo(RankMove(board, x)));
        Array.Sort(moves, (x, y) => CompareMove(x, y));
        foreach (Move move in moves)
        {
            board.MakeMove(move);
            int score = -NegaMax(board, depth - 1, -beta, -alpha);
            board.UndoMove(move);

            bestValue = Math.Max(bestValue, score);
            alpha = Math.Max(alpha, bestValue);

            if (beta <= alpha)
                break;
        }

        return bestValue;
    }


    public Move FindBestMove(Board board, Move[] moves, int depth, bool isWhite)
    {
        // !! FIND THE BEST MOVE //
        int bestValue = -infinity;
        Move bestMove = moves[0];
        foreach (Move move in board.GetLegalMoves())
        {
            board.MakeMove(move);
            int score = -NegaMax(board, depth - 1, -infinity + 1, infinity - 1);
            board.UndoMove(move);

            if (score > bestValue)
            {
                bestValue = score;
                bestMove = move;
            }
        }

        return bestMove;
    }

    public Move Think(Board board, Timer timer)
    {

        Move[] moves = board.GetLegalMoves();
        // Array.Sort(moves, (x, y) => RankMove(board, y).CompareTo(RankMove(board, x)));
        Array.Sort(moves, (x, y) => CompareMove(x, y));

        Move bestMove = moves[0];
        for (int depth = 1; depth <= 10; depth++)
        {
            bestMove = FindBestMove(board, moves, depth, board.IsWhiteToMove);
            Console.WriteLine($"Depth: {depth} Best move: {bestMove}");
            if ((timer.MillisecondsElapsedThisTurn > (timer.GameStartTimeMilliseconds/200) || timer.MillisecondsElapsedThisTurn > (timer.MillisecondsRemaining / 10)))
            {
                Console.WriteLine("Time is up!");
                return bestMove;
            }
        }
        return bestMove;
    }
}