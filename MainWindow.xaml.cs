//.....................................................................................................................
/* Simulador de vida artificial: C# y WPF
* El simulador es básico y se centra en la interacción de criaturas en un entorno con diferentes tipos de terreno y recursos.
* Las criaturas pueden moverse, alimentarse, reproducirse y depredarse entre sí.
* Se incluyen funcionalidades para guardar y cargar el estado de la simulación.
* Autor: FECORO
* Fecha: 2023-09-01
*/
//.....................................................................................................................

// mainwindow.xaml.cs

// ........... stacks:
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace SimuladorVidaArtificial
{
    public partial class MainWindow : Window
    {
        // variables globales
        private int tamanoMapa;
        private Celda[,] mapa = new Celda[0, 0]; // inicializado con un valor predeterminado
        private List<Criatura> criaturas; // inicializado en el constructor
        private DispatcherTimer timer; // inicializado en el constructor
        private Random random; // inicializado en el constructor
        private int contadorGeneraciones;
        private int recursosTotales;
        private DateTime tiempoInicioSimulacion;

        // constantes configurables para el simulador
        public const int EnergiaInicialMin = 10;
        public const int EnergiaInicialMax = 20;
        public const int VelocidadMin = 1;
        public const int VelocidadMax = 5;
        public const int TamanoMin = 1;
        public const int TamanoMax = 5;
        public const int EnergiaReproduccion = 30;
        public const int EnergiaConsumidaPorMovimiento = 1;
        public const int EnergiaGanadaPorRecurso = 5;
        public const int ProbabilidadMutacion = 10; // 10% de probabilidad de mutación

        public MainWindow()
        {
            InitializeComponent();
            criaturas = new List<Criatura>();
            timer = new DispatcherTimer();
            random = new Random();
            InicializarSimulador();
            ConfigurarInterfaz();
        }

        ///...<summary>
        ///...inicializa las variables y configura el timer.
        ///...</summary>
        private void InicializarSimulador()
        {
            timer.Tick += Timer_Tick;
            timer.Interval = TimeSpan.FromMilliseconds(16); // 60 FPS (1000ms / 60 ≈ 16ms)
        }

        ///...<summary>
        ///...configura la interfaz de usuario, como los sliders y el botón de información.
        ///...</summary>
        private void ConfigurarInterfaz()
        {
            // asegurar que los sliders estén en orden de menor a mayor
            SliderTamanoMapa.Minimum = 10;
            SliderTamanoMapa.Maximum = 50;
            SliderTamanoMapa.Value = 20;

            SliderCantidadCriaturas.Minimum = 1;
            SliderCantidadCriaturas.Maximum = 100;
            SliderCantidadCriaturas.Value = 20;

            SliderVelocidadSimulacion.Minimum = 1000;
            SliderVelocidadSimulacion.Maximum = 100;
            SliderVelocidadSimulacion.Value = 1000;

            // hacer visible el botón de información
            BtnInformacion.Visibility = Visibility.Visible;

            // agregar el texto "Made by FECORO" en la esquina inferior derecha
            TextBlock firma = new TextBlock
            {
                Text = "Made by FECORO",
                Foreground = Brushes.White,
                FontSize = 12,
                FontStyle = FontStyles.Italic,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(0, 0, 10, 10)
            };
            GridPrincipal.Children.Add(firma);
        }

        ///...<summary>
        ///...evento que se ejecuta en cada tick del timer, simula un paso.
        ///...</summary>
        private void Timer_Tick(object? sender, EventArgs e)
        {
            SimularPaso();
        }

        ///...<summary>
        ///...inicia la simulación al hacer clic en el botón.
        ///...</summary>
        private void BtnIniciar_Click(object sender, RoutedEventArgs e)
        {
            IniciarSimulacion();
        }

        ///...<summary>
        ///...configura y comienza la simulación.
        ///...</summary>
        private void IniciarSimulacion()
        {
            // detener el timer si está corriendo
            if (timer.IsEnabled)
            {
                timer.Stop();
            }

            // inicializar variables
            contadorGeneraciones = 0;
            tamanoMapa = (int)SliderTamanoMapa.Value;
            MapaCanvas.Children.Clear();
            mapa = new Celda[tamanoMapa, tamanoMapa];
            criaturas.Clear();
            recursosTotales = 0;
            tiempoInicioSimulacion = DateTime.Now;

            // configurar intervalo del timer según la velocidad seleccionada
            timer.Interval = TimeSpan.FromMilliseconds(SliderVelocidadSimulacion.Value);

            // generar mapa
            GenerarMapa();

            // colocar criaturas
            int cantidadCriaturas = (int)SliderCantidadCriaturas.Value;
            for (int i = 0; i < cantidadCriaturas; i++)
            {
                int x = random.Next(0, tamanoMapa);
                int y = random.Next(0, tamanoMapa);

                // crear criatura con atributos aleatorios
                Criatura criatura = new Criatura(x, y, random.Next(EnergiaInicialMin, EnergiaInicialMax),
                                                 random.Next(VelocidadMin, VelocidadMax),
                                                 random.Next(TamanoMin, TamanoMax));

                criaturas.Add(criatura);
                mapa[x, y].CriaturasEnCelda.Add(criatura);
            }

            ActualizarEstadisticas();

            // iniciar el timer
            timer.Start();
        }

        ///...<summary>
        ///...genera el mapa con diferentes tipos de terreno y recursos.
        ///...</summary>
        private void GenerarMapa()
        {
            double anchoCelda = MapaCanvas.ActualWidth / tamanoMapa;
            double altoCelda = MapaCanvas.ActualHeight / tamanoMapa;

            for (int i = 0; i < tamanoMapa; i++)
            {
                for (int j = 0; j < tamanoMapa; j++)
                {
                    // seleccionar tipo de terreno aleatoriamente con probabilidades
                    TipoTerreno tipoTerreno = ObtenerTipoTerrenoAleatorio();

                    Celda celda = new Celda(i, j, tipoTerreno);
                    mapa[i, j] = celda;

                    // crear representación gráfica de la celda
                    Rectangle rect = new Rectangle
                    {
                        Width = anchoCelda,
                        Height = altoCelda,
                        Fill = ObtenerColorTerreno(tipoTerreno, celda),
                        Stroke = Brushes.Black,
                        StrokeThickness = 0.5,
                        Effect = new DropShadowEffect
                        {
                            Color = Colors.Gray,
                            Direction = 320,
                            ShadowDepth = 3,
                            Opacity = 0.5,
                            RenderingBias = RenderingBias.Performance // optimizar sombras para mejorar fps
                        }
                    };

                    // posicionar la celda en el canvas
                    Canvas.SetLeft(rect, i * anchoCelda);
                    Canvas.SetTop(rect, j * altoCelda);
                    MapaCanvas.Children.Add(rect);

                    // generar recursos dependiendo del tipo de terreno
                    if (tipoTerreno == TipoTerreno.Bosque || tipoTerreno == TipoTerreno.Tierra)
                    {
                        int cantidadRecursos = random.Next(0, 3);
                        celda.Recursos = cantidadRecursos;
                        recursosTotales += cantidadRecursos;
                    }
                }
            }
        }

        ///...<summary>
        ///...obtiene un tipo de terreno aleatorio basado en probabilidades.
        ///...</summary>
        private TipoTerreno ObtenerTipoTerrenoAleatorio()
        {
            int valor = random.Next(1, 101); // valor entre 1 y 100

            if (valor <= 10)
                return TipoTerreno.Agua;
            else if (valor <= 40)
                return TipoTerreno.Bosque;
            else if (valor <= 80)
                return TipoTerreno.Tierra;
            else
                return TipoTerreno.Montana;
        }

        ///...<summary>
        ///...obtiene el color correspondiente al tipo de terreno.
        ///...</summary>
        private Brush ObtenerColorTerreno(TipoTerreno tipo, Celda celda)
        {
            SolidColorBrush color;
            switch (tipo)
            {
                case TipoTerreno.Agua:
                    color = Brushes.Blue;
                    break;
                case TipoTerreno.Tierra:
                    color = Brushes.SandyBrown;
                    break;
                case TipoTerreno.Bosque:
                    color = Brushes.Green;
                    break;
                case TipoTerreno.Montana:
                    color = Brushes.Gray;
                    break;
                default:
                    color = Brushes.White;
                    break;
            }

            // si la celda tiene recursos, ajustar el brillo
            if (celda.Recursos > 0)
            {
                Color c = ((SolidColorBrush)color).Color;
                Color recursoColor = Color.FromArgb(255, (byte)Math.Min(c.R + 30, 255), (byte)Math.Min(c.G + 30, 255), (byte)Math.Min(c.B + 30, 255));
                return new SolidColorBrush(recursoColor);
            }

            return color;
        }

        ///...<summary>
        ///...simula un paso en la simulación.
        ///...</summary>
        private void SimularPaso()
        {
            contadorGeneraciones++;
            List<Criatura> nuevasCriaturas = new List<Criatura>();
            List<Criatura> criaturasMuertas = new List<Criatura>();

            foreach (var criatura in criaturas.ToList()) // usar tolist para evitar modificar la colección mientras se itera
            {
                // reducir energía con el tiempo y por movimiento
                criatura.Energia -= EnergiaConsumidaPorMovimiento;

                // comprobar si la criatura muere por falta de energía
                if (criatura.Energia <= 0)
                {
                    criaturasMuertas.Add(criatura);
                    continue;
                }

                // comportamiento de la criatura
                ComportamientoCriatura(criatura, nuevasCriaturas);
            }

            // remover criaturas muertas
            foreach (var muerta in criaturasMuertas)
            {
                mapa[muerta.X, muerta.Y].CriaturasEnCelda.Remove(muerta);
                criaturas.Remove(muerta);
            }

            // agregar nuevas criaturas
            foreach (var nueva in nuevasCriaturas)
            {
                criaturas.Add(nueva);
                mapa[nueva.X, nueva.Y].CriaturasEnCelda.Add(nueva);
            }

            ActualizarEstadisticas();

            if (criaturas.Count == 0)
            {
                timer.Stop();
                MessageBox.Show("Todas las creaturas han muerto.");
            }

            // redibujar criaturas
            DibujarCriaturas();
        }

        ///...<summary>
        ///...define el comportamiento de una criatura.
        ///...</summary>
        private void ComportamientoCriatura(Criatura criatura, List<Criatura> nuevasCriaturas)
        {
            // lógica de movimiento basada en la velocidad
            for (int i = 0; i < criatura.Velocidad; i++)
            {
                // generar movimiento aleatorio
                int deltaX = random.Next(-1, 2);
                int deltaY = random.Next(-1, 2);

                int nuevoX = criatura.X + deltaX;
                int nuevoY = criatura.Y + deltaY;

                // verificar límites del mapa
                if (nuevoX < 0 || nuevoX >= tamanoMapa || nuevoY < 0 || nuevoY >= tamanoMapa)
                    continue;

                // verificar si el terreno es transitable
                if (mapa[nuevoX, nuevoY].TipoTerreno == TipoTerreno.Montana || mapa[nuevoX, nuevoY].TipoTerreno == TipoTerreno.Agua)
                    continue;

                // actualizar posición
                mapa[criatura.X, criatura.Y].CriaturasEnCelda.Remove(criatura);
                criatura.X = nuevoX;
                criatura.Y = nuevoY;
                mapa[criatura.X, criatura.Y].CriaturasEnCelda.Add(criatura);

                // interacciones con el entorno
                Celda celdaActual = mapa[criatura.X, criatura.Y];

                // alimentarse si hay recursos
                if (celdaActual.Recursos > 0)
                {
                    criatura.Energia += EnergiaGanadaPorRecurso;
                    celdaActual.Recursos--;
                    recursosTotales--;
                }

                // interacciones con otras criaturas
                if (celdaActual.CriaturasEnCelda.Count > 1)
                {
                    foreach (var otraCriatura in celdaActual.CriaturasEnCelda.ToList())
                    {
                        if (otraCriatura != criatura)
                        {
                            // ejemplo: depredación basada en tamaño
                            if (criatura.Tamano > otraCriatura.Tamano + 1)
                            {
                                criatura.Energia += otraCriatura.Energia / 2;
                                criaturas.Remove(otraCriatura);
                                mapa[otraCriatura.X, otraCriatura.Y].CriaturasEnCelda.Remove(otraCriatura);
                            }
                            else if (otraCriatura.Tamano > criatura.Tamano + 1)
                            {
                                otraCriatura.Energia += criatura.Energia / 2;
                                criaturas.Remove(criatura);
                                mapa[criatura.X, criatura.Y].CriaturasEnCelda.Remove(criatura);
                                return;
                            }
                        }
                    }
                }
            }

            // reproducción
            if (criatura.Energia > EnergiaReproduccion)
            {
                Criatura descendencia = criatura.Reproducirse(random, ProbabilidadMutacion);
                nuevasCriaturas.Add(descendencia);
                criatura.Energia /= 2;
            }
        }

        ///...<summary>
        ///...dibuja las criaturas en el mapa.
        ///...</summary>
        private void DibujarCriaturas()
        {
            // remover criaturas visuales actuales
            var elementosARemover = MapaCanvas.Children.OfType<Ellipse>().ToList();
            foreach (var elemento in elementosARemover)
            {
                MapaCanvas.Children.Remove(elemento);
            }

            double anchoCelda = MapaCanvas.ActualWidth / tamanoMapa;
            double altoCelda = MapaCanvas.ActualHeight / tamanoMapa;

            // dibujar cada criatura como una elipse
            foreach (var criatura in criaturas)
            {
                Ellipse elipse = new Ellipse
                {
                    Width = criatura.Tamano * 3,
                    Height = criatura.Tamano * 3,
                    Fill = ObtenerColorCriatura(criatura),
                    Effect = new DropShadowEffect
                    {
                        Color = Colors.Black,
                        Direction = 320,
                        ShadowDepth = 5,
                        Opacity = 0.7,
                        RenderingBias = RenderingBias.Performance // optimizar sombras para mejorar fps
                    }
                };

                Canvas.SetLeft(elipse, criatura.X * anchoCelda + (anchoCelda / 2) - elipse.Width / 2);
                Canvas.SetTop(elipse, criatura.Y * altoCelda + (altoCelda / 2) - elipse.Height / 2);
                MapaCanvas.Children.Add(elipse);
            }
        }

        ///...<summary>
        ///...obtiene el color de la criatura según sus características.
        ///...</summary>
        private Brush ObtenerColorCriatura(Criatura criatura)
        {
            // ejemplo: color basado en la velocidad y tamaño
            if (criatura.Velocidad >= 4)
            {
                return Brushes.Yellow;
            }
            else if (criatura.Tamano >= 4)
            {
                return Brushes.Red;
            }
            else
            {
                return Brushes.Purple;
            }
        }

        ///...<summary>
        ///...actualiza las estadísticas mostradas en la interfaz.
        ///...</summary>
        private void ActualizarEstadisticas()
        {
            TxtNumeroCriaturas.Text = $"Número de criaturas vivas: {criaturas.Count}";
            TxtGeneraciones.Text = $"Número de generaciones: {contadorGeneraciones}";
            TxtRecursosRestantes.Text = $"Recursos restantes en el entorno: {recursosTotales}";
            TimeSpan tiempoTranscurrido = DateTime.Now - tiempoInicioSimulacion;
            TxtTiempoSimulacion.Text = $"Tiempo de simulación: {tiempoTranscurrido.TotalSeconds:F1}s";
        }

        ///...<summary>
        ///...pausa o reanuda la simulación.
        ///...</summary>
        private void BtnPausar_Click(object sender, RoutedEventArgs e)
        {
            if (timer.IsEnabled)
            {
                timer.Stop();
                BtnPausar.Content = "Reanudar simulación";
            }
            else
            {
                timer.Start();
                BtnPausar.Content = "Pausar simulación";
            }
        }

        ///...<summary>
        ///...reinicia la simulación.
        ///...</summary>
        private void BtnReiniciar_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            IniciarSimulacion();
            BtnPausar.Content = "Pausar simulación";
        }

        ///...<summary>
        ///...guarda el estado actual de la simulación en un archivo.
        ///...</summary>
        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            EstadoSimulacion estado = new EstadoSimulacion
            {
                Criaturas = criaturas,
                ContadorGeneraciones = contadorGeneraciones,
                RecursosTotales = recursosTotales,
                TamanoMapa = tamanoMapa,
                Mapa = mapa,
                TiempoInicioSimulacion = tiempoInicioSimulacion
            };

            string json = JsonSerializer.Serialize(estado, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText("simulacion_guardada.json", json);
            MessageBox.Show("Simulación guardada correctamente.");
        }

        ///...<summary>
        ///...carga una simulación guardada desde un archivo.
        ///...</summary>
        private void BtnCargar_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists("simulacion_guardada.json"))
            {
                string json = File.ReadAllText("simulacion_guardada.json");
                EstadoSimulacion? estado = JsonSerializer.Deserialize<EstadoSimulacion>(json);

                if (estado != null)
                {
                    // restaurar estado
                    timer.Stop();
                    contadorGeneraciones = estado.ContadorGeneraciones;
                    tamanoMapa = estado.TamanoMapa;
                    mapa = estado.Mapa;
                    criaturas = estado.Criaturas;
                    recursosTotales = estado.RecursosTotales;
                    tiempoInicioSimulacion = estado.TiempoInicioSimulacion;

                    SliderTamanoMapa.Value = tamanoMapa;
                    SliderCantidadCriaturas.Value = criaturas.Count;

                    // redibujar mapa y criaturas
                    MapaCanvas.Children.Clear();
                    GenerarMapaDesdeEstado();
                    DibujarCriaturas();
                    ActualizarEstadisticas();
                    timer.Start();
                    BtnPausar.Content = "Pausar simulación";
                }
                else
                {
                    MessageBox.Show("El archivo de simulación guardada está corrupto.");
                }
            }
            else
            {
                MessageBox.Show("No se encontró ningún archivo de simulación guardada.");
            }
        }

        ///...<summary>
        ///...genera el mapa a partir de un estado guardado.
        ///...</summary>
        private void GenerarMapaDesdeEstado()
        {
            double anchoCelda = MapaCanvas.ActualWidth / tamanoMapa;
            double altoCelda = MapaCanvas.ActualHeight / tamanoMapa;

            for (int i = 0; i < tamanoMapa; i++)
            {
                for (int j = 0; j < tamanoMapa; j++)
                {
                    Celda celda = mapa[i, j];
                    Rectangle rect = new Rectangle
                    {
                        Width = anchoCelda,
                        Height = altoCelda,
                        Fill = ObtenerColorTerreno(celda.TipoTerreno, celda),
                        Stroke = Brushes.Black,
                        StrokeThickness = 0.5,
                        Effect = new DropShadowEffect
                        {
                            Color = Colors.Gray,
                            Direction = 320,
                            ShadowDepth = 3,
                            Opacity = 0.5,
                            RenderingBias = RenderingBias.Performance // optimizar sombras para mejorar fps
                        }
                    };

                    Canvas.SetLeft(rect, i * anchoCelda);
                    Canvas.SetTop(rect, j * altoCelda);
                    MapaCanvas.Children.Add(rect);
                }
            }
        }

        ///...<summary>
        ///...muestra información sobre los tipos de organismos, comida y terrenos.
        ///...</summary>
        private void BtnInformacion_Click(object sender, RoutedEventArgs e)
        {
            string mensaje = "Información sobre el simulador:\n\n" +
                             "Tipos de organismos:\n" +
                             "- amarillo: criaturas rápidas (velocidad >= 4).\n" +
                             "- rojo: criaturas grandes (tamaño >= 4).\n" +
                             "- morado: criaturas normales.\n\n" +
                             "Comida:\n" +
                             "- los recursos (puntos brillantes) se encuentran en bosques y tierras.\n" +
                             "- cada recurso otorga 5 de energía.\n\n" +
                             "Terrenos:\n" +
                             "- agua: no transitable.\n" +
                             "- bosque: transitable, puede contener recursos.\n" +
                             "- tierra: transitable, puede contener recursos.\n" +
                             "- montaña: no transitable.";

            MessageBox.Show(mensaje, "Información del simulador");
        }
    }

    ///...<summary>
    ///...enumeración de tipos de terreno.
    ///...</summary>
    public enum TipoTerreno
    {
        Agua,
        Tierra,
        Bosque,
        Montana
    }

    ///...<summary>
    ///...clase que representa una celda del mapa.
    ///...</summary>
    public class Celda
    {
        public int X { get; set; }
        public int Y { get; set; }
        public TipoTerreno TipoTerreno { get; set; }
        public int Recursos { get; set; }
        public List<Criatura> CriaturasEnCelda { get; set; }

        public Celda()
        {
            // constructor para serialización
            CriaturasEnCelda = new List<Criatura>();
        }

        public Celda(int x, int y, TipoTerreno tipoTerreno)
        {
            X = x;
            Y = y;
            TipoTerreno = tipoTerreno;
            Recursos = 0;
            CriaturasEnCelda = new List<Criatura>();
        }
    }

    ///...<summary>
    ///...clase que representa una criatura en el simulador.
    ///...</summary>
    public class Criatura
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Energia { get; set; }
        public int Velocidad { get; set; }
        public int Tamano { get; set; }
        public string Id { get; set; }
        public int Generacion { get; set; }

        // añadimos un constructor vacío necesario para la serialización/deserialización
        public Criatura()
        {
            Id = Guid.NewGuid().ToString();
            Generacion = 1;
        }

        public Criatura(int x, int y, int energia, int velocidad, int tamano)
        {
            X = x;
            Y = y;
            Energia = energia;
            Velocidad = velocidad;
            Tamano = tamano;
            Id = Guid.NewGuid().ToString();
            Generacion = 1;
        }

        ///...<summary>
        ///...genera una nueva criatura como descendencia con posibles mutaciones.
        ///...</summary>
        public Criatura Reproducirse(Random random, int probabilidadMutacion)
        {
            // heredar atributos con posible mutación
            int nuevaEnergia = Energia / 2;
            int nuevaVelocidad = Velocidad;
            int nuevoTamano = Tamano;

            if (random.Next(0, 100) < probabilidadMutacion)
            {
                nuevaVelocidad += random.Next(-1, 2);
                nuevoTamano += random.Next(-1, 2);

                // asegurar que los valores se mantienen dentro de límites
                nuevaVelocidad = Math.Clamp(nuevaVelocidad, MainWindow.VelocidadMin, MainWindow.VelocidadMax);
                nuevoTamano = Math.Clamp(nuevoTamano, MainWindow.TamanoMin, MainWindow.TamanoMax);
            }

            Criatura descendencia = new Criatura(X, Y, nuevaEnergia, nuevaVelocidad, nuevoTamano)
            {
                Generacion = this.Generacion + 1
            };

            return descendencia;
        }
    }

    ///...<summary>
    ///...clase para almacenar el estado de la simulación al guardar/cargar.
    ///...</summary>
    public class EstadoSimulacion
    {
        public List<Criatura> Criaturas { get; set; } = new List<Criatura>();
        public int ContadorGeneraciones { get; set; }
        public int RecursosTotales { get; set; }
        public int TamanoMapa { get; set; }
        public Celda[,] Mapa { get; set; } = new Celda[0, 0];
        public DateTime TiempoInicioSimulacion { get; set; }
    }
}
//.....................................................................................................................