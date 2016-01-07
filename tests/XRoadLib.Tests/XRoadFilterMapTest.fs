namespace XRoadLib.Tests

open FsUnit
open NUnit.Framework
open System
open XRoadLib.Serialization

[<TestFixture>]
module XRoadFilterMapTest =
    type CustomClass() =
        member val Member1 = "" with get, set
        member val Member2 = 0 with get, set
        member val Member3 = CustomClass() with get, set

    type NoCustomMap() = inherit XRoadFilterMap<CustomClass>("Group name")

    type CustomMap() as this =
        inherit XRoadFilterMap<CustomClass>("CustomMap")
        do
            this.Enable(fun x -> x.Member1)
            this.Enable(fun x -> x.Member3)
            this.Enable(fun x -> x.Member1)
            this.Enable(fun x -> x.Member1)

    type BrokenMap() as this =
        inherit XRoadFilterMap<CustomClass>("BrokenMap")
        do this.Enable(fun x -> x.Member3.Member1)

    type Broken2Map() as this =
        inherit XRoadFilterMap<CustomClass>("Broken2Map")
        do this.Enable(fun x -> true)

    [<Test>]
    let ``missing mappings`` () =
        let map = NoCustomMap() :> IXRoadFilterMap
        map.GroupName |> should equal "Group name"
        map.EnabledProperties |> should not' (be Null)
        map.EnabledProperties.Count |> should equal 0

    [<Test>]
    let ``ignore re-enabled members`` () =
        let map = CustomMap() :> IXRoadFilterMap
        map.GroupName |> should equal "CustomMap"
        map.EnabledProperties |> should not' (be Null)
        map.EnabledProperties.Count |> should equal 2
        map.EnabledProperties.Contains("Member1") |> should equal true
        map.EnabledProperties.Contains("Member3") |> should equal true

    [<Test>]
    let ``cannot handle nested properties`` () =
        TestDelegate(fun _ -> BrokenMap() |> ignore)
        |> should (throwWithMessage "Only parameter members should be used in mapping definition (BrokenMap).") typeof<ArgumentException>

    [<Test>]
    let ``cannot handle non-member expressions`` () =
        TestDelegate(fun _ -> Broken2Map() |> ignore)
        |> should (throwWithMessage "MemberExpression expected, but was ConstantExpression (Broken2Map).") typeof<ArgumentException>
