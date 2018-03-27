' ----------------------------------------------------------
' Copyright (c) Microsoft Corporation.  All rights reserved.
' ----------------------------------------------------------

Imports System
Imports System.Windows.Data
Imports ODataWinPhoneQuickstart.Netflix

Public Class TimeConverter
    Implements IValueConverter

    Public Function Convert(ByVal value As Object, ByVal targetType As Type, ByVal parameter As Object, ByVal culture As System.Globalization.CultureInfo) As Object Implements IValueConverter.Convert
        Return String.Format("{0} min.", CType(value, Integer) / 60)
    End Function

    Public Function ConvertBack(ByVal value As Object, ByVal targetType As Type, ByVal parameter As Object, ByVal culture As System.Globalization.CultureInfo) As Object Implements IValueConverter.ConvertBack
        Return value
    End Function
End Class

Public Class TruncateSynopsis
    Implements IValueConverter

    Public Function Convert(ByVal value As Object, ByVal targetType As Type, ByVal parameter As Object, ByVal culture As System.Globalization.CultureInfo) As Object Implements IValueConverter.Convert
        If (Not value Is Nothing) AndAlso (CType(value, String).Length > 35) Then

            Return (CType(value, String).Substring(0, 35) & "...")

        Else
            Return value
        End If
    End Function

    Public Function ConvertBack(ByVal value As Object, ByVal targetType As Type, ByVal parameter As Object, ByVal culture As System.Globalization.CultureInfo) As Object Implements IValueConverter.ConvertBack
        Throw New NotImplementedException()
    End Function
End Class
