' Port of Lib\Module\LibDraw.bas: VB6 rotated pictures through draw.dll on a hidden frmPaint surface.
' Its only caller, LibAlbum.RotatePicture, now rotates with System.Drawing (Image.RotateFlip), and
' Quartz.ImageFilter.Rotate covers the viewer -- so nothing is left in this module.
Public Module LibDraw

End Module
