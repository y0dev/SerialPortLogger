using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace SerialLogAnalyzer.Helpers
{
	public class ColorScheme
	{
		public ConsoleColor BackgroundColor { get; }
		public ConsoleColor TextColor { get; }
		public ConsoleColor NumberColor { get; }

		public ColorScheme(ConsoleColor backgroundColor, ConsoleColor textColor, ConsoleColor numberColor)
		{
			BackgroundColor = backgroundColor;
			TextColor = textColor;
			NumberColor = numberColor;
		}

		public static ColorScheme Default => new ColorScheme(ConsoleColor.Black, ConsoleColor.White, ConsoleColor.Yellow);
		public static ColorScheme DarkMode => new ColorScheme(ConsoleColor.Black, ConsoleColor.Gray, ConsoleColor.Cyan);
		public static ColorScheme LightMode => new ColorScheme(ConsoleColor.White, ConsoleColor.Black, ConsoleColor.Blue);
		public static ColorScheme SolarizedDark => new ColorScheme(ConsoleColor.DarkBlue, ConsoleColor.Gray, ConsoleColor.Yellow);
		public static ColorScheme SolarizedLight => new ColorScheme(ConsoleColor.White, ConsoleColor.DarkBlue, ConsoleColor.DarkGreen);
		public static ColorScheme Monokai => new ColorScheme(ConsoleColor.Black, ConsoleColor.Gray, ConsoleColor.Magenta);
		public static ColorScheme GruvboxDark => new ColorScheme(ConsoleColor.DarkGray, ConsoleColor.White, ConsoleColor.Yellow);
		public static ColorScheme GruvboxLight => new ColorScheme(ConsoleColor.White, ConsoleColor.DarkGray, ConsoleColor.Yellow);
		public static ColorScheme Nord => new ColorScheme(ConsoleColor.DarkBlue, ConsoleColor.Gray, ConsoleColor.Cyan);
		public static ColorScheme Ocean => new ColorScheme(ConsoleColor.DarkCyan, ConsoleColor.White, ConsoleColor.Cyan);
		public static ColorScheme Desert => new ColorScheme(ConsoleColor.DarkYellow, ConsoleColor.Black, ConsoleColor.DarkRed);
		public static ColorScheme Retro => new ColorScheme(ConsoleColor.Black, ConsoleColor.Green, ConsoleColor.Yellow);
		public static ColorScheme Cyberpunk => new ColorScheme(ConsoleColor.Black, ConsoleColor.Magenta, ConsoleColor.Cyan);
		public static ColorScheme Twilight => new ColorScheme(ConsoleColor.DarkMagenta, ConsoleColor.White, ConsoleColor.DarkYellow);
		public static ColorScheme Forest => new ColorScheme(ConsoleColor.DarkGreen, ConsoleColor.White, ConsoleColor.Yellow);
		public static ColorScheme Sunset => new ColorScheme(ConsoleColor.DarkYellow, ConsoleColor.DarkRed, ConsoleColor.Yellow);

		public static Brush ConvertToBrush(ConsoleColor consoleColor)
		{
			switch (consoleColor)
			{
				case ConsoleColor.Black:
					return Brushes.Black;
				case ConsoleColor.DarkBlue:
					return Brushes.DarkBlue;
				case ConsoleColor.DarkGreen:
					return Brushes.DarkGreen;
				case ConsoleColor.DarkCyan:
					return Brushes.DarkCyan;
				case ConsoleColor.DarkRed:
					return Brushes.DarkRed;
				case ConsoleColor.DarkMagenta:
					return Brushes.DarkMagenta;
				case ConsoleColor.DarkYellow:
					return Brushes.DarkGoldenrod;
				case ConsoleColor.Gray:
					return Brushes.Gray;
				case ConsoleColor.DarkGray:
					return Brushes.DarkGray;
				case ConsoleColor.Blue:
					return Brushes.Blue;
				case ConsoleColor.Green:
					return Brushes.Green;
				case ConsoleColor.Cyan:
					return Brushes.Cyan;
				case ConsoleColor.Red:
					return Brushes.Red;
				case ConsoleColor.Magenta:
					return Brushes.Magenta;
				case ConsoleColor.Yellow:
					return Brushes.Yellow;
				case ConsoleColor.White:
					return Brushes.White;
				default:
					return Brushes.Transparent; // Fallback
			}
		}
	}

}
