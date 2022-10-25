using System;
using DeBroglie;
using DeBroglie.Models;
using DeBroglie.Topo;
using Godot;
using SixLabors.ImageSharp.PixelFormats;

public class WfcRoot : Control {
   TextureRect? _inputTextureRect;
   TextureRect? _outputTextureRect;
   Button? _generateButton;
   Image? _inputImage;
   byte[]? _inputBytes;
   Rgba32[]? colors;
   TilePropagator? _propagator;
   
   
   public override void _Ready() {
      _inputTextureRect = GetNode<TextureRect>("%InputTexture");
      _outputTextureRect = GetNode<TextureRect>("%OutputTexture");
      _generateButton = GetNode<Button>("%GenerateButton");
      LoadInputImage();
      GeneratePng();
      _generateButton.Connect("pressed", this, nameof(GeneratePng));
   }

   void LoadInputImage() {
      var inTexture = GD.Load<Texture>("res://assets/sewers.png");
      inTexture.Flags = 0;
      _inputTextureRect!.Texture = inTexture;
      GD.Print(inTexture.GetData());
      _inputImage = inTexture.GetData();
      _inputBytes = _inputImage.GetData();
      
      Rgba32[] colors = new Rgba32[_inputBytes.Length / 3];
      var colorIndex = 0;
      for (var rgbIndex = 0; rgbIndex < _inputBytes.Length; rgbIndex += 3) {
         colors[colorIndex++] = new Rgba32(_inputBytes[rgbIndex],_inputBytes[rgbIndex+1],_inputBytes[rgbIndex+2]);
      }
      
      var topology = new GridTopology(DirectionSet.Cartesian2d, _inputImage.GetWidth(), _inputImage.GetHeight(),  false, false);
      ITopoArray<Tile> samples = TopoArray.Create(colors, topology).ToTiles();
      var model = new OverlappingModel(samples, 4, 4, true);
      _propagator = new TilePropagator(model, topology);
   }

   void GeneratePng() {
      _propagator.Clear();
      var status = _propagator.Run();
      if (status != Resolution.Decided) throw new Exception("undecided");
      
      var output = _propagator.ToValueArray<Rgba32>();
      var genData = new byte[_inputBytes.Length];

      var byteCount = 0;
      for (var topoIndex = 0; topoIndex < output.Topology.IndexCount; topoIndex++) {
         genData[byteCount++] = output.Get(topoIndex).R;
         genData[byteCount++] = output.Get(topoIndex).G;
         genData[byteCount++] = output.Get(topoIndex).B;
      }
      
      
      Console.WriteLine(genData);

      var genImg = new Image();
      genImg.CreateFromData(output.Topology.Width, output.Topology.Height, false, _inputImage.GetFormat(), genData);
      var genTexture = new ImageTexture();
      genTexture.CreateFromImage(genImg);
      genTexture.Flags = 0;
      _outputTextureRect!.Texture = genTexture;

   }

   void GenerateCharArray() {
      ITopoArray<char> sample = TopoArray.Create(new[] {
         new[] {'_', '_', '_'},
         new[] {'_', '*', '_'},
         new[] {'_', '_', '_'},
      }, periodic: false);
      var model = new AdjacentModel(sample.ToTiles());
      var topology = new GridTopology(10, 10, periodic: false);
      var propagator = new TilePropagator(model, topology);
      var status = propagator.Run();
      if (status != Resolution.Decided) throw new Exception("undecided");
      var output = propagator.ToValueArray<char>();

      for (var y = 0; y < 10; y++) {
         var line = "";
         for (var x = 0; x < 10; x++) {
            line += output.Get(x, y);
         }
         GD.Print(line);
      }
   } 
}
