using System;

namespace DriveExplorer.IoC
{
	/// <summary>
	/// Specifies which constructor should be used to instantiate when GetInstance is called. If there is only one constructor in the class, this attribute is not needed.
	/// </summary>
	[AttributeUsage(AttributeTargets.Constructor)]
	public sealed class PreferredConstructorAttribute : Attribute
	{
	}
}