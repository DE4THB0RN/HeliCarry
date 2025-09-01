using Entregador_Drone.Server.Modelos;

namespace Entregador_Drone.Server.Serviços
{
    public class GeradorGrafo
    {
        public static void Initialize(AppDbContext context)
        {
            context.Database.EnsureCreated();

            var cidade = context.Cidade.FirstOrDefault();

            var random = new Random();
            var nos = new List<C_No>();

            int largura = 100;
            int altura = 100;
            bool isBase = false;
            bool isObstaculo = false;

            for (int x = 0; x < largura; x++)
            {
                for (int y = 0; y < altura; y++)
                {
                    if(x == 23 && y == 45) 
                        isBase = true;
                    else
                        isBase = false;
                    
                    if(random.NextDouble() < 0.2) 
                        isObstaculo = true;
                    else
                        isObstaculo = false;

                    nos.Add(new C_No
                    {
                        CidadeId = cidade.Id, 
                        X = x,
                        Y = y,
                        IsBase = isBase,
                        IsObstaculo = isObstaculo,
                        Cidade = cidade
                    });
                }
            }
            cidade.Nos = nos;
            context.Cidade.Update(cidade);
            context.C_No.AddRange(nos);
            context.SaveChanges();
        }
    }
}
