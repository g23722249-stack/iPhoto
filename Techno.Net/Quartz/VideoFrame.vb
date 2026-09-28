Imports System.Drawing
Imports OpenCvSharp
Imports OpenCvSharp.Extensions

Namespace Quartz

    ''' <summary>A still picture out of a video (new in the .NET port; for album covers of folders that hold
    ''' only videos). OpenCV's FFmpeg reader (opencv_videoio_ffmpeg, shipped with OpenCvSharp4.runtime.win)
    ''' takes the Chinese paths of the album folders as they are.</summary>
    Public NotInheritable Class VideoFrame

        Private Sub New()
        End Sub

        ''' <summary>The frame at <paramref name="fraction"/> (0..1) of the video, fitted within
        ''' <paramref name="maxSide"/> px and shown at its display shape (VCD / DVD frames are 4:3, not
        ''' square-pixelled). A frame that is nearly black or flat (a fade, a title card) is passed over
        ''' for one a little later, a few times. Nothing when the video can't be read.</summary>
        Public Shared Function Grab(ByVal file As String, ByVal fraction As Double, ByVal maxSide As Integer) As Bitmap
            Try
                Using cap As New VideoCapture(file)
                    If Not cap.IsOpened() Then Return Nothing
                    Dim frames As Double = cap.Get(VideoCaptureProperties.FrameCount)
                    If frames <= 0 Then frames = 1
                    Dim best As Mat = Nothing
                    Dim bestScore As Double = -1
                    Dim f As Double = Math.Max(0, Math.Min(0.98, fraction))
                    For tryNo = 0 To 3
                        cap.Set(VideoCaptureProperties.PosFrames, Math.Floor(frames * f))
                        Dim m As New Mat()
                        If cap.Read(m) AndAlso Not m.Empty() Then
                            Dim score As Double = Liveliness(m)
                            If score > bestScore Then
                                best?.Dispose()
                                best = m
                                bestScore = score
                            Else
                                m.Dispose()
                            End If
                            If score >= 25 Then Exit For   ' bright and busy enough
                        Else
                            m.Dispose()
                        End If
                        f = (f + 0.13) Mod 0.95   ' a little later (wrapping round before the end)
                    Next
                    If best Is Nothing Then Return Nothing
                    Using best
                        Return ToDisplayBitmap(best, maxSide)
                    End Using
                End Using
            Catch ex As Exception When TypeOf ex Is OpenCVException OrElse TypeOf ex Is DllNotFoundException OrElse
                                       TypeOf ex Is TypeInitializationException OrElse   ' the native library didn't load
                                       TypeOf ex Is BadImageFormatException OrElse TypeOf ex Is AccessViolationException OrElse
                                       TypeOf ex Is ArgumentException
                Return Nothing
            End Try
        End Function

        ''' <summary>How much there is to see: mean brightness, but 0 for an almost flat picture.</summary>
        Private Shared Function Liveliness(ByVal m As Mat) As Double
            Using gray As New Mat()
                Cv2.CvtColor(m, gray, ColorConversionCodes.BGR2GRAY)
                Dim mean As Scalar, sd As Scalar
                Cv2.MeanStdDev(gray, mean, sd)
                If sd.Val0 < 8 Then Return 0
                Return Math.Min(mean.Val0, 255 - mean.Val0 + 60)   ' too dark or blown out both score low
            End Using
        End Function

        Private Shared Function ToDisplayBitmap(ByVal m As Mat, ByVal maxSide As Integer) As Bitmap
            Dim w As Double = m.Width, h As Double = m.Height
            ' VCD / DVD sizes store 4:3 pictures in non-square pixels
            Select Case $"{m.Width}x{m.Height}"
                Case "352x240", "352x288", "704x480", "704x576", "720x480", "720x576", "480x480", "480x576"
                    w = h * 4 / 3
            End Select
            Dim k As Double = Math.Min(maxSide / w, maxSide / h)
            Using sized As New Mat()
                Cv2.Resize(m, sized, New OpenCvSharp.Size(Math.Max(1, CInt(w * k)), Math.Max(1, CInt(h * k))), 0, 0, InterpolationFlags.Cubic)
                Return BitmapConverter.ToBitmap(sized)
            End Using
        End Function

    End Class

End Namespace
