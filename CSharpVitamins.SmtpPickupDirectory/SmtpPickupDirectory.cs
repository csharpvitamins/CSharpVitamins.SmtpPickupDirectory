using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Configuration;
using System.Net.Mail;
using System.Reflection;
using System.Web;
using System.Web.Configuration;

namespace CSharpVitamins
{
	/// <summary>
	/// Exposes helper methods for dealing with system.net/mailSettings
	/// </summary>
	public static class SmtpPickupDirectory
	{
		static bool? _isUsingPickupDirectory;

		/// <summary>
		/// Gets a value to indicate if the default SMTP Delivery method is SpecifiedPickupDirectory (using web.config, not tested with app.config)
		/// </summary>
		public static bool IsUsingPickupDirectory
		{
			get
			{
				if (!_isUsingPickupDirectory.HasValue)
				{
					Configuration config = WebConfigurationManager.OpenWebConfiguration("~/web.config");
					var mail = (MailSettingsSectionGroup)config.GetSectionGroup("system.net/mailSettings");
					_isUsingPickupDirectory = mail.Smtp.DeliveryMethod == SmtpDeliveryMethod.SpecifiedPickupDirectory;
				}
				return _isUsingPickupDirectory.Value;
			}
		}

		/// <summary>
		/// Sets the default PickupDirectoryLocation for the SmtpClient.
		/// </summary>
		/// <remarks>
		/// This method should be called to set the PickupDirectoryLocation 
		/// for the SmtpClient at runtime (Application_Start)
		/// 
		/// Reflection is used to set the private variable located in the 
		/// internal class for the SmtpClient's mail configuration: 
		/// System.Net.Mail.SmtpClient.MailConfiguration.Smtp.SpecifiedPickupDirectory.PickupDirectoryLocation
		/// 
		/// The folder must exist.
		/// 
		/// Alternate configuration method saves the web.config, triggering an app restart
		/// Configuration config = WebConfigurationManager.OpenWebConfiguration("~/web.config");
		/// var mail = (MailSettingsSectionGroup)config.GetSectionGroup("system.net/mailSettings");
		/// if (mail.Smtp.DeliveryMethod == SmtpDeliveryMethod.SpecifiedPickupDirectory)
		/// {
		///		string path = Path.Combine( HttpRuntime.AppDomainAppPath, @"..\..\mymail" );
		///		mail.Smtp.SpecifiedPickupDirectory.PickupDirectoryLocation = path;
		///		if (!Directory.Exists(path))
		///			Directory.CreateDirectory( path );
		///		config.Save();
		/// }
		/// </remarks>
		/// <param name="path"></param>
		public static void SetPickupDirectoryLocation(string path)
		{
			if (null == path)
				throw new ArgumentNullException("path");

			if (!Path.IsPathRooted(path))
				throw new ArgumentException("path must be absolute", "path");

			BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;
			PropertyInfo prop;
			object mailConfiguration, smtp, specifiedPickupDirectory;

			// get static internal property: MailConfiguration
			prop = typeof(SmtpClient).GetProperty("MailConfiguration", BindingFlags.Static | BindingFlags.NonPublic);
			mailConfiguration = prop.GetValue(null, null);

			// get internal property: Smtp
			prop = mailConfiguration.GetType().GetProperty("Smtp", instanceFlags);
			smtp = prop.GetValue(mailConfiguration, null);

			// get internal property: SpecifiedPickupDirectory
			prop = smtp.GetType().GetProperty("SpecifiedPickupDirectory", instanceFlags);
			specifiedPickupDirectory = prop.GetValue(smtp, null);

			// get private field: pickupDirectoryLocation, then set it to the supplied path
			FieldInfo field = specifiedPickupDirectory.GetType().GetField("pickupDirectoryLocation", instanceFlags);
			field.SetValue(specifiedPickupDirectory, path);
		}

		/// <summary>
		/// Sets the default PickupDirectoryLocation for the SmtpClient 
		/// to the relative path from the current web root.
		/// </summary>
		/// <param name="path">Relative path to the web root</param>
		public static void SetRelativePickupDirectoryLocation(string path)
		{
			if (null == path)
				throw new ArgumentNullException("path");

			SetPickupDirectoryLocation(Path.Combine(HttpRuntime.AppDomainAppPath, path));
		}

		/// <summary>
		/// Sets the default PickupDirectoryLocation for the SmtpClient to the first relative path that exists
		/// </summary>
		/// <param name="possibilities">An array of relative paths to test existance of</param>
		/// <returns>The full path of the folder, if it was found, otherwise null</returns>
		public static string SetRelativePickupDirectoryLocationIfPathExists(string[] possibilities)
		{
			string path = possibilities
				.Select(x => Path.Combine(HttpRuntime.AppDomainAppPath, x))
				.FirstOrDefault(x => Directory.Exists(x));

			if (null != path)
				SetPickupDirectoryLocation(path);

			return path;
		}
	}
}
