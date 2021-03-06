﻿using DriveExplorer.Models;

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DriveExplorer.Views {

	public class TypeVisibilityConverter : IValueConverter {

		#region Public Fields

		public static readonly TypeVisibilityConverter Instance = new TypeVisibilityConverter();

		#endregion Public Fields

		#region Public Methods

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var type = (ItemTypes)value;
			return type.IsMember(ItemTypes.Folders) ?
				Visibility.Visible : Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion Public Methods
	}
}