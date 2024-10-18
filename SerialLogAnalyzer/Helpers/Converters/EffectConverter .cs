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
			if (value == null || parameter == null)
				return null;

			// Check if the value is a boolean representing whether the mouse is over
			bool isMouseOver = (bool)value;

			// If the mouse is over, return the DropShadowEffect
			if (isMouseOver)
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
			if (value == null || parameter == null)
				return Binding.DoNothing;

			// Check if the value is a DropShadowEffect
			if (value is DropShadowEffect shadowEffect)
			{
				// Determine if the effect is active or not based on the shadow color
				return shadowEffect.Color == Colors.Gray; // Return true if hovered
			}

			return Binding.DoNothing; // If not a valid DropShadowEffect, do nothing
		}
	}
}
