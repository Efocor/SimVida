# Simulador de Vida Artificial

Este proyecto es un simulador básico de vida artificial desarrollado en C# utilizando WPF (Windows Presentation Foundation). El simulador permite crear un entorno dinámico donde criaturas virtuales interactúan con su entorno, consumen recursos, se reproducen y evolucionan a lo largo del tiempo. El objetivo es observar cómo estas criaturas se adaptan y evolucionan en un entorno simulado.

## Características principales

- **Generación de mapas dinámicos**: El simulador genera un mapa con diferentes tipos de terreno (agua, tierra, bosque, montaña) y recursos distribuidos aleatoriamente.
- **Criaturas interactivas**: Las criaturas tienen atributos como energía, velocidad y tamaño, que influyen en su comportamiento y supervivencia.
- **Sistema de reproducción y mutación**: Las criaturas pueden reproducirse, y existe una probabilidad de mutación que afecta sus atributos en la descendencia.
- **Simulación en tiempo real**: La simulación se ejecuta en tiempo real con un sistema de ticks que permite observar la evolución de las criaturas y el entorno.
- **Interfaz gráfica intuitiva**: La interfaz permite configurar parámetros como el tamaño del mapa, la cantidad de criaturas y la velocidad de la simulación.
- **Guardado y carga de simulaciones**: Es posible guardar el estado actual de la simulación y cargarlo posteriormente para continuar desde donde se dejó.

## Requisitos

- **.NET Framework**: El proyecto está desarrollado en C# utilizando .NET Framework. Asegúrate de tener instalado .NET Framework 4.7.2 o superior.
- **Visual Studio**: Se recomienda utilizar Visual Studio 2019 o superior para abrir y ejecutar el proyecto.

## Cómo ejecutar el simulador

1. **Clona el repositorio**:
   ```bash
   git clone https://github.com/tu-usuario/simulador-vida-artificial.git
   ```
2. **Abre el proyecto en Visual Studio**:
   - Abre Visual Studio y selecciona "Abrir un proyecto o solución".
   - Navega hasta la carpeta del proyecto y selecciona el archivo `SimuladorVidaArtificial.sln`.

3. **Compila y ejecuta el proyecto**:
   - Presiona `F5` o selecciona "Iniciar" desde el menú "Depurar" para compilar y ejecutar el simulador.

## Uso del simulador

### Configuración inicial

- **Tamaño del mapa**: Utiliza el slider "Tamaño del mapa" para ajustar el tamaño del entorno (entre 10x10 y 50x50 celdas).
- **Cantidad de criaturas**: Ajusta el número inicial de criaturas con el slider "Cantidad de criaturas".
- **Velocidad de simulación**: Controla la velocidad de la simulación con el slider "Velocidad de simulación".

### Controles

- **Iniciar simulación**: Haz clic en el botón "Iniciar simulación" para comenzar la simulación con los parámetros configurados.
- **Pausar/Reanudar**: Utiliza el botón "Pausar simulación" para pausar o reanudar la simulación.
- **Reiniciar**: Reinicia la simulación con los mismos parámetros utilizando el botón "Reiniciar simulación".
- **Guardar/Cargar**: Guarda el estado actual de la simulación con el botón "Guardar simulación" y carga una simulación guardada con el botón "Cargar simulación".
- **Información**: Puedes hacer clic en el botón "Información" para obtener detalles sobre los tipos de criaturas, recursos y terrenos.

### Dinámica de la simulación

- **Movimiento y energía**: Las criaturas se mueven aleatoriamente por el mapa, consumiendo energía con cada movimiento. Si su energía llega a cero, mueren.
- **Recursos**: Las criaturas pueden consumir recursos en las celdas para recuperar energía. Los recursos se generan en celdas de tipo bosque y tierra.
- **Reproducción**: Cuando una criatura tiene suficiente energía, puede reproducirse, generando una nueva criatura con atributos similares, pero con posibilidad de mutación.
- **Depredación**: Las criaturas más grandes pueden depredar a las más pequeñas, obteniendo parte de su energía.

---

¡Disfruta explorando el mundo de la vida artificial! Si tienes alguna pregunta o sugerencia, no dudes en abrir un issue en el repositorio.

**Made by FECORO**
