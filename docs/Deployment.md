# Deployment

Want to deploy Golden Ticket to a server? Thought so. Here's your guide.

There are a few things you'll want to setup or have the addresses and credentials for:


- Relational Database
- Email Server
- Application Server

## Relational Database

All the data and the security/identity information are stored in a relational database. Development was done against Microsoft SQL Server, but the application will work with any database that has a data provider for .NET (for example, PostgreSQL and Oracle DB have these).

To connect to your database, edit the following line in the `GoldenTicket/Web.config` file with the connection string for your database:

	<configuration>
		...
		<connectionStrings>
	    	<add name="GoldenTicketDbContext" connectionString="Data Source=(LocalDB)\v11.0;AttachDbFilename=|DataDirectory|\GoldenTicket.mdf;Integrated Security=True" providerName="System.Data.SqlClient"/>
		</connectionStrings>
		...
	</configuration>

The application is Code First, which means that upon starting the application up for the first time, it'll generate the database schema and install some default data from the `GoldenTicket/DAL/Seeds.cs` file. 

If your bureau's policy prevents applications from being able to execute DDL statements, then you may want to try the following to setup the database:

1. Create the database
2. Run the application locally via Visual Studio (see [Developer Setup](Developer Setup.md))
3. Open the file `App_Data/GoldenTicket.mdf` or open `GoldenTicketDbContext` via the Visual Studio Server Explorer. (These are for the LocalDB instance)
4. Copy the DDL statements for each table in LocalDB and execute in your database (assuming it's SQL Server and needs no modification). 
5. An initial set of data will have been populated in several of the tables in the LocalDB. Copy the data to your database.
6. When you package the application for deployment, remove  the `Seeds.cs` file for safety (it probably wouldn't run anyway, upon seeing the database being created).

## Email Server

By default, Golden Ticket requires access to an SMTP email server. Assuming this is what you'll use, it's simply a matter of editing the `GoldenTicket/Web.config` settings noted below:

	<configuration>
		...
		<appSettings>
			...
			<add key="SmtpAddress" value="localhost"/> <!-- Email server address-->
	    	<add key="SmtpPort" value="25"/> <!-- Port that the email server is running on -->
	    	<add key="SmtpUsername" value=""/> <!-- Username for the email server -->
	    	<add key="SmtpPassword" value=""/> <!-- Password for the email server-->
	    	<add key="MailFrom" value="no-reply@ride.ri.gov"/> <!-- This is the FROM address that emails will have -->
	  	</appSettings>
		...
	</configuration>

If you decide to us an alternate email service (such as SendGrid, etc.) that has its own API, you'll need to edit one Class, `GoldenTicket.Misc.EmailHelper` (found in `GoldenTicket/Misc/EmailHelper`). If you replace the code in the following method, everything should work just fine.

        public static void SendEmail(string toAddress, string fromAddress, string subject, string messageBody)
        {
            // Get the configuration settings
            var mailServerAddress = ConfigurationManager.AppSettings["SmtpAddress"];
            var mailServerPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"]);
            var mailServerUsername = ConfigurationManager.AppSettings["SmtpUsername"];
            var mailServerPassword = ConfigurationManager.AppSettings["SmtpPassword"];
            
            // Setup the SMTP connection 
            var mailClient = new SmtpClient(mailServerAddress, mailServerPort);
            mailClient.Credentials = new NetworkCredential(mailServerUsername, mailServerPassword);

            // Send the message
            var mailMessage = new MailMessage(to: toAddress, @from: fromAddress, subject: subject, body: messageBody);
            mailMessage.IsBodyHtml = true;
            mailClient.Send(mailMessage);
        } 

## Application Server

You'll need to have an IIS server or something capable of running a .NET 4.5 application. 

Golden Ticket is a pretty conventional application, using most of the defaults provided for an MVC ASP.NET application. 

The following guides should be sufficient for deploying it:

- [How to setup your first IIS web site](http://support.microsoft.com/kb/323972)
- [Web deployment overview for Visual Studio and ASP.NET](http://msdn.microsoft.com/en-us/library/dd394698.aspx)

After the application is deployed and running, you'll be able to get to the parent side of the application at the base URL and to the admin side at `(base URL)/Admin`.

## System Specs

In case someone asks what the stack required by the application is, here you go:

- C# 5
- .NET Framework 5
- ASP.NET MVC 5
- ASP.NET Identity 2

Specific versions of other dependencies can be found in the NuGet package file at `GoldenTicket/packages.config`.