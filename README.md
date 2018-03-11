# ImGuiNet.OpentK
An opentk helper for ImGui.Net. adapted from ImGuiCSSDL helper : https://github.com/0x0ade/ImGuiCS

#Install
the latest nuget package can be downloaded from here https://www.nuget.org/packages/ImGuiNet.OpenTK

#Build
1. Download and install .Net Core SDK 2.0+
2. Clone the repo.
3. Open terminal or shell in project folder and build with `dotnet build ImGuiNet.OpenTK.csproj -c Release`

#Dependency
This library required
1.https://github.com/mellinoe/ImGui.NET
2. OpenTK

#Usage
Extend the `ImGuiOpenTKWindow` class and override the `OnImGuiLayout method`. Implement your UI using ImGui.Net in that method.
`ImGuiOpenTKWindow` extends OpenTK's `GameWindow`, so call `base.UpdateFrame(e)` and `base.RenderFrame(e)` if you are overriding those methods
