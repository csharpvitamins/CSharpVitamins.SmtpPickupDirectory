# CSharpVitamins.SmtpPickupDirectory

This library simply helps set your `System.Net.Mail.SmtpClient` drop folder to a web relative location - which you can't do out of the box. This helps in multi machine/developer setups. 

 1. See [Using an smtp pickup directory for ASP.NET development](http://singular.co.nz/2007/11/using-an-smtp-pickup-directory-delivery-method-for-asp-net-development/) for reasons why using a **drop folder** in development is a good idea.
 2. And the follow up [Programmatically setting the SmtpClient pickup directory location at runtime](http://singular.co.nz/2007/12/programmatically-setting-the-smtpclient-pickup-directory-location-at-runtime/) for more reasoning on why this library came about.

Available on [NuGet](https://www.nuget.org/packages/csharpvitamins.smtppickupdirectory/). To install, run the following command in the Package Manager Console:

	PM> Install-Package CSharpVitamins.SmtpPickupDirectory


## Usage

Usage is fairly simple - you must set the `<smtp><deliveryMethod>` to "SpecifiedPickupDirectory". 

	<system.net>
	  <mailSettings>
	    <smtp deliveryMethod="SpecifiedPickupDirectory" from="no-reply@mydomain.com">
	      <specifiedPickupDirectory pickupDirectoryLocation="c:\MyWebApp\mail" />
	    </smtp>
	  </mailSettings>
	</system.net>

Then in your application start up code, pass it a relative location. 
 
	using CSharpVitamins;

	void Application_Start()
	{
		// optionally keep this out of release code with "#if DEBUG"
 
		#if DEBUG

		// set drop folder to the "mail" folder, one level up from the web root
		if (SmtpPickupDirectory.IsUsingPickupDirectory)
			SmtpPickupDirectory.SetRelativePickupDirectoryLocation(@"..\mail");

		#endif
	}

You can also use `SetRelativePickupDirectoryLocationIfPathExists` to pass several folder choices to cater for different folder arrangements may prefer. 

	SmtpPickupDirectory.SetRelativePickupDirectoryLocationIfPathExists(@"..\mail", @"..\..\top-mail");


Happy coding.