' ----------------------------------------------------------
' Copyright (c) Microsoft Corporation.  All rights reserved.
' ----------------------------------------------------------
Imports System
Imports ODataWinPhoneQuickstart

Namespace Netflix
    ' Extend the Title class to bind to the media resource URI.
    Partial Public Class Title

        ' Returns the media resource URI for binding.
        Public ReadOnly Property StreamUri As Uri
            Get
                ' Get the URI for the media resource stream.
                Return App.ViewModel.GetReadStreamUri(Me)
            End Get
        End Property
    End Class
End Namespace
