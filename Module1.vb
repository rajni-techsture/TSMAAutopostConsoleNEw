Imports Microsoft.VisualBasic
Imports DataAccessLayer.DataAccessLayer
Imports BusinessAccessLayer.BusinessLayer
Imports System.Data.SqlClient
Imports System.IO
Imports Facebook

Module Module1
    Dim objBAL As New BALSchedulePost
    Dim objLog As New Log

    Sub Main()
        Dim intTotal As Integer = 0
        Dim intSuccessFull As Integer = 0
        Dim intFailed As Integer = 0
        Dim strFailedFanPageId As String = ""
        'AutoPost()
        objLog.WritePlainLog(FormatDateTime(Now.Date, DateFormat.ShortDate))
        objLog.WritePlainLog("==========================================")
        objLog.WriteLog("START")
        AutoPost(intTotal, intSuccessFull, intFailed, strFailedFanPageId)
        objLog.WriteLog("END  ")
        objLog.WritePlainLog("------------------------------------------")
        objLog.WritePlainLog("Total Records : " & intTotal)
        objLog.WritePlainLog("Successfull   : " & intSuccessFull)
        objLog.WritePlainLog("Failed        : " & intFailed)
        objLog.WritePlainLog("--------------Failed Autopost Fan Page ID Start -----------")
        objLog.WritePlainLog("Failed Autopost Fan Page Id        : " & strFailedFanPageId)
        objLog.WritePlainLog("--------------Failed Autopost Fan Page ID End -----------")
        objLog.WritePlainLog("------------------------------------------")
        objLog.WritePlainLog("")
    End Sub

#Region "Auto Post"
    Sub AutoPost(ByRef intTotal As Integer, ByRef intSuccessFull As Integer, ByRef intFailed As Integer, ByRef strFailedFanPageId As String)
        Try
            Dim dataAccess As New DALDataAccess()
            Dim ds As New DataSet
            dataAccess.AddCommand(CommandType.StoredProcedure, "prc_GetRecordsForAutoPost")
            ds = dataAccess.GetDataset()
            intTotal = ds.Tables(0).Rows.Count
            For Each dtRow As DataRow In ds.Tables(0).Rows
                If dtRow("AccessToken") <> "" Then
                    Try
                        Dim path As String = ""
                        Dim fbApp = New FacebookClient(dtRow("AccessToken").ToString)
                        Dim postData = New Dictionary(Of String, Object)()

                        If dtRow("Image") <> "" Then
                            Dim imagePath As String = Convert.ToString(My.Application.Info.DirectoryPath & "\images\" & dtRow("Image").ToString).Replace("\bin\Debug", "")
                            'Dim imagePath As String = Convert.ToString(My.Application.Info.DirectoryPath & "\images\" & dtRow("Image").ToString).Replace("\bin\Debug", "")
                            'Dim imagePath As String = "C:\inetpub\wwwroot\release\forrent_20120216220109\Content\uploads\images\" & dtRow("Image").ToString ''forrent.com production server
                            Dim mediaObject As New FacebookMediaObject() With { _
                                .FileName = imagePath, _
                                .ContentType = "image/jpg" _
                            }
                            Dim fileBytes As Byte() = File.ReadAllBytes(mediaObject.FileName)
                            mediaObject.SetValue(fileBytes)

                            postData.Clear()
                            postData.Add("message", dtRow("Message").ToString.Replace("<br>", Chr(10)))
                            postData.Add("image", mediaObject)
                            path = dtRow("FBPageId").ToString & "/photos"
                            fbApp.Post(path, postData)
                        Else
                            postData.Add("message", dtRow("Message").ToString.Replace("<br>", Chr(10)))
                            If dtRow("Link") <> "" Then
                                postData.Add("link", dtRow("Link").ToString)
                            End If
                            path = dtRow("FBPageId").ToString & "/feed"
                            fbApp.Post(path, postData)

                        End If
                        objBAL.AutoPostId = dtRow("AutoPostId")
                        objBAL.UpdateAutoPostMasterNew()

                        intSuccessFull = intSuccessFull + 1
                    Catch ex As FacebookOAuthException
                        strFailedFanPageId = dtRow("FBPageId")
                        strFailedFanPageId = strFailedFanPageId & ","
                        InsertErrorLog(dtRow("AutoPostId"), dtRow("TSMAUserId"), dtRow("FBUserID"), dtRow("FBPageId"), dtRow("AccessToken"), ex.Message.ToString())
                        intFailed = intFailed + 1
                        
                    End Try
                Else
                    InsertErrorLog(dtRow("AutoPostId"), dtRow("TSMAUserId"), dtRow("FBUserID"), dtRow("FBPageId"), dtRow("AccessToken"), "Access token does not exists!")
                    intFailed = intFailed + 1
                    strFailedFanPageId = dtRow("FBPageId")
                    strFailedFanPageId = "," & strFailedFanPageId
                End If
            Next
        Catch ex As Exception
            InsertErrorLog("", "", "", "", "", ex.Message.ToString & "  : At start of getting data ")
        End Try

    End Sub
#End Region

#Region "Error Log"
    Sub InsertErrorLog(AutoPostId As Integer, TSMAUserId As Integer, FBUserID As String, FBPageId As String, AccessToken As String, ErrorDetails As String)
        Try
            Dim dataAccess As New DALDataAccess()
            dataAccess.AddCommand(CommandType.StoredProcedure, "prc_AddAutoPostErrorDetails")
            dataAccess.AddParam("@er_AutoPostId", SqlDbType.Int, AutoPostId)
            dataAccess.AddParam("@er_TSMAUserId", SqlDbType.Int, TSMAUserId)
            dataAccess.AddParam("@er_FBUserId", SqlDbType.VarChar, FBUserID)
            dataAccess.AddParam("@er_FBPageId", SqlDbType.VarChar, FBPageId)
            dataAccess.AddParam("@er_AcessToken", SqlDbType.VarChar, AccessToken)
            dataAccess.AddParam("@er_Error", SqlDbType.VarChar, ErrorDetails)
            dataAccess.ExecuteNonQuery()
        Catch ex As Exception
            InsertErrorLog("", "", "", "", "", ex.Message.ToString & "  : At the time of inserting records in tbl_autoposterrorlog table ")
        End Try
    End Sub
#End Region
End Module
