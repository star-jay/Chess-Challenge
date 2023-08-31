using ChessChallenge.API;
using System;


// [x] iterative deepening
// [x] eval sort moves
// [ ] mate in one
// [ ] transposition table
// [ ] killer moves
// [ ] quiescence search
// [ ] history heuristic
// [ ] null move pruning


public class NegaMaxFinal : IChessBot
{
    // Piece values: null, pawn, knight, bishop, rook, qumeen, king
    int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };
    int infinity = 1000000000;
    int NodesSearched = 0;
    Move bestMove = new Move();

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

    public int NegaMax(Board board, int ply, int depth, int alpha, int beta, bool maxPlayer)
    {
        // Evaluated a node
        NodesSearched++;
        if (depth <=0 ) return evaluate(board, ply);
        Move[] moves = board.GetLegalMoves();

        int bestValue = -infinity;
        int score;
        Array.Sort(moves, (x, y) => RankMove(board, y).CompareTo(RankMove(board, x)));
        // Array.Sort(moves, (x, y) => CompareMove(board, y, x));
        foreach (Move move in moves)
        {
            board.MakeMove(move);

            // if (board.IsInCheckmate())
            // {
            //     score = (ply -infinity) * (board.IsWhiteToMove ? 1 : -1);
            //     if (ply == 1) bestMove = move;
            //     board.UndoMove(move);

            //     alpha = Math.Max(alpha, score); // Update alpha with a high value
            //     return alpha;

            //     if (!maxPlayer)
            //     {
            //         beta = Math.Min(beta, score); // Update beta with a low value
            //         return beta;
            //     } else
            //     {

            //     }

            // }
            score = -NegaMax(board, ply+1, depth - 1, -beta, -alpha, !maxPlayer);
            board.UndoMove(move);

            // single call
            if (score > bestValue)
            {
                bestValue = score;
                if (ply == 1) {
                    bestMove = move;
                    // Console.WriteLine($"Best move {move} - {!board.IsWhiteToMove} - {score}");
                }
            }

            // bestValue = Math.Max(bestValue, score);
            alpha = Math.Max(alpha, bestValue);
            if (beta <= alpha)
                // Console.WriteLine($"CutOff  {depth+1}: {!board.IsWhiteToMove} - {score}: {board.GameMoveHistory }");
                break;
        }

        return bestValue;
    }


    // public Move FindBestMove(Board board, Move[] moves, int depth, bool isWhite)
    // {
    //     // !! FIND THE BEST MOVE //
    //     int bestValue = -infinity;
    //     Move bestMove = moves[0];
    //     foreach (Move move in board.GetLegalMoves())
    //     {
    //         board.MakeMove(move);
    //         int score = -NegaMax(board, 1, depth - 1, -infinity + 1, infinity - 1, false);
    //         board.UndoMove(move);

    //         if (score > bestValue)
    //         {
    //             bestValue = score;
    //             bestMove = move;
    //         }
    //     }

    //     return bestMove;
    // }

    public Move Think(Board board, Timer timer)
    {
        NodesSearched = 0;
        int initialAlpha = -infinity;
        int initialBeta = infinity;
        // Move[] moves = board.GetLegalMoves();
        // Array.Sort(moves, (x, y) => RankMove(board, y).CompareTo(RankMove(board, x)));
        // Array.Sort(moves, (x, y) => CompareMove(board, x, y));


        // single call
        for (int depth = 1; depth <= 8; depth++)
        {
            NegaMax(board, 1, depth - 1, initialAlpha, initialBeta, true);
            // Console.WriteLine($"Depth: {depth} Best move: {bestMove} - Visitied nodes: {NodesSearched}");

            if ((timer.MillisecondsElapsedThisTurn > (timer.GameStartTimeMilliseconds/200) || timer.MillisecondsElapsedThisTurn > (timer.MillisecondsRemaining / 10)))
            {
                // Console.WriteLine("Time is up! - Visitied nodes: " + NodesSearched);
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

        return bestMove;
    }
}