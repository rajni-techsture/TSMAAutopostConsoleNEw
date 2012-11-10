Imports System.IO
Public Class Log
    'Private ApplicationPath As String = My.Application.Info.DirectoryPath & "\uploads\"
    Private ApplicationPath As String = Convert.ToString(My.Application.Info.DirectoryPath & "\uploads\").Replace("\bin\Debug", "")
    Private LogFileName As String = "autopost_log.txt"
    Sub New()
        Dim strFileName As String = Me.ApplicationPath & LogFileName
        If Not File.Exists(strFileName) Then
            Dim ofile As System.IO.FileStream
            ofile = File.Create(strFileName)
            ofile.Close()
        End If
    End Sub
    Sub WriteLog(ByVal strMessage As String)
        Dim oWrite As System.IO.StreamWriter
        Dim strFileName As String = Me.ApplicationPath & LogFileName
        oWrite = New StreamWriter(strFileName, True)
        oWrite.WriteLine(strMessage & " : {0:dd-MMMM-yyyy - hh:mm:sstt}", Now())
        oWrite.Close()
    End Sub

    Sub WritePlainLog(ByVal strMessage As String)
        Dim oWrite As System.IO.StreamWriter
        Dim strFileName As String = Me.ApplicationPath & LogFileName
        oWrite = New StreamWriter(strFileName, True)
        oWrite.WriteLine(strMessage)
        oWrite.Close()
    End Sub
End Class
