# Mission-Mars-Fourth-Horizon
This repo includes the 3rd exercise of Mission to Mars event.<br/>
The exercise consists in a WPF app using Cognitive Services Face API to identify the crew members of the mission and eventually spot the alien intruder.
<img src="/CognitiveMissionMars.png" width="800">
## Setup
In MainWindows.xaml.cs replace the first parameter of FaceServiceClient constructor with your Face API Key
```csharp
 private readonly FaceServiceClient _faceServiceClient = new FaceServiceClient("Your Face API Key", "https://westeurope.api.cognitive.microsoft.com/face/v1.0");
```

*Authors: Francesco Bonacci*
