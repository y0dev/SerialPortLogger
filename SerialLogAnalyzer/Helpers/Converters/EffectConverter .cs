using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media.Effects;
using System.Windows.Data;
using System.Windows.Media;

namespace SerialLogAnalyzer.Helpers.Converters
{
	public class BooleanToDropShadowEffectConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is bool isMouseOver && isMouseOver)
			{
				return new DropShadowEffect
				{
					Color = Colors.Gray, // Adjust shadow color
					BlurRadius = 10, // Adjust shadow blur
					ShadowDepth = 5 // Adjust shadow depth
				};
			}
			return null; // No effect when not hovering
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
