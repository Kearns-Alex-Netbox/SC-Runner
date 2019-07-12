Imports Microsoft.VisualBasic.ApplicationServices

Namespace My
	' The following events are available for MyApplication:
	' Startup: Raised when the application starts, before the startup form is created.
	' Shutdown: Raised after all application forms are closed.  This event is not raised if the application terminates abnormally.
	' UnhandledException: Raised if the application encounters an unhandled exception.
	' StartupNextInstance: Raised when launching a single-instance application and the application is already active. 
	' NetworkAvailabilityChanged: Raised when the network connection is connected or disconnected.

	' https://docs.microsoft.com/en-us/dotnet/visual-basic/developing-apps/programming/log-info/
	' Make sure that the Runner.exe.config [app.config] file is uploaded when Changed
	Partial Friend Class MyApplication
		'Private Sub MyApplication_Shutdown(sender As Object, e As EventArgs) Handles Me.Shutdown
		'	Application.Log.WriteEntry("Application shut down at " & Computer.Clock.LocalTime.ToString)
		'End Sub

		'Private Sub MyApplication_Startup(sender As Object, e As StartupEventArgs) Handles Me.Startup
		'	Application.Log.WriteEntry("Application started at " & Computer.Clock.LocalTime.ToString)
		'End Sub

		Private Sub MyApplication_UnhandledException(sender As Object, e As UnhandledExceptionEventArgs) Handles Me.UnhandledException
			Application.Log.WriteException(e.Exception,
										   TraceEventType.Critical,
										   "Application Unhandled Exception at " & Computer.Clock.LocalTime.ToString & vbNewLine &
										   vbNewLine &
										   "Message:" & vbNewLine &
										   e.Exception.Message & vbNewLine &
										   vbNewLine &
										   "Source:" & vbNewLine &
										   e.Exception.Source &
										   vbNewLine &
										   "Stack Trace:" & vbNewLine &
										   e.Exception.StackTrace & vbNewLine &
										   vbNewLine &
										   "More Help: " & vbNewLine &
										   e.Exception.HelpLink & vbNewLine)
		End Sub
	End Class
End Namespace
