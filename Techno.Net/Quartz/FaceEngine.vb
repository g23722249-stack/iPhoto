Imports System.Drawing
Imports System.IO
Imports System.Runtime.InteropServices
Imports OpenCvSharp
Imports OpenCvSharp.Dnn
Imports OpenCvSharp.Extensions

Namespace Quartz

    ''' <summary>
    ''' Finds faces in a picture and describes each one with a 128-number feature: two faces of the same
    ''' person have features whose cosine is high (SFace: 0.363 or more). New in the .NET port (the VB6
    ''' iQuartz had no face support).
    '''
    ''' YuNet (face_detection_yunet_2023mar.onnx) and SFace (face_recognition_sface_2021dec.onnx) from
    ''' OpenCV Zoo run on OpenCV's DNN module. OpenCvSharp 4.10 doesn't wrap cv::FaceDetectorYN /
    ''' cv::FaceRecognizerSF, so this ports their C++ code (objdetect face_detect.cpp / face_recognize.cpp):
    '''   - detection: the picture shrunk to MaxSide, padded to a multiple of 32, one forward pass, the
    '''     three strides decoded, NMS;
    '''   - recognition: the 5 landmarks mapped onto the 112x112 ArcFace template (similarity transform),
    '''     one forward pass, L2-normalised feature.
    ''' Boxes and landmarks are returned as fractions (0..1) of the picture, so they fit any display size.
    ''' The file is decoded WITHOUT applying its EXIF orientation: the viewers show pictures through
    ''' System.Drawing, which doesn't apply it either, so the boxes line up with what is on screen.
    ''' One engine may be used from several threads; the networks are used one call at a time.
    ''' </summary>
    Public NotInheritable Class FaceEngine
        Implements IDisposable

        Public Const DetectorFile As String = "face_detection_yunet_2023mar.onnx"
        Public Const RecognizerFile As String = "face_recognition_sface_2021dec.onnx"
        Public Const FeatureLength As Integer = 128
        ''' <summary>SFace's same-person threshold for the cosine of two features.</summary>
        Public Const SameThreshold As Single = 0.363F
        ''' <summary>Written to FacePhoto.EngineVer (TEXT(20)): faces are re-analysed when this changes.</summary>
        Public Const Version As String = "YuNet2023+SFace2021"

        Public Class Face
            ''' <summary>Fractions (0..1) of the picture's width / height.</summary>
            Public Box As RectangleF
            ''' <summary>Right eye, left eye, nose tip, right / left mouth corner, as fractions.</summary>
            Public Landmarks(4) As PointF
            Public Score As Single
            Public Feature As Single()
        End Class

        Private Shared ReadOnly Strides As Integer() = {8, 16, 32}
        Private Shared ReadOnly OutNames As String() = {
            "cls_8", "cls_16", "cls_32", "obj_8", "obj_16", "obj_32",
            "bbox_8", "bbox_16", "bbox_32", "kps_8", "kps_16", "kps_32"}
        Private Shared ReadOnly Template As Point2f() = {
            New Point2f(38.2946F, 51.6963F), New Point2f(73.5318F, 51.5014F), New Point2f(56.0252F, 71.7366F),
            New Point2f(41.5493F, 92.3655F), New Point2f(70.7299F, 92.2041F)}

        Private ReadOnly m_detector As Net
        Private ReadOnly m_recognizer As Net
        Private ReadOnly m_lock As New Object

        ''' <summary>Pictures are shrunk to this long side before detection.</summary>
        Public Property MaxSide As Integer = 1280
        Public Property ScoreThreshold As Single = 0.8F
        Public Property NmsThreshold As Single = 0.3F
        ''' <summary>Faces narrower than this (pixels, after shrinking) are too small to recognise and are skipped.</summary>
        Public Property MinFaceSize As Integer = 24

        ''' <summary>True when both model files are in <paramref name="modelFolder"/>.</summary>
        Public Shared Function ModelsAvailable(ByVal modelFolder As String) As Boolean
            Return File.Exists(Path.Combine(modelFolder, DetectorFile)) AndAlso File.Exists(Path.Combine(modelFolder, RecognizerFile))
        End Function

        ''' <summary>The Models folder next to the application.</summary>
        Public Shared ReadOnly Property DefaultModelFolder As String
            Get
                Return Path.Combine(AppContext.BaseDirectory, "Models")
            End Get
        End Property

        Public Sub New(ByVal modelFolder As String)
            ' from bytes: cv::dnn can't open a path with Chinese characters on Windows
            m_detector = CvDnn.ReadNetFromOnnx(File.ReadAllBytes(Path.Combine(modelFolder, DetectorFile)))
            m_recognizer = CvDnn.ReadNetFromOnnx(File.ReadAllBytes(Path.Combine(modelFolder, RecognizerFile)))
            For Each n In {m_detector, m_recognizer}
                n.SetPreferableBackend(Backend.OPENCV)
                n.SetPreferableTarget(Target.CPU)
            Next
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            SyncLock m_lock
                m_detector.Dispose()
                m_recognizer.Dispose()
            End SyncLock
        End Sub

        '==============================================================================================
        ' Public
        '==============================================================================================
        ''' <summary>The faces of a picture file, each with its feature. Nothing when the file can't be read.</summary>
        Public Function Analyze(ByVal fileName As String) As List(Of Face)
            Using img As Mat = LoadShrunk(fileName)
                If img Is Nothing Then Return Nothing
                SyncLock m_lock
                    Dim faces As List(Of Face) = Detect(img, ScoreThreshold)
                    For Each f In faces
                        f.Feature = FeatureOf(img, f)
                    Next
                    Return faces.Select(Function(f) ToFraction(f, img)).ToList()
                End SyncLock
            End Using
        End Function

        ''' <summary>A face the user boxed by hand (<paramref name="box"/> in fractions): the detector is run
        ''' on the area around it with a lower threshold for the landmarks; when it still finds nothing the
        ''' box itself is used unaligned. Nothing when the file can't be read.</summary>
        Public Function AnalyzeRegion(ByVal fileName As String, ByVal box As RectangleF) As Face
            Using img As Mat = LoadShrunk(fileName)
                If img Is Nothing Then Return Nothing
                Dim r As New Rect(CInt(box.X * img.Cols), CInt(box.Y * img.Rows), CInt(box.Width * img.Cols), CInt(box.Height * img.Rows))
                Dim grow As New Rect(r.X - r.Width \ 2, r.Y - r.Height \ 2, r.Width * 2, r.Height * 2)
                grow = grow.Intersect(New Rect(0, 0, img.Cols, img.Rows))
                r = r.Intersect(New Rect(0, 0, img.Cols, img.Rows))
                If r.Width < 4 OrElse r.Height < 4 Then Return Nothing

                SyncLock m_lock
                    Dim found As Face = Nothing
                    Using area As New Mat(img, grow)
                        Dim centre As New Point2f(r.X + r.Width / 2.0F - grow.X, r.Y + r.Height / 2.0F - grow.Y)
                        found = Detect(area, 0.5F).
                            Where(Function(f) f.Box.Contains(New PointF(centre.X, centre.Y))).
                            OrderByDescending(Function(f) f.Score).FirstOrDefault()
                        If found IsNot Nothing Then
                            found.Feature = FeatureOf(area, found)
                        End If
                    End Using
                    Dim result As New Face With {.Box = box, .Score = 0}
                    If found IsNot Nothing Then
                        result.Feature = found.Feature
                        result.Score = found.Score
                        For n = 0 To 4
                            result.Landmarks(n) = New PointF((found.Landmarks(n).X + grow.X) / img.Cols, (found.Landmarks(n).Y + grow.Y) / img.Rows)
                        Next
                    Else
                        Using crop As New Mat(img, r), aligned As New Mat()
                            Cv2.Resize(crop, aligned, New OpenCvSharp.Size(112, 112))
                            result.Feature = Recognize(aligned)
                        End Using
                    End If
                    Return result
                End SyncLock
            End Using
        End Function

        ''' <summary>Cosine of two L2-normalised features (1 = identical).</summary>
        Public Shared Function Cosine(ByVal a As Single(), ByVal b As Single()) As Single
            If a Is Nothing OrElse b Is Nothing OrElse a.Length <> b.Length Then Return 0
            Dim s As Single = 0
            For i = 0 To a.Length - 1
                s += a(i) * b(i)
            Next
            Return s
        End Function

        ''' <summary>Feature as bytes for a database column (little-endian floats) and back.</summary>
        Public Shared Function FeatureToBytes(ByVal feature As Single()) As Byte()
            If feature Is Nothing Then Return Nothing
            Dim b(feature.Length * 4 - 1) As Byte
            Buffer.BlockCopy(feature, 0, b, 0, b.Length)
            Return b
        End Function

        Public Shared Function BytesToFeature(ByVal bytes As Byte()) As Single()
            If bytes Is Nothing OrElse bytes.Length = 0 OrElse bytes.Length Mod 4 <> 0 Then Return Nothing
            Dim f(bytes.Length \ 4 - 1) As Single
            Buffer.BlockCopy(bytes, 0, f, 0, bytes.Length)
            Return f
        End Function

        '==============================================================================================
        ' Loading
        '==============================================================================================
        Private Function LoadShrunk(ByVal fileName As String) As Mat
            Dim img As Mat = Nothing
            Try
                ' through bytes (cv::imread can't open Chinese paths); EXIF orientation not applied (see above)
                img = Cv2.ImDecode(File.ReadAllBytes(fileName), ImreadModes.Color Or ImreadModes.IgnoreOrientation)
                If img Is Nothing OrElse img.Empty() Then
                    img?.Dispose()
                    img = LoadThroughGdi(fileName)   ' GIF and anything else OpenCV doesn't decode
                End If
            Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException OrElse
                                       TypeOf ex Is OpenCVException OrElse TypeOf ex Is ArgumentException OrElse TypeOf ex Is OutOfMemoryException
                img?.Dispose()
                Return Nothing
            End Try
            If img Is Nothing OrElse img.Empty() Then
                img?.Dispose()
                Return Nothing
            End If

            Dim longSide As Integer = Math.Max(img.Cols, img.Rows)
            If longSide <= MaxSide Then Return img
            Dim k As Double = MaxSide / longSide
            Dim small As New Mat()
            Cv2.Resize(img, small, New OpenCvSharp.Size(Math.Max(1, CInt(img.Cols * k)), Math.Max(1, CInt(img.Rows * k))), 0, 0, InterpolationFlags.Area)
            img.Dispose()
            Return small
        End Function

        Private Shared Function LoadThroughGdi(ByVal fileName As String) As Mat
            Try
                Using fs As New FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite),
                      src As Image = Image.FromStream(fs, False, False),
                      bmp As New Bitmap(src.Width, src.Height, Imaging.PixelFormat.Format24bppRgb)
                    Using g As Graphics = Graphics.FromImage(bmp)
                        g.DrawImage(src, 0, 0, src.Width, src.Height)
                    End Using
                    Return BitmapConverter.ToMat(bmp)
                End Using
            Catch ex As Exception When TypeOf ex Is ArgumentException OrElse TypeOf ex Is IOException OrElse TypeOf ex Is OutOfMemoryException
                Return Nothing
            End Try
        End Function

        '==============================================================================================
        ' Detection (pixels of the Mat)
        '==============================================================================================
        Private Function Detect(ByVal bgr As Mat, ByVal threshold As Single) As List(Of Face)
            Dim padW As Integer = ((bgr.Cols - 1) \ 32 + 1) * 32
            Dim padH As Integer = ((bgr.Rows - 1) \ 32 + 1) * 32
            Dim outs As Mat() = OutNames.Select(Function(x) New Mat()).ToArray()
            Try
                Using padded As New Mat()
                    Cv2.CopyMakeBorder(bgr, padded, 0, padH - bgr.Rows, 0, padW - bgr.Cols, BorderTypes.Constant, Scalar.All(0))
                    Using blob As Mat = CvDnn.BlobFromImage(padded)
                        m_detector.SetInput(blob)
                        m_detector.Forward(outs, OutNames)
                    End Using
                End Using

                Dim boxes As New List(Of Rect), scores As New List(Of Single), faces As New List(Of Face)
                For i = 0 To Strides.Length - 1
                    Dim stride As Integer = Strides(i)
                    Dim cols As Integer = padW \ stride, rows As Integer = padH \ stride
                    Dim cls As Single() = ToArray(outs(i)), obj As Single() = ToArray(outs(i + 3))
                    Dim bbox As Single() = ToArray(outs(i + 6)), kps As Single() = ToArray(outs(i + 9))
                    For r = 0 To rows - 1
                        For c = 0 To cols - 1
                            Dim idx As Integer = r * cols + c
                            Dim score As Single = CSng(Math.Sqrt(Clamp01(cls(idx)) * Clamp01(obj(idx))))
                            If score < threshold Then Continue For
                            Dim cx As Single = (c + bbox(idx * 4)) * stride
                            Dim cy As Single = (r + bbox(idx * 4 + 1)) * stride
                            Dim w As Single = CSng(Math.Exp(bbox(idx * 4 + 2))) * stride
                            Dim h As Single = CSng(Math.Exp(bbox(idx * 4 + 3))) * stride
                            If w < MinFaceSize Then Continue For
                            Dim f As New Face With {.Box = New RectangleF(cx - w / 2, cy - h / 2, w, h), .Score = score}
                            For n = 0 To 4
                                f.Landmarks(n) = New PointF((kps(idx * 10 + 2 * n) + c) * stride, (kps(idx * 10 + 2 * n + 1) + r) * stride)
                            Next
                            faces.Add(f)
                            boxes.Add(New Rect(CInt(f.Box.X), CInt(f.Box.Y), CInt(f.Box.Width), CInt(f.Box.Height)))
                            scores.Add(score)
                        Next
                    Next
                Next

                Dim keep As Integer() = Nothing
                CvDnn.NMSBoxes(boxes, scores, threshold, NmsThreshold, keep)
                Return keep.Select(Function(k) faces(k)).
                            Where(Function(f) f.Box.X < bgr.Cols AndAlso f.Box.Y < bgr.Rows).ToList()
            Finally
                For Each m In outs
                    m.Dispose()
                Next
            End Try
        End Function

        '==============================================================================================
        ' Recognition
        '==============================================================================================
        Private Function FeatureOf(ByVal bgr As Mat, ByVal f As Face) As Single()
            Dim src As Point2f() = f.Landmarks.Select(Function(p) New Point2f(p.X, p.Y)).ToArray()
            Using m As Mat = Cv2.EstimateAffinePartial2D(InputArray.Create(src), InputArray.Create(Template),
                                                        Nothing, RobustEstimationAlgorithms.LMEDS),
                  aligned As New Mat()
                If m Is Nothing OrElse m.Empty() Then
                    Dim r As Rect = New Rect(CInt(f.Box.X), CInt(f.Box.Y), CInt(f.Box.Width), CInt(f.Box.Height)).Intersect(New Rect(0, 0, bgr.Cols, bgr.Rows))
                    If r.Width < 2 OrElse r.Height < 2 Then Return Nothing
                    Using crop As New Mat(bgr, r)
                        Cv2.Resize(crop, aligned, New OpenCvSharp.Size(112, 112))
                    End Using
                Else
                    Cv2.WarpAffine(bgr, aligned, m, New OpenCvSharp.Size(112, 112))
                End If
                Return Recognize(aligned)
            End Using
        End Function

        Private Function Recognize(ByVal aligned112 As Mat) As Single()
            Using blob As Mat = CvDnn.BlobFromImage(aligned112, 1.0, New OpenCvSharp.Size(112, 112), Scalar.All(0), True, False)
                m_recognizer.SetInput(blob)
                Using o As Mat = m_recognizer.Forward()
                    Dim v As Single() = ToArray(o)
                    Dim norm As Double = Math.Sqrt(v.Sum(Function(x) CDbl(x) * x))
                    If norm > 0 Then
                        For i = 0 To v.Length - 1
                            v(i) = CSng(v(i) / norm)
                        Next
                    End If
                    Return v
                End Using
            End Using
        End Function

        '==============================================================================================
        Private Shared Function ToFraction(ByVal f As Face, ByVal img As Mat) As Face
            Dim w As Single = img.Cols, h As Single = img.Rows
            Dim r As New Face With {
                .Box = RectangleF.FromLTRB(Math.Max(0, f.Box.Left) / w, Math.Max(0, f.Box.Top) / h, Math.Min(w, f.Box.Right) / w, Math.Min(h, f.Box.Bottom) / h),
                .Score = f.Score,
                .Feature = f.Feature}
            For n = 0 To 4
                r.Landmarks(n) = New PointF(f.Landmarks(n).X / w, f.Landmarks(n).Y / h)
            Next
            Return r
        End Function

        Private Shared Function ToArray(ByVal m As Mat) As Single()
            Dim n As Integer = CInt(m.Total())
            Dim a(n - 1) As Single
            If m.IsContinuous() Then
                Marshal.Copy(m.Data, a, 0, n)
            Else
                Using c As Mat = m.Clone()
                    Marshal.Copy(c.Data, a, 0, n)
                End Using
            End If
            Return a
        End Function

        Private Shared Function Clamp01(ByVal v As Single) As Single
            Return Math.Min(1.0F, Math.Max(0.0F, v))
        End Function

    End Class

End Namespace
