' Port of Lib\Module\LibToolTipText.bas + Lib\Class\cToolTip.cls: VB6 drew balloon tooltips itself
' (CreateWindowEx TOOLTIPS_CLASS). WinForms has System.Windows.Forms.ToolTip for that -- the converted
' forms already use one (vb6ToolTip) -- and nothing in iPhoto called this module, so it stays empty.
Public Module LibToolTipText

End Module
