using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;


namespace tablero1.Pages
{

    // logica de clase para cada casilla 
    public enum TipoCasilla
    {
         Inicio, 
         Obstaculo,
         Fin,
         Vacio
    }
    public class Casilla
    {

        public int X { get; set; }
        public int Y { get; set; }
        public TipoCasilla Tipo { get; set; }

     

        public override string ToString()
        {

            return $"({X}, {Y}, {Tipo})";
        }

    }


    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

     
        public void OnGet()
        {

        }

        // cambiamos el tipo de casilla en base a la seleccion


        // creamos la variable de tablero (null)
        public Casilla[,] Tablero { get; set; }
        public Casilla[,] TableroCaminos { get; set; }

        // creamos las variables finales
       public  List<List<Casilla>> CaminosOptimos { get; set; }
       public List<List<Casilla>> CaminosOptiomosDibujar { get; set; }
       public int CaminosTotales { get; set; }
       public  int CaminiosBuenos { get; set; }
       public  int CaminosOptimosTotal {  get; set; }
       public int PesoOptimo { get; set; }

        // generamos el tablero con las dimenciones del HTML

        public IActionResult OnPostGenerarTablaInicial(int dimenciones)
        {
        
            if (dimenciones < 3)
            {
                ModelState.AddModelError("tamaño", "El tamaño del tablero debe ser mayor a 3.");
                return Page();
            }

            if (dimenciones > 6) {
                ModelState.AddModelError("tamaño", "El tamaño del tablero debe ser menor o igual a 6");
                return Page();
            }

            // Generacion de un tablero de casillas 
            Tablero = new Casilla[dimenciones, dimenciones];
            // Se agrega cada casilla al tablero
            for (int i = 0; i < dimenciones; i++)
            {
                for (int j = 0; j < dimenciones; j++)
                {
                    Tablero[i, j] = new Casilla() { X = i, Y = j, Tipo= TipoCasilla.Vacio};
                }
            }

            return Page();
        }

        public int CalcRaiz(int x)
        {
            if (x == 0)
            {
                return 0;
            }

            int prev = 0;
            int result = 1;

            while (result != prev)
            {
                prev = result;
                result = (x / result + result) / 2;
            }

            return result;
        }

        public (Casilla inicio, List<Casilla> destinos) EncuentraInicioYFin()
        {
            Casilla inicio = null;
            List<Casilla> destinos = new List<Casilla>();

            for (int i = 0; i < TableroCaminos.GetLength(0); i++)
            {
                for (int j = 0; j < TableroCaminos.GetLength(1); j++)
                {
                    if (TableroCaminos[i, j].Tipo == TipoCasilla.Inicio)
                    {
                        inicio = TableroCaminos[i, j];
                    }
                    else if (TableroCaminos[i, j].Tipo == TipoCasilla.Fin)
                    {
                        destinos.Add(TableroCaminos[i, j]);
                    }
                }
            }

            return (inicio, destinos);
        }



        public ( List<List<Casilla>>, int) EncuentraCaminos()
        {
            var (inicio, destinos) = EncuentraInicioYFin();

            if (inicio == null || destinos.Count == 0)
            {
                throw new InvalidOperationException("No se encontraron casillas de inicio y/o fin en el laberinto.");
            }

            List<List<Casilla>> caminosOptimos = new List<List<Casilla>>();
            List<Casilla> caminoActual = new List<Casilla>();
            int contador = 0;
            contador = DFS(inicio, destinos, caminoActual, caminosOptimos, contador);

            return (caminosOptimos, contador);
        }

        private int DFS(Casilla casillaActual, List<Casilla> destinos, List<Casilla> caminoActual, List<List<Casilla>> caminosOptimos, int contador)
        {
            int x = casillaActual.X;
            int y = casillaActual.Y;
            int maxX = TableroCaminos.GetLength(0);
            int maxY = TableroCaminos.GetLength(1);
            Console.WriteLine("Analizando camino: " + contador);
            // Si la casilla está fuera de los límites, retornar
            if (x < 0 || y < 0 || x >= maxX || y >= maxY)
            {
                Console.WriteLine("---Topo con limite---");
                contador++;
                return contador;
            }; ;

            // Si la casilla es un obstáculo o ya ha sido visitada, retornar
            if (casillaActual.Tipo == TipoCasilla.Obstaculo) {
                Console.WriteLine("---Topo con obstaculo---");
                contador++;
                return contador;
            };

            // Si la casilla es un obstáculo o ya ha sido visitada, retornar
            if (caminoActual.Contains(casillaActual))
            {
                Console.WriteLine("---Intento regresar---");
                contador++;
                return contador;
            };

            // Agregar la casilla actual al camino
            caminoActual.Add(casillaActual);

            // Incrementar el contador siempre que se explora un nuevo camino




            // Si hemos llegado al fin, agregar el camino a la lista de caminos y retornar
            if (destinos.Contains(casillaActual))
            {
                caminosOptimos.Add(new List<Casilla>(caminoActual));
                contador++;
                Console.WriteLine("--- Encontro un final ---");
            }
            else
            {
                // Realizar DFS en las casillas vecinas si están dentro de los límites y sumar los caminos encontrados al contador
                if (x + 1 < maxX) contador = DFS(TableroCaminos[x + 1, y], destinos, caminoActual, caminosOptimos, contador);
                if (x - 1 >= 0) contador = DFS(TableroCaminos[x - 1, y], destinos, caminoActual, caminosOptimos, contador);
                if (y + 1 < maxY) contador = DFS(TableroCaminos[x, y + 1], destinos, caminoActual, caminosOptimos, contador);
                if (y - 1 >= 0) contador = DFS(TableroCaminos[x, y - 1], destinos, caminoActual, caminosOptimos, contador);
            }
        

            // Eliminar la casilla actual del camino para seguir explorando otros caminos
            caminoActual.RemoveAt(caminoActual.Count - 1);
          
            return contador;
        }

        public (int, int,  List<List<Casilla>> CaminosParaDibujar) ContarCaminosOptimos(List<List<Casilla>> caminos)
        {
            if (caminos == null || caminos.Count == 0)
            {
                throw new InvalidOperationException("No se encontraron caminos.");
            }

            // Encuentra la longitud mínima entre todos los caminos
            int minLongitud = caminos.Min(camino => camino.Count);

            // Cuenta cuántos caminos tienen la longitud mínima
            List<List<Casilla>> caminosOptimos = caminos.Where(camino => camino.Count == minLongitud).ToList();

            return (caminosOptimos.Count, minLongitud - 1, caminosOptimos);
        }
        public IActionResult OnPostActualizarTipoDeCasilla(string[] casillas)  
        {
            int dimenciones = CalcRaiz(casillas.Length);
            // Generacion de un tablero de casillas 
            TableroCaminos = new Casilla[dimenciones, dimenciones];

            foreach (var casilla in casillas)
            {
                
                var tempCasilla = casilla.Split("|");
                Console.WriteLine(casilla);
                int i = int.Parse(tempCasilla[0]);
                int j = int.Parse(tempCasilla[1]);
                TipoCasilla tipo = TipoCasilla.Vacio;
                if (tempCasilla[2] == "Inicio") tipo = TipoCasilla.Inicio;
                if (tempCasilla[2] == "Fin") tipo = TipoCasilla.Fin;
                if (tempCasilla[2] == "Obstaculo") tipo = TipoCasilla.Obstaculo;

                TableroCaminos[i, j] = new Casilla() { X = i, Y = j, Tipo = tipo };
            }

            var (caminos, contador) = EncuentraCaminos();
            var (caminosOptimosTotal, pesoOptimo, caminosOptiomosDibujar) = ContarCaminosOptimos(caminos);
            CaminiosBuenos = 0;
            CaminosTotales = contador;
            CaminosOptimosTotal = caminosOptimosTotal;
            PesoOptimo = pesoOptimo;
            CaminosOptiomosDibujar = caminosOptiomosDibujar;
            Console.WriteLine("--- Caminos que llegan al final ---");
            foreach (var camino in caminos)
            {
                CaminiosBuenos++;
                Console.WriteLine(string.Join(" -> ", camino));
            }
            Console.WriteLine("--- Caminos optimos ---");
            foreach (var camino in caminosOptiomosDibujar)
            {
                Console.WriteLine(string.Join(" -> ", camino));
            }
            return Page();
        }
    }

  

    
}