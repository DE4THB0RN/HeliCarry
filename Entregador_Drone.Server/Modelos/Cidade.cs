namespace Entregador_Drone.Server.Modelos
{
    public class C_No
    {
        public int Id { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public bool IsBase { get; set; } = false;
        public bool IsObstaculo { get; set; } = false;
        public int CidadeId { get; set; }
        public Cidade Cidade { get; set; }


    }

    public class Cidade
    {
        public int Id { get; set; }
        public ICollection<C_No> Nos { get; set; }

        // Busca um nó pela coordenada X, Y
        public C_No ObterNo(int x, int y)
        {
            return Nos.FirstOrDefault(n => n.X == x && n.Y == y);
        }

        // Retorna vizinhos de um nó, ignorando obstáculos
        public IEnumerable<C_No> ObterVizinhos(C_No no)
        {
            if (no.IsObstaculo) yield break; // não expande nós que são obstáculos

            var offsets = new (int dx, int dy)[]
            {
                (1,0), (-1,0), (0,1), (0,-1) // 4 direções
            };

            foreach (var (dx, dy) in offsets)
            {
                var vizinho = Nos.FirstOrDefault(n => n.X == no.X + dx && n.Y == no.Y + dy);
                if (vizinho != null && !vizinho.IsObstaculo) // Ignora obstáculos
                    yield return vizinho;
            }
        }

        // Obtém a base da cidade (único nó com IsBase = true)
        public C_No ObterBase()
        {
            return Nos.FirstOrDefault(n => n.IsBase);
        }
    }
}
