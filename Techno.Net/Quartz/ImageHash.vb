Imports OpenCvSharp

Namespace Quartz

    ''' <summary>A 64-bit difference hash of a picture (new in the .NET port; 尋找重複照片): the picture
    ''' decoded at 1/8 size in grey (libjpeg's scaled decoding -- fast, and the same for a photo and a
    ''' copy of it saved at another size), shrunk to 9 x 8; a bit per pixel brighter than its right
    ''' neighbour. The file is read as bytes (cv's own file reading can't take Chinese paths).</summary>
    Public NotInheritable Class ImageHash

        Private Sub New()
        End Sub

        ''' <summary>Below this spread of grey (standard deviation) a picture is too dark / flat to compare:
        ''' night shots and blank frames all hash nearly alike.</summary>
        Public Const FlatDeviation As Double = 14

        ''' <summary>Nothing when the picture can't be decoded; 0 when it is too flat to compare (FlatDeviation).</summary>
        Public Shared Function DHash(ByVal file As String) As ULong?
            Try
                Dim bytes() As Byte = IO.File.ReadAllBytes(file)
                Using gray As Mat = Cv2.ImDecode(bytes, ImreadModes.ReducedGrayscale8)
                    If gray Is Nothing OrElse gray.Empty() Then Return Nothing
                    Dim mean As Scalar, sd As Scalar
                    Cv2.MeanStdDev(gray, mean, sd)
                    If sd.Val0 < FlatDeviation Then Return 0UL
                    Using small As New Mat()
                        Cv2.Resize(gray, small, New Size(9, 8), 0, 0, InterpolationFlags.Area)
                        Dim h As ULong = 0UL
                        For y = 0 To 7
                            For x = 0 To 7
                                h = (h << 1) Or If(small.At(Of Byte)(y, x) > small.At(Of Byte)(y, x + 1), 1UL, 0UL)
                            Next
                        Next
                        Return h
                    End Using
                End Using
            Catch ex As Exception When TypeOf ex Is IO.IOException OrElse TypeOf ex Is UnauthorizedAccessException OrElse
                                       TypeOf ex Is OpenCVException OrElse TypeOf ex Is DllNotFoundException OrElse
                                       TypeOf ex Is TypeInitializationException OrElse TypeOf ex Is OutOfMemoryException
                Return Nothing
            End Try
        End Function

    End Class

End Namespace
