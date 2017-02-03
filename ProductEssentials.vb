Imports System.Xml
Imports Newtonsoft.Json
Imports System.Security.Cryptography

Public Class ProductEssentials

    Class ProductCollection
        Friend Products As List(Of Product) = Nothing

        Public Sub New()
            Products = New List(Of Product)
        End Sub

        Friend Function ToX() As XDocument
            Dim ans As New XDocument

            If Products.Count > 0 Then
                Dim productsNode As New XElement("products")
                ans.Add(productsNode)

                For Each p As Product In Products
                    Dim xP As XElement = p.ToX
                    If xP IsNot Nothing Then
                        productsNode.Add(xP)
                    End If
                Next
            End If

            Return ans
        End Function

        Shared Function FromX(xDoc As XDocument) As ProductCollection
            Dim ans As ProductCollection = Nothing

            Dim prodXElems() As XElement = xDoc.Elements("products").ToArray

            If prodXElems IsNot Nothing Then
                ans = New ProductCollection
                For Each ProductsElem As XElement In prodXElems
                    If ProductsElem IsNot Nothing Then
                        If ProductsElem.Elements.Count > 0 Then
                            For Each prodElem As XElement In ProductsElem.Elements
                                Dim thisProd As Product = Product.FromX(prodElem)
                                If thisProd IsNot Nothing Then ans.Products.Add(thisProd)
                            Next
                        End If

                    End If
                Next
            End If

            Return ans
        End Function

        Friend Function ToJSON() As String
            Dim ans As String = ""

            Dim xdoc As XDocument = ToX()
            ans = JsonConvert.SerializeXNode(xdoc)

            Return ans
        End Function
    End Class

    ''' <summary>
    ''' A class to store data about a product, like it's name, pricing and other info
    ''' </summary>
    Class Product
        Friend Attributes As AttributeDictionary = Nothing
        Friend ChoiceCollections As SuperChoiceCollection = Nothing

        Friend Description As String = ""

        Public Sub New()
            Attributes = New AttributeDictionary
            ChoiceCollections = New SuperChoiceCollection
        End Sub

        Friend Function ToX() As XElement
            Dim ans As XElement = Nothing

            If ChoiceCollections.Content.Count > 0 Then
                ans = New XElement("product")
                For Each cc As ChoiceCollection In ChoiceCollections.Content
                    ans.Add(cc.ToX)
                Next
            End If

            If Attributes.Content.Count > 0 Then
                If ans Is Nothing Then ans = New XElement("product")
                For Each attribkey As String In Attributes.Content.Keys
                    ans.Add(New XAttribute(attribkey, Attributes.Content.Item(attribkey)))
                Next
            End If

            If Description.Equals("") = False Then
                If ans Is Nothing Then ans = New XElement("product")
                ans.Add(New XElement("description", Description))
            End If

            Return ans
        End Function

        Shared Function FromX(xelem As XElement) As Product
            Dim ans As Product = Nothing

            If xelem.Elements.Count > 0 Then
                ans = New Product
                For Each subelem As XElement In xelem.Elements
                    Select Case subelem.Name.LocalName.ToLower
                        Case "choices"
                            Dim thisCc As ChoiceCollection = ChoiceCollection.FromX(subelem)
                            If thisCc IsNot Nothing Then ans.ChoiceCollections.Content.Add(thisCc)
                        Case "description"
                            ans.Description = subelem.Value
                    End Select
                Next
            End If

            If xelem.HasAttributes Then
                If ans Is Nothing Then ans = New Product

                For Each attrib As XAttribute In xelem.Attributes
                    ans.Attributes.Attribute(attrib.Name.LocalName) = attrib.Value
                Next
            End If

            Return ans
        End Function
    End Class

    Class SuperChoiceCollection
        Friend Content As List(Of ChoiceCollection) = Nothing

        Public Sub New()
            Content = New List(Of ChoiceCollection)
        End Sub

        Friend Function GetChoiceCollection(attribkey As String, attribvalue As String) As ChoiceCollection
            Dim ans As ChoiceCollection = Nothing

            If Content.Count > 0 Then
                For Each cc As ChoiceCollection In Content.ToArray
                    If cc.Attributes.Content.ContainsKey(attribkey) Then
                        If cc.Attributes.Content.Item(attribkey).Equals(attribvalue) Then
                            ans = cc
                            Exit For
                        End If
                    End If
                Next
            End If

            Return ans
        End Function

        Friend Function GetChoiceCollectionByName(name As String) As ChoiceCollection
            Return GetChoiceCollection("name", name)
        End Function

    End Class

    Class ChoiceCollection
        Friend Attributes As AttributeDictionary = Nothing
        Friend Content As List(Of Choice) = Nothing

        Public Sub New()
            Content = New List(Of Choice)
            Attributes = New AttributeDictionary
        End Sub

        Friend Function GetChoice(Name As String) As Choice
            Dim ans As Choice = Nothing

            If Content.Count > 0 Then
                For Each c As Choice In Content
                    If c.Name.Equals(Name) Then
                        ans = c
                        Exit For
                    End If
                Next
            End If

            Return ans
        End Function

        Friend Function GetChoice(key As String, value As String) As Choice
            Dim ans As Choice = Nothing

            If Content.Count > 0 Then
                For Each c As Choice In Content
                    If c.Content.Content.ContainsKey(key) Then
                        If c.Content.Content.Item(key).Equals(value) Then
                            ans = c
                            Exit For
                        End If
                    End If
                Next
            End If

            Return ans
        End Function

        Friend Function ToX() As XElement
            Dim ans As XElement = Nothing

            If Content.Count > 0 Then
                ans = New XElement("choices")
                For Each c As Choice In Content.ToArray
                    ans.Add(c.ToX)
                Next
            End If

            If Attributes.Content.Keys.Count > 0 Then
                If ans Is Nothing Then ans = New XElement("choices")
                For Each key As String In Attributes.Content.Keys
                    ans.Add(New XAttribute(key, Attributes.Content.Item(key)))
                Next
            End If

            Return ans
        End Function

        Shared Function FromX(xelem As XElement) As ChoiceCollection
            Dim ans As ChoiceCollection = Nothing

            If xelem.Elements.Count > 0 Then
                ans = New ChoiceCollection
                For Each subelem As XElement In xelem.Elements
                    If subelem.Name.LocalName.ToLower.Equals("choice") Then
                        Dim thisC As Choice = Choice.FromX(subelem)
                        If thisC IsNot Nothing Then ans.Content.Add(thisC)
                    End If
                Next
            End If

            If xelem.HasAttributes Then
                For Each attrib As XAttribute In xelem.Attributes
                    ans.Attributes.Attribute(attrib.Name.LocalName) = attrib.Value
                Next
            End If

            Return ans
        End Function
    End Class

    Class Choice
        Friend Content As AttributeDictionary = Nothing
        Friend Value As String = ""

        Public Sub New()
            Content = New AttributeDictionary
        End Sub

        Public Sub New(Name As String)
            Content = New AttributeDictionary
            Me.Name = Name
        End Sub

        Public Sub New(Name As String, Price As Double)
            Content = New AttributeDictionary
            Me.Name = Name
            Me.Price = Price
        End Sub

        Friend Property Name As String
            Get
                Return Content.Attribute("name")
            End Get
            Set(value As String)
                Content.Attribute("name") = value
            End Set
        End Property

        Friend Property Description As String
            Get
                Return Content.Attribute("description")
            End Get
            Set(value As String)
                Content.Attribute("description") = value
            End Set
        End Property

        Friend Property Price As Double
            Get
                If Content.Content.ContainsKey("price") Then
                    Dim priceInt As Integer = 0
                    If Integer.TryParse(Content.Content.Item("price"), priceInt) = True Then
                        Return priceInt
                    Else
                        If Content.Content.Item("price").Replace(" ", "").Equals("") Then
                            Return 0
                        Else
                            Return 99999
                        End If
                    End If
                Else
                    Return 0
                End If
            End Get
            Set(value As Double)
                Content.Attribute("price") = value.ToString
            End Set
        End Property

        Friend Function ToX() As XElement
            Dim ans As XElement = Nothing

            If Content.Content.Keys.Count > 0 Then
                ans = New XElement("choice")

                For Each key As String In Content.Content.Keys
                    ans.Add(New XAttribute(key, Content.Content.Item(key)))
                Next
            Else
                ans = Nothing
            End If

            If Value.Equals("") = False Then
                If ans Is Nothing Then ans = New XElement("choice")
                ans.Value = Value
            End If

            Return ans
        End Function

        Shared Function FromX(xelem As XElement) As Choice
            Dim ans As Choice = Nothing

            If xelem.Attributes.Count > 0 Then
                ans = New Choice
                For Each attrib As XAttribute In xelem.Attributes
                    ans.Content.Attribute(attrib.Name.LocalName) = attrib.Value
                Next
            End If

            If xelem.Value.Equals("") = False Then
                If ans Is Nothing Then ans = New Choice
                ans.Value = xelem.Value
            End If

            Return ans
        End Function
    End Class

    Class AttributeDictionary
        Friend Content As Dictionary(Of String, String) = Nothing

        Public Sub New()
            Content = New Dictionary(Of String, String)
        End Sub

        Friend Property Attribute(Key As String) As String
            Get
                If Content.ContainsKey(Key) Then
                    Return Content.Item(Key)
                Else
                    Return ""
                End If
            End Get
            Set(value As String)
                If Content.ContainsKey(Key) Then
                    Content.Item(Key) = value
                Else
                    Content.Add(Key, value)
                End If
            End Set
        End Property

    End Class

    ''' <summary>
    ''' A class to define what happens after a Product is purchased.
    ''' </summary>
    Class PurchaseAction
        Friend Type As PurchaseActionTypes = PurchaseActionTypes.DoNothing

    End Class

    Enum PurchaseActionTypes As Integer
        DoNothing = 0
        NotifyHuman = 1
        RunApp = 2
    End Enum

End Class
