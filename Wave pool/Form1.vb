Imports System.IO
Imports System.IO.Ports
Imports System.Threading
Imports System.Threading.Thread

Public Class Form1
    Private Tunda, deelay, terkirim As Integer
    Private WithEvents COMport1 As New SerialPort
    Private WithEvents COMport2 As New SerialPort
    Private WithEvents COMport3 As New SerialPort
    Private WithEvents COMport4 As New SerialPort
    Private WithEvents COMport5 As New SerialPort
    Private WithEvents COMport6 As New SerialPort
    Private WithEvents COMport7 As New SerialPort
    Private WithEvents COMport8 As New SerialPort
    Delegate Sub SetTextCallback(ByVal [text] As String)
    Dim thedatatable As New DataTable
    Dim myList As New List(Of String)()
    Dim myListPort As New List(Of String)()
    Dim myPort As Array
    Dim Ports As New List(Of IO.Ports.SerialPort)
    Dim arrayPort As Integer = 0
    Dim countNumber As Integer = 0
    Dim validationCount, comLoop As Integer

    Private filePath As String = "E:\ASIA TRANS\Wave pool\log.txt"

    Private Sub BtnBrowse_Click(sender As Object, e As EventArgs) Handles btnBrowse.Click
        If (OpenFileDialog1.ShowDialog() = DialogResult.OK) Then
            txtFile.Text = OpenFileDialog1.FileName
        End If
    End Sub

    Private Sub BtnUpload_Click(sender As Object, e As EventArgs) Handles btnUpload.Click
        Dim thereader As New IO.StreamReader(txtFile.Text.ToString, System.Text.Encoding.Default)
        Dim sline As String
        Do
            sline = thereader.ReadLine
            If sline Is Nothing Then Exit Do
            Dim thecolumns() As String = sline.Split(",")
            Dim newrow As DataRow = thedatatable.NewRow
            newrow("nomor") = thecolumns(0)
            thedatatable.Rows.Add(newrow)
        Loop
        thereader.Close()

        DataGridView1.DataSource = thedatatable
        lblJumlahRow.Text = terkirim & " / " & thedatatable.Rows.Count & "rows"
    End Sub

    Private Sub BtnSend_Click(sender As Object, e As EventArgs) Handles btnSend.Click

        deelay = (ComboBox1.Text * 1000) / myList.Count

        validationCount = DataGridView1.Rows.Count

        For x = 0 To myList.Count - 1
            Ports.Add(New SerialPort)
            Ports(x).PortName = myList(x)
            Ports(x).BaudRate = 115200
            Ports(x).WriteTimeout = 2000
            If Not Ports(x).IsOpen Then
                Ports(x).Open()
                Ports(x).Write("AT+CMGF=1" & Chr(13))
            Else
                Ports(x).Close()
                Ports(x).Open()
            End If
        Next

        Button1.Enabled = False

        With DataGridView1
            If validationCount = 0 Then
                MsgBox("Data Belum di Upload")
            Else
                If myList.Count > 0 Then
                    Do


                        If arrayPort > (myList.Count - 1) Then
                            arrayPort = 0
                            sendSms()
                        Else
                            sendSms()
                        End If

                    Loop Until countNumber > (DataGridView1.Rows.Count - 1)
                    MsgBox("Pesan Telah Selesai Terkirim. ")
                    Button1.Enabled = True
                End If
            End If
        End With
    End Sub

    Public Sub sendSms()
        Dim textAppend As String
        Dim str As String = DataGridView1.Rows(countNumber).Cells("nomor").Value.ToString()
        Dim result As String = str(0)
        Try
            If result = "8" Then
                txtNumber.Clear()
                txtNumber.Text = "0" & str
            ElseIf result = "6" Then
                txtNumber.Clear()
                txtNumber.Text = "+" & str
            Else
                txtNumber.Clear()
                txtNumber.Text = str
            End If

            Sleep(300)
            Tunda = 300
            Sleep(Tunda)


            Application.DoEvents()

            Ports(arrayPort).Write("AT+CMGS=" & Chr(34) & txtNumber.Text & Chr(34) & Chr(13))
            Sleep(Tunda)
            Ports(arrayPort).Write(txtMessage.Text & Chr(26))
            terkirim += 1
            countNumber += 1

            lblJumlahRow.Text = terkirim & " / " & thedatatable.Rows.Count & "Numbers"

            textAppend = txtNumber.Text + " " + "Terkirim " + DateTime.Now + vbCrLf


            Try
                File.AppendAllText(filePath, textAppend)
            Catch ex As Exception
                MsgBox(ex)
            End Try

            arrayPort += 1
            Sleep(deelay)
        Catch ex As Exception

        End Try
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        myPort = IO.Ports.SerialPort.GetPortNames()
        cmbPort.Items.AddRange(myPort)

        With thedatatable
            .Columns.Add("nomor", System.Type.GetType("System.String"))
        End With

        ComboBox1.Items.Add("10")
        ComboBox1.Items.Add("15")
        ComboBox1.Items.Add("20")
        ComboBox1.Items.Add("25")


    End Sub

    Private Sub SerialPort1_DataReceived(sender As System.Object, e As System.IO.Ports.SerialDataReceivedEventArgs) Handles SerialPort1.DataReceived
        ReceivedText(Ports(ListBox1.SelectedIndex).ReadExisting())
    End Sub

    Private Sub BtnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        System.Threading.Thread.Sleep(1000)
        If Ports(ListBox1.SelectedIndex).IsOpen Then
            Ports(ListBox1.SelectedIndex).Close()
            Ports.RemoveAt(ListBox1.SelectedIndex)
        Else
            MsgBox("port 0")
        End If
    End Sub
    Private Sub BtnOpen_Click(sender As Object, e As EventArgs) Handles btnOpen.Click
        myList.Add(cmbPort.Text)
        cmbPort.Items.Remove(cmbPort.Text)
        ListBox1.Items.Clear()
        Dim a As Integer = myList.Count
        ListBox1.Items.Clear()
        Dim ulang = 0
        Do
            ListBox1.Items.Add(myList(ulang).ToString)
            ulang += 1
        Loop Until ulang = a
        TextBox2.Text = myList.Count
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        txtNumber.Text = myList(1)
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs)
        Dim countNumber As Integer = 0
        With DataGridView1
            Do

                countNumber += 1
            Loop Until countNumber > (DataGridView1.Rows.Count - 1)
        End With
    End Sub

    Private Sub Button5_Click(sender As Object, e As EventArgs)
        COMport1.Close()
    End Sub

    Private Sub ReceivedText(ByVal [text] As String) 'input from ReadExisting
        If Me.TextBox1.InvokeRequired Then
            Dim x As New SetTextCallback(AddressOf ReceivedText)
            Me.Invoke(x, New Object() {(text)})
        Else
            Me.txtMessage.Text &= [text] 'append text
        End If
    End Sub
End Class
