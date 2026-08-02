using System.Linq;

public static class LevelSolver
{
    public static bool Verify(LevelData data)
    {
        Board board = Board.FromLevelData(data);
        int safety = 0;

        while (!board.IsCleared() && safety < 500)
        {
            safety++;
            bool movedAny = false;

            foreach (var v in board.vehicles.Where(x => x.isActive).ToList())
            {
                if (board.TryMoveVehicle(v.id, out int moved) && moved > 0)
                    movedAny = true;
            }

            if (!movedAny) return false; // deadlock thật
        }

        return board.IsCleared();
    }
}