' Printing helpers shared by frmPrint and frmPhotoIndex (in VB6 each form had its own copy): the
' printer list, the paper size in mm (VB6 Printer.ScaleWidth \ 56.7 = the printable area), the paper
' shadow of the preview, the page list from frmPrintPages, one print job for several pages, and
' pages saved as JPEG files.
Imports System.Drawing.Printing

Friend Module LibPrint

    Friend Const NoPrinterKey As String = "Null"

    ''' <summary>The installed printers (the default one selected); "未安裝任何印表機" when there are none.</summary>
    Friend Sub FillPrinters(ByVal cbo As Aqua.DropDownList)
        cbo.Items.Clear()
        Dim defaultName As String = ""
        Try
            defaultName = New PrinterSettings().PrinterName
        Catch ex As Exception When TypeOf ex Is InvalidPrinterException OrElse TypeOf ex Is ComponentModel.Win32Exception
        End Try
        Dim selected As Integer = 0
        For Each name As String In PrinterSettings.InstalledPrinters
            If name = defaultName Then selected = cbo.Items.Count
            cbo.Items.Add(New Aqua.MenuItem(name, name, 1, Nothing, Nothing))
        Next
        If cbo.Items.Count = 0 Then cbo.Items.Add(New Aqua.MenuItem(NoPrinterKey, "未安裝任何印表機", 1, Nothing, Nothing))
        cbo.SelectedIndex = selected
    End Sub

    ''' <summary>Settings for the printer chosen in the list; Nothing for "no printer".</summary>
    Friend Function SettingsFor(ByVal cbo As Aqua.DropDownList) As PrinterSettings
        Dim name As String = If(cbo.SelectedItem?.Name, "")
        If name = "" OrElse name = NoPrinterKey Then Return Nothing
        Return New PrinterSettings With {.PrinterName = name}
    End Function

    ''' <summary>The printable area of the printer's page in whole mm; False when the printer can't be read.</summary>
    Friend Function PrintableMm(ByVal settings As PrinterSettings, ByRef width As Integer, ByRef height As Integer) As Boolean
        width = 0 : height = 0
        Try
            If settings Is Nothing OrElse Not settings.IsValid Then Return False
            Dim area As RectangleF = settings.DefaultPageSettings.PrintableArea
            Dim w As Single = area.Width, h As Single = area.Height
            If settings.DefaultPageSettings.Landscape AndAlso w < h Then
                Dim t As Single = w : w = h : h = t
            End If
            width = CInt(Math.Floor(w * 0.254))
            height = CInt(Math.Floor(h * 0.254))
        Catch ex As Exception When TypeOf ex Is InvalidPrinterException OrElse TypeOf ex Is ComponentModel.Win32Exception
            Return False
        End Try
        Return width > 0 AndAlso height > 0
    End Function

    ''' <summary>The grey shadow drawn behind the preview paper.</summary>
    Friend Function ShadowImage(ByVal w As Integer, ByVal h As Integer, ByVal back As Color, ByVal outer As Color, ByVal inner As Color) As Bitmap
        Dim shadow As New Bitmap(Math.Max(1, w), Math.Max(1, h))
        Using g As Graphics = Graphics.FromImage(shadow)
            g.Clear(back)
            Using p As New Pen(outer, 2)
                g.DrawRectangle(p, 4, 4, w - 8, h - 8)
            End Using
            Using p As New Pen(inner, 1)
                g.DrawRectangle(p, 2, 2, w - 4, h - 4)
            End Using
        End Using
        Return shadow
    End Function

    ''' <summary>The pages (0-based) chosen in frmPrintPages ("ALL", the current page or "1,3,5");
    ''' Nothing when cancelled or no valid page was given.</summary>
    Friend Function ChoosePages(ByVal currentPage As Integer, ByVal totalPage As Integer) As List(Of Integer)
        Dim strValue As String
        If totalPage <= 1 Then
            strValue = "1"
        Else
            Using f As New frmPrintPages
                strValue = f.ShowPages(currentPage)
            End Using
        End If
        strValue = If(strValue, "").Trim().ToUpperInvariant()
        If strValue = "" Then Return Nothing          ' 放棄
        If strValue = "ALL" Then Return Enumerable.Range(0, totalPage).ToList()
        Dim pages As New List(Of Integer)
        For Each s As String In strValue.Split(","c)
            Dim n As Integer = CInt(Val(s))
            If n > 0 AndAlso n <= totalPage AndAlso Not pages.Contains(n - 1) Then pages.Add(n - 1)
        Next
        If pages.Count = 0 Then
            frmMsgBox.ShowCriticalMessage("不正確的列印頁次", "")
            Return Nothing
        End If
        Return pages
    End Function

    ''' <summary>Prints the pages as one job; drawPage gets the printer Graphics (origin = the printable
    ''' area, as VB6's Printer) and the page number. Returns "" or the error text.</summary>
    Friend Function PrintPages(ByVal settings As PrinterSettings, ByVal copies As Integer, ByVal pages As List(Of Integer),
                               ByVal drawPage As Action(Of Graphics, Integer)) As String
        Try
            Using doc As New PrintDocument
                doc.PrinterSettings = CType(settings.Clone(), PrinterSettings)
                doc.PrinterSettings.Copies = CShort(Math.Max(1, Math.Min(Short.MaxValue, copies)))
                doc.DocumentName = "iPhoto"
                doc.PrintController = New StandardPrintController()
                Dim k As Integer = 0
                AddHandler doc.PrintPage, Sub(s, e)
                                              drawPage(e.Graphics, pages(k))
                                              k += 1
                                              e.HasMorePages = k < pages.Count
                                          End Sub
                doc.Print()
            End Using
        Catch ex As Exception When TypeOf ex Is InvalidPrinterException OrElse TypeOf ex Is ComponentModel.Win32Exception
            Return ex.Message
        End Try
        Return ""
    End Function

    ''' <summary>Saves each page as "&lt;folder&gt;\&lt;base&gt;-NN.Jpg" (just "&lt;base&gt;.Jpg" for a single page), at
    ''' <paramref name="dpi"/> on white.</summary>
    Friend Sub SavePagesJpeg(ByVal folder As String, ByVal baseName As String, ByVal widthMm As Integer, ByVal heightMm As Integer,
                             ByVal pageCount As Integer, ByVal dpi As Single, ByVal drawPage As Action(Of Graphics, Integer))
        Dim pxPerMm As Double = dpi / 25.4
        Dim digits As New String("0"c, pageCount.ToString().Length)
        For page As Integer = 0 To pageCount - 1
            Using bmp As New Bitmap(Math.Max(1, CInt(widthMm * pxPerMm)), Math.Max(1, CInt(heightMm * pxPerMm)))
                bmp.SetResolution(dpi, dpi)
                Using g As Graphics = Graphics.FromImage(bmp)
                    g.Clear(Color.White)
                    drawPage(g, page)
                End Using
                Dim file As String = If(pageCount = 1, folder & "\" & baseName & ".Jpg",
                                        folder & "\" & baseName & "-" & (page + 1).ToString(digits) & ".Jpg")
                Quartz.SavePicture(bmp, file, 100)
            End Using
        Next
    End Sub

End Module
