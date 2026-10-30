Imports OpenCvSharp

Namespace Quartz

    ''' <summary>How sharp and how exposed a picture is (new in the .NET port; 整理清單): the picture decoded
    ''' at 1/4 size in grey (libjpeg's scaled decoding), its long edge brought to MeasureEdge.
    '''   Sharpness   the variance of the Laplacian in the sharpest of 3 x 3 tiles -- a photo with a sharp
    '''               subject and a soft background (bokeh, sky) is not blurry
    '''   Mean        average grey 0..255
    '''   DarkShare / BrightShare   the share of pixels below 24 / above 240
    ''' The file is read as bytes (cv's own file reading can't take Chinese paths).</summary>
    Public NotInheritable Class ImageQuality

        Public Const MeasureEdge As Integer = 800

        Public Sharpness As Double
        Public Mean As Double
        Public DarkShare As Double
        Public BrightShare As Double

        ''' <summary>Nothing when the picture can't be decoded.</summary>
        Public Shared Function Measure(ByVal file As String) As ImageQuality
            Try
                Dim bytes() As Byte = IO.File.ReadAllBytes(file)
                Using gray As Mat = Cv2.ImDecode(bytes, ImreadModes.ReducedGrayscale4)
                    If gray Is Nothing OrElse gray.Empty() Then Return Nothing
                    Dim k As Double = MeasureEdge / Math.Max(gray.Width, gray.Height)
                    Using small As New Mat()
                        If k < 1 Then
                            Cv2.Resize(gray, small, New Size(Math.Max(1, CInt(gray.Width * k)), Math.Max(1, CInt(gray.Height * k))), 0, 0, InterpolationFlags.Area)
                        Else
                            gray.CopyTo(small)
                        End If
                        Dim q As New ImageQuality With {.Mean = Cv2.Mean(small).Val0}
                        Dim total As Double = small.Width * small.Height
                        Using mask As New Mat()
                            Cv2.Threshold(small, mask, 23, 255, ThresholdTypes.BinaryInv)
                            q.DarkShare = Cv2.CountNonZero(mask) / total
                            Cv2.Threshold(small, mask, 240, 255, ThresholdTypes.Binary)
                            q.BrightShare = Cv2.CountNonZero(mask) / total
                        End Using
                        Using lap As New Mat()
                            Cv2.Laplacian(small, lap, MatType.CV_64F)
                            Dim tw As Integer = lap.Width \ 3, th As Integer = lap.Height \ 3
                            For ty = 0 To 2
                                For tx = 0 To 2
                                    Using tile As New Mat(lap, New Rect(tx * tw, ty * th, tw, th))
                                        Dim m As Scalar, sd As Scalar
                                        Cv2.MeanStdDev(tile, m, sd)
                                        q.Sharpness = Math.Max(q.Sharpness, sd.Val0 * sd.Val0)
                                    End Using
                                Next
                            Next
                        End Using
                        Return q
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
