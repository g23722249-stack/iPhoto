' Application-wide objects and constants. VB6 declared these in each app's LibMain.bas
' (iPhoto\Module\LibMain.bas), but the shared Lib classes and forms use them too; a .NET library
' can't reach back into the exe, so they live here and every app (iPhoto, later iExport/iPrint)
' fills them in at startup. Names are kept as-is so ported code compiles unchanged.
Public Module Globals

    Public Const gc_intOpacityBackground As Integer = 180
    Public Const gc_bolOpenOpacity As Boolean = True

    Public g_lpFileSystem As Carbon.FileSystem
    Public g_lpConfig As Config
    Public g_lpStorage As Storage
    Public g_lpDatabase As Database
    ''' <summary>Face recognition; Nothing when it can't run (no database / face tables / models).</summary>
    Public g_lpFaces As FaceLibrary

    Public g_lpDock As Dock
    Public g_lpImport As Import

    Public g_lpSearch() As strSearch
    Public g_intSearch As Integer

    Public g_lpMonitors As CoreMedia.Monitors
    Public g_intMonitorCount As Integer

    Public Enum enumScreenRatio
        rtNormal = 0
        rtWidth = 1
    End Enum

    Public emScreenRatio() As enumScreenRatio
    Public g_intMainScreenIndex As Integer
    Public g_bolHaveWidthScreen As Boolean

End Module
